# Art prompts: the parts every character is built from, for ChatGPT

The owner's model (2026-11-06): every person in the game is one shared base mannequin wearing a body (the armor), a cape and a helm, holding a weapon, with only the face changed; NPCs differ in size by morphs, height and width factors on the whole figure. This file has the prompts to make every part in ChatGPT, each one an edit of the base image so it lines up exactly, the recipe that builds each character from the parts, and full-figure prompts for the creatures that aren't people.

The parts follow the Art Guide (§1 visual identity, §2 resolution, §11 prompts for art made outside the repo) and the looks in `03-art-overhaul.md` §7 (who looks like what, the tiers and the reserved hues), §6 (live items) and §9 (creatures). Where a figure on the owner's guide sheet (`tools/source/art-guide.png`) maps onto a character, it's the basis. Where no brief exists, the design comes from the scripts and the voice sheets, and its Basis line says **(derived)**. No prompt names a game, a studio or a company, and every one asks for an original design.

---

## 1. How to use this file

### 1.1 The model

| Slot | What it is | Id |
| --- | --- | --- |
| **armor** | The whole outfit, which is the visible body below the neck: clothes, boots, gloves, belts, bags. For the people who aren't flesh (the Hollowed, the constructs, the drowned, Vorlaith) it's the body itself | `armor-…` |
| **cape** | What hangs behind the body: cloaks, capes, a pedlar's pack, and the Emberkin's and Wildkin's tails | `cape-…` |
| **helm** | What sits over the head: helms, hats, hoods, crowns, circlets | `helm-…` |
| **weapon** | What the hands hold: one weapon or prop, or a pair drawn as one piece (a sword and shield, two knives) | `weapon-…` |
| **face** | The head: the face, the hair, the beard, and the peoples' ears and horns. The only thing unique to a person | `face-…` |
| **morph** | A height and a width factor on the whole figure. It's how the builds and every NPC's size are made | numbers in §7 |

Bare skin on an armor keeps the base's warm tan tone, and the game tints it to match the face. Boots, gloves, rings and amulets in the item catalog change a character's numbers, not their picture; the armor already shows boots and gloves. The game draws back to front: cape, armor, face, helm, weapon (Emberkin horns and Wildkin ears draw over a hood, `03-art-overhaul.md` §3.6).

### 1.2 Follow-ups

Two prompts you'll reuse. Ask them in the same chat as the part they're for.

**Expressions** · `<face-id>.<expression>.png`

> Edit the attached image (this character's face on the base): change ONLY the expression, to the one I name at the end. Move only the eyes, eyebrows and mouth; the head, the hair, the skin and everything else stay exactly the same and in exactly the same place. The expressions: smile (eyes open, brows level, a gentle smile); grin (eyes half-closed, brows raised, a wide grin showing teeth); laugh (eyes closed, brows raised, mouth open); smirk (eyes narrowed, one brow up, a one-sided smirk); sad (eyes looking down, brows tilted up in the middle, a frown); worried (eyes open, brows knit and tilted up, mouth flat); afraid (eyes wide, brows tilted up, mouth open); angry (eyes narrowed, brows knit down, teeth gritted); cold (eyes narrowed, brows level, mouth flat, no warmth at all); tired (eyes half-closed, brows level, mouth flat); shock (eyes wide, brows raised high, mouth open). Return it as a PNG at the same size and background. The expression for this one: ____. I'll save it as <face-id>.<expression>.png.

The face picture itself is `neutral`. The twelve are `03-art-overhaul.md` §5.2's: notable characters (§7.4) get all eleven others, named minor characters five (smile, sad, worried, angry, shock), and generic faces two (smile, worried). The game cuts the eyes, brows and mouth out of each by comparing it with the neutral face.

**Realign** · `<part-id>.png`

> That moved things that should have stayed put. Do the same edit again on the attached base image, but this time leave everything outside the part exactly where it was, pixel for pixel: the same pose, body, head, framing, canvas size and position, and background. Change only the part I asked for. I'll save it as <part-id>.png.

### 1.3 Making the parts

1. **Make the base first** (§3): `base-male` and `base-female`, each in its own ChatGPT chat. Paste the style block (§2.1), the avoid list (§2.2) and the base prompt. When you're happy with it, keep that chat: every part is made from it.
2. **For every part,** attach that base PNG (or keep working in the base's chat, so it's already there), paste the style block and the avoid list, then the part's prompt. Each entry's Basis line says which base it edits. A variant (an entry that says it edits another part's picture) attaches that picture instead. One part per request.
3. **Check it, then send it.** Nothing outside the part may move: if it did, paste the Realign follow-up. Send the PNG to Claude named `<part-id>.png`, exactly the id in the entry's heading (every prompt ends with the file name). Claude cuts the part out by comparing it with the base, files it under `tools/source/characters/`, and credits it in `ASSETS.md`.
4. **Characters are recipes** (§7): once their parts exist there's nothing more to draw. **Creatures** (§8) are drawn whole, in their own chats, as before.

Every part is the base's canvas: 1024×1536, the standard figure about 85% of its height. Morphs resize characters in the game, so a child's or a Stoneborn's armor is drawn on the full-size base. Creatures that are wider than they are tall use a landscape canvas (the Art Guide's §2 allows larger canvases for creatures and bosses) and say how big they are next to a standard adult.

Priorities: **P0** is everything the Prologue to Act 1 Chapter 4 needs (including the creation screen and Act 1's shops), **P1** the rest of Act 1, **P2** Acts 2 and 3. A part takes the priority of the earliest character who wears it.

---

## 2. The shared style block

### 2.1 Style block

Paste this first, every time, exactly as it is.

> STYLE. Original character art for a 2D fantasy role-playing game, drawn in a classic 2D fantasy MMORPG character-art style. Bold, clean outlines in deep brown to near-black, never pure black, heavier on the outer silhouette and finer inside the figure; crisp, vector-like shapes. Cel shading: two to three flat tones per material with hard-edged shadow shapes and one soft highlight; no heavy gradients, no painterly blur. A strong, readable silhouette that still reads when the figure is small on screen. Slightly heroic but believable proportions: an adult is about five and a quarter heads tall, with natural shoulders, waist and hips, and hands with real fingers; not chibi, no oversized head. Faces are semi-realistic with a light anime lean: expressive eyes that aren't oversized, a defined nose, natural lips, real jaws and cheekbones, and age shown honestly. Hair is drawn as clean, clumped locks with two or three tones and a modest highlight. Costumes are grounded and well made, with visible seams, belts, buckles, straps, stitching, trims and a little wear at the hems and knees. Colours are vibrant but harmonious, for a bright, mystical fantasy world of blue-and-pink skies, navy and gold, and warm stone: clothes in rich, grounded colours, the strongest accents saved for magic, gold and gems. Full body, standing in a relaxed, ready pose, in a three-quarter view facing the right-hand side of the image: the body turned about 45 degrees toward the right, the head turned a little more toward us so both eyes show, the near leg slightly forward, the feet about a head's width apart, the weight on the back foot, a slight forward lean, the near arm in front of the body and the far arm mostly behind it. Even light from the upper left, a soft fill, and a thin, subtle rim of light along the back edges.

### 2.2 Avoid list

Paste this second.

> AVOID. Pixel art. Chibi or super-deformed proportions, an oversized head, huge anime eyes. Glossy 3D-render or plastic shading, airbrushed gradients, painterly blur, photorealism. Pure-black or uneven outlines. Neon or candy colours on clothes. A front view, a flat side profile or a back view, unless I ask for one. Scenery, a floor, a ground shadow, a frame or border. Any text, letters, numbers, labels, signature or watermark. More than one figure, unless I ask for a sheet. Cropped feet, hands, hair or weapon. Extra or missing fingers, or mitten hands, unless the prompt says a finger or hand is missing. Random spikes, skulls and clutter. Anything copied from an existing game, film, comic or brand.

---

### 2.3 How ChatGPT uses the bases

The bases are the owner's own pair (2026-11-06): a cel-shaded anime man and woman with tan skin and blue eyes. Both are bald and stand in a three-quarter view facing right with fists clenched, in dark underwear, on a solid black background. They are cut into `tools/source/characters/base-male.png` and `base-female.png` (1024 by 1536). Every part is an edit of one of them, so the base image itself sets the style, and these rules replace the style block for parts.

The owner's slot map (2026-11-06, the head fixed 2026-11-08): **armor is the body all the way up to the neck, the head is the mannequin's own and never changes shape, and the cape is the back.** The face is only the features painted on that head (eyes, brows, nose, mouth, marks, facial hair). **Hair and helm are one slot**, what's on the head: a hairstyle is a piece in it just as a helm is, and a helm that wants hair showing has it painted in. The weapon is what the near hand holds. The game stacks the parts in this order, back to front: cape, armor, head, face, hair or helm, weapon.

Faces, hairs and helms are painted on the **blank bases** (`base-male-blank` and `base-female-blank`, §3): the same figures with the facial features taken off, so every face lands on the same head and every hair and helm fits it.

**Where the regions fall on the bases.** The values are approximate pixels, measured on the 1024 by 1536 canvas. The importer uses them; ChatGPT gets the same regions in words.

| Landmark | `base-male` | `base-female` |
|---|---|---|
| Crown of the head | y 116 | y 151 |
| Jawline, where the head meets the neck (the seam between head and armor): a diagonal from under the ear to the chin | x 510, y 250 to x 595, y 293 | x 490, y 272 to x 578, y 325 |
| Neck meets the shoulders | y 330 | y 365 |
| Near fist (lit, on the left of the picture) | x 280–400, y 760–860 | x 305–420, y 760–860 |
| Far fist (in shadow, on the right) | x 650–745, y 760–850 | x 640–730, y 760–850 |
| Soles of the feet | y 1466 | y 1466 |

The back of the figure faces the left of the picture. A cape therefore shows past the figure's left edge, above the shoulders behind the neck, and between and below the legs.

**Setup brief**

Paste this once at the start of each ChatGPT chat, before the first part. Or put it in a ChatGPT Project's instructions and upload both bases to the Project. Every card's prompt carries its own slot rule too, so a card still works without the brief.

> I'm making the characters for a 2D fantasy game, and I need you to paint their parts as edits of a base character. Please read this whole brief before the first image, and follow it for every image in this chat.
>
> THE BASES. I'll attach one of two base characters, base-male.png or base-female.png, or their blank versions, base-male-blank.png and base-female-blank.png, the same figures with the facial features taken off. Each is 1024 × 1536 pixels: a bald, tan-skinned, cel-shaded anime-style figure with blue eyes. It stands in a three-quarter view facing right with both fists clenched at its sides, wears plain dark underwear, and is on a solid black background. Every character in the game is built on these exact two figures. The game stacks your images on top of each other like transparent sheets, so every image must line up with the base pixel for pixel. Anything that shifts, even a little, shows up as a seam.
>
> THE SLOTS. Each image you make is exactly one part, in one slot:
> 1. ARMOR is the body, everything from the jawline down: the neck, shoulders, chest, back, arms, hands, waist, hips, legs and feet. An armor replaces all the skin and the underwear below the jaw with the outfit. Its collar sits at the neck, the fists get gloves or stay bare, and the feet get boots or shoes. It never touches the head.
> 2. HAIR OR HELM is what's on the head: one slot for both, painted on the blank head. A HAIR is a hairstyle; a HELM is any helmet, hood, hat, crown, circlet or mask. Either may hang down over the neck and shoulders (long hair, a hood's drape, a mail coif, a plume, a veil), because the game draws it on top of the armor. A helm replaces the hair entirely, so if a helm should show hair (under a hat, round a circlet), paint that hair in as part of the helm. Where either leaves the face open, keep the blank face exactly as it is, because the game puts each character's own face there.
> 3. CAPE is the back: anything that hangs behind the body from the shoulders or the waist, such as a cape, a cloak, wings, a tail, a pack or a quiver. The game draws it behind the body. So paint it only in the black space behind and around the figure, and never over the body, the arms or the head. In this pose the figure's back is toward the LEFT of the picture. A cape therefore shows past the left edge of the shoulders, back and legs, above the shoulders behind the neck, and between and below the legs.
> 4. WEAPON is what the near hand holds. The near hand is the lit fist on the LEFT of the picture; the fist on the right, in shadow, is the far hand. The grip passes through the near fist, and the curled fingers stay exactly as they are, closed over it. The weapon shows above and below the fist. A shield or a second item goes on the far arm only when I ask for it.
> 5. FACE is only the features, painted on the blank head: eyes, eyebrows, nose, mouth, marks and scars, and any beard or moustache. The head itself never changes: keep its outline, size, angle and position, the skull and the ears, and paint no hair on it (the hair is a part of its own). Only pointed ears or horns, when I describe them, may change the ears or rise from the head. The neck belongs to the armor, so leave the neck and everything below it exactly as they are, except a beard falling past the jaw.
>
> FACE SHEETS. Sometimes I'll attach a face sheet instead of a base: eight copies of the blank head and shoulders in a grid of four columns and two rows, divided by thin grey lines. Then paint one face per cell, in the order I list them: the features only, no hair, every head exactly where it is in its cell, as in a single edit.
>
> HAIR SHEETS. Sometimes I'll attach a hair sheet: four copies of the blank figure from the head to the hips, side by side in columns divided by thin grey lines. Then give each head the hairstyle I list for its column, keep every head and face exactly where they are, as in a single edit, and keep each hairstyle inside its own column.
>
> ARMOR SHEETS. Sometimes I'll attach an armor sheet: four copies of the base standing full length, side by side in columns divided by thin grey lines. Then dress each figure in the outfit I list for its column, and keep every figure, its bald head and its pose exactly where they are, as in a single edit. Clothes stay inside their own column.
>
> HELM, CAPE AND WEAPON SHEETS. Sometimes I'll attach a helm sheet (four blank figures from the top of the head to the hips) or a cape or weapon sheet (four full figures, a little smaller, side by side). Then give each figure the helm, the cape or the held item I list for its column, keep every figure exactly where it is, as in a single edit, and keep each part inside its own column.
>
> The game layers them back to front: cape, armor, face, hair or helm, weapon.
>
> THE RULES FOR EVERY IMAGE:
> - Edit the attached image; never redraw it from scratch. Change only the slot I ask for, and keep every other pixel exactly as it is in the base.
> - Keep the same 1024 × 1536 canvas, the same solid black background, the same pose, the same body proportions, and the same position and size in the frame. Don't crop, zoom, shift, rotate or flip anything.
> - Use the same art style as the base: crisp dark outlines, flat cel shading in two to four tones per colour, light from the upper left, and the same colouring.
> - Wherever your slot doesn't reach, keep the base's bald head, face, skin and underwear unchanged.
> - No text, no labels, no ground, no floor shadow, no frame, and no extra objects or figures. No glow may spill onto the background unless I ask for it.
> - Give me back one PNG, 1024 × 1536.
>
> Each request will start with the rule for its slot and then describe the part. If a request ever seems to break these rules, follow the rules. Reply "Ready" if you understand, and wait for the first request.

**Every edit**

> Edit the attached image. It is a base character for a 2D game. The game stacks parts painted on it like transparent sheets, so your edit has to line up with it pixel for pixel. Keep the same 1024 × 1536 canvas, the same solid black background, the same pose with clenched fists, the same body proportions, the same position and size in the frame, and the same three-quarter view facing right. Keep exactly the same art style: crisp dark outlines, flat cel shading, the same light from the upper left, the same colouring. Change only the slot named below, and keep every other pixel exactly as it is. Don't crop, zoom, shift, rotate or flip anything. No text, no ground, no shadow on the floor, no extra objects or people.

**Armor**

> This is an ARMOR, which is the body from the jawline down. Replace all the skin and the underwear below the jaw (the neck, shoulders, chest, back, arms, hands, waist, hips, legs and feet) with the outfit described below. Fit it to this exact body and pose: its collar at the neck, gloves or bare hands on the same clenched fists, boots or shoes on the same feet. Where the outfit leaves skin showing, keep the base's tan skin there. The outline can grow where the clothes are loose (sleeves, skirts, coat tails, pauldrons), but the arms, legs, hands and feet stay exactly where they are. Don't touch the head above the jawline: keep the same bald head and the same face. The outfit:

**Helm**

> This is a HELM, which goes in the head's one slot: hair and helm are the same slot, so a helm replaces the hair entirely. Put the helmet, hood, hat, crown or mask described below on the blank bald head, fitted to its exact size, angle and position. It may hang down over the neck and shoulders (a hood's drape, a mail coif, a plume, a veil). If the description shows hair (under a hat's brim, round a circlet or a crown), paint that hair in as part of the helm; otherwise add none. Where the helm leaves the face open, keep the blank face exactly as it is, because the game puts each character's own face there. Don't change the body or the underwear. The helm:

**Cape**

> This is a CAPE, which is the back. Add the cape, cloak or back piece described below, hanging behind the body from the shoulders (or the waist). The game draws it behind the figure, so paint it only in the black space behind and around the body. In this pose the figure's back is toward the LEFT of the picture. So the cape shows past the left edge of the shoulders, back and legs, above the shoulders behind the neck, and between and below the legs. Don't paint over the body, the arms, the head or the underwear at all, and put no straps, clasps or collars across the front of the body. The cape:

**Hair**

> This is a HAIR, which goes in the head's one slot with the helms. Give the blank bald head the hairstyle described below, fitted to its exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Don't change the body or the underwear. The hair:

**Hair sheet**

> This is a HAIR SHEET: four copies of the same blank base character from the head to the hips, side by side in four equal columns divided by thin grey lines, on a solid black background. Give each head a different hairstyle, following the numbered list below; the figures are numbered 1 to 4 from left to right. Fit each to its head's exact size, angle and position, and keep every head, face and body exactly where it is, as in a single edit. Long hair may fall over the shoulders and down the back, but keep each hairstyle inside its own column, and keep the grey lines and the black exactly as they are.

**Weapon**

> This is a WEAPON, which is what the near hand holds. The near hand is the lit fist on the LEFT of the picture; the fist on the right, in shadow, is the far hand. Put the weapon described below in the near fist at a natural resting angle. The grip passes through the curled fingers, the fingers stay exactly as they are and close over it, and the weapon shows above and below the fist. Keep the whole weapon inside the canvas. Don't change the body, the head or the underwear. A shield or a second item goes on the far arm only if the description says so. The weapon:

**Face**

> This is a FACE, which is only the features. Paint the features described below onto the blank head: the eyes, eyebrows, nose and mouth, any marks or scars, and any beard or moustache. The head never changes: keep its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head; the hair is a part of its own. Paint the skin in the tone the description gives; the game tints the neck and body to match. The neck belongs to the armor, so leave the neck, the body and the underwear exactly as they are, except a beard falling past the jaw. The face:

**Face sheet**

> This is a FACE SHEET: eight copies of the same blank base character's head and shoulders, in a grid of four columns and two rows of equal cells divided by thin grey lines, on a solid black background. Paint a different face onto the blank head in each cell, following the numbered list below. The cells are numbered left to right, 1 to 4 along the top row and 5 to 8 along the bottom row. In every cell, keep the head exactly where it is, at the same size, angle and position, with the same head outline, skull and ears, in a three-quarter view facing right. Paint only the features (eyes, eyebrows, nose, mouth, marks, and any beard or moustache), and no hair: the hair is a part of its own. Keep the grey lines, the black and the shoulders exactly as they are.

**Armor sheet**

> This is an ARMOR SHEET: four copies of the same bald base character standing full length, side by side in four equal columns divided by thin grey lines, on a solid black background. Dress each figure in a different outfit, following the numbered list below; the figures are numbered 1 to 4 from left to right. Each outfit is the body from the jawline down: replace the skin and the underwear below the jaw (the neck, shoulders, chest, back, arms, hands, waist, hips, legs and feet) with the outfit, fitted to that exact body and pose, its collar at the neck, gloves or bare hands on the same clenched fists, boots or shoes on the same feet. Where an outfit leaves skin showing, keep the base's tan skin there. In every column keep the figure exactly where it is, at the same size and in the same pose, with the same bald head and the same face, untouched above the jawline. Loose clothes (sleeves, skirts, coat tails, pauldrons) may widen the outline, but they must stay inside their own column and never cross a grey line; the arms, legs, hands and feet stay exactly where they are. Make each outfit its own: don't let colours or pieces of one leak into another. Use exactly the base's art style: crisp dark outlines, flat cel shading, light from the upper left. Don't move, resize, crop or flip any figure; keep the grey lines and the black background; no text, numbers or labels anywhere. Give it back as one PNG, 1536 × 1024. The outfits:

**Helm sheet**

> This is a HELM SHEET: four copies of the same blank base character from the top of the head to the hips, side by side in four equal columns divided by thin grey lines, on a solid black background. Put a different helm, hood, hat, crown or mask on each head, following the numbered list below; the figures are numbered 1 to 4 from left to right. Hair and helm are one slot: a helm replaces the hair, so if one should show hair (under a brim, round a circlet or a crown), paint that hair in as part of it; otherwise add none. Fit each to its head's exact size, angle and position, and where it leaves the face open, keep the blank face exactly as it is. A helm may hang down over the neck and shoulders (a hood's drape, a mail coif, a plume, a veil), but keep each inside its own column, keep every head and body exactly where it is, as in a single edit, and keep the grey lines and the black exactly as they are.

**Cape sheet**

> This is a CAPE SHEET: four copies of the same base character standing full length, side by side in four equal columns divided by thin grey lines, on a solid black background. Give each figure a different cape, cloak, tail or back piece, following the numbered list below; the figures are numbered 1 to 4 from left to right. A cape hangs behind the body: paint it only in the black space behind and around each figure, never over the body, the arms, the head or the underwear, and put no straps, clasps or collars across the front of the body. Each figure's back faces the LEFT of its column, so a cape shows past the left edge of the shoulders, back and legs, above the shoulders behind the neck, and between and below the legs. Keep every figure exactly where it is, at the same size and in the same pose, with the same bald head and face; keep each cape inside its own column, never across a grey line; and keep the grey lines and the black exactly as they are. Give it back as one PNG, 1536 × 1024.

**Weapon sheet**

> This is a WEAPON SHEET: four copies of the same base character standing full length, side by side in four equal columns divided by thin grey lines, on a solid black background. Give each figure a different weapon or held item, following the numbered list below; the figures are numbered 1 to 4 from left to right. An item goes in the NEAR hand, the lit fist on the LEFT of each figure, unless the list says otherwise: the grip passes through the curled fingers, the fingers stay exactly as they are and close over it, and the item shows above and below the fist. The far hand, on the right of each figure, holds something only when the list says so. Keep every figure exactly where it is, at the same size and in the same pose, with the same bald head, face and body; keep each item whole and inside its own column, never across a grey line; and keep the grey lines and the black exactly as they are. Give it back as one PNG, 1536 × 1024.

**Creature**

> Use the attached image ONLY as a reference for the art style (line weight, cel shading, colouring, light) and for scale (a standard adult is that tall). Draw a new creature, full body, facing right, on a solid black background, in exactly that style. The creature:

### 2.4 The vector look

The owner's call (2026-11-08): the parts are drawn in ChatGPT as clean vector-style art, flat colours, one clean outline and hard-edged shading, so each one is right in one go and traces cleanly in `tools/vectorize.py`. The Art Prompts page's **Vector look** switch (on by default) wraps every part's prompt in the two blocks below, so each picture is one message: attach the picture the card names and paste. ChatGPT makes PNGs, not vector files; the vector shapes are `tools/vectorize.py`'s, which also brings the parts painted before this into the same look.

**Before every card**

> VECTOR LOOK. Draw what you add as clean vector-style 2D game art, as if it were made in a vector illustration program: every shape a closed area of flat colour with crisp, smooth edges; one clean outline round every shape, deep brown, almost black, never pure black, heavier on the outer silhouette (about 3 px) and lighter inside (about 1.5 px), in smooth, confident curves with no sketchy or doubled lines; cel shading in flat shapes only, each material with its base colour, one or two hard-edged shadow shapes and at most one small flat highlight, lit from the upper left; three or four flat tones per material, in bright, rich, harmonious colours; details such as belts, buckles, seams and trims as clear flat pieces that still read when small. No gradients, no airbrushing, no soft or blurred shadows, no texture, no noise, no brush strokes. Wherever the instructions below say to keep the base's art style, keep its line positions, proportions, light and colours, and draw in this vector look. Everything else in the instructions below holds exactly.

**After every card**

> Before you send it, check: nothing outside the part moved or changed size, every new edge is crisp, there are no gradients or textures, and the background is still flat black. Fix anything that fails before you send it.

**The vector setup brief.** Optional, since every wrapped card is complete on its own: paste it into a ChatGPT Project's instructions, with the four bases uploaded, to keep a run of parts consistent.

> I'm making the characters for an original 2D fantasy game. Every image you make in this chat is one part of a character, painted as an EDIT of a base image I attach, in a clean VECTOR art style. Read this whole brief first and follow it for every image in this chat. Reply "Ready" and wait for my first request.
>
> THE LOOK: VECTOR GAME ART
> Draw everything as if it were made in a vector illustration program for a 2D game:
> - Every shape is a closed area of FLAT colour with crisp, smooth edges. No gradients, no airbrushing, no soft or blurred shadows, no texture, no noise, no paper grain, no painterly brush strokes.
> - One clean outline round every shape, the same colour everywhere: deep brown, almost black, never pure black. Heavier on the outer silhouette (about 3 px at this canvas size), lighter inside (about 1.5 px). Smooth, confident curves; no sketchy, doubled or broken lines.
> - Shading is cel shading in flat shapes: each material gets its base colour, one or two hard-edged shadow shapes, and at most one small flat highlight shape. Light comes from the upper left; shadows fall to the lower right.
> - Few colours: three or four flat tones per material. Colours are bright and rich but harmonious, for a sunny, magical fantasy world (blue and pink skies, navy and gold, warm stone). Keep the strongest colours for magic, gold and gems.
> - Faces are clean, semi-realistic anime: expressive eyes that aren't oversized, drawn as flat shapes with one white highlight each, a simple defined nose, a small mouth.
> - Details stay readable when small: belts, buckles, seams and trims are clear flat pieces, not fine scribbles.
>
> THE BASES
> I'll attach one of four base images: base-male.png, base-female.png, or their blank versions base-male-blank.png and base-female-blank.png (the same figures with the facial features taken off). Each is 1024 x 1536: a bald, tan-skinned anime figure in a three-quarter view facing right, fists clenched at its sides, in plain dark underwear, on a solid black background. The game stacks your images on top of each other like transparent sheets, so every image must line up with the base pixel for pixel.
>
> THE SLOTS (each image is exactly one part)
> 1. ARMOR is the body from the jawline down: neck, shoulders, chest, arms, hands, waist, legs and feet. It replaces the skin and underwear below the jaw with the outfit: collar at the neck, gloves or bare fists, boots or shoes. Where the outfit leaves skin showing, keep the base's skin. It never touches the head.
> 2. HAIR or HELM is what sits on the head, one slot for both, painted on the blank head. A helm replaces the hair entirely; if a helm should show hair, paint that hair in as part of the helm. Either may hang over the neck and shoulders. Wherever it leaves the face open, keep the blank face exactly as it is.
> 3. CAPE is the back: cloaks, capes, packs, quivers, tails. Paint it only in the black space behind and around the figure, never over the body, arms or head. The figure's back faces the LEFT of the picture, so a cape shows past the left edge of the shoulders, back and legs, and between and below the legs.
> 4. WEAPON is what the near hand holds: the lit fist on the LEFT of the picture. The grip passes through the curled fingers, which stay exactly as they are; the weapon shows above and below the fist and stays inside the canvas.
> 5. FACE is only the features painted on the blank head: eyes, brows, nose, mouth, marks and scars, and any beard or moustache. The head never changes shape, and no hair goes on it (hair is its own part). Pointed ears or horns only when I describe them.
>
> SHEETS
> Sometimes I'll attach a sheet instead of a base, its cells divided by thin grey lines:
> - FACE SHEET: eight blank heads, 4 across and 2 down. Paint one face per cell in the order I list them (1-4 along the top row, 5-8 along the bottom).
> - HAIR SHEET: four blank figures from the head to the hips, side by side. One hairstyle per column, numbered 1-4 from the left.
> - ARMOR SHEET: four full figures side by side, 1536 x 1024. One outfit per column, numbered 1-4 from the left.
> - HELM SHEET: four blank figures from the top of the head to the hips, side by side. One helm per column, numbered 1-4 from the left.
> - CAPE SHEET or WEAPON SHEET: four full figures side by side, 1536 x 1024. One cape, or one held item, per column, numbered 1-4 from the left.
> Keep every figure exactly where it is in its cell, keep each part inside its own cell, and keep the grey lines and the black.
>
> THE RULES FOR EVERY IMAGE
> - EDIT the attached image; never redraw it from scratch. Change only the slot I ask for and keep every other pixel as it is.
> - Same canvas size, same flat pure-black background, same pose, same proportions, same position and size in the frame. Don't crop, zoom, shift, rotate or flip anything.
> - Where a request says "the same art style as the base", read it as: the base's line positions, proportions, light and colours, drawn in the vector look above.
> - No text, letters, numbers, labels, signature or watermark. No ground, floor shadow, frame or border. No extra objects or figures. No glow spilling onto the background unless I ask for it.
> - Original designs only: nothing copied from an existing game, film, comic or brand.
> - Give me one PNG at the same size as the image I attached.
>
> Before you send each image, check it: the figure hasn't moved or changed size, only the requested slot changed, every edge is crisp, there are no gradients or textures, and the background is still flat black. If any check fails, fix it before sending.
>
> Each request will name the slot and describe the part. If a request seems to break these rules, follow these rules.

## 3. Base

### 3.1 The mannequin

Two figures, the alignment reference for everything else. Make them first and keep their chats.

#### `base-male` · The base mannequin, man · P0

**Basis:** From the owner's slot model (2026-11-06); the Art Guide §3 (one shared rig); `03-art-overhaul.md` §2.1 and §2.2 (the angle and proportions).

The base mannequin for a 2D game's character system. Every face, outfit, cape, helm and weapon will be painted onto this exact image by editing it, so it must be plain and exact. A man's mannequin: natural, athletic shoulders, chest, waist and hips, not bulky, the standard build, about five and a quarter heads tall, smooth like a shop-window mannequin with no anatomical detail, and hands with clearly drawn, relaxed, slightly open fingers. The whole body is one even, light, neutral grey, drawn with the style block's outlines, cel shading and light. The head is bald, with a calm, simple face (blue eyes, light brows, a small mouth) that every face edit repaints. Stand it exactly in the standard pose: three-quarter view facing right, the body turned about 45 degrees and the head a little more toward us, the near leg slightly forward, the feet a head's width apart, the weight on the back foot, the near arm hanging just in front of the body and a little away from it, the far arm mostly behind it with its hand showing past the hip. Output: PNG, 1024×1536, on a solid black background. One figure, the full body centred with the feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: a standard adult man, about 85% of the canvas height. An original design. I'll save it as base-male.png.

#### `base-female` · The base mannequin, woman · P0

**Basis:** From the owner's slot model (2026-11-06); the Art Guide §3 (one shared rig); `03-art-overhaul.md` §2.1 and §2.2 (the angle and proportions).

The base mannequin for a 2D game's character system. Every face, outfit, cape, helm and weapon will be painted onto this exact image by editing it, so it must be plain and exact. A woman's mannequin: narrower shoulders, a defined waist, wider hips and a modest bust, the standard build, about five and a quarter heads tall, smooth like a shop-window mannequin with no anatomical detail, and hands with clearly drawn, relaxed, slightly open fingers. The whole body is one even, light, neutral grey, drawn with the style block's outlines, cel shading and light. The head is bald, with a calm, simple face (blue eyes, light brows, a small mouth) that every face edit repaints. Stand it exactly in the standard pose: three-quarter view facing right, the body turned about 45 degrees and the head a little more toward us, the near leg slightly forward, the feet a head's width apart, the weight on the back foot, the near arm hanging just in front of the body and a little away from it, the far arm mostly behind it with its hand showing past the hip. Output: PNG, 1024×1536, on a solid black background. One figure, the full body centred with the feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: a standard adult woman, about 84% of the canvas height. An original design. I'll save it as base-female.png.

#### `base-male-blank` · The blank mannequin, man · P0

**Basis:** Edits `base-male`. The head everyone's drawn with: every face, hair and helm is painted on it.

Edit the attached base image: change ONLY the face: take the facial features off, so the head is a blank mannequin's. No eyes, no eyebrows, no mouth, and only the faintest suggestion of the nose's shape; the face is smooth, even skin in the same warm tan, with the same soft cel shading, as if the features had been gently sanded away. Keep the bald head's exact outline, size, angle and position, the ears, the neck and the whole body exactly the same, not a single pixel moved. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as base-male-blank.png.

#### `base-female-blank` · The blank mannequin, woman · P0

**Basis:** Edits `base-female`. The head everyone's drawn with: every face, hair and helm is painted on it.

Edit the attached base image: change ONLY the face: take the facial features off, so the head is a blank mannequin's. No eyes, no eyebrows, no mouth, and only the faintest suggestion of the nose's shape; the face is smooth, even skin in the same warm tan, with the same soft cel shading, as if the features had been gently sanded away. Keep the bald head's exact outline, size, angle and position, the ears, the neck and the whole body exactly the same, not a single pixel moved. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as base-female-blank.png.

### 3.2 Builds and peoples

There's no separate base for any build or people: a build is a morph (`03-art-overhaul.md` §3.5).

| Build | Morph (height × width) | Who |
| --- | --- | --- |
| `small` | 0.72 × 0.80; children of eight about 0.62 × 0.66 | Hearthlings, children, thornlings |
| `lean` | 1.04 × 0.90; very tall people up to 1.10 × 0.85 | Sylvans, Mira, Valencia, Elara, Crane, Severin (Vorlaith 1.20 × 0.95) |
| `standard` | 1.00 × 1.00 | Humans, Emberkin, Wildkin, the default hero |
| `stout` | 0.85 × 1.30 | Stoneborn (Brannock 1.36 wide, Grell 1.55) |
| `big` | 1.08 × 1.15 | Thugs, knights, heavy guards; the constructs 1.20 × 1.30 |

What marks a people goes on the face: Sylvan pointed ears, Emberkin horns, Wildkin beast ears, and a Stoneborn's or a Hearthling's own face. The Emberkin's and Wildkin's tails go in the cape slot (`cape-tail-…`), so a character with a tail and a cloak needs a cape that shows both (`cape-tamsin`).

---

## 4. Faces

One per character in the cast and the party, the player's selectable faces, the townsfolk pool's faces, and the heads of the humanoid foes. Each paints only the features onto the blank head of the base whose sex matches (§3); the head itself never changes, and the hair is a part of its own (§4.8). A child's face is drawn on the full-size base and the morph makes it small. The faces painted before 2026-11-08 were head and hair together; the importer splits each into its face and its hair till it's painted again.

### 4.0 Face sheets (eight at a time)

The owner's call (2026-11-06): make the faces eight at a time. Every face painted on a base is in one sheet here, and each sheet is one ChatGPT picture. The attached template (`tools/source/characters/face-sheet-male.png` or `face-sheet-female.png`, made by `python tools/part_sheet.py templates`) is eight copies of the blank base's head and shoulders at full size, four across and two down, so each face lines up with the base just as a single edit would. `python tools/part_sheet.py cut <png> <sheet-id>` puts each cell back on its base as `tools/source/characters/faces/<face-id>.png`. The single face cards in §4.1 onward stay for redoing one face, and for the variants that edit another face.

The cells are numbered left to right, 1 to 4 along the top row and 5 to 8 along the bottom. The sheets are filled in priority order, so the early ones hold the faces the game needs first.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `face-sheet-m01` | `base-male` | P0 | `face-player-1`, `face-player-6`, `face-player-8`, `face-kael`, `face-brannock`, `face-mc-child`, `face-lookout`, `face-tomas` |
| `face-sheet-m02` | `base-male` | P0 | `face-red-gull`, `face-red-gull-2`, `face-red-gull-3`, `face-grell`, `face-hale`, `face-fennick`, `face-brisk`, `face-refugee-1` |
| `face-sheet-m03` | `base-male` | P0 | `face-refugee-2`, `face-leon`, `face-crane`, `face-pip`, `face-osric`, `face-lark`, `face-oswin`, `face-town-1` |
| `face-sheet-m04` | `base-male` | P0 | `face-town-3`, `face-town-5`, `face-town-7`, `face-town-9`, `face-town-11`, `face-town-12`, `face-town-14`, `face-town-16` |
| `face-sheet-m05` | `base-male` | P0 | `face-town-17`, `face-hollowed`, `face-construct-vault`, `face-garrick`, `face-aldric`, `face-recruit`, `face-tobin`, `face-harrow` |
| `face-sheet-m06` | `base-male` | P1 | `face-hobb`, `face-maleficio`, `face-lost-boy`, `face-varkhul`, `face-cube-cultist`, `face-corwin`, `face-severin`, `face-pell` |
| `face-sheet-m07` | `base-male` | P2 | `face-blood-house-steward`, `face-hollowmere-child`, `face-ruck`, `face-davin`, `face-vorlaith`, `face-thrall`, `face-drowned`, `face-blood-wight` |
| `face-sheet-m08` | `base-male` | P2 | `face-sun-scorched`, `face-thornling`, `face-construct-priest`, `face-construct-crown`, `face-unmade` |
| `face-sheet-f01` | `base-female` | P0 | `face-player-2`, `face-player-3`, `face-player-4`, `face-player-5`, `face-player-7`, `face-sera`, `face-mira`, `face-sb-child` |
| `face-sheet-f02` | `base-female` | P0 | `face-corra`, `face-odelle`, `face-hedda`, `face-refugee-child`, `face-refugee-3`, `face-elara`, `face-anvil-barkeep`, `face-netty` |
| `face-sheet-f03` | `base-female` | P0 | `face-maud`, `face-ilsa`, `face-hesper`, `face-wren`, `face-imra-vell`, `face-tamsin`, `face-town-2`, `face-town-4` |
| `face-sheet-f04` | `base-female` | P0 | `face-town-6`, `face-town-8`, `face-town-10`, `face-town-13`, `face-town-15`, `face-town-18`, `face-mira-young`, `face-thessaly` |
| `face-sheet-f05` | `base-female` | P1 | `face-brenna`, `face-gannet`, `face-hester`, `face-mags`, `face-valencia`, `face-anselle`, `face-orla`, `face-isadora` |
| `face-sheet-f06` | `base-female` | P2 | `face-valencia-child`, `face-ide`, `face-maren-elder`, `face-hollowmere-farmer`, `face-guild-apprentice`, `face-vreya` |

#### `face-sheet-m01` · Face sheet: 8 men (You, face 1, You, face 6, You, face 8, Kael Duskmantle, Brannock Ironvein, You, aged eight, The lookout, Old Tomas) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-player-1`, 2 `face-player-6`, 3 `face-player-8`, 4 `face-kael`, 5 `face-brannock`, 6 `face-mc-child`, 7 `face-lookout`, 8 `face-tomas`.

1. You, face 1: a young human man of about twenty-four from a poor fishing cove. An open, likeable oval face, a straight nose, warm brown eyes under straight, expressive brows, clean-shaven, fair skin lightly browned by the sea.
2. You, face 6: an Emberkin man of about twenty-two. A sharp, handsome face, amber eyes, a small fang showing at the corner of the mouth, tan skin. Two dark, ridged horns curl round from the temples like a ram's, old bone going ivory at the tips.
3. You, face 8: a Stoneborn man who looks about forty-five (the game's morph makes him short and broad, so draw the face at the base's size). A broad, heavy face, deep-set grey eyes, a wide nose, weathered golden skin.
4. Kael Duskmantle: Kael, an Emberkin thief of twenty-three. A sharp, handsome face, amber eyes, straight brows, a small fang just showing, tan skin. Two dark, ridged horns sweep straight back from the temples in a long curve, going pale like old ivory at the tips.
5. Brannock Ironvein: Brannock, a Stoneborn old soldier nearly two hundred years old, which on a Stoneborn looks like a hard-weathered sixty. A square, heavy face, a nose broken more than once, deep-set brown eyes under thick, fierce brows, old scars, golden-tan skin.
6. You, aged eight: the player as a boy of eight in a fishing cove (the game's morph makes him child-sized). A round, freckled child's face, sunburn across the nose, big honest brown eyes, a gap in his front teeth, fair skin.
7. The lookout: a wiry human sailor of about thirty. A narrow face, sun-dark brown skin, sharp pale-grey eyes used to squinting at the horizon, a short scruffy beard.
8. Old Tomas: Old Tomas, a human fisherman of about seventy. A weathered, deeply lined face, brown-tan skin, bushy white eyebrows over squinting blue-grey eyes, a short white stubble beard, a big nose reddened by the wind.
I'll save it as face-sheet-m01.png.

#### `face-sheet-m02` · Face sheet: 8 men (Red Gull Thug, face 1, Red Gull Thug, face 2, Red Gull Thug, face 3, Bosun Grell, Captain Roderic Hale, Questmaster Fennick, Sergeant Brisk, Old Aurek) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-red-gull`, 2 `face-red-gull-2`, 3 `face-red-gull-3`, 4 `face-grell`, 5 `face-hale`, 6 `face-fennick`, 7 `face-brisk`, 8 `face-refugee-1`.

1. Red Gull Thug, face 1: a harbour gang thug of about thirty. A brutal, smug face, a heavy jaw with dark stubble, small hard eyes under a thick brow, a flattened nose, deep brown skin.
2. Red Gull Thug, face 2: another harbour gang thug, about thirty. A broken nose, a scraggly ginger beard, pale eyes, light, freckled skin.
3. Red Gull Thug, face 3: a third harbour gang thug, about twenty-five. A narrow, sneering face, a gold tooth, dark eyes, tan skin.
4. Bosun Grell: Bosun Grell, a Stoneborn gang boss of middle age. A broad, mean face, small fierce eyes under heavy brows, a puckered scar across one cheek, tan skin.
5. Captain Roderic Hale: Captain Hale of the City Watch, a human man of about fifty-two. A square, tired face, grey eyes with heavy bags under them, a short grey stubble beard, a crooked nose, fair skin.
6. Questmaster Fennick: Questmaster Fennick, a human clerk of about fifty-eight. A round, pleasant face, pale skin, grey eyes behind small round spectacles, a neat grey moustache, a satisfied little smile.
7. Sergeant Brisk: Sergeant Brisk of the City Watch, a human man of about forty. A worried, harried face, brown eyes, a thick black moustache, a five-o'clock shadow, brown skin.
8. Old Aurek: Old Aurek, a sick refugee farmer of about seventy-five. A gaunt, feverish face, kind grey eyes, a long silver beard, tan skin with deep lines.
I'll save it as face-sheet-m02.png.

#### `face-sheet-m03` · Face sheet: 8 men (Teodor, King Leon, Lord Chancellor Vesper Crane, Pip the Potboy, Osric Penhallow, Lark the Busker, Guardsman Oswin, Townsfolk face 1) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-refugee-2`, 2 `face-leon`, 3 `face-crane`, 4 `face-pip`, 5 `face-osric`, 6 `face-lark`, 7 `face-oswin`, 8 `face-town-1`.

1. Teodor: Teodor, a refugee lad of about sixteen with a fever. A face flushed with fever, sweat on the brow, tired brown eyes, the first fuzz of a moustache, brown skin.
2. King Leon: King Leon, a human man of about sixty-eight, kind and tired. A broad, gentle face, grey eyes with deep laugh lines, thick grey brows, a full, neatly trimmed silver beard, fair skin with the flush of age.
3. Lord Chancellor Vesper Crane: Lord Chancellor Crane, a human man of about fifty-five. A long, narrow face, high cheekbones, pale porcelain skin, sharp grey eyes, finely arched brows, a thin black moustache, a polite smile that doesn't reach his eyes.
4. Pip the Potboy: Pip, a Hearthling potboy of about seventeen (an adult face, not a child's). A freckled, nervous face, wide brown eyes, fair skin.
5. Osric Penhallow: Osric, a human silk merchant of about forty-five. A neat, anxious face, brown eyes glancing sideways, a carefully trimmed black moustache, porcelain-pale skin.
6. Lark the Busker: Lark, a young-looking Sylvan street singer. A bright, cheeky face, pale lilac-grey eyes, a performer's half-smile, porcelain skin, long pointed ears.
7. Guardsman Oswin: Oswin, a young human palace guard of about twenty-two. A very straight-faced, serious face, blue eyes, clean-shaven, fair skin.
8. Townsfolk face 1: a young human farmhand of about twenty. An open, wide-eyed face, blue eyes, sunburnt cheeks, a peeling nose, fair skin.
I'll save it as face-sheet-m03.png.

#### `face-sheet-m04` · Face sheet: 8 men (Townsfolk face 3, Townsfolk face 5, Townsfolk face 7, Townsfolk face 9, Townsfolk face 11, Townsfolk face 12, Townsfolk face 14, Townsfolk face 16) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-town-3`, 2 `face-town-5`, 3 `face-town-7`, 4 `face-town-9`, 5 `face-town-11`, 6 `face-town-12`, 7 `face-town-14`, 8 `face-town-16`.

1. Townsfolk face 3: a strong human man of about forty. A strong, calm face, dark eyes, a short black beard, deep brown skin.
2. Townsfolk face 5: an old human man of about seventy-five. A grumpy, twinkly face, small grey eyes, a big nose, white mutton-chop whiskers, fair, ruddy skin.
3. Townsfolk face 7: a human boy of about eight. A cheeky child's face, brown eyes, a missing front tooth, a scraped nose, light-brown skin.
4. Townsfolk face 9: a human boy of about fifteen. An eager, conspiratorial face, green eyes, a few freckles, fair skin.
5. Townsfolk face 11: a human man of about thirty. A tired, good-natured face, dark eyes, stubble, olive skin.
6. Townsfolk face 12: a weathered human man of about fifty. A loud, cheerful face, bright brown eyes, a big grey-shot black beard, weathered olive skin.
7. Townsfolk face 14: a human man of about thirty-five. A tired, friendly face, grey eyes, a few days of brown stubble, weathered fair skin.
8. Townsfolk face 16: a Sylvan man who looks about thirty. A narrow, calm face, long pointed ears, grey-green eyes, porcelain skin.
I'll save it as face-sheet-m04.png.

#### `face-sheet-m05` · Face sheet: 8 men (Townsfolk face 17, The Hollowed, Vault construct's head, Garrick the caravan master, Commander Aldric Voss, The recruit, Tobin Ashgrove, Lord Harrow of Heronsgate) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-town-17`, 2 `face-hollowed`, 3 `face-construct-vault`, 4 `face-garrick`, 5 `face-aldric`, 6 `face-recruit`, 7 `face-tobin`, 8 `face-harrow`.

1. Townsfolk face 17: a Stoneborn man who looks about fifty. A broad face, deep-set brown eyes, ruddy tan skin.
2. The Hollowed: a Hollowed, a dead soldier raised as an empty husk. A gaunt, lipless head with skin like ash-grey burnt paper, cracked all over in fine lines that glow ember orange from inside; empty, dark eye sockets with a single ember-orange point of light in each; a few wisps of scorched hair.
3. Vault construct's head: an ancient stone guardian: the head is not a face at all. In place of the head, a flat, round disc of polished sun-gold metal with a simple sunburst engraved on it and no other features, set on a short neck of pale dressed stone with thin gold inlay.
4. Garrick the caravan master: Garrick, a human caravan master of about fifty. A lean, leathery face, tan skin with squint lines, brown eyes, a grey-shot black moustache, an old scar through one eyebrow.
5. Commander Aldric Voss: Commander Aldric Voss, an old human soldier of about sixty-five. A craggy, humorous face, pale-blue eyes with deep crow's feet, a grey stubble beard, an old notch missing from one ear, weathered fair skin.
6. The recruit: a brand-new human soldier of about seventeen. An eager, open face, red-cheeked from the cold, blue eyes, a few spots, fair skin.
7. Tobin Ashgrove: Tobin, a human cider-presser of about fifty-five. A gentle, round face, kind brown eyes, freckles, a short greying copper-red beard, fair skin reddened by the sun.
8. Lord Harrow of Heronsgate: Lord Harrow, a bored human lord of about fifty-five. A bland, pale face, heavy-lidded eyes looking off to one side at nothing, grey side-whiskers, a small, polite, empty smile.
I'll save it as face-sheet-m05.png.

#### `face-sheet-m06` · Face sheet: 8 men (Hobb the woodcutter, Maleficio, The woodcutter's boy, Varkhul, Cube Cultist, Sir Corwin Halbeck, Lord Severin Morvaine, Pell) · P1

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-hobb`, 2 `face-maleficio`, 3 `face-lost-boy`, 4 `face-varkhul`, 5 `face-cube-cultist`, 6 `face-corwin`, 7 `face-severin`, 8 `face-pell`.

1. Hobb the woodcutter: Hobb, a human woodcutters' foreman of about forty-five. A broad, ruddy face, worried blue eyes, a big bushy red beard, freckled fair skin.
2. Maleficio: Maleficio, a gentle old human scholar of about sixty-five. A kind, lined face, tired dark eyes, a close-trimmed grey beard, a high forehead, pale skin that hasn't seen the sun in years.
3. The woodcutter's boy: a lost human boy of about eight. A freckled, tear-streaked child's face, wide blue eyes, fair skin.
4. Varkhul: Varkhul, a dying old human knight of about sixty. A gaunt, noble face, grey eyes with the last faint ember glow fading from them, hollow cheeks, a grey beard, fair skin gone ashen.
5. Cube Cultist: a calm human cultist of about thirty. A calm, courteous face, pale eyes, light skin, grey ash smeared in a square pattern on the brow.
6. Sir Corwin Halbeck: Sir Corwin, a knight-commander of about forty-five. A square-jawed, certain face, clear blue eyes, clean-shaven, a small scar on the chin, fair skin.
7. Lord Severin Morvaine: Lord Severin, a vampire lord who looks about forty-five. A long, beautiful, very tired face, pale grey-white skin, dark-red eyes, high cheekbones, a straight nose, a mouth set in old sorrow, the tips of fangs barely showing.
8. Pell: Pell, a village lad of about seventeen from a cold, dark valley. A pale, anxious face, big dark eyes, chapped lips, freckles, very pale skin.
I'll save it as face-sheet-m06.png.

#### `face-sheet-m07` · Face sheet: 8 men (The blood-house steward, A Hollowmere child, Ruck, the bosun, Davin, Vorlaith, the Unmade King, Thrall, Drowned, Blood-wight) · P2

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-blood-house-steward`, 2 `face-hollowmere-child`, 3 `face-ruck`, 4 `face-davin`, 5 `face-vorlaith`, 6 `face-thrall`, 7 `face-drowned`, 8 `face-blood-wight`.

1. The blood-house steward: the steward of a vampire's manor, about fifty. A long, colourless face, pale watery eyes, thin lips pressed together, very pale skin.
2. A Hollowmere child: a village boy of about eight. A pale, suspicious child's face, narrowed grey eyes, a smudged nose.
3. Ruck, the bosun: Ruck, a ship's bosun of about fifty. A battered, good-natured face, small grey eyes, a nose broken twice, a grey chin beard, weathered dark skin.
4. Davin: Davin, a squire of about eighteen. An earnest, grieving face, steady brown eyes, a boyish jaw, light-brown skin.
5. Vorlaith, the Unmade King: Vorlaith, an ancient being of living glass. A head of clear, colourless glass cracked with fine branching fractures, a cold white light drifting inside it like smoke, smooth, calm, faintly amused features, and two points of white light for eyes.
6. Thrall: a living human soldier enslaved to a vampire lord. A pale, blank face, glassy dark-red eyes, two small healed bite marks on the side of the neck, very pale skin.
7. Drowned: a drowned corpse that walks out of the sea. A puffy, slack, grey-green waterlogged face, milky white eyes, the mouth hanging open, barnacles crusted on the jaw.
8. Blood-wight: an undead thing that walks under a darkened sun. A skull-like head of taut, papery ash-grey skin, sunken red eyes, long fangs, thin grey hair.
I'll save it as face-sheet-m07.png.

#### `face-sheet-m08` · Face sheet: 5 men (Sun-scorched, Thornling, Priest-construct's head, Crown-construct's head, The Unmade) · P2

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-sun-scorched`, 2 `face-thornling`, 3 `face-construct-priest`, 4 `face-construct-crown`, 5 `face-unmade`.

1. Sun-scorched: one of the burned dead of a desert massacre. A blackened face cracked like dried mud, the cracks glowing a faint hot gold, the features half gone, two pale, blazing eyes, the mouth open.
2. Thornling: a small, spiteful forest sprite. A knot of rough dark bark for a head, with a hollow, sly face, two glowing yellow-green eyes and a mouth like a split in a branch, and a crown of small twigs and new leaves.
3. Priest-construct's head: a stone guardian of a ruined sun temple. In place of a face, a round, polished bronze mirror, glinting, set inside a hood of sun-bleached sandstone carved like a priest's.
4. Crown-construct's head: a war machine shaped like a knight. No face: in its place, a stylised gold crown shape, with a violet light glowing behind the gaps between its points, on a neck of polished dark violet metal.
5. The Unmade: a soldier pulled out of time. Where the face should be, a smooth, featureless oval of dark, glossy glass reflecting a faint white light; the rest of the head and neck faded and slightly greyed, as if not quite here.
Leave cells 6 to 8 exactly as they are: bald, unchanged.
I'll save it as face-sheet-m08.png.

#### `face-sheet-f01` · Face sheet: 8 women (You, face 2, You, face 3, You, face 4, You, face 5, You, face 7, Sera Vantis, Mira Ashgrove, {sb}, aged eight) · P0

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-player-2`, 2 `face-player-3`, 3 `face-player-4`, 4 `face-player-5`, 5 `face-player-7`, 6 `face-sera`, 7 `face-mira`, 8 `face-sb-child`.

1. You, face 2: a young human woman of about twenty-four from a poor fishing cove. An open, likeable oval face, a straight nose, warm brown eyes with defined lashes, straight brows, a dusting of freckles over the nose, fair skin lightly browned by the sea.
2. You, face 3: a young human woman of about twenty-four, warm, quick and ambitious. A heart-shaped face, sharp dark-brown eyes under arched brows, the corner of the mouth lifted as if about to tease, fair skin with a little sea-sun on it.
3. You, face 4: a Hearthling woman of about thirty: a small people, but this is an adult's face, not a child's (the game's morph makes her small). A round, cheerful face, wide hazel eyes, a snub nose, rosy brown skin.
4. You, face 5: a Sylvan woman, ageless, who looks about thirty. A narrow face with high cheekbones, long, elegant pointed ears swept back from the sides of the head, pale silver-grey eyes, porcelain skin.
5. You, face 7: a Wildkin woman of the wolf line, about twenty-eight. A strong jaw, bright yellow-green eyes, a small scar on the chin, deep brown skin. Two tall, pointed wolf's ears stand on top of the head, furred grey-brown.
6. Sera Vantis: Sera, a Sylvan forest warden, about a hundred and forty but young for her people, so she looks a weathered thirty. A narrow, still, unsmiling face, high cheekbones, long pointed ears swept back, sharp green eyes, soft straight brows, porcelain skin with a little weather on it.
7. Mira Ashgrove: Mira, a gentle human archivist of twenty-six. A soft, round face, gentle brown eyes, soft brows a little worried, freckles across the nose and cheeks, fair skin, an ink smudge on one cheek.
8. {sb}, aged eight: the Soul-Bound as a girl of eight in a fishing cove. A sharp, grinning child's face, mischievous dark-brown eyes, a scab on the chin, fair skin pink from the wind.
I'll save it as face-sheet-f01.png.

#### `face-sheet-f02` · Face sheet: 8 women (Corra, Captain Odelle Marsh, Hedda, Lise, Yselle the Glassblower, Princess Elara Aurelian, Hessa the Barkeep, Netty the Net-Mender) · P0

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-corra`, 2 `face-odelle`, 3 `face-hedda`, 4 `face-refugee-child`, 5 `face-refugee-3`, 6 `face-elara`, 7 `face-anvil-barkeep`, 8 `face-netty`.

1. Corra: Corra, a human net-mender of about fifty, the Soul-Bound's mother. A weathered, kind face, a strong nose, steady grey eyes with deep lines at the corners, a mouth used to keeping quiet, tan skin.
2. Captain Odelle Marsh: Captain Odelle Marsh, a ship's captain of about forty-five. A long, stern face with laugh lines, brown eyes, a small gold ring in one ear, deep brown skin.
3. Hedda: Hedda, a human refugee mother of about thirty-five. A hollow-cheeked, fierce face, brown eyes ringed with tiredness, a set jaw, tan skin smudged with road dirt.
4. Lise: Lise, a refugee girl of twelve. A pale, serious child's face, big brown eyes, her mother's set jaw, a smudge of dirt on one cheek, tan skin.
5. Yselle the Glassblower: Yselle, a Stoneborn glassblower of about forty. A round, freckled face, amber eyes, a determined mouth, golden-tan skin with a little soot.
6. Princess Elara Aurelian: Princess Elara, a human woman of twenty-four, impatient and clever. An oval face, clear grey eyes with defined lashes, straight dark brows, a firm mouth that's about to interrupt someone, fair skin.
7. Hessa the Barkeep: Hessa, a Wildkin barkeep of the bear line, about forty. A broad, unimpressed face, small brown eyes, a heavy jaw, an old scar through the near eyebrow and down the cheek, golden-tan skin. Two small, round, furry black bear's ears on top of the head.
8. Netty the Net-Mender: Netty, a harbour net-mender of about fifty-five. A freckled, cheerful face, green eyes, laugh lines, tan skin.
I'll save it as face-sheet-f02.png.

#### `face-sheet-f03` · Face sheet: 8 women (Maud Tolley, Ilsa Fenwright, Sister Hesper, Wren, Master Imra Vell, Ranger Tamsin, Townsfolk face 2, Townsfolk face 4) · P0

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-maud`, 2 `face-ilsa`, 3 `face-hesper`, 4 `face-wren`, 5 `face-imra-vell`, 6 `face-tamsin`, 7 `face-town-2`, 8 `face-town-4`.

1. Maud Tolley: Maud Tolley, a harbour widow of about seventy. A soft, sad face, pale grey eyes, a gentle mouth, fair skin with fine lines.
2. Ilsa Fenwright: Ilsa, a human scroll clerk of about twenty-five. An earnest, slightly harried face, green eyes, a determined mouth, golden-tan skin.
3. Sister Hesper: Sister Hesper, a temple sister of about sixty. A calm, kind face, warm brown eyes, laugh lines, deep brown skin.
4. Wren: Wren, a Hearthling street pickpocket of about ten. A sly, freckled child's face, bright amber eyes, the hint of a gap-toothed grin, brown skin.
5. Master Imra Vell: Master Imra Vell, a human weapons-master of about fifty. A square, dry-humoured face, dark eyes with a glint, an old scar across the bridge of the nose, deep brown skin.
6. Ranger Tamsin: Ranger Tamsin, a Wildkin woman of the hawk line, about forty. A hawkish, level face, sharp amber eyes, a thin scar from cheekbone to jaw, tan skin. In place of ears, two swept-back tufts of barred brown-and-cream feathers grow from the top of the head.
7. Townsfolk face 2: a young human woman of about twenty-five. A sweet, dimpled face, green eyes, tan skin.
8. Townsfolk face 4: a human woman of about forty. A rosy, good-humoured face, warm brown eyes, fair skin.
I'll save it as face-sheet-f03.png.

#### `face-sheet-f04` · Face sheet: 8 women (Townsfolk face 6, Townsfolk face 8, Townsfolk face 10, Townsfolk face 13, Townsfolk face 15, Townsfolk face 18, Mira, aged thirteen, Archmage Thessaly Varn) · P0

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-town-6`, 2 `face-town-8`, 3 `face-town-10`, 4 `face-town-13`, 5 `face-town-15`, 6 `face-town-18`, 7 `face-mira-young`, 8 `face-thessaly`.

1. Townsfolk face 6: an old human woman of about seventy-five. A round, kindly face, sharp black eyes, many wrinkles, rosy fair skin.
2. Townsfolk face 8: a human girl of about eight. A bright child's face, big blue eyes, freckles, fair skin.
3. Townsfolk face 10: a human woman of about thirty. A neat, freckled face, blue eyes, fair skin.
4. Townsfolk face 13: a human woman of about forty-five. A serene, weathered face, dark eyes, a gentle smile, brown skin.
5. Townsfolk face 15: a Hearthling woman of about thirty (an adult's face). A round, busy face, bright hazel eyes, rosy fair skin.
6. Townsfolk face 18: a human woman of about thirty from a cold, shadowed valley. A pale, guarded face, dark eyes, a straight mouth, very pale skin.
7. Mira, aged thirteen: Mira as a girl of thirteen, new apprentice in a great archive. A rounder, younger face with the same freckles and gentle brown eyes, serious and proud, fair skin.
8. Archmage Thessaly Varn: Archmage Thessaly, a human woman of about sixty. A lean, sharp face, deep lines round the mouth, steady grey eyes that miss nothing, thin lips, fair skin.
I'll save it as face-sheet-f04.png.

#### `face-sheet-f05` · Face sheet: 8 women (Brenna of Ashfall, Mother Gannet, Hester Hollis, Pedlar Mags, Valencia Morvaine, Mother Superior Anselle, Sister Orla, Lady Isadora) · P1

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-brenna`, 2 `face-gannet`, 3 `face-hester`, 4 `face-mags`, 5 `face-valencia`, 6 `face-anselle`, 7 `face-orla`, 8 `face-isadora`.

1. Brenna of Ashfall: Brenna, a hard human fighter of about thirty-five from a burned country. A lean, hard face, grey eyes, a burn scar along the jaw, ash smudged on the cheeks and brow, fair skin weathered tan.
2. Mother Gannet: Mother Gannet, a human landlady of about sixty. A broad, shrewd face, heavy-lidded eyes, a small, tight smile, a double chin, ruddy fair skin.
3. Hester Hollis: Hester Hollis, a human woman of about eighty. A thin, kind face, deeply lined, sharp pale-blue eyes, a pointed chin, pale skin with age spots.
4. Pedlar Mags: Pedlar Mags, a Stoneborn woman of about sixty. A weathered, merry face, crinkled brown eyes, a broad nose, apple cheeks, ruddy tan skin.
5. Valencia Morvaine: Valencia, a vampire noblewoman who looks about twenty-five. A heart-shaped, elegant face, cool porcelain skin, grey eyes with a faint red ring round the iris, long lashes, arched brows, a dry, amused mouth with the tips of two small fangs just showing.
6. Mother Superior Anselle: Mother Anselle, the head of an abbey, about sixty-five. A round, shrewd face, bright hazel eyes, a small knowing smile, rosy fair skin with laugh lines.
7. Sister Orla: Sister Orla, an abbey beekeeper of about sixty. A broad, ruddy face, small cheerful blue eyes, a wide, frank mouth, sun-browned fair skin with freckles.
8. Lady Isadora: Lady Isadora, a noblewoman who looks about forty, remembered in an echo. A warm, practical face, gentle dark eyes, a straight nose, an amused mouth, pale skin.
I'll save it as face-sheet-f05.png.

#### `face-sheet-f06` · Face sheet: 6 women (Valencia as a child, Ide, Maren, Hollowmere's elder, A Hollowmere farmer, A Guild apprentice, Vreya) · P2

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-valencia-child`, 2 `face-ide`, 3 `face-maren-elder`, 4 `face-hollowmere-farmer`, 5 `face-guild-apprentice`, 6 `face-vreya`.

1. Valencia as a child: Valencia as a little girl of about eight. A small heart-shaped face, solemn, pale skin, grey eyes.
2. Ide: Ide, a young woman of about nineteen, rescued from a cellar. A sharp, pale face, dark shadows under sharp dark eyes, a tart, sideways mouth, very pale skin.
3. Maren, Hollowmere's elder: Maren, a village elder of about sixty-five. A stern, proud face, pale grey eyes, a thin mouth, deep lines, very pale skin.
4. A Hollowmere farmer: a village farm woman of about forty. A tired, stubborn face, dark eyes, weathered pale skin, a set mouth.
5. A Guild apprentice: a Mages Guild apprentice of about seventeen. A pale, frightened face, wide green eyes, freckles, light skin.
6. Vreya: Vreya, a vampire knight. A pale, gleeful face, red eyes, a mocking smirk, the tips of fangs showing, very pale skin.
Leave cells 7 and 8 exactly as they are: bald, unchanged.
I'll save it as face-sheet-f06.png.

### 4.1 The player and the Soul-Bound

#### `face-player-1` · You, face 1 (the default man) · P0

**Basis:** Edits `base-male-blank`. From the guide figure PLAYER (BASE); `Origins.cs` (`you`).

Edit the attached blank base image: change ONLY the face. Paint the face of a young human man of about twenty-four from a poor fishing cove onto the blank head: an open, likeable oval face, a straight nose, warm brown eyes under straight, expressive brows, clean-shaven, fair skin lightly browned by the sea. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-1.png.

#### `face-player-2` · You, face 2 (the default woman) · P0

**Basis:** Edits `base-female-blank`. **(derived)** From the guide's PLAYER (BASE) as a woman.

Edit the attached blank base image: change ONLY the face. Paint the face of a young human woman of about twenty-four from a poor fishing cove onto the blank head: an open, likeable oval face, a straight nose, warm brown eyes with defined lashes, straight brows, a dusting of freckles over the nose, fair skin lightly browned by the sea. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-2.png.

#### `face-player-3` · You, face 3 (the Soul-Bound's default) · P0

**Basis:** Edits `base-female-blank`. From the guide figure SERA (the dark ponytail); `Origins.cs` (`soulbound`: long black hair).

Edit the attached blank base image: change ONLY the face. Paint the face of a young human woman of about twenty-four, warm, quick and ambitious onto the blank head: a heart-shaped face, sharp dark-brown eyes under arched brows, the corner of the mouth lifted as if about to tease, fair skin with a little sea-sun on it. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-3.png.

#### `face-player-4` · You, face 4 (a Hearthling) · P0

**Basis:** Edits `base-female-blank`. **(derived)** From `Characters/LookOptions.cs` (the Hearthlings).

Edit the attached blank base image: change ONLY the face. Paint the face of a Hearthling woman of about thirty: a small people, but this is an adult's face, not a child's (the game's morph makes her small) onto the blank head: a round, cheerful face, wide hazel eyes, a snub nose, rosy brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-4.png.

#### `face-player-5` · You, face 5 (a Sylvan) · P0

**Basis:** Edits `base-female-blank`. **(derived)** From `Enums/Ancestry.cs` (Sylvans: long-lived, pointed ears).

Edit the attached blank base image: change ONLY the face. Paint the face of a Sylvan woman, ageless, who looks about thirty onto the blank head: a narrow face with high cheekbones, long, elegant pointed ears swept back from the sides of the head, pale silver-grey eyes, porcelain skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-5.png.

#### `face-player-6` · You, face 6 (an Emberkin) · P0

**Basis:** Edits `base-male-blank`. **(derived)** From `03-art-overhaul.md` §3.6 (`horns-emberkin-curled`).

Edit the attached blank base image: change ONLY the face. Paint the face of an Emberkin man of about twenty-two onto the blank head: a sharp, handsome face, amber eyes, a small fang showing at the corner of the mouth, tan skin. Two dark, ridged horns curl round from the temples like a ram's, old bone going ivory at the tips. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-6.png.

#### `face-player-7` · You, face 7 (a Wildkin) · P0

**Basis:** Edits `base-female-blank`. **(derived)** From `03-art-overhaul.md` §3.6 (`ears-wildkin-wolf`).

Edit the attached blank base image: change ONLY the face. Paint the face of a Wildkin woman of the wolf line, about twenty-eight onto the blank head: a strong jaw, bright yellow-green eyes, a small scar on the chin, deep brown skin. Two tall, pointed wolf's ears stand on top of the head, furred grey-brown. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-7.png.

#### `face-player-8` · You, face 8 (a Stoneborn) · P0

**Basis:** Edits `base-male-blank`. **(derived)** From `Enums/Ancestry.cs` (Stoneborn: mountain clans, short and broad).

Edit the attached blank base image: change ONLY the face. Paint the face of a Stoneborn man who looks about forty-five (the game's morph makes him short and broad, so draw the face at the base's size) onto the blank head: a broad, heavy face, deep-set grey eyes, a wide nose, weathered golden skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-player-8.png.

### 4.2 The party

#### `face-sera` · Sera Vantis · P0

**Basis:** Edits `base-female-blank`. From the guide figure KIAN; `03-art-overhaul.md` §7.4 (one braid to the waist); `COMPANIONS.md`.

Edit the attached blank base image: change ONLY the face. Paint the face of Sera, a Sylvan forest warden, about a hundred and forty but young for her people, so she looks a weathered thirty onto the blank head: a narrow, still, unsmiling face, high cheekbones, long pointed ears swept back, sharp green eyes, soft straight brows, porcelain skin with a little weather on it. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-sera.png.

#### `face-mira` · Mira Ashgrove · P0

**Basis:** Edits `base-female-blank`. From the guide figure NOREL; `03-art-overhaul.md` §7.4 (hair tied up with a pen through it); `COMPANIONS.md` (twenty-six).

Edit the attached blank base image: change ONLY the face. Paint the face of Mira, a gentle human archivist of twenty-six onto the blank head: a soft, round face, gentle brown eyes, soft brows a little worried, freckles across the nose and cheeks, fair skin, an ink smudge on one cheek. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-mira.png.

#### `face-mira-young` · Mira, aged thirteen · P1

**Basis:** Edits `base-female-blank`. **(derived)** From `04-script-prologue-act1.md` A1-C8-01 (`echo mira-young`).

Edit the attached blank base image: change ONLY the face. Paint the face of Mira as a girl of thirteen, new apprentice in a great archive onto the blank head: a rounder, younger face with the same freckles and gentle brown eyes, serious and proud, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-mira-young.png.

#### `face-kael` · Kael Duskmantle · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (swept horns); `Origins.cs` (Emberkin, swept horns, amber eyes); `COMPANIONS.md` (twenty-three).

Edit the attached blank base image: change ONLY the face. Paint the face of Kael, an Emberkin thief of twenty-three onto the blank head: a sharp, handsome face, amber eyes, straight brows, a small fang just showing, tan skin. Two dark, ridged horns sweep straight back from the temples in a long curve, going pale like old ivory at the tips. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-kael.png.

#### `face-brannock` · Brannock Ironvein · P0

**Basis:** Edits `base-male-blank`. From the guide figure CERRO; `03-art-overhaul.md` §7.4 (a braided beard to the belt with two gold clasps); `COMPANIONS.md` (nearly two hundred).

Edit the attached blank base image: change ONLY the face. Paint the face of Brannock, a Stoneborn old soldier nearly two hundred years old, which on a Stoneborn looks like a hard-weathered sixty onto the blank head: a square, heavy face, a nose broken more than once, deep-set brown eyes under thick, fierce brows, old scars, golden-tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-brannock.png.

#### `face-valencia` · Valencia Morvaine · P2

**Basis:** Edits `base-female-blank`. From the guide figure VALENCIA (silver hair); `COMPANIONS.md` (a Dawnborn vampire who looks twenty-five).

Edit the attached blank base image: change ONLY the face. Paint the face of Valencia, a vampire noblewoman who looks about twenty-five onto the blank head: a heart-shaped, elegant face, cool porcelain skin, grey eyes with a faint red ring round the iris, long lashes, arched brows, a dry, amused mouth with the tips of two small fangs just showing. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-valencia.png.

#### `face-mc-child` · You, aged eight · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (the children on the pier: the player's look at eight); the default `face-player-1`.

Edit the attached blank base image: change ONLY the face. Paint the face of the player as a boy of eight in a fishing cove (the game's morph makes him child-sized) onto the blank head: a round, freckled child's face, sunburn across the nose, big honest brown eyes, a gap in his front teeth, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-mc-child.png.

#### `face-sb-child` · {sb}, aged eight · P0

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4; the default `face-player-3`.

Edit the attached blank base image: change ONLY the face. Paint the face of the Soul-Bound as a girl of eight in a fishing cove onto the blank head: a sharp, grinning child's face, mischievous dark-brown eyes, a scab on the chin, fair skin pink from the wind. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-sb-child.png.

#### `face-brannock-bare` · Brannock, the clasps gone · P2

**Basis:** Edits `face-brannock`. From `03-art-overhaul.md` §7.4 (none after A3-C2-05).

Edit the attached image (face-brannock.png, the face on the base): change ONLY the beard's clasps: the two gold clasps are gone from the braid, which is tied with plain cord instead. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-brannock-bare.png.

### 4.3 Act 1

#### `face-corra` · Corra · P0

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4; `Cast.cs` (tan skin, grey eyes, chestnut hair).

Edit the attached blank base image: change ONLY the face. Paint the face of Corra, a human net-mender of about fifty, the Soul-Bound's mother onto the blank head: a weathered, kind face, a strong nose, steady grey eyes with deep lines at the corners, a mouth used to keeping quiet, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-corra.png.

#### `face-lookout` · The lookout · P0

**Basis:** Edits `base-male-blank`. **(derived)** From `04-script-prologue-act1.md` P-04.

Edit the attached blank base image: change ONLY the face. Paint the face of a wiry human sailor of about thirty onto the blank head: a narrow face, sun-dark brown skin, sharp pale-grey eyes used to squinting at the horizon, a short scruffy beard. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-lookout.png.

#### `face-odelle` · Captain Odelle Marsh · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (brown skin, a stern face, long black hair); the voice sheet ("salt and cheek").

Edit the attached blank base image: change ONLY the face. Paint the face of Captain Odelle Marsh, a ship's captain of about forty-five onto the blank head: a long, stern face with laugh lines, brown eyes, a small gold ring in one ear, deep brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-odelle.png.

#### `face-tomas` · Old Tomas · P0

**Basis:** Edits `base-male-blank`. From (derived in part) the voice sheet ("fifty years a fisherman").

Edit the attached blank base image: change ONLY the face. Paint the face of Old Tomas, a human fisherman of about seventy onto the blank head: a weathered, deeply lined face, brown-tan skin, bushy white eyebrows over squinting blue-grey eyes, a short white stubble beard, a big nose reddened by the wind. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-tomas.png.

#### `face-red-gull` · Red Gull Thug, face 1 · P0

**Basis:** Edits `base-male-blank`. From the guide figure KELL; the art plan (three seeded heads).

Edit the attached blank base image: change ONLY the face. Paint the face of a harbour gang thug of about thirty onto the blank head: a brutal, smug face, a heavy jaw with dark stubble, small hard eyes under a thick brow, a flattened nose, deep brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-red-gull.png.

#### `face-red-gull-2` · Red Gull Thug, face 2 · P0

**Basis:** Edits `base-male-blank`. From the art plan (the second of three heads).

Edit the attached blank base image: change ONLY the face. Paint the face of another harbour gang thug, about thirty onto the blank head: a broken nose, a scraggly ginger beard, pale eyes, light, freckled skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-red-gull-2.png.

#### `face-red-gull-3` · Red Gull Thug, face 3 · P0

**Basis:** Edits `base-male-blank`. From the art plan (the third of three heads).

Edit the attached blank base image: change ONLY the face. Paint the face of a third harbour gang thug, about twenty-five onto the blank head: a narrow, sneering face, a gold tooth, dark eyes, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-red-gull-3.png.

#### `face-grell` · Bosun Grell · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (bald); `Cast.cs` (Stoneborn, a full beard).

Edit the attached blank base image: change ONLY the face. Paint the face of Bosun Grell, a Stoneborn gang boss of middle age onto the blank head: a broad, mean face, small fierce eyes under heavy brows, a puckered scar across one cheek, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-grell.png.

#### `face-hale` · Captain Roderic Hale · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (fair skin, grey eyes, grey hair, stubble); the voice sheet.

Edit the attached blank base image: change ONLY the face. Paint the face of Captain Hale of the City Watch, a human man of about fifty-two onto the blank head: a square, tired face, grey eyes with heavy bags under them, a short grey stubble beard, a crooked nose, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hale.png.

#### `face-fennick` · Questmaster Fennick · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs`; the voice sheet ("pedantic, kind"); the spectacles are (derived).

Edit the attached blank base image: change ONLY the face. Paint the face of Questmaster Fennick, a human clerk of about fifty-eight onto the blank head: a round, pleasant face, pale skin, grey eyes behind small round spectacles, a neat grey moustache, a satisfied little smile. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-fennick.png.

#### `face-hedda` · Hedda · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (tan skin, brown eyes, chestnut hair tied back); the map's `refugee-mother` uses it.

Edit the attached blank base image: change ONLY the face. Paint the face of Hedda, a human refugee mother of about thirty-five onto the blank head: a hollow-cheeked, fierce face, brown eyes ringed with tiredness, a set jaw, tan skin smudged with road dirt. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hedda.png.

#### `face-refugee-child` · Lise · P0

**Basis:** Edits `base-female-blank`. **(derived)** From `Cast.cs` (Lise, Hedda's daughter).

Edit the attached blank base image: change ONLY the face. Paint the face of Lise, a refugee girl of twelve onto the blank head: a pale, serious child's face, big brown eyes, her mother's set jaw, a smudge of dirt on one cheek, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-refugee-child.png.

#### `face-brisk` · Sergeant Brisk · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (brown skin, brown eyes, black hair, a moustache).

Edit the attached blank base image: change ONLY the face. Paint the face of Sergeant Brisk of the City Watch, a human man of about forty onto the blank head: a worried, harried face, brown eyes, a thick black moustache, a five-o'clock shadow, brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-brisk.png.

#### `face-refugee-1` · Old Aurek · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (an old orchard farmer; bald, a beard).

Edit the attached blank base image: change ONLY the face. Paint the face of Old Aurek, a sick refugee farmer of about seventy-five onto the blank head: a gaunt, feverish face, kind grey eyes, a long silver beard, tan skin with deep lines. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-refugee-1.png.

#### `face-refugee-2` · Teodor · P0

**Basis:** Edits `base-male-blank`. **(derived)** From `Cast.cs` (brown skin, tousled black hair, stubble).

Edit the attached blank base image: change ONLY the face. Paint the face of Teodor, a refugee lad of about sixteen with a fever onto the blank head: a face flushed with fever, sweat on the brow, tired brown eyes, the first fuzz of a moustache, brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-refugee-2.png.

#### `face-refugee-3` · Yselle the Glassblower · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (Stoneborn; golden skin, amber eyes, freckles, a copper braid).

Edit the attached blank base image: change ONLY the face. Paint the face of Yselle, a Stoneborn glassblower of about forty onto the blank head: a round, freckled face, amber eyes, a determined mouth, golden-tan skin with a little soot. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-refugee-3.png.

#### `face-elara` · Princess Elara Aurelian · P0

**Basis:** Edits `base-female-blank`. From the guide figure ELARA (short dark hair); `Cast.cs` (grey eyes); `COMPANIONS.md` (twenty-four).

Edit the attached blank base image: change ONLY the face. Paint the face of Princess Elara, a human woman of twenty-four, impatient and clever onto the blank head: an oval face, clear grey eyes with defined lashes, straight dark brows, a firm mouth that's about to interrupt someone, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-elara.png.

#### `face-leon` · King Leon · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4; `Cast.cs` (silver hair and beard); the voice sheet.

Edit the attached blank base image: change ONLY the face. Paint the face of King Leon, a human man of about sixty-eight, kind and tired onto the blank head: a broad, gentle face, grey eyes with deep laugh lines, thick grey brows, a full, neatly trimmed silver beard, fair skin with the flush of age. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-leon.png.

#### `face-crane` · Lord Chancellor Vesper Crane · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (slicked hair); `Cast.cs` (porcelain skin, grey eyes, a moustache).

Edit the attached blank base image: change ONLY the face. Paint the face of Lord Chancellor Crane, a human man of about fifty-five onto the blank head: a long, narrow face, high cheekbones, pale porcelain skin, sharp grey eyes, finely arched brows, a thin black moustache, a polite smile that doesn't reach his eyes. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-crane.png.

#### `face-anvil-barkeep` · Hessa the Barkeep · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (a Wildkin of the bear line; golden skin, a scarred face, black hair tied back).

Edit the attached blank base image: change ONLY the face. Paint the face of Hessa, a Wildkin barkeep of the bear line, about forty onto the blank head: a broad, unimpressed face, small brown eyes, a heavy jaw, an old scar through the near eyebrow and down the cheek, golden-tan skin. Two small, round, furry black bear's ears on top of the head. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-anvil-barkeep.png.

#### `face-netty` · Netty the Net-Mender · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (tan skin, freckles, green eyes, a silver braid).

Edit the attached blank base image: change ONLY the face. Paint the face of Netty, a harbour net-mender of about fifty-five onto the blank head: a freckled, cheerful face, green eyes, laugh lines, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-netty.png.

#### `face-maud` · Maud Tolley · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (fair skin, grey eyes, silver hair in a bun).

Edit the attached blank base image: change ONLY the face. Paint the face of Maud Tolley, a harbour widow of about seventy onto the blank head: a soft, sad face, pale grey eyes, a gentle mouth, fair skin with fine lines. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-maud.png.

#### `face-pip` · Pip the Potboy · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (a Hearthling; fair, freckled, copper hair).

Edit the attached blank base image: change ONLY the face. Paint the face of Pip, a Hearthling potboy of about seventeen (an adult face, not a child's) onto the blank head: a freckled, nervous face, wide brown eyes, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-pip.png.

#### `face-osric` · Osric Penhallow · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (porcelain skin, brown eyes, black hair, a moustache).

Edit the attached blank base image: change ONLY the face. Paint the face of Osric, a human silk merchant of about forty-five onto the blank head: a neat, anxious face, brown eyes glancing sideways, a carefully trimmed black moustache, porcelain-pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-osric.png.

#### `face-lark` · Lark the Busker · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (a Sylvan; rose hair, lilac eyes).

Edit the attached blank base image: change ONLY the face. Paint the face of Lark, a young-looking Sylvan street singer onto the blank head: a bright, cheeky face, pale lilac-grey eyes, a performer's half-smile, porcelain skin, long pointed ears. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-lark.png.

#### `face-ilsa` · Ilsa Fenwright · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (golden skin, green eyes, a gold braid).

Edit the attached blank base image: change ONLY the face. Paint the face of Ilsa, a human scroll clerk of about twenty-five onto the blank head: an earnest, slightly harried face, green eyes, a determined mouth, golden-tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-ilsa.png.

#### `face-oswin` · Guardsman Oswin · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (fair skin, blue eyes, gold hair).

Edit the attached blank base image: change ONLY the face. Paint the face of Oswin, a young human palace guard of about twenty-two onto the blank head: a very straight-faced, serious face, blue eyes, clean-shaven, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-oswin.png.

#### `face-hesper` · Sister Hesper · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (deep skin, brown eyes, long silver hair).

Edit the attached blank base image: change ONLY the face. Paint the face of Sister Hesper, a temple sister of about sixty onto the blank head: a calm, kind face, warm brown eyes, laugh lines, deep brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hesper.png.

#### `face-wren` · Wren · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (a Hearthling; brown skin, freckles, amber eyes, black hair).

Edit the attached blank base image: change ONLY the face. Paint the face of Wren, a Hearthling street pickpocket of about ten onto the blank head: a sly, freckled child's face, bright amber eyes, the hint of a gap-toothed grin, brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-wren.png.

#### `face-imra-vell` · Master Imra Vell · P0

**Basis:** Edits `base-female-blank`. **(derived)** From `10-classes.md` §5.1.

Edit the attached blank base image: change ONLY the face. Paint the face of Master Imra Vell, a human weapons-master of about fifty onto the blank head: a square, dry-humoured face, dark eyes with a glint, an old scar across the bridge of the nose, deep brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-imra-vell.png.

#### `face-tamsin` · Ranger Tamsin · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (a Wildkin of the hawk line; tan skin, a scarred face, amber eyes, chestnut hair tied back).

Edit the attached blank base image: change ONLY the face. Paint the face of Ranger Tamsin, a Wildkin woman of the hawk line, about forty onto the blank head: a hawkish, level face, sharp amber eyes, a thin scar from cheekbone to jaw, tan skin. In place of ears, two swept-back tufts of barred brown-and-cream feathers grow from the top of the head. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-tamsin.png.

#### `face-garrick` · Garrick the caravan master · P1

**Basis:** Edits `base-male-blank`. **(derived)** From A1-C5-01.

Edit the attached blank base image: change ONLY the face. Paint the face of Garrick, a human caravan master of about fifty onto the blank head: a lean, leathery face, tan skin with squint lines, brown eyes, a grey-shot black moustache, an old scar through one eyebrow. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-garrick.png.

#### `face-aldric` · Commander Aldric Voss · P1

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (grey stubble); the voice sheet.

Edit the attached blank base image: change ONLY the face. Paint the face of Commander Aldric Voss, an old human soldier of about sixty-five onto the blank head: a craggy, humorous face, pale-blue eyes with deep crow's feet, a grey stubble beard, an old notch missing from one ear, weathered fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-aldric.png.

#### `face-recruit` · The recruit · P1

**Basis:** Edits `base-male-blank`. **(derived)** From A1-C7-01.

Edit the attached blank base image: change ONLY the face. Paint the face of a brand-new human soldier of about seventeen onto the blank head: an eager, open face, red-cheeked from the cold, blue eyes, a few spots, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-recruit.png.

#### `face-thessaly` · Archmage Thessaly Varn · P1

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4 (cropped grey hair); the voice sheet.

Edit the attached blank base image: change ONLY the face. Paint the face of Archmage Thessaly, a human woman of about sixty onto the blank head: a lean, sharp face, deep lines round the mouth, steady grey eyes that miss nothing, thin lips, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-thessaly.png.

#### `face-brenna` · Brenna of Ashfall · P1

**Basis:** Edits `base-female-blank`. From (derived in part) the voice sheet; A1-C7-05 (Varkhul's daughter).

Edit the attached blank base image: change ONLY the face. Paint the face of Brenna, a hard human fighter of about thirty-five from a burned country onto the blank head: a lean, hard face, grey eyes, a burn scar along the jaw, ash smudged on the cheeks and brow, fair skin weathered tan. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-brenna.png.

#### `face-gannet` · Mother Gannet · P1

**Basis:** Edits `base-female-blank`. **(derived)** From `03-art-overhaul.md` §7.4.

Edit the attached blank base image: change ONLY the face. Paint the face of Mother Gannet, a human landlady of about sixty onto the blank head: a broad, shrewd face, heavy-lidded eyes, a small, tight smile, a double chin, ruddy fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-gannet.png.

#### `face-hester` · Hester Hollis · P1

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4 (a grey bun).

Edit the attached blank base image: change ONLY the face. Paint the face of Hester Hollis, a human woman of about eighty onto the blank head: a thin, kind face, deeply lined, sharp pale-blue eyes, a pointed chin, pale skin with age spots. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hester.png.

#### `face-tobin` · Tobin Ashgrove · P1

**Basis:** Edits `base-male-blank`. **(derived)** From `COMPANIONS.md` (Mira's father).

Edit the attached blank base image: change ONLY the face. Paint the face of Tobin, a human cider-presser of about fifty-five onto the blank head: a gentle, round face, kind brown eyes, freckles, a short greying copper-red beard, fair skin reddened by the sun. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-tobin.png.

#### `face-harrow` · Lord Harrow of Heronsgate · P1

**Basis:** Edits `base-male-blank`. **(derived)** From the voice sheet ("never looks at the person he's ruining").

Edit the attached blank base image: change ONLY the face. Paint the face of Lord Harrow, a bored human lord of about fifty-five onto the blank head: a bland, pale face, heavy-lidded eyes looking off to one side at nothing, grey side-whiskers, a small, polite, empty smile. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-harrow.png.

#### `face-hobb` · Hobb the woodcutter · P1

**Basis:** Edits `base-male-blank`. From `08-random-encounters.md` ("He's big. He's got a red beard.").

Edit the attached blank base image: change ONLY the face. Paint the face of Hobb, a human woodcutters' foreman of about forty-five onto the blank head: a broad, ruddy face, worried blue eyes, a big bushy red beard, freckled fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hobb.png.

#### `face-maleficio` · Maleficio · P1

**Basis:** Edits `base-male-blank`. **(derived)** From `03-art-overhaul.md` §7.4; the voice sheet ("a teacher").

Edit the attached blank base image: change ONLY the face. Paint the face of Maleficio, a gentle old human scholar of about sixty-five onto the blank head: a kind, lined face, tired dark eyes, a close-trimmed grey beard, a high forehead, pale skin that hasn't seen the sun in years. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-maleficio.png.

#### `face-lost-boy` · The woodcutter's boy · P1

**Basis:** Edits `base-male-blank`. **(derived)** From `08-random-encounters.md` (`enc-woodcutters-boy`: Hobb's son).

Edit the attached blank base image: change ONLY the face. Paint the face of a lost human boy of about eight onto the blank head: a freckled, tear-streaked child's face, wide blue eyes, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-lost-boy.png.

#### `face-mags` · Pedlar Mags · P1

**Basis:** Edits `base-female-blank`. **(derived)** From `08-random-encounters.md` (`enc-mags`: a Stoneborn woman).

Edit the attached blank base image: change ONLY the face. Paint the face of Pedlar Mags, a Stoneborn woman of about sixty onto the blank head: a weathered, merry face, crinkled brown eyes, a broad nose, apple cheeks, ruddy tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-mags.png.

#### `face-corra-flashback` · Corra, sixteen years younger · P0

**Basis:** Edits `face-corra`. From `03-art-overhaul.md` §7.4 (a flashback version, sixteen years younger); P-03.

Edit the attached image (face-corra.png, the face on the base): change ONLY her age: make her sixteen years younger, about thirty-four, with no grey in the chestnut hair, fewer lines, and the hair damp and coming loose from the rain. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-corra-flashback.png.

#### `face-maleficio-young` · Maleficio, remembered · P1

**Basis:** Edits `face-maleficio`. From `03-art-overhaul.md` §7.4 (`maleficio-young`, an echo); A1-C8-01.

Edit the attached image (face-maleficio.png, the face on the base): change ONLY his age: make him twenty years younger, about forty-five, the face fuller and warmer, the beard short and dark with grey only at the chin, the hair dark with grey at the temples. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-maleficio-young.png.

### 4.4 Act 2

#### `face-anselle` · Mother Superior Anselle · P2

**Basis:** Edits `base-female-blank`. **(derived)** From the voice sheet ("knows more than she says").

Edit the attached blank base image: change ONLY the face. Paint the face of Mother Anselle, the head of an abbey, about sixty-five onto the blank head: a round, shrewd face, bright hazel eyes, a small knowing smile, rosy fair skin with laugh lines. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-anselle.png.

#### `face-orla` · Sister Orla · P2

**Basis:** Edits `base-female-blank`. **(derived)** From the voice sheet ("practical, unembarrassed").

Edit the attached blank base image: change ONLY the face. Paint the face of Sister Orla, an abbey beekeeper of about sixty onto the blank head: a broad, ruddy face, small cheerful blue eyes, a wide, frank mouth, sun-browned fair skin with freckles. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-orla.png.

#### `face-corwin` · Sir Corwin Halbeck · P2

**Basis:** Edits `base-male-blank`. **(derived)** From the voice sheet ("liturgy and certainty").

Edit the attached blank base image: change ONLY the face. Paint the face of Sir Corwin, a knight-commander of about forty-five onto the blank head: a square-jawed, certain face, clear blue eyes, clean-shaven, a small scar on the chin, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-corwin.png.

#### `face-severin` · Lord Severin Morvaine · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (silver hair to the shoulders); the voice sheet ("grieving").

Edit the attached blank base image: change ONLY the face. Paint the face of Lord Severin, a vampire lord who looks about forty-five onto the blank head: a long, beautiful, very tired face, pale grey-white skin, dark-red eyes, high cheekbones, a straight nose, a mouth set in old sorrow, the tips of fangs barely showing. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-severin.png.

#### `face-isadora` · Lady Isadora · P2

**Basis:** Edits `base-female-blank`. **(derived)** From the voice sheet ("warm, practical").

Edit the attached blank base image: change ONLY the face. Paint the face of Lady Isadora, a noblewoman who looks about forty, remembered in an echo onto the blank head: a warm, practical face, gentle dark eyes, a straight nose, an amused mouth, pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-isadora.png.

#### `face-valencia-child` · Valencia as a child · P2

**Basis:** Edits `base-female-blank`. **(derived)** From A2-C3-03 (`echo valencia-child`).

Edit the attached blank base image: change ONLY the face. Paint the face of Valencia as a little girl of about eight onto the blank head: a small heart-shaped face, solemn, pale skin, grey eyes. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-valencia-child.png.

#### `face-pell` · Pell · P2

**Basis:** Edits `base-male-blank`. **(derived)** From A2-C3-02; the Morvaine pool (pale skin).

Edit the attached blank base image: change ONLY the face. Paint the face of Pell, a village lad of about seventeen from a cold, dark valley onto the blank head: a pale, anxious face, big dark eyes, chapped lips, freckles, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-pell.png.

#### `face-ide` · Ide · P2

**Basis:** Edits `base-female-blank`. **(derived)** From `08-random-encounters.md` §5.1.

Edit the attached blank base image: change ONLY the face. Paint the face of Ide, a young woman of about nineteen, rescued from a cellar onto the blank head: a sharp, pale face, dark shadows under sharp dark eyes, a tart, sideways mouth, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-ide.png.

#### `face-maren-elder` · Maren, Hollowmere's elder · P2

**Basis:** Edits `base-female-blank`. **(derived)** From A2-C3-02.

Edit the attached blank base image: change ONLY the face. Paint the face of Maren, a village elder of about sixty-five onto the blank head: a stern, proud face, pale grey eyes, a thin mouth, deep lines, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-maren-elder.png.

#### `face-blood-house-steward` · The blood-house steward · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `08-random-encounters.md` §5.1.

Edit the attached blank base image: change ONLY the face. Paint the face of the steward of a vampire's manor, about fifty onto the blank head: a long, colourless face, pale watery eyes, thin lips pressed together, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-blood-house-steward.png.

#### `face-hollowmere-child` · A Hollowmere child · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `enc-hollowmere-child`.

Edit the attached blank base image: change ONLY the face. Paint the face of a village boy of about eight onto the blank head: a pale, suspicious child's face, narrowed grey eyes, a smudged nose. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hollowmere-child.png.

#### `face-hollowmere-farmer` · A Hollowmere farmer · P2

**Basis:** Edits `base-female-blank`. **(derived)** From `enc-tithe-riders` (a goodwife).

Edit the attached blank base image: change ONLY the face. Paint the face of a village farm woman of about forty onto the blank head: a tired, stubborn face, dark eyes, weathered pale skin, a set mouth. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hollowmere-farmer.png.

#### `face-ruck` · Ruck, the bosun · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `10-classes.md` (`train-odelle`).

Edit the attached blank base image: change ONLY the face. Paint the face of Ruck, a ship's bosun of about fifty onto the blank head: a battered, good-natured face, small grey eyes, a nose broken twice, a grey chin beard, weathered dark skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-ruck.png.

### 4.5 Act 3

#### `face-davin` · Davin · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `03-art-overhaul.md` §7.4 (young).

Edit the attached blank base image: change ONLY the face. Paint the face of Davin, a squire of about eighteen onto the blank head: an earnest, grieving face, steady brown eyes, a boyish jaw, light-brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-davin.png.

#### `face-vorlaith` · Vorlaith, the Unmade King · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (cracked glass with light inside).

Edit the attached blank base image: change ONLY the face. Paint the face of Vorlaith, an ancient being of living glass onto the blank head: a head of clear, colourless glass cracked with fine branching fractures, a cold white light drifting inside it like smoke, smooth, calm, faintly amused features, and two points of white light for eyes. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-vorlaith.png.

#### `face-guild-apprentice` · A Guild apprentice · P2

**Basis:** Edits `base-female-blank`. **(derived)** From A3-C2-01.

Edit the attached blank base image: change ONLY the face. Paint the face of a Mages Guild apprentice of about seventeen onto the blank head: a pale, frightened face, wide green eyes, freckles, light skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-guild-apprentice.png.

#### `face-refugee-child-aged` · Lise, aged by the tear · P2

**Basis:** Edits `face-refugee-child`. From `08-random-encounters.md` §6 ("I'm forty-six").

Edit the attached image (face-refugee-child.png, the face on the base): change ONLY her age: make her a woman of forty-six, as if decades passed in an instant: the same face grown up and lined, grey threads in the two chestnut plaits. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-refugee-child-aged.png.

#### `face-leon-ill` · King Leon, ill · P2

**Basis:** Edits `face-leon`. From `03-art-overhaul.md` §7.4 (Leon, ill, Act 3).

Edit the attached image (face-leon.png, the face on the base): change ONLY his health: he's ill now, thinner, his face grey and drawn, the beard less tidy, his eyes still kind. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-leon-ill.png.

#### `face-severin-cell` · Severin, a prisoner · P2

**Basis:** Edits `face-severin`. From `03-art-overhaul.md` §7.4 (`severin-cell`: thin, grey).

Edit the attached image (face-severin.png, the face on the base): change ONLY his condition: much thinner, the skin gone grey, the silver hair lank and dull, the eyes dimmed. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-severin-cell.png.

### 4.6 Townsfolk

#### `face-town-1` · Townsfolk face 1 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From `03-art-overhaul.md` §7.5 (the generic pools).

Edit the attached blank base image: change ONLY the face. Paint the face of a young human farmhand of about twenty onto the blank head: an open, wide-eyed face, blue eyes, sunburnt cheeks, a peeling nose, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-1.png.

#### `face-town-2` · Townsfolk face 2 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a young human woman of about twenty-five onto the blank head: a sweet, dimpled face, green eyes, tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-2.png.

#### `face-town-3` · Townsfolk face 3 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a strong human man of about forty onto the blank head: a strong, calm face, dark eyes, a short black beard, deep brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-3.png.

#### `face-town-4` · Townsfolk face 4 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human woman of about forty onto the blank head: a rosy, good-humoured face, warm brown eyes, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-4.png.

#### `face-town-5` · Townsfolk face 5 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of an old human man of about seventy-five onto the blank head: a grumpy, twinkly face, small grey eyes, a big nose, white mutton-chop whiskers, fair, ruddy skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-5.png.

#### `face-town-6` · Townsfolk face 6 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of an old human woman of about seventy-five onto the blank head: a round, kindly face, sharp black eyes, many wrinkles, rosy fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-6.png.

#### `face-town-7` · Townsfolk face 7 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human boy of about eight onto the blank head: a cheeky child's face, brown eyes, a missing front tooth, a scraped nose, light-brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-7.png.

#### `face-town-8` · Townsfolk face 8 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human girl of about eight onto the blank head: a bright child's face, big blue eyes, freckles, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-8.png.

#### `face-town-9` · Townsfolk face 9 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human boy of about fifteen onto the blank head: an eager, conspiratorial face, green eyes, a few freckles, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-9.png.

#### `face-town-10` · Townsfolk face 10 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human woman of about thirty onto the blank head: a neat, freckled face, blue eyes, fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-10.png.

#### `face-town-11` · Townsfolk face 11 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human man of about thirty onto the blank head: a tired, good-natured face, dark eyes, stubble, olive skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-11.png.

#### `face-town-12` · Townsfolk face 12 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a weathered human man of about fifty onto the blank head: a loud, cheerful face, bright brown eyes, a big grey-shot black beard, weathered olive skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-12.png.

#### `face-town-13` · Townsfolk face 13 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human woman of about forty-five onto the blank head: a serene, weathered face, dark eyes, a gentle smile, brown skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-13.png.

#### `face-town-14` · Townsfolk face 14 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5.

Edit the attached blank base image: change ONLY the face. Paint the face of a human man of about thirty-five onto the blank head: a tired, friendly face, grey eyes, a few days of brown stubble, weathered fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-14.png.

#### `face-town-15` · Townsfolk face 15 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5; the Hearthlings.

Edit the attached blank base image: change ONLY the face. Paint the face of a Hearthling woman of about thirty (an adult's face) onto the blank head: a round, busy face, bright hazel eyes, rosy fair skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-15.png.

#### `face-town-16` · Townsfolk face 16 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5; the Sylvans.

Edit the attached blank base image: change ONLY the face. Paint the face of a Sylvan man who looks about thirty onto the blank head: a narrow, calm face, long pointed ears, grey-green eyes, porcelain skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-16.png.

#### `face-town-17` · Townsfolk face 17 · P0

**Basis:** Edits `base-male-blank`. **(derived)** From §7.5; the Stoneborn.

Edit the attached blank base image: change ONLY the face. Paint the face of a Stoneborn man who looks about fifty onto the blank head: a broad face, deep-set brown eyes, ruddy tan skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-17.png.

#### `face-town-18` · Townsfolk face 18 · P0

**Basis:** Edits `base-female-blank`. **(derived)** From §7.5; the Morvaine pool (pale skin).

Edit the attached blank base image: change ONLY the face. Paint the face of a human woman of about thirty from a cold, shadowed valley onto the blank head: a pale, guarded face, dark eyes, a straight mouth, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-town-18.png.

### 4.7 Humanoid foes

#### `face-varkhul` · Varkhul · P1

**Basis:** Edits `base-male-blank`. From `04-script-prologue-act1.md` A1-C6 ("Inside is a grey-haired man"); Brenna's father.

Edit the attached blank base image: change ONLY the face. Paint the face of Varkhul, a dying old human knight of about sixty onto the blank head: a gaunt, noble face, grey eyes with the last faint ember glow fading from them, hollow cheeks, a grey beard, fair skin gone ashen. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-varkhul.png.

#### `face-cube-cultist` · Cube Cultist · P1

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (ash on the face).

Edit the attached blank base image: change ONLY the face. Paint the face of a calm human cultist of about thirty onto the blank head: a calm, courteous face, pale eyes, light skin, grey ash smeared in a square pattern on the brow. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-cube-cultist.png.

#### `face-hollowed` · The Hollowed · P0

**Basis:** Edits `base-male-blank`. From the art plan (ash skin, cracks lit ember, empty sockets with ember points); `03-art-overhaul.md` §7.3.

Edit the attached blank base image: change ONLY the face. Paint the face of a Hollowed, a dead soldier raised as an empty husk onto the blank head: a gaunt, lipless head with skin like ash-grey burnt paper, cracked all over in fine lines that glow ember orange from inside; empty, dark eye sockets with a single ember-orange point of light in each; a few wisps of scorched hair. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hollowed.png.

#### `face-construct-vault` · Vault construct's head · P0

**Basis:** Edits `base-male-blank`. From the art plan (a sun-disc face).

Edit the attached blank base image: change ONLY the face. Paint the face of an ancient stone guardian: the head is not a face at all onto the blank head: in place of the head, a flat, round disc of polished sun-gold metal with a simple sunburst engraved on it and no other features, set on a short neck of pale dressed stone with thin gold inlay. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-vault.png.

#### `face-vreya` · Vreya · P2

**Basis:** Edits `base-female-blank`. **(derived)** From the voice sheet (sing-song, mocking).

Edit the attached blank base image: change ONLY the face. Paint the face of Vreya, a vampire knight onto the blank head: a pale, gleeful face, red eyes, a mocking smirk, the tips of fangs showing, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-vreya.png.

#### `face-thrall` · Thrall · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `03-art-overhaul.md` §7.3; `enc-thrall-patrol`.

Edit the attached blank base image: change ONLY the face. Paint the face of a living human soldier enslaved to a vampire lord onto the blank head: a pale, blank face, glassy dark-red eyes, two small healed bite marks on the side of the neck, very pale skin. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-thrall.png.

#### `face-drowned` · Drowned · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (grey-green, kelp hair).

Edit the attached blank base image: change ONLY the face. Paint the face of a drowned corpse that walks out of the sea onto the blank head: a puffy, slack, grey-green waterlogged face, milky white eyes, the mouth hanging open, barnacles crusted on the jaw. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-drowned.png.

#### `face-blood-wight` · Blood-wight · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (thin, grey).

Edit the attached blank base image: change ONLY the face. Paint the face of an undead thing that walks under a darkened sun onto the blank head: a skull-like head of taut, papery ash-grey skin, sunken red eyes, long fangs, thin grey hair. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-blood-wight.png.

#### `face-sun-scorched` · Sun-scorched · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (burned to the skin).

Edit the attached blank base image: change ONLY the face. Paint the face of one of the burned dead of a desert massacre onto the blank head: a blackened face cracked like dried mud, the cracks glowing a faint hot gold, the features half gone, two pale, blazing eyes, the mouth open. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-sun-scorched.png.

#### `face-thornling` · Thornling · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (a bark-skinned sprite).

Edit the attached blank base image: change ONLY the face. Paint the face of a small, spiteful forest sprite onto the blank head: a knot of rough dark bark for a head, with a hollow, sly face, two glowing yellow-green eyes and a mouth like a split in a branch, and a crown of small twigs and new leaves. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-thornling.png.

#### `face-construct-priest` · Priest-construct's head · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `03b-world-art.md` (`priest-construct`).

Edit the attached blank base image: change ONLY the face. Paint the face of a stone guardian of a ruined sun temple onto the blank head: in place of a face, a round, polished bronze mirror, glinting, set inside a hood of sun-bleached sandstone carved like a priest's. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-priest.png.

#### `face-construct-crown` · Crown-construct's head · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (a crown motif for a face).

Edit the attached blank base image: change ONLY the face. Paint the face of a war machine shaped like a knight onto the blank head: no face: in its place, a stylised gold crown shape, with a violet light glowing behind the gaps between its points, on a neck of polished dark violet metal. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-crown.png.

#### `face-unmade` · The Unmade · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (no face: smooth dark glass where it would be).

Edit the attached blank base image: change ONLY the face. Paint the face of a soldier pulled out of time onto the blank head: where the face should be, a smooth, featureless oval of dark, glossy glass reflecting a faint white light; the rest of the head and neck faded and slightly greyed, as if not quite here. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head: only the eyebrows, and any beard or moustache described, which may fall past the jaw; the hair is a part of its own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-unmade.png.

#### `face-crown-hollowed` · Crown-raised Hollowed · P2

**Basis:** Edits `face-hollowed`. From `03-art-overhaul.md` §9 (violet eye light).

Edit the attached image (face-hollowed.png, the face on the base): change ONLY the colour of the light: the glowing cracks and the points of light in the eye sockets are violet instead of ember orange. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-crown-hollowed.png.

---

### 4.8 Hair

Every character's hair, lifted out of their face's old prompt: hair and helm are one slot, what's on the head, and a hair fits every head because the head never changes. Each is painted on the blank base whose sex matches. The player's creation screen offers the `hair-player-*` ones. `tools/import_parts.py` cuts them from `tools/source/characters/hairs/`.

#### `hair-player-1` · You, face 1's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-player-1` (§4), on its own: what's on You, face 1's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut brown, short and messy, tousled locks falling over the forehead and round the ears. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-1.png.

#### `hair-player-2` · You, face 2's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-player-2` (§4), on its own: what's on You, face 2's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut brown, shoulder length, tied back in a low, loose ponytail with messy strands round the face. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-2.png.

#### `hair-player-3` · You, face 3's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-player-3` (§4), on its own: what's on You, face 3's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, long and straight, in a high ponytail that falls past the shoulder blades, a few loose strands framing the face. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-3.png.

#### `hair-player-4` · You, face 4's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-player-4` (§4), on its own: what's on You, face 4's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: thick, curly dark brown to the shoulders, held back with a cloth band. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-4.png.

#### `hair-player-5` · You, face 5's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-player-5` (§4), on its own: what's on You, face 5's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, straight silver-blonde, past the shoulders, with one thin braid at the temple. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-5.png.

#### `hair-player-6` · You, face 6's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-player-6` (§4), on its own: what's on You, face 6's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, an undercut, the top swept back between the horns. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-6.png.

#### `hair-player-7` · You, face 7's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-player-7` (§4), on its own: what's on You, face 7's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: shaggy grey-brown, to the jaw. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-7.png.

#### `hair-player-8` · You, face 8's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-player-8` (§4), on its own: what's on You, face 8's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper red, cut short. A thick copper beard, braided into one short braid. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-player-8.png.

#### `hair-sera` · Sera Vantis's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-sera` (§4), on its own: what's on Sera Vantis's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: honey blonde, in one long, thick braid over the near shoulder to the waist, tied with a leather thong and a single grey kestrel feather. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-sera.png.

#### `hair-mira` · Mira Ashgrove's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-mira` (§4), on its own: what's on Mira Ashgrove's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper red, pinned up in a loose, messy bun with a writing pen pushed through it, strands escaping round the face and neck. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-mira.png.

#### `hair-mira-young` · Mira, aged thirteen's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-mira-young` (§4), on its own: what's on Mira, aged thirteen's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper red, in two long plaits. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-mira-young.png.

#### `hair-kael` · Kael Duskmantle's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-kael` (§4), on its own: what's on Kael Duskmantle's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, cropped short and untidy. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-kael.png.

#### `hair-brannock` · Brannock Ironvein's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-brannock` (§4), on its own: what's on Brannock Ironvein's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper red going grey, short and curly. A full copper beard, grey at the chin, braided into one thick braid that falls to the belt, held by two small gold clasps. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-brannock.png.

#### `hair-valencia` · Valencia Morvaine's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-valencia` (§4), on its own: what's on Valencia Morvaine's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, straight, silver-white, to the middle of the back, parted to one side. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-valencia.png.

#### `hair-mc-child` · You, aged eight's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-mc-child` (§4), on its own: what's on You, aged eight's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: an untidy chestnut mop. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-mc-child.png.

#### `hair-sb-child` · {sb}, aged eight's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-sb-child` (§4), on its own: what's on {sb}, aged eight's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, in a short, stubby ponytail coming loose. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-sb-child.png.

#### `hair-corra` · Corra's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-corra` (§4), on its own: what's on Corra's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut going grey, pinned up in a practical bun with loose wisps. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-corra.png.

#### `hair-lookout` · The lookout's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-lookout` (§4), on its own: what's on The lookout's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark, tied back short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-lookout.png.

#### `hair-odelle` · Captain Odelle Marsh's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-odelle` (§4), on its own: what's on Captain Odelle Marsh's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long and black with a few grey strands, in a loose tail down the back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-odelle.png.

#### `hair-tomas` · Old Tomas's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-tomas` (§4), on its own: what's on Old Tomas's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bald on top, a fringe of white at the sides. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-tomas.png.

#### `hair-red-gull` · Red Gull Thug, face 1's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-red-gull` (§4), on its own: what's on Red Gull Thug, face 1's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a shaved, bald head with a small tattoo of a gull in flight on the side of the scalp. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-red-gull.png.

#### `hair-red-gull-2` · Red Gull Thug, face 2's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-red-gull-2` (§4), on its own: what's on Red Gull Thug, face 2's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: lank ginger, short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-red-gull-2.png.

#### `hair-red-gull-3` · Red Gull Thug, face 3's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-red-gull-3` (§4), on its own: what's on Red Gull Thug, face 3's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: the sides shaved, a short black topknot. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-red-gull-3.png.

#### `hair-grell` · Bosun Grell's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-grell` (§4), on its own: what's on Bosun Grell's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: completely bald, the scalp shiny. A full, bushy copper beard. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-grell.png.

#### `hair-hale` · Captain Roderic Hale's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-hale` (§4), on its own: what's on Captain Roderic Hale's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, cropped short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hale.png.

#### `hair-fennick` · Questmaster Fennick's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-fennick` (§4), on its own: what's on Questmaster Fennick's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, neatly combed and parted, a quill pen tucked behind one ear. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-fennick.png.

#### `hair-hedda` · Hedda's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-hedda` (§4), on its own: what's on Hedda's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut, tied back roughly with a strip of cloth, strands coming loose. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hedda.png.

#### `hair-refugee-child` · Lise's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-refugee-child` (§4), on its own: what's on Lise's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut, in two uneven plaits tied with string. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-refugee-child.png.

#### `hair-brisk` · Sergeant Brisk's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-brisk` (§4), on its own: what's on Sergeant Brisk's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, cropped short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-brisk.png.

#### `hair-refugee-1` · Old Aurek's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-refugee-1` (§4), on its own: what's on Old Aurek's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bald, with a few white wisps. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-refugee-1.png.

#### `hair-refugee-2` · Teodor's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-refugee-2` (§4), on its own: what's on Teodor's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, tousled and damp. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-refugee-2.png.

#### `hair-refugee-3` · Yselle the Glassblower's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-refugee-3` (§4), on its own: what's on Yselle the Glassblower's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper red, in one thick braid down the back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-refugee-3.png.

#### `hair-elara` · Princess Elara Aurelian's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-elara` (§4), on its own: what's on Princess Elara Aurelian's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, shoulder length and choppy, cut in layers she clearly does herself. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-elara.png.

#### `hair-leon` · King Leon's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-leon` (§4), on its own: what's on King Leon's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver, swept back to the collar. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-leon.png.

#### `hair-crane` · Lord Chancellor Vesper Crane's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-crane` (§4), on its own: what's on Lord Chancellor Vesper Crane's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, slicked straight back from a high forehead. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-crane.png.

#### `hair-anvil-barkeep` · Hessa the Barkeep's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-anvil-barkeep` (§4), on its own: what's on Hessa the Barkeep's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black and thick, tied back in a short bundle. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-anvil-barkeep.png.

#### `hair-netty` · Netty the Net-Mender's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-netty` (§4), on its own: what's on Netty the Net-Mender's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver, in a braid pinned round the head like a crown. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-netty.png.

#### `hair-maud` · Maud Tolley's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-maud` (§4), on its own: what's on Maud Tolley's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver, in a low bun. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-maud.png.

#### `hair-pip` · Pip the Potboy's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-pip` (§4), on its own: what's on Pip the Potboy's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: copper, tousled. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-pip.png.

#### `hair-osric` · Osric Penhallow's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-osric` (§4), on its own: what's on Osric Penhallow's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black with a blue sheen, short and combed. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-osric.png.

#### `hair-lark` · Lark the Busker's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-lark` (§4), on its own: what's on Lark the Busker's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: rose pink, long and loose to the shoulders. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-lark.png.

#### `hair-ilsa` · Ilsa Fenwright's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-ilsa` (§4), on its own: what's on Ilsa Fenwright's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: gold, in one neat braid. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-ilsa.png.

#### `hair-oswin` · Guardsman Oswin's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-oswin` (§4), on its own: what's on Guardsman Oswin's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: gold, cropped. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-oswin.png.

#### `hair-hesper` · Sister Hesper's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-hesper` (§4), on its own: what's on Sister Hesper's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long and silver, loose down the back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hesper.png.

#### `hair-wren` · Wren's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-wren` (§4), on its own: what's on Wren's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, scraped back into a scruffy tail. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-wren.png.

#### `hair-imra-vell` · Master Imra Vell's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-imra-vell` (§4), on its own: what's on Master Imra Vell's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black flecked with grey, cut short and practical. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-imra-vell.png.

#### `hair-tamsin` · Ranger Tamsin's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-tamsin` (§4), on its own: what's on Ranger Tamsin's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: chestnut, tied back tightly. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-tamsin.png.

#### `hair-garrick` · Garrick the caravan master's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-garrick` (§4), on its own: what's on Garrick the caravan master's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black going grey. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-garrick.png.

#### `hair-aldric` · Commander Aldric Voss's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-aldric` (§4), on its own: what's on Commander Aldric Voss's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, cropped close. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-aldric.png.

#### `hair-recruit` · The recruit's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-recruit` (§4), on its own: what's on The recruit's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: straw blond, sticking out untidily. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-recruit.png.

#### `hair-thessaly` · Archmage Thessaly Varn's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-thessaly` (§4), on its own: what's on Archmage Thessaly Varn's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: iron grey, cropped very short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-thessaly.png.

#### `hair-brenna` · Brenna of Ashfall's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-brenna` (§4), on its own: what's on Brenna of Ashfall's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark brown, cropped short at the sides, the top tied back in a short tail. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-brenna.png.

#### `hair-gannet` · Mother Gannet's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-gannet` (§4), on its own: what's on Mother Gannet's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: iron grey, scraped into a tight bun. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-gannet.png.

#### `hair-hester` · Hester Hollis's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-hester` (§4), on its own: what's on Hester Hollis's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: white-grey, in a neat bun. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hester.png.

#### `hair-tobin` · Tobin Ashgrove's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-tobin` (§4), on its own: what's on Tobin Ashgrove's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: thinning copper with grey, a bald crown. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-tobin.png.

#### `hair-harrow` · Lord Harrow of Heronsgate's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-harrow` (§4), on its own: what's on Lord Harrow of Heronsgate's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey and receding, combed back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-harrow.png.

#### `hair-hobb` · Hobb the woodcutter's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-hobb` (§4), on its own: what's on Hobb the woodcutter's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: red and thick. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hobb.png.

#### `hair-maleficio` · Maleficio's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-maleficio` (§4), on its own: what's on Maleficio's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey and receding, swept back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-maleficio.png.

#### `hair-lost-boy` · The woodcutter's boy's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-lost-boy` (§4), on its own: what's on The woodcutter's boy's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bright red, sticking up. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-lost-boy.png.

#### `hair-mags` · Pedlar Mags's hair · P1

**Basis:** Edits `base-female-blank`. The hair painted with `face-mags` (§4), on its own: what's on Pedlar Mags's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey-streaked brown, in two thick braids. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-mags.png.

#### `hair-anselle` · Mother Superior Anselle's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-anselle` (§4), on its own: what's on Mother Superior Anselle's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short and grey, mostly under a close white linen coif. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-anselle.png.

#### `hair-orla` · Sister Orla's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-orla` (§4), on its own: what's on Sister Orla's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey-blonde, tucked up out of the way. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-orla.png.

#### `hair-corwin` · Sir Corwin Halbeck's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-corwin` (§4), on its own: what's on Sir Corwin Halbeck's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: fair going grey, cut short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-corwin.png.

#### `hair-severin` · Lord Severin Morvaine's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-severin` (§4), on its own: what's on Lord Severin Morvaine's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver, straight, to the shoulders. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-severin.png.

#### `hair-isadora` · Lady Isadora's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-isadora` (§4), on its own: what's on Lady Isadora's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark brown, braided and pinned up, a few strands loose. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-isadora.png.

#### `hair-valencia-child` · Valencia as a child's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-valencia-child` (§4), on its own: what's on Valencia as a child's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver-white, loose to the shoulders, with a small dark ribbon. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-valencia-child.png.

#### `hair-pell` · Pell's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-pell` (§4), on its own: what's on Pell's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark and shaggy, falling into his eyes. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-pell.png.

#### `hair-ide` · Ide's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-ide` (§4), on its own: what's on Ide's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark, long and tangled. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-ide.png.

#### `hair-maren-elder` · Maren, Hollowmere's elder's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-maren-elder` (§4), on its own: what's on Maren, Hollowmere's elder's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: white, in a tight braid round the head. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-maren-elder.png.

#### `hair-blood-house-steward` · The blood-house steward's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-blood-house-steward` (§4), on its own: what's on The blood-house steward's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, oiled flat. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-blood-house-steward.png.

#### `hair-hollowmere-child` · A Hollowmere child's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-hollowmere-child` (§4), on its own: what's on A Hollowmere child's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark, cut short and uneven. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hollowmere-child.png.

#### `hair-hollowmere-farmer` · A Hollowmere farmer's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-hollowmere-farmer` (§4), on its own: what's on A Hollowmere farmer's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown with grey, pulled back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-hollowmere-farmer.png.

#### `hair-ruck` · Ruck, the bosun's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-ruck` (§4), on its own: what's on Ruck, the bosun's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bald on top, with a short grey tarred pigtail at the back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-ruck.png.

#### `hair-davin` · Davin's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-davin` (§4), on its own: what's on Davin's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown, short and untidy. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-davin.png.

#### `hair-guild-apprentice` · A Guild apprentice's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-guild-apprentice` (§4), on its own: what's on A Guild apprentice's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: sandy, in a messy short ponytail. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-guild-apprentice.png.

#### `hair-town-1` · Townsfolk face 1's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-1` (§4), on its own: what's on Townsfolk face 1's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: straw coloured and shaggy. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-1.png.

#### `hair-town-2` · Townsfolk face 2's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-2` (§4), on its own: what's on Townsfolk face 2's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark brown, pinned up at the back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-2.png.

#### `hair-town-3` · Townsfolk face 3's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-3` (§4), on its own: what's on Townsfolk face 3's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, short and tightly curled. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-3.png.

#### `hair-town-4` · Townsfolk face 4's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-4` (§4), on its own: what's on Townsfolk face 4's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown, pinned up with strands escaping. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-4.png.

#### `hair-town-5` · Townsfolk face 5's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-5` (§4), on its own: what's on Townsfolk face 5's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, sticking out round a bald crown. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-5.png.

#### `hair-town-6` · Townsfolk face 6's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-6` (§4), on its own: what's on Townsfolk face 6's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: silver-white, pinned up. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-6.png.

#### `hair-town-7` · Townsfolk face 7's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-7` (§4), on its own: what's on Townsfolk face 7's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown and short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-7.png.

#### `hair-town-8` · Townsfolk face 8's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-8` (§4), on its own: what's on Townsfolk face 8's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: blonde, in two bunches. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-8.png.

#### `hair-town-9` · Townsfolk face 9's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-9` (§4), on its own: what's on Townsfolk face 9's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown and tousled. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-9.png.

#### `hair-town-10` · Townsfolk face 10's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-10` (§4), on its own: what's on Townsfolk face 10's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bright copper red, long and wavy. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-10.png.

#### `hair-town-11` · Townsfolk face 11's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-11` (§4), on its own: what's on Townsfolk face 11's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, short and messy. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-11.png.

#### `hair-town-12` · Townsfolk face 12's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-12` (§4), on its own: what's on Townsfolk face 12's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black going grey, short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-12.png.

#### `hair-town-13` · Townsfolk face 13's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-13` (§4), on its own: what's on Townsfolk face 13's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black with grey, pulled back. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-13.png.

#### `hair-town-14` · Townsfolk face 14's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-14` (§4), on its own: what's on Townsfolk face 14's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: brown, short. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-14.png.

#### `hair-town-15` · Townsfolk face 15's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-15` (§4), on its own: what's on Townsfolk face 15's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: curly, light brown, in a bun. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-15.png.

#### `hair-town-16` · Townsfolk face 16's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-16` (§4), on its own: what's on Townsfolk face 16's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark, straight, to the shoulders. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-16.png.

#### `hair-town-17` · Townsfolk face 17's hair · P0

**Basis:** Edits `base-male-blank`. The hair painted with `face-town-17` (§4), on its own: what's on Townsfolk face 17's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, short; a grey beard braided into two short braids. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-17.png.

#### `hair-town-18` · Townsfolk face 18's hair · P0

**Basis:** Edits `base-female-blank`. The hair painted with `face-town-18` (§4), on its own: what's on Townsfolk face 18's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: black, in a long plait over one shoulder. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-town-18.png.

#### `hair-varkhul` · Varkhul's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-varkhul` (§4), on its own: what's on Varkhul's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: grey, matted. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-varkhul.png.

#### `hair-cube-cultist` · Cube Cultist's hair · P1

**Basis:** Edits `base-male-blank`. The hair painted with `face-cube-cultist` (§4), on its own: what's on Cube Cultist's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a shaved head. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-cube-cultist.png.

#### `hair-vreya` · Vreya's hair · P2

**Basis:** Edits `base-female-blank`. The hair painted with `face-vreya` (§4), on its own: what's on Vreya's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: white-blonde, mostly to be hidden under a helm, one lock escaping. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-vreya.png.

#### `hair-thrall` · Thrall's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-thrall` (§4), on its own: what's on Thrall's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: dark, cropped. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-thrall.png.

#### `hair-drowned` · Drowned's hair · P2

**Basis:** Edits `base-male-blank`. The hair painted with `face-drowned` (§4), on its own: what's on Drowned's head when there's no helm.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, matted strands of dark kelp. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-drowned.png.

### 4.9 Hair sheets (four at a time)

The hairs four at a time, each sheet one ChatGPT picture. The attached template (`tools/source/characters/hair-sheet-male.png` or `hair-sheet-female.png`, made by `python tools/part_sheet.py templates`) is four copies of the blank base from the head to the hips at full size, side by side, so a long braid has room. `python tools/part_sheet.py cut <png> <sheet-id>` puts each back on its base as `tools/source/characters/hairs/<hair-id>.png`. The single hair cards in §4.8 stay for redoing one.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `hair-sheet-m01` | `base-male` | P0 | `hair-player-1`, `hair-player-6`, `hair-player-8`, `hair-kael` |
| `hair-sheet-m02` | `base-male` | P0 | `hair-brannock`, `hair-mc-child`, `hair-lookout`, `hair-tomas` |
| `hair-sheet-m03` | `base-male` | P0 | `hair-red-gull`, `hair-red-gull-2`, `hair-red-gull-3`, `hair-grell` |
| `hair-sheet-m04` | `base-male` | P0 | `hair-hale`, `hair-fennick`, `hair-brisk`, `hair-refugee-1` |
| `hair-sheet-m05` | `base-male` | P0 | `hair-refugee-2`, `hair-leon`, `hair-crane`, `hair-pip` |
| `hair-sheet-m06` | `base-male` | P0 | `hair-osric`, `hair-lark`, `hair-oswin`, `hair-town-1` |
| `hair-sheet-m07` | `base-male` | P0 | `hair-town-3`, `hair-town-5`, `hair-town-7`, `hair-town-9` |
| `hair-sheet-m08` | `base-male` | P0 | `hair-town-11`, `hair-town-12`, `hair-town-14`, `hair-town-16` |
| `hair-sheet-m09` | `base-male` | P0 | `hair-town-17`, `hair-garrick`, `hair-aldric`, `hair-recruit` |
| `hair-sheet-m10` | `base-male` | P1 | `hair-tobin`, `hair-harrow`, `hair-hobb`, `hair-maleficio` |
| `hair-sheet-m11` | `base-male` | P1 | `hair-lost-boy`, `hair-varkhul`, `hair-cube-cultist`, `hair-corwin` |
| `hair-sheet-m12` | `base-male` | P2 | `hair-severin`, `hair-pell`, `hair-blood-house-steward`, `hair-hollowmere-child` |
| `hair-sheet-m13` | `base-male` | P2 | `hair-ruck`, `hair-davin`, `hair-thrall`, `hair-drowned` |
| `hair-sheet-f01` | `base-female` | P0 | `hair-player-2`, `hair-player-3`, `hair-player-4`, `hair-player-5` |
| `hair-sheet-f02` | `base-female` | P0 | `hair-player-7`, `hair-sera`, `hair-mira`, `hair-sb-child` |
| `hair-sheet-f03` | `base-female` | P0 | `hair-corra`, `hair-odelle`, `hair-hedda`, `hair-refugee-child` |
| `hair-sheet-f04` | `base-female` | P0 | `hair-refugee-3`, `hair-elara`, `hair-anvil-barkeep`, `hair-netty` |
| `hair-sheet-f05` | `base-female` | P0 | `hair-maud`, `hair-ilsa`, `hair-hesper`, `hair-wren` |
| `hair-sheet-f06` | `base-female` | P0 | `hair-imra-vell`, `hair-tamsin`, `hair-town-2`, `hair-town-4` |
| `hair-sheet-f07` | `base-female` | P0 | `hair-town-6`, `hair-town-8`, `hair-town-10`, `hair-town-13` |
| `hair-sheet-f08` | `base-female` | P0 | `hair-town-15`, `hair-town-18`, `hair-mira-young`, `hair-thessaly` |
| `hair-sheet-f09` | `base-female` | P1 | `hair-brenna`, `hair-gannet`, `hair-hester`, `hair-mags` |
| `hair-sheet-f10` | `base-female` | P2 | `hair-valencia`, `hair-anselle`, `hair-orla`, `hair-isadora` |
| `hair-sheet-f11` | `base-female` | P2 | `hair-valencia-child`, `hair-ide`, `hair-maren-elder`, `hair-hollowmere-farmer` |
| `hair-sheet-f12` | `base-female` | P2 | `hair-guild-apprentice`, `hair-vreya` |

#### `hair-sheet-m01` · Hair sheet: 4 hairstyles (You, face 1, You, face 6, You, face 8, Kael Duskmantle) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-player-1`, 2 `hair-player-6`, 3 `hair-player-8`, 4 `hair-kael`.

1. You, face 1: chestnut brown, short and messy, tousled locks falling over the forehead and round the ears.
2. You, face 6: black, an undercut, the top swept back between the horns.
3. You, face 8: copper red, cut short. A thick copper beard, braided into one short braid.
4. Kael Duskmantle: black, cropped short and untidy.
I'll save it as hair-sheet-m01.png.

#### `hair-sheet-m02` · Hair sheet: 4 hairstyles (Brannock Ironvein, You, aged eight, The lookout, Old Tomas) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-brannock`, 2 `hair-mc-child`, 3 `hair-lookout`, 4 `hair-tomas`.

1. Brannock Ironvein: copper red going grey, short and curly. A full copper beard, grey at the chin, braided into one thick braid that falls to the belt, held by two small gold clasps.
2. You, aged eight: an untidy chestnut mop.
3. The lookout: dark, tied back short.
4. Old Tomas: bald on top, a fringe of white at the sides.
I'll save it as hair-sheet-m02.png.

#### `hair-sheet-m03` · Hair sheet: 4 hairstyles (Red Gull Thug, face 1, Red Gull Thug, face 2, Red Gull Thug, face 3, Bosun Grell) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-red-gull`, 2 `hair-red-gull-2`, 3 `hair-red-gull-3`, 4 `hair-grell`.

1. Red Gull Thug, face 1: a shaved, bald head with a small tattoo of a gull in flight on the side of the scalp.
2. Red Gull Thug, face 2: lank ginger, short.
3. Red Gull Thug, face 3: the sides shaved, a short black topknot.
4. Bosun Grell: completely bald, the scalp shiny. A full, bushy copper beard.
I'll save it as hair-sheet-m03.png.

#### `hair-sheet-m04` · Hair sheet: 4 hairstyles (Captain Roderic Hale, Questmaster Fennick, Sergeant Brisk, Old Aurek) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-hale`, 2 `hair-fennick`, 3 `hair-brisk`, 4 `hair-refugee-1`.

1. Captain Roderic Hale: grey, cropped short.
2. Questmaster Fennick: grey, neatly combed and parted, a quill pen tucked behind one ear.
3. Sergeant Brisk: black, cropped short.
4. Old Aurek: bald, with a few white wisps.
I'll save it as hair-sheet-m04.png.

#### `hair-sheet-m05` · Hair sheet: 4 hairstyles (Teodor, King Leon, Lord Chancellor Vesper Crane, Pip the Potboy) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-refugee-2`, 2 `hair-leon`, 3 `hair-crane`, 4 `hair-pip`.

1. Teodor: black, tousled and damp.
2. King Leon: silver, swept back to the collar.
3. Lord Chancellor Vesper Crane: black, slicked straight back from a high forehead.
4. Pip the Potboy: copper, tousled.
I'll save it as hair-sheet-m05.png.

#### `hair-sheet-m06` · Hair sheet: 4 hairstyles (Osric Penhallow, Lark the Busker, Guardsman Oswin, Townsfolk face 1) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-osric`, 2 `hair-lark`, 3 `hair-oswin`, 4 `hair-town-1`.

1. Osric Penhallow: black with a blue sheen, short and combed.
2. Lark the Busker: rose pink, long and loose to the shoulders.
3. Guardsman Oswin: gold, cropped.
4. Townsfolk face 1: straw coloured and shaggy.
I'll save it as hair-sheet-m06.png.

#### `hair-sheet-m07` · Hair sheet: 4 hairstyles (Townsfolk face 3, Townsfolk face 5, Townsfolk face 7, Townsfolk face 9) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-town-3`, 2 `hair-town-5`, 3 `hair-town-7`, 4 `hair-town-9`.

1. Townsfolk face 3: black, short and tightly curled.
2. Townsfolk face 5: grey, sticking out round a bald crown.
3. Townsfolk face 7: brown and short.
4. Townsfolk face 9: brown and tousled.
I'll save it as hair-sheet-m07.png.

#### `hair-sheet-m08` · Hair sheet: 4 hairstyles (Townsfolk face 11, Townsfolk face 12, Townsfolk face 14, Townsfolk face 16) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-town-11`, 2 `hair-town-12`, 3 `hair-town-14`, 4 `hair-town-16`.

1. Townsfolk face 11: black, short and messy.
2. Townsfolk face 12: black going grey, short.
3. Townsfolk face 14: brown, short.
4. Townsfolk face 16: dark, straight, to the shoulders.
I'll save it as hair-sheet-m08.png.

#### `hair-sheet-m09` · Hair sheet: 4 hairstyles (Townsfolk face 17, Garrick the caravan master, Commander Aldric Voss, The recruit) · P0

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-town-17`, 2 `hair-garrick`, 3 `hair-aldric`, 4 `hair-recruit`.

1. Townsfolk face 17: grey, short; a grey beard braided into two short braids.
2. Garrick the caravan master: black going grey.
3. Commander Aldric Voss: grey, cropped close.
4. The recruit: straw blond, sticking out untidily.
I'll save it as hair-sheet-m09.png.

#### `hair-sheet-m10` · Hair sheet: 4 hairstyles (Tobin Ashgrove, Lord Harrow of Heronsgate, Hobb the woodcutter, Maleficio) · P1

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-tobin`, 2 `hair-harrow`, 3 `hair-hobb`, 4 `hair-maleficio`.

1. Tobin Ashgrove: thinning copper with grey, a bald crown.
2. Lord Harrow of Heronsgate: grey and receding, combed back.
3. Hobb the woodcutter: red and thick.
4. Maleficio: grey and receding, swept back.
I'll save it as hair-sheet-m10.png.

#### `hair-sheet-m11` · Hair sheet: 4 hairstyles (The woodcutter's boy, Varkhul, Cube Cultist, Sir Corwin Halbeck) · P1

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-lost-boy`, 2 `hair-varkhul`, 3 `hair-cube-cultist`, 4 `hair-corwin`.

1. The woodcutter's boy: bright red, sticking up.
2. Varkhul: grey, matted.
3. Cube Cultist: a shaved head.
4. Sir Corwin Halbeck: fair going grey, cut short.
I'll save it as hair-sheet-m11.png.

#### `hair-sheet-m12` · Hair sheet: 4 hairstyles (Lord Severin Morvaine, Pell, The blood-house steward, A Hollowmere child) · P2

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-severin`, 2 `hair-pell`, 3 `hair-blood-house-steward`, 4 `hair-hollowmere-child`.

1. Lord Severin Morvaine: silver, straight, to the shoulders.
2. Pell: dark and shaggy, falling into his eyes.
3. The blood-house steward: grey, oiled flat.
4. A Hollowmere child: dark, cut short and uneven.
I'll save it as hair-sheet-m12.png.

#### `hair-sheet-m13` · Hair sheet: 4 hairstyles (Ruck, the bosun, Davin, Thrall, Drowned) · P2

**Basis:** Edits `hair-sheet-male`. Cells: 1 `hair-ruck`, 2 `hair-davin`, 3 `hair-thrall`, 4 `hair-drowned`.

1. Ruck, the bosun: bald on top, with a short grey tarred pigtail at the back.
2. Davin: brown, short and untidy.
3. Thrall: dark, cropped.
4. Drowned: long, matted strands of dark kelp.
I'll save it as hair-sheet-m13.png.

#### `hair-sheet-f01` · Hair sheet: 4 hairstyles (You, face 2, You, face 3, You, face 4, You, face 5) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-player-2`, 2 `hair-player-3`, 3 `hair-player-4`, 4 `hair-player-5`.

1. You, face 2: chestnut brown, shoulder length, tied back in a low, loose ponytail with messy strands round the face.
2. You, face 3: black, long and straight, in a high ponytail that falls past the shoulder blades, a few loose strands framing the face.
3. You, face 4: thick, curly dark brown to the shoulders, held back with a cloth band.
4. You, face 5: long, straight silver-blonde, past the shoulders, with one thin braid at the temple.
I'll save it as hair-sheet-f01.png.

#### `hair-sheet-f02` · Hair sheet: 4 hairstyles (You, face 7, Sera Vantis, Mira Ashgrove, {sb}, aged eight) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-player-7`, 2 `hair-sera`, 3 `hair-mira`, 4 `hair-sb-child`.

1. You, face 7: shaggy grey-brown, to the jaw.
2. Sera Vantis: honey blonde, in one long, thick braid over the near shoulder to the waist, tied with a leather thong and a single grey kestrel feather.
3. Mira Ashgrove: copper red, pinned up in a loose, messy bun with a writing pen pushed through it, strands escaping round the face and neck.
4. {sb}, aged eight: black, in a short, stubby ponytail coming loose.
I'll save it as hair-sheet-f02.png.

#### `hair-sheet-f03` · Hair sheet: 4 hairstyles (Corra, Captain Odelle Marsh, Hedda, Lise) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-corra`, 2 `hair-odelle`, 3 `hair-hedda`, 4 `hair-refugee-child`.

1. Corra: chestnut going grey, pinned up in a practical bun with loose wisps.
2. Captain Odelle Marsh: long and black with a few grey strands, in a loose tail down the back.
3. Hedda: chestnut, tied back roughly with a strip of cloth, strands coming loose.
4. Lise: chestnut, in two uneven plaits tied with string.
I'll save it as hair-sheet-f03.png.

#### `hair-sheet-f04` · Hair sheet: 4 hairstyles (Yselle the Glassblower, Princess Elara Aurelian, Hessa the Barkeep, Netty the Net-Mender) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-refugee-3`, 2 `hair-elara`, 3 `hair-anvil-barkeep`, 4 `hair-netty`.

1. Yselle the Glassblower: copper red, in one thick braid down the back.
2. Princess Elara Aurelian: black, shoulder length and choppy, cut in layers she clearly does herself.
3. Hessa the Barkeep: black and thick, tied back in a short bundle.
4. Netty the Net-Mender: silver, in a braid pinned round the head like a crown.
I'll save it as hair-sheet-f04.png.

#### `hair-sheet-f05` · Hair sheet: 4 hairstyles (Maud Tolley, Ilsa Fenwright, Sister Hesper, Wren) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-maud`, 2 `hair-ilsa`, 3 `hair-hesper`, 4 `hair-wren`.

1. Maud Tolley: silver, in a low bun.
2. Ilsa Fenwright: gold, in one neat braid.
3. Sister Hesper: long and silver, loose down the back.
4. Wren: black, scraped back into a scruffy tail.
I'll save it as hair-sheet-f05.png.

#### `hair-sheet-f06` · Hair sheet: 4 hairstyles (Master Imra Vell, Ranger Tamsin, Townsfolk face 2, Townsfolk face 4) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-imra-vell`, 2 `hair-tamsin`, 3 `hair-town-2`, 4 `hair-town-4`.

1. Master Imra Vell: black flecked with grey, cut short and practical.
2. Ranger Tamsin: chestnut, tied back tightly.
3. Townsfolk face 2: dark brown, pinned up at the back.
4. Townsfolk face 4: brown, pinned up with strands escaping.
I'll save it as hair-sheet-f06.png.

#### `hair-sheet-f07` · Hair sheet: 4 hairstyles (Townsfolk face 6, Townsfolk face 8, Townsfolk face 10, Townsfolk face 13) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-town-6`, 2 `hair-town-8`, 3 `hair-town-10`, 4 `hair-town-13`.

1. Townsfolk face 6: silver-white, pinned up.
2. Townsfolk face 8: blonde, in two bunches.
3. Townsfolk face 10: bright copper red, long and wavy.
4. Townsfolk face 13: black with grey, pulled back.
I'll save it as hair-sheet-f07.png.

#### `hair-sheet-f08` · Hair sheet: 4 hairstyles (Townsfolk face 15, Townsfolk face 18, Mira, aged thirteen, Archmage Thessaly Varn) · P0

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-town-15`, 2 `hair-town-18`, 3 `hair-mira-young`, 4 `hair-thessaly`.

1. Townsfolk face 15: curly, light brown, in a bun.
2. Townsfolk face 18: black, in a long plait over one shoulder.
3. Mira, aged thirteen: copper red, in two long plaits.
4. Archmage Thessaly Varn: iron grey, cropped very short.
I'll save it as hair-sheet-f08.png.

#### `hair-sheet-f09` · Hair sheet: 4 hairstyles (Brenna of Ashfall, Mother Gannet, Hester Hollis, Pedlar Mags) · P1

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-brenna`, 2 `hair-gannet`, 3 `hair-hester`, 4 `hair-mags`.

1. Brenna of Ashfall: dark brown, cropped short at the sides, the top tied back in a short tail.
2. Mother Gannet: iron grey, scraped into a tight bun.
3. Hester Hollis: white-grey, in a neat bun.
4. Pedlar Mags: grey-streaked brown, in two thick braids.
I'll save it as hair-sheet-f09.png.

#### `hair-sheet-f10` · Hair sheet: 4 hairstyles (Valencia Morvaine, Mother Superior Anselle, Sister Orla, Lady Isadora) · P2

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-valencia`, 2 `hair-anselle`, 3 `hair-orla`, 4 `hair-isadora`.

1. Valencia Morvaine: long, straight, silver-white, to the middle of the back, parted to one side.
2. Mother Superior Anselle: short and grey, mostly under a close white linen coif.
3. Sister Orla: grey-blonde, tucked up out of the way.
4. Lady Isadora: dark brown, braided and pinned up, a few strands loose.
I'll save it as hair-sheet-f10.png.

#### `hair-sheet-f11` · Hair sheet: 4 hairstyles (Valencia as a child, Ide, Maren, Hollowmere's elder, A Hollowmere farmer) · P2

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-valencia-child`, 2 `hair-ide`, 3 `hair-maren-elder`, 4 `hair-hollowmere-farmer`.

1. Valencia as a child: silver-white, loose to the shoulders, with a small dark ribbon.
2. Ide: dark, long and tangled.
3. Maren, Hollowmere's elder: white, in a tight braid round the head.
4. A Hollowmere farmer: brown with grey, pulled back.
I'll save it as hair-sheet-f11.png.

#### `hair-sheet-f12` · Hair sheet: 2 hairstyles (A Guild apprentice, Vreya) · P2

**Basis:** Edits `hair-sheet-female`. Cells: 1 `hair-guild-apprentice`, 2 `hair-vreya`.

1. A Guild apprentice: sandy, in a messy short ponytail.
2. Vreya: white-blonde, mostly to be hidden under a helm, one lock escaping.
Leave figures 3 and 4 exactly as they are: bald, unchanged.
I'll save it as hair-sheet-f12.png.

## 5. Armors

The outfits described across the cast, deduplicated: a unique armor for each notable character, shared armors for groups, and the armors in the item catalog. Each one dresses the bald base.

### 5.0 Armor sheets (four at a time)

The owner's call (2026-11-07): make the armors four at a time, as the faces are made eight at a time (§4.0). Every armor that dresses a base is in one sheet here, and each sheet is one ChatGPT picture. The attached template (`tools/source/characters/armor-sheet-male.png` or `armor-sheet-female.png`, made by `python tools/part_sheet.py templates`) is four copies of the whole base, side by side, at five-eighths of its size: still more detail than the game keeps. `python tools/part_sheet.py cut <png> <sheet-id>` lines each figure up by its bald head and scales it back onto the base's canvas as `tools/source/characters/armors/<armor-id>.png`, exactly as a single edit would be, and `python tools/import_parts.py` takes it into the game. The single armor cards in §5.1 onward stay for redoing one armor, for the variants that edit another armor, and for the creature bodies (the ones that change the body's shape), which need a whole canvas each.

The figures are numbered left to right, 1 to 4. The sheets are filled in priority order, so the early ones hold the armors the game needs first.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `armor-sheet-m01` | `base-male` | P0 | `armor-traveller`, `armor-child-cove`, `armor-kael`, `armor-brannock` |
| `armor-sheet-m02` | `base-male` | P0 | `armor-sailor`, `armor-fisher`, `armor-red-gull`, `armor-grell` |
| `armor-sheet-m03` | `base-male` | P0 | `armor-watch`, `armor-watch-captain`, `armor-clerk`, `armor-refugee-man` |
| `armor-sheet-m04` | `base-male` | P0 | `armor-leon`, `armor-crane`, `armor-royal-livery`, `armor-royal-guard` |
| `armor-sheet-m05` | `base-male` | P0 | `armor-villager-man`, `armor-merchant`, `armor-busker`, `armor-urchin` |
| `armor-sheet-m06` | `base-male` | P0 | `armor-noble`, `armor-elder-coat`, `armor-smith`, `armor-road` |
| `armor-sheet-m07` | `base-male` | P0 | `armor-courier`, `armor-porter`, `armor-padded`, `armor-leather` |
| `armor-sheet-m08` | `base-male` | P0 | `armor-studded`, `armor-hide`, `armor-chain-shirt`, `armor-chain-mail` |
| `armor-sheet-m09` | `base-male` | P0 | `armor-scale`, `armor-plate`, `armor-mage-robes`, `armor-champion` |
| `armor-sheet-m10` | `base-male` | P1 | `armor-caravan`, `armor-aldric`, `armor-recruit`, `armor-cider-presser` |
| `armor-sheet-m11` | `base-male` | P1 | `armor-levy`, `armor-harrow`, `armor-woodcutter`, `armor-maleficio` |
| `armor-sheet-m12` | `base-male` | P1 | `armor-scholar`, `armor-old-pattern`, `armor-varkhul`, `armor-cultist` |
| `armor-sheet-m13` | `base-male` | P1 | `armor-bandit`, `armor-kael-shadow`, `armor-corwin`, `armor-severin` |
| `armor-sheet-m14` | `base-male` | P2 | `armor-morvaine-man`, `armor-morvaine-livery`, `armor-prisoner`, `armor-davin` |
| `armor-sheet-m15` | `base-male` | P2 | `armor-thrall`, `armor-thrall-knight`, `armor-gilded-knight`, `armor-protectorate` |
| `armor-sheet-m16` | `base-male` | P2 | `armor-protectorate-warden`, `armor-unmade-bronze`, `armor-pikeman`, `armor-ashfall-raider` |
| `armor-sheet-m17` | `base-male` | P0 | `armor-tavern-m` |
| `armor-sheet-f01` | `base-female` | P0 | `armor-traveller-f`, `armor-sb-traveller`, `armor-sera`, `armor-mira` |
| `armor-sheet-f02` | `base-female` | P0 | `armor-corra`, `armor-odelle`, `armor-refugee-woman`, `armor-refugee-child` |
| `armor-sheet-f03` | `base-female` | P0 | `armor-glassblower`, `armor-elara`, `armor-elara-disguise`, `armor-tavern` |
| `armor-sheet-f04` | `base-female` | P0 | `armor-villager-woman`, `armor-elder-woman`, `armor-temple`, `armor-fencer` |
| `armor-sheet-f05` | `base-female` | P0 | `armor-lodge-ranger`, `armor-noble-gown`, `armor-trader`, `armor-baker` |
| `armor-sheet-f06` | `base-female` | P0 | `armor-pilgrim`, `armor-maid`, `armor-padded-f`, `armor-leather-f` |
| `armor-sheet-f07` | `base-female` | P0 | `armor-studded-f`, `armor-hide-f`, `armor-chain-shirt-f`, `armor-chain-mail-f` |
| `armor-sheet-f08` | `base-female` | P0 | `armor-scale-f`, `armor-plate-f`, `armor-mage-robes-f`, `armor-champion-f` |
| `armor-sheet-f09` | `base-female` | P1 | `armor-mira-apprentice`, `armor-thessaly`, `armor-brenna`, `armor-gannet` |
| `armor-sheet-f10` | `base-female` | P1 | `armor-pedlar`, `armor-lord-protector`, `armor-sovereign`, `armor-valencia` |
| `armor-sheet-f11` | `base-female` | P2 | `armor-abbey-habit`, `armor-anselle`, `armor-beekeeper`, `armor-isadora` |
| `armor-sheet-f12` | `base-female` | P2 | `armor-child-dress`, `armor-morvaine-woman`, `armor-shift`, `armor-abbey-lay` |
| `armor-sheet-f13` | `base-female` | P0 | `armor-guild-apprentice`, `armor-smuggler`, `armor-child-cove-f`, `armor-urchin-f` |
| `armor-sheet-f14` | `base-female` | P2 | `armor-thrall-knight-f` |

#### `armor-sheet-m01` · Armor sheet: 4 outfits (The traveller, A cove child's clothes, Kael's hooded leathers, Brannock's soldier-smith kit) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-traveller`, 2 `armor-child-cove`, 3 `armor-kael`, 4 `armor-brannock`.

1. The traveller: a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest.
2. A cove child's clothes: a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet.
3. Kael's hooded leathers: a close-fitting dark leather coat to the hip in layered panels, with straps and buckles across the chest; a charcoal half-cloak over the shoulders with its pointed hood hanging down the back; a wide ember-red sash wound round the waist, its ends hanging at the far hip; dark grey trousers; soft dark boots with the legs wrapped in cloth bands to the knee; dark fingerless gloves; a few lockpicks in a strap on the forearm.
4. Brannock's soldier-smith kit: a charcoal padded work shirt with the sleeves pushed up; a chain-mail shirt over it to the hips; a battered iron breastplate with tarnished-gold edging, dented and scratched; one heavy iron pauldron on the near shoulder; a thick dark-leather apron-like skirt to the knees over dark trousers, like a smith's; a wide belt with a big iron buckle and a battered metal flask; heavy iron-capped boots.
I'll save it as armor-sheet-m01.png.

#### `armor-sheet-m02` · Armor sheet: 4 outfits (A sailor's clothes, A fisherman's oilskins, A Red Gull's rig, Bosun Grell's coat and hook) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-sailor`, 2 `armor-fisher`, 3 `armor-red-gull`, 4 `armor-grell`.

1. A sailor's clothes: a navy-and-cream striped knitted jersey with the sleeves pushed up; a sleeveless brown oilskin waistcoat; canvas trousers rolled to the shin; bare feet with tarred soles; a rope lanyard at the belt.
2. A fisherman's oilskins: a thick grey knitted jumper, darned at the elbows; a sleeveless mustard oilskin smock over it, cracked and patched; canvas trousers rolled at the shin; tall black tarred sea boots; a brass fish hook on a cord at the belt.
3. A Red Gull's rig: a raspberry-red bandana knotted round the throat; a cream-and-navy striped jersey with the sleeves rolled over the forearms; a scuffed leather shoulder guard on the near shoulder, buckled with straps across the chest; a wide raspberry-red sash at the waist; brown canvas trousers; tall worn boots.
4. Bosun Grell's coat and hook: an open, knee-length bosun's coat in raspberry red, frayed at the cuffs and stained with tar, with tarnished brass buttons; an oatmeal shirt under it, half unlaced; a gull tattoo on the near forearm; a broad belt with a big brass buckle; dark canvas trousers; heavy boots. The far hand is gone at the wrist: in its place, a steel hook strapped to a leather cuff.
I'll save it as armor-sheet-m02.png.

#### `armor-sheet-m03` · Armor sheet: 4 outfits (The City Watch uniform, The Watch captain's coat, A clerk's waistcoat, A refugee man's clothes) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-watch`, 2 `armor-watch-captain`, 3 `armor-clerk`, 4 `armor-refugee-man`.

1. The City Watch uniform: a navy-blue Watch coat to mid-thigh over a padded jack, brass buttons, a small brass badge shaped like a lantern on the chest; a brown leather belt; navy trousers; black boots.
2. The Watch captain's coat: a navy-blue Watch coat to mid-thigh over a chain-mail shirt, with brass buttons and a larger brass lantern badge on the chest; a brown leather belt and baldric with a short sword; a rolled paper roster tucked into the belt; navy trousers; worn black boots, well polished.
3. A clerk's waistcoat: an oatmeal shirt with black sleeve garters; a mustard-ochre waistcoat with a watch chain; sand-coloured trousers; polished brown shoes; ink stains on the fingers.
4. A refugee man's clothes: a faded black wool coat with mismatched patches over a grey shirt; a charcoal scarf; grey trousers tied at the ankle with twine; cracked boots.
I'll save it as armor-sheet-m03.png.

#### `armor-sheet-m04` · Armor sheet: 4 outfits (King Leon's robes, Crane's long coat, The palace livery, The Royal Guard's armour) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-leon`, 2 `armor-crane`, 3 `armor-royal-livery`, 4 `armor-royal-guard`.

1. King Leon's robes: a long white robe-coat to the ankles with sun-gold trim at the hem and cuffs and a gold sash; a broad collar of ermine (white fur with small black tips) over the shoulders; soft white boots; and at the hip, a long, plain, very old sword in a worn scabbard, its grip blue leather worn pale, a small gold sun on the crossguard.
2. Crane's long coat: a floor-length plum-coloured coat, close at the chest and long in the skirt, with a high standing collar, silver trim and a column of small silver buttons; a dark-grey waistcoat; a white stock at the throat; dark trousers and narrow black boots; silver rings on several fingers, one a signet set with a small black cube.
3. The palace livery: a navy-blue livery tunic with a gold sun badge on the chest and gold piping; a white shirt; navy breeches; soft brown boots; a dispatch pouch on a strap across the body.
4. The Royal Guard's armour: a navy-blue tabard with a large gold sun embroidered on the chest, over polished mail; steel pauldrons with gold edging; a white sash; navy trousers; steel-capped black boots.
I'll save it as armor-sheet-m04.png.

#### `armor-sheet-m05` · Armor sheet: 4 outfits (A townsman's clothes, A merchant's waistcoat, A busker's finery, A street child's hand-me-downs) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-villager-man`, 2 `armor-merchant`, 3 `armor-busker`, 4 `armor-urchin`.

1. A townsman's clothes: a loose oatmeal shirt with the sleeves rolled and a rust-orange neckerchief; a grey-blue waistcoat, unbuttoned; brown trousers held up with braces; muddy boots.
2. A merchant's waistcoat: a white shirt with full sleeves; an embroidered dusty-plum waistcoat; dark burgundy-brown trousers; polished shoes; a tailor's tape round the neck.
3. A busker's finery: a blush-pink shirt with billowing sleeves; a rose waistcoat; sand-coloured trousers; soft boots.
4. A street child's hand-me-downs: a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet.
I'll save it as armor-sheet-m05.png.

#### `armor-sheet-m06` · Armor sheet: 4 outfits (A courtier's doublet, An old man's long coat, A smith's apron, A traveller's road clothes) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-noble`, 2 `armor-elder-coat`, 3 `armor-smith`, 4 `armor-road`.

1. A courtier's doublet: a fitted velvet doublet in deep teal with slashed sleeves showing cream silk; a lace collar; dark hose and breeches; soft black shoes with buckles; a ring or two.
2. An old man's long coat: a long dove-grey coat to the shin with dark-grey trim and pewter buttons; a slate waistcoat; a white shirt with a soft collar; grey trousers; black shoes.
3. A smith's apron: a cream shirt with the sleeves rolled above the elbow, open at the throat; a heavy, scorched brown leather apron to the knee; thick leather gloves tucked into the apron string; dark trousers; heavy boots.
4. A traveller's road clothes: a dark-brown tunic belted at the waist; charcoal trousers; boots worn through at the toes and wrapped in cloth; a small pack on the back with a bedroll and a tin cup.
I'll save it as armor-sheet-m06.png.

#### `armor-sheet-m07` · Armor sheet: 4 outfits (A courier's clothes, A porter's clothes, Padded armour, Leather armour) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-courier`, 2 `armor-porter`, 3 `armor-padded`, 4 `armor-leather`.

1. A courier's clothes: a cream shirt with rolled sleeves; dark-grey braces; charcoal trousers tucked into soft running boots; a big leather mail satchel bulging with letters on a strap across the body.
2. A porter's clothes: a sweat-stained cream shirt with rolled sleeves; brown braces; a leather shoulder pad on the far shoulder; brown canvas trousers; heavy boots; a crate hook at the belt.
3. Padded armour: a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots.
4. Leather armour: boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel.
I'll save it as armor-sheet-m07.png.

#### `armor-sheet-m08` · Armor sheet: 4 outfits (Studded leather, Hide armour, Chain shirt, Chain mail) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-studded`, 2 `armor-hide`, 3 `armor-chain-shirt`, 4 `armor-chain-mail`.

1. Studded leather: the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots.
2. Hide armour: bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots.
3. Chain shirt: a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots.
4. Chain mail: a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots.
I'll save it as armor-sheet-m08.png.

#### `armor-sheet-m09` · Armor sheet: 4 outfits (Scale mail, Plate armour, Mage robes, The Champion's tabard) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-scale`, 2 `armor-plate`, 3 `armor-mage-robes`, 4 `armor-champion`.

1. Scale mail: a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots.
2. Plate armour: full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate.
3. Mage robes: a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes.
4. The Champion's tabard: the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges.
I'll save it as armor-sheet-m09.png.

#### `armor-sheet-m10` · Armor sheet: 4 outfits (A caravan master's duster, Aldric's worn plate, A recruit's gambeson, A cider-presser's apron) · P1

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-caravan`, 2 `armor-aldric`, 3 `armor-recruit`, 4 `armor-cider-presser`.

1. A caravan master's duster: a long oiled-brown duster coat to the shin, open; a faded grey scarf against the road dust; a leather waistcoat over a grey shirt; dark trousers; long riding boots; a coiled driving whip at the belt.
2. Aldric's worn plate: worn steel plate that doesn't quite match, dented and scratched (a breastplate, rounded pauldrons, vambraces and greaves), over chain mail and a padded jack; a battered sword belt; old boots.
3. A recruit's gambeson: a quilted navy-blue gambeson with brass buttons, a little too long; a brown belt; grey wool trousers; boots with snow on them; a grey woollen scarf.
4. A cider-presser's apron: an apple-green work shirt with the sleeves rolled; a heavy, stained brown leather presser's apron; brown trousers; sturdy boots; a wide, old burn scar across the back of the near hand.
I'll save it as armor-sheet-m10.png.

#### `armor-sheet-m11` · Armor sheet: 4 outfits (A levy coat, Lord Harrow's heron coat, A woodcutter's clothes, Maleficio's high-backed robes) · P1

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-levy`, 2 `armor-harrow`, 3 `armor-woodcutter`, 4 `armor-maleficio`.

1. A levy coat: a patched grey levy coat over a work shirt, mud to the knees, rope for a belt, worn boots.
2. Lord Harrow's heron coat: a long frock coat in heron green (a deep grey-green) with a grey heron embroidered on the breast; a grey silk waistcoat; a white cravat; grey breeches; white stockings and black buckled shoes; grey kid gloves.
3. A woodcutter's clothes: a faded brown-and-ochre checked flannel shirt with the sleeves rolled; a leather waistcoat; canvas trousers held up with braces; heavy hobnailed boots; a coil of rope at the belt; sawdust everywhere.
4. Maleficio's high-backed robes: long charcoal scholar's robes to the floor, layered, with a tall, stiff, fan-shaped back to the collar that rises behind the head; the hems, cuffs and the edge of the high collar glow faintly ember orange, like paper just starting to burn; a dark sash with an archivist's keys; soft dark shoes.
I'll save it as armor-sheet-m11.png.

#### `armor-sheet-m12` · Armor sheet: 4 outfits (A scholar's robes, Old-pattern mail and surcoat, Varkhul's Hollow plate, A cube cultist's robe) · P1

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-scholar`, 2 `armor-old-pattern`, 3 `armor-varkhul`, 4 `armor-cultist`.

1. A scholar's robes: plain brown-and-charcoal scholar's robes to the ankle with the sleeves pushed up, a cord belt with a pen case, soft shoes, ink on the fingers.
2. Old-pattern mail and surcoat: an old-fashioned long mail hauberk to the knees with a padded coif down round the neck; a navy surcoat over it with a faded gold sun badge in an older, simpler style; a plain belt; old riveted boots.
3. Varkhul's Hollow plate: full plate of blackened iron, ash grey at the edges, old and fine under the corruption, with a faded engraved crest on the breastplate; its seams are cracked and leak a dull ember-orange glow from inside; a ragged ash-grey surcoat hanging in strips; armoured gauntlets and sabatons.
4. A cube cultist's robe: a plain black robe to the ankle with a high collar, belted with black cord; a pendant of a small black iron cube on a chain at the chest; a pouch of black iron coins stamped with a cube; black wrapped boots.
I'll save it as armor-sheet-m12.png.

#### `armor-sheet-m13` · Armor sheet: 4 outfits (A bandit's leathers, Kael, the Protectorate Shadow, Sir Corwin's gilded plate, Severin's velvet coat) · P1

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-bandit`, 2 `armor-kael-shadow`, 3 `armor-corwin`, 4 `armor-severin`.

1. A bandit's leathers: a scuffed leather jerkin; a mismatched leather bracer; dark trousers; worn boots; a dirty brown scarf round the neck; a sack of stolen goods at the belt.
2. Kael, the Protectorate Shadow: finer black leathers with violet piping, layered and buckled; a long black coat with a high collar; a violet sash with a gold crown badge pinned to it; a black leather half-mask pulled down round the neck; black gloves; polished black boots.
3. Sir Corwin's gilded plate: full plate armour in polished steel with gold-leaf edging on every plate and a small sun engraved on each pauldron; a long white tabard over it with a large gold sun on the chest and gold trim; gilded gauntlets; armoured boots.
4. Severin's velvet coat: a long, close-fitting coat of black velvet to the ankles, with a high collar lined in wine red and tarnished silver clasps; a wine-red waistcoat; a black cravat with a dark-red stone pin; black gloves; long black boots.
I'll save it as armor-sheet-m13.png.

#### `armor-sheet-m14` · Armor sheet: 4 outfits (A valley man's wool coat, A vampire household's livery, A prisoner's shirt, Davin's cut-down tabard) · P2

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-morvaine-man`, 2 `armor-morvaine-livery`, 3 `armor-prisoner`, 4 `armor-davin`.

1. A valley man's wool coat: a high-collared coat of dark grey wool, too thin for the weather, buttoned to the chin; a black knitted scarf; patched charcoal trousers; worn boots.
2. A vampire household's livery: a black livery coat with a high collar and wine-red piping; a black waistcoat; a starched white stock; white gloves; black breeches and stockings; polished black shoes; a ring of keys at the belt.
3. A prisoner's shirt: a plain, stained linen shirt and dark trousers, bare feet, the wrists in iron cuffs with a short chain between them.
4. Davin's cut-down tabard: a mail shirt a little big for the wearer; over it a white tabard with a large gold sun, plainly cut down from a bigger man's and re-hemmed by hand with uneven stitches; a brown belt; leather gloves; plain steel greaves over brown boots.
I'll save it as armor-sheet-m14.png.

#### `armor-sheet-m15` · Armor sheet: 4 outfits (A thrall's tabard, A thrall-knight's black plate, A Gilded knight's armour, The Protectorate's tabard) · P2

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-thrall`, 2 `armor-thrall-knight`, 3 `armor-gilded-knight`, 4 `armor-protectorate`.

1. A thrall's tabard: a dark mail shirt under a wine-red tabard with a black thorn emblem; a high black wool collar; black leather gloves; dark trousers; black boots.
2. A thrall-knight's black plate: full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons.
3. A Gilded knight's armour: steel plate with gold-painted edges (plainer than a commander's); a white tabard with a gold sun on the chest; armoured boots.
4. The Protectorate's tabard: the navy-blue Watch coat with brass buttons, and over it a violet tabard with a gold border and a gold stylised crown on the chest; a violet armband; a brown belt with a pouch of papers; navy trousers; black boots.
I'll save it as armor-sheet-m15.png.

#### `armor-sheet-m16` · Armor sheet: 4 outfits (A Protectorate warden's plate, Ancient bronze scale, A pikeman's breastplate, An Ashfall raider's kit) · P2

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-protectorate-warden`, 2 `armor-unmade-bronze`, 3 `armor-pikeman`, 4 `armor-ashfall-raider`.

1. A Protectorate warden's plate: heavy steel plate with a violet enamel finish on the breastplate and pauldrons and gold edging, a stylised gold crown embossed on the chest; a violet surcoat skirt; heavy gauntlets and sabatons.
2. Ancient bronze scale: ancient bronze scale armour, bronze greaves, a short pleated kilt, sandals; the colours faded and slightly greyed, as if not quite here.
3. A pikeman's breastplate: a steel breastplate and tassets over a padded doublet with puffed sleeves, breeches and tall boots; the colours faded and slightly greyed, as if not quite here.
4. An Ashfall raider's kit: patched black-and-grey leather armour over a mail shirt; a violet strip of cloth tied round the near arm; wrapped boots.
I'll save it as armor-sheet-m16.png.

#### `armor-sheet-m17` · Armor sheet: 1 outfit (A tavern apron, on the man's base) · P0

**Basis:** Edits `armor-sheet-male`. Cells: 1 `armor-tavern-m`.

1. A tavern apron, on the man's base: a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots.
Leave figures 2, 3 and 4 exactly as they are: bald, in the plain underwear, unchanged.
I'll save it as armor-sheet-m17.png.

#### `armor-sheet-f01` · Armor sheet: 4 outfits (The traveller, on the woman's base, The Soul-Bound's traveller, Sera's ranger coat, Mira's archivist's clothes) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-traveller-f`, 2 `armor-sb-traveller`, 3 `armor-sera`, 4 `armor-mira`.

1. The traveller, on the woman's base: a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest.
2. The Soul-Bound's traveller: a fitted, knee-length coat-tunic in deep brick crimson (warm and bright, not a wine colour) with a high collar, a row of dark buttons and dark trim, split at the hips; a cream shirt under it, the cuffs showing; a dark leather belt with two pouches and, at the back, a short whittling knife with a cord through its handle; charcoal-grey fitted trousers; tall dark-brown boots to the knee; dark fingerless gloves; and a small driftwood herring on a waxed cord at the neck.
3. Sera's ranger coat: a moss-green ranger's coat to mid-thigh, fitted and belted, split at the sides, with a stand collar and thin cream piping; an oatmeal shirt under it; a wide belt of undyed leather with a hunting knife, a pouch and a quiver of grey-fletched arrows at the far hip; olive trousers; tall undyed-leather boots laced to the knee; a leather bracer on the far forearm.
4. Mira's archivist's clothes: a cream blouse with full sleeves pushed up to the elbow; a fitted ink-blue waistcoat-bodice, laced at the front, with dark-blue piping; a long parchment-coloured skirt to the ankle, the hem worn; sensible brown lace-up boots; a brown leather belt with a small pouch and an ink bottle; and a battered brown leather satchel worn across the body, stuffed with papers, with three leather scroll tubes strapped to its side.
I'll save it as armor-sheet-f01.png.

#### `armor-sheet-f02` · Armor sheet: 4 outfits (Corra's net-mender's clothes, Captain Odelle's sea-coat, A refugee woman's clothes, A refugee child's smock) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-corra`, 2 `armor-odelle`, 3 `armor-refugee-woman`, 4 `armor-refugee-child`.

1. Corra's net-mender's clothes: a thick, cove-blue (a soft, sea-grey blue) knitted shawl round the shoulders, knotted at the front; an oatmeal blouse with the sleeves rolled; a faded blue kerchief at the throat; a long apron of oiled brown canvas over a long brown wool skirt; plain worn boots; a ball of tarred twine and a wooden netting needle hanging from the belt on a cord.
2. Captain Odelle's sea-coat: a white linen shirt, open at the neck, the sleeves loose; a fitted crimson waistcoat with brass buttons; a long charcoal sea-coat worn open, its skirts to the knee and its cuffs turned back; charcoal breeches; tall black sea boots, salt-stained; a wide leather belt holding a short one-shot flintlock pistol.
3. A refugee woman's clothes: an oatmeal shirt patched with sacking; a charcoal wool waistcoat; a long, mud-hemmed chestnut-brown skirt; boots split at the toes and bound with twine.
4. A refugee child's smock: an oversized oatmeal smock belted with a length of rope; a small charcoal shawl round the shoulders; brown woollen stockings, one sagging; boots far too big.
I'll save it as armor-sheet-f02.png.

#### `armor-sheet-f03` · Armor sheet: 4 outfits (A glassblower's apron, Elara's riding coat, Elara in disguise, A tavern apron) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-glassblower`, 2 `armor-elara`, 3 `armor-elara-disguise`, 4 `armor-tavern`.

1. A glassblower's apron: a blush-pink linen shirt with the sleeves rolled up; a teal waistcoat; a long sand-coloured skirt; a thick leather glassblower's apron, scorched and pitted; sturdy boots; both hands wrapped in clean linen bandages to the wrist.
2. Elara's riding coat: a fitted riding coat to the knee in deep royal blue, with a high collar, a double row of small gold buttons and sun-gold trim at the hem, cuffs and collar, split at the back; a short royal-blue cape over the near shoulder only, fastened with a gold sun-shaped clasp; a white shirt; dark navy riding breeches; tall black riding boots; brown leather riding gloves; a slim sword at the hip.
3. Elara in disguise: plain clothes with no gold anywhere: a plain charcoal waistcoat over a white shirt; dark breeches; scuffed riding boots; a plain sword at the hip.
4. A tavern apron: a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots.
I'll save it as armor-sheet-f03.png.

#### `armor-sheet-f04` · Armor sheet: 4 outfits (A townswoman's clothes, An old woman's dress and shawl, A temple sister's tabard, A fencing-master's doublet) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-villager-woman`, 2 `armor-elder-woman`, 3 `armor-temple`, 4 `armor-fencer`.

1. A townswoman's clothes: a cream blouse with full sleeves; a teal bodice-waistcoat laced at the front; a long mustard skirt with a dusty-pink underskirt showing at the hem; a white apron; brown shoes.
2. An old woman's dress and shawl: a plain grey-brown wool dress to the ankle; a soft, muted mauve-grey knitted shawl crossed over both shoulders and tied at the back; a clean linen apron; soft, worn shoes.
3. A temple sister's tabard: a white blouse; an ochre tabard-tunic with a small gold sun embroidered on the chest; a long sand-coloured skirt; an ochre shawl; plain sandals.
4. A fencing-master's doublet: a fitted, quilted fencing doublet in cream with navy panels and brass buckles down one side; leather bracers on both forearms; a wide belt hung with eight small enamel tokens, each a different colour; navy breeches; soft leather boots.
I'll save it as armor-sheet-f04.png.

#### `armor-sheet-f05` · Armor sheet: 4 outfits (The Lodge ranger's kit, A lady of the court's gown, A trader's waistcoat, A baker's dress and apron) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-lodge-ranger`, 2 `armor-noble-gown`, 3 `armor-trader`, 4 `armor-baker`.

1. The Lodge ranger's kit: an oatmeal shirt; a forest-green leather jerkin; an olive ranger's coat to mid-thigh with a moss-green collar; olive trousers; tall laced boots; a thick leather falconer's glove on the far hand; a short sword at the hip.
2. A lady of the court's gown: a fitted gown in dusty rose silk with a square neckline, long sleeves slashed with cream, a fine embroidered bodice, and a full skirt to the floor; a pearl necklace.
3. A trader's waistcoat: a cream shirt with rolled sleeves; a tan suede waistcoat with many pockets; brown trousers; tall brown boots; a leather satchel across the body; a small set of brass scales at the belt.
4. A baker's dress and apron: a long-sleeved dress in deep brick red with the sleeves pushed up; a big white apron dusted with flour; sturdy brown shoes.
I'll save it as armor-sheet-f05.png.

#### `armor-sheet-f06` · Armor sheet: 4 outfits (A pilgrim's robe, A maid's dress, Padded armour, woman's base, Leather armour, woman's base) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-pilgrim`, 2 `armor-maid`, 3 `armor-padded-f`, 4 `armor-leather-f`.

1. A pilgrim's robe: a long, dusty, dark plum-brown travelling robe to the ankle with its hood down, belted with a rope; a small round brass sun token on a cord at the chest; sandals over wrapped feet; a cloth bag of bread at the hip.
2. A maid's dress: a dusty-blue dress with short puffed sleeves; a white apron with a frill; black shoes.
3. Padded armour, woman's base: a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots.
4. Leather armour, woman's base: boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel.
I'll save it as armor-sheet-f06.png.

#### `armor-sheet-f07` · Armor sheet: 4 outfits (Studded leather, woman's base, Hide armour, woman's base, Chain shirt, woman's base, Chain mail, woman's base) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-studded-f`, 2 `armor-hide-f`, 3 `armor-chain-shirt-f`, 4 `armor-chain-mail-f`.

1. Studded leather, woman's base: the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots.
2. Hide armour, woman's base: bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots.
3. Chain shirt, woman's base: a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots.
4. Chain mail, woman's base: a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots.
I'll save it as armor-sheet-f07.png.

#### `armor-sheet-f08` · Armor sheet: 4 outfits (Scale mail, woman's base, Plate armour, woman's base, Mage robes, woman's base, The Champion's tabard, on the woman's base) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-scale-f`, 2 `armor-plate-f`, 3 `armor-mage-robes-f`, 4 `armor-champion-f`.

1. Scale mail, woman's base: a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots.
2. Plate armour, woman's base: full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate.
3. Mage robes, woman's base: a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes.
4. The Champion's tabard, on the woman's base: the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges.
I'll save it as armor-sheet-f08.png.

#### `armor-sheet-f09` · Armor sheet: 4 outfits (An archive apprentice's smock, The Archmage's robes, Brenna's ash-walker kit, Mother Gannet's shawl and dress) · P1

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-mira-apprentice`, 2 `armor-thessaly`, 3 `armor-brenna`, 4 `armor-gannet`.

1. An archive apprentice's smock: an oversized parchment-coloured apprentice's smock to the knee with ink-blue cuffs, belted with a cord; dark stockings; plain shoes.
2. The Archmage's robes: an A-line robe to the ankle in deep cobalt blue, with wide sleeves and two bands of trim at the hem and cuffs carrying a plain, repeated geometric motif in pale silver thread; a short cobalt mantle over the shoulders with a high collar; a dark sash with a few small brass keys and a slim book on a chain; plain dark boots. On the near hand, the ring finger and the little finger are missing, healed long ago.
3. Brenna's ash-walker kit: a black-dyed leather jerkin over a mail shirt; one old, cracked knight's pauldron on the near shoulder, finer than the rest of the kit, with a worn engraved crest; black trousers; boots wrapped in grey cloth to the knee.
4. Mother Gannet's shawl and dress: a heavy mustard-yellow wool shawl round the shoulders, pinned with a large brass brooch; a brown wool dress to the floor; a ring of iron keys and a fat coin purse at the belt; rings on several fingers.
I'll save it as armor-sheet-f09.png.

#### `armor-sheet-f10` · Armor sheet: 4 outfits (A pedlar's patched coat, The Lord Protector's coat, The Sovereign's armour, Valencia's gown) · P1

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-pedlar`, 2 `armor-lord-protector`, 3 `armor-sovereign`, 4 `armor-valencia`.

1. A pedlar's patched coat: a patched mustard-and-brown skirt over trousers; a man's old green coat with deep pockets; a striped scarf; stout boots; a smooth grey stone with a hole in it hanging on a cord at the neck.
2. The Lord Protector's coat: a long coat to mid-calf in deep navy blue, close at the chest and flaring at the hem, with a stiff standing collar, a double row of small gold buttons and narrow violet-and-gold edging, a rich violet lining showing at the front opening and the turned-back cuffs; a cream shirt and dark cravat under it; charcoal trousers and polished black knee boots; a dark belt with a heavy gold key with a sun-shaped bow on the near hip; the driftwood herring on its cord at the neck; and a faint violet light leaking from the belt pouch.
3. The Sovereign's armour: dark, close-fitting plate armour of blackened steel with violet-and-gold edging on every plate, over a long navy coat whose high, stiff standing collar is turned up round the jaw and whose violet lining shows at the split of the skirts; armoured boots; no herring at the neck.
4. Valencia's gown: a fitted bone-white gown, long-sleeved, with a very high, stiff standing collar that frames the jaw; a wine-red underskirt showing at the front split of the skirt; thin wine-red trim along the seams; long dark-wine gloves to above the elbow; neat black heeled boots.
I'll save it as armor-sheet-f10.png.

#### `armor-sheet-f11` · Armor sheet: 4 outfits (The abbey habit, Mother Anselle's habit, A beekeeper's smock, Lady Isadora's gown) · P2

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-abbey-habit`, 2 `armor-anselle`, 3 `armor-beekeeper`, 4 `armor-isadora`.

1. The abbey habit: a plain, ankle-length habit of soft grey wool with a white linen collar; a plain rope belt with a small carved wooden bee on a cord; a white linen apron, a little stained with honey; plain dark shoes.
2. Mother Anselle's habit: a plain ankle-length habit of soft grey wool with a white linen collar and white cuffs; a wide leather belt with a heavy iron ring of many keys at the hip and a short iron mace; a small carved wooden bee on a cord round the neck; sturdy black shoes.
3. A beekeeper's smock: a brown wool habit with the sleeves rolled to the elbow; a honey-yellow canvas smock-apron over it, sticky at the front; thick leather gloves tucked in the belt; a hive tool on a cord; heavy clogs.
4. Lady Isadora's gown: a long gown in deep midnight blue with fitted sleeves, a modest square neckline, a long train behind, and fine silver embroidery of small night-flowers at the hem; a pair of gardening gloves tucked into the belt; a small pair of silver shears on a chain.
I'll save it as armor-sheet-f11.png.

#### `armor-sheet-f12` · Armor sheet: 4 outfits (A little girl's dress, A valley woman's wool dress, A linen shift and shawl, A lay sister's habit) · P2

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-child-dress`, 2 `armor-morvaine-woman`, 3 `armor-shift`, 4 `armor-abbey-lay`.

1. A little girl's dress: a plain dark-blue dress to the shin with a white collar, cut for a child (the game's morph makes her small); dark stockings; little black shoes.
2. A valley woman's wool dress: a high-collared dress of dark grey wool to the ankle; a black wool shawl pinned with a pewter brooch; a dark apron; thick dark stockings; black boots; a small pewter cup on a chain at the belt.
3. A linen shift and shawl: a plain, greyish linen shift to the shin; a dark wool shawl round the shoulders; a linen bandage round the inside of the far elbow; bare feet.
4. A lay sister's habit: a plain ankle-length habit of undyed oatmeal wool; a brown work apron with a pocket of seed packets; a rope belt with a small wooden bee charm; clogs.
I'll save it as armor-sheet-f12.png.

#### `armor-sheet-f13` · Armor sheet: 4 outfits (A Guild apprentice's robe, A smuggler's oilskin, A cove child's clothes, on the woman's base, A street child's hand-me-downs, on the woman's base) · P0

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-guild-apprentice`, 2 `armor-smuggler`, 3 `armor-child-cove-f`, 4 `armor-urchin-f`.

1. A Guild apprentice's robe: a short cobalt-blue apprentice's robe to the knee with plain trim, belted with a cord; grey trousers; soft shoes; a satchel.
2. A smuggler's oilskin: a long, dark oilskin coat, salt-stained; a grey knitted jersey; a wide belt with hidden pouches; dark trousers; tall sea boots.
3. A cove child's clothes, on the woman's base: a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet.
4. A street child's hand-me-downs, on the woman's base: a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet.
I'll save it as armor-sheet-f13.png.

#### `armor-sheet-f14` · Armor sheet: 1 outfit (A thrall-knight's black plate, on the woman's base) · P2

**Basis:** Edits `armor-sheet-female`. Cells: 1 `armor-thrall-knight-f`.

1. A thrall-knight's black plate, on the woman's base: full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons.
Leave figures 2, 3 and 4 exactly as they are: bald, in the plain underwear, unchanged.
I'll save it as armor-sheet-f14.png.

### 5.1 The player and the Soul-Bound

#### `armor-traveller` · The traveller (the player's default) · P0

**Basis:** Edits `base-male`. From the guide figure PLAYER (BASE); `03-art-overhaul.md` §7.2 (Act 1); the item `travelling-clothes`.

Edit the attached base image: dress the body in a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-traveller.png.

#### `armor-traveller-f` · The traveller, on the woman's base · P0

**Basis:** Edits `base-female`. As `armor-traveller`, fitted to the woman's base.

Edit the attached base image: dress the body in a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-traveller-f.png.

#### `armor-sb-traveller` · The Soul-Bound's traveller · P0

**Basis:** Edits `base-female`. From the guide figure SERA (burgundy, a dark cape); `Origins.cs` (crimson); `03-art-overhaul.md` §6.4 (the herring, the whittling knife).

Edit the attached base image: dress the body in a fitted, knee-length coat-tunic in deep brick crimson (warm and bright, not a wine colour) with a high collar, a row of dark buttons and dark trim, split at the hips; a cream shirt under it, the cuffs showing; a dark leather belt with two pouches and, at the back, a short whittling knife with a cord through its handle; charcoal-grey fitted trousers; tall dark-brown boots to the knee; dark fingerless gloves; and a small driftwood herring on a waxed cord at the neck. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-sb-traveller.png.

#### `armor-champion` · The Champion's tabard · P1

**Basis:** Edits `base-male`. **(derived)** From `03-art-overhaul.md` §7.2 (a Champion's tabard over the traveller's clothes, gold Soul-Bond trim) and §7.3 (sun gold).

Edit the attached base image: dress the body in the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-champion.png.

#### `armor-champion-f` · The Champion's tabard, on the woman's base · P1

**Basis:** Edits `base-female`. As `armor-champion`, fitted to the woman's base.

Edit the attached base image: dress the body in the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-champion-f.png.

#### `armor-lord-protector` · The Lord Protector's coat · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.2 (long, high-collared, deep blue with violet lining) and §6.4 (the outer key; the shard's light in the pouch).

Edit the attached base image: dress the body in a long coat to mid-calf in deep navy blue, close at the chest and flaring at the hem, with a stiff standing collar, a double row of small gold buttons and narrow violet-and-gold edging, a rich violet lining showing at the front opening and the turned-back cuffs; a cream shirt and dark cravat under it; charcoal trousers and polished black knee boots; a dark belt with a heavy gold key with a sun-shaped bow on the near hip; the driftwood herring on its cord at the neck; and a faint violet light leaking from the belt pouch. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-lord-protector.png.

#### `armor-sovereign` · The Sovereign's armour · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.2 (dark plate with violet-gold edging, the coat's collar kept).

Edit the attached base image: dress the body in dark, close-fitting plate armour of blackened steel with violet-and-gold edging on every plate, over a long navy coat whose high, stiff standing collar is turned up round the jaw and whose violet lining shows at the split of the skirts; armoured boots; no herring at the neck. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-sovereign.png.

#### `armor-child-cove` · A cove child's clothes · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (the children on the pier: fishing-town kids, cove blue).

Edit the attached base image: dress the body in a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-child-cove.png.

#### `armor-child-cove-f` · A cove child's clothes, on the woman's base · P0

**Basis:** Edits `base-female`. As `armor-child-cove`, fitted to the woman's base, for `sb-child`, whose face is painted on it.

Edit the attached base image: dress the body in a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-child-cove-f.png.

### 5.2 The party

#### `armor-sera` · Sera's ranger coat · P0

**Basis:** Edits `base-female`. From the guide figure KIAN (a green ranger's coat); `03-art-overhaul.md` §7.4 (moss green and undyed leather).

Edit the attached base image: dress the body in a moss-green ranger's coat to mid-thigh, fitted and belted, split at the sides, with a stand collar and thin cream piping; an oatmeal shirt under it; a wide belt of undyed leather with a hunting knife, a pouch and a quiver of grey-fletched arrows at the far hip; olive trousers; tall undyed-leather boots laced to the knee; a leather bracer on the far forearm. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-sera.png.

#### `armor-mira` · Mira's archivist's clothes · P0

**Basis:** Edits `base-female`. From the guide figures NOREL and CATE; `03-art-overhaul.md` §7.4 (a satchel with three scroll tubes; ink blue and parchment).

Edit the attached base image: dress the body in a cream blouse with full sleeves pushed up to the elbow; a fitted ink-blue waistcoat-bodice, laced at the front, with dark-blue piping; a long parchment-coloured skirt to the ankle, the hem worn; sensible brown lace-up boots; a brown leather belt with a small pouch and an ink bottle; and a battered brown leather satchel worn across the body, stuffed with papers, with three leather scroll tubes strapped to its side. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-mira.png.

#### `armor-mira-apprentice` · An archive apprentice's smock · P1

**Basis:** Edits `base-female`. **(derived)** From A1-C8-01 (`echo mira-young`).

Edit the attached base image: dress the body in an oversized parchment-coloured apprentice's smock to the knee with ink-blue cuffs, belted with a cord; dark stockings; plain shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-mira-apprentice.png.

#### `armor-kael` · Kael's hooded leathers · P0

**Basis:** Edits `base-male`. From the guide figure DRAINOR (a hooded dark cloak over layered leathers); `03-art-overhaul.md` §7.4 (charcoal, an ember-red sash).

Edit the attached base image: dress the body in a close-fitting dark leather coat to the hip in layered panels, with straps and buckles across the chest; a charcoal half-cloak over the shoulders with its pointed hood hanging down the back; a wide ember-red sash wound round the waist, its ends hanging at the far hip; dark grey trousers; soft dark boots with the legs wrapped in cloth bands to the knee; dark fingerless gloves; a few lockpicks in a strap on the forearm. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-kael.png.

#### `armor-kael-shadow` · Kael, the Protectorate Shadow · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (`kael-shadow`: Protectorate colours).

Edit the attached base image: dress the body in finer black leathers with violet piping, layered and buckled; a long black coat with a high collar; a violet sash with a gold crown badge pinned to it; a black leather half-mask pulled down round the neck; black gloves; polished black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-kael-shadow.png.

#### `armor-brannock` · Brannock's soldier-smith kit · P0

**Basis:** Edits `base-male`. From the guide figure CERRO (a work shirt and an apron); `03-art-overhaul.md` §7.4 (iron grey, tarnished gold, plate); `Origins.cs` (chain).

Edit the attached base image: dress the body in a charcoal padded work shirt with the sleeves pushed up; a chain-mail shirt over it to the hips; a battered iron breastplate with tarnished-gold edging, dented and scratched; one heavy iron pauldron on the near shoulder; a thick dark-leather apron-like skirt to the knees over dark trousers, like a smith's; a wide belt with a big iron buckle and a battered metal flask; heavy iron-capped boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-brannock.png.

#### `armor-valencia` · Valencia's gown · P2

**Basis:** Edits `base-female`. From the guide figure VALENCIA (a pale dress); `03-art-overhaul.md` §7.4 (bone white and wine red; a high stiff collar and long gloves).

Edit the attached base image: dress the body in a fitted bone-white gown, long-sleeved, with a very high, stiff standing collar that frames the jaw; a wine-red underskirt showing at the front split of the skirt; thin wine-red trim along the seams; long dark-wine gloves to above the elbow; neat black heeled boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-valencia.png.

#### `armor-abbey-habit` · The abbey habit · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Sister Vale's habit); §7.5 (the `abbey` pool).

Edit the attached base image: dress the body in a plain, ankle-length habit of soft grey wool with a white linen collar; a plain rope belt with a small carved wooden bee on a cord; a white linen apron, a little stained with honey; plain dark shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-abbey-habit.png.

### 5.3 Act 1 and the towns

#### `armor-corra` · Corra's net-mender's clothes · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a net-mender's shawl, a ball of twine at the belt; cove blue and oiled brown).

Edit the attached base image: dress the body in a thick, cove-blue (a soft, sea-grey blue) knitted shawl round the shoulders, knotted at the front; an oatmeal blouse with the sleeves rolled; a faded blue kerchief at the throat; a long apron of oiled brown canvas over a long brown wool skirt; plain worn boots; a ball of tarred twine and a wooden netting needle hanging from the belt on a cord. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-corra.png.

#### `armor-sailor` · A sailor's clothes · P0

**Basis:** Edits `base-male`. **(derived)** From the Saltwind pool (oilskins, caps, rolled sleeves).

Edit the attached base image: dress the body in a navy-and-cream striped knitted jersey with the sleeves pushed up; a sleeveless brown oilskin waistcoat; canvas trousers rolled to the shin; bare feet with tarred soles; a rope lanyard at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-sailor.png.

#### `armor-odelle` · Captain Odelle's sea-coat · P0

**Basis:** Edits `base-female`. From `Cast.cs` (a white shirt, a crimson waistcoat, a charcoal cloak); the guide figure LEVL; `10-classes.md` (Deckhand: a pistol).

Edit the attached base image: dress the body in a white linen shirt, open at the neck, the sleeves loose; a fitted crimson waistcoat with brass buttons; a long charcoal sea-coat worn open, its skirts to the knee and its cuffs turned back; charcoal breeches; tall black sea boots, salt-stained; a wide leather belt holding a short one-shot flintlock pistol. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-odelle.png.

#### `armor-fisher` · A fisherman's oilskins · P0

**Basis:** Edits `base-male`. **(derived)** From the Saltwind pool; Old Tomas.

Edit the attached base image: dress the body in a thick grey knitted jumper, darned at the elbows; a sleeveless mustard oilskin smock over it, cracked and patched; canvas trousers rolled at the shin; tall black tarred sea boots; a brass fish hook on a cord at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-fisher.png.

#### `armor-red-gull` · A Red Gull's rig · P0

**Basis:** Edits `base-male`. From the guide figure KELL (a striped shirt, a shoulder guard); the art plan (a gull-red bandana and sash).

Edit the attached base image: dress the body in a raspberry-red bandana knotted round the throat; a cream-and-navy striped jersey with the sleeves rolled over the forearms; a scuffed leather shoulder guard on the near shoulder, buckled with straps across the chest; a wide raspberry-red sash at the waist; brown canvas trousers; tall worn boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-red-gull.png.

#### `armor-grell` · Bosun Grell's coat and hook · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a hook for a hand; Red Gull red); the art plan (an open bosun's coat, a gull tattoo).

Edit the attached base image: dress the body in an open, knee-length bosun's coat in raspberry red, frayed at the cuffs and stained with tar, with tarnished brass buttons; an oatmeal shirt under it, half unlaced; a gull tattoo on the near forearm; a broad belt with a big brass buckle; dark canvas trousers; heavy boots. The far hand is gone at the wrist: in its place, a steel hook strapped to a leather cuff. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-grell.png.

#### `armor-watch` · The City Watch uniform · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the Watch's blue); `03-art-overhaul.md` §7.3 (Watch navy and brass).

Edit the attached base image: dress the body in a navy-blue Watch coat to mid-thigh over a padded jack, brass buttons, a small brass badge shaped like a lantern on the chest; a brown leather belt; navy trousers; black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-watch.png.

#### `armor-watch-captain` · The Watch captain's coat · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Hale: Watch navy and brass).

Edit the attached base image: dress the body in a navy-blue Watch coat to mid-thigh over a chain-mail shirt, with brass buttons and a larger brass lantern badge on the chest; a brown leather belt and baldric with a short sword; a rolled paper roster tucked into the belt; navy trousers; worn black boots, well polished. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-watch-captain.png.

#### `armor-clerk` · A clerk's waistcoat · P0

**Basis:** Edits `base-male`. **(derived)** From Fennick, the Guild's and the docks' clerks.

Edit the attached base image: dress the body in an oatmeal shirt with black sleeve garters; a mustard-ochre waistcoat with a watch chain; sand-coloured trousers; polished brown shoes; ink stains on the fingers. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-clerk.png.

#### `armor-refugee-woman` · A refugee woman's clothes · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Hedda: "sacking, charcoal and oat"); the `doomward` pool.

Edit the attached base image: dress the body in an oatmeal shirt patched with sacking; a charcoal wool waistcoat; a long, mud-hemmed chestnut-brown skirt; boots split at the toes and bound with twine. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-refugee-woman.png.

#### `armor-refugee-man` · A refugee man's clothes · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the refugees); the `doomward` pool (grey and black, patched, practical).

Edit the attached base image: dress the body in a faded black wool coat with mismatched patches over a grey shirt; a charcoal scarf; grey trousers tied at the ankle with twine; cracked boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-refugee-man.png.

#### `armor-refugee-child` · A refugee child's smock · P0

**Basis:** Edits `base-female`. **(derived)** From Lise.

Edit the attached base image: dress the body in an oversized oatmeal smock belted with a length of rope; a small charcoal shawl round the shoulders; brown woollen stockings, one sagging; boots far too big. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-refugee-child.png.

#### `armor-glassblower` · A glassblower's apron · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Yselle: blush, teal, sand; burned hands).

Edit the attached base image: dress the body in a blush-pink linen shirt with the sleeves rolled up; a teal waistcoat; a long sand-coloured skirt; a thick leather glassblower's apron, scorched and pitted; sturdy boots; both hands wrapped in clean linen bandages to the wrist. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-glassblower.png.

#### `armor-elara` · Elara's riding coat · P0

**Basis:** Edits `base-female`. From the guide figure ELARA (the long fitted coat), recoloured to royal blue because violet is reserved for {sb}; `03-art-overhaul.md` §7.4 (a short cape on one shoulder; royal blue and sun gold).

Edit the attached base image: dress the body in a fitted riding coat to the knee in deep royal blue, with a high collar, a double row of small gold buttons and sun-gold trim at the hem, cuffs and collar, split at the back; a short royal-blue cape over the near shoulder only, fastened with a gold sun-shaped clasp; a white shirt; dark navy riding breeches; tall black riding boots; brown leather riding gloves; a slim sword at the hip. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elara.png.

#### `armor-elara-disguise` · Elara in disguise · P0

**Basis:** Edits `base-female`. From `Cast.cs` (`elara-hooded`: a charcoal vest); A1-C4-01.

Edit the attached base image: dress the body in plain clothes with no gold anywhere: a plain charcoal waistcoat over a white shirt; dark breeches; scuffed riding boots; a plain sword at the hip. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elara-disguise.png.

#### `armor-leon` · King Leon's robes · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a broad ermine collar; white and sun gold) and §6.4 (`leon-sword`).

Edit the attached base image: dress the body in a long white robe-coat to the ankles with sun-gold trim at the hem and cuffs and a gold sash; a broad collar of ermine (white fur with small black tips) over the shoulders; soft white boots; and at the hip, a long, plain, very old sword in a worn scabbard, its grip blue leather worn pale, a small gold sun on the crossguard. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-leon.png.

#### `armor-crane` · Crane's long coat · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a very long coat, rings that catch the light; plum and silver) and §6.4 (`cube-seal-ring`).

Edit the attached base image: dress the body in a floor-length plum-coloured coat, close at the chest and long in the skirt, with a high standing collar, silver trim and a column of small silver buttons; a dark-grey waistcoat; a white stock at the throat; dark trousers and narrow black boots; silver rings on several fingers, one a signet set with a small black cube. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-crane.png.

#### `armor-royal-livery` · The palace livery · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the palace's navy and gold).

Edit the attached base image: dress the body in a navy-blue livery tunic with a gold sun badge on the chest and gold piping; a white shirt; navy breeches; soft brown boots; a dispatch pouch on a strap across the body. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-royal-livery.png.

#### `armor-royal-guard` · The Royal Guard's armour · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the palace guards in navy and gold).

Edit the attached base image: dress the body in a navy-blue tabard with a large gold sun embroidered on the chest, over polished mail; steel pauldrons with gold edging; a white sash; navy trousers; steel-capped black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-royal-guard.png.

#### `armor-tavern` · A tavern apron · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Hessa: a charcoal vest, an apron); the inns' staff.

Edit the attached base image: dress the body in a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-tavern.png.

#### `armor-tavern-m` · A tavern apron, on the man's base · P0

**Basis:** Edits `base-male`. As `armor-tavern`, fitted to the man's base, for `pip`, whose face is painted on it.

Edit the attached base image: dress the body in a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-tavern-m.png.

#### `armor-villager-woman` · A townswoman's clothes · P0

**Basis:** Edits `base-female`. **(derived)** From the `questeria` pool (bright, layered cloth; aprons); the townswomen.

Edit the attached base image: dress the body in a cream blouse with full sleeves; a teal bodice-waistcoat laced at the front; a long mustard skirt with a dusty-pink underskirt showing at the hem; a white apron; brown shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-villager-woman.png.

#### `armor-villager-man` · A townsman's clothes · P0

**Basis:** Edits `base-male`. **(derived)** From the `questeria` pool; the farmhands and youths.

Edit the attached base image: dress the body in a loose oatmeal shirt with the sleeves rolled and a rust-orange neckerchief; a grey-blue waistcoat, unbuttoned; brown trousers held up with braces; muddy boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-villager-man.png.

#### `armor-elder-woman` · An old woman's dress and shawl · P0

**Basis:** Edits `base-female`. **(derived)** From Hester, Maud and the old women.

Edit the attached base image: dress the body in a plain grey-brown wool dress to the ankle; a soft, muted mauve-grey knitted shawl crossed over both shoulders and tied at the back; a clean linen apron; soft, worn shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elder-woman.png.

#### `armor-merchant` · A merchant's waistcoat · P0

**Basis:** Edits `base-male`. From `Cast.cs` (Osric: white, plum).

Edit the attached base image: dress the body in a white shirt with full sleeves; an embroidered dusty-plum waistcoat; dark burgundy-brown trousers; polished shoes; a tailor's tape round the neck. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-merchant.png.

#### `armor-busker` · A busker's finery · P0

**Basis:** Edits `base-male`. From `Cast.cs` (Lark: blush, rose, sand).

Edit the attached base image: dress the body in a blush-pink shirt with billowing sleeves; a rose waistcoat; sand-coloured trousers; soft boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-busker.png.

#### `armor-temple` · A temple sister's tabard · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Hesper: white, ochre, a long skirt); (derived) the sun badge.

Edit the attached base image: dress the body in a white blouse; an ochre tabard-tunic with a small gold sun embroidered on the chest; a long sand-coloured skirt; an ochre shawl; plain sandals. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-temple.png.

#### `armor-urchin` · A street child's hand-me-downs · P0

**Basis:** Edits `base-male`. From `Cast.cs` (Wren: oat, charcoal, a cloak); the lost boy; the city's children.

Edit the attached base image: dress the body in a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-urchin.png.

#### `armor-urchin-f` · A street child's hand-me-downs, on the woman's base · P0

**Basis:** Edits `base-female`. As `armor-urchin`, fitted to the woman's base, for `wren`, whose face is painted on it.

Edit the attached base image: dress the body in a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-urchin-f.png.

#### `armor-fencer` · A fencing-master's doublet · P0

**Basis:** Edits `base-female`. **(derived)** From Imra Vell, `10-classes.md` §5.1.

Edit the attached base image: dress the body in a fitted, quilted fencing doublet in cream with navy panels and brass buckles down one side; leather bracers on both forearms; a wide belt hung with eight small enamel tokens, each a different colour; navy breeches; soft leather boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-fencer.png.

#### `armor-lodge-ranger` · The Lodge ranger's kit · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Tamsin: oat, forest, olive); `03-art-overhaul.md` §7.3 (moss green for the Lodge).

Edit the attached base image: dress the body in an oatmeal shirt; a forest-green leather jerkin; an olive ranger's coat to mid-thigh with a moss-green collar; olive trousers; tall laced boots; a thick leather falconer's glove on the far hand; a short sword at the hip. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-lodge-ranger.png.

#### `armor-noble` · A courtier's doublet · P0

**Basis:** Edits `base-male`. **(derived)** From the court in `Content/Zones/sunspire-keep.txt` ("Velvet in this heat").

Edit the attached base image: dress the body in a fitted velvet doublet in deep teal with slashed sleeves showing cream silk; a lace collar; dark hose and breeches; soft black shoes with buckles; a ring or two. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-noble.png.

#### `armor-noble-gown` · A lady of the court's gown · P0

**Basis:** Edits `base-female`. **(derived)** From the court in `Content/Zones/sunspire-keep.txt`.

Edit the attached base image: dress the body in a fitted gown in dusty rose silk with a square neckline, long sleeves slashed with cream, a fine embroidered bodice, and a full skirt to the floor; a pearl necklace. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-noble-gown.png.

#### `armor-elder-coat` · An old man's long coat · P0

**Basis:** Edits `base-male`. **(derived)** From the `elder` and `old-man` sheets.

Edit the attached base image: dress the body in a long dove-grey coat to the shin with dark-grey trim and pewter buttons; a slate waistcoat; a white shirt with a soft collar; grey trousers; black shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elder-coat.png.

#### `armor-trader` · A trader's waistcoat · P0

**Basis:** Edits `base-female`. **(derived)** From the `trader` sheet.

Edit the attached base image: dress the body in a cream shirt with rolled sleeves; a tan suede waistcoat with many pockets; brown trousers; tall brown boots; a leather satchel across the body; a small set of brass scales at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-trader.png.

#### `armor-baker` · A baker's dress and apron · P0

**Basis:** Edits `base-female`. **(derived)** From the `baker` sheet.

Edit the attached base image: dress the body in a long-sleeved dress in deep brick red with the sleeves pushed up; a big white apron dusted with flour; sturdy brown shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-baker.png.

#### `armor-smith` · A smith's apron · P0

**Basis:** Edits `base-male`. **(derived)** From the `smith` sheet; masons.

Edit the attached base image: dress the body in a cream shirt with the sleeves rolled above the elbow, open at the throat; a heavy, scorched brown leather apron to the knee; thick leather gloves tucked into the apron string; dark trousers; heavy boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-smith.png.

#### `armor-road` · A traveller's road clothes · P0

**Basis:** Edits `base-male`. **(derived)** From the `traveller` sheet.

Edit the attached base image: dress the body in a dark-brown tunic belted at the waist; charcoal trousers; boots worn through at the toes and wrapped in cloth; a small pack on the back with a bedroll and a tin cup. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-road.png.

#### `armor-courier` · A courier's clothes · P0

**Basis:** Edits `base-male`. **(derived)** From the `courier` sheet.

Edit the attached base image: dress the body in a cream shirt with rolled sleeves; dark-grey braces; charcoal trousers tucked into soft running boots; a big leather mail satchel bulging with letters on a strap across the body. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-courier.png.

#### `armor-pilgrim` · A pilgrim's robe · P0

**Basis:** Edits `base-female`. **(derived)** From the `pilgrim` sheet; `enc-pilgrims` (a sun token).

Edit the attached base image: dress the body in a long, dusty, dark plum-brown travelling robe to the ankle with its hood down, belted with a rope; a small round brass sun token on a cord at the chest; sandals over wrapped feet; a cloth bag of bread at the hip. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-pilgrim.png.

#### `armor-porter` · A porter's clothes · P0

**Basis:** Edits `base-male`. **(derived)** From the `porter` sheet.

Edit the attached base image: dress the body in a sweat-stained cream shirt with rolled sleeves; brown braces; a leather shoulder pad on the far shoulder; brown canvas trousers; heavy boots; a crate hook at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-porter.png.

#### `armor-maid` · A maid's dress · P0

**Basis:** Edits `base-female`. **(derived)** From the `maid` sheet.

Edit the attached base image: dress the body in a dusty-blue dress with short puffed sleeves; a white apron with a frill; black shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-maid.png.

#### `armor-caravan` · A caravan master's duster · P1

**Basis:** Edits `base-male`. **(derived)** From Garrick, A1-C5-01.

Edit the attached base image: dress the body in a long oiled-brown duster coat to the shin, open; a faded grey scarf against the road dust; a leather waistcoat over a grey shirt; dark trousers; long riding boots; a coiled driving whip at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-caravan.png.

#### `armor-aldric` · Aldric's worn plate · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (worn plate; steel and faded red).

Edit the attached base image: dress the body in worn steel plate that doesn't quite match, dented and scratched (a breastplate, rounded pauldrons, vambraces and greaves), over chain mail and a padded jack; a battered sword belt; old boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-aldric.png.

#### `armor-recruit` · A recruit's gambeson · P1

**Basis:** Edits `base-male`. **(derived)** From A1-C7-01.

Edit the attached base image: dress the body in a quilted navy-blue gambeson with brass buttons, a little too long; a brown belt; grey wool trousers; boots with snow on them; a grey woollen scarf. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-recruit.png.

#### `armor-thessaly` · The Archmage's robes · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Guild cobalt; two fingers missing on the near hand).

Edit the attached base image: dress the body in an A-line robe to the ankle in deep cobalt blue, with wide sleeves and two bands of trim at the hem and cuffs carrying a plain, repeated geometric motif in pale silver thread; a short cobalt mantle over the shoulders with a high collar; a dark sash with a few small brass keys and a slim book on a chain; plain dark boots. On the near hand, the ring finger and the little finger are missing, healed long ago. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-thessaly.png.

#### `armor-brenna` · Brenna's ash-walker kit · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (one cracked knight's pauldron; ash grey and black).

Edit the attached base image: dress the body in a black-dyed leather jerkin over a mail shirt; one old, cracked knight's pauldron on the near shoulder, finer than the rest of the kit, with a worn engraved crest; black trousers; boots wrapped in grey cloth to the knee. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-brenna.png.

#### `armor-gannet` · Mother Gannet's shawl and dress · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a heavy shawl; mustard and brown).

Edit the attached base image: dress the body in a heavy mustard-yellow wool shawl round the shoulders, pinned with a large brass brooch; a brown wool dress to the floor; a ring of iron keys and a fat coin purse at the belt; rings on several fingers. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-gannet.png.

#### `armor-cider-presser` · A cider-presser's apron · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Tobin: a cider-presser's apron, a burn on the near hand; apple green and brown).

Edit the attached base image: dress the body in an apple-green work shirt with the sleeves rolled; a heavy, stained brown leather presser's apron; brown trousers; sturdy boots; a wide, old burn scar across the back of the near hand. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-cider-presser.png.

#### `armor-levy` · A levy coat · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Tobin's levy coat); `07-companions.md` (`mira-tobin`).

Edit the attached base image: dress the body in a patched grey levy coat over a work shirt, mud to the knees, rope for a belt, worn boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-levy.png.

#### `armor-harrow` · Lord Harrow's heron coat · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a green coat with a grey heron on the breast; heron green and grey).

Edit the attached base image: dress the body in a long frock coat in heron green (a deep grey-green) with a grey heron embroidered on the breast; a grey silk waistcoat; a white cravat; grey breeches; white stockings and black buckled shoes; grey kid gloves. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-harrow.png.

#### `armor-woodcutter` · A woodcutter's clothes · P1

**Basis:** Edits `base-male`. **(derived)** From Hobb and the cutters.

Edit the attached base image: dress the body in a faded brown-and-ochre checked flannel shirt with the sleeves rolled; a leather waistcoat; canvas trousers held up with braces; heavy hobnailed boots; a coil of rope at the belt; sawdust everywhere. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-woodcutter.png.

#### `armor-maleficio` · Maleficio's high-backed robes · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (scholar's robes with a stiff high back; charcoal with ember edges).

Edit the attached base image: dress the body in long charcoal scholar's robes to the floor, layered, with a tall, stiff, fan-shaped back to the collar that rises behind the head; the hems, cuffs and the edge of the high collar glow faintly ember orange, like paper just starting to burn; a dark sash with an archivist's keys; soft dark shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-maleficio.png.

#### `armor-scholar` · A scholar's robes · P1

**Basis:** Edits `base-male`. **(derived)** From `maleficio-young`; the archive's and the Guild's scholars.

Edit the attached base image: dress the body in plain brown-and-charcoal scholar's robes to the ankle with the sleeves pushed up, a cord belt with a pen case, soft shoes, ink on the fingers. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-scholar.png.

#### `armor-old-pattern` · Old-pattern mail and surcoat · P1

**Basis:** Edits `base-male`. **(derived)** From `enc-relief-ghost` ("old-pattern armour"); the Unmade's knight.

Edit the attached base image: dress the body in an old-fashioned long mail hauberk to the knees with a padded coif down round the neck; a navy surcoat over it with a faded gold sun badge in an older, simpler style; a plain belt; old riveted boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-old-pattern.png.

#### `armor-pedlar` · A pedlar's patched coat · P1

**Basis:** Edits `base-female`. **(derived)** From `enc-mags`.

Edit the attached base image: dress the body in a patched mustard-and-brown skirt over trousers; a man's old green coat with deep pockets; a striped scarf; stout boots; a smooth grey stone with a hole in it hanging on a cord at the neck. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-pedlar.png.

#### `armor-varkhul` · Varkhul's Hollow plate · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Hollowed plate; ash, ember); A1-C6 (a knight once).

Edit the attached base image: dress the body in full plate of blackened iron, ash grey at the edges, old and fine under the corruption, with a faded engraved crest on the breastplate; its seams are cracked and leak a dull ember-orange glow from inside; a ragged ash-grey surcoat hanging in strips; armoured gauntlets and sabatons. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-varkhul.png.

### 5.4 Act 2

#### `armor-anselle` · Mother Anselle's habit · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (an abbey habit with no hood; a heavy key ring).

Edit the attached base image: dress the body in a plain ankle-length habit of soft grey wool with a white linen collar and white cuffs; a wide leather belt with a heavy iron ring of many keys at the hip and a short iron mace; a small carved wooden bee on a cord round the neck; sturdy black shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-anselle.png.

#### `armor-beekeeper` · A beekeeper's smock · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Orla: honey yellow and brown).

Edit the attached base image: dress the body in a brown wool habit with the sleeves rolled to the elbow; a honey-yellow canvas smock-apron over it, sticky at the front; thick leather gloves tucked in the belt; a hive tool on a cord; heavy clogs. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-beekeeper.png.

#### `armor-corwin` · Sir Corwin's gilded plate · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (gilded plate; a sun tabard; gold-white).

Edit the attached base image: dress the body in full plate armour in polished steel with gold-leaf edging on every plate and a small sun engraved on each pauldron; a long white tabard over it with a large gold sun on the chest and gold trim; gilded gauntlets; armoured boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-corwin.png.

#### `armor-severin` · Severin's velvet coat · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a long black velvet coat; black and wine red).

Edit the attached base image: dress the body in a long, close-fitting coat of black velvet to the ankles, with a high collar lined in wine red and tarnished silver clasps; a wine-red waistcoat; a black cravat with a dark-red stone pin; black gloves; long black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-severin.png.

#### `armor-isadora` · Lady Isadora's gown · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a long gown with a train; midnight blue).

Edit the attached base image: dress the body in a long gown in deep midnight blue with fitted sleeves, a modest square neckline, a long train behind, and fine silver embroidery of small night-flowers at the hem; a pair of gardening gloves tucked into the belt; a small pair of silver shears on a chain. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-isadora.png.

#### `armor-child-dress` · A little girl's dress · P2

**Basis:** Edits `base-female`. **(derived)** From `echo valencia-child`.

Edit the attached base image: dress the body in a plain dark-blue dress to the shin with a white collar, cut for a child (the game's morph makes her small); dark stockings; little black shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-child-dress.png.

#### `armor-morvaine-man` · A valley man's wool coat · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.5 (the `morvaine` pool: dark wool, high collars).

Edit the attached base image: dress the body in a high-collared coat of dark grey wool, too thin for the weather, buttoned to the chin; a black knitted scarf; patched charcoal trousers; worn boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-morvaine-man.png.

#### `armor-morvaine-woman` · A valley woman's wool dress · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.5 (the `morvaine` pool).

Edit the attached base image: dress the body in a high-collared dress of dark grey wool to the ankle; a black wool shawl pinned with a pewter brooch; a dark apron; thick dark stockings; black boots; a small pewter cup on a chain at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-morvaine-woman.png.

#### `armor-shift` · A linen shift and shawl · P2

**Basis:** Edits `base-female`. **(derived)** From Ide, `08-random-encounters.md` §5.1.

Edit the attached base image: dress the body in a plain, greyish linen shift to the shin; a dark wool shawl round the shoulders; a linen bandage round the inside of the far elbow; bare feet. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-shift.png.

#### `armor-morvaine-livery` · A vampire household's livery · P2

**Basis:** Edits `base-male`. **(derived)** From the blood-house steward.

Edit the attached base image: dress the body in a black livery coat with a high collar and wine-red piping; a black waistcoat; a starched white stock; white gloves; black breeches and stockings; polished black shoes; a ring of keys at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-morvaine-livery.png.

#### `armor-abbey-lay` · A lay sister's habit · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.5 (the `abbey` pool: habits, aprons).

Edit the attached base image: dress the body in a plain ankle-length habit of undyed oatmeal wool; a brown work apron with a pocket of seed packets; a rope belt with a small wooden bee charm; clogs. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-abbey-lay.png.

### 5.5 Act 3

#### `armor-prisoner` · A prisoner's shirt · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (`severin-cell`: a plain shirt).

Edit the attached base image: dress the body in a plain, stained linen shirt and dark trousers, bare feet, the wrists in iron cuffs with a short chain between them. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-prisoner.png.

#### `armor-davin` · Davin's cut-down tabard · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Corwin's tabard cut down to fit).

Edit the attached base image: dress the body in a mail shirt a little big for the wearer; over it a white tabard with a large gold sun, plainly cut down from a bigger man's and re-hemmed by hand with uneven stitches; a brown belt; leather gloves; plain steel greaves over brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-davin.png.

#### `armor-vorlaith` · Vorlaith's glass body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a body of cracked glass with light moving inside).

Edit the attached base image: change everything below the neck into a body made entirely of clear, colourless glass, cracked all over with fine branching fractures, a cold white light drifting slowly inside like smoke in a bottle, brightest in the chest; the glass forms long, still robes that fall to the ground and a high collar; long glass fingers, relaxed. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-vorlaith.png.

#### `armor-guild-apprentice` · A Guild apprentice's robe · P2

**Basis:** Edits `base-female`. **(derived)** From A3-C2-01; Guild cobalt.

Edit the attached base image: dress the body in a short cobalt-blue apprentice's robe to the knee with plain trim, belted with a cord; grey trousers; soft shoes; a satchel. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-guild-apprentice.png.

#### `armor-elara-field` · Elara, leading the resistance · P2

**Basis:** Edits `armor-elara`. From `03-art-overhaul.md` §7.4 and §6.5 (Leon's sword on her back from A3-C2-05).

Edit the attached image (armor-elara.png, the armor on the base): change ONLY her clothes after months of war in a forest: the royal-blue riding coat is stained with mud and torn at the hem, the gold trim dulled, the slim sword gone from the hip; and her father's long, plain, very old sword is strapped across her back, its blue leather grip and the small gold sun on its crossguard showing over the near shoulder. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elara-field.png.

#### `armor-elara-queen` · Elara, crowned · P2

**Basis:** Edits `armor-elara`. From `03-art-overhaul.md` §7.4 (crowned, A3-C5-06).

Edit the attached image (armor-elara.png, the armor on the base): change ONLY the cape: replace the short one-shoulder cape with a short white-and-gold mantle over both shoulders, and put her father's old sword (blue leather grip, a small gold sun on the crossguard) at her hip in place of the slim one. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-elara-queen.png.

#### `armor-leon-furs` · King Leon, in furs · P2

**Basis:** Edits `armor-leon`. From `03-art-overhaul.md` §7.4 (Leon, "ill, in furs").

Edit the attached image (armor-leon.png, the armor on the base): change ONLY the robes: wrap him in a heavy fur-lined robe of navy and white with a thick fur collar and a woollen blanket round the shoulders, keeping the old sword at the hip. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-leon-furs.png.

#### `armor-brenna-protectorate` · Brenna, sworn to the Protector · P2

**Basis:** Edits `armor-brenna`. From `03-art-overhaul.md` §7.4 (an Act 3 Protectorate sash).

Edit the attached image (armor-brenna.png, the armor on the base): change ONLY one thing: add a violet sash bound across the body from the near shoulder to the far hip, fixed with a gold crown badge. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-brenna-protectorate.png.

### 5.6 Humanoid foes

#### `armor-hollowed-footman` · A Hollowed footman's body · P0

**Basis:** Edits `base-male`. From the art plan (ash skin, cracks lit ember, a dented cuirass, a rotted tabard).

Edit the attached base image: change everything below the neck into the body of a Hollowed footman, a dead soldier raised as an ash-grey husk: arms, hands and legs of dry ash-grey skin like burnt paper, cracked all over in fine lines glowing ember orange from inside; a dented, rust-streaked iron cuirass over rotting padded cloth; a ragged tabard of rotted brown-red cloth hanging in strips; rusted mail sleeves; wrapped legs and broken boots. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hollowed-footman.png.

#### `armor-hollowed-hexer` · A Hollowed hexer's body · P0

**Basis:** Edits `base-male`. From the art plan (a tattered robe).

Edit the attached base image: change everything below the neck into the body of a Hollowed spellcaster: long, bony ash-grey hands with faint ember-lit cracks, and a tattered, ash-stained robe in charcoal and faded brown to the ground, hanging in strips at the hem and sleeves; a cord belt with a bone-handled dagger and strings of small charms. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hollowed-hexer.png.

#### `armor-hollowed-sapper` · A Hollowed sapper's body · P0

**Basis:** Edits `base-male`. From the art plan (soot, a jerkin, charges, a lantern at the back of the belt).

Edit the attached base image: change everything below the neck into the body of a Hollowed tunneller: ash-grey, ember-cracked skin black with soot; a filthy leather jerkin over a torn shirt; a heavy belt with a pouch of round clay blasting charges with short fuses and a small iron lantern glowing orange at the back; wrapped legs and heavy broken boots; a coil of rope over one shoulder. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hollowed-sapper.png.

#### `armor-hollowed-chief` · The sapper chief's body · P0

**Basis:** Edits `base-male`. From the art plan (the sapper plus a bandolier of charges).

Edit the attached base image: change everything below the neck into the body of the Hollowed sappers' leader: ash-grey, ember-cracked skin; a heavy, scorched leather jerkin reinforced with iron plates; a bandolier of round clay blasting charges across the chest; a wide belt with tools; thick gauntlets; heavy boots. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hollowed-chief.png.

#### `armor-construct-stone` · A vault construct's stone body · P0

**Basis:** Edits `base-male`. From the art plan (dressed stone with gold inlay; joints that are dark gaps with gold light inside).

Edit the attached base image: change everything below the neck into the body of an ancient guardian statue built from blocks of pale, dressed sandstone with thin inlaid lines of gold running along their edges in geometric patterns: broad, blocky shoulders, a narrow stone waist, heavy legs and big three-fingered stone hands; every joint (shoulders, elbows, wrists, hips, knees) a dark gap with a warm gold light glowing inside; a few cracks and chips, moss in one crack. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-construct-stone.png.

#### `armor-hollowed-climber` · A Hollowed climber's body · P1

**Basis:** Edits `base-male`. **(derived)** From `01-build-plan.md` §9; `11-combat.md` (climbers).

Edit the attached base image: change everything below the neck into the body of a wiry Hollowed wall-climber: lean, ash-grey, ember-cracked limbs; ragged, close-bound strips of dark cloth; a rope with an iron grapnel coiled over the shoulder; iron climbing hooks strapped to both forearms; bare, clawed feet. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hollowed-climber.png.

#### `armor-cultist` · A cube cultist's robe · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (a black robe, a cube pendant).

Edit the attached base image: dress the body in a plain black robe to the ankle with a high collar, belted with black cord; a pendant of a small black iron cube on a chain at the chest; a pouch of black iron coins stamped with a cube; black wrapped boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-cultist.png.

#### `armor-bandit` · A bandit's leathers · P1

**Basis:** Edits `base-male`. **(derived)** From `08-random-encounters.md` §3 (bandits).

Edit the attached base image: dress the body in a scuffed leather jerkin; a mismatched leather bracer; dark trousers; worn boots; a dirty brown scarf round the neck; a sack of stolen goods at the belt. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-bandit.png.

#### `armor-thrall` · A thrall's tabard · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.3 (wine red for Morvaine's thralls).

Edit the attached base image: dress the body in a dark mail shirt under a wine-red tabard with a black thorn emblem; a high black wool collar; black leather gloves; dark trousers; black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-thrall.png.

#### `armor-thrall-knight` · A thrall-knight's black plate · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Vesk, Vreya, Mordain: black plate with red trim).

Edit the attached base image: dress the body in full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-thrall-knight.png.

#### `armor-thrall-knight-f` · A thrall-knight's black plate, on the woman's base · P2

**Basis:** Edits `base-female`. As `armor-thrall-knight`, fitted to the woman's base, for `vreya`, whose face is painted on it.

Edit the attached base image: dress the body in full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-thrall-knight-f.png.

#### `armor-gilded-knight` · A Gilded knight's armour · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.5 (the `order` pool: gilded plate and tabards).

Edit the attached base image: dress the body in steel plate with gold-painted edges (plainer than a commander's); a white tabard with a gold sun on the chest; armoured boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-gilded-knight.png.

#### `armor-drowned` · A drowned body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (grey-green, waterlogged clothes).

Edit the attached base image: change everything below the neck into the body of a drowned corpse: bloated, sagging, grey-green waterlogged skin wrinkled at the fingers, barnacles crusted on one shoulder; the soaked, torn remains of a fisherman's jersey and canvas trousers, dark with water and tangled in fishing net; one bare foot, one boot; water running off it. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-drowned.png.

#### `armor-blood-wight` · A blood-wight's body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (thin, grey, a red glow in the chest).

Edit the attached base image: change everything below the neck into the body of a terribly thin undead thing: all bones and taut ash-grey papery skin stretched over the ribs, with a deep red glow beating like a heart under the ribs; the rotted remains of a dark noble's coat hanging in strips; long fingers ending in black claws. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-blood-wight.png.

#### `armor-sun-scorched` · A sun-scorched body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (old priest's robes burned to the skin).

Edit the attached base image: change everything below the neck into the body of one of the burned dead: skin blackened and cracked like dried mud, the cracks glowing a faint hot gold; the remains of an old priest's white-and-gold robes burned onto the body, cloth and skin fused, the hems charred to nothing; a melted gold sun pendant fused to the chest; wisps of heat haze rising from the shoulders. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-sun-scorched.png.

#### `armor-thornling` · A thornling's bark body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (bark-skinned, thorn fingers).

Edit the attached base image: change everything below the neck into the body of a forest sprite made of living wood (the game's morph makes it child-sized): thin, knobbly limbs of rough dark bark with green moss in the cracks, long fingers ending in sharp black thorns, root-like feet, and a few tiny white flowers growing on one shoulder. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-thornling.png.

#### `armor-construct-priest` · A priest-construct's stone body · P2

**Basis:** Edits `base-male`. **(derived)** From `03b-world-art.md` (`priest-construct`).

Edit the attached base image: change everything below the neck into the body of a stone guardian carved as a robed priest: sun-bleached sandstone robes falling in straight folds to the feet, gold inlay worn thin by sand, joints that are dark gaps with a hot white-gold light inside, cracks and sand-scour, one shoulder broken off and held together with old bronze bands. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-construct-priest.png.

#### `armor-smuggler` · A smuggler's oilskin · P2

**Basis:** Edits `base-female`. **(derived)** From `08-random-encounters.md` §3 (smugglers).

Edit the attached base image: dress the body in a long, dark oilskin coat, salt-stained; a grey knitted jersey; a wide belt with hidden pouches; dark trousers; tall sea boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-smuggler.png.

#### `armor-protectorate` · The Protectorate's tabard · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.5 (the `protectorate` pool: violet-and-gold tabards over the Watch's kit).

Edit the attached base image: dress the body in the navy-blue Watch coat with brass buttons, and over it a violet tabard with a gold border and a gold stylised crown on the chest; a violet armband; a brown belt with a pouch of papers; navy trousers; black boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-protectorate.png.

#### `armor-protectorate-warden` · A Protectorate warden's plate · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the warden in heavy plate).

Edit the attached base image: dress the body in heavy steel plate with a violet enamel finish on the breastplate and pauldrons and gold edging, a stylised gold crown embossed on the chest; a violet surcoat skirt; heavy gauntlets and sabatons. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-protectorate-warden.png.

#### `armor-construct-crown` · A crown-construct's metal body · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (violet-gold metal).

Edit the attached base image: change everything below the neck into the body of a sleek war machine shaped like a knight: polished dark violet metal with bright gold inlay and gold edges, a narrow waist and broad shoulders, joints that are tight gold rings with violet light inside, and forearms that end in heavy gold-edged blades instead of hands. This replaces the base's body, skin included. The neck is part of this body, so change it to match; keep the bald head and its face above the jawline exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-construct-crown.png.

#### `armor-unmade-bronze` · Ancient bronze scale · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the Unmade: four armour variants); A3-C5-07 ("the armour of every age").

Edit the attached base image: dress the body in ancient bronze scale armour, bronze greaves, a short pleated kilt, sandals; the colours faded and slightly greyed, as if not quite here. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-unmade-bronze.png.

#### `armor-pikeman` · A pikeman's breastplate · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the Unmade).

Edit the attached base image: dress the body in a steel breastplate and tassets over a padded doublet with puffed sleeves, breeches and tall boots; the colours faded and slightly greyed, as if not quite here. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-pikeman.png.

#### `armor-ashfall-raider` · An Ashfall raider's kit · P2

**Basis:** Edits `base-male`. **(derived)** From `enc-brenna-raiders`; the `doomward` pool.

Edit the attached base image: dress the body in patched black-and-grey leather armour over a mail shirt; a violet strip of cloth tied round the near arm; wrapped boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-ashfall-raider.png.

#### `armor-maleficio-ascendant` · Maleficio, the Sovereign · P1

**Basis:** Edits `armor-maleficio`. From `04-script-prologue-act1.md` A1-C8-02 (the Sovereign phase).

Edit the attached image (armor-maleficio.png, the armor on the base): change ONLY the robes' state: lift them and the high collar as if in a hot updraught, set every ember-orange edge burning bright with sparks streaming upward, and run fine ember-orange cracks of light across the hands. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-maleficio-ascendant.png.

#### `armor-varkhul-split` · Varkhul, the armour split open · P1

**Basis:** Edits `armor-varkhul`. From `04-script-prologue-act1.md` A1-C6-01 ("The armour splits along its seams").

Edit the attached image (armor-varkhul.png, the armor on the base): change ONLY the armour's state: it has split open along its seams and fallen away from the chest, the ember light gone out of it, showing a plain, stained padded jack underneath. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-varkhul-split.png.

#### `armor-crown-hollowed` · Crown-raised Hollowed body · P2

**Basis:** Edits `armor-hollowed-footman`. From `03-art-overhaul.md` §9 (Hollowed in Protectorate sashes, violet eye light).

Edit the attached image (armor-hollowed-footman.png, the armor on the base): change ONLY two things: the glowing cracks in the skin are violet instead of ember orange, and a violet sash with a gold crown badge is tied across the body over the old rotted tabard. Keep the bald head and its face exactly as they are, and keep everything else, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-crown-hollowed.png.

### 5.7 Items

#### `armor-padded` · Padded armour · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`padded-armour`); the art plan's §1.8 (cloth: quilting diamonds; matte).

Edit the attached base image: dress the body in a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-padded.png.

#### `armor-leather` · Leather armour · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`leather-armour`); the art plan's §1.8 (stiff rounded panels, stitching, straps; one wide highlight).

Edit the attached base image: dress the body in boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-leather.png.

#### `armor-studded` · Studded leather · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`studded-leather`).

Edit the attached base image: dress the body in the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-studded.png.

#### `armor-hide` · Hide armour · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`hide-armour`); the art plan's §1.8 (bulky, ragged edges, a fur collar, X-lacing).

Edit the attached base image: dress the body in bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hide.png.

#### `armor-chain-shirt` · Chain shirt · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`chain-shirt`); the art plan's §1.8 (sags, a scalloped hem).

Edit the attached base image: dress the body in a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-chain-shirt.png.

#### `armor-chain-mail` · Chain mail · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`chain-mail`).

Edit the attached base image: dress the body in a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-chain-mail.png.

#### `armor-scale` · Scale mail · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`scale-mail`); the art plan's §1.8 (scales in half-offset rows).

Edit the attached base image: dress the body in a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-scale.png.

#### `armor-plate` · Plate armour · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`plate-armour`); the art plan's §1.8 (rolled rims, rivets, lames; a reflection of the sky).

Edit the attached base image: dress the body in full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-plate.png.

#### `armor-mage-robes` · Mage robes · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`mage-robes`); the art plan's §1.8 (an A-line to the ankle, wide sleeves, trim bands).

Edit the attached base image: dress the body in a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-mage-robes.png.

#### `armor-padded-f` · Padded armour, woman's base · P0

**Basis:** Edits `base-female`. As `armor-padded`.

Edit the attached base image: dress the body in a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-padded-f.png.

#### `armor-leather-f` · Leather armour, woman's base · P0

**Basis:** Edits `base-female`. As `armor-leather`.

Edit the attached base image: dress the body in boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-leather-f.png.

#### `armor-studded-f` · Studded leather, woman's base · P0

**Basis:** Edits `base-female`. As `armor-studded`.

Edit the attached base image: dress the body in the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-studded-f.png.

#### `armor-hide-f` · Hide armour, woman's base · P0

**Basis:** Edits `base-female`. As `armor-hide`.

Edit the attached base image: dress the body in bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-hide-f.png.

#### `armor-chain-shirt-f` · Chain shirt, woman's base · P0

**Basis:** Edits `base-female`. As `armor-chain-shirt`.

Edit the attached base image: dress the body in a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-chain-shirt-f.png.

#### `armor-chain-mail-f` · Chain mail, woman's base · P0

**Basis:** Edits `base-female`. As `armor-chain-mail`.

Edit the attached base image: dress the body in a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-chain-mail-f.png.

#### `armor-scale-f` · Scale mail, woman's base · P0

**Basis:** Edits `base-female`. As `armor-scale`.

Edit the attached base image: dress the body in a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-scale-f.png.

#### `armor-plate-f` · Plate armour, woman's base · P0

**Basis:** Edits `base-female`. As `armor-plate`.

Edit the attached base image: dress the body in full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-plate-f.png.

#### `armor-mage-robes-f` · Mage robes, woman's base · P0

**Basis:** Edits `base-female`. As `armor-mage-robes`.

Edit the attached base image: dress the body in a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes. Any bare skin (hands, arms, neck, feet) keeps the base's warm tan skin tone; the game tints it to the wearer's skin. Keep the bald head and its face exactly as they are, and change nothing else: keep the body's proportions under the clothes, and the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as armor-mage-robes-f.png.

---

## 6. Capes, helms and weapons

The game's cloaks, helms and weapons (`Items/ItemCatalog.cs`) and the hats in `Characters/LookOptions.cs`, plus every character's signature pieces. Shields are drawn on the far forearm; a character's own sword-and-shield or pair of blades is one piece.

### 6.0 Sheets (four at a time)

The owner's call (2026-11-08): the capes, helms and weapons four at a time, like the hair and the armors. Every card in §6.1 to §6.3 that's painted on a base is in one sheet here (29 sheets for the 107 P0 parts, 8 for P1's 25, 13 for P2's 38), each one ChatGPT picture. The attached template (`tools/source/characters/<kind>-sheet-male.png` or `-female.png`, made by `python tools/part_sheet.py templates`) is four copies of the base: a helm sheet's are the blank base from the top of the canvas to the hips, at full size, with room for a crown; a cape sheet's and a weapon sheet's are the whole figure, a little smaller than an armor sheet's, with room behind the back for a cloak and over the head for a staff. `python tools/part_sheet.py cut <png> <sheet-id>` puts each cell back on its base as `tools/source/characters/helms/`, `capes/` or `weapons/<id>.png`. A sheet holds one priority, so the opening chapters' parts are done first; the cards that edit another part (`weapon-black-box`, `weapon-brannock-1`, `weapon-brannock-41`, `weapon-brannock-42`) stay single, as do any redos.

Each sheet's prompt is its kind's sheet block (§2.3) and the list below it.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `cape-sheet-m01` | `base-male` | P0 | `cape-tail-ember`, `cape-watch`, `cape-watch-captain`, `cape-royal` |
| `cape-sheet-m02` | `base-male` | P0 | `cape-leon`, `cape-travelling`, `cape-blanket`, `cape-short-lavender` |
| `cape-sheet-m03` | `base-male` | P1 | `cape-protection`, `cape-campaign`, `cape-varkhul` |
| `cape-sheet-m04` | `base-male` | P2 | `cape-severin`, `cape-severin-wings`, `cape-thrall-knight`, `cape-corwin` |
| `cape-sheet-m05` | `base-male` | P2 | `cape-gilded` |
| `cape-sheet-f01` | `base-female` | P0 | `cape-sb`, `cape-sera`, `cape-tail-wolf`, `cape-tail-hawk` |
| `cape-sheet-f02` | `base-female` | P0 | `cape-tail-bear`, `cape-elara-hooded`, `cape-tamsin`, `cape-old-woman` |
| `cape-sheet-f03` | `base-female` | P1 | `cape-ash`, `cape-pedlar-pack` |
| `cape-sheet-f04` | `base-female` | P2 | `cape-valencia` |
| `helm-sheet-m01` | `base-male` | P0 | `helm-watch`, `helm-watch-crest`, `helm-royal-guard`, `helm-crown` |
| `helm-sheet-m02` | `base-male` | P0 | `helm-kael-hood`, `helm-beret`, `helm-knit-cap`, `helm-bandana-red` |
| `helm-sheet-m03` | `base-male` | P0 | `helm-bandana`, `helm-hollowed-kettle`, `helm-hollowed-hood`, `helm-sapper-goggles` |
| `helm-sheet-m04` | `base-male` | P0 | `helm-sapper-faceplate`, `helm-straw-hat`, `helm-flat-cap`, `helm-leather-cap` |
| `helm-sheet-m05` | `base-male` | P0 | `helm-iron-helm` |
| `helm-sheet-m06` | `base-male` | P1 | `helm-kettle`, `helm-varkhul`, `helm-nasal`, `helm-bandit-hood` |
| `helm-sheet-m07` | `base-male` | P2 | `helm-corwin`, `helm-gilded`, `helm-crown-thorns`, `helm-vesk-wolf` |
| `helm-sheet-m08` | `base-male` | P2 | `helm-mordain-beak`, `helm-vorlaith`, `helm-warden`, `helm-bronze-crest` |
| `helm-sheet-m09` | `base-male` | P2 | `helm-morion` |
| `helm-sheet-f01` | `base-female` | P0 | `helm-tricorn`, `helm-circlet`, `helm-hood-charcoal`, `helm-wide-brim` |
| `helm-sheet-f02` | `base-female` | P0 | `helm-kerchief`, `helm-circlet-of-focus` |
| `helm-sheet-f03` | `base-female` | P1 | `helm-lace-cap` |
| `helm-sheet-f04` | `base-female` | P2 | `helm-abbey-hood`, `helm-bee-veil`, `helm-circlet-silver`, `helm-vreya-crown` |
| `helm-sheet-f05` | `base-female` | P2 | `helm-crown-dusk-dawn` |
| `weapon-sheet-m01` | `base-male` | P0 | `weapon-dagger`, `weapon-shortsword`, `weapon-longsword`, `weapon-rapier` |
| `weapon-sheet-m02` | `base-male` | P0 | `weapon-scimitar`, `weapon-handaxe`, `weapon-battleaxe`, `weapon-greatsword` |
| `weapon-sheet-m03` | `base-male` | P0 | `weapon-warhammer`, `weapon-mace`, `weapon-quarterstaff`, `weapon-spear` |
| `weapon-sheet-m04` | `base-male` | P0 | `weapon-shortbow`, `weapon-longbow`, `weapon-light-crossbow`, `weapon-ember-dagger` |
| `weapon-sheet-m05` | `base-male` | P0 | `offhand-wooden-shield`, `offhand-steel-shield`, `offhand-warden-shield`, `offhand-dagger` |
| `weapon-sheet-m06` | `base-male` | P0 | `offhand-shortsword`, `offhand-scimitar`, `offhand-handaxe`, `offhand-ember-dagger` |
| `weapon-sheet-m07` | `base-male` | P0 | `weapon-kael-knives`, `weapon-kael-rapier`, `weapon-brannock`, `weapon-cutlass` |
| `weapon-sheet-m08` | `base-male` | P0 | `weapon-gaff`, `weapon-cudgel`, `weapon-watch-kit`, `weapon-halberd` |
| `weapon-sheet-m09` | `base-male` | P0 | `weapon-telescope`, `weapon-tack-hammer`, `weapon-walking-stick`, `weapon-tin-cup` |
| `weapon-sheet-m10` | `base-male` | P0 | `weapon-quill-letter`, `weapon-letter`, `weapon-letters`, `weapon-stew-pot` |
| `weapon-sheet-m11` | `base-male` | P0 | `weapon-silk-bolt`, `weapon-lute`, `weapon-hollowed-sword`, `weapon-hexer-staff` |
| `weapon-sheet-m12` | `base-male` | P0 | `weapon-sapper-pick`, `weapon-chief-pick-lantern`, `weapon-toy-sword`, `weapon-pitchfork` |
| `weapon-sheet-m13` | `base-male` | P0 | `weapon-crate`, `weapon-hammer-tongs` |
| `weapon-sheet-m14` | `base-male` | P1 | `weapon-thornwood-bow`, `weapon-sunsteel-blade`, `weapon-mallet`, `weapon-spade` |
| `weapon-sheet-m15` | `base-male` | P1 | `weapon-heron-cane`, `weapon-felling-axe`, `weapon-oil-lamp`, `weapon-varkhul-greatsword` |
| `weapon-sheet-m16` | `base-male` | P1 | `weapon-cultist-dagger`, `weapon-climber-hook`, `weapon-relief-kit` |
| `weapon-sheet-m17` | `base-male` | P2 | `weapon-corwin-kit`, `weapon-corwin-sword`, `weapon-gilded-kit`, `weapon-mace-shield` |
| `weapon-sheet-m18` | `base-male` | P2 | `weapon-spear-lantern`, `weapon-vesk-sword`, `weapon-mordain-glaive`, `weapon-unmade-spear-shield` |
| `weapon-sheet-m19` | `base-male` | P2 | `weapon-pike`, `weapon-warden-kit`, `weapon-protectorate-kit`, `weapon-raider-kit` |
| `weapon-sheet-f01` | `base-female` | P0 | `weapon-sb-blades`, `weapon-sera-longbow`, `weapon-netting-needle`, `weapon-loaf` |
| `weapon-sheet-f02` | `base-female` | P0 | `weapon-rag-doll`, `weapon-tankard`, `weapon-scrolls`, `weapon-broom` |
| `weapon-sheet-f03` | `base-female` | P0 | `weapon-coin`, `weapon-practice-sword`, `weapon-flat-cap-held`, `weapon-basket` |
| `weapon-sheet-f04` | `base-female` | P0 | `weapon-flower-basket`, `weapon-pilgrim-staff`, `weapon-peel`, `weapon-spindle` |
| `weapon-sheet-f05` | `base-female` | P0 | `weapon-coin-purse`, `weapon-feather-duster` |
| `weapon-sheet-f06` | `base-female` | P1 | `weapon-staff-ring`, `weapon-ledger-pen`, `weapon-folded-shawl`, `weapon-sketchbook` |
| `weapon-sheet-f07` | `base-female` | P2 | `weapon-lantern`, `weapon-smoker`, `weapon-night-flowers`, `weapon-vreya-blade` |
| `weapon-sheet-f08` | `base-female` | P2 | `weapon-glass-lamp`, `weapon-map` |

#### `cape-sheet-m01` · Cape sheet: 4 capes (An Emberkin's ember tail, The Watch's short cape, The Watch captain's cloak, The palace's ochre cloak) · P0

**Basis:** Edits `cape-sheet-male`. Cells: 1 `cape-tail-ember`, 2 `cape-watch`, 3 `cape-watch-captain`, 4 `cape-royal`.

1. An Emberkin's ember tail: A thin crimson tail growing from the small of the back, curving down and back to about the knee, its tip glowing like a live ember, red through orange to yellow.
2. The Watch's short cape: A short navy-blue cape from the shoulders to the middle of the back, fastened with a small brass clasp.
3. The Watch captain's cloak: A navy-blue cloak to the knee, fastened at one shoulder with a brass clasp.
4. The palace's ochre cloak: An ochre-gold cloak from the shoulders to the knee, with a white lining.
I'll save it as cape-sheet-m01.png.

#### `cape-sheet-m02` · Cape sheet: 4 capes (King Leon's cloak, Travelling cloak, A blanket worn as a cloak, A short lavender cape) · P0

**Basis:** Edits `cape-sheet-male`. Cells: 1 `cape-leon`, 2 `cape-travelling`, 3 `cape-blanket`, 4 `cape-short-lavender`.

1. King Leon's cloak: A long navy-blue cloak from the shoulders to the floor, heavy and plain.
2. Travelling cloak: A heavy dark-green wool travelling cloak to the calf, its hood down, a simple iron clasp, a little frayed at the hem.
3. A blanket worn as a cloak: A worn charcoal wool blanket round the shoulders, tied at the chest, hanging to the backs of the knees.
4. A short lavender cape: A short lavender cape over the shoulders, reaching the middle of the back, with a small silver clasp.
I'll save it as cape-sheet-m02.png.

#### `cape-sheet-m03` · Cape sheet: 3 capes (Cloak of protection, Aldric's campaign cloak, Varkhul's tattered cloak) · P1

**Basis:** Edits `cape-sheet-male`. Cells: 1 `cape-protection`, 2 `cape-campaign`, 3 `cape-varkhul`.

1. Cloak of protection: A fine cloak to the calf in deep blue-grey, with a subtle pattern of small silver stars woven into it, a silver clasp and a faint shimmer along its edge.
2. Aldric's campaign cloak: An old campaign cloak in faded brick red, patched and singed at the hem, to the knee.
3. Varkhul's tattered cloak: A tattered ash-grey cloak hanging in strips, scorched at the edges.
Leave figure 4 exactly as it is, with nothing added.
I'll save it as cape-sheet-m03.png.

#### `cape-sheet-m04` · Cape sheet: 4 capes (Severin's velvet cape, Severin's cape, spread like wings, A thrall-knight's cape, Sir Corwin's white cloak) · P2

**Basis:** Edits `cape-sheet-male`. Cells: 1 `cape-severin`, 2 `cape-severin-wings`, 3 `cape-thrall-knight`, 4 `cape-corwin`.

1. Severin's velvet cape: A long black velvet cape lined in wine red, hanging from the shoulders to the ground.
2. Severin's cape, spread like wings: The same long black velvet cape lined in wine red, spread out wide on either side like the wings of a huge bat, its edges ragged and stretched into points.
3. A thrall-knight's cape: A black cape lined in wine red, to the calf, ragged at the hem.
4. Sir Corwin's white cloak: A white cloak edged in gold, to the calf.
I'll save it as cape-sheet-m04.png.

#### `cape-sheet-m05` · Cape sheet: 1 cape (A Gilded knight's cape) · P2

**Basis:** Edits `cape-sheet-male`. Cells: 1 `cape-gilded`.

1. A Gilded knight's cape: A short white cape to the middle of the back, edged with a thin gold line.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as cape-sheet-m05.png.

#### `cape-sheet-f01` · Cape sheet: 4 capes (The Soul-Bound's cape, Sera's shoulder cape, A Wildkin wolf's tail, A Wildkin hawk's tail feathers) · P0

**Basis:** Edits `cape-sheet-female`. Cells: 1 `cape-sb`, 2 `cape-sera`, 3 `cape-tail-wolf`, 4 `cape-tail-hawk`.

1. The Soul-Bound's cape: A long charcoal cape that hangs from the shoulders to the calves, plain, with a little wear at the hem.
2. Sera's shoulder cape: A short dark-moss-green wool shoulder cape with its hood down, fastened at the collar with a wooden toggle, reaching the middle of the back.
3. A Wildkin wolf's tail: A bushy grey-brown wolf's tail growing from the small of the back, hanging down and back to below the knee.
4. A Wildkin hawk's tail feathers: A short fan of barred brown-and-cream tail feathers growing from the small of the back, angled down and back.
I'll save it as cape-sheet-f01.png.

#### `cape-sheet-f02` · Cape sheet: 4 capes (A Wildkin bear's tail, A plain charcoal cloak, The Lodge's teal cloak and hawk feathers, An old woman's rose cape) · P0

**Basis:** Edits `cape-sheet-female`. Cells: 1 `cape-tail-bear`, 2 `cape-elara-hooded`, 3 `cape-tamsin`, 4 `cape-old-woman`.

1. A Wildkin bear's tail: A short, round, furry black bear's tail at the small of the back.
2. A plain charcoal cloak: A plain charcoal wool cloak to the calf, hanging from the shoulders, no ornament at all.
3. The Lodge's teal cloak and hawk feathers: A short teal ranger's cloak to the middle of the back, and below its hem a short fan of barred brown-and-cream tail feathers growing from the small of the back.
4. An old woman's rose cape: A short hooded cape in a deep, dusty rose (not wine red), the hood down, to the middle of the back.
I'll save it as cape-sheet-f02.png.

#### `cape-sheet-f03` · Cape sheet: 2 capes (An ash-grey hooded cloak, A pedlar's pack frame) · P1

**Basis:** Edits `cape-sheet-female`. Cells: 1 `cape-ash`, 2 `cape-pedlar-pack`.

1. An ash-grey hooded cloak: A long ash-grey cloak with its hood down, ragged and singed at the hem, to the calf.
2. A pedlar's pack frame: A wooden pack frame on the back, strapped over the shoulders and loaded high with coils of rope, pots, a lantern and cloth bundles.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as cape-sheet-f03.png.

#### `cape-sheet-f04` · Cape sheet: 1 cape (Valencia's cape) · P2

**Basis:** Edits `cape-sheet-female`. Cells: 1 `cape-valencia`.

1. Valencia's cape: A long cape in charcoal-black, lined with wine red, falling from the shoulders to the ground.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as cape-sheet-f04.png.

#### `helm-sheet-m01` · Helm sheet: 4 helms (The Watch helm, The Watch captain's crested helm, The Royal Guard's helm, A small gold crown) · P0

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-watch`, 2 `helm-watch-crest`, 3 `helm-royal-guard`, 4 `helm-crown`.

1. The Watch helm: An open-faced steel helm with a brass rim and no crest.
2. The Watch captain's crested helm: An open-faced steel helm with a brass rim and a short, stiff crest of navy horsehair running from front to back.
3. The Royal Guard's helm: A polished steel helm with a gold-trimmed brim and a short navy plume.
4. A small gold crown: A small, plain gold crown, a simple band with low points and one ruby at the front.
I'll save it as helm-sheet-m01.png.

#### `helm-sheet-m02` · Helm sheet: 4 helms (Kael's pointed hood, A beret, A knitted cap, A Red Gull bandana) · P0

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-kael-hood`, 2 `helm-beret`, 3 `helm-knit-cap`, 4 `helm-bandana-red`.

1. Kael's pointed hood: A charcoal hood worn up, ending in a soft point at the back, the edge of the face in shadow (the game draws the horns over it).
2. A beret: A soft navy beret worn tilted, with a short white feather.
3. A knitted cap: A dark-blue knitted fisherman's cap, snug and rolled at the brim.
4. A Red Gull bandana: A raspberry-red bandana tied over the head, knotted at the back with the ends hanging.
I'll save it as helm-sheet-m02.png.

#### `helm-sheet-m03` · Helm sheet: 4 helms (A bandana, A rusted kettle helm, A hexer's deep hood, A sapper's cap and goggles) · P0

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-bandana`, 2 `helm-hollowed-kettle`, 3 `helm-hollowed-hood`, 4 `helm-sapper-goggles`.

1. A bandana: A dark grey bandana tied over the head, knotted at the back with the ends hanging.
2. A rusted kettle helm: A rusted, dented iron kettle helm with a wide brim, sitting low.
3. A hexer's deep hood: A deep, ragged, ash-stained hood worn up and pulled far forward, hiding the face except the lower jaw.
4. A sapper's cap and goggles: A battered leather cap with ear flaps and a pair of cracked brass goggles pushed up on it.
I'll save it as helm-sheet-m03.png.

#### `helm-sheet-m04` · Helm sheet: 4 helms (A cracked iron faceplate, A straw hat, A flat cap, Leather cap) · P0

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-sapper-faceplate`, 2 `helm-straw-hat`, 3 `helm-flat-cap`, 4 `helm-leather-cap`.

1. A cracked iron faceplate: A cracked iron faceplate riveted to a leather hood, covering the whole face, with ember-orange light glowing through its eye slits and cracks.
2. A straw hat: A battered, wide straw hat.
3. A flat cap: A slate-blue flat cap.
4. Leather cap: A padded brown leather cap with ear flaps, stitched in panels.
I'll save it as helm-sheet-m04.png.

#### `helm-sheet-m05` · Helm sheet: 1 helm (Iron helm) · P0

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-iron-helm`.

1. Iron helm: A closed iron helm with a narrow visor slit and a riveted brow band, hiding the whole head.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as helm-sheet-m05.png.

#### `helm-sheet-m06` · Helm sheet: 4 helms (A kettle helm, Varkhul's cracked helm, An old-pattern nasal helm, A bandit's hood and scarf) · P1

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-kettle`, 2 `helm-varkhul`, 3 `helm-nasal`, 4 `helm-bandit-hood`.

1. A kettle helm: A plain steel kettle helm with a wide brim, a size too big, tipped back.
2. Varkhul's cracked helm: A closed great helm of blackened iron, the visor cracked down the middle, and through the crack a single burning ember-orange eye.
3. An old-pattern nasal helm: An old-fashioned round-topped steel helm with a nose guard.
4. A bandit's hood and scarf: A ragged brown hood worn up, and a dirty brown scarf pulled up over the nose.
I'll save it as helm-sheet-m06.png.

#### `helm-sheet-m07` · Helm sheet: 4 helms (Sir Corwin's plumed helm, A Gilded knight's helm, The crown of thorns, Vesk's wolf helm) · P2

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-corwin`, 2 `helm-gilded`, 3 `helm-crown-thorns`, 4 `helm-vesk-wolf`.

1. Sir Corwin's plumed helm: An open-faced gilded helm with a tall white plume rising from the crest.
2. A Gilded knight's helm: An open-faced steel helm with a narrow gold band and no plume.
3. The crown of thorns: A circlet of black iron wrought into thorns.
4. Vesk's wolf helm: A closed black helm with thin wine-red trim, shaped like a snarling wolf's head, the snout forming the visor, two red points of light in the eye slits.
I'll save it as helm-sheet-m07.png.

#### `helm-sheet-m08` · Helm sheet: 4 helms (Mordain's beaked helm, Vorlaith's crown of clock-hands, A Protectorate warden's great helm, A crested bronze helm) · P2

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-mordain-beak`, 2 `helm-vorlaith`, 3 `helm-warden`, 4 `helm-bronze-crest`.

1. Mordain's beaked helm: A closed black helm with thin wine-red trim, drawn out into a long, curved beak like a carrion bird's, with round red-glass eyepieces.
2. Vorlaith's crown of clock-hands: A crown made of broken clock-hands of old, dark iron and brass, of different sizes, pointing every way.
3. A Protectorate warden's great helm: A closed great helm of steel with a violet enamel finish, a narrow visor slit and a gold crest ridge.
4. A crested bronze helm: An ancient bronze helm with cheek guards and a tall horsehair crest, faded and slightly greyed.
I'll save it as helm-sheet-m08.png.

#### `helm-sheet-m09` · Helm sheet: 1 helm (A crested open helm) · P2

**Basis:** Edits `helm-sheet-male`. Cells: 1 `helm-morion`.

1. A crested open helm: A steel open helm with a high comb crest and an upturned brim, faded and slightly greyed.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as helm-sheet-m09.png.

#### `helm-sheet-f01` · Helm sheet: 4 helms (A tricorn hat, A gold circlet, A charcoal hood, A wide-brimmed hat) · P0

**Basis:** Edits `helm-sheet-female`. Cells: 1 `helm-tricorn`, 2 `helm-circlet`, 3 `helm-hood-charcoal`, 4 `helm-wide-brim`.

1. A tricorn hat: A weathered black tricorn hat with a thin gold trim.
2. A gold circlet: A slim gold circlet with a small sapphire at the brow.
3. A charcoal hood: A deep charcoal wool hood worn up and pulled forward, hiding the whole head and putting the upper face in shadow.
4. A wide-brimmed hat: A wide-brimmed tan felt hat with a band of braided cord.
I'll save it as helm-sheet-f01.png.

#### `helm-sheet-f02` · Helm sheet: 2 helms (A kerchief, Circlet of focus) · P0

**Basis:** Edits `helm-sheet-female`. Cells: 1 `helm-kerchief`, 2 `helm-circlet-of-focus`.

1. A kerchief: A cream kerchief tied over the hair, knotted at the nape.
2. Circlet of focus: A slim silver circlet with a small blue stone at the brow that gives off a faint glow.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as helm-sheet-f02.png.

#### `helm-sheet-f03` · Helm sheet: 1 helm (A lace cap) · P1

**Basis:** Edits `helm-sheet-female`. Cells: 1 `helm-lace-cap`.

1. A lace cap: A small white lace cap pinned on top of the head.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as helm-sheet-f03.png.

#### `helm-sheet-f04` · Helm sheet: 4 helms (The abbey's square hood, A bee veil, A silver circlet, Vreya's crowned helm) · P2

**Basis:** Edits `helm-sheet-female`. Cells: 1 `helm-abbey-hood`, 2 `helm-bee-veil`, 3 `helm-circlet-silver`, 4 `helm-vreya-crown`.

1. The abbey's square hood: A stiff, square-cornered grey hood worn up, with a white linen band inside it, hiding all the hair and shading the eyes.
2. A bee veil: A wide-brimmed straw hat with a fine honey-coloured mesh bee veil, pinned up at the front so the face shows and hanging down at the back and sides.
3. A silver circlet: A slender, simple silver circlet with a tiny dark-red stone.
4. Vreya's crowned helm: An open-faced black helm with thin wine-red trim, crowned with a ring of tall, sharp points, framing the face.
I'll save it as helm-sheet-f04.png.

#### `helm-sheet-f05` · Helm sheet: 1 helm (The crown of dusk and dawn) · P2

**Basis:** Edits `helm-sheet-female`. Cells: 1 `helm-crown-dusk-dawn`.

1. The crown of dusk and dawn: A crown of black iron braided with strands of gold glass, a jagged black crystal splinter set at the front glowing violet.
Leave figures 2, 3 and 4 exactly as they are, with nothing added.
I'll save it as helm-sheet-f05.png.

#### `weapon-sheet-m01` · Weapon sheet: 4 weapons (Dagger, Shortsword, Longsword, Rapier) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-dagger`, 2 `weapon-shortsword`, 3 `weapon-longsword`, 4 `weapon-rapier`.

1. Dagger: In the near hand, a plain steel dagger with a leather-wrapped grip, held low, point forward.
2. Shortsword: In the near hand, a plain steel shortsword with a simple crossguard and a leather grip, held low, point forward.
3. Longsword: In the near hand, a plain steel longsword with a straight crossguard and a leather-wrapped grip, held low, point forward.
4. Rapier: In the near hand, a slim rapier with a swept steel hilt, held low, point forward.
I'll save it as weapon-sheet-m01.png.

#### `weapon-sheet-m02` · Weapon sheet: 4 weapons (Scimitar, Hand axe, Battleaxe, Greatsword) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-scimitar`, 2 `weapon-handaxe`, 3 `weapon-battleaxe`, 4 `weapon-greatsword`.

1. Scimitar: In the near hand, a curved steel scimitar with a brass guard, held low.
2. Hand axe: In the near hand, a short hand axe with a steel head and a wooden haft, held low.
3. Battleaxe: In the near hand, a long-hafted bearded battleaxe with a heavy steel head, held across the body.
4. Greatsword: A two-handed steel greatsword held in both hands in front of the body, point down near the ground.
I'll save it as weapon-sheet-m02.png.

#### `weapon-sheet-m03` · Weapon sheet: 4 weapons (Warhammer, Mace, Quarterstaff, Spear) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-warhammer`, 2 `weapon-mace`, 3 `weapon-quarterstaff`, 4 `weapon-spear`.

1. Warhammer: In the near hand, a warhammer with a square steel head and a back spike on a wooden haft, held low.
2. Mace: In the near hand, a flanged steel mace with a leather grip, held low.
3. Quarterstaff: In the near hand, a plain ash-wood quarterstaff, a little taller than a person, held upright with its foot on the ground.
4. Spear: In the near hand, a plain spear with a leaf-shaped steel head, held upright with its butt on the ground.
I'll save it as weapon-sheet-m03.png.

#### `weapon-sheet-m04` · Weapon sheet: 4 weapons (Shortbow, Longbow, Light crossbow, Ember dagger) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-shortbow`, 2 `weapon-longbow`, 3 `weapon-light-crossbow`, 4 `weapon-ember-dagger`.

1. Shortbow: In the near hand, a strung recurved shortbow, held low and vertical by its grip.
2. Longbow: In the near hand, a strung plain wooden longbow, taller than a person's shoulder, held upright by its grip.
3. Light crossbow: In the near hand, a small crossbow of wood and steel, held by its grip and pointed at the ground.
4. Ember dagger: In the near hand, a dagger with a blade of dark metal that glows orange along its edge like a coal, held low.
I'll save it as weapon-sheet-m04.png.

#### `weapon-sheet-m05` · Weapon sheet: 4 weapons (Wooden shield, Steel shield, Warden's shield, Dagger, in the off hand) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `offhand-wooden-shield`, 2 `offhand-steel-shield`, 3 `offhand-warden-shield`, 4 `offhand-dagger`.

1. Wooden shield: On the FAR arm only (the one on the right): strap to the outside of it, the hand through the grip, a round wooden shield with an iron rim and a central iron boss, scuffed; the near hand stays empty.
2. Steel shield: On the FAR arm only (the one on the right): strap to the outside of it, the hand through the grip, a steel heater shield with a rolled rim; the near hand stays empty.
3. Warden's shield: On the FAR arm only (the one on the right): strap to the outside of it, the hand through the grip, a tall steel kite shield with a brass rim and an embossed oak-leaf emblem; the near hand stays empty.
4. Dagger, in the off hand: In the FAR hand only (the one on the right), a plain steel dagger with a leather-wrapped grip, held low, point forward; the near hand stays empty.
I'll save it as weapon-sheet-m05.png.

#### `weapon-sheet-m06` · Weapon sheet: 4 weapons (Shortsword, in the off hand, Scimitar, in the off hand, Hand axe, in the off hand, Ember dagger, in the off hand) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `offhand-shortsword`, 2 `offhand-scimitar`, 3 `offhand-handaxe`, 4 `offhand-ember-dagger`.

1. Shortsword, in the off hand: In the FAR hand only (the one on the right), a plain steel shortsword with a simple crossguard and a leather grip, held low, point forward; the near hand stays empty.
2. Scimitar, in the off hand: In the FAR hand only (the one on the right), a curved steel scimitar with a brass guard, held low; the near hand stays empty.
3. Hand axe, in the off hand: In the FAR hand only (the one on the right), a short hand axe with a steel head and a wooden haft, held low; the near hand stays empty.
4. Ember dagger, in the off hand: In the FAR hand only (the one on the right), a dagger with a blade of dark metal that glows orange along its edge like a coal, held low; the near hand stays empty.
I'll save it as weapon-sheet-m06.png.

#### `weapon-sheet-m07` · Weapon sheet: 4 weapons (Kael's twin knives, Kael's rapier and parrying dagger, Brannock's sword and shield, Cutlass) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-kael-knives`, 2 `weapon-kael-rapier`, 3 `weapon-brannock`, 4 `weapon-cutlass`.

1. Kael's twin knives: A pair of long knives with red cord wound round their grips, one in each hand, the near one held reversed.
2. Kael's rapier and parrying dagger: A slim rapier with a swept hilt in the near hand, extended forward in a fencer's guard, and a parrying dagger held reversed in the far hand.
3. Brannock's sword and shield: A plain, heavy longsword in the near hand, held low, and a battered round wooden shield with an iron rim strapped to the far forearm, its rim still bare.
4. Cutlass: In the near hand, a notched cutlass with a brass basket guard, held casually, point down.
I'll save it as weapon-sheet-m07.png.

#### `weapon-sheet-m08` · Weapon sheet: 4 weapons (A fisherman's gaff, Cudgel, The Watch's spear and shield, Halberd) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-gaff`, 2 `weapon-cudgel`, 3 `weapon-watch-kit`, 4 `weapon-halberd`.

1. A fisherman's gaff: In the near hand, a long wooden gaff with an iron hook at the end, held upright like a walking staff.
2. Cudgel: In the near hand, a knotted oak cudgel bound with iron bands, held low.
3. The Watch's spear and shield: A plain spear held upright in the near hand, and a round wooden shield with a brass boss on the far forearm.
4. Halberd: In the near hand, a tall halberd with a polished blade, held upright, its butt on the ground.
I'll save it as weapon-sheet-m08.png.

#### `weapon-sheet-m09` · Weapon sheet: 4 weapons (A brass telescope, A tack hammer and a notice, A walking stick, A tin cup) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-telescope`, 2 `weapon-tack-hammer`, 3 `weapon-walking-stick`, 4 `weapon-tin-cup`.

1. A brass telescope: In the near hand, a brass telescope, half extended.
2. A tack hammer and a notice: A small tack hammer in the near hand, and a fresh notice on a clipboard in the far hand, its writing only scribbled lines.
3. A walking stick: In the near hand, a crooked wooden walking stick, its foot on the ground, taking the weight.
4. A tin cup: In the near hand, a dented tin cup of water.
I'll save it as weapon-sheet-m09.png.

#### `weapon-sheet-m10` · Weapon sheet: 4 weapons (A quill and a sealed letter, A sealed letter, A bundle of letters, A stew pot) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-quill-letter`, 2 `weapon-letter`, 3 `weapon-letters`, 4 `weapon-stew-pot`.

1. A quill and a sealed letter: A quill pen in the near hand, and a folded letter with a dark wax seal in the far hand.
2. A sealed letter: In the near hand, a rolled letter with a gold wax seal and a ribbon.
3. A bundle of letters: In the near hand, a bundle of sealed letters tied with string.
4. A stew pot: A steaming iron stew pot carried in front with both hands, a cloth wrapped round the handle.
I'll save it as weapon-sheet-m10.png.

#### `weapon-sheet-m11` · Weapon sheet: 4 weapons (A bolt of silk, A lute, A rusted longsword, A hexer's crystal staff) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-silk-bolt`, 2 `weapon-lute`, 3 `weapon-hollowed-sword`, 4 `weapon-hexer-staff`.

1. A bolt of silk: A bolt of rose-coloured silk draped over the far forearm, the near hand raised, patting a pocket.
2. A lute: A small round-backed lute held across the body, mid-strum.
3. A rusted longsword: In the near hand, a long, rusted, notched longsword held low, its point dragging.
4. A hexer's crystal staff: In the near hand, a crooked staff of blackened wood topped with a jagged cluster of black crystal wrapped in a soft violet glow.
I'll save it as weapon-sheet-m11.png.

#### `weapon-sheet-m12` · Weapon sheet: 4 weapons (A pick and a lit charge, A pick and a burning lantern, A wooden toy sword, A pitchfork) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-sapper-pick`, 2 `weapon-chief-pick-lantern`, 3 `weapon-toy-sword`, 4 `weapon-pitchfork`.

1. A pick and a lit charge: A heavy iron pick in the near hand, and a round clay blasting charge with its fuse sparking in the far hand.
2. A pick and a burning lantern: A big two-handed pick held in the near hand, and in the far hand a large iron lantern with its door open and fire roaring inside.
3. A wooden toy sword: In the near hand, a small wooden toy sword.
4. A pitchfork: In the near hand, a wooden pitchfork resting on the near shoulder.
I'll save it as weapon-sheet-m12.png.

#### `weapon-sheet-m13` · Weapon sheet: 2 weapons (A crate on the shoulder, A forging hammer and tongs) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-crate`, 2 `weapon-hammer-tongs`.

1. A crate on the shoulder: A wooden crate carried on the far shoulder and steadied with the far hand, the near hand free.
2. A forging hammer and tongs: A forging hammer in the near hand, and a pair of tongs in the far hand.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as weapon-sheet-m13.png.

#### `weapon-sheet-m14` · Weapon sheet: 4 weapons (Thornwood bow, Sunsteel blade, A mallet, A spade) · P1

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-thornwood-bow`, 2 `weapon-sunsteel-blade`, 3 `weapon-mallet`, 4 `weapon-spade`.

1. Thornwood bow: In the near hand, a strung bow of dark, twisted living wood with thorns along its back and small green leaves at the tips, held upright.
2. Sunsteel blade: In the near hand, a longsword of bright, pale-gold steel with a sunburst on the guard and a faint warm glow, held low.
3. A mallet: In the near hand, a wooden mallet.
4. A spade: In the near hand, a spade resting over the near shoulder.
I'll save it as weapon-sheet-m14.png.

#### `weapon-sheet-m15` · Weapon sheet: 4 weapons (A heron-headed cane, A felling axe, An oil lamp, Varkhul's greatsword) · P1

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-heron-cane`, 2 `weapon-felling-axe`, 3 `weapon-oil-lamp`, 4 `weapon-varkhul-greatsword`.

1. A heron-headed cane: In the near hand, a slim cane with a silver heron's-head handle.
2. A felling axe: In the near hand, a long felling axe resting over the near shoulder.
3. An oil lamp: In the near hand, a small brass oil lamp, lit.
4. Varkhul's greatsword: A huge two-handed greatsword of black iron held in both hands with the point down, a jagged black splinter of crystal set in its hilt pulsing with violet light.
I'll save it as weapon-sheet-m15.png.

#### `weapon-sheet-m16` · Weapon sheet: 3 weapons (A cube-pommelled dagger, A hooked blade, An old-pattern spear and kite shield) · P1

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-cultist-dagger`, 2 `weapon-climber-hook`, 3 `weapon-relief-kit`.

1. A cube-pommelled dagger: In the near hand, a black iron dagger with a small cube-shaped pommel.
2. A hooked blade: In the near hand, a short, rusted hooked blade.
3. An old-pattern spear and kite shield: A plain spear held upright in the near hand, and a kite-shaped wooden shield painted navy with a faded gold sun on the far forearm.
Leave figure 4 exactly as it is, with nothing added.
I'll save it as weapon-sheet-m16.png.

#### `weapon-sheet-m17` · Weapon sheet: 4 weapons (Sir Corwin's sword and sun shield, Sir Corwin's sword, too big, A Gilded knight's sword and shield, A mace and a small shield) · P2

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-corwin-kit`, 2 `weapon-corwin-sword`, 3 `weapon-gilded-kit`, 4 `weapon-mace-shield`.

1. Sir Corwin's sword and sun shield: A longsword with a gold hilt and a sun-shaped pommel held upright in the near hand, and a round steel shield with a gold sun boss on the far forearm.
2. Sir Corwin's sword, too big: A longsword with a gold hilt and a sun-shaped pommel, too long for the bearer, held upright in front of the body in both hands.
3. A Gilded knight's sword and shield: A plain longsword in the near hand, and a white kite shield with a gold sun on the far forearm.
4. A mace and a small shield: A flanged mace in the near hand, and a small round steel shield on the far forearm.
I'll save it as weapon-sheet-m17.png.

#### `weapon-sheet-m18` · Weapon sheet: 4 weapons (A spear and a lantern, Vesk's black sword, Mordain's glaive, An ancient spear and bronze shield) · P2

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-spear-lantern`, 2 `weapon-vesk-sword`, 3 `weapon-mordain-glaive`, 4 `weapon-unmade-spear-shield`.

1. A spear and a lantern: A plain spear held upright in the near hand, and a lit iron lantern hanging from the far hand.
2. Vesk's black sword: A long black sword held in front, point down, both gauntleted hands crossed on the pommel.
3. Mordain's glaive: In the near hand, a long glaive with a thin, curved blade, held upright.
4. An ancient spear and bronze shield: A long bronze-tipped spear held upright in the near hand, and a round bronze shield on the far forearm, faded and slightly greyed.
I'll save it as weapon-sheet-m18.png.

#### `weapon-sheet-m19` · Weapon sheet: 4 weapons (A pike, A tower shield and halberd, A cudgel and a violet shield, An axe and a grey shield) · P2

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-pike`, 2 `weapon-warden-kit`, 3 `weapon-protectorate-kit`, 4 `weapon-raider-kit`.

1. A pike: In the near hand, a very long pike held upright, faded and slightly greyed.
2. A tower shield and halberd: A heavy halberd in the near hand, and a tall rectangular tower shield, violet with a gold crown, on the far forearm.
3. A cudgel and a violet shield: A wooden cudgel in the near hand, and a round wooden shield with a brass boss, freshly painted violet, on the far forearm.
4. An axe and a grey shield: A bearded axe in the near hand, and a round wooden shield painted grey on the far forearm.
I'll save it as weapon-sheet-m19.png.

#### `weapon-sheet-f01` · Weapon sheet: 4 weapons (The Soul-Bound's blades, Sera's longbow, A netting needle, A loaf) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-sb-blades`, 2 `weapon-sera-longbow`, 3 `weapon-netting-needle`, 4 `weapon-loaf`.

1. The Soul-Bound's blades: A short sword in the near hand, held low and forward, and a dagger held reversed along the far forearm.
2. Sera's longbow: A plain, strung longbow taller than the figure's head, held upright in the near hand, and a single grey-fletched arrow held loosely in the far hand.
3. A netting needle: In the near hand, a wooden netting needle threaded with tarred twine.
4. A loaf: In the near hand, a round loaf of bread, held protectively.
I'll save it as weapon-sheet-f01.png.

#### `weapon-sheet-f02` · Weapon sheet: 4 weapons (A rag doll, A tankard, An armful of scrolls, A broom) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-rag-doll`, 2 `weapon-tankard`, 3 `weapon-scrolls`, 4 `weapon-broom`.

1. A rag doll: In the near hand, a battered cloth doll held by one arm.
2. A tankard: In the near hand, a pewter tankard.
3. An armful of scrolls: An armful of rolled scrolls tied with ribbon and closed with wax seals, held against the chest.
4. A broom: In the near hand, a straw broom held upright.
I'll save it as weapon-sheet-f02.png.

#### `weapon-sheet-f03` · Weapon sheet: 4 weapons (A coin on the knuckles, A wooden practice sword, A cap held in both hands, A covered basket) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-coin`, 2 `weapon-practice-sword`, 3 `weapon-flat-cap-held`, 4 `weapon-basket`.

1. A coin on the knuckles: In the near hand, a copper coin balanced on the knuckles of the near hand.
2. A wooden practice sword: In the near hand, a wooden practice sword resting on the near shoulder.
3. A cap held in both hands: An old flat cap held against the chest in both hands.
4. A covered basket: In the near hand, a wicker basket with a cloth over it, hung over the near forearm.
I'll save it as weapon-sheet-f03.png.

#### `weapon-sheet-f04` · Weapon sheet: 4 weapons (A flower basket, A pilgrim's staff, A baker's peel and a loaf, A drop spindle) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-flower-basket`, 2 `weapon-pilgrim-staff`, 3 `weapon-peel`, 4 `weapon-spindle`.

1. A flower basket: A wide wicker basket of pink roses, white lilies and daisies over the far arm, and a single rose held out in the near hand.
2. A pilgrim's staff: In the near hand, a tall pilgrim's staff with a small brass bell tied at the top.
3. A baker's peel and a loaf: A long wooden baker's peel in the near hand, and a round loaf tucked in the crook of the far arm.
4. A drop spindle: In the near hand, a drop spindle with a length of navy-and-gold thread.
I'll save it as weapon-sheet-f04.png.

#### `weapon-sheet-f05` · Weapon sheet: 2 weapons (A purse of coins, A feather duster) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-coin-purse`, 2 `weapon-feather-duster`.

1. A purse of coins: In the near hand, a small cloth bag of coins, held up and jingled.
2. A feather duster: In the near hand, a feather duster.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as weapon-sheet-f05.png.

#### `weapon-sheet-f06` · Weapon sheet: 4 weapons (Thessaly's ringed staff, A ledger and a pen, A folded shawl, A sketchbook) · P1

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-staff-ring`, 2 `weapon-ledger-pen`, 3 `weapon-folded-shawl`, 4 `weapon-sketchbook`.

1. Thessaly's ringed staff: In the near hand, a tall staff of dark wood topped with a plain open ring of silver metal, held upright.
2. A ledger and a pen: A thick leather-bound ledger tucked under the far arm, and a pen in the near hand.
3. A folded shawl: In the near hand, a folded knitted shawl, held out as if offering it.
4. A sketchbook: A sketchbook held open against the chest in the far hand, and a stick of charcoal in the near hand.
I'll save it as weapon-sheet-f06.png.

#### `weapon-sheet-f07` · Weapon sheet: 4 weapons (A lit lantern, A beekeeper's smoker, A sprig of night-flowers, Vreya's wavy blade) · P2

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-lantern`, 2 `weapon-smoker`, 3 `weapon-night-flowers`, 4 `weapon-vreya-blade`.

1. A lit lantern: In the near hand, a brass lantern, lit, its warm light on the near side of the face.
2. A beekeeper's smoker: In the near hand, a tin beekeeper's smoker with bellows, a wisp of pale smoke curling from its spout, two or three bees nearby.
3. A sprig of night-flowers: In the near hand, a sprig of pale, star-shaped night-flowers.
4. Vreya's wavy blade: In the near hand, a long, wavy-bladed black sword held over the near shoulder.
I'll save it as weapon-sheet-f07.png.

#### `weapon-sheet-f08` · Weapon sheet: 2 weapons (A glowing glass lamp, A rolled map) · P2

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-glass-lamp`, 2 `weapon-map`.

1. A glowing glass lamp: In the near hand, a small glass lamp glowing with a cool white magical light.
2. A rolled map: In the near hand, a rolled map tied with a cord.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as weapon-sheet-f08.png.

### 6.1 Capes

#### `cape-sb` · The Soul-Bound's cape · P0

**Basis:** Edits `base-female`. From the guide figure SERA (a dark cape).

Edit the attached base image: change ONLY what hangs behind the body: add a long charcoal cape that hangs from the shoulders to the calves, plain, with a little wear at the hem. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-sb.png.

#### `cape-sera` · Sera's shoulder cape · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a short ranger's cape; moss green).

Edit the attached base image: change ONLY what hangs behind the body: add a short dark-moss-green wool shoulder cape with its hood down, fastened at the collar with a wooden toggle, reaching the middle of the back. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-sera.png.

#### `cape-tail-ember` · An Emberkin's ember tail · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §3.6 (`tail-emberkin`); `tools/generate_paperdoll.py` (crimson, the tip burning red through orange to yellow).

Edit the attached base image: change ONLY what hangs behind the body: add a thin crimson tail growing from the small of the back, curving down and back to about the knee, its tip glowing like a live ember, red through orange to yellow. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-tail-ember.png.

#### `cape-tail-wolf` · A Wildkin wolf's tail · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §3.6 (`tail-wildkin-wolf`).

Edit the attached base image: change ONLY what hangs behind the body: add a bushy grey-brown wolf's tail growing from the small of the back, hanging down and back to below the knee. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-tail-wolf.png.

#### `cape-tail-hawk` · A Wildkin hawk's tail feathers · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §3.6 (the hawk line).

Edit the attached base image: change ONLY what hangs behind the body: add a short fan of barred brown-and-cream tail feathers growing from the small of the back, angled down and back. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-tail-hawk.png.

#### `cape-tail-bear` · A Wildkin bear's tail · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §3.6 (the bear line).

Edit the attached base image: change ONLY what hangs behind the body: add a short, round, furry black bear's tail at the small of the back. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-tail-bear.png.

#### `cape-watch` · The Watch's short cape · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the Watch's blue).

Edit the attached base image: change ONLY what hangs behind the body: add a short navy-blue cape from the shoulders to the middle of the back, fastened with a small brass clasp. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-watch.png.

#### `cape-watch-captain` · The Watch captain's cloak · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Hale).

Edit the attached base image: change ONLY what hangs behind the body: add a navy-blue cloak to the knee, fastened at one shoulder with a brass clasp. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-watch-captain.png.

#### `cape-royal` · The palace's ochre cloak · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the palace's navy and gold).

Edit the attached base image: change ONLY what hangs behind the body: add an ochre-gold cloak from the shoulders to the knee, with a white lining. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-royal.png.

#### `cape-leon` · King Leon's cloak · P0

**Basis:** Edits `base-male`. From `Cast.cs` (Leon's cloak); `03-art-overhaul.md` §7.4.

Edit the attached base image: change ONLY what hangs behind the body: add a long navy-blue cloak from the shoulders to the floor, heavy and plain. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-leon.png.

#### `cape-travelling` · Travelling cloak · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`travelling-cloak`); the `traveller` sheet.

Edit the attached base image: change ONLY what hangs behind the body: add a heavy dark-green wool travelling cloak to the calf, its hood down, a simple iron clasp, a little frayed at the hem. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-travelling.png.

#### `cape-elara-hooded` · A plain charcoal cloak · P0

**Basis:** Edits `base-female`. From `Cast.cs` (`elara-hooded`: a charcoal cloak).

Edit the attached base image: change ONLY what hangs behind the body: add a plain charcoal wool cloak to the calf, hanging from the shoulders, no ornament at all. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-elara-hooded.png.

#### `cape-protection` · Cloak of protection · P1

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`cloak-of-protection`).

Edit the attached base image: change ONLY what hangs behind the body: add a fine cloak to the calf in deep blue-grey, with a subtle pattern of small silver stars woven into it, a silver clasp and a faint shimmer along its edge. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-protection.png.

#### `cape-blanket` · A blanket worn as a cloak · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the refugees' charcoal); `08-random-encounters.md` §6.

Edit the attached base image: change ONLY what hangs behind the body: add a worn charcoal wool blanket round the shoulders, tied at the chest, hanging to the backs of the knees. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-blanket.png.

#### `cape-short-lavender` · A short lavender cape · P0

**Basis:** Edits `base-male`. From `Cast.cs` (Osric's and Lark's lavender cloaks).

Edit the attached base image: change ONLY what hangs behind the body: add a short lavender cape over the shoulders, reaching the middle of the back, with a small silver clasp. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-short-lavender.png.

#### `cape-tamsin` · The Lodge's teal cloak and hawk feathers · P0

**Basis:** Edits `base-female`. From `Cast.cs` (Tamsin's teal cloak; the hawk line).

Edit the attached base image: change ONLY what hangs behind the body: add a short teal ranger's cloak to the middle of the back, and below its hem a short fan of barred brown-and-cream tail feathers growing from the small of the back. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-tamsin.png.

#### `cape-old-woman` · An old woman's rose cape · P0

**Basis:** Edits `base-female`. **(derived)** From the `old-woman` sheet.

Edit the attached base image: change ONLY what hangs behind the body: add a short hooded cape in a deep, dusty rose (not wine red), the hood down, to the middle of the back. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-old-woman.png.

#### `cape-campaign` · Aldric's campaign cloak · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (an old campaign cloak; faded red).

Edit the attached base image: change ONLY what hangs behind the body: add an old campaign cloak in faded brick red, patched and singed at the hem, to the knee. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-campaign.png.

#### `cape-ash` · An ash-grey hooded cloak · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Brenna: an ash-grey cloak).

Edit the attached base image: change ONLY what hangs behind the body: add a long ash-grey cloak with its hood down, ragged and singed at the hem, to the calf. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-ash.png.

#### `cape-pedlar-pack` · A pedlar's pack frame · P1

**Basis:** Edits `base-female`. From `08-random-encounters.md` (`enc-mags`).

Edit the attached base image: change ONLY what hangs behind the body: add a wooden pack frame on the back, strapped over the shoulders and loaded high with coils of rope, pots, a lantern and cloth bundles. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-pedlar-pack.png.

#### `cape-valencia` · Valencia's cape · P2

**Basis:** Edits `base-female`. From the guide figure VALENCIA (a dark cape); `03-art-overhaul.md` §7.3 (wine red).

Edit the attached base image: change ONLY what hangs behind the body: add a long cape in charcoal-black, lined with wine red, falling from the shoulders to the ground. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-valencia.png.

#### `cape-severin` · Severin's velvet cape · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a cape that spreads like wings).

Edit the attached base image: change ONLY what hangs behind the body: add a long black velvet cape lined in wine red, hanging from the shoulders to the ground. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-severin.png.

#### `cape-severin-wings` · Severin's cape, spread like wings · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (`severin-boss`).

Edit the attached base image: change ONLY what hangs behind the body: add the same long black velvet cape lined in wine red, spread out wide on either side like the wings of a huge bat, its edges ragged and stretched into points. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-severin-wings.png.

#### `cape-thrall-knight` · A thrall-knight's cape · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (black and wine red).

Edit the attached base image: change ONLY what hangs behind the body: add a black cape lined in wine red, to the calf, ragged at the hem. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-thrall-knight.png.

#### `cape-corwin` · Sir Corwin's white cloak · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (gold-white).

Edit the attached base image: change ONLY what hangs behind the body: add a white cloak edged in gold, to the calf. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-corwin.png.

#### `cape-gilded` · A Gilded knight's cape · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.5 (the `order` pool).

Edit the attached base image: change ONLY what hangs behind the body: add a short white cape to the middle of the back, edged with a thin gold line. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-gilded.png.

#### `cape-varkhul` · Varkhul's tattered cloak · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (ash).

Edit the attached base image: change ONLY what hangs behind the body: add a tattered ash-grey cloak hanging in strips, scorched at the edges. At this angle it hangs behind the figure and shows past the body's far side and below it; nothing is drawn over the front of the body. Keep the bald head, its face and the body in its underwear exactly as they are, and keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as cape-varkhul.png.

### 6.2 Helms

Hair and helm are one slot, what's on the head (§4.8 has the hairs): a helm replaces the hair, and one that should show hair has it painted in. Each is painted on the blank base.

#### `helm-watch` · The Watch helm · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (the Watch's helm); `Characters/LookOptions.cs` (HELM).

Edit the attached blank base image: change ONLY the head: put an open-faced steel helm with a brass rim and no crest on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-watch.png.

#### `helm-watch-crest` · The Watch captain's crested helm · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a Watch helm with a crest brush).

Edit the attached blank base image: change ONLY the head: put an open-faced steel helm with a brass rim and a short, stiff crest of navy horsehair running from front to back on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-watch-crest.png.

#### `helm-royal-guard` · The Royal Guard's helm · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (the palace guards' helm).

Edit the attached blank base image: change ONLY the head: put a polished steel helm with a gold-trimmed brim and a short navy plume on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-royal-guard.png.

#### `helm-tricorn` · A tricorn hat · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (Odelle's tricorn); `Characters/LookOptions.cs` (TRICORN).

Edit the attached blank base image: change ONLY the head: put a weathered black tricorn hat with a thin gold trim on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-tricorn.png.

#### `helm-crown` · A small gold crown · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (Leon: a small plain gold crown); `Characters/LookOptions.cs` (CROWN).

Edit the attached blank base image: change ONLY the head: put a small, plain gold crown, a simple band with low points and one ruby at the front on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-crown.png.

#### `helm-circlet` · A gold circlet · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (Elara's circlet); `Characters/LookOptions.cs` (CIRCLET).

Edit the attached blank base image: change ONLY the head: put a slim gold circlet with a small sapphire at the brow on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-circlet.png.

#### `helm-hood-charcoal` · A charcoal hood · P0

**Basis:** Edits `base-female-blank`. From `Cast.cs` (`elara-hooded`); `Characters/LookOptions.cs` (HOOD hides the hair).

Edit the attached blank base image: change ONLY the head: put a deep charcoal wool hood worn up and pulled forward, hiding the whole head and putting the upper face in shadow on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-hood-charcoal.png.

#### `helm-kael-hood` · Kael's pointed hood · P0

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (horns through a pointed hood) and §3.6 (horns are drawn over any hood).

Edit the attached blank base image: change ONLY the head: put a charcoal hood worn up, ending in a soft point at the back, the edge of the face in shadow (the game draws the horns over it) on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-kael-hood.png.

#### `helm-beret` · A beret · P0

**Basis:** Edits `base-male-blank`. From the art plan's §3 (a beret); `Cast.cs` (the messenger, Lark).

Edit the attached blank base image: change ONLY the head: put a soft navy beret worn tilted, with a short white feather on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-beret.png.

#### `helm-knit-cap` · A knitted cap · P0

**Basis:** Edits `base-male-blank`. **(derived)** From the Saltwind pool (caps).

Edit the attached blank base image: change ONLY the head: put a dark-blue knitted fisherman's cap, snug and rolled at the brim on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-knit-cap.png.

#### `helm-bandana-red` · A Red Gull bandana · P0

**Basis:** Edits `base-male-blank`. From `Cast.cs` (the Red Gulls' bandanas).

Edit the attached blank base image: change ONLY the head: put a raspberry-red bandana tied over the head, knotted at the back with the ends hanging on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-bandana-red.png.

#### `helm-bandana` · A bandana · P0

**Basis:** Edits `base-male-blank`. From `Characters/LookOptions.cs` (BANDANA).

Edit the attached blank base image: change ONLY the head: put a dark grey bandana tied over the head, knotted at the back with the ends hanging on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-bandana.png.

#### `helm-kettle` · A kettle helm · P1

**Basis:** Edits `base-male-blank`. **(derived)** From the recruit, A1-C7-01.

Edit the attached blank base image: change ONLY the head: put a plain steel kettle helm with a wide brim, a size too big, tipped back on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-kettle.png.

#### `helm-hollowed-kettle` · A rusted kettle helm · P0

**Basis:** Edits `base-male-blank`. From the art plan (the footman's rusted kettle helm).

Edit the attached blank base image: change ONLY the head: put a rusted, dented iron kettle helm with a wide brim, sitting low on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-hollowed-kettle.png.

#### `helm-hollowed-hood` · A hexer's deep hood · P0

**Basis:** Edits `base-male-blank`. From the art plan (a deep hood with only the eye points showing).

Edit the attached blank base image: change ONLY the head: put a deep, ragged, ash-stained hood worn up and pulled far forward, hiding the face except the lower jaw on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-hollowed-hood.png.

#### `helm-sapper-goggles` · A sapper's cap and goggles · P0

**Basis:** Edits `base-male-blank`. From the art plan (a leather cap and goggles).

Edit the attached blank base image: change ONLY the head: put a battered leather cap with ear flaps and a pair of cracked brass goggles pushed up on it on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-sapper-goggles.png.

#### `helm-sapper-faceplate` · A cracked iron faceplate · P0

**Basis:** Edits `base-male-blank`. From the art plan (the sapper chief's cracked iron faceplate).

Edit the attached blank base image: change ONLY the head: put a cracked iron faceplate riveted to a leather hood, covering the whole face, with ember-orange light glowing through its eye slits and cracks on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-sapper-faceplate.png.

#### `helm-wide-brim` · A wide-brimmed hat · P0

**Basis:** Edits `base-female-blank`. From the guide sheet's accessories (a wide-brim hat); the `trader` sheet.

Edit the attached blank base image: change ONLY the head: put a wide-brimmed tan felt hat with a band of braided cord on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-wide-brim.png.

#### `helm-straw-hat` · A straw hat · P0

**Basis:** Edits `base-male-blank`. **(derived)** From the `farmhand` sheet; Mags.

Edit the attached blank base image: change ONLY the head: put a battered, wide straw hat on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-straw-hat.png.

#### `helm-flat-cap` · A flat cap · P0

**Basis:** Edits `base-male-blank`. **(derived)** From the `courier` and `child` sheets.

Edit the attached blank base image: change ONLY the head: put a slate-blue flat cap on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-flat-cap.png.

#### `helm-kerchief` · A kerchief · P0

**Basis:** Edits `base-female-blank`. **(derived)** From the `baker` and `flower-seller` sheets.

Edit the attached blank base image: change ONLY the head: put a cream kerchief tied over the hair, knotted at the nape on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-kerchief.png.

#### `helm-lace-cap` · A lace cap · P1

**Basis:** Edits `base-female-blank`. **(derived)** From Mother Gannet.

Edit the attached blank base image: change ONLY the head: put a small white lace cap pinned on top of the head on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-lace-cap.png.

#### `helm-varkhul` · Varkhul's cracked helm · P1

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a cracked visor; ember eyes).

Edit the attached blank base image: change ONLY the head: put a closed great helm of blackened iron, the visor cracked down the middle, and through the crack a single burning ember-orange eye on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-varkhul.png.

#### `helm-nasal` · An old-pattern nasal helm · P1

**Basis:** Edits `base-male-blank`. **(derived)** From `enc-relief-ghost` (old-pattern armour).

Edit the attached blank base image: change ONLY the head: put an old-fashioned round-topped steel helm with a nose guard on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-nasal.png.

#### `helm-abbey-hood` · The abbey's square hood · P2

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4 (Sister Vale: the habit's square hood).

Edit the attached blank base image: change ONLY the head: put a stiff, square-cornered grey hood worn up, with a white linen band inside it, hiding all the hair and shading the eyes on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-abbey-hood.png.

#### `helm-bee-veil` · A bee veil · P2

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4 (Orla: a bee veil on a wide hat).

Edit the attached blank base image: change ONLY the head: put a wide-brimmed straw hat with a fine honey-coloured mesh bee veil, pinned up at the front so the face shows and hanging down at the back and sides on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-bee-veil.png.

#### `helm-corwin` · Sir Corwin's plumed helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a tall plume).

Edit the attached blank base image: change ONLY the head: put an open-faced gilded helm with a tall white plume rising from the crest on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-corwin.png.

#### `helm-gilded` · A Gilded knight's helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.5 (the `order` pool).

Edit the attached blank base image: change ONLY the head: put an open-faced steel helm with a narrow gold band and no plume on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-gilded.png.

#### `helm-crown-thorns` · The crown of thorns · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §6.4 (`crown-of-thorns`: Severin's iron thorn circlet).

Edit the attached blank base image: change ONLY the head: put a circlet of black iron wrought into thorns on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-crown-thorns.png.

#### `helm-circlet-silver` · A silver circlet · P2

**Basis:** Edits `base-female-blank`. **(derived)** From the guide figure VALENCIA (a crown).

Edit the attached blank base image: change ONLY the head: put a slender, simple silver circlet with a tiny dark-red stone on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-circlet-silver.png.

#### `helm-vesk-wolf` · Vesk's wolf helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a helm shaped as a wolf).

Edit the attached blank base image: change ONLY the head: put a closed black helm with thin wine-red trim, shaped like a snarling wolf's head, the snout forming the visor, two red points of light in the eye slits on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-vesk-wolf.png.

#### `helm-vreya-crown` · Vreya's crowned helm · P2

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §7.4 (a helm shaped as a crown).

Edit the attached blank base image: change ONLY the head: put an open-faced black helm with thin wine-red trim, crowned with a ring of tall, sharp points, framing the face on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-vreya-crown.png.

#### `helm-mordain-beak` · Mordain's beaked helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a helm shaped as a beak).

Edit the attached blank base image: change ONLY the head: put a closed black helm with thin wine-red trim, drawn out into a long, curved beak like a carrion bird's, with round red-glass eyepieces on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-mordain-beak.png.

#### `helm-crown-dusk-dawn` · The crown of dusk and dawn · P2

**Basis:** Edits `base-female-blank`. From `03-art-overhaul.md` §6.4 (`crown-dusk-dawn`: black iron braided with gold glass, the shard at the front).

Edit the attached blank base image: change ONLY the head: put a crown of black iron braided with strands of gold glass, a jagged black crystal splinter set at the front glowing violet on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-crown-dusk-dawn.png.

#### `helm-vorlaith` · Vorlaith's crown of clock-hands · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (a crown of broken clock-hands).

Edit the attached blank base image: change ONLY the head: put a crown made of broken clock-hands of old, dark iron and brass, of different sizes, pointing every way on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-vorlaith.png.

#### `helm-warden` · A Protectorate warden's great helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (the warden in heavy plate).

Edit the attached blank base image: change ONLY the head: put a closed great helm of steel with a violet enamel finish, a narrow visor slit and a gold crest ridge on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-warden.png.

#### `helm-bronze-crest` · A crested bronze helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (the Unmade: armour of every age).

Edit the attached blank base image: change ONLY the head: put an ancient bronze helm with cheek guards and a tall horsehair crest, faded and slightly greyed on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-bronze-crest.png.

#### `helm-morion` · A crested open helm · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (the Unmade).

Edit the attached blank base image: change ONLY the head: put a steel open helm with a high comb crest and an upturned brim, faded and slightly greyed on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-morion.png.

#### `helm-bandit-hood` · A bandit's hood and scarf · P1

**Basis:** Edits `base-male-blank`. **(derived)** From `08-random-encounters.md` §3 (bandits).

Edit the attached blank base image: change ONLY the head: put a ragged brown hood worn up, and a dirty brown scarf pulled up over the nose on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-bandit-hood.png.

#### `helm-leather-cap` · Leather cap · P0

**Basis:** Edits `base-male-blank`. From `Items/ItemCatalog.cs` (`leather-cap`).

Edit the attached blank base image: change ONLY the head: put a padded brown leather cap with ear flaps, stitched in panels on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-leather-cap.png.

#### `helm-iron-helm` · Iron helm · P0

**Basis:** Edits `base-male-blank`. From `Items/ItemCatalog.cs` (`iron-helm`); `03-art-overhaul.md` §3.7 (a great helm hides the hair).

Edit the attached blank base image: change ONLY the head: put a closed iron helm with a narrow visor slit and a riveted brow band, hiding the whole head on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-iron-helm.png.

#### `helm-circlet-of-focus` · Circlet of focus · P0

**Basis:** Edits `base-female-blank`. From `Items/ItemCatalog.cs` (`circlet-of-focus`).

Edit the attached blank base image: change ONLY the head: put a slim silver circlet with a small blue stone at the brow that gives off a faint glow on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-circlet-of-focus.png.

### 6.3 Weapons

#### `weapon-dagger` · Dagger · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`dagger`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a plain steel dagger with a leather-wrapped grip, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-dagger.png.

#### `weapon-shortsword` · Shortsword · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`shortsword`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a plain steel shortsword with a simple crossguard and a leather grip, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-shortsword.png.

#### `weapon-longsword` · Longsword · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`longsword`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a plain steel longsword with a straight crossguard and a leather-wrapped grip, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-longsword.png.

#### `weapon-rapier` · Rapier · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`rapier`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a slim rapier with a swept steel hilt, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-rapier.png.

#### `weapon-scimitar` · Scimitar · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`scimitar`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a curved steel scimitar with a brass guard, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-scimitar.png.

#### `weapon-handaxe` · Hand axe · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`handaxe`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a short hand axe with a steel head and a wooden haft, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-handaxe.png.

#### `weapon-battleaxe` · Battleaxe · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`battleaxe`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long-hafted bearded battleaxe with a heavy steel head, held across the body. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-battleaxe.png.

#### `weapon-greatsword` · Greatsword · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`greatsword`).

Edit the attached base image: change ONLY the hands, giving them a two-handed steel greatsword held in both hands in front of the body, point down near the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-greatsword.png.

#### `weapon-warhammer` · Warhammer · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`warhammer`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a warhammer with a square steel head and a back spike on a wooden haft, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-warhammer.png.

#### `weapon-mace` · Mace · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`mace`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a flanged steel mace with a leather grip, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-mace.png.

#### `weapon-quarterstaff` · Quarterstaff · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`quarterstaff`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a plain ash-wood quarterstaff, a little taller than a person, held upright with its foot on the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-quarterstaff.png.

#### `weapon-spear` · Spear · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`spear`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a plain spear with a leaf-shaped steel head, held upright with its butt on the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spear.png.

#### `weapon-shortbow` · Shortbow · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`shortbow`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a strung recurved shortbow, held low and vertical by its grip. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-shortbow.png.

#### `weapon-longbow` · Longbow · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`longbow`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a strung plain wooden longbow, taller than a person's shoulder, held upright by its grip. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-longbow.png.

#### `weapon-light-crossbow` · Light crossbow · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`light-crossbow`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small crossbow of wood and steel, held by its grip and pointed at the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-light-crossbow.png.

#### `weapon-ember-dagger` · Ember dagger · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`ember-dagger`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a dagger with a blade of dark metal that glows orange along its edge like a coal, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-ember-dagger.png.

#### `weapon-thornwood-bow` · Thornwood bow · P1

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`thornwood-bow`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a strung bow of dark, twisted living wood with thorns along its back and small green leaves at the tips, held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-thornwood-bow.png.

#### `weapon-sunsteel-blade` · Sunsteel blade · P1

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`sunsteel-blade`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a longsword of bright, pale-gold steel with a sunburst on the guard and a faint warm glow, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sunsteel-blade.png.

#### `offhand-wooden-shield` · Wooden shield · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`wooden-shield`).

Edit the attached base image: change ONLY the far forearm: strap to the outside of it, the hand through the grip, a round wooden shield with an iron rim and a central iron boss, scuffed. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-wooden-shield.png.

#### `offhand-steel-shield` · Steel shield · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`steel-shield`).

Edit the attached base image: change ONLY the far forearm: strap to the outside of it, the hand through the grip, a steel heater shield with a rolled rim. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-steel-shield.png.

#### `offhand-warden-shield` · Warden's shield · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`warden-shield`).

Edit the attached base image: change ONLY the far forearm: strap to the outside of it, the hand through the grip, a tall steel kite shield with a brass rim and an embossed oak-leaf emblem. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-warden-shield.png.

#### `offhand-dagger` · Dagger, in the off hand · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`dagger`): a light weapon can go in the off hand.

Edit the attached base image: change ONLY the far hand, the one on the right of the picture: close its fingers naturally round a plain steel dagger with a leather-wrapped grip, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the near hand, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-dagger.png.

#### `offhand-shortsword` · Shortsword, in the off hand · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`shortsword`): a light weapon can go in the off hand.

Edit the attached base image: change ONLY the far hand, the one on the right of the picture: close its fingers naturally round a plain steel shortsword with a simple crossguard and a leather grip, held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the near hand, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-shortsword.png.

#### `offhand-scimitar` · Scimitar, in the off hand · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`scimitar`): a light weapon can go in the off hand.

Edit the attached base image: change ONLY the far hand, the one on the right of the picture: close its fingers naturally round a curved steel scimitar with a brass guard, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the near hand, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-scimitar.png.

#### `offhand-handaxe` · Hand axe, in the off hand · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`handaxe`): a light weapon can go in the off hand.

Edit the attached base image: change ONLY the far hand, the one on the right of the picture: close its fingers naturally round a short hand axe with a steel head and a wooden haft, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the near hand, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-handaxe.png.

#### `offhand-ember-dagger` · Ember dagger, in the off hand · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`ember-dagger`): a light weapon can go in the off hand.

Edit the attached base image: change ONLY the far hand, the one on the right of the picture: close its fingers naturally round a dagger with a blade of dark metal that glows orange along its edge like a coal, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the near hand, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as offhand-ember-dagger.png.

#### `weapon-sb-blades` · The Soul-Bound's blades · P0

**Basis:** Edits `base-female`. From `Origins.cs` (`soulbound`: a Shadowstep).

Edit the attached base image: change ONLY the hands, giving them a short sword in the near hand, held low and forward, and a dagger held reversed along the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sb-blades.png.

#### `weapon-sera-longbow` · Sera's longbow · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a longbow taller than her head).

Edit the attached base image: change ONLY the hands, giving them a plain, strung longbow taller than the figure's head, held upright in the near hand, and a single grey-fletched arrow held loosely in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sera-longbow.png.

#### `weapon-kael-knives` · Kael's twin knives · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §6.4 (`kael-knives`: twin long knives, red cord on the grips).

Edit the attached base image: change ONLY the hands, giving them a pair of long knives with red cord wound round their grips, one in each hand, the near one held reversed. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-kael-knives.png.

#### `weapon-kael-rapier` · Kael's rapier and parrying dagger · P0

**Basis:** Edits `base-male`. From `EnemyCatalog.cs` (`kael-duel`: a rapier and an off-hand dagger).

Edit the attached base image: change ONLY the hands, giving them a slim rapier with a swept hilt in the near hand, extended forward in a fencer's guard, and a parrying dagger held reversed in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-kael-rapier.png.

#### `weapon-brannock` · Brannock's sword and shield · P0

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §6.4 (`brannock-shield`: a battered round shield, an iron rim); `Origins.cs` (a longsword).

Edit the attached base image: change ONLY the hands, giving them a plain, heavy longsword in the near hand, held low, and a battered round wooden shield with an iron rim strapped to the far forearm, its rim still bare. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-brannock.png.

#### `weapon-cutlass` · Cutlass · P0

**Basis:** Edits `base-male`. **(derived)** From Odelle, Grell, Ruck, the smugglers.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a notched cutlass with a brass basket guard, held casually, point down. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-cutlass.png.

#### `weapon-gaff` · A fisherman's gaff · P0

**Basis:** Edits `base-male`. From `10-classes.md` (Netcaster: hooks).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long wooden gaff with an iron hook at the end, held upright like a walking staff. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-gaff.png.

#### `weapon-cudgel` · Cudgel · P0

**Basis:** Edits `base-male`. From `EnemyCatalog.cs` (the Red Gulls' cudgels); `10-classes.md` (Watchblade: cudgels).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a knotted oak cudgel bound with iron bands, held low. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-cudgel.png.

#### `weapon-watch-kit` · The Watch's spear and shield · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the Watch).

Edit the attached base image: change ONLY the hands, giving them a plain spear held upright in the near hand, and a round wooden shield with a brass boss on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-watch-kit.png.

#### `weapon-halberd` · Halberd · P0

**Basis:** Edits `base-male`. From `Cast.cs` (the palace guards).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a tall halberd with a polished blade, held upright, its butt on the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-halberd.png.

#### `weapon-netting-needle` · A netting needle · P0

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Corra, a net-mender).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden netting needle threaded with tarred twine. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-netting-needle.png.

#### `weapon-telescope` · A brass telescope · P0

**Basis:** Edits `base-male`. **(derived)** From the lookout, P-04.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a brass telescope, half extended. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-telescope.png.

#### `weapon-tack-hammer` · A tack hammer and a notice · P0

**Basis:** Edits `base-male`. **(derived)** From Fennick, the job board.

Edit the attached base image: change ONLY the hands, giving them a small tack hammer in the near hand, and a fresh notice on a clipboard in the far hand, its writing only scribbled lines. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-tack-hammer.png.

#### `weapon-loaf` · A loaf · P0

**Basis:** Edits `base-female`. From `08-random-encounters.md` §6 (Hedda's loaf).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a round loaf of bread, held protectively. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-loaf.png.

#### `weapon-rag-doll` · A rag doll · P0

**Basis:** Edits `base-female`. **(derived)** From Lise.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a battered cloth doll held by one arm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-rag-doll.png.

#### `weapon-walking-stick` · A walking stick · P0

**Basis:** Edits `base-male`. **(derived)** From the old and the travellers.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a crooked wooden walking stick, its foot on the ground, taking the weight. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-walking-stick.png.

#### `weapon-tin-cup` · A tin cup · P0

**Basis:** Edits `base-male`. **(derived)** From Teodor.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a dented tin cup of water. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-tin-cup.png.

#### `weapon-quill-letter` · A quill and a sealed letter · P0

**Basis:** Edits `base-male`. **(derived)** From Crane.

Edit the attached base image: change ONLY the hands, giving them a quill pen in the near hand, and a folded letter with a dark wax seal in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-quill-letter.png.

#### `weapon-letter` · A sealed letter · P0

**Basis:** Edits `base-male`. **(derived)** From the royal messenger.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a rolled letter with a gold wax seal and a ribbon. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-letter.png.

#### `weapon-letters` · A bundle of letters · P0

**Basis:** Edits `base-male`. **(derived)** From the `courier` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a bundle of sealed letters tied with string. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-letters.png.

#### `weapon-tankard` · A tankard · P0

**Basis:** Edits `base-female`. **(derived)** From Hessa.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a pewter tankard. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-tankard.png.

#### `weapon-stew-pot` · A stew pot · P0

**Basis:** Edits `base-male`. **(derived)** From Pip.

Edit the attached base image: change ONLY the hands, giving them a steaming iron stew pot carried in front with both hands, a cloth wrapped round the handle. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-stew-pot.png.

#### `weapon-silk-bolt` · A bolt of silk · P0

**Basis:** Edits `base-male`. **(derived)** From Osric.

Edit the attached base image: change ONLY the hands, giving them a bolt of rose-coloured silk draped over the far forearm, the near hand raised, patting a pocket. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-silk-bolt.png.

#### `weapon-lute` · A lute · P0

**Basis:** Edits `base-male`. **(derived)** From Lark.

Edit the attached base image: change ONLY the hands, giving them a small round-backed lute held across the body, mid-strum. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-lute.png.

#### `weapon-scrolls` · An armful of scrolls · P0

**Basis:** Edits `base-female`. **(derived)** From Ilsa.

Edit the attached base image: change ONLY the hands, giving them an armful of rolled scrolls tied with ribbon and closed with wax seals, held against the chest. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-scrolls.png.

#### `weapon-broom` · A broom · P0

**Basis:** Edits `base-female`. **(derived)** From Hesper; the maids.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a straw broom held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-broom.png.

#### `weapon-coin` · A coin on the knuckles · P0

**Basis:** Edits `base-female`. **(derived)** From Wren.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a copper coin balanced on the knuckles of the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-coin.png.

#### `weapon-practice-sword` · A wooden practice sword · P0

**Basis:** Edits `base-female`. From `10-classes.md` §5.1 (`train-imra`: a practice sword).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden practice sword resting on the near shoulder. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-practice-sword.png.

#### `weapon-flat-cap-held` · A cap held in both hands · P0

**Basis:** Edits `base-female`. **(derived)** From Maud, Pell, the lost boy.

Edit the attached base image: change ONLY the hands, giving them an old flat cap held against the chest in both hands. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-flat-cap-held.png.

#### `weapon-staff-ring` · Thessaly's ringed staff · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a staff with a ring at the top).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a tall staff of dark wood topped with a plain open ring of silver metal, held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-staff-ring.png.

#### `weapon-ledger-pen` · A ledger and a pen · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Gannet: a ledger always under the arm).

Edit the attached base image: change ONLY the hands, giving them a thick leather-bound ledger tucked under the far arm, and a pen in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-ledger-pen.png.

#### `weapon-folded-shawl` · A folded shawl · P1

**Basis:** Edits `base-female`. From `07-companions.md` (`brannock-hester`: she holds a shawl).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a folded knitted shawl, held out as if offering it. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-folded-shawl.png.

#### `weapon-mallet` · A mallet · P1

**Basis:** Edits `base-male`. **(derived)** From Tobin; the masons.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden mallet. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-mallet.png.

#### `weapon-spade` · A spade · P1

**Basis:** Edits `base-male`. **(derived)** From the levy, `mira-tobin`.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a spade resting over the near shoulder. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spade.png.

#### `weapon-heron-cane` · A heron-headed cane · P1

**Basis:** Edits `base-male`. **(derived)** From Harrow.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a slim cane with a silver heron's-head handle. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-heron-cane.png.

#### `weapon-felling-axe` · A felling axe · P1

**Basis:** Edits `base-male`. **(derived)** From Hobb.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long felling axe resting over the near shoulder. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-felling-axe.png.

#### `weapon-black-box` · The Black Box · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §6.4 (`black-box`: a palm-sized lightless cube, faint seams, a hum ring).

Edit the attached base image: change ONLY the near hand: turn it palm-up, with a small cube about the size of a fist, of perfect lightless black with faint seams on its faces, floating just above the open palm, a thin ring of dull violet light turning slowly round it. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-black-box.png.

#### `weapon-oil-lamp` · An oil lamp · P1

**Basis:** Edits `base-male`. **(derived)** From `maleficio-young`.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small brass oil lamp, lit. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-oil-lamp.png.

#### `weapon-sketchbook` · A sketchbook · P1

**Basis:** Edits `base-female`. **(derived)** From `mira-young`.

Edit the attached base image: change ONLY the hands, giving them a sketchbook held open against the chest in the far hand, and a stick of charcoal in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sketchbook.png.

#### `weapon-varkhul-greatsword` · Varkhul's greatsword · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a greatsword with the shard in the hilt).

Edit the attached base image: change ONLY the hands, giving them a huge two-handed greatsword of black iron held in both hands with the point down, a jagged black splinter of crystal set in its hilt pulsing with violet light. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-varkhul-greatsword.png.

#### `weapon-cultist-dagger` · A cube-pommelled dagger · P1

**Basis:** Edits `base-male`. **(derived)** From the cube cultists; `03-art-overhaul.md` §6.4 (`black-iron-blade`).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a black iron dagger with a small cube-shaped pommel. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-cultist-dagger.png.

#### `weapon-hollowed-sword` · A rusted longsword · P0

**Basis:** Edits `base-male`. From `EnemyCatalog.cs` (a rusted longsword).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long, rusted, notched longsword held low, its point dragging. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-hollowed-sword.png.

#### `weapon-hexer-staff` · A hexer's crystal staff · P0

**Basis:** Edits `base-male`. From the art plan (a staff topped with black crystal under a violet glow).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a crooked staff of blackened wood topped with a jagged cluster of black crystal wrapped in a soft violet glow. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-hexer-staff.png.

#### `weapon-sapper-pick` · A pick and a lit charge · P0

**Basis:** Edits `base-male`. From `EnemyCatalog.cs` (Pick, Blasting Charge).

Edit the attached base image: change ONLY the hands, giving them a heavy iron pick in the near hand, and a round clay blasting charge with its fuse sparking in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sapper-pick.png.

#### `weapon-chief-pick-lantern` · A pick and a burning lantern · P0

**Basis:** Edits `base-male`. From `EnemyCatalog.cs` (`sapper-chief`: Pick and Lantern).

Edit the attached base image: change ONLY the hands, giving them a big two-handed pick held in the near hand, and in the far hand a large iron lantern with its door open and fire roaring inside. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-chief-pick-lantern.png.

#### `weapon-climber-hook` · A hooked blade · P1

**Basis:** Edits `base-male`. **(derived)** From the climbers.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a short, rusted hooked blade. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-climber-hook.png.

#### `weapon-relief-kit` · An old-pattern spear and kite shield · P1

**Basis:** Edits `base-male`. **(derived)** From `enc-relief-ghost`.

Edit the attached base image: change ONLY the hands, giving them a plain spear held upright in the near hand, and a kite-shaped wooden shield painted navy with a faded gold sun on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-relief-kit.png.

#### `weapon-lantern` · A lit lantern · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §6.4 (`lantern`); `10-classes.md` (Lamplighter: lanterns).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a brass lantern, lit, its warm light on the near side of the face. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-lantern.png.

#### `weapon-smoker` · A beekeeper's smoker · P2

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §6.4 (`smoke-pot`: a beekeeper's smoker, a wisp).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a tin beekeeper's smoker with bellows, a wisp of pale smoke curling from its spout, two or three bees nearby. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-smoker.png.

#### `weapon-corwin-kit` · Sir Corwin's sword and sun shield · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §6.4 (`corwin-sword`).

Edit the attached base image: change ONLY the hands, giving them a longsword with a gold hilt and a sun-shaped pommel held upright in the near hand, and a round steel shield with a gold sun boss on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-corwin-kit.png.

#### `weapon-corwin-sword` · Sir Corwin's sword, too big · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (Davin carries `corwin-sword`).

Edit the attached base image: change ONLY the hands, giving them a longsword with a gold hilt and a sun-shaped pommel, too long for the bearer, held upright in front of the body in both hands. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-corwin-sword.png.

#### `weapon-gilded-kit` · A Gilded knight's sword and shield · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.5 (the `order` pool).

Edit the attached base image: change ONLY the hands, giving them a plain longsword in the near hand, and a white kite shield with a gold sun on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-gilded-kit.png.

#### `weapon-mace-shield` · A mace and a small shield · P2

**Basis:** Edits `base-male`. **(derived)** From the Gilded sergeant.

Edit the attached base image: change ONLY the hands, giving them a flanged mace in the near hand, and a small round steel shield on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-mace-shield.png.

#### `weapon-night-flowers` · A sprig of night-flowers · P2

**Basis:** Edits `base-female`. **(derived)** From Isadora's glasshouse.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a sprig of pale, star-shaped night-flowers. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-night-flowers.png.

#### `weapon-toy-sword` · A wooden toy sword · P0

**Basis:** Edits `base-male`. From `08-random-encounters.md` (`enc-hollowmere-child`); the `child` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small wooden toy sword. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-toy-sword.png.

#### `weapon-basket` · A covered basket · P0

**Basis:** Edits `base-female`. **(derived)** From the old women and farm wives.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wicker basket with a cloth over it, hung over the near forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-basket.png.

#### `weapon-flower-basket` · A flower basket · P0

**Basis:** Edits `base-female`. **(derived)** From the `flower-seller` sheet.

Edit the attached base image: change ONLY the hands, giving them a wide wicker basket of pink roses, white lilies and daisies over the far arm, and a single rose held out in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-flower-basket.png.

#### `weapon-spear-lantern` · A spear and a lantern · P2

**Basis:** Edits `base-male`. From `08-random-encounters.md` (`enc-thrall-patrol`: a lantern).

Edit the attached base image: change ONLY the hands, giving them a plain spear held upright in the near hand, and a lit iron lantern hanging from the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spear-lantern.png.

#### `weapon-vesk-sword` · Vesk's black sword · P2

**Basis:** Edits `base-male`. **(derived)** From Vesk.

Edit the attached base image: change ONLY the hands, giving them a long black sword held in front, point down, both gauntleted hands crossed on the pommel. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-vesk-sword.png.

#### `weapon-vreya-blade` · Vreya's wavy blade · P2

**Basis:** Edits `base-female`. **(derived)** From Vreya.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long, wavy-bladed black sword held over the near shoulder. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-vreya-blade.png.

#### `weapon-mordain-glaive` · Mordain's glaive · P2

**Basis:** Edits `base-male`. **(derived)** From Mordain.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long glaive with a thin, curved blade, held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-mordain-glaive.png.

#### `weapon-glass-lamp` · A glowing glass lamp · P2

**Basis:** Edits `base-female`. **(derived)** From the Guild apprentice.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small glass lamp glowing with a cool white magical light. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-glass-lamp.png.

#### `weapon-map` · A rolled map · P2

**Basis:** Edits `base-female`. **(derived)** From Elara in Act 3.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a rolled map tied with a cord. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-map.png.

#### `weapon-unmade-spear-shield` · An ancient spear and bronze shield · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the Unmade).

Edit the attached base image: change ONLY the hands, giving them a long bronze-tipped spear held upright in the near hand, and a round bronze shield on the far forearm, faded and slightly greyed. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-unmade-spear-shield.png.

#### `weapon-pike` · A pike · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the Unmade).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a very long pike held upright, faded and slightly greyed. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-pike.png.

#### `weapon-warden-kit` · A tower shield and halberd · P2

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §9 (the warden).

Edit the attached base image: change ONLY the hands, giving them a heavy halberd in the near hand, and a tall rectangular tower shield, violet with a gold crown, on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-warden-kit.png.

#### `weapon-protectorate-kit` · A cudgel and a violet shield · P2

**Basis:** Edits `base-male`. From `08-random-encounters.md` (`enc-checkpoint`: nonlethal).

Edit the attached base image: change ONLY the hands, giving them a wooden cudgel in the near hand, and a round wooden shield with a brass boss, freshly painted violet, on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-protectorate-kit.png.

#### `weapon-raider-kit` · An axe and a grey shield · P2

**Basis:** Edits `base-male`. **(derived)** From `enc-brenna-raiders`.

Edit the attached base image: change ONLY the hands, giving them a bearded axe in the near hand, and a round wooden shield painted grey on the far forearm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-raider-kit.png.

#### `weapon-pilgrim-staff` · A pilgrim's staff · P0

**Basis:** Edits `base-female`. **(derived)** From the `pilgrim` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a tall pilgrim's staff with a small brass bell tied at the top. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-pilgrim-staff.png.

#### `weapon-pitchfork` · A pitchfork · P0

**Basis:** Edits `base-male`. **(derived)** From the `farmhand` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden pitchfork resting on the near shoulder. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-pitchfork.png.

#### `weapon-crate` · A crate on the shoulder · P0

**Basis:** Edits `base-male`. **(derived)** From the `porter` sheet.

Edit the attached base image: change ONLY the hands, giving them a wooden crate carried on the far shoulder and steadied with the far hand, the near hand free. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-crate.png.

#### `weapon-peel` · A baker's peel and a loaf · P0

**Basis:** Edits `base-female`. **(derived)** From the `baker` sheet.

Edit the attached base image: change ONLY the hands, giving them a long wooden baker's peel in the near hand, and a round loaf tucked in the crook of the far arm. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-peel.png.

#### `weapon-hammer-tongs` · A forging hammer and tongs · P0

**Basis:** Edits `base-male`. **(derived)** From the `smith` sheet.

Edit the attached base image: change ONLY the hands, giving them a forging hammer in the near hand, and a pair of tongs in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-hammer-tongs.png.

#### `weapon-spindle` · A drop spindle · P0

**Basis:** Edits `base-female`. **(derived)** From the `weaver` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a drop spindle with a length of navy-and-gold thread. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spindle.png.

#### `weapon-coin-purse` · A purse of coins · P0

**Basis:** Edits `base-female`. **(derived)** From the `trader` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small cloth bag of coins, held up and jingled. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-coin-purse.png.

#### `weapon-feather-duster` · A feather duster · P0

**Basis:** Edits `base-female`. **(derived)** From the `maid` sheet.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a feather duster. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-feather-duster.png.

#### `weapon-brannock-1` · Brannock's shield, one name · P1

**Basis:** Edits `weapon-brannock`. From `03-art-overhaul.md` §6.4 (`brannock-shield`: pictures `0`, `1`, `41`, `42` names); A1-C6-03.

Edit the attached image (weapon-brannock.png): change ONLY the shield's rim: one name is freshly scratched into it, drawn as a short scratched mark that reads as a name from a distance, not as real letters. Keep everything else, the sword, the hands, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-brannock-1.png.

#### `weapon-brannock-41` · Brannock's shield, forty-one names · P1

**Basis:** Edits `weapon-brannock`. From `03-art-overhaul.md` §6.4; `07-companions.md` (Forty Letters).

Edit the attached image (weapon-brannock.png): change ONLY the shield's rim: it's crowded all the way round with forty-one short scratched names, drawn as marks that read as names from a distance, not as real letters. Keep everything else, the sword, the hands, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-brannock-41.png.

#### `weapon-brannock-42` · Brannock's shield, forty-two names · P2

**Basis:** Edits `weapon-brannock`. From `03-art-overhaul.md` §6.4; `COMPANIONS.md` (a forty-second: his own).

Edit the attached image (weapon-brannock.png): change ONLY the shield's rim: forty-one short scratched names all the way round, and one more freshly cut, drawn as marks that read as names from a distance, not as real letters. Keep everything else, the sword, the hands, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-brannock-42.png.

---

## 7. Characters as recipes

Every person and humanoid foe, as the parts they wear and their morph. The Helm / hair column is what's on the head, one slot: their hair, or a helm in its place. `—` is an empty slot. A variant id (`elara.act3`) is the same character at another point in the story; `swap` cues in the scripts switch between them. Names with ids in backticks in the notes are other entries.

| Id | Name | Face | Armor | Cape | Helm / hair | Weapon | Morph (h × w) | Priority | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `you-male` | You, the male default | `face-player-1` | `armor-traveller` | — | `hair-player-1` | `weapon-longsword` | 1.00 × 1.00 | P0 | the player swaps any slot on the creation screen |
| `you-female` | You, the female default | `face-player-2` | `armor-traveller-f` | — | `hair-player-2` | `weapon-longsword` | 1.00 × 1.00 | P0 |  |
| `soulbound` | {sb}, the default | `face-player-3` | `armor-sb-traveller` | `cape-sb` | `hair-player-3` | `weapon-sb-blades` | 1.02 × 1.00 | P0 | the player can change every slot |
| `you-male.champion` | You, Champion of Fable | `face-player-1` | `armor-champion` | — | `hair-player-1` | `weapon-longsword` | 1.00 × 1.00 | P1 | Act 1's end |
| `you-female.champion` | You, Champion of Fable | `face-player-2` | `armor-champion-f` | — | `hair-player-2` | `weapon-longsword` | 1.00 × 1.00 | P1 |  |
| `soulbound.champion` | {sb}, Champion of Fable | `face-player-3` | `armor-champion-f` | `cape-sb` | `hair-player-3` | `weapon-sb-blades` | 1.02 × 1.00 | P1 |  |
| `soulbound.lord-protector` | {sb}, Lord Protector | `face-player-3` | `armor-lord-protector` | — | `hair-player-3` | `weapon-longsword` | 1.02 × 1.00 | P2 | Act 2's end |
| `mc-child` | You, aged eight | `face-mc-child` | `armor-child-cove` | — | `hair-mc-child` | — | 0.62 × 0.66 | P0 |  |
| `sb-child` | {sb}, aged eight | `face-sb-child` | `armor-child-cove-f` | — | `hair-sb-child` | — | 0.62 × 0.66 | P0 |  |
| `sera` | Sera Vantis | `face-sera` | `armor-sera` | `cape-sera` | `hair-sera` | `weapon-sera-longbow` | 1.04 × 0.90 | P0 |  |
| `mira` | Mira Ashgrove | `face-mira` | `armor-mira` | — | `hair-mira` | `weapon-quarterstaff` | 1.01 × 0.90 | P0 | her healing glow is an effect |
| `mira-young` | Mira, aged thirteen | `face-mira-young` | `armor-mira-apprentice` | — | `hair-mira-young` | `weapon-sketchbook` | 0.80 × 0.80 | P1 | drawn as an echo |
| `kael` | Kael Duskmantle | `face-kael` | `armor-kael` | `cape-tail-ember` | `helm-kael-hood` | `weapon-kael-knives` | 1.00 × 0.95 | P0 | the horns draw over the hood |
| `brannock` | Brannock Ironvein | `face-brannock` | `armor-brannock` | — | `hair-brannock` | `weapon-brannock` | 0.85 × 1.36 | P0 | shield `weapon-brannock-1`, `-41`, `-42` as the names come |
| `brannock.clasps-gone` | Brannock, the clasps gone | `face-brannock-bare` | `armor-brannock` | — | — | `weapon-brannock-42` | 0.85 × 1.36 | P2 | from A3-C2-05 |
| `valencia` | Valencia Morvaine | `face-valencia` | `armor-valencia` | `cape-valencia` | `helm-circlet-silver` | `weapon-dagger` | 1.09 × 0.90 | P2 |  |
| `valencia.sister-vale` | Sister Vale | `face-valencia` | `armor-abbey-habit` | — | `helm-abbey-hood` | — | 1.09 × 0.90 | P2 | `valencia-unhooded`: the same without the helm, eyes red (an expression) |
| `corra` | Corra | `face-corra` | `armor-corra` | — | `hair-corra` | `weapon-netting-needle` | 0.95 × 1.00 | P0 |  |
| `corra.flashback` | Corra, sixteen years younger | `face-corra-flashback` | `armor-corra` | — | — | `weapon-netting-needle` | 0.95 × 1.00 | P0 | soaked by rain (an effect) |
| `lookout` | The lookout | `face-lookout` | `armor-sailor` | — | `helm-knit-cap` | `weapon-telescope` | 1.00 × 0.95 | P0 |  |
| `odelle` | Captain Odelle Marsh | `face-odelle` | `armor-odelle` | — | `helm-tricorn` | `weapon-cutlass` | 1.00 × 1.00 | P0 |  |
| `tomas` | Old Tomas | `face-tomas` | `armor-fisher` | — | `helm-knit-cap` | `weapon-gaff` | 0.95 × 1.00 | P0 |  |
| `red-gull` | Red Gull Thug | `face-red-gull` | `armor-red-gull` | — | `hair-red-gull` | `weapon-cudgel` | 1.08 × 1.15 | P0 | also the enemy `red-gull-thug` |
| `red-gull.b` | Red Gull Thug, second head | `face-red-gull-2` | `armor-red-gull` | — | `helm-bandana-red` | `weapon-cudgel` | 1.08 × 1.15 | P0 |  |
| `red-gull.c` | Red Gull Thug, third head | `face-red-gull-3` | `armor-red-gull` | — | `hair-red-gull-3` | `weapon-cudgel` | 1.06 × 1.12 | P0 |  |
| `grell` | Bosun Grell | `face-grell` | `armor-grell` | — | `hair-grell` | `weapon-cutlass` | 0.85 × 1.55 | P0 | also the enemy `bosun-grell` |
| `hale` | Captain Roderic Hale | `face-hale` | `armor-watch-captain` | `cape-watch-captain` | `helm-watch-crest` | `weapon-cudgel` | 1.03 × 1.14 | P0 |  |
| `watch` | City Watch | `face-town-14` | `armor-watch` | `cape-watch` | `helm-watch` | `weapon-watch-kit` | 1.00 × 1.00 | P0 | other watchmen: `face-town-3`, `face-town-11` |
| `fennick` | Questmaster Fennick | `face-fennick` | `armor-clerk` | — | `hair-fennick` | `weapon-tack-hammer` | 1.00 × 1.08 | P0 |  |
| `hedda` | Hedda (also `refugee-mother`) | `face-hedda` | `armor-refugee-woman` | `cape-blanket` | `hair-hedda` | `weapon-loaf` | 0.99 × 0.95 | P0 |  |
| `refugee-child` | Lise | `face-refugee-child` | `armor-refugee-child` | — | `hair-refugee-child` | `weapon-rag-doll` | 0.75 × 0.75 | P0 |  |
| `brisk` | Sergeant Brisk | `face-brisk` | `armor-watch` | `cape-watch` | `helm-watch` | `weapon-cudgel` | 1.00 × 1.06 | P0 |  |
| `refugee-1` | Old Aurek | `face-refugee-1` | `armor-refugee-man` | `cape-blanket` | `hair-refugee-1` | `weapon-walking-stick` | 0.93 × 0.95 | P0 |  |
| `refugee-2` | Teodor | `face-refugee-2` | `armor-refugee-man` | `cape-blanket` | `hair-refugee-2` | `weapon-tin-cup` | 0.95 × 0.90 | P0 |  |
| `refugee-3` | Yselle the Glassblower | `face-refugee-3` | `armor-glassblower` | — | `hair-refugee-3` | — | 0.82 × 1.25 | P0 |  |
| `elara` | Princess Elara | `face-elara` | `armor-elara` | — | `helm-circlet` | — | 1.08 × 0.88 | P0 | the slim sword is part of the armor |
| `elara-hooded` | Princess Elara, hooded | `face-elara` | `armor-elara-disguise` | `cape-elara-hooded` | `helm-hood-charcoal` | — | 1.08 × 0.88 | P0 |  |
| `leon` | King Leon | `face-leon` | `armor-leon` | `cape-leon` | `helm-crown` | — | 0.97 × 1.16 | P0 | his sword is part of the armor |
| `crane` | Lord Chancellor Crane | `face-crane` | `armor-crane` | — | `hair-crane` | `weapon-quill-letter` | 1.10 × 0.85 | P0 |  |
| `royal-messenger` | Royal Messenger | `face-town-9` | `armor-royal-livery` | `cape-royal` | `helm-beret` | `weapon-letter` | 0.99 × 0.95 | P0 |  |
| `royal-guard` | Royal Guard | `face-town-3` | `armor-royal-guard` | `cape-royal` | `helm-royal-guard` | `weapon-halberd` | 1.04 × 1.08 | P0 | the captain and lieutenant too |
| `anvil-barkeep` | Hessa the Barkeep | `face-anvil-barkeep` | `armor-tavern` | `cape-tail-bear` | `hair-anvil-barkeep` | `weapon-tankard` | 1.08 × 1.20 | P0 |  |
| `netty` | Netty the Net-Mender | `face-netty` | `armor-villager-woman` | — | `hair-netty` | `weapon-netting-needle` | 0.98 × 1.00 | P0 |  |
| `maud` | Maud Tolley | `face-maud` | `armor-elder-woman` | — | `hair-maud` | `weapon-flat-cap-held` | 0.93 × 0.95 | P0 |  |
| `pip` | Pip the Potboy | `face-pip` | `armor-tavern-m` | — | `hair-pip` | `weapon-stew-pot` | 0.72 × 0.80 | P0 |  |
| `osric` | Osric Penhallow | `face-osric` | `armor-merchant` | `cape-short-lavender` | `hair-osric` | `weapon-silk-bolt` | 1.00 × 1.00 | P0 |  |
| `lark` | Lark the Busker | `face-lark` | `armor-busker` | `cape-short-lavender` | `helm-beret` | `weapon-lute` | 1.04 × 0.90 | P0 |  |
| `ilsa` | Ilsa Fenwright | `face-ilsa` | `armor-villager-woman` | — | `hair-ilsa` | `weapon-scrolls` | 0.99 × 0.95 | P0 |  |
| `oswin` | Guardsman Oswin | `face-oswin` | `armor-royal-guard` | `cape-royal` | `helm-royal-guard` | `weapon-halberd` | 1.00 × 1.00 | P0 |  |
| `hesper` | Sister Hesper | `face-hesper` | `armor-temple` | — | `hair-hesper` | `weapon-broom` | 0.99 × 1.00 | P0 |  |
| `wren` | Wren | `face-wren` | `armor-urchin-f` | — | `hair-wren` | `weapon-coin` | 0.62 × 0.66 | P0 |  |
| `imra-vell` | Master Imra Vell | `face-imra-vell` | `armor-fencer` | — | `hair-imra-vell` | `weapon-practice-sword` | 0.99 × 1.00 | P0 | trainer |
| `tamsin` | Ranger Tamsin | `face-tamsin` | `armor-lodge-ranger` | `cape-tamsin` | `hair-tamsin` | `weapon-shortbow` | 1.00 × 1.00 | P0 | trainer; her hawk is the creature `lodge-hawk` |
| `garrick` | Garrick the caravan master | `face-garrick` | `armor-caravan` | — | `helm-wide-brim` | `weapon-light-crossbow` | 1.00 × 1.00 | P1 |  |
| `aldric` | Commander Aldric Voss | `face-aldric` | `armor-aldric` | `cape-campaign` | `hair-aldric` | `weapon-longsword` | 1.00 × 1.05 | P1 |  |
| `recruit` | The recruit | `face-recruit` | `armor-recruit` | — | `helm-kettle` | `weapon-spear` | 0.96 × 0.92 | P1 |  |
| `thessaly` | Archmage Thessaly Varn | `face-thessaly` | `armor-thessaly` | — | `hair-thessaly` | `weapon-staff-ring` | 1.00 × 1.00 | P1 | trainer |
| `thessaly.thessaly-crystal` | Thessaly, frozen in crystal | `face-thessaly` | `armor-thessaly` | — | `hair-thessaly` | `weapon-staff-ring` | 1.00 × 1.00 | P2 | the crystal round her is a prop, not a part |
| `brenna` | Brenna of Ashfall | `face-brenna` | `armor-brenna` | `cape-ash` | `hair-brenna` | `weapon-battleaxe` | 1.01 × 1.00 | P1 | trainer |
| `brenna.protectorate` | Brenna, Act 3 | `face-brenna` | `armor-brenna-protectorate` | `cape-ash` | `hair-brenna` | `weapon-battleaxe` | 1.01 × 1.00 | P2 | also her boss fight |
| `gannet` | Mother Gannet | `face-gannet` | `armor-gannet` | — | `helm-lace-cap` | `weapon-ledger-pen` | 0.99 × 1.20 | P1 |  |
| `hester` | Hester Hollis | `face-hester` | `armor-elder-woman` | — | `hair-hester` | `weapon-folded-shawl` | 0.90 × 0.90 | P1 |  |
| `tobin` | Tobin Ashgrove | `face-tobin` | `armor-cider-presser` | — | `hair-tobin` | `weapon-mallet` | 1.00 × 1.00 | P1 |  |
| `tobin.levy` | Tobin in the levy | `face-tobin` | `armor-levy` | — | `hair-tobin` | `weapon-spade` | 1.00 × 0.95 | P1 | how he looks when you meet him |
| `harrow` | Lord Harrow | `face-harrow` | `armor-harrow` | — | `hair-harrow` | `weapon-heron-cane` | 1.00 × 1.05 | P1 |  |
| `hobb` | Hobb the woodcutter | `face-hobb` | `armor-woodcutter` | — | `helm-knit-cap` | `weapon-felling-axe` | 1.08 × 1.15 | P1 |  |
| `maleficio` | Maleficio | `face-maleficio` | `armor-maleficio` | — | `hair-maleficio` | `weapon-black-box` | 1.01 × 0.95 | P1 |  |
| `maleficio-young` | Maleficio, remembered | `face-maleficio-young` | `armor-scholar` | — | — | `weapon-oil-lamp` | 1.00 × 1.00 | P1 | drawn as an echo |
| `relief-column-echo` | The relief column | `face-town-14` | `armor-old-pattern` | — | `helm-nasal` | `weapon-relief-kit` | 1.00 × 1.00 | P1 | drawn pale |
| `lost-boy` | The woodcutter's boy | `face-lost-boy` | `armor-urchin` | — | `hair-lost-boy` | `weapon-flat-cap-held` | 0.62 × 0.66 | P1 |  |
| `mags` | Pedlar Mags | `face-mags` | `armor-pedlar` | `cape-pedlar-pack` | `helm-straw-hat` | `weapon-walking-stick` | 0.82 × 1.25 | P1 | trainer; Duchess is a creature |
| `leon.ill` | King Leon, ill | `face-leon-ill` | `armor-leon-furs` | `cape-leon` | `helm-crown` | `weapon-walking-stick` | 0.95 × 1.10 | P2 |  |
| `elara.act3` | Elara, leading the resistance | `face-elara` | `armor-elara-field` | `cape-travelling` | `hair-elara` | `weapon-map` | 1.08 × 0.88 | P2 |  |
| `elara.crowned` | Elara, crowned | `face-elara` | `armor-elara-queen` | — | `helm-crown` | — | 1.08 × 0.88 | P2 |  |
| `hale.protectorate` | Captain Hale, Act 3 | `face-hale` | `armor-protectorate` | `cape-watch-captain` | `helm-watch-crest` | `weapon-cudgel` | 1.03 × 1.14 | P2 |  |
| `refugee-child.aged` | Lise, aged by the tear | `face-refugee-child-aged` | `armor-refugee-child` | — | — | — | 0.98 × 0.95 | P2 | the child's smock, tight on her |
| `anselle` | Mother Superior Anselle | `face-anselle` | `armor-anselle` | — | `hair-anselle` | `weapon-lantern` | 0.98 × 1.05 | P2 | trainer |
| `orla` | Sister Orla | `face-orla` | `armor-beekeeper` | — | `helm-bee-veil` | `weapon-smoker` | 0.98 × 1.10 | P2 | trainer |
| `corwin` | Sir Corwin Halbeck | `face-corwin` | `armor-corwin` | `cape-corwin` | `helm-corwin` | `weapon-corwin-kit` | 1.04 × 1.05 | P2 | trainer |
| `severin` | Lord Severin Morvaine | `face-severin` | `armor-severin` | `cape-severin` | `helm-crown-thorns` | — | 1.10 × 0.90 | P2 |  |
| `severin.severin-cell` | Severin, a prisoner | `face-severin-cell` | `armor-prisoner` | — | — | — | 1.10 × 0.85 | P2 |  |
| `isadora` | Lady Isadora | `face-isadora` | `armor-isadora` | — | `hair-isadora` | `weapon-night-flowers` | 1.00 × 0.95 | P2 | drawn as an echo |
| `valencia-child` | Valencia as a child | `face-valencia-child` | `armor-child-dress` | — | `hair-valencia-child` | — | 0.62 × 0.66 | P2 | drawn as an echo |
| `pell` | Pell | `face-pell` | `armor-morvaine-man` | — | `hair-pell` | `weapon-flat-cap-held` | 0.95 × 0.90 | P2 |  |
| `ide` | Ide | `face-ide` | `armor-shift` | — | `hair-ide` | — | 0.97 × 0.88 | P2 |  |
| `maren-elder` | Maren, Hollowmere's elder | `face-maren-elder` | `armor-morvaine-woman` | — | `hair-maren-elder` | `weapon-walking-stick` | 0.98 × 0.92 | P2 |  |
| `blood-house-steward` | The blood-house steward | `face-blood-house-steward` | `armor-morvaine-livery` | — | `hair-blood-house-steward` | `weapon-ledger-pen` | 1.00 × 0.92 | P2 |  |
| `hollowmere-child` | A Hollowmere child | `face-hollowmere-child` | `armor-morvaine-man` | — | `hair-hollowmere-child` | `weapon-toy-sword` | 0.62 × 0.66 | P2 |  |
| `hollowmere-farmer` | A Hollowmere farmer | `face-hollowmere-farmer` | `armor-morvaine-woman` | — | `helm-kerchief` | `weapon-basket` | 1.00 × 1.00 | P2 |  |
| `ruck` | Ruck, the bosun | `face-ruck` | `armor-sailor` | — | `hair-ruck` | `weapon-cutlass` | 0.95 × 1.10 | P2 | trainer |
| `davin` | Davin | `face-davin` | `armor-davin` | — | `hair-davin` | `weapon-corwin-sword` | 0.98 × 0.92 | P2 | trainer |
| `vorlaith` | Vorlaith, the Unmade King | `face-vorlaith` | `armor-vorlaith` | — | `helm-vorlaith` | — | 1.20 × 0.95 | P2 | `vorlaith-shape`: the same at 40% opacity |
| `guild-apprentice` | A Guild apprentice | `face-guild-apprentice` | `armor-guild-apprentice` | — | `hair-guild-apprentice` | `weapon-glass-lamp` | 0.94 × 0.88 | P2 |  |
| `mason` | A mason | `face-town-12` | `armor-smith` | — | `helm-flat-cap` | `weapon-mallet` | 1.00 × 1.05 | P2 |  |
| `elder` | The Elder (sheet) | `face-town-5` | `armor-elder-coat` | — | `hair-town-5` | `weapon-walking-stick` | 0.97 × 0.95 | P0 |  |
| `trader` | The Trader (sheet) | `face-town-2` | `armor-trader` | — | `helm-wide-brim` | `weapon-coin-purse` | 1.00 × 1.00 | P0 | also the Chandler |
| `baker` | The Baker (sheet) | `face-town-4` | `armor-baker` | — | `helm-kerchief` | `weapon-peel` | 0.98 × 1.10 | P0 | also the Innkeeper |
| `smith` | The Smith (sheet) | `face-town-3` | `armor-smith` | — | `hair-town-3` | `weapon-hammer-tongs` | 1.00 × 1.10 | P0 |  |
| `flower-seller` | The Flower Seller (sheet) | `face-town-2` | `armor-villager-woman` | — | `helm-kerchief` | `weapon-flower-basket` | 0.98 × 0.92 | P0 |  |
| `traveller` | A Traveller (sheet) | `face-town-14` | `armor-road` | `cape-travelling` | `hair-town-14` | `weapon-walking-stick` | 1.00 × 1.00 | P0 | also the Harbourmaster, a Regular |
| `seamstress` | The Seamstress (sheet) | `face-town-10` | `armor-villager-woman` | — | `hair-town-10` | — | 0.98 × 0.92 | P0 |  |
| `courier` | A Courier (sheet) | `face-town-9` | `armor-courier` | — | `helm-flat-cap` | `weapon-letters` | 0.96 × 0.90 | P0 | also `COURIER` in Act 3 |
| `pilgrim` | A Pilgrim (sheet) | `face-town-13` | `armor-pilgrim` | — | `hair-town-13` | `weapon-pilgrim-staff` | 1.00 × 1.00 | P0 | also `PILGRIM` |
| `weaver` | The Weaver (sheet) | `face-town-4` | `armor-villager-woman` | — | `hair-town-4` | `weapon-spindle` | 1.00 × 1.00 | P0 |  |
| `farmhand` | A Farmhand (sheet) | `face-town-1` | `armor-villager-man` | — | `helm-straw-hat` | `weapon-pitchfork` | 1.00 × 1.00 | P0 | also a Dock Hand, `FARMER` |
| `old-man` | An Old Man (sheet) | `face-town-5` | `armor-elder-coat` | — | `hair-town-5` | `weapon-walking-stick` | 0.94 × 1.00 | P0 |  |
| `maid` | A Maid (sheet) | `face-town-10` | `armor-maid` | — | `hair-town-10` | `weapon-feather-duster` | 0.97 × 0.92 | P0 |  |
| `porter` | A Porter (sheet) | `face-town-11` | `armor-porter` | — | `hair-town-11` | `weapon-crate` | 1.00 × 1.08 | P0 | also a Carrier, a Warehand |
| `old-woman` | An Old Woman (sheet) | `face-town-6` | `armor-elder-woman` | `cape-old-woman` | `hair-town-6` | `weapon-basket` | 0.90 × 0.95 | P0 | also a Drinker |
| `child` | A Child (sheet) | `face-town-7` | `armor-urchin` | — | `helm-flat-cap` | `weapon-toy-sword` | 0.62 × 0.66 | P0 |  |
| `youth` | A Youth (sheet) | `face-town-9` | `armor-villager-man` | — | `hair-town-9` | — | 0.90 × 0.88 | P0 |  |
| `pool-questeria` | The city pool (sample) | `face-town-4` | `armor-villager-woman` | — | `helm-kerchief` | `weapon-basket` | 1.00 × 1.00 | P0 | the generator mixes the town faces and armors by seed |
| `pool-court` | The court (sample: a courtier) | `face-town-16` | `armor-noble` | — | `helm-beret` | — | 1.00 × 0.95 | P0 | the courtiers and the scribe in the throne room |
| `pool-court-lady` | The court (sample: a lady) | `face-town-10` | `armor-noble-gown` | — | `hair-town-10` | — | 0.98 × 0.92 | P0 |  |
| `pool-saltwind` | The docks pool (sample) | `face-town-12` | `armor-fisher` | — | `helm-knit-cap` | — | 1.00 × 1.05 | P0 | also `NET-MENDER`, `SAILOR` |
| `pool-doomward` | The Doomward pool (sample) | `face-town-14` | `armor-refugee-man` | `cape-blanket` | `hair-town-14` | `weapon-walking-stick` | 1.00 × 0.95 | P0 |  |
| `pool-morvaine` | The Morvaine pool (sample) | `face-town-18` | `armor-morvaine-woman` | — | `hair-town-18` | `weapon-lantern` | 1.00 × 0.92 | P2 |  |
| `pool-abbey` | The abbey pool (sample) | `face-town-15` | `armor-abbey-lay` | — | `helm-straw-hat` | `weapon-basket` | 0.72 × 0.80 | P2 |  |
| `red-gull-thug` | Red Gull Thug | `face-red-gull` | `armor-red-gull` | — | `hair-red-gull` | `weapon-cudgel` | 1.08 × 1.15 | P0 | the same as `red-gull`, with its two other heads |
| `bosun-grell` | Bosun Grell | `face-grell` | `armor-grell` | — | `hair-grell` | `weapon-cutlass` | 0.85 × 1.55 | P0 | the same as `grell` |
| `hollowed-footman` | Hollowed Footman | `face-hollowed` | `armor-hollowed-footman` | — | `helm-hollowed-kettle` | `weapon-hollowed-sword` | 0.97 × 1.00 | P0 | hunched (a clip) |
| `hollowed-hexer` | Hollowed Hexer | `face-hollowed` | `armor-hollowed-hexer` | — | `helm-hollowed-hood` | `weapon-hexer-staff` | 1.01 × 0.90 | P0 |  |
| `hollowed-sapper` | Hollowed Sapper | `face-hollowed` | `armor-hollowed-sapper` | — | `helm-sapper-goggles` | `weapon-sapper-pick` | 0.95 × 1.05 | P0 |  |
| `sapper-chief` | Sapper Chief | `face-hollowed` | `armor-hollowed-chief` | — | `helm-sapper-faceplate` | `weapon-chief-pick-lantern` | 1.06 × 1.20 | P0 |  |
| `vault-construct` | Vault Construct | `face-construct-vault` | `armor-construct-stone` | — | — | — | 1.20 × 1.35 | P0 |  |
| `kael-duel` | Kael, duelling | `face-kael` | `armor-kael` | `cape-tail-ember` | `helm-kael-hood` | `weapon-kael-rapier` | 1.00 × 0.95 | P0 |  |
| `hollowed-climber` | Hollowed Climber | `face-hollowed` | `armor-hollowed-climber` | — | — | `weapon-climber-hook` | 1.02 × 0.88 | P1 | crouched (a clip) |
| `varkhul` | Varkhul, the Hollow Knight | `face-varkhul` | `armor-varkhul` | `cape-varkhul` | `helm-varkhul` | `weapon-varkhul-greatsword` | 1.20 × 1.25 | P1 | boss |
| `varkhul.varkhul-unmasked` | Varkhul, unmasked | `face-varkhul` | `armor-varkhul-split` | `cape-varkhul` | `hair-varkhul` | — | 1.20 × 1.25 | P1 | kneels (a clip) |
| `maleficio-boss` | Maleficio, the Sovereign | `face-maleficio` | `armor-maleficio-ascendant` | — | `hair-maleficio` | `weapon-black-box` | 1.01 × 0.95 | P1 | floats (the game lifts it); swaps to the creature `hungering` |
| `cube-cultist` | Cube Cultist | `face-cube-cultist` | `armor-cultist` | — | `hair-cube-cultist` | `weapon-cultist-dagger` | 1.00 × 1.00 | P1 |  |
| `bandit` | Bandit | `face-town-11` | `armor-bandit` | — | `helm-bandit-hood` | `weapon-shortsword` | 1.00 × 1.00 | P1 | a new id |
| `thrall` | Thrall | `face-thrall` | `armor-thrall` | — | `hair-thrall` | `weapon-spear-lantern` | 1.00 × 1.00 | P2 |  |
| `vesk` | Vesk | `face-thrall` | `armor-thrall-knight` | `cape-thrall-knight` | `helm-vesk-wolf` | `weapon-vesk-sword` | 1.08 × 1.15 | P2 |  |
| `vreya` | Vreya | `face-vreya` | `armor-thrall-knight-f` | `cape-thrall-knight` | `helm-vreya-crown` | `weapon-vreya-blade` | 1.06 × 1.00 | P2 |  |
| `mordain` | Mordain | `face-thrall` | `armor-thrall-knight` | `cape-thrall-knight` | `helm-mordain-beak` | `weapon-mordain-glaive` | 1.08 × 0.90 | P2 |  |
| `gilded-knight` | Gilded Knight | `face-town-14` | `armor-gilded-knight` | `cape-gilded` | `helm-gilded` | `weapon-gilded-kit` | 1.02 × 1.05 | P2 | the `order` pool |
| `gilded-sergeant` | Gilded Sergeant | `face-town-12` | `armor-gilded-knight` | `cape-gilded` | `hair-town-12` | `weapon-mace-shield` | 1.00 × 1.12 | P2 |  |
| `drowned` | Drowned | `face-drowned` | `armor-drowned` | — | `hair-drowned` | — | 0.97 × 1.05 | P2 |  |
| `blood-wight` | Blood-wight | `face-blood-wight` | `armor-blood-wight` | — | — | — | 1.10 × 0.85 | P2 |  |
| `sun-scorched` | Sun-scorched | `face-sun-scorched` | `armor-sun-scorched` | — | — | — | 1.00 × 1.00 | P2 |  |
| `thornling` | Thornling | `face-thornling` | `armor-thornling` | — | — | — | 0.68 × 0.70 | P2 |  |
| `priest-construct` | Priest-construct | `face-construct-priest` | `armor-construct-priest` | — | — | — | 1.20 × 1.30 | P2 |  |
| `severin-boss` | Severin, unbound | `face-severin` | `armor-severin` | `cape-severin-wings` | `helm-crown-thorns` | — | 1.10 × 0.90 | P2 | floats; claws and blood magic are effects |
| `smuggler` | Smuggler | `face-town-10` | `armor-smuggler` | — | `helm-bandana` | `weapon-cutlass` | 1.00 × 1.00 | P2 | a new id |
| `protectorate-watchman` | Protectorate watchman | `face-town-11` | `armor-protectorate` | `cape-watch` | `helm-watch` | `weapon-protectorate-kit` | 1.00 × 1.00 | P2 | also `PROTECTORATE GUARD`; the `protectorate` pool |
| `protectorate-warden` | Protectorate warden | `face-town-3` | `armor-protectorate-warden` | — | `helm-warden` | `weapon-warden-kit` | 1.08 × 1.18 | P2 |  |
| `crown-hollowed` | Crown-raised Hollowed | `face-crown-hollowed` | `armor-crown-hollowed` | — | `helm-hollowed-kettle` | `weapon-spear` | 1.00 × 1.00 | P2 |  |
| `crown-construct` | Crown-construct | `face-construct-crown` | `armor-construct-crown` | — | — | — | 1.25 × 1.20 | P2 |  |
| `kael-shadow` | Kael, the Protectorate Shadow | `face-kael` | `armor-kael-shadow` | `cape-tail-ember` | `hair-kael` | `weapon-kael-knives` | 1.00 × 0.95 | P2 | boss |
| `sb-sovereign` | {sb}, the Sovereign Protector | `face-player-3` | `armor-sovereign` | — | `helm-crown-dusk-dawn` | `weapon-longsword` | 1.02 × 1.00 | P2 | the final boss; the player's own face and look; floats in phase 2 |
| `unmade` | The Unmade (ancient) | `face-unmade` | `armor-unmade-bronze` | — | `helm-bronze-crest` | `weapon-unmade-spear-shield` | 1.00 × 1.00 | P2 |  |
| `unmade.b` | The Unmade (a knight) | `face-unmade` | `armor-old-pattern` | — | `helm-nasal` | `weapon-longsword` | 1.00 × 1.00 | P2 | the game greys it |
| `unmade.c` | The Unmade (a pikeman) | `face-unmade` | `armor-pikeman` | — | `helm-morion` | `weapon-pike` | 1.00 × 1.00 | P2 |  |
| `unmade.d` | The Unmade (a watchman) | `face-unmade` | `armor-watch` | `cape-watch` | `helm-watch` | `weapon-cudgel` | 1.00 × 1.00 | P2 | the game greys it |
| `ashfall-raider` | Ashfall raider | `face-town-12` | `armor-ashfall-raider` | `cape-ash` | `hair-town-12` | `weapon-raider-kit` | 1.00 × 1.00 | P2 |  |

### 7.1 Gear

What each item in `Items/ItemCatalog.cs` puts on whoever's wearing it, on each base. `tools/import_parts.py` reads this into `parts.json`, and equipping an item swaps the part in its slot (`PartCatalog.Dress`). The body armour replaces the armor; with nothing on the body, a character wears their recipe's armor, which is their clothes. A cloak replaces the cape and a head item the helm; with nothing worn there, the recipe's own cape or helm shows. A hand with nothing in it holds nothing. Gloves, boots, amulets and rings draw nothing: the armor already has its hands and feet. An item whose part isn't painted yet draws nothing, and body armour that isn't painted yet leaves the clothes on.

| Item | Slot | On the man | On the woman |
| --- | --- | --- | --- |
| `padded-armour` | body | `armor-padded` | `armor-padded-f` |
| `leather-armour` | body | `armor-leather` | `armor-leather-f` |
| `studded-leather` | body | `armor-studded` | `armor-studded-f` |
| `hide-armour` | body | `armor-hide` | `armor-hide-f` |
| `chain-shirt` | body | `armor-chain-shirt` | `armor-chain-shirt-f` |
| `scale-mail` | body | `armor-scale` | `armor-scale-f` |
| `chain-mail` | body | `armor-chain-mail` | `armor-chain-mail-f` |
| `plate-armour` | body | `armor-plate` | `armor-plate-f` |
| `mage-robes` | body | `armor-mage-robes` | `armor-mage-robes-f` |
| `travelling-cloak` | cloak | `cape-travelling` | `cape-travelling` |
| `cloak-of-protection` | cloak | `cape-protection` | `cape-protection` |
| `leather-cap` | head | `helm-leather-cap` | `helm-leather-cap` |
| `iron-helm` | head | `helm-iron-helm` | `helm-iron-helm` |
| `circlet-of-focus` | head | `helm-circlet-of-focus` | `helm-circlet-of-focus` |
| `dagger` | main hand | `weapon-dagger` | `weapon-dagger` |
| `shortsword` | main hand | `weapon-shortsword` | `weapon-shortsword` |
| `longsword` | main hand | `weapon-longsword` | `weapon-longsword` |
| `rapier` | main hand | `weapon-rapier` | `weapon-rapier` |
| `scimitar` | main hand | `weapon-scimitar` | `weapon-scimitar` |
| `handaxe` | main hand | `weapon-handaxe` | `weapon-handaxe` |
| `battleaxe` | main hand | `weapon-battleaxe` | `weapon-battleaxe` |
| `greatsword` | main hand | `weapon-greatsword` | `weapon-greatsword` |
| `warhammer` | main hand | `weapon-warhammer` | `weapon-warhammer` |
| `mace` | main hand | `weapon-mace` | `weapon-mace` |
| `quarterstaff` | main hand | `weapon-quarterstaff` | `weapon-quarterstaff` |
| `spear` | main hand | `weapon-spear` | `weapon-spear` |
| `shortbow` | main hand | `weapon-shortbow` | `weapon-shortbow` |
| `longbow` | main hand | `weapon-longbow` | `weapon-longbow` |
| `light-crossbow` | main hand | `weapon-light-crossbow` | `weapon-light-crossbow` |
| `ember-dagger` | main hand | `weapon-ember-dagger` | `weapon-ember-dagger` |
| `thornwood-bow` | main hand | `weapon-thornwood-bow` | `weapon-thornwood-bow` |
| `sunsteel-blade` | main hand | `weapon-sunsteel-blade` | `weapon-sunsteel-blade` |
| `dagger` | off hand | `offhand-dagger` | `offhand-dagger` |
| `shortsword` | off hand | `offhand-shortsword` | `offhand-shortsword` |
| `scimitar` | off hand | `offhand-scimitar` | `offhand-scimitar` |
| `handaxe` | off hand | `offhand-handaxe` | `offhand-handaxe` |
| `ember-dagger` | off hand | `offhand-ember-dagger` | `offhand-ember-dagger` |
| `wooden-shield` | off hand | `offhand-wooden-shield` | `offhand-wooden-shield` |
| `steel-shield` | off hand | `offhand-steel-shield` | `offhand-steel-shield` |
| `warden-shield` | off hand | `offhand-warden-shield` | `offhand-warden-shield` |

### 7.2 The party's own gear

A companion's starting kit is drawn as their own pieces, not the shop's: Sera's shortbow is her longbow, and Brannock's longsword and shield are the one piece. These rows win over §7.1 while that character wears that item in that slot, on their own recipe; `—` draws nothing there, because another piece already holds it. Everyone starts in a travelling cloak (the class kits' basics, `Rules/ClassCatalog.cs`); the Soul-Bound's, Sera's and Kael's draws as their own cape or tail, so it's their recipe's piece that shows (added 2026-11-09, when the first cape test showed the plain cloak hiding the Soul-Bound's). Anything else they put on draws as §7.1 says.

| Who | Slot | Item | Part |
| --- | --- | --- | --- |
| `soulbound` | main hand | `shortsword` | `weapon-sb-blades` |
| `soulbound` | off hand | `dagger` | — |
| `soulbound` | body | `leather-armour` | `armor-sb-traveller` |
| `soulbound` | cloak | `travelling-cloak` | `cape-sb` |
| `sera` | main hand | `shortbow` | `weapon-sera-longbow` |
| `sera` | body | `leather-armour` | `armor-sera` |
| `sera` | cloak | `travelling-cloak` | `cape-sera` |
| `kael` | main hand | `dagger` | `weapon-kael-knives` |
| `kael` | off hand | `dagger` | — |
| `kael` | body | `leather-armour` | `armor-kael` |
| `kael` | cloak | `travelling-cloak` | `cape-tail-ember` |
| `mira` | body | `scale-mail` | `armor-mira` |
| `brannock` | main hand | `longsword` | `weapon-brannock` |
| `brannock` | off hand | `wooden-shield` | — |
| `brannock` | body | `chain-mail` | `armor-brannock` |

---

## 8. Creatures

The foes and animals that aren't people keep full-figure prompts: they don't use the base. Paste the style block, the avoid list and the prompt in a new chat for each.

#### `sea-wraith` · Sea-wraith · P0

**Basis:** From the art plan's §4 (a drowned sailor from the ribs up, below that a twisting tail of seawater; long arms with webbed claws; kelp-lock hair; a seawater body; solid glowing eyes; drips; it hovers); the guide's masked figure (the gaunt, mask-like face); `Creatures.cs`. It's a layered sprite in the game, not rigged (`03-art-overhaul.md` §9).

A sea-wraith, the ghost of a drowned sailor that climbs up over a ship's rail: from the ribs up, the gaunt body of a drowned man, and below the ribs, no legs, only a long tail of twisting seawater that curls down to a thin, splashing point. Its skin and body are translucent sea-blue shading to pale turquoise at the edges, so you can almost see through it, with a few bubbles caught inside. Face: hollow and mask-like, a gaping mouth, no nose, two solid glowing eyes of pale sea-green light. Hair: long, dripping locks of dark-green kelp. Clothes: the rotted rags of a sailor's striped shirt clinging to its chest and shoulders, a frayed rope round one wrist. Long, thin arms ending in webbed, clawed hands, reaching forward. Water drips from its fingers and chin. Mood: hungry, mournful, reaching. Output: PNG, 1024×1536, on a solid black background. One creature, centred, the tip of its water-tail near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: from the top of its head to the tip of its tail, about 80% of the canvas height; it floats a little above the deck. An original design. I'll save it as sea-wraith.png.

#### `gloomfang` · Gloomfang Wolf · P0

**Basis:** From the guide sheet's creature panel (a gaunt, spiky dark wolf-beast with a ragged mane, bared teeth and long claws); the art plan (a lean wolf, navy-grey fur drawn as locks along the spine and ruff, pale violet eyes trailing black smoke, three seeded fur shades); `EnemyCatalog.cs` ("black smoke leaking from their eyes").

A gloomfang, a forest wolf twisted by dark magic: a gaunt, rangy wolf, long-legged and narrow-hipped, its ribs showing under its coat, standing in a low, stalking stance with its head down and forward, in a three-quarter side view facing right. Fur: dark grey-brown with a cold navy cast, a paler, dusty underside, drawn as clumped, spiky locks that bristle into a ragged mane along the neck, shoulders and spine. Head: a long, narrow muzzle with the lips drawn back over long yellowed fangs; ears flat back; pale violet eyes with thin trails of black smoke leaking from them and drifting back over the head. Long, dark claws on big feet. A thin, ragged tail held low. Mood: hungry, silent, about to lunge. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game its shoulder comes to a standard adult's hip, and nose to tail it's a little longer than an adult is tall. An original design. I'll save it as gloomfang.png.

#### `gloomfang.b` · Gloomfang Wolf, second coat · P0

**Basis:** As `gloomfang`; the art plan (three seeded fur shades).

A gloomfang, a forest wolf twisted by dark magic: a gaunt, rangy wolf, long-legged and narrow-hipped, its ribs showing under its coat, standing in a low, stalking stance with its head down and forward, in a three-quarter side view facing right. Fur: almost black, with a grizzled grey muzzle and a torn ear, drawn as clumped, spiky locks that bristle into a ragged mane along the neck, shoulders and spine. Head: a long, narrow muzzle with the lips drawn back over long yellowed fangs; ears flat back; pale violet eyes with thin trails of black smoke leaking from them and drifting back over the head. Long, dark claws on big feet. A thin, ragged tail held low. Mood: hungry, silent, about to lunge. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game its shoulder comes to a standard adult's hip, and nose to tail it's a little longer than an adult is tall. An original design. I'll save it as gloomfang.b.png.

#### `gloomfang.c` · Gloomfang Wolf, third coat · P0

**Basis:** As `gloomfang`; the art plan (three seeded fur shades).

A gloomfang, a forest wolf twisted by dark magic: a gaunt, rangy wolf, long-legged and narrow-hipped, its ribs showing under its coat, standing in a low, stalking stance with its head down and forward, in a three-quarter side view facing right. Fur: brown-grey with darker patches and a scarred flank, a paler underside, drawn as clumped, spiky locks that bristle into a ragged mane along the neck, shoulders and spine. Head: a long, narrow muzzle with the lips drawn back over long yellowed fangs; ears flat back; pale violet eyes with thin trails of black smoke leaking from them and drifting back over the head. Long, dark claws on big feet. A thin, ragged tail held low. Mood: hungry, silent, about to lunge. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game its shoulder comes to a standard adult's hip, and nose to tail it's a little longer than an adult is tall. An original design. I'll save it as gloomfang.c.png.

#### `thornmother` · The Thornmother · P0

**Basis:** From `03-art-overhaul.md` §9 (the quadruped rig at 2.2 with a hump); the art plan (a near-black she-bear; black crystal clusters edged with a glow break her silhouette at the hump and shoulder; thorns pierce her hide; violet veins); the guide alignment (crystals in muted violet, not neon); `Creatures.cs` (a giant she-bear gone wrong). A boss.

The Thornmother, a giant she-bear corrupted by a black crystal: a huge, heavy bear with a high, humped back, standing on all fours in a three-quarter side view facing right, head low and swinging toward the viewer. Fur: near-black and shaggy, a dusky brown muzzle, the coat matted and torn. Clusters of jagged black crystal grow out of her hump and her near shoulder, breaking the outline of her body, each crystal edged with a soft, muted violet glow; thin glowing violet veins spread out from them under her skin. Long, dark, woody thorns pierce out through her hide along her back, flanks and forelegs, some as long as a man's arm. Head: a broad skull, small burning violet eyes, the jaws open on long teeth, thorns round the jaw. Huge paws with long pale claws. Mood: in pain and furious, defending her den. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 90% of the canvas width; in the game she stands half again as tall as a standard adult at the hump, and twice as long. An original design. I'll save it as thornmother.png.

#### `thornmother.rearing` · The Thornmother, rearing · P0

**Basis:** As `thornmother`; the art plan (`rear` clip, 420 px rearing).

The Thornmother, a giant she-bear corrupted by a black crystal, rearing up on her hind legs in a three-quarter view facing right, forepaws raised and claws spread, roaring. Fur: near-black and shaggy, a dusky brown muzzle, the coat matted and torn. Clusters of jagged black crystal grow out of her hump and her near shoulder, each edged with a muted violet glow that flares brighter now, thin glowing violet veins spreading under her skin; long, dark, woody thorns pierce her hide along her back, flanks and forelegs. Small burning violet eyes, long teeth, huge paws with long pale claws. Mood: towering fury. Output: PNG, 1024×1536 portrait, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 90% of the canvas height; in the game she's over twice a standard adult's height rearing. An original design. I'll save it as thornmother.rearing.png.

#### `ram-beast` · The Hollowed ram beast · P1

**Basis:** **(derived)** From `01-build-plan.md` §9 (a mini-boss that breaks the outer gate); `03-art-overhaul.md` §9 (a Hollowed beast, a four-frame sheet); the art plan (the quadruped rig at 1.8); the guide sheet's flesh mutant in chains.

A Hollowed ram beast, a huge siege animal raised from the dead to break gates: a massive, heavy, four-legged beast like a giant ram crossed with a bull, its hide ash grey and cracked with glowing ember lines, standing in a three-quarter side view facing right with its head lowered to charge. Its head is armoured with a riveted iron battering cap and two enormous curled, chipped horns. Rusted chains wrap its body and drag from iron collars; scraps of iron plate are bolted to its shoulders and flanks; broken spears stick out of its back. Small ember-orange eyes. Thick legs ending in cloven hooves shod with iron. Mood: blind fury, about to charge. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 90% of the canvas width; in the game it stands about as tall as a standard adult at the shoulder, and twice as long. An original design. I'll save it as ram-beast.png.

#### `hungering` · The Hungering · P1

**Basis:** From `04-script-prologue-act1.md` A1-C8-03 ("The Box opens. It pours into him, and then it pulls, and he folds into it"); the art plan (the wisp rig at ×3); the guide sheet's hulking flesh mutant; `03-art-overhaul.md` §9 (a layered sprite that moves as a whole). Survive, not kill.

The Hungering, what's left of a scholar after a cursed black cube devoured him: a towering, hunched mass three times a man's height, made of shredded charcoal robes and ash-grey flesh folding in on itself, with no legs, its base a ragged, smoking skirt of tatters drifting above the ground. Many long, thin, grasping arms reach out from its body, some human, some wrong. In the middle of its chest, the stretched, sorrowful face of an old man, eyes closed. At its heart, where a heart would be, a perfect black square void with violet light bleeding from its edges, pulling everything in: the edges of the creature curl toward it. Ember-orange cracks and sparks across its surface. Mood: endless, mindless hunger. Output: PNG, 1024×1536, on a solid black background. One creature, centred, its base near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 92% of the canvas height; in the game it's three times a standard adult's height. An original design. I'll save it as hungering.png.

#### `rot-boar` · Rot-boar · P1

**Basis:** From `03-art-overhaul.md` §9 (a bristled boar, black veins under the hide, red eyes; the quadruped rig, stout); `08-random-encounters.md` (`enc-rot-boars`).

A rot-boar, a wild boar sickened by a corrupted forest: a heavy, bristle-backed boar, low and deep-chested, in a three-quarter side view facing right, head down. Coarse dark-brown bristles stand up in a crest along its spine; under the patchy hide, black veins spread like cracks over its shoulders and face. Small, bloodshot red eyes; long, yellowed, chipped tusks; froth at the mouth; a short tufted tail; mud to the knees. Mood: maddened, about to charge. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game its back comes to a standard adult's hip. An original design. I'll save it as rot-boar.png.

#### `ash-hound` · Ash-hound · P1

**Basis:** From `03-art-overhaul.md` §9 (a grey dog the colour of the ground, ember eyes; the gloomfang's skeleton); `08-random-encounters.md` (`enc-ash-hounds`: "You only see them when they move").

An ash-hound, a feral dog of a burned wasteland: a lean, deep-chested hound with long legs and a narrow head, in a three-quarter side view facing right, low and stalking. Its short, dusty fur is exactly the grey of ash and cinders, mottled like burned ground, with a sooty muzzle; a few patches of fur have burned away to cracked grey skin with faint ember-orange lines. Eyes: glowing ember orange. Ears: pointed, one torn. A thin, whip-like tail. Ash dust puffs from its paws. Mood: patient, hungry, hunting as a pack. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game its shoulder comes to a standard adult's hip. An original design. I'll save it as ash-hound.png.

#### `dire-bat` · Dire bat · P2

**Basis:** From `03-art-overhaul.md` §9 (a wingspan of a tall man, ragged ears; the flier rig).

A dire bat, a huge bat of a dark valley: wings spread wide in mid-flight, seen in a three-quarter view from the side and a little below, facing right. Body: about the size of a large dog, covered in dark-brown, matted fur. Wings: leathery, dark grey-brown membranes, torn and ragged at the edges, the finger bones clearly visible. Head: a pig-like snout, a mouth full of needle teeth, small red eyes, and very large, ragged, notched ears. Clawed feet tucked up. Mood: screeching, diving. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, centred, nothing cropped (the whole of both wings). No text, no frame, no ground shadow. Scale: fill about 90% of the canvas width; in the game its wingspan is the height of a tall man. An original design. I'll save it as dire-bat.png.

#### `tithe-rider` · Tithe rider · P2

**Basis:** From `03-art-overhaul.md` §9 (a Morvaine rider in dark wool on a black horse; a person on a quadruped's saddle); `08-random-encounters.md` (`enc-tithe-riders`: the rider holds out a tithe cup at the new moon).

A tithe rider, a vampire lord's collector of blood, on horseback: a human rider of about forty in dark wool, sitting straight on a tall black horse, both seen in a three-quarter side view facing right. Rider: a pale, hard face with cold grey eyes, a black beard trimmed close; a long, high-collared riding coat of dark charcoal wool, a wine-red sash, black gloves, a black wide-brimmed hat; in the near hand, held out, a small pewter cup; a sheathed sabre at the hip. Horse: a tall, lean black horse with a long mane, red-glinting eyes, black tack with pewter fittings, a wine-red saddle cloth, and its breath steaming. Mood: polite, patient and pitiless. Output: PNG, 1536×1536, on a solid black background. One horse and rider, centred, the hooves near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 88% of the canvas height; in the game the horse's shoulder is at a standard adult's shoulder, and the rider's head is well above a standing man's. An original design. I'll save it as tithe-rider.png.

#### `rockjaw` · Rockjaw · P2

**Basis:** From `03-art-overhaul.md` §9 (a boulder with a jaw; moss on its back; the quadruped rig, stout); `08-random-encounters.md` (`enc-rockjaws`: "The scree moves. Then it stands up.").

A rockjaw, a creature that looks like a boulder until it stands up: a round, heavy body of grey granite with moss and small ferns growing on its back, on four short, thick, stony legs, in a three-quarter side view facing right. The front of the boulder splits open into a huge, wide jaw of jagged stone teeth, with a small pair of glowing amber eyes set deep above it. Pebbles and grit trickle from its joints. Mood: grumpy, territorial, about to bite. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 80% of the canvas width; in the game its back comes to a standard adult's chest. An original design. I'll save it as rockjaw.png.

#### `glass-scorpion` · Glass scorpion · P2

**Basis:** From `03-art-overhaul.md` §9 (a red glass shell, a sun-bright sting; the arthropod rig).

A glass scorpion, a desert creature with a body like stained glass: a large scorpion in a three-quarter side view facing right, its tail curled high over its back. Its shell is translucent dark-red glass with lighter red edges where the light passes through, the segments outlined in darker lines like leading. Two big pincers raised. Six thin, jointed legs. At the tip of the tail, a sting that glows sun-bright gold-white, as if a spark of the sun were trapped in it. Sand pours off its back. Mood: clicking, alert, poised to strike. Output: PNG, 1536×1024 landscape, on a solid black background. One creature, the whole body centred with its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 85% of the canvas width; in the game it's as long as a standard adult is tall, and knee-high. An original design. I'll save it as glass-scorpion.png.

#### `lodge-hawk` · The Lodge's hawk · P0

**Basis:** From `10-classes.md` (`train-tamsin`: the hawk on Tamsin's shoulder; Lodge Ranger's Hawk Strike); the flier rig. Its look is (derived).

A trained hawk of a forest rangers' lodge: a sleek goshawk in a three-quarter side view facing right, wings half open as it lands, talons forward. Plumage: slate-grey back and wings, a white chest finely barred with grey, a pale stripe over a fierce orange eye, a hooked dark beak, yellow feet. A leather jess with a small brass bell on one leg. Output: PNG, 1536×1024 landscape, on a solid black background. One bird, centred, nothing cropped (both wingtips). No text, no frame, no ground shadow. Scale: fill about 80% of the canvas width; in the game its wingspan is about the width of a standard adult's shoulders. An original design. I'll save it as lodge-hawk.png.

#### `burr` · Burr, Sera's kestrel · P1

**Basis:** From `COMPANIONS.md` (a grey-winged kestrel called Burr, usually off scouting); `08-random-encounters.md` (`enc-burr`: he drops onto Sera's glove with something in his beak); `03-art-overhaul.md` §9 (the flier rig, small). Its plumage is (derived).

Burr, a small falcon, a grey-winged kestrel: in a three-quarter side view facing right, wings spread as he comes in to land, tail fanned. Plumage: blue-grey head and wings, a rust-red back spotted with black, a cream chest with dark speckles, a black band at the tip of the grey tail, a dark "tear" mark under the eye; bright dark eyes; yellow feet. He holds a small old brass key in his beak. Mood: smug. Output: PNG, 1536×1024 landscape, on a solid black background. One bird, centred, nothing cropped (both wingtips). No text, no frame, no ground shadow. Scale: fill about 80% of the canvas width; in the game he's a small bird, his wingspan about the length of a standard adult's arm. An original design. I'll save it as burr.png.

#### `duchess` · Duchess, Mags's donkey · P1

**Basis:** From `08-random-encounters.md` (`enc-mags`: a donkey called Duchess who "hasn't got all day"); `03-art-overhaul.md` §9 (the quadruped rig: Duchess the donkey). Her look is (derived).

Duchess, a pedlar's donkey with opinions: a sturdy grey-brown donkey in a three-quarter side view facing right, standing with one back hoof cocked. A pale muzzle and belly, dark rings round the eyes, long fuzzy ears, a dark stripe down the back, a scruffy mane and a tufted tail. Two big wicker panniers on a pack saddle, loaded with rolled rugs, pots, a lantern and a bundle of rope; a battered straw hat with holes cut for her ears; a red tassel on the bridle. Her expression: half-lidded, unimpressed. Output: PNG, 1536×1024 landscape, on a solid black background. One donkey, centred, the hooves near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 80% of the canvas width; in the game her back is level with a standard adult's chest. An original design. I'll save it as duchess.png.

#### `otter` · Otter · P1

**Basis:** **(derived)** From `08-random-encounters.md` (`enc-otters`: four otters back on the stream).

A river otter on a stream bank, in a three-quarter side view facing right, standing up on its hind legs to look around, front paws held to its chest. Sleek, wet dark-brown fur, a pale cream throat, small round ears, bright black eyes, long whiskers, a thick tapering tail on the ground behind it. Mood: curious. Output: PNG, 1536×1024 landscape, on a solid black background. One animal, centred, its feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 50% of the canvas width; in the game it's about knee-high to a standard adult standing up. An original design. I'll save it as otter.png.

#### `hare` · The stopped hare · P2

**Basis:** From `08-random-encounters.md` (`enc-stopped-hare`: a hare frozen in mid-leap, a little violet light caught in its fur); `03-art-overhaul.md` §8 (the frozen effect is done in the game). Violet is the colour of stopped time.

A brown hare caught in the middle of a leap, stretched out in the air with its long hind legs extended behind and its forelegs reaching forward, ears back, in a three-quarter side view facing right. Brown-grey fur, white belly, black-tipped ears, a wide dark eye. A few tiny motes of violet light are caught, glinting, in its fur, and a few flakes of snow hang in the air round it. Output: PNG, 1536×1024 landscape, on a solid black background. One animal, centred, nothing cropped. No text, no frame, no ground shadow. Scale: fill about 70% of the canvas width; in the game it's a normal hare, knee-high. An original design. I'll save it as hare.png.

#### `leviathan-shadow` · The leviathan's shadow · P2

**Basis:** From `08-random-encounters.md` (`enc-leviathan-shadow`: "A shadow crosses the sea from one horizon to the other, too big to be a cloud. It has fins."); `03-art-overhaul.md` §9 (the flier rig at scale 12, only ever a shadow). It's seen from above, on the water, so this one breaks the three-quarter rule.

The shadow of an enormous sea creature passing under the surface, seen from directly above: a long, smooth, dark silhouette like a whale crossed with a manta ray, with two vast, wing-like pectoral fins, a long tapering tail with a forked fin, and a blunt head, all in one flat, soft-edged shape of deep blue-black, darker in the middle and fading at the edges, as a shadow under water would. No details inside the shape, no eyes, no water, no background. Output: PNG, 1536×1024 landscape, on a solid black background. One shape, centred, nothing cropped. No text, no frame. Scale: fill about 90% of the canvas width; the game draws it across the whole sea. An original design. I'll save it as leviathan-shadow.png.

---

## 9. Index

Every part and creature in this file, in order. The characters are in §7.

| Id | Name | Slot | Section | Priority |
| --- | --- | --- | --- | --- |
| `base-male` | The base mannequin, man | base | §3 | P0 |
| `base-female` | The base mannequin, woman | base | §3 | P0 |
| `base-male-blank` | The blank mannequin, man | base | §3 | P0 |
| `base-female-blank` | The blank mannequin, woman | base | §3 | P0 |
| `face-player-1` | You, face 1 (the default man) | face | §4 | P0 |
| `face-player-2` | You, face 2 (the default woman) | face | §4 | P0 |
| `face-player-3` | You, face 3 (the Soul-Bound's default) | face | §4 | P0 |
| `face-player-4` | You, face 4 (a Hearthling) | face | §4 | P0 |
| `face-player-5` | You, face 5 (a Sylvan) | face | §4 | P0 |
| `face-player-6` | You, face 6 (an Emberkin) | face | §4 | P0 |
| `face-player-7` | You, face 7 (a Wildkin) | face | §4 | P0 |
| `face-player-8` | You, face 8 (a Stoneborn) | face | §4 | P0 |
| `face-sera` | Sera Vantis | face | §4 | P0 |
| `face-mira` | Mira Ashgrove | face | §4 | P0 |
| `face-mira-young` | Mira, aged thirteen | face | §4 | P1 |
| `face-kael` | Kael Duskmantle | face | §4 | P0 |
| `face-brannock` | Brannock Ironvein | face | §4 | P0 |
| `face-valencia` | Valencia Morvaine | face | §4 | P2 |
| `face-mc-child` | You, aged eight | face | §4 | P0 |
| `face-sb-child` | {sb}, aged eight | face | §4 | P0 |
| `face-brannock-bare` | Brannock, the clasps gone | face | §4 | P2 |
| `face-corra` | Corra | face | §4 | P0 |
| `face-lookout` | The lookout | face | §4 | P0 |
| `face-odelle` | Captain Odelle Marsh | face | §4 | P0 |
| `face-tomas` | Old Tomas | face | §4 | P0 |
| `face-red-gull` | Red Gull Thug, face 1 | face | §4 | P0 |
| `face-red-gull-2` | Red Gull Thug, face 2 | face | §4 | P0 |
| `face-red-gull-3` | Red Gull Thug, face 3 | face | §4 | P0 |
| `face-grell` | Bosun Grell | face | §4 | P0 |
| `face-hale` | Captain Roderic Hale | face | §4 | P0 |
| `face-fennick` | Questmaster Fennick | face | §4 | P0 |
| `face-hedda` | Hedda | face | §4 | P0 |
| `face-refugee-child` | Lise | face | §4 | P0 |
| `face-brisk` | Sergeant Brisk | face | §4 | P0 |
| `face-refugee-1` | Old Aurek | face | §4 | P0 |
| `face-refugee-2` | Teodor | face | §4 | P0 |
| `face-refugee-3` | Yselle the Glassblower | face | §4 | P0 |
| `face-elara` | Princess Elara Aurelian | face | §4 | P0 |
| `face-leon` | King Leon | face | §4 | P0 |
| `face-crane` | Lord Chancellor Vesper Crane | face | §4 | P0 |
| `face-anvil-barkeep` | Hessa the Barkeep | face | §4 | P0 |
| `face-netty` | Netty the Net-Mender | face | §4 | P0 |
| `face-maud` | Maud Tolley | face | §4 | P0 |
| `face-pip` | Pip the Potboy | face | §4 | P0 |
| `face-osric` | Osric Penhallow | face | §4 | P0 |
| `face-lark` | Lark the Busker | face | §4 | P0 |
| `face-ilsa` | Ilsa Fenwright | face | §4 | P0 |
| `face-oswin` | Guardsman Oswin | face | §4 | P0 |
| `face-hesper` | Sister Hesper | face | §4 | P0 |
| `face-wren` | Wren | face | §4 | P0 |
| `face-imra-vell` | Master Imra Vell | face | §4 | P0 |
| `face-tamsin` | Ranger Tamsin | face | §4 | P0 |
| `face-garrick` | Garrick the caravan master | face | §4 | P1 |
| `face-aldric` | Commander Aldric Voss | face | §4 | P1 |
| `face-recruit` | The recruit | face | §4 | P1 |
| `face-thessaly` | Archmage Thessaly Varn | face | §4 | P1 |
| `face-brenna` | Brenna of Ashfall | face | §4 | P1 |
| `face-gannet` | Mother Gannet | face | §4 | P1 |
| `face-hester` | Hester Hollis | face | §4 | P1 |
| `face-tobin` | Tobin Ashgrove | face | §4 | P1 |
| `face-harrow` | Lord Harrow of Heronsgate | face | §4 | P1 |
| `face-hobb` | Hobb the woodcutter | face | §4 | P1 |
| `face-maleficio` | Maleficio | face | §4 | P1 |
| `face-lost-boy` | The woodcutter's boy | face | §4 | P1 |
| `face-mags` | Pedlar Mags | face | §4 | P1 |
| `face-corra-flashback` | Corra, sixteen years younger | face | §4 | P0 |
| `face-maleficio-young` | Maleficio, remembered | face | §4 | P1 |
| `face-anselle` | Mother Superior Anselle | face | §4 | P2 |
| `face-orla` | Sister Orla | face | §4 | P2 |
| `face-corwin` | Sir Corwin Halbeck | face | §4 | P2 |
| `face-severin` | Lord Severin Morvaine | face | §4 | P2 |
| `face-isadora` | Lady Isadora | face | §4 | P2 |
| `face-valencia-child` | Valencia as a child | face | §4 | P2 |
| `face-pell` | Pell | face | §4 | P2 |
| `face-ide` | Ide | face | §4 | P2 |
| `face-maren-elder` | Maren, Hollowmere's elder | face | §4 | P2 |
| `face-blood-house-steward` | The blood-house steward | face | §4 | P2 |
| `face-hollowmere-child` | A Hollowmere child | face | §4 | P2 |
| `face-hollowmere-farmer` | A Hollowmere farmer | face | §4 | P2 |
| `face-ruck` | Ruck, the bosun | face | §4 | P2 |
| `face-davin` | Davin | face | §4 | P2 |
| `face-vorlaith` | Vorlaith, the Unmade King | face | §4 | P2 |
| `face-guild-apprentice` | A Guild apprentice | face | §4 | P2 |
| `face-refugee-child-aged` | Lise, aged by the tear | face | §4 | P2 |
| `face-leon-ill` | King Leon, ill | face | §4 | P2 |
| `face-severin-cell` | Severin, a prisoner | face | §4 | P2 |
| `face-town-1` | Townsfolk face 1 | face | §4 | P0 |
| `face-town-2` | Townsfolk face 2 | face | §4 | P0 |
| `face-town-3` | Townsfolk face 3 | face | §4 | P0 |
| `face-town-4` | Townsfolk face 4 | face | §4 | P0 |
| `face-town-5` | Townsfolk face 5 | face | §4 | P0 |
| `face-town-6` | Townsfolk face 6 | face | §4 | P0 |
| `face-town-7` | Townsfolk face 7 | face | §4 | P0 |
| `face-town-8` | Townsfolk face 8 | face | §4 | P0 |
| `face-town-9` | Townsfolk face 9 | face | §4 | P0 |
| `face-town-10` | Townsfolk face 10 | face | §4 | P0 |
| `face-town-11` | Townsfolk face 11 | face | §4 | P0 |
| `face-town-12` | Townsfolk face 12 | face | §4 | P0 |
| `face-town-13` | Townsfolk face 13 | face | §4 | P0 |
| `face-town-14` | Townsfolk face 14 | face | §4 | P0 |
| `face-town-15` | Townsfolk face 15 | face | §4 | P0 |
| `face-town-16` | Townsfolk face 16 | face | §4 | P0 |
| `face-town-17` | Townsfolk face 17 | face | §4 | P0 |
| `face-town-18` | Townsfolk face 18 | face | §4 | P0 |
| `face-varkhul` | Varkhul | face | §4 | P1 |
| `face-cube-cultist` | Cube Cultist | face | §4 | P1 |
| `face-hollowed` | The Hollowed | face | §4 | P0 |
| `face-construct-vault` | Vault construct's head | face | §4 | P0 |
| `face-vreya` | Vreya | face | §4 | P2 |
| `face-thrall` | Thrall | face | §4 | P2 |
| `face-drowned` | Drowned | face | §4 | P2 |
| `face-blood-wight` | Blood-wight | face | §4 | P2 |
| `face-sun-scorched` | Sun-scorched | face | §4 | P2 |
| `face-thornling` | Thornling | face | §4 | P2 |
| `face-construct-priest` | Priest-construct's head | face | §4 | P2 |
| `face-construct-crown` | Crown-construct's head | face | §4 | P2 |
| `face-unmade` | The Unmade | face | §4 | P2 |
| `face-crown-hollowed` | Crown-raised Hollowed | face | §4 | P2 |
| `hair-player-1` | You, face 1's hair | hair | §4.8 | P0 |
| `hair-player-2` | You, face 2's hair | hair | §4.8 | P0 |
| `hair-player-3` | You, face 3's hair | hair | §4.8 | P0 |
| `hair-player-4` | You, face 4's hair | hair | §4.8 | P0 |
| `hair-player-5` | You, face 5's hair | hair | §4.8 | P0 |
| `hair-player-6` | You, face 6's hair | hair | §4.8 | P0 |
| `hair-player-7` | You, face 7's hair | hair | §4.8 | P0 |
| `hair-player-8` | You, face 8's hair | hair | §4.8 | P0 |
| `hair-sera` | Sera Vantis's hair | hair | §4.8 | P0 |
| `hair-mira` | Mira Ashgrove's hair | hair | §4.8 | P0 |
| `hair-mira-young` | Mira, aged thirteen's hair | hair | §4.8 | P1 |
| `hair-kael` | Kael Duskmantle's hair | hair | §4.8 | P0 |
| `hair-brannock` | Brannock Ironvein's hair | hair | §4.8 | P0 |
| `hair-valencia` | Valencia Morvaine's hair | hair | §4.8 | P2 |
| `hair-mc-child` | You, aged eight's hair | hair | §4.8 | P0 |
| `hair-sb-child` | {sb}, aged eight's hair | hair | §4.8 | P0 |
| `hair-corra` | Corra's hair | hair | §4.8 | P0 |
| `hair-lookout` | The lookout's hair | hair | §4.8 | P0 |
| `hair-odelle` | Captain Odelle Marsh's hair | hair | §4.8 | P0 |
| `hair-tomas` | Old Tomas's hair | hair | §4.8 | P0 |
| `hair-red-gull` | Red Gull Thug, face 1's hair | hair | §4.8 | P0 |
| `hair-red-gull-2` | Red Gull Thug, face 2's hair | hair | §4.8 | P0 |
| `hair-red-gull-3` | Red Gull Thug, face 3's hair | hair | §4.8 | P0 |
| `hair-grell` | Bosun Grell's hair | hair | §4.8 | P0 |
| `hair-hale` | Captain Roderic Hale's hair | hair | §4.8 | P0 |
| `hair-fennick` | Questmaster Fennick's hair | hair | §4.8 | P0 |
| `hair-hedda` | Hedda's hair | hair | §4.8 | P0 |
| `hair-refugee-child` | Lise's hair | hair | §4.8 | P0 |
| `hair-brisk` | Sergeant Brisk's hair | hair | §4.8 | P0 |
| `hair-refugee-1` | Old Aurek's hair | hair | §4.8 | P0 |
| `hair-refugee-2` | Teodor's hair | hair | §4.8 | P0 |
| `hair-refugee-3` | Yselle the Glassblower's hair | hair | §4.8 | P0 |
| `hair-elara` | Princess Elara Aurelian's hair | hair | §4.8 | P0 |
| `hair-leon` | King Leon's hair | hair | §4.8 | P0 |
| `hair-crane` | Lord Chancellor Vesper Crane's hair | hair | §4.8 | P0 |
| `hair-anvil-barkeep` | Hessa the Barkeep's hair | hair | §4.8 | P0 |
| `hair-netty` | Netty the Net-Mender's hair | hair | §4.8 | P0 |
| `hair-maud` | Maud Tolley's hair | hair | §4.8 | P0 |
| `hair-pip` | Pip the Potboy's hair | hair | §4.8 | P0 |
| `hair-osric` | Osric Penhallow's hair | hair | §4.8 | P0 |
| `hair-lark` | Lark the Busker's hair | hair | §4.8 | P0 |
| `hair-ilsa` | Ilsa Fenwright's hair | hair | §4.8 | P0 |
| `hair-oswin` | Guardsman Oswin's hair | hair | §4.8 | P0 |
| `hair-hesper` | Sister Hesper's hair | hair | §4.8 | P0 |
| `hair-wren` | Wren's hair | hair | §4.8 | P0 |
| `hair-imra-vell` | Master Imra Vell's hair | hair | §4.8 | P0 |
| `hair-tamsin` | Ranger Tamsin's hair | hair | §4.8 | P0 |
| `hair-garrick` | Garrick the caravan master's hair | hair | §4.8 | P1 |
| `hair-aldric` | Commander Aldric Voss's hair | hair | §4.8 | P1 |
| `hair-recruit` | The recruit's hair | hair | §4.8 | P1 |
| `hair-thessaly` | Archmage Thessaly Varn's hair | hair | §4.8 | P1 |
| `hair-brenna` | Brenna of Ashfall's hair | hair | §4.8 | P1 |
| `hair-gannet` | Mother Gannet's hair | hair | §4.8 | P1 |
| `hair-hester` | Hester Hollis's hair | hair | §4.8 | P1 |
| `hair-tobin` | Tobin Ashgrove's hair | hair | §4.8 | P1 |
| `hair-harrow` | Lord Harrow of Heronsgate's hair | hair | §4.8 | P1 |
| `hair-hobb` | Hobb the woodcutter's hair | hair | §4.8 | P1 |
| `hair-maleficio` | Maleficio's hair | hair | §4.8 | P1 |
| `hair-lost-boy` | The woodcutter's boy's hair | hair | §4.8 | P1 |
| `hair-mags` | Pedlar Mags's hair | hair | §4.8 | P1 |
| `hair-anselle` | Mother Superior Anselle's hair | hair | §4.8 | P2 |
| `hair-orla` | Sister Orla's hair | hair | §4.8 | P2 |
| `hair-corwin` | Sir Corwin Halbeck's hair | hair | §4.8 | P2 |
| `hair-severin` | Lord Severin Morvaine's hair | hair | §4.8 | P2 |
| `hair-isadora` | Lady Isadora's hair | hair | §4.8 | P2 |
| `hair-valencia-child` | Valencia as a child's hair | hair | §4.8 | P2 |
| `hair-pell` | Pell's hair | hair | §4.8 | P2 |
| `hair-ide` | Ide's hair | hair | §4.8 | P2 |
| `hair-maren-elder` | Maren, Hollowmere's elder's hair | hair | §4.8 | P2 |
| `hair-blood-house-steward` | The blood-house steward's hair | hair | §4.8 | P2 |
| `hair-hollowmere-child` | A Hollowmere child's hair | hair | §4.8 | P2 |
| `hair-hollowmere-farmer` | A Hollowmere farmer's hair | hair | §4.8 | P2 |
| `hair-ruck` | Ruck, the bosun's hair | hair | §4.8 | P2 |
| `hair-davin` | Davin's hair | hair | §4.8 | P2 |
| `hair-guild-apprentice` | A Guild apprentice's hair | hair | §4.8 | P2 |
| `hair-town-1` | Townsfolk face 1's hair | hair | §4.8 | P0 |
| `hair-town-2` | Townsfolk face 2's hair | hair | §4.8 | P0 |
| `hair-town-3` | Townsfolk face 3's hair | hair | §4.8 | P0 |
| `hair-town-4` | Townsfolk face 4's hair | hair | §4.8 | P0 |
| `hair-town-5` | Townsfolk face 5's hair | hair | §4.8 | P0 |
| `hair-town-6` | Townsfolk face 6's hair | hair | §4.8 | P0 |
| `hair-town-7` | Townsfolk face 7's hair | hair | §4.8 | P0 |
| `hair-town-8` | Townsfolk face 8's hair | hair | §4.8 | P0 |
| `hair-town-9` | Townsfolk face 9's hair | hair | §4.8 | P0 |
| `hair-town-10` | Townsfolk face 10's hair | hair | §4.8 | P0 |
| `hair-town-11` | Townsfolk face 11's hair | hair | §4.8 | P0 |
| `hair-town-12` | Townsfolk face 12's hair | hair | §4.8 | P0 |
| `hair-town-13` | Townsfolk face 13's hair | hair | §4.8 | P0 |
| `hair-town-14` | Townsfolk face 14's hair | hair | §4.8 | P0 |
| `hair-town-15` | Townsfolk face 15's hair | hair | §4.8 | P0 |
| `hair-town-16` | Townsfolk face 16's hair | hair | §4.8 | P0 |
| `hair-town-17` | Townsfolk face 17's hair | hair | §4.8 | P0 |
| `hair-town-18` | Townsfolk face 18's hair | hair | §4.8 | P0 |
| `hair-varkhul` | Varkhul's hair | hair | §4.8 | P1 |
| `hair-cube-cultist` | Cube Cultist's hair | hair | §4.8 | P1 |
| `hair-vreya` | Vreya's hair | hair | §4.8 | P2 |
| `hair-thrall` | Thrall's hair | hair | §4.8 | P2 |
| `hair-drowned` | Drowned's hair | hair | §4.8 | P2 |
| `armor-traveller` | The traveller (the player's default) | armor | §5 | P0 |
| `armor-traveller-f` | The traveller, on the woman's base | armor | §5 | P0 |
| `armor-sb-traveller` | The Soul-Bound's traveller | armor | §5 | P0 |
| `armor-champion` | The Champion's tabard | armor | §5 | P1 |
| `armor-champion-f` | The Champion's tabard, on the woman's base | armor | §5 | P1 |
| `armor-lord-protector` | The Lord Protector's coat | armor | §5 | P2 |
| `armor-sovereign` | The Sovereign's armour | armor | §5 | P2 |
| `armor-child-cove` | A cove child's clothes | armor | §5 | P0 |
| `armor-sera` | Sera's ranger coat | armor | §5 | P0 |
| `armor-mira` | Mira's archivist's clothes | armor | §5 | P0 |
| `armor-mira-apprentice` | An archive apprentice's smock | armor | §5 | P1 |
| `armor-kael` | Kael's hooded leathers | armor | §5 | P0 |
| `armor-kael-shadow` | Kael, the Protectorate Shadow | armor | §5 | P2 |
| `armor-brannock` | Brannock's soldier-smith kit | armor | §5 | P0 |
| `armor-valencia` | Valencia's gown | armor | §5 | P2 |
| `armor-abbey-habit` | The abbey habit | armor | §5 | P2 |
| `armor-corra` | Corra's net-mender's clothes | armor | §5 | P0 |
| `armor-sailor` | A sailor's clothes | armor | §5 | P0 |
| `armor-odelle` | Captain Odelle's sea-coat | armor | §5 | P0 |
| `armor-fisher` | A fisherman's oilskins | armor | §5 | P0 |
| `armor-red-gull` | A Red Gull's rig | armor | §5 | P0 |
| `armor-grell` | Bosun Grell's coat and hook | armor | §5 | P0 |
| `armor-watch` | The City Watch uniform | armor | §5 | P0 |
| `armor-watch-captain` | The Watch captain's coat | armor | §5 | P0 |
| `armor-clerk` | A clerk's waistcoat | armor | §5 | P0 |
| `armor-refugee-woman` | A refugee woman's clothes | armor | §5 | P0 |
| `armor-refugee-man` | A refugee man's clothes | armor | §5 | P0 |
| `armor-refugee-child` | A refugee child's smock | armor | §5 | P0 |
| `armor-glassblower` | A glassblower's apron | armor | §5 | P0 |
| `armor-elara` | Elara's riding coat | armor | §5 | P0 |
| `armor-elara-disguise` | Elara in disguise | armor | §5 | P0 |
| `armor-leon` | King Leon's robes | armor | §5 | P0 |
| `armor-crane` | Crane's long coat | armor | §5 | P0 |
| `armor-royal-livery` | The palace livery | armor | §5 | P0 |
| `armor-royal-guard` | The Royal Guard's armour | armor | §5 | P0 |
| `armor-tavern` | A tavern apron | armor | §5 | P0 |
| `armor-villager-woman` | A townswoman's clothes | armor | §5 | P0 |
| `armor-villager-man` | A townsman's clothes | armor | §5 | P0 |
| `armor-elder-woman` | An old woman's dress and shawl | armor | §5 | P0 |
| `armor-merchant` | A merchant's waistcoat | armor | §5 | P0 |
| `armor-busker` | A busker's finery | armor | §5 | P0 |
| `armor-temple` | A temple sister's tabard | armor | §5 | P0 |
| `armor-urchin` | A street child's hand-me-downs | armor | §5 | P0 |
| `armor-fencer` | A fencing-master's doublet | armor | §5 | P0 |
| `armor-lodge-ranger` | The Lodge ranger's kit | armor | §5 | P0 |
| `armor-noble` | A courtier's doublet | armor | §5 | P0 |
| `armor-noble-gown` | A lady of the court's gown | armor | §5 | P0 |
| `armor-elder-coat` | An old man's long coat | armor | §5 | P0 |
| `armor-trader` | A trader's waistcoat | armor | §5 | P0 |
| `armor-baker` | A baker's dress and apron | armor | §5 | P0 |
| `armor-smith` | A smith's apron | armor | §5 | P0 |
| `armor-road` | A traveller's road clothes | armor | §5 | P0 |
| `armor-courier` | A courier's clothes | armor | §5 | P0 |
| `armor-pilgrim` | A pilgrim's robe | armor | §5 | P0 |
| `armor-porter` | A porter's clothes | armor | §5 | P0 |
| `armor-maid` | A maid's dress | armor | §5 | P0 |
| `armor-caravan` | A caravan master's duster | armor | §5 | P1 |
| `armor-aldric` | Aldric's worn plate | armor | §5 | P1 |
| `armor-recruit` | A recruit's gambeson | armor | §5 | P1 |
| `armor-thessaly` | The Archmage's robes | armor | §5 | P1 |
| `armor-brenna` | Brenna's ash-walker kit | armor | §5 | P1 |
| `armor-gannet` | Mother Gannet's shawl and dress | armor | §5 | P1 |
| `armor-cider-presser` | A cider-presser's apron | armor | §5 | P1 |
| `armor-levy` | A levy coat | armor | §5 | P1 |
| `armor-harrow` | Lord Harrow's heron coat | armor | §5 | P1 |
| `armor-woodcutter` | A woodcutter's clothes | armor | §5 | P1 |
| `armor-maleficio` | Maleficio's high-backed robes | armor | §5 | P1 |
| `armor-scholar` | A scholar's robes | armor | §5 | P1 |
| `armor-old-pattern` | Old-pattern mail and surcoat | armor | §5 | P1 |
| `armor-pedlar` | A pedlar's patched coat | armor | §5 | P1 |
| `armor-varkhul` | Varkhul's Hollow plate | armor | §5 | P1 |
| `armor-anselle` | Mother Anselle's habit | armor | §5 | P2 |
| `armor-beekeeper` | A beekeeper's smock | armor | §5 | P2 |
| `armor-corwin` | Sir Corwin's gilded plate | armor | §5 | P2 |
| `armor-severin` | Severin's velvet coat | armor | §5 | P2 |
| `armor-isadora` | Lady Isadora's gown | armor | §5 | P2 |
| `armor-child-dress` | A little girl's dress | armor | §5 | P2 |
| `armor-morvaine-man` | A valley man's wool coat | armor | §5 | P2 |
| `armor-morvaine-woman` | A valley woman's wool dress | armor | §5 | P2 |
| `armor-shift` | A linen shift and shawl | armor | §5 | P2 |
| `armor-morvaine-livery` | A vampire household's livery | armor | §5 | P2 |
| `armor-abbey-lay` | A lay sister's habit | armor | §5 | P2 |
| `armor-prisoner` | A prisoner's shirt | armor | §5 | P2 |
| `armor-davin` | Davin's cut-down tabard | armor | §5 | P2 |
| `armor-vorlaith` | Vorlaith's glass body | armor | §5 | P2 |
| `armor-guild-apprentice` | A Guild apprentice's robe | armor | §5 | P2 |
| `armor-elara-field` | Elara, leading the resistance | armor | §5 | P2 |
| `armor-elara-queen` | Elara, crowned | armor | §5 | P2 |
| `armor-leon-furs` | King Leon, in furs | armor | §5 | P2 |
| `armor-brenna-protectorate` | Brenna, sworn to the Protector | armor | §5 | P2 |
| `armor-hollowed-footman` | A Hollowed footman's body | armor | §5 | P0 |
| `armor-hollowed-hexer` | A Hollowed hexer's body | armor | §5 | P0 |
| `armor-hollowed-sapper` | A Hollowed sapper's body | armor | §5 | P0 |
| `armor-hollowed-chief` | The sapper chief's body | armor | §5 | P0 |
| `armor-construct-stone` | A vault construct's stone body | armor | §5 | P0 |
| `armor-hollowed-climber` | A Hollowed climber's body | armor | §5 | P1 |
| `armor-cultist` | A cube cultist's robe | armor | §5 | P1 |
| `armor-bandit` | A bandit's leathers | armor | §5 | P1 |
| `armor-thrall` | A thrall's tabard | armor | §5 | P2 |
| `armor-thrall-knight` | A thrall-knight's black plate | armor | §5 | P2 |
| `armor-gilded-knight` | A Gilded knight's armour | armor | §5 | P2 |
| `armor-drowned` | A drowned body | armor | §5 | P2 |
| `armor-blood-wight` | A blood-wight's body | armor | §5 | P2 |
| `armor-sun-scorched` | A sun-scorched body | armor | §5 | P2 |
| `armor-thornling` | A thornling's bark body | armor | §5 | P2 |
| `armor-construct-priest` | A priest-construct's stone body | armor | §5 | P2 |
| `armor-smuggler` | A smuggler's oilskin | armor | §5 | P2 |
| `armor-protectorate` | The Protectorate's tabard | armor | §5 | P2 |
| `armor-protectorate-warden` | A Protectorate warden's plate | armor | §5 | P2 |
| `armor-construct-crown` | A crown-construct's metal body | armor | §5 | P2 |
| `armor-unmade-bronze` | Ancient bronze scale | armor | §5 | P2 |
| `armor-pikeman` | A pikeman's breastplate | armor | §5 | P2 |
| `armor-ashfall-raider` | An Ashfall raider's kit | armor | §5 | P2 |
| `armor-maleficio-ascendant` | Maleficio, the Sovereign | armor | §5 | P1 |
| `armor-varkhul-split` | Varkhul, the armour split open | armor | §5 | P1 |
| `armor-crown-hollowed` | Crown-raised Hollowed body | armor | §5 | P2 |
| `armor-padded` | Padded armour | armor | §5 | P0 |
| `armor-leather` | Leather armour | armor | §5 | P0 |
| `armor-studded` | Studded leather | armor | §5 | P0 |
| `armor-hide` | Hide armour | armor | §5 | P0 |
| `armor-chain-shirt` | Chain shirt | armor | §5 | P0 |
| `armor-chain-mail` | Chain mail | armor | §5 | P0 |
| `armor-scale` | Scale mail | armor | §5 | P0 |
| `armor-plate` | Plate armour | armor | §5 | P0 |
| `armor-mage-robes` | Mage robes | armor | §5 | P0 |
| `armor-padded-f` | Padded armour, woman's base | armor | §5 | P0 |
| `armor-leather-f` | Leather armour, woman's base | armor | §5 | P0 |
| `armor-studded-f` | Studded leather, woman's base | armor | §5 | P0 |
| `armor-hide-f` | Hide armour, woman's base | armor | §5 | P0 |
| `armor-chain-shirt-f` | Chain shirt, woman's base | armor | §5 | P0 |
| `armor-chain-mail-f` | Chain mail, woman's base | armor | §5 | P0 |
| `armor-scale-f` | Scale mail, woman's base | armor | §5 | P0 |
| `armor-plate-f` | Plate armour, woman's base | armor | §5 | P0 |
| `armor-mage-robes-f` | Mage robes, woman's base | armor | §5 | P0 |
| `cape-sb` | The Soul-Bound's cape | cape | §6.1 | P0 |
| `cape-sera` | Sera's shoulder cape | cape | §6.1 | P0 |
| `cape-tail-ember` | An Emberkin's ember tail | cape | §6.1 | P0 |
| `cape-tail-wolf` | A Wildkin wolf's tail | cape | §6.1 | P0 |
| `cape-tail-hawk` | A Wildkin hawk's tail feathers | cape | §6.1 | P0 |
| `cape-tail-bear` | A Wildkin bear's tail | cape | §6.1 | P0 |
| `cape-watch` | The Watch's short cape | cape | §6.1 | P0 |
| `cape-watch-captain` | The Watch captain's cloak | cape | §6.1 | P0 |
| `cape-royal` | The palace's ochre cloak | cape | §6.1 | P0 |
| `cape-leon` | King Leon's cloak | cape | §6.1 | P0 |
| `cape-travelling` | Travelling cloak | cape | §6.1 | P0 |
| `cape-elara-hooded` | A plain charcoal cloak | cape | §6.1 | P0 |
| `cape-protection` | Cloak of protection | cape | §6.1 | P1 |
| `cape-blanket` | A blanket worn as a cloak | cape | §6.1 | P0 |
| `cape-short-lavender` | A short lavender cape | cape | §6.1 | P0 |
| `cape-tamsin` | The Lodge's teal cloak and hawk feathers | cape | §6.1 | P0 |
| `cape-old-woman` | An old woman's rose cape | cape | §6.1 | P0 |
| `cape-campaign` | Aldric's campaign cloak | cape | §6.1 | P1 |
| `cape-ash` | An ash-grey hooded cloak | cape | §6.1 | P1 |
| `cape-pedlar-pack` | A pedlar's pack frame | cape | §6.1 | P1 |
| `cape-valencia` | Valencia's cape | cape | §6.1 | P2 |
| `cape-severin` | Severin's velvet cape | cape | §6.1 | P2 |
| `cape-severin-wings` | Severin's cape, spread like wings | cape | §6.1 | P2 |
| `cape-thrall-knight` | A thrall-knight's cape | cape | §6.1 | P2 |
| `cape-corwin` | Sir Corwin's white cloak | cape | §6.1 | P2 |
| `cape-gilded` | A Gilded knight's cape | cape | §6.1 | P2 |
| `cape-varkhul` | Varkhul's tattered cloak | cape | §6.1 | P1 |
| `helm-watch` | The Watch helm | helm | §6.2 | P0 |
| `helm-watch-crest` | The Watch captain's crested helm | helm | §6.2 | P0 |
| `helm-royal-guard` | The Royal Guard's helm | helm | §6.2 | P0 |
| `helm-tricorn` | A tricorn hat | helm | §6.2 | P0 |
| `helm-crown` | A small gold crown | helm | §6.2 | P0 |
| `helm-circlet` | A gold circlet | helm | §6.2 | P0 |
| `helm-hood-charcoal` | A charcoal hood | helm | §6.2 | P0 |
| `helm-kael-hood` | Kael's pointed hood | helm | §6.2 | P0 |
| `helm-beret` | A beret | helm | §6.2 | P0 |
| `helm-knit-cap` | A knitted cap | helm | §6.2 | P0 |
| `helm-bandana-red` | A Red Gull bandana | helm | §6.2 | P0 |
| `helm-bandana` | A bandana | helm | §6.2 | P0 |
| `helm-kettle` | A kettle helm | helm | §6.2 | P1 |
| `helm-hollowed-kettle` | A rusted kettle helm | helm | §6.2 | P0 |
| `helm-hollowed-hood` | A hexer's deep hood | helm | §6.2 | P0 |
| `helm-sapper-goggles` | A sapper's cap and goggles | helm | §6.2 | P0 |
| `helm-sapper-faceplate` | A cracked iron faceplate | helm | §6.2 | P0 |
| `helm-wide-brim` | A wide-brimmed hat | helm | §6.2 | P0 |
| `helm-straw-hat` | A straw hat | helm | §6.2 | P0 |
| `helm-flat-cap` | A flat cap | helm | §6.2 | P0 |
| `helm-kerchief` | A kerchief | helm | §6.2 | P0 |
| `helm-lace-cap` | A lace cap | helm | §6.2 | P1 |
| `helm-varkhul` | Varkhul's cracked helm | helm | §6.2 | P1 |
| `helm-nasal` | An old-pattern nasal helm | helm | §6.2 | P1 |
| `helm-abbey-hood` | The abbey's square hood | helm | §6.2 | P2 |
| `helm-bee-veil` | A bee veil | helm | §6.2 | P2 |
| `helm-corwin` | Sir Corwin's plumed helm | helm | §6.2 | P2 |
| `helm-gilded` | A Gilded knight's helm | helm | §6.2 | P2 |
| `helm-crown-thorns` | The crown of thorns | helm | §6.2 | P2 |
| `helm-circlet-silver` | A silver circlet | helm | §6.2 | P2 |
| `helm-vesk-wolf` | Vesk's wolf helm | helm | §6.2 | P2 |
| `helm-vreya-crown` | Vreya's crowned helm | helm | §6.2 | P2 |
| `helm-mordain-beak` | Mordain's beaked helm | helm | §6.2 | P2 |
| `helm-crown-dusk-dawn` | The crown of dusk and dawn | helm | §6.2 | P2 |
| `helm-vorlaith` | Vorlaith's crown of clock-hands | helm | §6.2 | P2 |
| `helm-warden` | A Protectorate warden's great helm | helm | §6.2 | P2 |
| `helm-bronze-crest` | A crested bronze helm | helm | §6.2 | P2 |
| `helm-morion` | A crested open helm | helm | §6.2 | P2 |
| `helm-bandit-hood` | A bandit's hood and scarf | helm | §6.2 | P1 |
| `helm-leather-cap` | Leather cap | helm | §6.2 | P0 |
| `helm-iron-helm` | Iron helm | helm | §6.2 | P0 |
| `helm-circlet-of-focus` | Circlet of focus | helm | §6.2 | P0 |
| `weapon-dagger` | Dagger | weapon | §6.3 | P0 |
| `weapon-shortsword` | Shortsword | weapon | §6.3 | P0 |
| `weapon-longsword` | Longsword | weapon | §6.3 | P0 |
| `weapon-rapier` | Rapier | weapon | §6.3 | P0 |
| `weapon-scimitar` | Scimitar | weapon | §6.3 | P0 |
| `weapon-handaxe` | Hand axe | weapon | §6.3 | P0 |
| `weapon-battleaxe` | Battleaxe | weapon | §6.3 | P0 |
| `weapon-greatsword` | Greatsword | weapon | §6.3 | P0 |
| `weapon-warhammer` | Warhammer | weapon | §6.3 | P0 |
| `weapon-mace` | Mace | weapon | §6.3 | P0 |
| `weapon-quarterstaff` | Quarterstaff | weapon | §6.3 | P0 |
| `weapon-spear` | Spear | weapon | §6.3 | P0 |
| `weapon-shortbow` | Shortbow | weapon | §6.3 | P0 |
| `weapon-longbow` | Longbow | weapon | §6.3 | P0 |
| `weapon-light-crossbow` | Light crossbow | weapon | §6.3 | P0 |
| `weapon-ember-dagger` | Ember dagger | weapon | §6.3 | P0 |
| `weapon-thornwood-bow` | Thornwood bow | weapon | §6.3 | P1 |
| `weapon-sunsteel-blade` | Sunsteel blade | weapon | §6.3 | P1 |
| `offhand-wooden-shield` | Wooden shield | offhand | §6.3 | P0 |
| `offhand-steel-shield` | Steel shield | offhand | §6.3 | P0 |
| `offhand-warden-shield` | Warden's shield | offhand | §6.3 | P0 |
| `offhand-dagger` | Dagger, in the off hand | offhand | §6.3 | P0 |
| `offhand-shortsword` | Shortsword, in the off hand | offhand | §6.3 | P0 |
| `offhand-scimitar` | Scimitar, in the off hand | offhand | §6.3 | P0 |
| `offhand-handaxe` | Hand axe, in the off hand | offhand | §6.3 | P0 |
| `offhand-ember-dagger` | Ember dagger, in the off hand | offhand | §6.3 | P0 |
| `weapon-sb-blades` | The Soul-Bound's blades | weapon | §6.3 | P0 |
| `weapon-sera-longbow` | Sera's longbow | weapon | §6.3 | P0 |
| `weapon-kael-knives` | Kael's twin knives | weapon | §6.3 | P0 |
| `weapon-kael-rapier` | Kael's rapier and parrying dagger | weapon | §6.3 | P0 |
| `weapon-brannock` | Brannock's sword and shield | weapon | §6.3 | P0 |
| `weapon-cutlass` | Cutlass | weapon | §6.3 | P0 |
| `weapon-gaff` | A fisherman's gaff | weapon | §6.3 | P0 |
| `weapon-cudgel` | Cudgel | weapon | §6.3 | P0 |
| `weapon-watch-kit` | The Watch's spear and shield | weapon | §6.3 | P0 |
| `weapon-halberd` | Halberd | weapon | §6.3 | P0 |
| `weapon-netting-needle` | A netting needle | weapon | §6.3 | P0 |
| `weapon-telescope` | A brass telescope | weapon | §6.3 | P0 |
| `weapon-tack-hammer` | A tack hammer and a notice | weapon | §6.3 | P0 |
| `weapon-loaf` | A loaf | weapon | §6.3 | P0 |
| `weapon-rag-doll` | A rag doll | weapon | §6.3 | P0 |
| `weapon-walking-stick` | A walking stick | weapon | §6.3 | P0 |
| `weapon-tin-cup` | A tin cup | weapon | §6.3 | P0 |
| `weapon-quill-letter` | A quill and a sealed letter | weapon | §6.3 | P0 |
| `weapon-letter` | A sealed letter | weapon | §6.3 | P0 |
| `weapon-letters` | A bundle of letters | weapon | §6.3 | P0 |
| `weapon-tankard` | A tankard | weapon | §6.3 | P0 |
| `weapon-stew-pot` | A stew pot | weapon | §6.3 | P0 |
| `weapon-silk-bolt` | A bolt of silk | weapon | §6.3 | P0 |
| `weapon-lute` | A lute | weapon | §6.3 | P0 |
| `weapon-scrolls` | An armful of scrolls | weapon | §6.3 | P0 |
| `weapon-broom` | A broom | weapon | §6.3 | P0 |
| `weapon-coin` | A coin on the knuckles | weapon | §6.3 | P0 |
| `weapon-practice-sword` | A wooden practice sword | weapon | §6.3 | P0 |
| `weapon-flat-cap-held` | A cap held in both hands | weapon | §6.3 | P0 |
| `weapon-staff-ring` | Thessaly's ringed staff | weapon | §6.3 | P1 |
| `weapon-ledger-pen` | A ledger and a pen | weapon | §6.3 | P1 |
| `weapon-folded-shawl` | A folded shawl | weapon | §6.3 | P1 |
| `weapon-mallet` | A mallet | weapon | §6.3 | P1 |
| `weapon-spade` | A spade | weapon | §6.3 | P1 |
| `weapon-heron-cane` | A heron-headed cane | weapon | §6.3 | P1 |
| `weapon-felling-axe` | A felling axe | weapon | §6.3 | P1 |
| `weapon-black-box` | The Black Box | weapon | §6.3 | P1 |
| `weapon-oil-lamp` | An oil lamp | weapon | §6.3 | P1 |
| `weapon-sketchbook` | A sketchbook | weapon | §6.3 | P1 |
| `weapon-varkhul-greatsword` | Varkhul's greatsword | weapon | §6.3 | P1 |
| `weapon-cultist-dagger` | A cube-pommelled dagger | weapon | §6.3 | P1 |
| `weapon-hollowed-sword` | A rusted longsword | weapon | §6.3 | P0 |
| `weapon-hexer-staff` | A hexer's crystal staff | weapon | §6.3 | P0 |
| `weapon-sapper-pick` | A pick and a lit charge | weapon | §6.3 | P0 |
| `weapon-chief-pick-lantern` | A pick and a burning lantern | weapon | §6.3 | P0 |
| `weapon-climber-hook` | A hooked blade | weapon | §6.3 | P1 |
| `weapon-relief-kit` | An old-pattern spear and kite shield | weapon | §6.3 | P1 |
| `weapon-lantern` | A lit lantern | weapon | §6.3 | P2 |
| `weapon-smoker` | A beekeeper's smoker | weapon | §6.3 | P2 |
| `weapon-corwin-kit` | Sir Corwin's sword and sun shield | weapon | §6.3 | P2 |
| `weapon-corwin-sword` | Sir Corwin's sword, too big | weapon | §6.3 | P2 |
| `weapon-gilded-kit` | A Gilded knight's sword and shield | weapon | §6.3 | P2 |
| `weapon-mace-shield` | A mace and a small shield | weapon | §6.3 | P2 |
| `weapon-night-flowers` | A sprig of night-flowers | weapon | §6.3 | P2 |
| `weapon-toy-sword` | A wooden toy sword | weapon | §6.3 | P0 |
| `weapon-basket` | A covered basket | weapon | §6.3 | P0 |
| `weapon-flower-basket` | A flower basket | weapon | §6.3 | P0 |
| `weapon-spear-lantern` | A spear and a lantern | weapon | §6.3 | P2 |
| `weapon-vesk-sword` | Vesk's black sword | weapon | §6.3 | P2 |
| `weapon-vreya-blade` | Vreya's wavy blade | weapon | §6.3 | P2 |
| `weapon-mordain-glaive` | Mordain's glaive | weapon | §6.3 | P2 |
| `weapon-glass-lamp` | A glowing glass lamp | weapon | §6.3 | P2 |
| `weapon-map` | A rolled map | weapon | §6.3 | P2 |
| `weapon-unmade-spear-shield` | An ancient spear and bronze shield | weapon | §6.3 | P2 |
| `weapon-pike` | A pike | weapon | §6.3 | P2 |
| `weapon-warden-kit` | A tower shield and halberd | weapon | §6.3 | P2 |
| `weapon-protectorate-kit` | A cudgel and a violet shield | weapon | §6.3 | P2 |
| `weapon-raider-kit` | An axe and a grey shield | weapon | §6.3 | P2 |
| `weapon-pilgrim-staff` | A pilgrim's staff | weapon | §6.3 | P0 |
| `weapon-pitchfork` | A pitchfork | weapon | §6.3 | P0 |
| `weapon-crate` | A crate on the shoulder | weapon | §6.3 | P0 |
| `weapon-peel` | A baker's peel and a loaf | weapon | §6.3 | P0 |
| `weapon-hammer-tongs` | A forging hammer and tongs | weapon | §6.3 | P0 |
| `weapon-spindle` | A drop spindle | weapon | §6.3 | P0 |
| `weapon-coin-purse` | A purse of coins | weapon | §6.3 | P0 |
| `weapon-feather-duster` | A feather duster | weapon | §6.3 | P0 |
| `weapon-brannock-1` | Brannock's shield, one name | weapon | §6.3 | P1 |
| `weapon-brannock-41` | Brannock's shield, forty-one names | weapon | §6.3 | P1 |
| `weapon-brannock-42` | Brannock's shield, forty-two names | weapon | §6.3 | P2 |
| `sea-wraith` | Sea-wraith | creature | §8 | P0 |
| `gloomfang` | Gloomfang Wolf | creature | §8 | P0 |
| `gloomfang.b` | Gloomfang Wolf, second coat | creature | §8 | P0 |
| `gloomfang.c` | Gloomfang Wolf, third coat | creature | §8 | P0 |
| `thornmother` | The Thornmother | creature | §8 | P0 |
| `thornmother.rearing` | The Thornmother, rearing | creature | §8 | P0 |
| `ram-beast` | The Hollowed ram beast | creature | §8 | P1 |
| `hungering` | The Hungering | creature | §8 | P1 |
| `rot-boar` | Rot-boar | creature | §8 | P1 |
| `ash-hound` | Ash-hound | creature | §8 | P1 |
| `dire-bat` | Dire bat | creature | §8 | P2 |
| `tithe-rider` | Tithe rider | creature | §8 | P2 |
| `rockjaw` | Rockjaw | creature | §8 | P2 |
| `glass-scorpion` | Glass scorpion | creature | §8 | P2 |
| `lodge-hawk` | The Lodge's hawk | creature | §8 | P0 |
| `burr` | Burr, Sera's kestrel | creature | §8 | P1 |
| `duchess` | Duchess, Mags's donkey | creature | §8 | P1 |
| `otter` | Otter | creature | §8 | P1 |
| `hare` | The stopped hare | creature | §8 | P2 |
| `leviathan-shadow` | The leviathan's shadow | creature | §8 | P2 |

Counts: base 2, face 114, armor 134, cape 27, helm 42, weapon 100, creature 20; 156 recipes. By priority: P0 248, P1 75, P2 116.
