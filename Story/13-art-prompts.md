# Art prompts: the parts every character is built from, for ChatGPT

The owner's model (2026-11-06): every person in the game is one shared base mannequin wearing a body (the armor), a cape and a helm, holding a weapon, with only the face changed; NPCs differ in size by morphs, height and width factors on the whole figure. This file has the prompts to make every part in ChatGPT, each one an edit of the base image so it lines up exactly, the recipe that builds each character from the parts, and full-figure prompts for the creatures that aren't people.

The parts follow the Art Guide (§1 visual identity, §2 resolution, §11 prompts for art made outside the repo) and the looks in `03-art-overhaul.md` §7 (who looks like what, the tiers and the reserved hues), §6 (live items) and §9 (creatures). Where a figure on the owner's guide sheet (`tools/source/art-guide.png`) maps onto a character, it's the basis. Where no brief exists, the design comes from the scripts and the voice sheets, and its Basis line says **(derived)**. No prompt names a game, a studio or a company, and every one asks for an original design.

---

## 1. How to use this file

### 1.1 The model

| Slot | What it is | Id |
| --- | --- | --- |
| **armor** | The whole outfit, which is the visible body below the neck, painted in the body's pieces (§5.0): clothes, boots, gloves, belts, bags. For the people who aren't flesh (the Hollowed, the constructs, the drowned, Vorlaith) it's the body itself | `armor-…` |
| **cape** | What hangs behind the body: cloaks, capes, a pedlar's pack, and the Emberkin's and Wildkin's tails | `cape-…` |
| **helm** | What sits over the head: helms, hats, hoods, crowns, circlets; or a hairstyle from the shared library (§4.8), in the same slot | `helm-…`, `hair-…` |
| **extras** | Beards, jewellery, scars and a people's ears and horns, any number, shown with any hair or helm (§4.10) | `beard-…`, `jewel-…`, `skin-…`, `ears-…`, `horns-…` |
| **weapon** | What the hands hold: one weapon or prop, or a pair drawn as one piece (a sword and shield, two knives) | `weapon-…` |
| **face** | The features on the bald head: one of ten men's or ten women's faces that everyone shares, recoloured for each character (§4) | `face-…` |
| **morph** | A height and a width factor on the whole figure. It's how the builds and every NPC's size are made | numbers in §7 |

Bare skin on an armor keeps the base's warm tan tone, and the game tints it to match the face. Boots, gloves, rings and amulets in the item catalog change a character's numbers, not their picture; the armor already shows boots and gloves. The game draws back to front: cape, armor, face, helm, weapon (Emberkin horns and Wildkin ears draw over a hood, `03-art-overhaul.md` §3.6).

### 1.2 Follow-ups

Two prompts you'll reuse. Ask them in the same chat as the part they're for.

**Expressions** · `<face-id>.<expression>.png`

> Edit the attached image (this character's face on the base): change ONLY the expression, to the one I name at the end. Move only the eyes, eyebrows and mouth; the head, the hair, the skin and everything else stay exactly the same and in exactly the same place. The expressions: smile (eyes open, brows level, a gentle smile); grin (eyes half-closed, brows raised, a wide grin showing teeth); laugh (eyes closed, brows raised, mouth open); smirk (eyes narrowed, one brow up, a one-sided smirk); sad (eyes looking down, brows tilted up in the middle, a frown); worried (eyes open, brows knit and tilted up, mouth flat); afraid (eyes wide, brows tilted up, mouth open); angry (eyes narrowed, brows knit down, teeth gritted); cold (eyes narrowed, brows level, mouth flat, no warmth at all); tired (eyes half-closed, brows level, mouth flat); shock (eyes wide, brows raised high, mouth open). Return it as a PNG at the same size and background. The expression for this one: ____. I'll save it as <face-id>.<expression>.png.

The face picture itself is `neutral`. The twelve are `03-art-overhaul.md` §5.2's: notable characters (§7.4) get all eleven others, named minor characters five (smile, sad, worried, angry, shock), and generic faces two (smile, worried). The game cuts the eyes, brows and mouth out of each by comparing it with the neutral face.

**Fix the size** · `<part-id>.png`

> That came back at a different size or layout from the picture I attached: something is cropped, zoomed, moved, or drawn bigger or smaller than it was. Do the same edit again on the attached picture. It is exactly the size it is (1024 × 1536 for a body or an armor, 1536 × 1024 for every sheet): give it back at exactly that size, laid out exactly as it is, every piece, figure and grey line in exactly the same place, at exactly the same size and angle. Don't crop, pad, zoom, stretch, rotate, flip, re-centre or re-frame it, add no border, and draw nothing bigger, smaller, longer or shorter: paint over the shapes that are there. I'll save it as <part-id>.png.

Use it when Claude's check (§2.3, the size lock) says a picture has to be redone: a piece or a cell cropped off the edge, missing, or drawn far bigger or smaller than its template's. Attach the template again, not the picture that came back.

**Realign** · `<part-id>.png`

> That moved things that should have stayed put. Do the same edit again on the attached base image, but this time leave everything outside the part exactly where it was, pixel for pixel: the same pose, body, head, framing, canvas size and position, and background. Change only the part I asked for. I'll save it as <part-id>.png.

### 1.3 Making the parts

1. **Paint the two bodies first** (§3.0): attach `body-parts-man.png` and paste the `body-man` prompt; then `body-parts-woman.png` and `body-woman`, each in a chat of its own. Send them back as `body-man.png` and `body-woman.png`. Claude cuts each into its fourteen pieces, stands the figure up as `base-male.png` or `base-female.png`, and draws every other template again from it, so everything after is painted on your own bodies.
2. **Every other part is painted on a template** (a sheet): the card says which (its Basis line, and the Art Prompts page's Attach line). Attach that PNG, paste the card's prompt (the page's Copy button gives the whole of it: the sheet's block, the size lock and the list), and send what comes back named exactly as the prompt's last line says. One picture per message. The armors are one outfit to a picture, dressed piece by piece on `armor-parts-man.png` or `armor-parts-woman.png` (§5.0); faces, hair, extras, helms, capes and weapons come four to twelve to a picture (§4.0, §4.9, §4.10, §6.0).
3. **The size lock.** Every template is exactly a size ChatGPT makes (1024 × 1536, or 1536 × 1024 for the sheets of four to twelve), and every prompt says to give it back at exactly that size, laid out exactly as it is. Claude checks each picture against its template before cutting it (§2.3): a little zoom, shift or other size, or a piece drawn a few per cent off, is put back exactly; a piece or a cell that's cropped, missing or redrawn far off comes back to you to paint again with the **Fix the size** follow-up (§1.2).
4. **Characters are recipes** (§7): once their parts exist there's nothing more to draw. **Creatures** (§8) are drawn whole, in their own chats, as before.

The templates are drawn from the bodies stood up at rest, and till the bodies are painted, from the body templates' own pieces stood up the same way: every piece in the same place at the same size, so a sheet painted before the bodies come still cuts. The single cards (one part on the base, 1024 × 1536) are for redoing one part. Morphs resize characters in the game, so a child's or a Stoneborn's armor is drawn on the full-size body. Creatures that are wider than they are tall use a landscape canvas (the Art Guide's §2 allows larger canvases for creatures and bosses) and say how big they are next to a standard adult.

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

The figures are a cut-out ("paper doll") rig (the owner's call, 2026-11-10): each body is painted in its fourteen pieces (§3.0), and `tools/body_parts.py` puts the pieces back together at rest as `tools/source/characters/base-male.png` and `base-female.png`: a bald, tan-skinned, cel-shaded man and woman with featureless faces, in dark underwear, on solid black, in a three-quarter view facing right, the arms a little away from the body with the fists closed, the legs apart, the far arm in shadow. The game puts the pieces on the skeleton and turns each about its joint (`tools/rig/skeleton.py`, `clips.py`).

Every part is painted on a template made from those pieces, never on a blank canvas:

| Template | Size | What's on it | Parts |
|---|---|---|---|
| `body-parts-man`, `body-parts-woman` | 1024 × 1536 | the figure pulled apart into its fourteen pieces | the body (§3.0) |
| `armor-parts-man`, `armor-parts-woman` | 1024 × 1536 | the same, less the head: the thirteen pieces an outfit dresses | each armor, one to a picture (§5.0) |
| `face-sheet-male`, `face-sheet-female` | 1536 × 1024 | ten copies of the head and shoulders | the faces (§4.0) |
| `hair-sheet` | 1536 × 1024 | eight columns, the man from the head to the hips above and the woman below | the hair (§4.9) |
| `extra-sheet`, `extra-sheet-male` | 1536 × 1024 | six columns of the man's and the woman's head and chest; twelve of the man's | the extras (§4.10) |
| `helm-sheet-male`, `helm-sheet-female` | 1536 × 1024 | four copies, the head to the hips, with room over the head | the helms (§6.0) |
| `cape-sheet-male`, `cape-sheet-female` | 1536 × 1024 | four whole figures with room behind the back | the capes (§6.0) |
| `weapon-sheet-male`, `weapon-sheet-female` | 1536 × 1024 | four columns of the two fists alone | what the hands hold (§6.0) |

`python tools/body_parts.py template` and `armor` write the first two from the skeleton; `python tools/part_sheet.py templates` writes the rest from the figure stood up. Till the bodies are painted the sheets show the body templates' own pieces stood up the same way, and they're drawn again from the painted bodies when those come: every piece in the same place at the same size, so a sheet painted on either cuts the same.

The owner's slot map (2026-11-06, the head fixed 2026-11-08): **armor is the body all the way up to the neck, piece by piece, the head is the mannequin's own and never changes shape, and the cape is the back.** The face is only the features painted on that head (eyes, brows, nose, mouth). **Hair and helm are one slot**, what's on the head: a hairstyle is a piece in it just as a helm is, and a helm hides the hair (a helm that wants hair showing has it painted in). The hair, the helm, the face and the extras turn with the head; the cape hangs from the torso; the weapon is what the near hand holds and turns with it. The game stacks the parts in this order, back to front: cape, armor, head, face, extras, hair or helm, ears and horns, weapon, each limb's pieces in the skeleton's own order (which a clip can change: a punch brings the far arm in front).

**The size lock.** ChatGPT redraws the whole picture it's given, and can send it back zoomed, shifted, cropped, at another size, or with a piece drawn bigger than it was. So:

- every template is exactly a size ChatGPT makes (1024 × 1536 or 1536 × 1024), so it has no reason to resize anything;
- every prompt carries the **Size** block below, the tall one for a body or an armor and the wide one for a sheet;
- every picture that comes back is checked against its template before it's cut: `python tools/body_parts.py check <png> <id>` for a body or an armor, `python tools/part_sheet.py check <png> <sheet-id>` for a sheet. A small zoom, shift or other size is put back exactly by the cut, and each body piece is held to its template's size and place (a forearm drawn 6% longer still meets the elbow and the wrist); a piece or a cell that's cropped off the edge, missing, redrawn far off its size, or painted where it'd be lost means the picture is redone with **Fix the size** (§1.2).

**Size (tall)** · bodies and armors

> SIZE. The attached picture is exactly 1024 × 1536 pixels. Give it back at exactly 1024 × 1536, laid out exactly as it is: every piece and figure in exactly the same place, at exactly the same size and the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame the picture, add no border or frame, and draw nothing bigger, smaller, longer or shorter than it is: paint over the shapes that are there.

**Size (wide)** · every sheet

> SIZE. The attached picture is exactly 1536 × 1024 pixels. Give it back at exactly 1536 × 1024, laid out exactly as it is: every piece and figure, and every grey line, in exactly the same place, at exactly the same size and the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame the picture, add no border or frame, and draw nothing bigger, smaller, longer or shorter than it is: paint over the shapes that are there.

**Where the regions fall on the bases.** The values are approximate pixels, measured on the 1024 by 1536 canvas of the figure stood up. The importer uses them; ChatGPT gets the same regions in words.

| Landmark | `base-male` | `base-female` |
|---|---|---|
| Crown of the head | y 112 | y 145 |
| Jawline, where the head meets the neck (the seam between head and armor): a diagonal from under the ear to the chin | x 490, y 262 to x 566, y 304 | x 482, y 292 to x 553, y 330 |
| Neck meets the shoulders | y 335 | y 370 |
| Near fist (lit, on the left of the picture) | x 256–373, y 752–869 | x 270–368, y 756–872 |
| Far fist (in shadow, on the right) | x 676–769, y 748–863 | x 669–753, y 766–869 |
| Soles of the feet | y 1466 | y 1466 |

The back of the figure faces the left of the picture. A cape therefore shows past the figure's left edge, above the shoulders behind the neck, and between and below the legs.

**Setup brief**

Paste this once at the start of each ChatGPT chat, before the first picture. Or put it in a ChatGPT Project's instructions and upload the templates to the Project. Every card's prompt carries its own rules too, so a card still works without the brief.

> I'm making the characters for an original 2D fantasy game, built as a cut-out ("paper doll") rig: every figure is drawn in separate pieces that the game puts on a skeleton and turns at the joints. Every image you make in this chat is an EDIT of a template I attach. Please read this whole brief before the first image, and follow it for every image in this chat.
>
> THE TEMPLATES. Each shows a bald, tan-skinned, cel-shaded mannequin, a man or a woman, with a featureless face, in a three-quarter view facing right, on a solid black background: the near arm and leg are on the left of the picture, the far ones on the right, and the far arm is in shadow. I'll attach one of these:
> - a BODY or an ARMOR template, 1024 × 1536: the figure pulled apart into its pieces, with black gaps between them: the head; the torso, one piece from the neck down to the shorts; each upper arm, forearm and fist; each thigh, shin and foot. An armor template has no head. Repaint or dress every piece exactly where it is, each on its own.
> - a FACE, HAIR, EXTRA or HELM sheet, 1536 × 1024: copies of the figure's head and shoulders (or the head to the hips) in cells divided by thin grey lines. Paint on each head what I list for its cell, numbered left to right along the top row and then the bottom.
> - a CAPE sheet, 1536 × 1024: four whole figures, one to a column. Hang the cape I list behind each.
> - a WEAPON sheet, 1536 × 1024: four columns showing only the two closed fists. Put the item I list through the near (left, lit) fist of each, with no arm or body.
>
> THE SLOTS. Each image is one kind of part:
> 1. ARMOR is the body from the jaw down, piece by piece: each piece gets its own part of the outfit (the torso the shirt or the breastplate and, below it, the belt and the top of the trousers or the skirt, each arm piece its sleeve, each fist a glove or stays bare, each leg piece its trouser leg, each foot its boot). It never touches the head.
> 2. HAIR or HELM is what's on the head, one slot for both. A helm hides the hair entirely; if a helm should show hair, paint that hair in as part of it. Either may hang over the neck and shoulders. Wherever it leaves the face open, keep the blank face exactly as it is. A hair is painted plain mid brown.
> 3. FACE is only the features painted on the blank head: eyes, brows, nose and mouth, clean-shaven, in the base's own tan with mid-brown eyes and brows. The head itself never changes shape.
> 4. EXTRA is one thing on the head on its own: a beard (plain mid brown), a piece of jewellery, a skin mark, or a people's ears or horns.
> 5. CAPE is the back: cloaks, capes, packs, tails. Paint it only in the black behind and around the figure, never over the body. The figure's back faces the LEFT of the picture.
> 6. WEAPON is what the near hand holds: the grip passes through the curled fingers, which stay exactly as they are.
>
> THE SIZE RULE, for every image: give it back at exactly the size of the picture I attached, laid out exactly as it is: every piece, figure and grey line in the same place, at the same size and the same angle. Never crop, pad, zoom, stretch, rotate, flip, re-centre or re-frame the picture, never add a border, and never draw a piece bigger, smaller, longer or shorter than it is. The game lays your picture over the template pixel for pixel, so anything that moves shows as a seam, or a gap at a joint.
>
> THE RULES FOR EVERY IMAGE:
> - Edit the attached image; never redraw it from scratch. Change only what I ask for.
> - Keep the solid black background and the black gaps between the pieces: no piece may touch another.
> - Use the template's art style: crisp dark outlines, flat cel shading in two to four tones per colour, light from the upper left.
> - No text, labels, numbers, guide lines, ground, floor shadow, frame, or extra objects or figures. No glow spilling onto the background unless I ask for it.
> - Original designs only: nothing copied from an existing game, film, comic or brand.
>
> Each request will name what to paint. If a request ever seems to break these rules, follow the rules. Reply "Ready" if you understand, and wait for the first request.

**Every edit**

> Edit the attached image. It is a base character for a 2D game. The game stacks parts painted on it like transparent sheets, so your edit has to line up with it pixel for pixel. Keep the same 1024 × 1536 canvas, the same solid black background, the same pose with the arms a little away from the body and the fists closed, the same body proportions, the same position and size in the frame, and the same three-quarter view facing right. Keep exactly the same art style: crisp dark outlines, flat cel shading, the same light from the upper left, the same colouring. Change only the slot named below, and keep every other pixel exactly as it is. Don't crop, zoom, shift, rotate or flip anything. No text, no ground, no shadow on the floor, no extra objects or people.

**Armor pieces**

> This is an ARMOR, painted in pieces. The attached picture is a body for a 2D game's cut-out character rig, pulled apart into its thirteen pieces on a black background, everything but the head: the torso, one piece from the neck down to the shorts; and for each arm the upper arm, the forearm and the closed fist, for each leg the thigh, the shin and the foot. The figure faces right in three-quarter view: the near arm and leg are on the left of the picture, the far arm and leg on the right, and the far arm is in shadow. The game puts the pieces back together on a skeleton and turns each one at its joint, so dress every piece on its own, exactly where it is, at its size and its angle, in the outfit described below: the torso gets the shirt, the coat's body, the breastplate and the collar at the neck and, where its shorts are, the belt and the top of the trousers, the shorts or the skirt; each upper arm and forearm its own part of the sleeve (a pauldron goes on the upper arm); each fist a glove, or it stays bare and closed; each thigh and shin its own part of the trouser leg, the skirt or the coat's tails; each foot its boot or shoe. A skirt, a robe or a long coat is split the same way, its top on the torso and the rest over each thigh and shin. Draw every piece whole, as it would look if nothing were in front of it, its round ends at the joints dressed too, so it can turn under the next piece without a gap. Loose clothes may make a piece a little bigger than the body under it, but keep the black gaps: no piece may touch another, and nothing is painted in the gaps or where the head would be. Where the outfit leaves skin bare, keep the body's own warm tan skin. Don't add a head, a helmet, a hood, a cape or a weapon: they're parts of their own. Use exactly the template's art style: thick deep-brown outlines, flat cel shading, light from the upper left. No text, labels or numbers.

**Helm**

> This is a HELM, which goes in the head's one slot: hair and helm are the same slot, so a helm replaces the hair entirely. Put the helmet, hood, hat, crown or mask described below on the blank bald head, fitted to its exact size, angle and position. It may hang down over the neck and shoulders (a hood's drape, a mail coif, a plume, a veil). If the description shows hair (under a hat's brim, round a circlet or a crown), paint that hair in as part of the helm; otherwise add none. Where the helm leaves the face open, keep the blank face exactly as it is, because the game puts each character's own face there. Don't change the body or the underwear. The helm:

**Cape**

> This is a CAPE, which is the back. Add the cape, cloak or back piece described below, hanging behind the body from the shoulders (or the waist). The game draws it behind the figure, so paint it only in the black space behind and around the body. In this pose the figure's back is toward the LEFT of the picture. So the cape shows past the left edge of the shoulders, back and legs, above the shoulders behind the neck, and between and below the legs. Don't paint over the body, the arms, the head or the underwear at all, and put no straps, clasps or collars across the front of the body. The cape:

**Hair**

> This is a HAIR, which goes in the head's one slot with the helms. Give the blank bald head the hairstyle described below, fitted to its exact size, angle and position, and paint it a plain mid brown (the game recolours it for each character who wears it). Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Don't change the body or the underwear. The hair:

**Hair sheet**

> This is a HAIR SHEET: eight equal columns divided by thin grey lines, on a solid black background, each column the same blank man from the head to the hips in its top cell and the same blank woman in its bottom cell. Give both heads in each column the hairstyle listed for it, the same style on the man and on the woman, following the numbered list below; the columns are numbered 1 to 8 from left to right. Paint every style a plain mid brown. Fit each to its head's exact size, angle and position, and keep every head, face and body exactly where it is, as in a single edit. Long hair may fall over the shoulders and down the back, but keep each hairstyle inside its own cell, and keep the grey lines and the black exactly as they are.

**Extra**

> This is an EXTRA: one thing on the head that isn't the face or the hair. Paint only the thing described below on the blank head, exactly where it sits (a beard on the jaw, an earring at the ear, spectacles on the nose, a scar on the skin, ears or horns rising from the head), fitted to the head's exact size, angle and position. Change nothing else: keep the blank face, the head's outline, the neck and the body exactly as they are. The extra:

**Extra sheet**

> This is an EXTRA SHEET: six equal columns divided by thin grey lines, on a solid black background, each column the same blank man's head, neck and chest in its top cell and the same blank woman's in its bottom cell (a beard sheet is twelve cells, all of the man). Paint the one extra listed for each column on both heads (on a beard sheet, the one listed for each cell, numbered 1 to 6 along the top row and 7 to 12 along the bottom), exactly where it sits, and nothing else. Keep every head, face and body exactly where it is, as in a single edit, keep each extra inside its own cell, and keep the grey lines and the black exactly as they are.

**Weapon**

> This is a WEAPON, which is what the near hand holds. The near hand is the lit fist on the LEFT of the picture; the fist on the right, in shadow, is the far hand. Put the weapon described below in the near fist at a natural resting angle. The grip passes through the curled fingers, the fingers stay exactly as they are and close over it, and the weapon shows above and below the fist. Keep the whole weapon inside the canvas. Don't change the body, the head or the underwear. A shield or a second item goes on the far arm only if the description says so. The weapon:

**Face**

> This is a FACE, which is only the features. Paint the features described below onto the blank head: the eyes, eyebrows, nose and mouth, clean-shaven (a beard is an extra). The head never changes: keep its outline, size, angle and position, the skull and the ears. Paint no hair on the head; the hair is a part of its own. This face is shared by many characters, so paint the skin in the base's own warm tan, the eyes and eyebrows a mid brown; the game recolours them for each. The neck belongs to the armor, so leave the neck, the body and the underwear exactly as they are. The face:

**Face sheet**

> This is a FACE SHEET: ten copies of the same blank base character's head and shoulders, in a grid of five columns and two rows of equal cells divided by thin grey lines, on a solid black background. Paint a different face onto the blank head in each cell, following the numbered list below. The cells are numbered left to right, 1 to 5 along the top row and 6 to 10 along the bottom row. In every cell, keep the head exactly where it is, at the same size, angle and position, with the same head outline, skull and ears, in a three-quarter view facing right. Paint only the features (eyes, eyebrows, nose and mouth), clean-shaven, and no hair: the hair and any beard are parts of their own. Make the ten faces clearly different people: different shapes, ages and characters. Paint every one with the skin in the base's own warm tan and the eyes and brows a mid brown, since the game recolours them. Keep the grey lines, the black and the shoulders exactly as they are.

**Helm sheet**

> This is a HELM SHEET: four copies of the same blank base character from the top of the head to the hips, side by side in four equal columns divided by thin grey lines, on a solid black background. Put a different helm, hood, hat, crown or mask on each head, following the numbered list below; the figures are numbered 1 to 4 from left to right. Hair and helm are one slot: a helm replaces the hair, so if one should show hair (under a brim, round a circlet or a crown), paint that hair in as part of it; otherwise add none. Fit each to its head's exact size, angle and position, and where it leaves the face open, keep the blank face exactly as it is. A helm may hang down over the neck and shoulders (a hood's drape, a mail coif, a plume, a veil), but keep each inside its own column, keep every head and body exactly where it is, as in a single edit, and keep the grey lines and the black exactly as they are.

**Cape sheet**

> This is a CAPE SHEET: four copies of the same base character standing full length, side by side in four equal columns divided by thin grey lines, on a solid black background. Give each figure a different cape, cloak, tail or back piece, following the numbered list below; the figures are numbered 1 to 4 from left to right. A cape hangs behind the body: paint it only in the black space behind and around each figure, never over the body, the arms, the head or the underwear, and put no straps, clasps or collars across the front of the body. Each figure's back faces the LEFT of its column, so a cape shows past the left edge of the shoulders, back and legs, above the shoulders behind the neck, and between and below the legs. Keep every figure exactly where it is, at the same size and in the same pose, with the same bald head and face; keep each cape inside its own column, never across a grey line; and keep the grey lines and the black exactly as they are.

**Weapon sheet**

> This is a WEAPON SHEET: four columns divided by thin grey lines, on a solid black background. Each column shows the same two closed fists, each with a short stub of forearm, floating where a character's hands are; the rest of the figure is left out on purpose. The lit fist on the LEFT of each column is the near hand; the shadowed fist on the RIGHT is the far hand. Give each column a different weapon or held item, following the numbered list below; the columns are numbered 1 to 4 from left to right. Hold every item the way a 2D game character holds one, with no pose at all: the closed fist grips it as it is, the grip passing straight through the curled fingers, which stay exactly as they are, and the item shows above and below the fist. A weapon is held low with its point or head forward, toward the right, unless the list says otherwise. A carried thing (a basket, a pot, a lantern) hangs straight down from the fist by its handle. A shield is worn on the far forearm, its middle just above the far fist. The far fist holds something only when the list says so. Don't draw arms, a body or a head, and don't move, turn, open or redraw the fists. Keep each item whole and inside its own column, never across a grey line, and keep the grey lines and the black exactly as they are.

Each sheet's prompt is its block above, then **Size (wide)**, then the sheet's list; each armor's is **Armor pieces**, then **Size (tall)**, then the outfit (§5.0); a single card's is **Every edit**, its slot's block and the card, then **Size (tall)**.

**Creature**

> Use the attached image ONLY as a reference for the art style (line weight, cel shading, colouring, light) and for scale (a standard adult is that tall). Draw a new creature, full body, facing right, on a solid black background, in exactly that style. The creature:

### 2.4 The vector look

The owner's call (2026-11-08): the parts are drawn in ChatGPT as clean vector-style art, flat colours, one clean outline and hard-edged shading, so each one is right in one go and traces cleanly in `tools/vectorize.py`. The Art Prompts page's **Vector look** switch (on by default) wraps every part's prompt in the two blocks below, so each picture is one message: attach the picture the card names and paste. ChatGPT makes PNGs, not vector files; the vector shapes are `tools/vectorize.py`'s, which also brings the parts painted before this into the same look.

**Before every card**

> VECTOR LOOK. Draw what you add as clean vector-style 2D game art, as if it were made in a vector illustration program: every shape a closed area of flat colour with crisp, smooth edges; one clean outline round every shape, deep brown, almost black, never pure black, heavier on the outer silhouette (about 3 px) and lighter inside (about 1.5 px), in smooth, confident curves with no sketchy or doubled lines; cel shading in flat shapes only, each material with its base colour, one or two hard-edged shadow shapes and at most one small flat highlight, lit from the upper left; three or four flat tones per material, in bright, rich, harmonious colours; details such as belts, buckles, seams and trims as clear flat pieces that still read when small. No gradients, no airbrushing, no soft or blurred shadows, no texture, no noise, no brush strokes. Wherever the instructions below say to keep the base's art style, keep its line positions, proportions, light and colours, and draw in this vector look. Everything else in the instructions below holds exactly.

**After every card**

> Before you send it, check: the picture is exactly the size of the one I attached, every piece and figure is exactly where it was and the same size (nothing cropped, zoomed, moved, stretched or drawn bigger), nothing outside the part changed, every new edge is crisp, there are no gradients or textures, and the background and the gaps between the pieces are still flat black. Fix anything that fails before you send it.

**The vector setup brief.** Optional, since every wrapped card is complete on its own: paste it into a ChatGPT Project's instructions, with the templates uploaded, to keep a run of parts consistent.

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
> THE TEMPLATES
> I'll attach a template: a bald, tan-skinned mannequin with a featureless face, man or woman, in a three-quarter view facing right, on a solid black background, the near arm and leg on the left of the picture, the far arm in shadow. The game is a cut-out rig: it puts the figure's pieces on a skeleton and turns them at the joints, and stacks your images over the template pixel for pixel.
> - BODY or ARMOR template, 1024 x 1536: the figure pulled apart into its pieces with black gaps between them (head; torso; hips; each upper arm, forearm and fist; each thigh, shin and foot; an armor template has no head). Repaint or dress every piece where it is, on its own, whole, its round joint ends included; never let pieces touch.
> - FACE / HAIR / EXTRA / HELM sheet, 1536 x 1024: heads (with the shoulders, or to the hips) in cells divided by thin grey lines; paint each cell's item on its head.
> - CAPE sheet, 1536 x 1024: four whole figures; hang each cape behind its figure.
> - WEAPON sheet, 1536 x 1024: four columns of the two closed fists only; put each item through the near (left) fist, with no arm or body.
>
> THE SLOTS (each image is one kind of part)
> 1. ARMOR is the body from the jaw down, piece by piece: each piece gets its own part of the outfit. Where the outfit leaves skin, keep the body's skin. It never touches the head.
> 2. HAIR or HELM is what sits on the head, one slot for both. A helm hides the hair; if a helm should show hair, paint that hair in as part of it. Wherever it leaves the face open, keep the blank face exactly as it is. A hair is painted plain mid brown.
> 3. FACE is only the features painted on the blank head: eyes, brows, nose and mouth, clean-shaven, in the template's tan with mid-brown eyes and brows. The head never changes shape.
> 4. EXTRA is one thing on the head on its own: a beard (plain mid brown), a piece of jewellery, a skin mark, or a people's ears or horns.
> 5. CAPE is the back: paint it only in the black behind and around the figure, never over the body. The figure's back faces the LEFT of the picture.
> 6. WEAPON is what the near hand holds: the grip passes through the curled fingers, which stay exactly as they are.
>
> THE SIZE RULE
> Give every image back at exactly the size of the picture I attached, laid out exactly as it is: every piece, figure and grey line in the same place, at the same size and angle. Never crop, pad, zoom, stretch, rotate, flip, re-centre or re-frame it, never add a border, and never draw a piece bigger, smaller, longer or shorter.
>
> THE RULES FOR EVERY IMAGE
> - EDIT the attached image; never redraw it from scratch. Change only the slot I ask for and keep every other pixel as it is.
> - Same canvas size, same flat pure-black background, same pieces in the same places at the same sizes, the same black gaps between them. Don't crop, zoom, shift, rotate or flip anything.
> - Where a request says "the template's art style" or "the same art style as the base", read it as: its line positions, proportions, light and colours, drawn in the vector look above.
> - No text, letters, numbers, labels, signature or watermark. No ground, floor shadow, frame or border. No extra objects or figures. No glow spilling onto the background unless I ask for it.
> - Original designs only: nothing copied from an existing game, film, comic or brand.
> - Give me one PNG at the same size as the image I attached.
>
> Before you send each image, check it: it's exactly the size of the picture I attached, no piece or figure has moved or changed size, only the requested part changed, every edge is crisp, there are no gradients or textures, and the background is still flat black. If any check fails, fix it before sending.
>
> Each request will name the slot and describe the part. If a request seems to break these rules, follow these rules.

## 3. Base

### 3.0 The mannequins in pieces

The two bodies are painted in pieces, not as whole figures: a cut-out ("paper doll") rig, in which each
piece is drawn whole on its own and turns about its joint on the skeleton, and everything worn is drawn
the same way, piece by piece. Each body's template (`tools/source/characters/body-parts-man.png` and
`body-parts-woman.png`, made by `python tools/body_parts.py template` from the mannequin before them)
is the figure pulled apart into its fourteen pieces, in the pose and the proportions the skeleton, the
sockets and every clip are made for; `python tools/body_parts.py check <png> body-man` (or `body-woman`)
says whether ChatGPT kept to it (§2.3, the size lock), and `python tools/body_parts.py cut <png> body-man`
puts each painted piece back where it belongs, held to its template's size and place, and stands the
figure up as `base-male.png` (or `base-female.png`), which every other template is drawn from.

#### `body-man` · The man in pieces · P0

**Basis:** Edits `body-parts-man` (the owner's call on 2026-11-10: a cut-out rig, the torso one piece, the same view; on 2026-11-11: no hips piece, the torso runs from the neck to the shorts).

Edit the attached picture. It shows a jointed man's mannequin for a 2D game's cut-out character rig, pulled apart into its fourteen pieces on a black background: the head; the torso, one piece from the neck down to the end of its dark grey boxer shorts, which have a light band round the waist and the legs; and for each arm the upper arm, the forearm and the closed fist, for each leg the thigh, the shin and the foot. The figure faces right in three-quarter view: the near arm and leg are on the left of the picture, the far arm and leg on the right, and the far arm is in shadow. Repaint every piece as a clean, finished piece of a bald mannequin with a featureless face (no eyes, nose, mouth or ears), each in exactly the same place, at exactly the same size and at the same angle as the piece already there, and keep the black gaps between them: no piece may touch another. Draw each piece whole, as it would look if nothing were in front of it, and finish it with a smooth round end wherever it meets the next piece (the neck, the shoulders, elbows, wrists, hips, knees and ankles), so it can turn under that piece without a gap. Keep the proportions and the muscle shapes as they are. Paint it as clean vector-style 2D game art: thick deep-brown outlines, flat hard-edged cel shading in two or three tones, light from the upper left, a warm tan skin, smooth curves, no texture, no gradients, no blur. Keep the solid black background; add nothing else: no labels, numbers, guide lines, joints drawn as circles, ground shadow or extra pieces. SIZE. The attached picture is exactly 1024 × 1536 pixels. Give it back at exactly 1024 × 1536, laid out exactly as it is: every piece and figure in exactly the same place, at exactly the same size and the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame the picture, add no border or frame, and draw nothing bigger, smaller, longer or shorter than it is: paint over the shapes that are there. I'll save it as body-man.png.

#### `body-woman` · The woman in pieces · P0

**Basis:** Edits `body-parts-woman` (the owner's call on 2026-11-10: a cut-out rig, the torso one piece, the same view; on 2026-11-11: no hips piece, the torso runs from the neck to the shorts).

Edit the attached picture. It shows a jointed woman's mannequin for a 2D game's cut-out character rig, pulled apart into its fourteen pieces on a black background: the head; the torso, one piece from the neck down to her dark grey briefs, with her top, a dark grey strapless band, across the chest; and for each arm the upper arm, the forearm and the closed fist, for each leg the thigh, the shin and the foot. The figure faces right in three-quarter view: the near arm and leg are on the left of the picture, the far arm and leg on the right, and the far arm is in shadow. Repaint every piece as a clean, finished piece of a bald mannequin with a featureless face (no eyes, nose, mouth or ears), each in exactly the same place, at exactly the same size and at the same angle as the piece already there, and keep the black gaps between them: no piece may touch another. Draw each piece whole, as it would look if nothing were in front of it, and finish it with a smooth round end wherever it meets the next piece (the neck, the shoulders, elbows, wrists, hips, knees and ankles), so it can turn under that piece without a gap. Keep the proportions and the muscle shapes as they are. Paint it as clean vector-style 2D game art: thick deep-brown outlines, flat hard-edged cel shading in two or three tones, light from the upper left, a warm tan skin, smooth curves, no texture, no gradients, no blur. Keep the solid black background; add nothing else: no labels, numbers, guide lines, joints drawn as circles, ground shadow or extra pieces. SIZE. The attached picture is exactly 1024 × 1536 pixels. Give it back at exactly 1024 × 1536, laid out exactly as it is: every piece and figure in exactly the same place, at exactly the same size and the same angle. Don't crop, pad, zoom, stretch, squash, rotate, flip, re-centre or re-frame the picture, add no border or frame, and draw nothing bigger, smaller, longer or shorter than it is: paint over the shapes that are there. I'll save it as body-woman.png.

### 3.1 The mannequin (whole, retired)

The whole figures these were before §3.0's pieces, kept for the record; `tools/body_parts.py cut` now writes `base-male.png` and `base-female.png` from the pieces.

#### `base-male` · The base mannequin, man · P0

**Basis:** The owner's jointed mannequins (2026-11-09), made both at once in one picture and stood on their own canvases by `tools/split_bases.py`; the owner's slot model (2026-11-06); `03-art-overhaul.md` §2.1 and §2.2 (the angle and proportions).

The base mannequin for a 2D game's character system, built from separate pieces that turn on ball joints, like an artist's wooden mannequin or a jointed doll, so each piece can be rotated on its own in cut-out animation. Every face, outfit, cape, helm and weapon will be painted onto this exact image by editing it, so it must be plain and exact. A man's mannequin: natural, athletic shoulders, chest, waist and hips, not bulky, the standard build, about five and a quarter heads tall. The head is bald with a smooth, featureless face, and the body is smooth between the joints, warm tan, in plain dark underwear, drawn in clean cel shading with crisp dark outlines and the light from the upper left. THE JOINTS: each is a clean round ball with a thin outline seam where one piece meets the next. The top of each upper arm is a full round ball set into the shoulder, the chest's outline curving round it; at the elbows and knees both pieces end round, centred on the joint, the lower one's round end tucked under the upper one; a thin seam ring where the hand meets the forearm and the foot meets the shin; and the pelvis ends in a smooth curved line across the top of each thigh, each thigh's top round under it. THE POSE: three-quarter view facing right, both arms held slightly away from the body with a clear gap of black between each arm and the torso from the armpit to the fist, the far arm just clear of the far side of the body and in deep shadow, the fists closed as if round a grip, the legs a little apart in an easy stride, the near leg slightly forward. Output: PNG, 1024×1536, on a solid black background. One figure, the full body centred with the feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: a standard adult man, about 85% of the canvas height. An original design. I'll save it as base-male.png.

#### `base-female` · The base mannequin, woman · P0

**Basis:** The owner's jointed mannequins (2026-11-09), made both at once in one picture and stood on their own canvases by `tools/split_bases.py`; the owner's slot model (2026-11-06); `03-art-overhaul.md` §2.1 and §2.2 (the angle and proportions).

The base mannequin for a 2D game's character system, built from separate pieces that turn on ball joints, like an artist's wooden mannequin or a jointed doll, so each piece can be rotated on its own in cut-out animation. Every face, outfit, cape, helm and weapon will be painted onto this exact image by editing it, so it must be plain and exact. A woman's mannequin: narrower shoulders, a defined waist, wider hips and a modest bust, the standard build, about five and a quarter heads tall. The head is bald with a smooth, featureless face, and the body is smooth between the joints, warm tan, in plain dark underwear, drawn in clean cel shading with crisp dark outlines and the light from the upper left. THE JOINTS: each is a clean round ball with a thin outline seam where one piece meets the next. The top of each upper arm is a full round ball set into the shoulder, the chest's outline curving round it; at the elbows and knees both pieces end round, centred on the joint, the lower one's round end tucked under the upper one; a thin seam ring where the hand meets the forearm and the foot meets the shin; and the pelvis ends in a smooth curved line across the top of each thigh, each thigh's top round under it. THE POSE: three-quarter view facing right, both arms held slightly away from the body with a clear gap of black between each arm and the torso from the armpit to the fist, the far arm just clear of the far side of the body and in deep shadow, the fists closed as if round a grip, the legs a little apart in an easy stride, the near leg slightly forward. Output: PNG, 1024×1536, on a solid black background. One figure, the full body centred with the feet near the bottom, nothing cropped. No text, no frame, no ground shadow. Scale: a standard adult woman, a little shorter than the man, about 85% of the canvas height. An original design. I'll save it as base-female.png.

#### `base-male-blank` · The blank mannequin, man · P0

**Basis:** Edits `base-male`. The head everyone's drawn with: every face, hair and helm is painted on it.

Nothing to paint: the jointed mannequin's face is featureless already, so `base-male-blank.png` is the same picture as `base-male.png`, and `tools/split_bases.py` writes both. (For a base with a face, the edit would take the features off and keep the bald head's outline, size, angle and position exactly.)

#### `base-female-blank` · The blank mannequin, woman · P0

**Basis:** Edits `base-female`. The head everyone's drawn with: every face, hair and helm is painted on it.

Nothing to paint: the jointed mannequin's face is featureless already, so `base-female-blank.png` is the same picture as `base-female.png`, and `tools/split_bases.py` writes both. (For a base with a face, the edit would take the features off and keep the bald head's outline, size, angle and position exactly.)

### 3.2 Builds and peoples

There's no separate base for any build or people: a build is a morph (`03-art-overhaul.md` §3.5).

| Build | Morph (height × width) | Who |
| --- | --- | --- |
| `small` | 0.72 × 0.80; children of eight about 0.62 × 0.66 | Hearthlings, children, thornlings |
| `lean` | 1.04 × 0.90; very tall people up to 1.10 × 0.85 | Sylvans, Mira, Valencia, Elara, Crane, Severin (Vorlaith 1.20 × 0.95) |
| `standard` | 1.00 × 1.00 | Humans, Emberkin, Wildkin, the default hero |
| `stout` | 0.85 × 1.30 | Stoneborn (Brannock 1.36 wide, Grell 1.55) |
| `big` | 1.08 × 1.15 | Thugs, knights, heavy guards; the constructs 1.20 × 1.30 |

What marks a people goes with the hair, or with the hood or helm worn in its place: Sylvan pointed ears, Emberkin horns, Wildkin beast ears or feathers. The faces are shared (§4), so a Stoneborn or a Hearthling wears one of the set, made stout or small by the morph and coloured by §7. The Emberkin's and Wildkin's tails go in the cape slot (`cape-tail-…`), so a character with a tail and a cloak needs a cape that shows both (`cape-tamsin`).

---

### 3.3 The holding hands (retired)

Retired with the jointed mannequins (2026-11-09). The figures are animated now (`tools/rig/skeleton.py`): each body and armor is cut into its bones' pieces, and what a hand holds is drawn just under that hand's own piece, so the fist, skin or glove as it's painted, closes over the grip and turns with it. There's nothing to paint here.

---

## 4. Faces

The owner's call (2026-11-09): **ten men's faces and ten women's faces, shared by everyone.** A face is only the features painted on the blank head of the base whose sex matches (§3): eyes, brows, nose and mouth. Beards, earrings, scars and freckles are extras (§4.10), so every face is clean-shaven and any of them goes with any face. The head itself never changes, and the hair is a part of its own (§4.8). Each face is painted in the base's own warm tan with mid-brown eyes and brows, and every character who wears it gets their own skin, hair and eye colours from §7's Colours column (the player picks theirs on the creation screen), so the same face reads as many people. The ten run from a child to the old, so a child, a youth, a soldier, a schemer and an elder each have one that fits; a child's face is drawn at the base's size and the morph makes it small. The faces painted before 2026-11-09, one per character, are retired: §7 gives everyone a face from the set.

What marks a people isn't on the face any more: Sylvan pointed ears, Emberkin horns and Wildkin beast ears or feathers are painted with the character's hair, or with the hood or helm they wear instead (§3.2). The heads that aren't faces at all (the constructs, the walking dead, the glass king) keep a head of their own (§4.3).

### 4.0 Face sheets (ten at a time)

Two sheets make the whole set, one for each base. The attached template (`tools/source/characters/face-sheet-male.png` or `face-sheet-female.png`, made by `python tools/part_sheet.py templates`) is ten copies of the head and shoulders of the body stood up (§2.3) at full size, five across and two down, so each face lines up with the base just as a single edit would. `python tools/part_sheet.py check <png> <sheet-id>` says whether ChatGPT kept to the template, and `python tools/part_sheet.py fit <png> <sheet-id>` puts each cell back on its base as `tools/source/characters/faces/<face-id>.png`. The single cards in §4.1 and §4.2 are for redoing one face.

The cells are numbered left to right, 1 to 5 along the top row and 6 to 10 along the bottom.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `face-sheet-m01` | `base-male` | P0 | `face-m01`, `face-m02`, `face-m03`, `face-m04`, `face-m05`, `face-m06`, `face-m07`, `face-m08`, `face-m09`, `face-m10` |
| `face-sheet-f01` | `base-female` | P0 | `face-f01`, `face-f02`, `face-f03`, `face-f04`, `face-f05`, `face-f06`, `face-f07`, `face-f08`, `face-f09`, `face-f10` |

#### `face-sheet-m01` · Face sheet: the ten men's faces (Man 1, the boy, Man 2, the lad, Man 3, the hero, Man 4, the rogue, Man 5, the soldier, Man 6, the bruiser, Man 7, the kindly man, Man 8, the noble, Man 9, the veteran, Man 10, the elder) · P0

**Basis:** Edits `face-sheet-male`. Cells: 1 `face-m01`, 2 `face-m02`, 3 `face-m03`, 4 `face-m04`, 5 `face-m05`, 6 `face-m06`, 7 `face-m07`, 8 `face-m08`, 9 `face-m09`, 10 `face-m10`.

1. Man 1, the boy: a boy of about ten (the game's morph makes a child small, so draw the face at the base's size): a round child's face, big bright eyes, a small snub nose, full cheeks, a light dusting of freckles, an open, curious look.
2. Man 2, the lad: a lad of about seventeen: a smooth, eager face, wide-open eyes, a soft jaw not yet filled out, a straight nose a little big for the face, clean-shaven.
3. Man 3, the hero: a young man of about twenty-four: an open, likeable oval face, a straight nose, steady eyes under straight, expressive brows, a firm, friendly mouth, clean-shaven.
4. Man 4, the rogue: a young man of about twenty-five: a narrow, sharp-featured face, high cheekbones, keen, narrowed eyes under angled brows, a thin nose, one corner of the mouth lifted in a knowing half-smile, clean-shaven.
5. Man 5, the soldier: a man of about thirty-eight: a square-jawed, steady face, level eyes under a strong, straight brow, a broad straight nose, a firm mouth, clean-shaven.
6. Man 6, the bruiser: a heavy-set man of about thirty-five: a broad, blunt face, a thick brow ridge over small, hard eyes, a flattened nose broken more than once, a heavy, square jaw.
7. Man 7, the kindly man: a man of about forty-five: a round, pleasant face, soft cheeks, kind eyes with laugh lines at the corners, a short nose, a small, contented smile.
8. Man 8, the noble: a man of about fifty: a long, narrow, refined face, high cheekbones, cool, heavy-lidded eyes under finely arched brows, a long straight nose, thin lips held in a polite, guarded line, clean-shaven.
9. Man 9, the veteran: a weathered man of about fifty-five: a craggy, lined face, deep-set eyes with crow's feet under thick, fierce brows, a crooked nose, an old nick through one brow, a firm, weathered mouth.
10. Man 10, the elder: an old man of about seventy: a deeply lined, gentle face, drooping lids over kind eyes, bushy brows, a big nose, hollowed cheeks, a thin, gentle mouth.
Paint every face clean-shaven, with the skin in the base's own warm tan and the eyes and eyebrows a mid brown: the game recolours each for the character wearing it.
I'll save it as face-sheet-m01.png.

#### `face-sheet-f01` · Face sheet: the ten women's faces (Woman 1, the girl, Woman 2, the maiden, Woman 3, the heroine, Woman 4, the quick wit, Woman 5, the lady, Woman 6, the gentle soul, Woman 7, the fighter, Woman 8, the matron, Woman 9, the matriarch, Woman 10, the grandmother) · P0

**Basis:** Edits `face-sheet-female`. Cells: 1 `face-f01`, 2 `face-f02`, 3 `face-f03`, 4 `face-f04`, 5 `face-f05`, 6 `face-f06`, 7 `face-f07`, 8 `face-f08`, 9 `face-f09`, 10 `face-f10`.

1. Woman 1, the girl: a girl of about ten (the game's morph makes a child small, so draw the face at the base's size): a round child's face, big bright eyes, a small snub nose, full cheeks, a dusting of freckles, a quick, cheeky look.
2. Woman 2, the maiden: a girl of about seventeen: a soft, youthful face, wide eyes with long lashes, gentle brows, a small nose, a shy, uncertain mouth.
3. Woman 3, the heroine: a young woman of about twenty-four: an open, likeable oval face, a straight nose, warm eyes with defined lashes, straight brows, a friendly mouth.
4. Woman 4, the quick wit: a young woman of about twenty-four: a heart-shaped face, sharp, bright eyes under arched brows, a small pointed chin, one corner of the mouth lifted as if about to tease.
5. Woman 5, the lady: a woman of about twenty-eight: a fine oval face with high cheekbones, clear, cool eyes under straight, elegant brows, a narrow nose, a composed, unsmiling mouth.
6. Woman 6, the gentle soul: a woman of about thirty-five: a soft, round face, gentle eyes under softly worried brows, rounded cheeks, a small, kind mouth.
7. Woman 7, the fighter: a woman of about forty: a lean, strong face, a firm jaw, level, steady eyes under straight brows, a straight nose, a set mouth, a small old scar on the chin.
8. Woman 8, the matron: a woman of about fifty-five: a broad, weathered face, crinkled eyes with deep laugh lines, apple cheeks, a broad nose, a wide, good-humoured mouth.
9. Woman 9, the matriarch: a woman of about sixty: a lean, sharp face, deep lines round the mouth, steady, piercing eyes under thin brows, a long nose, thin lips pressed together.
10. Woman 10, the grandmother: an old woman of about seventy-five: a soft, round, kindly face, many fine wrinkles, gentle eyes in deep creases, a small nose, a sweet, tired smile.
Paint every face clean-shaven, with the skin in the base's own warm tan and the eyes and eyebrows a mid brown: the game recolours each for the character wearing it.
I'll save it as face-sheet-f01.png.

### 4.1 The men's faces

#### `face-m01` · Man 1, the boy · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a boy of about ten (the game's morph makes a child small, so draw the face at the base's size) onto the blank head: a round child's face, big bright eyes, a small snub nose, full cheeks, a light dusting of freckles, an open, curious look. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m01.png.

#### `face-m02` · Man 2, the lad · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a lad of about seventeen onto the blank head: a smooth, eager face, wide-open eyes, a soft jaw not yet filled out, a straight nose a little big for the face, clean-shaven. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m02.png.

#### `face-m03` · Man 3, the hero · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a young man of about twenty-four onto the blank head: an open, likeable oval face, a straight nose, steady eyes under straight, expressive brows, a firm, friendly mouth, clean-shaven. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m03.png.

#### `face-m04` · Man 4, the rogue · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a young man of about twenty-five onto the blank head: a narrow, sharp-featured face, high cheekbones, keen, narrowed eyes under angled brows, a thin nose, one corner of the mouth lifted in a knowing half-smile, clean-shaven. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m04.png.

#### `face-m05` · Man 5, the soldier · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a man of about thirty-eight onto the blank head: a square-jawed, steady face, level eyes under a strong, straight brow, a broad straight nose, a firm mouth, clean-shaven. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m05.png.

#### `face-m06` · Man 6, the bruiser · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a heavy-set man of about thirty-five onto the blank head: a broad, blunt face, a thick brow ridge over small, hard eyes, a flattened nose broken more than once, a heavy, square jaw. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m06.png.

#### `face-m07` · Man 7, the kindly man · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a man of about forty-five onto the blank head: a round, pleasant face, soft cheeks, kind eyes with laugh lines at the corners, a short nose, a small, contented smile. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m07.png.

#### `face-m08` · Man 8, the noble · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a man of about fifty onto the blank head: a long, narrow, refined face, high cheekbones, cool, heavy-lidded eyes under finely arched brows, a long straight nose, thin lips held in a polite, guarded line, clean-shaven. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m08.png.

#### `face-m09` · Man 9, the veteran · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a weathered man of about fifty-five onto the blank head: a craggy, lined face, deep-set eyes with crow's feet under thick, fierce brows, a crooked nose, an old nick through one brow, a firm, weathered mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m09.png.

#### `face-m10` · Man 10, the elder · P0

**Basis:** Edits `base-male-blank`. One of the ten men's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of an old man of about seventy onto the blank head: a deeply lined, gentle face, drooping lids over kind eyes, bushy brows, a big nose, hollowed cheeks, a thin, gentle mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-m10.png.

### 4.2 The women's faces

#### `face-f01` · Woman 1, the girl · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a girl of about ten (the game's morph makes a child small, so draw the face at the base's size) onto the blank head: a round child's face, big bright eyes, a small snub nose, full cheeks, a dusting of freckles, a quick, cheeky look. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f01.png.

#### `face-f02` · Woman 2, the maiden · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a girl of about seventeen onto the blank head: a soft, youthful face, wide eyes with long lashes, gentle brows, a small nose, a shy, uncertain mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f02.png.

#### `face-f03` · Woman 3, the heroine · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a young woman of about twenty-four onto the blank head: an open, likeable oval face, a straight nose, warm eyes with defined lashes, straight brows, a friendly mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f03.png.

#### `face-f04` · Woman 4, the quick wit · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a young woman of about twenty-four onto the blank head: a heart-shaped face, sharp, bright eyes under arched brows, a small pointed chin, one corner of the mouth lifted as if about to tease. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f04.png.

#### `face-f05` · Woman 5, the lady · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a woman of about twenty-eight onto the blank head: a fine oval face with high cheekbones, clear, cool eyes under straight, elegant brows, a narrow nose, a composed, unsmiling mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f05.png.

#### `face-f06` · Woman 6, the gentle soul · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a woman of about thirty-five onto the blank head: a soft, round face, gentle eyes under softly worried brows, rounded cheeks, a small, kind mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f06.png.

#### `face-f07` · Woman 7, the fighter · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a woman of about forty onto the blank head: a lean, strong face, a firm jaw, level, steady eyes under straight brows, a straight nose, a set mouth, a small old scar on the chin. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f07.png.

#### `face-f08` · Woman 8, the matron · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a woman of about fifty-five onto the blank head: a broad, weathered face, crinkled eyes with deep laugh lines, apple cheeks, a broad nose, a wide, good-humoured mouth. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f08.png.

#### `face-f09` · Woman 9, the matriarch · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of a woman of about sixty onto the blank head: a lean, sharp face, deep lines round the mouth, steady, piercing eyes under thin brows, a long nose, thin lips pressed together. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f09.png.

#### `face-f10` · Woman 10, the grandmother · P0

**Basis:** Edits `base-female-blank`. One of the ten women's faces anyone can wear (§4.0).

Edit the attached blank base image: change ONLY the face. Paint the face of an old woman of about seventy-five onto the blank head: a soft, round, kindly face, many fine wrinkles, gentle eyes in deep creases, a small nose, a sweet, tired smile. The expression is neutral and relaxed, eyes open, looking ahead. Paint the skin in the base's own warm tan, the eyes a mid brown and the eyebrows a mid brown: many characters wear this face, and the game recolours the skin, the eyes and the hair for each. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-f10.png.

### 4.3 Heads that aren't faces

The humanoid foes whose heads aren't a person's: each is its own part, painted with its own colours, and nobody else wears it.

#### `face-vorlaith` · Vorlaith, the Unmade King · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §7.4 (cracked glass with light inside).

Edit the attached blank base image: change ONLY the face. Paint the face of Vorlaith, an ancient being of living glass onto the blank head: a head of clear, colourless glass cracked with fine branching fractures, a cold white light drifting inside it like smoke, smooth, calm, faintly amused features, and two points of white light for eyes. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-vorlaith.png.

#### `face-hollowed` · The Hollowed · P0

**Basis:** Edits `base-male-blank`. From the art plan (ash skin, cracks lit ember, empty sockets with ember points); `03-art-overhaul.md` §7.3.

Edit the attached blank base image: change ONLY the face. Paint the face of a Hollowed, a dead soldier raised as an empty husk onto the blank head: a gaunt, lipless head with skin like ash-grey burnt paper, cracked all over in fine lines that glow ember orange from inside; empty, dark eye sockets with a single ember-orange point of light in each; a few wisps of scorched hair. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-hollowed.png.

#### `face-crown-hollowed` · Crown-raised Hollowed · P2

**Basis:** Edits `face-hollowed`. From `03-art-overhaul.md` §9 (violet eye light).

Edit the attached image (face-hollowed.png, the face on the base): change ONLY the colour of the light: the glowing cracks and the points of light in the eye sockets are violet instead of ember orange. Keep everything else, the rest of the face and hair, the body, the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-crown-hollowed.png.

---

#### `face-construct-vault` · Vault construct's head · P0

**Basis:** Edits `base-male-blank`. From the art plan (a sun-disc face).

Edit the attached blank base image: change ONLY the face. Paint the face of an ancient stone guardian: the head is not a face at all onto the blank head: in place of the head, a flat, round disc of polished sun-gold metal with a simple sunburst engraved on it and no other features, set on a short neck of pale dressed stone with thin gold inlay. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-vault.png.

#### `face-construct-priest` · Priest-construct's head · P2

**Basis:** Edits `base-male-blank`. **(derived)** From `03b-world-art.md` (`priest-construct`).

Edit the attached blank base image: change ONLY the face. Paint the face of a stone guardian of a ruined sun temple onto the blank head: in place of a face, a round, polished bronze mirror, glinting, set inside a hood of sun-bleached sandstone carved like a priest's. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-priest.png.

#### `face-construct-crown` · Crown-construct's head · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (a crown motif for a face).

Edit the attached blank base image: change ONLY the face. Paint the face of a war machine shaped like a knight onto the blank head: no face: in its place, a stylised gold crown shape, with a violet light glowing behind the gaps between its points, on a neck of polished dark violet metal. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-construct-crown.png.

#### `face-drowned` · Drowned · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (grey-green, kelp hair).

Edit the attached blank base image: change ONLY the face. Paint the face of a drowned corpse that walks out of the sea onto the blank head: a puffy, slack, grey-green waterlogged face, milky white eyes, the mouth hanging open, barnacles crusted on the jaw. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-drowned.png.

#### `face-blood-wight` · Blood-wight · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (thin, grey).

Edit the attached blank base image: change ONLY the face. Paint the face of an undead thing that walks under a darkened sun onto the blank head: a skull-like head of taut, papery ash-grey skin, sunken red eyes, long fangs, thin grey hair. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-blood-wight.png.

#### `face-sun-scorched` · Sun-scorched · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (burned to the skin).

Edit the attached blank base image: change ONLY the face. Paint the face of one of the burned dead of a desert massacre onto the blank head: a blackened face cracked like dried mud, the cracks glowing a faint hot gold, the features half gone, two pale, blazing eyes, the mouth open. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-sun-scorched.png.

#### `face-thornling` · Thornling · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (a bark-skinned sprite).

Edit the attached blank base image: change ONLY the face. Paint the face of a small, spiteful forest sprite onto the blank head: a knot of rough dark bark for a head, with a hollow, sly face, two glowing yellow-green eyes and a mouth like a split in a branch, and a crown of small twigs and new leaves. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-thornling.png.

#### `face-unmade` · The Unmade · P2

**Basis:** Edits `base-male-blank`. From `03-art-overhaul.md` §9 (no face: smooth dark glass where it would be).

Edit the attached blank base image: change ONLY the face. Paint the face of a soldier pulled out of time onto the blank head: where the face should be, a smooth, featureless oval of dark, glossy glass reflecting a faint white light; the rest of the head and neck faded and slightly greyed, as if not quite here. The expression is neutral and relaxed, eyes open, looking ahead. Keep the bald head exactly as it is, its outline, size, angle and position, the skull and the ears, except where the description gives pointed ears or horns. Paint no hair on the head and no beard: only the eyebrows. The hair and any beard are parts of their own. Nothing else below the jaw changes: the neck belongs to the armor. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as face-unmade.png.

### 4.8 Hair

The owner's call (2026-11-09): **a shared hair library, painted in bulk, every style fitting both the man and the woman.** Like the faces, the hairs are worn by everyone: §7 gives each character a style and colours it to them (the Colours column's hair). Each style is painted on both blank bases at once (§4.9), the man's copy `hair-NN` and the woman's `hair-NN-f`, and the game puts on the copy for the figure's own base. Every style is painted a plain mid brown, for the game to recolour. Hair and helm are still one slot: a helm replaces the hair. The player's creation screen offers the whole library. What used to be painted with a character's hair (a beard, a people's ears or horns) is an extra now (§4.10), so it shows with any hair or helm. The per-character hairs of before 2026-11-09 are retired. `tools/import_parts.py` cuts them from `tools/source/characters/hairs/`.

#### `hair-01` · Hair 1, the buzz · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-01-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: cropped very close all over, a shadow of hair on the scalp. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-01.png.

#### `hair-02` · Hair 2, the crop · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-02-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short and neat, cropped close at the back and sides, a little longer on top. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-02.png.

#### `hair-03` · Hair 3, short and messy · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-03-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short and messy, tousled locks falling over the forehead and round the ears. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-03.png.

#### `hair-04` · Hair 4, short curls · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-04-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short, tight curls close to the head. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-04.png.

#### `hair-05` · Hair 5, the side parting · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-05-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short and neatly combed, with a side parting. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-05.png.

#### `hair-06` · Hair 6, slicked back · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-06-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: slicked straight back from the forehead, close to the head, to the nape. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-06.png.

#### `hair-07` · Hair 7, the undercut · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-07-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: an undercut: the sides shaved short, the longer top swept back. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-07.png.

#### `hair-08` · Hair 8, spiky · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-08-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: short and spiky, sticking up untidily. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-08.png.

#### `hair-09` · Hair 9, the short topknot · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-09-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: the sides shaved, the top tied in a short topknot at the crown. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-09.png.

#### `hair-10` · Hair 10, receding · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-10-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: receding and thinning, combed back, short at the sides. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-10.png.

#### `hair-11` · Hair 11, the bald crown · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-11-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bald on top, with a short fringe of hair round the sides and the back. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-11.png.

#### `hair-12` · Hair 12, bald with wisps · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-12-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: bald, with a few thin wisps over the crown. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-12.png.

#### `hair-13` · Hair 13, the pixie cut · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-13-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a short pixie cut, feathered, with a side-swept fringe. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-13.png.

#### `hair-14` · Hair 14, the mop · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-14-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a thick, untidy mop falling over the forehead and the ears. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-14.png.

#### `hair-15` · Hair 15, the fringed bob · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-15-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a straight, chin-length bob with a blunt fringe. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-15.png.

#### `hair-16` · Hair 16, the choppy cut · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-16-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: jaw-length, choppy and layered, with no fringe, as if cut with a knife. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-16.png.

#### `hair-17` · Hair 17, shaggy · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-17-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: shaggy and layered, to the jaw, falling into the eyes. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-17.png.

#### `hair-18` · Hair 18, wavy to the jaw · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-18-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: wavy, to the jaw, parted to one side. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-18.png.

#### `hair-19` · Hair 19, shoulder curls · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-19-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a thick mass of curls to the shoulders. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-19.png.

#### `hair-20` · Hair 20, the afro · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-20-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a full, rounded afro. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-20.png.

#### `hair-21` · Hair 21, straight to the shoulders · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-21-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: straight, to the shoulders, parted in the middle and tucked behind the ear. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-21.png.

#### `hair-22` · Hair 22, the short tail · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-22-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: to the shoulders, tied back in a short, low ponytail, loose strands round the face. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-22.png.

#### `hair-23` · Hair 23, windswept · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-23-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: wild and windswept, to the shoulders. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-23.png.

#### `hair-24` · Hair 24, swept back · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-24-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: swept back from the forehead, to the collar. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-24.png.

#### `hair-25` · Hair 25, the half-up knot · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-25-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: to the shoulders, the top half tied back in a small knot. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-25.png.

#### `hair-26` · Hair 26, pulled back tight · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-26-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: pulled back tightly from the face into a short tail at the nape. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-26.png.

#### `hair-27` · Hair 27, long and straight · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-27-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long and straight, loose to the middle of the back, parted in the middle. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-27.png.

#### `hair-28` · Hair 28, long, over one shoulder · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-28-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long and straight, to the middle of the back, parted to one side and swept over the near shoulder. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-28.png.

#### `hair-29` · Hair 29, long and wavy · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-29-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long and wavy, loose past the shoulders. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-29.png.

#### `hair-30` · Hair 30, long curls · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-30-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, loose curls past the shoulders. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-30.png.

#### `hair-31` · Hair 31, the high ponytail · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-31-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, in a high ponytail that falls past the shoulder blades, a few loose strands framing the face. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-31.png.

#### `hair-32` · Hair 32, the low tail · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-32-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, tied at the nape in a loose tail down the back. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-32.png.

#### `hair-33` · Hair 33, the shoulder braid · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-33-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: one long, thick braid over the near shoulder to the waist. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-33.png.

#### `hair-34` · Hair 34, the back braid · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-34-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: one thick braid down the back. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-34.png.

#### `hair-35` · Hair 35, two plaits · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-35-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: two long plaits, one over each shoulder. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-35.png.

#### `hair-36` · Hair 36, two bunches · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-36-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: two short bunches tied high at the sides of the head. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-36.png.

#### `hair-37` · Hair 37, the low bun · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-37-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: pinned up in a neat, low bun at the nape. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-37.png.

#### `hair-38` · Hair 38, the messy bun · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-38-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: pinned up in a loose, messy bun, strands escaping round the face and neck. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-38.png.

#### `hair-39` · Hair 39, the high bun · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-39-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: pulled up into a tight, high bun on the crown. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-39.png.

#### `hair-40` · Hair 40, the crown braid · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-40-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: braided and pinned round the head like a crown. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-40.png.

#### `hair-41` · Hair 41, the braided updo · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-41-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: braided and pinned up at the back, a few strands loose. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-41.png.

#### `hair-42` · Hair 42, the warrior's topknot · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-42-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: the sides shaved short, the long top tied back in a warrior's topknot. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-42.png.

#### `hair-43` · Hair 43, the mohawk · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-43-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: a mohawk: the sides shaved, a short spiked crest along the top. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-43.png.

#### `hair-44` · Hair 44, locs · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-44-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: thick locs to the shoulders, tied back loosely. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-44.png.

#### `hair-45` · Hair 45, many braids · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-45-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: many long, thin braids, tied back at the nape. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-45.png.

#### `hair-46` · Hair 46, to the waist · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-46-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: very long and straight, to the waist. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-46.png.

#### `hair-47` · Hair 47, long and wild · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-47-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, wild and tangled, past the shoulders. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-47.png.

#### `hair-48` · Hair 48, long and wispy · P0

**Basis:** Edits `base-male-blank`. One of the hair library's styles (§4.8); the woman's copy is `hair-48-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY the hair. Give the bald head this hair: long, thin and wispy, to the shoulders, receding at the brow. Paint it a plain mid brown, since the game recolours it for each character. Fit it to the head's exact size, angle and position. Keep the blank face, the ears and the head's outline exactly as they are wherever the hair doesn't cover them. Long hair may fall over the shoulders and down the back, but nothing else below the jaw changes. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as hair-48.png.

### 4.9 Hair sheets (eight styles at a time, on both bases)

The library eight styles at a time, each sheet one ChatGPT picture. The attached template (`tools/source/characters/hair-sheet.png`, made by `python tools/part_sheet.py templates`) is the man stood up (§2.3) eight times along the top and the woman eight times along the bottom, each from the top of the head to the hips at half size (the size the game keeps its parts, so nothing's lost), so a long braid has room. Each column is one style: ChatGPT paints it on the man above and the woman below. `python tools/part_sheet.py check <png> <sheet-id>` says whether ChatGPT kept to the template, and `python tools/part_sheet.py fit <png> <sheet-id>` puts each back on its base as `tools/source/characters/hairs/<id>.png` (the man's) and `<id>-f.png` (the woman's).

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `hair-sheet-01` | `both` | P0 | `hair-01`, `hair-02`, `hair-03`, `hair-04`, `hair-05`, `hair-06`, `hair-07`, `hair-08` |
| `hair-sheet-02` | `both` | P0 | `hair-09`, `hair-10`, `hair-11`, `hair-12`, `hair-13`, `hair-14`, `hair-15`, `hair-16` |
| `hair-sheet-03` | `both` | P0 | `hair-17`, `hair-18`, `hair-19`, `hair-20`, `hair-21`, `hair-22`, `hair-23`, `hair-24` |
| `hair-sheet-04` | `both` | P0 | `hair-25`, `hair-26`, `hair-27`, `hair-28`, `hair-29`, `hair-30`, `hair-31`, `hair-32` |
| `hair-sheet-05` | `both` | P0 | `hair-33`, `hair-34`, `hair-35`, `hair-36`, `hair-37`, `hair-38`, `hair-39`, `hair-40` |
| `hair-sheet-06` | `both` | P0 | `hair-41`, `hair-42`, `hair-43`, `hair-44`, `hair-45`, `hair-46`, `hair-47`, `hair-48` |

#### `hair-sheet-01` · Hair sheet 1: hair 1, hair 2, hair 3, hair 4, hair 5, hair 6, hair 7, hair 8 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-01`, 2 `hair-02`, 3 `hair-03`, 4 `hair-04`, 5 `hair-05`, 6 `hair-06`, 7 `hair-07`, 8 `hair-08`.

1. Cropped very close all over, a shadow of hair on the scalp.
2. Short and neat, cropped close at the back and sides, a little longer on top.
3. Short and messy, tousled locks falling over the forehead and round the ears.
4. Short, tight curls close to the head.
5. Short and neatly combed, with a side parting.
6. Slicked straight back from the forehead, close to the head, to the nape.
7. An undercut: the sides shaved short, the longer top swept back.
8. Short and spiky, sticking up untidily.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-01.png.

#### `hair-sheet-02` · Hair sheet 2: hair 9, hair 10, hair 11, hair 12, hair 13, hair 14, hair 15, hair 16 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-09`, 2 `hair-10`, 3 `hair-11`, 4 `hair-12`, 5 `hair-13`, 6 `hair-14`, 7 `hair-15`, 8 `hair-16`.

1. The sides shaved, the top tied in a short topknot at the crown.
2. Receding and thinning, combed back, short at the sides.
3. Bald on top, with a short fringe of hair round the sides and the back.
4. Bald, with a few thin wisps over the crown.
5. A short pixie cut, feathered, with a side-swept fringe.
6. A thick, untidy mop falling over the forehead and the ears.
7. A straight, chin-length bob with a blunt fringe.
8. Jaw-length, choppy and layered, with no fringe, as if cut with a knife.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-02.png.

#### `hair-sheet-03` · Hair sheet 3: hair 17, hair 18, hair 19, hair 20, hair 21, hair 22, hair 23, hair 24 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-17`, 2 `hair-18`, 3 `hair-19`, 4 `hair-20`, 5 `hair-21`, 6 `hair-22`, 7 `hair-23`, 8 `hair-24`.

1. Shaggy and layered, to the jaw, falling into the eyes.
2. Wavy, to the jaw, parted to one side.
3. A thick mass of curls to the shoulders.
4. A full, rounded afro.
5. Straight, to the shoulders, parted in the middle and tucked behind the ear.
6. To the shoulders, tied back in a short, low ponytail, loose strands round the face.
7. Wild and windswept, to the shoulders.
8. Swept back from the forehead, to the collar.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-03.png.

#### `hair-sheet-04` · Hair sheet 4: hair 25, hair 26, hair 27, hair 28, hair 29, hair 30, hair 31, hair 32 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-25`, 2 `hair-26`, 3 `hair-27`, 4 `hair-28`, 5 `hair-29`, 6 `hair-30`, 7 `hair-31`, 8 `hair-32`.

1. To the shoulders, the top half tied back in a small knot.
2. Pulled back tightly from the face into a short tail at the nape.
3. Long and straight, loose to the middle of the back, parted in the middle.
4. Long and straight, to the middle of the back, parted to one side and swept over the near shoulder.
5. Long and wavy, loose past the shoulders.
6. Long, loose curls past the shoulders.
7. Long, in a high ponytail that falls past the shoulder blades, a few loose strands framing the face.
8. Long, tied at the nape in a loose tail down the back.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-04.png.

#### `hair-sheet-05` · Hair sheet 5: hair 33, hair 34, hair 35, hair 36, hair 37, hair 38, hair 39, hair 40 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-33`, 2 `hair-34`, 3 `hair-35`, 4 `hair-36`, 5 `hair-37`, 6 `hair-38`, 7 `hair-39`, 8 `hair-40`.

1. One long, thick braid over the near shoulder to the waist.
2. One thick braid down the back.
3. Two long plaits, one over each shoulder.
4. Two short bunches tied high at the sides of the head.
5. Pinned up in a neat, low bun at the nape.
6. Pinned up in a loose, messy bun, strands escaping round the face and neck.
7. Pulled up into a tight, high bun on the crown.
8. Braided and pinned round the head like a crown.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-05.png.

#### `hair-sheet-06` · Hair sheet 6: hair 41, hair 42, hair 43, hair 44, hair 45, hair 46, hair 47, hair 48 · P0

**Basis:** Edits `hair-sheet`. Columns: 1 `hair-41`, 2 `hair-42`, 3 `hair-43`, 4 `hair-44`, 5 `hair-45`, 6 `hair-46`, 7 `hair-47`, 8 `hair-48`.

1. Braided and pinned up at the back, a few strands loose.
2. The sides shaved short, the long top tied back in a warrior's topknot.
3. A mohawk: the sides shaved, a short spiked crest along the top.
4. Thick locs to the shoulders, tied back loosely.
5. Many long, thin braids, tied back at the nape.
6. Very long and straight, to the waist.
7. Long, wild and tangled, past the shoulders.
8. Long, thin and wispy, to the shoulders, receding at the brow.
Paint every style a plain mid brown, the same style on the man above and the woman below.
I'll save it as hair-sheet-06.png.

### 4.10 Extras

What goes on the head that isn't the face or the hair, so it shows whatever hair or helm is worn: **beards and moustaches** (`beard-*`, the man's), **jewellery and the like** (`jewel-*`: earrings, a nose ring, spectacles, an eyepatch, a monocle, a quill behind the ear), **skin marks** (`skin-*`: freckles, scars, a tattoo, ash) and **a people's ears and horns** (`ears-*`, `horns-*`: a Sylvan's pointed ears, a Wildkin's beast ears or feathers, an Emberkin's horns). Beards, jewellery and skin marks are drawn over the face and under the hair; ears and horns over the hair or the helm (a hood included). A character can wear any number of them (§7's Extras column). All but the beards are painted on both bases (the woman's copy `-f`). A beard is recoloured to the character's hair, a skin mark and pointed ears to their skin, fur to their hair; jewellery and horns keep their own colours. `tools/import_parts.py` cuts them from `tools/source/characters/extras/`.

The extra sheets (`tools/source/characters/extra-sheet.png` for both bases, the man's six above and the woman's six below, one item a column; `extra-sheet-male.png` for the beards, twelve on the man) hold each head, neck and chest at three-quarter size. `python tools/part_sheet.py check <png> <sheet-id>`, then `fit`, cuts them into `tools/source/characters/extras/`.

| Sheet | Base | Priority | Cells, in order |
|---|---|---|---|
| `extra-sheet-01` | `base-male` | P0 | `beard-01`, `beard-02`, `beard-03`, `beard-04`, `beard-05`, `beard-06`, `beard-07`, `beard-08`, `beard-09`, `beard-10`, `beard-11`, `beard-12` |
| `extra-sheet-02` | `both` | P0 | `jewel-01`, `jewel-02`, `jewel-03`, `jewel-04`, `jewel-05`, `jewel-06` |
| `extra-sheet-03` | `both` | P0 | `jewel-07`, `jewel-08`, `jewel-09`, `jewel-10`, `jewel-11`, `jewel-12` |
| `extra-sheet-04` | `both` | P0 | `skin-01`, `skin-02`, `skin-03`, `skin-04`, `skin-05`, `skin-06` |
| `extra-sheet-05` | `both` | P0 | `ears-01`, `ears-02`, `ears-03`, `ears-04`, `ears-05`, `ears-06` |
| `extra-sheet-06` | `both` | P0 | `horns-01`, `horns-02`, `horns-03`, `horns-04`, `horns-05`, `horns-06` |
| `extra-sheet-07` | `both` | P2 | `jewel-13`, `jewel-14`, `jewel-15`, `jewel-16`, `jewel-17`, `jewel-18` |
| `extra-sheet-08` | `both` | P2 | `jewel-19`, `jewel-20`, `jewel-21`, `jewel-22`, `jewel-23`, `jewel-24` |

#### `extra-sheet-01` · Extra sheet: the twelve beards and moustaches · P0

**Basis:** Edits `extra-sheet-male`. Cells: 1 `beard-01`, 2 `beard-02`, 3 `beard-03`, 4 `beard-04`, 5 `beard-05`, 6 `beard-06`, 7 `beard-07`, 8 `beard-08`, 9 `beard-09`, 10 `beard-10`, 11 `beard-11`, 12 `beard-12`.

1. A few days' stubble along the jaw, the chin and the upper lip.
2. A thin, neatly trimmed moustache.
3. A neat, full moustache.
4. A thick moustache drooping past the corners of the mouth.
5. A goatee: a short chin beard joined to a moustache.
6. A narrow beard along the jawline, with no moustache.
7. A short, neatly trimmed full beard and moustache.
8. A full, bushy beard and moustache.
9. A long, full beard falling to the chest, with a moustache.
10. A full beard braided into one thick braid that falls to the chest, with a moustache.
11. Mutton-chop side-whiskers down the cheeks, the chin shaved.
12. A scruffy, uneven beard, a few weeks' growth.
Each cell is one item, numbered 1 to 6 along the top row and 7 to 12 along the bottom, all on the man.
I'll save it as extra-sheet-01.png.

#### `extra-sheet-02` · Extra sheet: earrings and piercings · P0

**Basis:** Edits `extra-sheet`. Columns: 1 `jewel-01`, 2 `jewel-02`, 3 `jewel-03`, 4 `jewel-04`, 5 `jewel-05`, 6 `jewel-06`.

1. A small gold hoop in the ear.
2. A large gold hoop earring.
3. A small silver stud in the ear lobe.
4. A dangling gold earring with a small blue gem at the end.
5. Three small silver rings up the edge of the ear.
6. A feather earring hanging to the jaw.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-02.png.

#### `extra-sheet-03` · Extra sheet: spectacles, an eyepatch and more · P0

**Basis:** Edits `extra-sheet`. Columns: 1 `jewel-07`, 2 `jewel-08`, 3 `jewel-09`, 4 `jewel-10`, 5 `jewel-11`, 6 `jewel-12`.

1. A small gold ring through the side of the nose.
2. Small round wire-rimmed spectacles.
3. A black leather eyepatch over the near eye, its strap running round the head.
4. A gold-rimmed monocle over the near eye, on a thin gold chain.
5. A quill pen tucked behind the ear.
6. A single pearl earring.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-03.png.

#### `extra-sheet-04` · Extra sheet: freckles, scars and marks · P0

**Basis:** Edits `extra-sheet`. Columns: 1 `skin-01`, 2 `skin-02`, 3 `skin-03`, 4 `skin-04`, 5 `skin-05`, 6 `skin-06`.

1. A dusting of freckles over the nose and cheeks.
2. A thin, pale scar through one eyebrow and down the cheek.
3. An old scar across the bridge of the nose.
4. A burn scar along the jaw.
5. A small dark tattoo of a bird in flight on the side of the scalp.
6. A smudge of grey ash or soot across the cheek and the brow.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-04.png.

#### `extra-sheet-05` · Extra sheet: a people's ears · P0

**Basis:** Edits `extra-sheet`. Columns: 1 `ears-01`, 2 `ears-02`, 3 `ears-03`, 4 `ears-04`, 5 `ears-05`, 6 `ears-06`.

1. Long, elegant pointed ears swept back from the sides of the head, in the head's own skin.
2. Two tall, pointed wolf's ears standing on top of the head, furred.
3. Two small, round, furry bear's ears on top of the head.
4. Two pointed fox's ears on top of the head, furred, with pale fur inside.
5. In place of ears, two swept-back tufts of barred brown-and-cream feathers growing from the top of the head.
6. Ears a little longer than a person's, ending in short points, in the head's own skin.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-05.png.

#### `extra-sheet-06` · Extra sheet: horns · P0

**Basis:** Edits `extra-sheet`. Columns: 1 `horns-01`, 2 `horns-02`, 3 `horns-03`, 4 `horns-04`, 5 `horns-05`, 6 `horns-06`.

1. Two dark, ridged horns curling round from the temples like a ram's, old bone going ivory at the tips.
2. Two dark, ridged horns sweeping straight back from the temples in a long curve, going pale like old ivory at the tips.
3. Two short, straight dark horns rising from the brow.
4. Two horns curving out and up from the temples like a bull's, pale with dark tips.
5. Small branching antlers, the colour of old wood, rising from the top of the head.
6. Two small dark horns curling back behind the ears.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-06.png.

#### `extra-sheet-07` · Extra sheet: piercings, ears and nose · P2

**Basis:** Edits `extra-sheet`. Columns: 1 `jewel-13`, 2 `jewel-14`, 3 `jewel-15`, 4 `jewel-16`, 5 `jewel-17`, 6 `jewel-18`.

1. A row of small silver studs up the edge of the ear, and one at the lobe.
2. A silver bar through the top of the ear, with a stud and a ring lower down.
3. A silver cuff and small rings up the ear, and a square stud at the lobe.
4. A silver ring through the septum and a small stud in the nostril.
5. Two silver studs through the eyebrow and one under the lower lip.
6. Silver rings through the eyebrow, the nostril and both corners of the lower lip.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-07.png.

#### `extra-sheet-08` · Extra sheet: piercings, more · P2

**Basis:** Edits `extra-sheet`. Columns: 1 `jewel-19`, 2 `jewel-20`, 3 `jewel-21`, 4 `jewel-22`, 5 `jewel-23`, 6 `jewel-24`.

1. Gold cuffs and small rings up the edge of the ear.
2. A round gold plug stretching the earlobe.
3. Two small silver studs through the eyebrow.
4. A beaded silver ring through the septum.
5. Two silver studs under the lower lip, one either side.
6. Gold cuffs up the ear with a small drop at the lobe, a stud in the nostril and one under the lip.
Each column is one item, on the man above and the woman below.
I'll save it as extra-sheet-08.png.

#### `beard-01` · Stubble · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a few days' stubble along the jaw, the chin and the upper lip. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-01.png.

#### `beard-02` · The thin moustache · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a thin, neatly trimmed moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-02.png.

#### `beard-03` · The moustache · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a neat, full moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-03.png.

#### `beard-04` · The drooping moustache · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a thick moustache drooping past the corners of the mouth. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-04.png.

#### `beard-05` · The goatee · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a goatee: a short chin beard joined to a moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-05.png.

#### `beard-06` · The chin strap · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a narrow beard along the jawline, with no moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-06.png.

#### `beard-07` · The short beard · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a short, neatly trimmed full beard and moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-07.png.

#### `beard-08` · The full beard · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a full, bushy beard and moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-08.png.

#### `beard-09` · The long beard · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a long, full beard falling to the chest, with a moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-09.png.

#### `beard-10` · The braided beard · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a full beard braided into one thick braid that falls to the chest, with a moustache. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-10.png.

#### `beard-11` · The side-whiskers · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: mutton-chop side-whiskers down the cheeks, the chin shaved. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-11.png.

#### `beard-12` · The scruffy beard · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10).

Edit the attached blank base image: change ONLY this: a scruffy, uneven beard, a few weeks' growth. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as beard-12.png.

#### `jewel-01` · The small hoop · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-01-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a small gold hoop in the ear. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-01.png.

#### `jewel-02` · The big hoop · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-02-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a large gold hoop earring. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-02.png.

#### `jewel-03` · The stud · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-03-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a small silver stud in the ear lobe. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-03.png.

#### `jewel-04` · The drop · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-04-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a dangling gold earring with a small blue gem at the end. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-04.png.

#### `jewel-05` · The ear rings · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-05-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: three small silver rings up the edge of the ear. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-05.png.

#### `jewel-06` · The feather · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-06-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a feather earring hanging to the jaw. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-06.png.

#### `jewel-07` · The nose ring · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-07-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a small gold ring through the side of the nose. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-07.png.

#### `jewel-08` · The spectacles · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-08-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: small round wire-rimmed spectacles. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-08.png.

#### `jewel-09` · The eyepatch · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-09-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a black leather eyepatch over the near eye, its strap running round the head. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-09.png.

#### `jewel-10` · The monocle · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-10-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a gold-rimmed monocle over the near eye, on a thin gold chain. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-10.png.

#### `jewel-11` · The quill · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-11-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a quill pen tucked behind the ear. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-11.png.

#### `jewel-12` · The pearl · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-12-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a single pearl earring. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-12.png.

#### `jewel-13` · Ear studs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-13-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a row of small silver studs up the edge of the ear, and one at the lobe. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-13.png.

#### `jewel-14` · The industrial bar · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-14-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a silver bar through the top of the ear, with a stud and a ring lower down. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-14.png.

#### `jewel-15` · Ear cuffs and studs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-15-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a silver cuff and small rings up the ear, and a square stud at the lobe. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-15.png.

#### `jewel-16` · The septum ring · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-16-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a silver ring through the septum and a small stud in the nostril. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-16.png.

#### `jewel-17` · Brow and lip studs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-17-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two silver studs through the eyebrow and one under the lower lip. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-17.png.

#### `jewel-18` · Rings · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-18-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: silver rings through the eyebrow, the nostril and both corners of the lower lip. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-18.png.

#### `jewel-19` · Gold ear cuffs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-19-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: gold cuffs and small rings up the edge of the ear. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-19.png.

#### `jewel-20` · The ear plug · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-20-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a round gold plug stretching the earlobe. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-20.png.

#### `jewel-21` · Brow studs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-21-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two small silver studs through the eyebrow. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-21.png.

#### `jewel-22` · The beaded septum ring · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-22-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a beaded silver ring through the septum. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-22.png.

#### `jewel-23` · Lip studs · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-23-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two silver studs under the lower lip, one either side. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-23.png.

#### `jewel-24` · Mixed piercings · P2

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `jewel-24-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: gold cuffs up the ear with a small drop at the lobe, a stud in the nostril and one under the lip. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in its own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as jewel-24.png.

#### `skin-01` · Freckles · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-01-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a dusting of freckles over the nose and cheeks. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-01.png.

#### `skin-02` · The brow scar · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-02-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a thin, pale scar through one eyebrow and down the cheek. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-02.png.

#### `skin-03` · The nose scar · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-03-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: an old scar across the bridge of the nose. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-03.png.

#### `skin-04` · The burn · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-04-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a burn scar along the jaw. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-04.png.

#### `skin-05` · The tattoo · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-05-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a small dark tattoo of a bird in flight on the side of the scalp. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-05.png.

#### `skin-06` · The ash · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `skin-06-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: a smudge of grey ash or soot across the cheek and the brow. It goes over the face and under the hair, so paint it on the blank head as it is. Paint it in the base's own warm tan skin tones (and any ink or soot in its own colour), since the game tints it to each character's skin. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as skin-06.png.

#### `ears-01` · Pointed ears · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-01-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: long, elegant pointed ears swept back from the sides of the head, in the head's own skin. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-01.png.

#### `ears-02` · Wolf's ears · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-02-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two tall, pointed wolf's ears standing on top of the head, furred. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-02.png.

#### `ears-03` · Bear's ears · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-03-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two small, round, furry bear's ears on top of the head. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-03.png.

#### `ears-04` · Fox's ears · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-04-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two pointed fox's ears on top of the head, furred, with pale fur inside. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-04.png.

#### `ears-05` · Feather tufts · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-05-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: in place of ears, two swept-back tufts of barred brown-and-cream feathers growing from the top of the head. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-05.png.

#### `ears-06` · Short points · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `ears-06-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: ears a little longer than a person's, ending in short points, in the head's own skin. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Pointed ears are the head's own skin, which the game tints; paint fur a plain mid brown, since the game recolours it to each character's hair. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as ears-06.png.

#### `horns-01` · Ram's horns · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-01-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two dark, ridged horns curling round from the temples like a ram's, old bone going ivory at the tips. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-01.png.

#### `horns-02` · Swept horns · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-02-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two dark, ridged horns sweeping straight back from the temples in a long curve, going pale like old ivory at the tips. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-02.png.

#### `horns-03` · Short horns · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-03-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two short, straight dark horns rising from the brow. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-03.png.

#### `horns-04` · Bull's horns · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-04-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two horns curving out and up from the temples like a bull's, pale with dark tips. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-04.png.

#### `horns-05` · Antlers · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-05-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: small branching antlers, the colour of old wood, rising from the top of the head. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-05.png.

#### `horns-06` · Curled horns · P0

**Basis:** Edits `base-male-blank`. One of the extras (§4.10). The woman's copy is `horns-06-f`, on `base-female-blank`.

Edit the attached blank base image: change ONLY this: two small dark horns curling back behind the ears. It goes over the hair or the helm, so paint it rising from the head as if nothing were there. Paint them in their own colours. Keep the blank face, the head's outline and everything else exactly as they are. Keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as horns-06.png.


## 5. Armors

The outfits described across the cast, deduplicated: a unique armor for each notable character, shared armors for groups, and the armors in the item catalog. Each one dresses the body's pieces (§5.0).

### 5.0 Armors in pieces (one to a picture)

The owner's call (2026-11-10, a cut-out rig): every armor is painted in pieces, on its base's armor template (`tools/source/characters/armor-parts-man.png` or `armor-parts-woman.png`, written by `python tools/body_parts.py armor`): the body's template less its head, the thirteen pieces an outfit dresses, in their places, 1024 × 1536. One outfit to a picture, since each is thirteen pieces and they need the room. Each card's prompt is §2.3's **Armor pieces** block, then **Size (tall)**, then the outfit; the variants that change another armor attach that armor's own picture instead, and say so. `python tools/body_parts.py check <png> <armor-id>` says whether ChatGPT kept the template's size and layout (every piece covering its body piece, nothing lost in the gaps), and `python tools/body_parts.py cut <png> <armor-id>` puts each piece back at rest as `tools/source/characters/armors/<armor-id>/<bone>.png`. The armors of the bodies that aren't flesh (the Hollowed, the constructs, the drowned, the glass king) are painted the same way: the whole body, skin and all, piece by piece.

The four-to-a-picture armor sheets of 2026-11-07 are retired with the whole figures. Make the armors in the order below (§5.1 onward), P0 first.

### 5.1 The player and the Soul-Bound

#### `armor-traveller` · The traveller (the player's default) · P0

**Basis:** Edits `armor-parts-man`. From the guide figure PLAYER (BASE); `03-art-overhaul.md` §7.2 (Act 1); the item `travelling-clothes`.

Dress the pieces in a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-traveller.png.

#### `armor-traveller-f` · The traveller, on the woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-traveller`, fitted to the woman's base.

Dress the pieces in a practical traveller's outfit: a cream linen shirt with the sleeves rolled to the elbow and the collar open; a fitted navy-blue waistcoat with small brass buttons, a little worn at the edges; a brown leather belt with a brass buckle, a pouch and a small sheathed knife; worn brown wool trousers, patched at one knee; scuffed brown leather boots to mid-calf; brown fingerless leather gloves; and a small herring whittled from pale driftwood on a waxed cord round the neck, resting on the chest. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-traveller-f.png.

#### `armor-sb-traveller` · The Soul-Bound's traveller · P0

**Basis:** Edits `armor-parts-woman`. From the guide figure SERA (burgundy, a dark cape); `Origins.cs` (crimson); `03-art-overhaul.md` §6.4 (the herring, the whittling knife).

Dress the pieces in a fitted, knee-length coat-tunic in deep brick crimson (warm and bright, not a wine colour) with a high collar, a row of dark buttons and dark trim, split at the hips; a cream shirt under it, the cuffs showing; a dark leather belt with two pouches and, at the back, a short whittling knife with a cord through its handle; charcoal-grey fitted trousers; tall dark-brown boots to the knee; dark fingerless gloves; and a small driftwood herring on a waxed cord at the neck. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-sb-traveller.png.

#### `armor-champion` · The Champion's tabard · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From `03-art-overhaul.md` §7.2 (a Champion's tabard over the traveller's clothes, gold Soul-Bond trim) and §7.3 (sun gold).

Dress the pieces in the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-champion.png.

#### `armor-champion-f` · The Champion's tabard, on the woman's base · P1

**Basis:** Edits `armor-parts-woman`. As `armor-champion`, fitted to the woman's base.

Dress the pieces in the traveller's clothes (a cream shirt with rolled sleeves, brown trousers, brown boots, fingerless gloves, the driftwood herring on its cord) with, over them, a knee-length tabard in deep navy blue, open at the sides and belted at the waist, a gold sun embroidered on the chest, and a border of sun-gold trim worked in a pattern of two linked rings all round its edges. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-champion-f.png.

#### `armor-lord-protector` · The Lord Protector's coat · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.2 (long, high-collared, deep blue with violet lining) and §6.4 (the outer key; the shard's light in the pouch).

Dress the pieces in a long coat to mid-calf in deep navy blue, close at the chest and flaring at the hem, with a stiff standing collar, a double row of small gold buttons and narrow violet-and-gold edging, a rich violet lining showing at the front opening and the turned-back cuffs; a cream shirt and dark cravat under it; charcoal trousers and polished black knee boots; a dark belt with a heavy gold key with a sun-shaped bow on the near hip; the driftwood herring on its cord at the neck; and a faint violet light leaking from the belt pouch. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-lord-protector.png.

#### `armor-sovereign` · The Sovereign's armour · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.2 (dark plate with violet-gold edging, the coat's collar kept).

Dress the pieces in dark, close-fitting plate armour of blackened steel with violet-and-gold edging on every plate, over a long navy coat whose high, stiff standing collar is turned up round the jaw and whose violet lining shows at the split of the skirts; armoured boots; no herring at the neck. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-sovereign.png.

#### `armor-child-cove` · A cove child's clothes · P0

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (the children on the pier: fishing-town kids, cove blue).

Dress the pieces in a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-child-cove.png.

#### `armor-child-cove-f` · A cove child's clothes, on the woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-child-cove`, fitted to the woman's base, for `sb-child`, whose face is painted on it.

Dress the pieces in a child's poor clothes from a fishing cove, cut to fit (the game's morph makes the wearer child-sized): a cove-blue (a soft, sea-grey blue) knitted jumper, darned at both elbows and a little too big; a cream shirt collar showing; brown canvas trousers rolled to the knee and patched; bare feet. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-child-cove-f.png.

### 5.2 The party

#### `armor-sera` · Sera's ranger coat · P0

**Basis:** Edits `armor-parts-woman`. From the guide figure KIAN (a green ranger's coat); `03-art-overhaul.md` §7.4 (moss green and undyed leather).

Dress the pieces in a moss-green ranger's coat to mid-thigh, fitted and belted, split at the sides, with a stand collar and thin cream piping; an oatmeal shirt under it; a wide belt of undyed leather with a hunting knife, a pouch and a quiver of grey-fletched arrows at the far hip; olive trousers; tall undyed-leather boots laced to the knee; a leather bracer on the far forearm. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-sera.png.

#### `armor-mira` · Mira's archivist's clothes · P0

**Basis:** Edits `armor-parts-woman`. From the guide figures NOREL and CATE; `03-art-overhaul.md` §7.4 (a satchel with three scroll tubes; ink blue and parchment).

Dress the pieces in a cream blouse with full sleeves pushed up to the elbow; a fitted ink-blue waistcoat-bodice, laced at the front, with dark-blue piping; a long parchment-coloured skirt to the ankle, the hem worn; sensible brown lace-up boots; a brown leather belt with a small pouch and an ink bottle; and a battered brown leather satchel worn across the body, stuffed with papers, with three leather scroll tubes strapped to its side. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-mira.png.

#### `armor-mira-apprentice` · An archive apprentice's smock · P1

**Basis:** Edits `armor-parts-woman`. **(derived)** From A1-C8-01 (`echo mira-young`).

Dress the pieces in an oversized parchment-coloured apprentice's smock to the knee with ink-blue cuffs, belted with a cord; dark stockings; plain shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-mira-apprentice.png.

#### `armor-kael` · Kael's hooded leathers · P0

**Basis:** Edits `armor-parts-man`. From the guide figure DRAINOR (a hooded dark cloak over layered leathers); `03-art-overhaul.md` §7.4 (charcoal, an ember-red sash).

Dress the pieces in a close-fitting dark leather coat to the hip in layered panels, with straps and buckles across the chest; a charcoal half-cloak over the shoulders with its pointed hood hanging down the back; a wide ember-red sash wound round the waist, its ends hanging at the far hip; dark grey trousers; soft dark boots with the legs wrapped in cloth bands to the knee; dark fingerless gloves; a few lockpicks in a strap on the forearm. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-kael.png.

#### `armor-kael-shadow` · Kael, the Protectorate Shadow · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (`kael-shadow`: Protectorate colours).

Dress the pieces in finer black leathers with violet piping, layered and buckled; a long black coat with a high collar; a violet sash with a gold crown badge pinned to it; a black leather half-mask pulled down round the neck; black gloves; polished black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-kael-shadow.png.

#### `armor-brannock` · Brannock's soldier-smith kit · P0

**Basis:** Edits `armor-parts-man`. From the guide figure CERRO (a work shirt and an apron); `03-art-overhaul.md` §7.4 (iron grey, tarnished gold, plate); `Origins.cs` (chain).

Dress the pieces in a charcoal padded work shirt with the sleeves pushed up; a chain-mail shirt over it to the hips; a battered iron breastplate with tarnished-gold edging, dented and scratched; one heavy iron pauldron on the near shoulder; a thick dark-leather apron-like skirt to the knees over dark trousers, like a smith's; a wide belt with a big iron buckle and a battered metal flask; heavy iron-capped boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-brannock.png.

#### `armor-valencia` · Valencia's gown · P2

**Basis:** Edits `armor-parts-woman`. From the guide figure VALENCIA (a pale dress); `03-art-overhaul.md` §7.4 (bone white and wine red; a high stiff collar and long gloves).

Dress the pieces in a fitted bone-white gown, long-sleeved, with a very high, stiff standing collar that frames the jaw; a wine-red underskirt showing at the front split of the skirt; thin wine-red trim along the seams; long dark-wine gloves to above the elbow; neat black heeled boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-valencia.png.

#### `armor-abbey-habit` · The abbey habit · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (Sister Vale's habit); §7.5 (the `abbey` pool).

Dress the pieces in a plain, ankle-length habit of soft grey wool with a white linen collar; a plain rope belt with a small carved wooden bee on a cord; a white linen apron, a little stained with honey; plain dark shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-abbey-habit.png.

### 5.3 Act 1 and the towns

#### `armor-corra` · Corra's net-mender's clothes · P0

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (a net-mender's shawl, a ball of twine at the belt; cove blue and oiled brown).

Dress the pieces in a thick, cove-blue (a soft, sea-grey blue) knitted shawl round the shoulders, knotted at the front; an oatmeal blouse with the sleeves rolled; a faded blue kerchief at the throat; a long apron of oiled brown canvas over a long brown wool skirt; plain worn boots; a ball of tarred twine and a wooden netting needle hanging from the belt on a cord. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-corra.png.

#### `armor-sailor` · A sailor's clothes · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the Saltwind pool (oilskins, caps, rolled sleeves).

Dress the pieces in a navy-and-cream striped knitted jersey with the sleeves pushed up; a sleeveless brown oilskin waistcoat; canvas trousers rolled to the shin; bare feet with tarred soles; a rope lanyard at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-sailor.png.

#### `armor-odelle` · Captain Odelle's sea-coat · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (a white shirt, a crimson waistcoat, a charcoal cloak); the guide figure LEVL; `10-classes.md` (Deckhand: a pistol).

Dress the pieces in a white linen shirt, open at the neck, the sleeves loose; a fitted crimson waistcoat with brass buttons; a long charcoal sea-coat worn open, its skirts to the knee and its cuffs turned back; charcoal breeches; tall black sea boots, salt-stained; a wide leather belt holding a short one-shot flintlock pistol. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-odelle.png.

#### `armor-fisher` · A fisherman's oilskins · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the Saltwind pool; Old Tomas.

Dress the pieces in a thick grey knitted jumper, darned at the elbows; a sleeveless mustard oilskin smock over it, cracked and patched; canvas trousers rolled at the shin; tall black tarred sea boots; a brass fish hook on a cord at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-fisher.png.

#### `armor-red-gull` · A Red Gull's rig · P0

**Basis:** Edits `armor-parts-man`. From the guide figure KELL (a striped shirt, a shoulder guard); the art plan (a gull-red bandana and sash).

Dress the pieces in a raspberry-red bandana knotted round the throat; a cream-and-navy striped jersey with the sleeves rolled over the forearms; a scuffed leather shoulder guard on the near shoulder, buckled with straps across the chest; a wide raspberry-red sash at the waist; brown canvas trousers; tall worn boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-red-gull.png.

#### `armor-grell` · Bosun Grell's coat and hook · P0

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a hook for a hand; Red Gull red); the art plan (an open bosun's coat, a gull tattoo).

Dress the pieces in an open, knee-length bosun's coat in raspberry red, frayed at the cuffs and stained with tar, with tarnished brass buttons; an oatmeal shirt under it, half unlaced; a gull tattoo on the near forearm; a broad belt with a big brass buckle; dark canvas trousers; heavy boots. The far hand is gone at the wrist: in its place, a steel hook strapped to a leather cuff. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-grell.png.

#### `armor-watch` · The City Watch uniform · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (the Watch's blue); `03-art-overhaul.md` §7.3 (Watch navy and brass).

Dress the pieces in a navy-blue Watch coat to mid-thigh over a padded jack, brass buttons, a small brass badge shaped like a lantern on the chest; a brown leather belt; navy trousers; black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-watch.png.

#### `armor-watch-captain` · The Watch captain's coat · P0

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Hale: Watch navy and brass).

Dress the pieces in a navy-blue Watch coat to mid-thigh over a chain-mail shirt, with brass buttons and a larger brass lantern badge on the chest; a brown leather belt and baldric with a short sword; a rolled paper roster tucked into the belt; navy trousers; worn black boots, well polished. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-watch-captain.png.

#### `armor-clerk` · A clerk's waistcoat · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From Fennick, the Guild's and the docks' clerks.

Dress the pieces in an oatmeal shirt with black sleeve garters; a mustard-ochre waistcoat with a watch chain; sand-coloured trousers; polished brown shoes; ink stains on the fingers. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-clerk.png.

#### `armor-clerk-f` · A clerk's waistcoat, on the woman's base · P1

**Basis:** Edits `armor-parts-woman`. As `armor-clerk`, fitted to the woman's base, for the town's women in the crowd (Caleb painted it on the woman's template, 2026-11-13).

Dress the pieces in an oatmeal shirt with black sleeve garters; a mustard-ochre waistcoat with a watch chain; sand-coloured trousers; polished brown shoes; ink stains on the fingers. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-clerk-f.png.

#### `armor-refugee-woman` · A refugee woman's clothes · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (Hedda: "sacking, charcoal and oat"); the `doomward` pool.

Dress the pieces in an oatmeal shirt patched with sacking; a charcoal wool waistcoat; a long, mud-hemmed chestnut-brown skirt; boots split at the toes and bound with twine. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-refugee-woman.png.

#### `armor-refugee-man` · A refugee man's clothes · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (the refugees); the `doomward` pool (grey and black, patched, practical).

Dress the pieces in a faded black wool coat with mismatched patches over a grey shirt; a charcoal scarf; grey trousers tied at the ankle with twine; cracked boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-refugee-man.png.

#### `armor-refugee-man-f` · A refugee's coat, on the woman's base · P1

**Basis:** Edits `armor-parts-woman`. As `armor-refugee-man`, fitted to the woman's base, for the town's women in the crowd (Caleb painted it on the woman's template, 2026-11-13).

Dress the pieces in a faded black wool coat with mismatched patches over a grey shirt; a charcoal scarf; grey trousers tied at the ankle with twine; cracked boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-refugee-man-f.png.

#### `armor-refugee-child` · A refugee child's smock · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From Lise.

Dress the pieces in an oversized oatmeal smock belted with a length of rope; a small charcoal shawl round the shoulders; brown woollen stockings, one sagging; boots far too big. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-refugee-child.png.

#### `armor-glassblower` · A glassblower's apron · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (Yselle: blush, teal, sand; burned hands).

Dress the pieces in a blush-pink linen shirt with the sleeves rolled up; a teal waistcoat; a long sand-coloured skirt; a thick leather glassblower's apron, scorched and pitted; sturdy boots; both hands wrapped in clean linen bandages to the wrist. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-glassblower.png.

#### `armor-elara` · Elara's riding coat · P0

**Basis:** Edits `armor-parts-woman`. From the guide figure ELARA (the long fitted coat), recoloured to royal blue because violet is reserved for {sb}; `03-art-overhaul.md` §7.4 (a short cape on one shoulder; royal blue and sun gold).

Dress the pieces in a fitted riding coat to the knee in deep royal blue, with a high collar, a double row of small gold buttons and sun-gold trim at the hem, cuffs and collar, split at the back; a short royal-blue cape over the near shoulder only, fastened with a gold sun-shaped clasp; a white shirt; dark navy riding breeches; tall black riding boots; brown leather riding gloves; a slim sword at the hip. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-elara.png.

#### `armor-elara-disguise` · Elara in disguise · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (`elara-hooded`: a charcoal vest); A1-C4-01.

Dress the pieces in plain clothes with no gold anywhere: a plain charcoal waistcoat over a white shirt; dark breeches; scuffed riding boots; a plain sword at the hip. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-elara-disguise.png.

#### `armor-leon` · King Leon's robes · P0

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a broad ermine collar; white and sun gold) and §6.4 (`leon-sword`).

Dress the pieces in a long white robe-coat to the ankles with sun-gold trim at the hem and cuffs and a gold sash; a broad collar of ermine (white fur with small black tips) over the shoulders; soft white boots; and at the hip, a long, plain, very old sword in a worn scabbard, its grip blue leather worn pale, a small gold sun on the crossguard. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-leon.png.

#### `armor-crane` · Crane's long coat · P0

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a very long coat, rings that catch the light; plum and silver) and §6.4 (`cube-seal-ring`).

Dress the pieces in a floor-length plum-coloured coat, close at the chest and long in the skirt, with a high standing collar, silver trim and a column of small silver buttons; a dark-grey waistcoat; a white stock at the throat; dark trousers and narrow black boots; silver rings on several fingers, one a signet set with a small black cube. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-crane.png.

#### `armor-crane-f` · A long plum coat, on the woman's base · P1

**Basis:** Edits `armor-parts-woman`. As `armor-crane`, fitted to the woman's base, a lady's long coat, for whoever needs one (Caleb painted it on the woman's template, 2026-11-13).

Dress the pieces in a floor-length plum-coloured coat, close at the chest and long in the skirt, with a high standing collar, silver trim and a column of small silver buttons; a dark-grey waistcoat; a white stock at the throat; dark trousers and narrow black boots; silver rings on several fingers, one a signet set with a small black cube. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-crane-f.png.

#### `armor-royal-livery` · The palace livery · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (the palace's navy and gold).

Dress the pieces in a navy-blue livery tunic with a gold sun badge on the chest and gold piping; a white shirt; navy breeches; soft brown boots; a dispatch pouch on a strap across the body. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-royal-livery.png.

#### `armor-royal-guard` · The Royal Guard's armour · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (the palace guards in navy and gold).

Dress the pieces in a navy-blue tabard with a large gold sun embroidered on the chest, over polished mail; steel pauldrons with gold edging; a white sash; navy trousers; steel-capped black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-royal-guard.png.

#### `armor-tavern` · A tavern apron · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (Hessa: a charcoal vest, an apron); the inns' staff.

Dress the pieces in a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-tavern.png.

#### `armor-tavern-m` · A tavern apron, on the man's base · P0

**Basis:** Edits `armor-parts-man`. As `armor-tavern`, fitted to the man's base, for `pip`, whose face is painted on it.

Dress the pieces in a cream shirt with the sleeves rolled up; a charcoal waistcoat; a long, stained leather apron; dark trousers; heavy boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-tavern-m.png.

#### `armor-villager-woman` · A townswoman's clothes · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the `questeria` pool (bright, layered cloth; aprons); the townswomen.

Dress the pieces in a cream blouse with full sleeves; a teal bodice-waistcoat laced at the front; a long mustard skirt with a dusty-pink underskirt showing at the hem; a white apron; brown shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-villager-woman.png.

#### `armor-villager-man` · A townsman's clothes · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `questeria` pool; the farmhands and youths.

Dress the pieces in a loose oatmeal shirt with the sleeves rolled and a rust-orange neckerchief; a grey-blue waistcoat, unbuttoned; brown trousers held up with braces; muddy boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-villager-man.png.

#### `armor-elder-woman` · An old woman's dress and shawl · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From Hester, Maud and the old women.

Dress the pieces in a plain grey-brown wool dress to the ankle; a soft, muted mauve-grey knitted shawl crossed over both shoulders and tied at the back; a clean linen apron; soft, worn shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-elder-woman.png.

#### `armor-merchant` · A merchant's waistcoat · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (Osric: white, plum).

Dress the pieces in a white shirt with full sleeves; an embroidered dusty-plum waistcoat; dark burgundy-brown trousers; polished shoes; a tailor's tape round the neck. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-merchant.png.

#### `armor-busker` · A busker's finery · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (Lark: blush, rose, sand).

Dress the pieces in a blush-pink shirt with billowing sleeves; a rose waistcoat; sand-coloured trousers; soft boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-busker.png.

#### `armor-temple` · A temple sister's tabard · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (Hesper: white, ochre, a long skirt); (derived) the sun badge.

Dress the pieces in a white blouse; an ochre tabard-tunic with a small gold sun embroidered on the chest; a long sand-coloured skirt; an ochre shawl; plain sandals. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-temple.png.

#### `armor-urchin` · A street child's hand-me-downs · P0

**Basis:** Edits `armor-parts-man`. From `Cast.cs` (Wren: oat, charcoal, a cloak); the lost boy; the city's children.

Dress the pieces in a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-urchin.png.

#### `armor-urchin-f` · A street child's hand-me-downs, on the woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-urchin`, fitted to the woman's base, for `wren`, whose face is painted on it.

Dress the pieces in a child's hand-me-downs (the game's morph makes the wearer small): an oatmeal shirt; a charcoal waistcoat far too big; bark-brown trousers rolled at the ankle; bare, dirty feet. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-urchin-f.png.

#### `armor-fencer` · A fencing-master's doublet · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From Imra Vell, `10-classes.md` §5.1.

Dress the pieces in a fitted, quilted fencing doublet in cream with navy panels and brass buckles down one side; leather bracers on both forearms; a wide belt hung with eight small enamel tokens, each a different colour; navy breeches; soft leather boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-fencer.png.

#### `armor-lodge-ranger` · The Lodge ranger's kit · P0

**Basis:** Edits `armor-parts-woman`. From `Cast.cs` (Tamsin: oat, forest, olive); `03-art-overhaul.md` §7.3 (moss green for the Lodge).

Dress the pieces in an oatmeal shirt; a forest-green leather jerkin; an olive ranger's coat to mid-thigh with a moss-green collar; olive trousers; tall laced boots; a thick leather falconer's glove on the far hand; a short sword at the hip. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-lodge-ranger.png.

#### `armor-noble` · A courtier's doublet · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the court in `Content/Zones/sunspire-keep.txt` ("Velvet in this heat").

Dress the pieces in a fitted velvet doublet in deep teal with slashed sleeves showing cream silk; a lace collar; dark hose and breeches; soft black shoes with buckles; a ring or two. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-noble.png.

#### `armor-noble-gown` · A lady of the court's gown · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the court in `Content/Zones/sunspire-keep.txt`.

Dress the pieces in a fitted gown in dusty rose silk with a square neckline, long sleeves slashed with cream, a fine embroidered bodice, and a full skirt to the floor; a pearl necklace. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-noble-gown.png.

#### `armor-elder-coat` · An old man's long coat · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `elder` and `old-man` sheets.

Dress the pieces in a long dove-grey coat to the shin with dark-grey trim and pewter buttons; a slate waistcoat; a white shirt with a soft collar; grey trousers; black shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-elder-coat.png.

#### `armor-trader` · A trader's waistcoat · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the `trader` sheet.

Dress the pieces in a cream shirt with rolled sleeves; a tan suede waistcoat with many pockets; brown trousers; tall brown boots; a leather satchel across the body; a small set of brass scales at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-trader.png.

#### `armor-baker` · A baker's dress and apron · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the `baker` sheet.

Dress the pieces in a long-sleeved dress in deep brick red with the sleeves pushed up; a big white apron dusted with flour; sturdy brown shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-baker.png.

#### `armor-smith` · A smith's apron · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `smith` sheet; masons.

Dress the pieces in a cream shirt with the sleeves rolled above the elbow, open at the throat; a heavy, scorched brown leather apron to the knee; thick leather gloves tucked into the apron string; dark trousers; heavy boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-smith.png.

#### `armor-road` · A traveller's road clothes · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `traveller` sheet.

Dress the pieces in a dark-brown tunic belted at the waist; charcoal trousers; boots worn through at the toes and wrapped in cloth; a small pack on the back with a bedroll and a tin cup. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-road.png.

#### `armor-courier` · A courier's clothes · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `courier` sheet.

Dress the pieces in a cream shirt with rolled sleeves; dark-grey braces; charcoal trousers tucked into soft running boots; a big leather mail satchel bulging with letters on a strap across the body. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-courier.png.

#### `armor-pilgrim` · A pilgrim's robe · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the `pilgrim` sheet; `enc-pilgrims` (a sun token).

Dress the pieces in a long, dusty, dark plum-brown travelling robe to the ankle with its hood down, belted with a rope; a small round brass sun token on a cord at the chest; sandals over wrapped feet; a cloth bag of bread at the hip. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-pilgrim.png.

#### `armor-porter` · A porter's clothes · P0

**Basis:** Edits `armor-parts-man`. **(derived)** From the `porter` sheet.

Dress the pieces in a sweat-stained cream shirt with rolled sleeves; brown braces; a leather shoulder pad on the far shoulder; brown canvas trousers; heavy boots; a crate hook at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-porter.png.

#### `armor-maid` · A maid's dress · P0

**Basis:** Edits `armor-parts-woman`. **(derived)** From the `maid` sheet.

Dress the pieces in a dusty-blue dress with short puffed sleeves; a white apron with a frill; black shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-maid.png.

#### `armor-caravan` · A caravan master's duster · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From Garrick, A1-C5-01.

Dress the pieces in a long oiled-brown duster coat to the shin, open; a faded grey scarf against the road dust; a leather waistcoat over a grey shirt; dark trousers; long riding boots; a coiled driving whip at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-caravan.png.

#### `armor-aldric` · Aldric's worn plate · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (worn plate; steel and faded red).

Dress the pieces in worn steel plate that doesn't quite match, dented and scratched (a breastplate, rounded pauldrons, vambraces and greaves), over chain mail and a padded jack; a battered sword belt; old boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-aldric.png.

#### `armor-recruit` · A recruit's gambeson · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From A1-C7-01.

Dress the pieces in a quilted navy-blue gambeson with brass buttons, a little too long; a brown belt; grey wool trousers; boots with snow on them; a grey woollen scarf. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-recruit.png.

#### `armor-thessaly` · The Archmage's robes · P1

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (Guild cobalt; two fingers missing on the near hand).

Dress the pieces in an A-line robe to the ankle in deep cobalt blue, with wide sleeves and two bands of trim at the hem and cuffs carrying a plain, repeated geometric motif in pale silver thread; a short cobalt mantle over the shoulders with a high collar; a dark sash with a few small brass keys and a slim book on a chain; plain dark boots. On the near hand, the ring finger and the little finger are missing, healed long ago. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-thessaly.png.

#### `armor-brenna` · Brenna's ash-walker kit · P1

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (one cracked knight's pauldron; ash grey and black).

Dress the pieces in a black-dyed leather jerkin over a mail shirt; one old, cracked knight's pauldron on the near shoulder, finer than the rest of the kit, with a worn engraved crest; black trousers; boots wrapped in grey cloth to the knee. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-brenna.png.

#### `armor-gannet` · Mother Gannet's shawl and dress · P1

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (a heavy shawl; mustard and brown).

Dress the pieces in a heavy mustard-yellow wool shawl round the shoulders, pinned with a large brass brooch; a brown wool dress to the floor; a ring of iron keys and a fat coin purse at the belt; rings on several fingers. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-gannet.png.

#### `armor-cider-presser` · A cider-presser's apron · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Tobin: a cider-presser's apron, a burn on the near hand; apple green and brown).

Dress the pieces in an apple-green work shirt with the sleeves rolled; a heavy, stained brown leather presser's apron; brown trousers; sturdy boots; a wide, old burn scar across the back of the near hand. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-cider-presser.png.

#### `armor-levy` · A levy coat · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Tobin's levy coat); `07-companions.md` (`mira-tobin`).

Dress the pieces in a patched grey levy coat over a work shirt, mud to the knees, rope for a belt, worn boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-levy.png.

#### `armor-harrow` · Lord Harrow's heron coat · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a green coat with a grey heron on the breast; heron green and grey).

Dress the pieces in a long frock coat in heron green (a deep grey-green) with a grey heron embroidered on the breast; a grey silk waistcoat; a white cravat; grey breeches; white stockings and black buckled shoes; grey kid gloves. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-harrow.png.

#### `armor-woodcutter` · A woodcutter's clothes · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From Hobb and the cutters.

Dress the pieces in a faded brown-and-ochre checked flannel shirt with the sleeves rolled; a leather waistcoat; canvas trousers held up with braces; heavy hobnailed boots; a coil of rope at the belt; sawdust everywhere. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-woodcutter.png.

#### `armor-maleficio` · Maleficio's high-backed robes · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (scholar's robes with a stiff high back; charcoal with ember edges).

Dress the pieces in long charcoal scholar's robes to the floor, layered, with a tall, stiff, fan-shaped back to the collar that rises behind the head; the hems, cuffs and the edge of the high collar glow faintly ember orange, like paper just starting to burn; a dark sash with an archivist's keys; soft dark shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-maleficio.png.

#### `armor-scholar` · A scholar's robes · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From `maleficio-young`; the archive's and the Guild's scholars.

Dress the pieces in plain brown-and-charcoal scholar's robes to the ankle with the sleeves pushed up, a cord belt with a pen case, soft shoes, ink on the fingers. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-scholar.png.

#### `armor-old-pattern` · Old-pattern mail and surcoat · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From `enc-relief-ghost` ("old-pattern armour"); the Unmade's knight.

Dress the pieces in an old-fashioned long mail hauberk to the knees with a padded coif down round the neck; a navy surcoat over it with a faded gold sun badge in an older, simpler style; a plain belt; old riveted boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-old-pattern.png.

#### `armor-pedlar` · A pedlar's patched coat · P1

**Basis:** Edits `armor-parts-woman`. **(derived)** From `enc-mags`.

Dress the pieces in a patched mustard-and-brown skirt over trousers; a man's old green coat with deep pockets; a striped scarf; stout boots; a smooth grey stone with a hole in it hanging on a cord at the neck. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-pedlar.png.

#### `armor-varkhul` · Varkhul's Hollow plate · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Hollowed plate; ash, ember); A1-C6 (a knight once).

Dress the pieces in full plate of blackened iron, ash grey at the edges, old and fine under the corruption, with a faded engraved crest on the breastplate; its seams are cracked and leak a dull ember-orange glow from inside; a ragged ash-grey surcoat hanging in strips; armoured gauntlets and sabatons. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-varkhul.png.

### 5.4 Act 2

#### `armor-anselle` · Mother Anselle's habit · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (an abbey habit with no hood; a heavy key ring).

Dress the pieces in a plain ankle-length habit of soft grey wool with a white linen collar and white cuffs; a wide leather belt with a heavy iron ring of many keys at the hip and a short iron mace; a small carved wooden bee on a cord round the neck; sturdy black shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-anselle.png.

#### `armor-beekeeper` · A beekeeper's smock · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (Orla: honey yellow and brown).

Dress the pieces in a brown wool habit with the sleeves rolled to the elbow; a honey-yellow canvas smock-apron over it, sticky at the front; thick leather gloves tucked in the belt; a hive tool on a cord; heavy clogs. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-beekeeper.png.

#### `armor-corwin` · Sir Corwin's gilded plate · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (gilded plate; a sun tabard; gold-white).

Dress the pieces in full plate armour in polished steel with gold-leaf edging on every plate and a small sun engraved on each pauldron; a long white tabard over it with a large gold sun on the chest and gold trim; gilded gauntlets; armoured boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-corwin.png.

#### `armor-severin` · Severin's velvet coat · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a long black velvet coat; black and wine red).

Dress the pieces in a long, close-fitting coat of black velvet to the ankles, with a high collar lined in wine red and tarnished silver clasps; a wine-red waistcoat; a black cravat with a dark-red stone pin; black gloves; long black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-severin.png.

#### `armor-isadora` · Lady Isadora's gown · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.4 (a long gown with a train; midnight blue).

Dress the pieces in a long gown in deep midnight blue with fitted sleeves, a modest square neckline, a long train behind, and fine silver embroidery of small night-flowers at the hem; a pair of gardening gloves tucked into the belt; a small pair of silver shears on a chain. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-isadora.png.

#### `armor-child-dress` · A little girl's dress · P2

**Basis:** Edits `armor-parts-woman`. **(derived)** From `echo valencia-child`.

Dress the pieces in a plain dark-blue dress to the shin with a white collar, cut for a child (the game's morph makes her small); dark stockings; little black shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-child-dress.png.

#### `armor-morvaine-man` · A valley man's wool coat · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.5 (the `morvaine` pool: dark wool, high collars).

Dress the pieces in a high-collared coat of dark grey wool, too thin for the weather, buttoned to the chin; a black knitted scarf; patched charcoal trousers; worn boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-morvaine-man.png.

#### `armor-morvaine-woman` · A valley woman's wool dress · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.5 (the `morvaine` pool).

Dress the pieces in a high-collared dress of dark grey wool to the ankle; a black wool shawl pinned with a pewter brooch; a dark apron; thick dark stockings; black boots; a small pewter cup on a chain at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-morvaine-woman.png.

#### `armor-shift` · A linen shift and shawl · P2

**Basis:** Edits `armor-parts-woman`. **(derived)** From Ide, `08-random-encounters.md` §5.1.

Dress the pieces in a plain, greyish linen shift to the shin; a dark wool shawl round the shoulders; a linen bandage round the inside of the far elbow; bare feet. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-shift.png.

#### `armor-morvaine-livery` · A vampire household's livery · P2

**Basis:** Edits `armor-parts-man`. **(derived)** From the blood-house steward.

Dress the pieces in a black livery coat with a high collar and wine-red piping; a black waistcoat; a starched white stock; white gloves; black breeches and stockings; polished black shoes; a ring of keys at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-morvaine-livery.png.

#### `armor-abbey-lay` · A lay sister's habit · P2

**Basis:** Edits `armor-parts-woman`. From `03-art-overhaul.md` §7.5 (the `abbey` pool: habits, aprons).

Dress the pieces in a plain ankle-length habit of undyed oatmeal wool; a brown work apron with a pocket of seed packets; a rope belt with a small wooden bee charm; clogs. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-abbey-lay.png.

### 5.5 Act 3

#### `armor-prisoner` · A prisoner's shirt · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (`severin-cell`: a plain shirt).

Dress the pieces in a plain, stained linen shirt and dark trousers, bare feet, the wrists in iron cuffs with a short chain between them. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-prisoner.png.

#### `armor-davin` · Davin's cut-down tabard · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Corwin's tabard cut down to fit).

Dress the pieces in a mail shirt a little big for the wearer; over it a white tabard with a large gold sun, plainly cut down from a bigger man's and re-hemmed by hand with uneven stitches; a brown belt; leather gloves; plain steel greaves over brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-davin.png.

#### `armor-vorlaith` · Vorlaith's glass body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (a body of cracked glass with light moving inside).

Turn the pieces into a body made entirely of clear, colourless glass, cracked all over with fine branching fractures, a cold white light drifting slowly inside like smoke in a bottle, brightest in the chest; the glass forms long, still robes that fall to the ground and a high collar; long glass fingers, relaxed. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-vorlaith.png.

#### `armor-guild-apprentice` · A Guild apprentice's robe · P2

**Basis:** Edits `armor-parts-woman`. **(derived)** From A3-C2-01; Guild cobalt.

Dress the pieces in a short cobalt-blue apprentice's robe to the knee with plain trim, belted with a cord; grey trousers; soft shoes; a satchel. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-guild-apprentice.png.

#### `armor-elara-field` · Elara, leading the resistance · P2

**Basis:** Edits `armor-elara`. From `03-art-overhaul.md` §7.4 and §6.5 (Leon's sword on her back from A3-C2-05).

Edit the attached picture (armor-elara.png, that outfit painted in pieces on the armor template): change ONLY her clothes after months of war in a forest: the royal-blue riding coat is stained with mud and torn at the hem, the gold trim dulled, the slim sword gone from the hip; and her father's long, plain, very old sword is strapped across her back, its blue leather grip and the small gold sun on its crossguard showing over the near shoulder. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-elara-field.png.

#### `armor-elara-queen` · Elara, crowned · P2

**Basis:** Edits `armor-elara`. From `03-art-overhaul.md` §7.4 (crowned, A3-C5-06).

Edit the attached picture (armor-elara.png, that outfit painted in pieces on the armor template): change ONLY the cape: replace the short one-shoulder cape with a short white-and-gold mantle over both shoulders, and put her father's old sword (blue leather grip, a small gold sun on the crossguard) at her hip in place of the slim one. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-elara-queen.png.

#### `armor-leon-furs` · King Leon, in furs · P2

**Basis:** Edits `armor-leon`. From `03-art-overhaul.md` §7.4 (Leon, "ill, in furs").

Edit the attached picture (armor-leon.png, that outfit painted in pieces on the armor template): change ONLY the robes: wrap him in a heavy fur-lined robe of navy and white with a thick fur collar and a woollen blanket round the shoulders, keeping the old sword at the hip. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-leon-furs.png.

#### `armor-brenna-protectorate` · Brenna, sworn to the Protector · P2

**Basis:** Edits `armor-brenna`. From `03-art-overhaul.md` §7.4 (an Act 3 Protectorate sash).

Edit the attached picture (armor-brenna.png, that outfit painted in pieces on the armor template): change ONLY one thing: add a violet sash bound across the body from the near shoulder to the far hip, fixed with a gold crown badge. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-brenna-protectorate.png.

### 5.6 Humanoid foes

#### `armor-hollowed-footman` · A Hollowed footman's body · P0

**Basis:** Edits `armor-parts-man`. From the art plan (ash skin, cracks lit ember, a dented cuirass, a rotted tabard).

Turn the pieces into the body of a Hollowed footman, a dead soldier raised as an ash-grey husk: arms, hands and legs of dry ash-grey skin like burnt paper, cracked all over in fine lines glowing ember orange from inside; a dented, rust-streaked iron cuirass over rotting padded cloth; a ragged tabard of rotted brown-red cloth hanging in strips; rusted mail sleeves; wrapped legs and broken boots. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-hollowed-footman.png.

#### `armor-hollowed-hexer` · A Hollowed hexer's body · P0

**Basis:** Edits `armor-parts-man`. From the art plan (a tattered robe).

Turn the pieces into the body of a Hollowed spellcaster: long, bony ash-grey hands with faint ember-lit cracks, and a tattered, ash-stained robe in charcoal and faded brown to the ground, hanging in strips at the hem and sleeves; a cord belt with a bone-handled dagger and strings of small charms. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-hollowed-hexer.png.

#### `armor-hollowed-sapper` · A Hollowed sapper's body · P0

**Basis:** Edits `armor-parts-man`. From the art plan (soot, a jerkin, charges, a lantern at the back of the belt).

Turn the pieces into the body of a Hollowed tunneller: ash-grey, ember-cracked skin black with soot; a filthy leather jerkin over a torn shirt; a heavy belt with a pouch of round clay blasting charges with short fuses and a small iron lantern glowing orange at the back; wrapped legs and heavy broken boots; a coil of rope over one shoulder. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-hollowed-sapper.png.

#### `armor-hollowed-chief` · The sapper chief's body · P0

**Basis:** Edits `armor-parts-man`. From the art plan (the sapper plus a bandolier of charges).

Turn the pieces into the body of the Hollowed sappers' leader: ash-grey, ember-cracked skin; a heavy, scorched leather jerkin reinforced with iron plates; a bandolier of round clay blasting charges across the chest; a wide belt with tools; thick gauntlets; heavy boots. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-hollowed-chief.png.

#### `armor-construct-stone` · A vault construct's stone body · P0

**Basis:** Edits `armor-parts-man`. From the art plan (dressed stone with gold inlay; joints that are dark gaps with gold light inside).

Turn the pieces into the body of an ancient guardian statue built from blocks of pale, dressed sandstone with thin inlaid lines of gold running along their edges in geometric patterns: broad, blocky shoulders, a narrow stone waist, heavy legs and big three-fingered stone hands; every joint (shoulders, elbows, wrists, hips, knees) a dark gap with a warm gold light glowing inside; a few cracks and chips, moss in one crack. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-construct-stone.png.

#### `armor-hollowed-climber` · A Hollowed climber's body · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From `01-build-plan.md` §9; `11-combat.md` (climbers).

Turn the pieces into the body of a wiry Hollowed wall-climber: lean, ash-grey, ember-cracked limbs; ragged, close-bound strips of dark cloth; a rope with an iron grapnel coiled over the shoulder; iron climbing hooks strapped to both forearms; bare, clawed feet. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-hollowed-climber.png.

#### `armor-cultist` · A cube cultist's robe · P1

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (a black robe, a cube pendant).

Dress the pieces in a plain black robe to the ankle with a high collar, belted with black cord; a pendant of a small black iron cube on a chain at the chest; a pouch of black iron coins stamped with a cube; black wrapped boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-cultist.png.

#### `armor-bandit` · A bandit's leathers · P1

**Basis:** Edits `armor-parts-man`. **(derived)** From `08-random-encounters.md` §3 (bandits).

Dress the pieces in a scuffed leather jerkin; a mismatched leather bracer; dark trousers; worn boots; a dirty brown scarf round the neck; a sack of stolen goods at the belt. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-bandit.png.

#### `armor-thrall` · A thrall's tabard · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.3 (wine red for Morvaine's thralls).

Dress the pieces in a dark mail shirt under a wine-red tabard with a black thorn emblem; a high black wool collar; black leather gloves; dark trousers; black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-thrall.png.

#### `armor-thrall-knight` · A thrall-knight's black plate · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.4 (Vesk, Vreya, Mordain: black plate with red trim).

Dress the pieces in full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-thrall-knight.png.

#### `armor-thrall-knight-f` · A thrall-knight's black plate, on the woman's base · P2

**Basis:** Edits `armor-parts-woman`. As `armor-thrall-knight`, fitted to the woman's base, for `vreya`, whose face is painted on it.

Dress the pieces in full plate of glossy black steel with thin wine-red trim on every edge, sharp but elegant, with layered black cloth showing between the plates and flared tassets over the hips; black gauntlets and pointed sabatons. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-thrall-knight-f.png.

#### `armor-gilded-knight` · A Gilded knight's armour · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.5 (the `order` pool: gilded plate and tabards).

Dress the pieces in steel plate with gold-painted edges (plainer than a commander's); a white tabard with a gold sun on the chest; armoured boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-gilded-knight.png.

#### `armor-drowned` · A drowned body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (grey-green, waterlogged clothes).

Turn the pieces into the body of a drowned corpse: bloated, sagging, grey-green waterlogged skin wrinkled at the fingers, barnacles crusted on one shoulder; the soaked, torn remains of a fisherman's jersey and canvas trousers, dark with water and tangled in fishing net; one bare foot, one boot; water running off it. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-drowned.png.

#### `armor-blood-wight` · A blood-wight's body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (thin, grey, a red glow in the chest).

Turn the pieces into the body of a terribly thin undead thing: all bones and taut ash-grey papery skin stretched over the ribs, with a deep red glow beating like a heart under the ribs; the rotted remains of a dark noble's coat hanging in strips; long fingers ending in black claws. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-blood-wight.png.

#### `armor-sun-scorched` · A sun-scorched body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (old priest's robes burned to the skin).

Turn the pieces into the body of one of the burned dead: skin blackened and cracked like dried mud, the cracks glowing a faint hot gold; the remains of an old priest's white-and-gold robes burned onto the body, cloth and skin fused, the hems charred to nothing; a melted gold sun pendant fused to the chest; wisps of heat haze rising from the shoulders. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-sun-scorched.png.

#### `armor-thornling` · A thornling's bark body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (bark-skinned, thorn fingers).

Turn the pieces into the body of a forest sprite made of living wood (the game's morph makes it child-sized): thin, knobbly limbs of rough dark bark with green moss in the cracks, long fingers ending in sharp black thorns, root-like feet, and a few tiny white flowers growing on one shoulder. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-thornling.png.

#### `armor-construct-priest` · A priest-construct's stone body · P2

**Basis:** Edits `armor-parts-man`. **(derived)** From `03b-world-art.md` (`priest-construct`).

Turn the pieces into the body of a stone guardian carved as a robed priest: sun-bleached sandstone robes falling in straight folds to the feet, gold inlay worn thin by sand, joints that are dark gaps with a hot white-gold light inside, cracks and sand-scour, one shoulder broken off and held together with old bronze bands. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-construct-priest.png.

#### `armor-smuggler` · A smuggler's oilskin · P2

**Basis:** Edits `armor-parts-woman`. **(derived)** From `08-random-encounters.md` §3 (smugglers).

Dress the pieces in a long, dark oilskin coat, salt-stained; a grey knitted jersey; a wide belt with hidden pouches; dark trousers; tall sea boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-smuggler.png.

#### `armor-protectorate` · The Protectorate's tabard · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §7.5 (the `protectorate` pool: violet-and-gold tabards over the Watch's kit).

Dress the pieces in the navy-blue Watch coat with brass buttons, and over it a violet tabard with a gold border and a gold stylised crown on the chest; a violet armband; a brown belt with a pouch of papers; navy trousers; black boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-protectorate.png.

#### `armor-protectorate-warden` · A Protectorate warden's plate · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (the warden in heavy plate).

Dress the pieces in heavy steel plate with a violet enamel finish on the breastplate and pauldrons and gold edging, a stylised gold crown embossed on the chest; a violet surcoat skirt; heavy gauntlets and sabatons. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-protectorate-warden.png.

#### `armor-construct-crown` · A crown-construct's metal body · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (violet-gold metal).

Turn the pieces into the body of a sleek war machine shaped like a knight: polished dark violet metal with bright gold inlay and gold edges, a narrow waist and broad shoulders, joints that are tight gold rings with violet light inside, and forearms that end in heavy gold-edged blades instead of hands. This is the whole body, skin and all, so paint every piece as it, the neck at the top of the torso too; it may be a little bulkier than the body under it, but keep the black gaps.
I'll save it as armor-construct-crown.png.

#### `armor-unmade-bronze` · Ancient bronze scale · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (the Unmade: four armour variants); A3-C5-07 ("the armour of every age").

Dress the pieces in ancient bronze scale armour, bronze greaves, a short pleated kilt, sandals; the colours faded and slightly greyed, as if not quite here. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-unmade-bronze.png.

#### `armor-pikeman` · A pikeman's breastplate · P2

**Basis:** Edits `armor-parts-man`. From `03-art-overhaul.md` §9 (the Unmade).

Dress the pieces in a steel breastplate and tassets over a padded doublet with puffed sleeves, breeches and tall boots; the colours faded and slightly greyed, as if not quite here. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-pikeman.png.

#### `armor-ashfall-raider` · An Ashfall raider's kit · P2

**Basis:** Edits `armor-parts-man`. **(derived)** From `enc-brenna-raiders`; the `doomward` pool.

Dress the pieces in patched black-and-grey leather armour over a mail shirt; a violet strip of cloth tied round the near arm; wrapped boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-ashfall-raider.png.

#### `armor-maleficio-ascendant` · Maleficio, the Sovereign · P1

**Basis:** Edits `armor-maleficio`. From `04-script-prologue-act1.md` A1-C8-02 (the Sovereign phase).

Edit the attached picture (armor-maleficio.png, that outfit painted in pieces on the armor template): change ONLY the robes' state: lift them and the high collar as if in a hot updraught, set every ember-orange edge burning bright with sparks streaming upward, and run fine ember-orange cracks of light across the hands. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-maleficio-ascendant.png.

#### `armor-varkhul-split` · Varkhul, the armour split open · P1

**Basis:** Edits `armor-varkhul`. From `04-script-prologue-act1.md` A1-C6-01 ("The armour splits along its seams").

Edit the attached picture (armor-varkhul.png, that outfit painted in pieces on the armor template): change ONLY the armour's state: it has split open along its seams and fallen away from the chest, the ember light gone out of it, showing a plain, stained padded jack underneath. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-varkhul-split.png.

#### `armor-crown-hollowed` · Crown-raised Hollowed body · P2

**Basis:** Edits `armor-hollowed-footman`. From `03-art-overhaul.md` §9 (Hollowed in Protectorate sashes, violet eye light).

Edit the attached picture (armor-hollowed-footman.png, that outfit painted in pieces on the armor template): change ONLY two things: the glowing cracks in the skin are violet instead of ember orange, and a violet sash with a gold crown badge is tied across the body over the old rotted tabard. Keep every piece on its own, exactly where it is, with the black gaps between them, and keep the line style.
I'll save it as armor-crown-hollowed.png.

### 5.7 Items

#### `armor-padded` · Padded armour · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`padded-armour`); the art plan's §1.8 (cloth: quilting diamonds; matte).

Dress the pieces in a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-padded.png.

#### `armor-leather` · Leather armour · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`leather-armour`); the art plan's §1.8 (stiff rounded panels, stitching, straps; one wide highlight).

Dress the pieces in boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-leather.png.

#### `armor-studded` · Studded leather · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`studded-leather`).

Dress the pieces in the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-studded.png.

#### `armor-hide` · Hide armour · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`hide-armour`); the art plan's §1.8 (bulky, ragged edges, a fur collar, X-lacing).

Dress the pieces in bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-hide.png.

#### `armor-chain-shirt` · Chain shirt · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`chain-shirt`); the art plan's §1.8 (sags, a scalloped hem).

Dress the pieces in a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-chain-shirt.png.

#### `armor-chain-mail` · Chain mail · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`chain-mail`).

Dress the pieces in a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-chain-mail.png.

#### `armor-scale` · Scale mail · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`scale-mail`); the art plan's §1.8 (scales in half-offset rows).

Dress the pieces in a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-scale.png.

#### `armor-plate` · Plate armour · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`plate-armour`); the art plan's §1.8 (rolled rims, rivets, lames; a reflection of the sky).

Dress the pieces in full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-plate.png.

#### `armor-mage-robes` · Mage robes · P0

**Basis:** Edits `armor-parts-man`. From `Items/ItemCatalog.cs` (`mage-robes`); the art plan's §1.8 (an A-line to the ankle, wide sleeves, trim bands).

Dress the pieces in a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-mage-robes.png.

#### `armor-padded-f` · Padded armour, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-padded`.

Dress the pieces in a quilted padded gambeson to mid-thigh, stitched in a diamond pattern, with a high collar and laced sides, in undyed oatmeal; quilted leg wraps over brown trousers; plain brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-padded-f.png.

#### `armor-leather-f` · Leather armour, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-leather`.

Dress the pieces in boiled-leather armour in stiff, rounded brown panels, stitched at the edges, strapped and buckled: a cuirass, shoulder guards, bracers and short tassets, over a dark shirt and leather leggings; brown boots; a few scuffs and one wide soft highlight on each panel. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-leather-f.png.

#### `armor-studded-f` · Studded leather, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-studded`.

Dress the pieces in the same kind of boiled-leather armour in stiff, rounded brown panels (a cuirass, shoulder guards, bracers, short tassets), studded all over with neat rows of small brass rivets; leather leggings; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-studded-f.png.

#### `armor-hide-f` · Hide armour, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-hide`.

Dress the pieces in bulky, layered animal hides in tan and brown with ragged edges, laced together with crossed thongs, a thick grey-brown fur collar over the shoulders, fur-trimmed leg wraps and hide boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-hide-f.png.

#### `armor-chain-shirt-f` · Chain shirt, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-chain-shirt`.

Dress the pieces in a short chain shirt to the hips with short sleeves, its rings drawn as offset rows, dark steel with small bright highlights on the lit side and a scalloped hem, worn over a padded jack whose collar and cuffs show; dark trousers; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-chain-shirt-f.png.

#### `armor-chain-mail-f` · Chain mail, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-chain-mail`.

Dress the pieces in a long chain-mail coat to the knee with long sleeves and a split skirt, belted at the waist, drawn as offset rows of small rings in dark steel with dense small highlights, worn over a padded jack; mail chausses; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-chain-mail-f.png.

#### `armor-scale-f` · Scale mail, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-scale`.

Dress the pieces in a coat of small overlapping steel scales to the knee, laid in half-offset rows like a fish's, each scale with a highlight on its top edge and a deep shadow under the row above, edged with brown leather at the collar, hems and armholes, belted at the waist; leather leggings; brown boots. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-scale-f.png.

#### `armor-plate-f` · Plate armour, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-plate`.

Dress the pieces in full plate armour of well-fitted steel: a breastplate, a gorget, wide pauldrons, articulated arms with three lames at the elbows, gauntlets, tassets, cuisses, poleyns and greaves, sabatons; rolled rims, small rivets, a padded arming jacket showing at the joints; the steel reflects a band of the sky (navy to soft pink to cream) across each plate, with one small bright glint and one small engraved border on the breastplate. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-plate-f.png.

#### `armor-mage-robes-f` · Mage robes, woman's base · P0

**Basis:** Edits `armor-parts-woman`. As `armor-mage-robes`.

Dress the pieces in a mage's robe to the ankle in deep teal, with wide, bell-shaped sleeves, layered hems, two trim bands at the hem and cuffs carrying a small repeated geometric motif dotted with gold thread, and a knotted sash; soft shoes. Any bare skin keeps the body's warm tan; the game tints it to the wearer's skin.
I'll save it as armor-mage-robes-f.png.

---

## 6. Capes, helms and weapons

The game's cloaks, helms and weapons (`Items/ItemCatalog.cs`) and the hats in `Characters/LookOptions.cs`, plus every character's signature pieces. Shields are drawn on the far forearm; a character's own sword-and-shield or pair of blades is one piece.

### 6.0 Sheets (four at a time)

The owner's call (2026-11-08): the capes, helms and weapons four at a time, like the hair and the armors. Every card in §6.1 to §6.3 that's painted on a base is in one sheet here (29 sheets for the 107 P0 parts, 8 for P1's 25, 13 for P2's 38), each one ChatGPT picture. The attached template (`tools/source/characters/<kind>-sheet-male.png` or `-female.png`, made by `python tools/part_sheet.py templates`) is four copies of the base: a helm sheet's are the blank base from the top of the canvas to the hips, at full size, with room for a crown; a cape sheet's and a weapon sheet's are the whole figure, a little smaller than an armor sheet's, with room behind the back for a cloak and over the head for a staff. `python tools/part_sheet.py check <png> <sheet-id>` says whether ChatGPT kept to the template, and `python tools/part_sheet.py fit <png> <sheet-id>` puts each cell back on its base as `tools/source/characters/helms/`, `capes/` or `weapons/<id>.png`. A sheet holds one priority, so the opening chapters' parts are done first; the cards that edit another part (`weapon-black-box`, `weapon-brannock-1`, `weapon-brannock-41`, `weapon-brannock-42`) stay single, as do any redos.

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
3. Battleaxe: In the near hand, a long-hafted bearded battleaxe with a heavy steel head, held low, its head forward.
4. Greatsword: A two-handed steel greatsword held low in the near hand alone, point forward.
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
2. Kael's rapier and parrying dagger: A slim rapier with a swept hilt in the near hand, held low, point forward, and a parrying dagger held reversed in the far hand.
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
3. A walking stick: In the near hand, a crooked wooden walking stick, held upright, its foot near the ground.
4. A tin cup: In the near hand, a dented tin cup of water.
I'll save it as weapon-sheet-m09.png.

#### `weapon-sheet-m10` · Weapon sheet: 4 weapons (A quill and a sealed letter, A sealed letter, A bundle of letters, A stew pot) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-quill-letter`, 2 `weapon-letter`, 3 `weapon-letters`, 4 `weapon-stew-pot`.

1. A quill and a sealed letter: A quill pen in the near hand, and a folded letter with a dark wax seal in the far hand.
2. A sealed letter: In the near hand, a rolled letter with a gold wax seal and a ribbon.
3. A bundle of letters: In the near hand, a bundle of sealed letters tied with string.
4. A stew pot: A steaming iron stew pot hanging by its handle from the near hand, a cloth wrapped round the handle.
I'll save it as weapon-sheet-m10.png.

#### `weapon-sheet-m11` · Weapon sheet: 4 weapons (A bolt of silk, A lute, A rusted longsword, A hexer's crystal staff) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-silk-bolt`, 2 `weapon-lute`, 3 `weapon-hollowed-sword`, 4 `weapon-hexer-staff`.

1. A bolt of silk: A bolt of rose-coloured silk hanging from the far hand.
2. A lute: A small round-backed lute held by its neck in the near hand, hanging.
3. A rusted longsword: In the near hand, a long, rusted, notched longsword held low, its point dragging.
4. A hexer's crystal staff: In the near hand, a crooked staff of blackened wood topped with a jagged cluster of black crystal wrapped in a soft violet glow.
I'll save it as weapon-sheet-m11.png.

#### `weapon-sheet-m12` · Weapon sheet: 4 weapons (A pick and a lit charge, A pick and a burning lantern, A wooden toy sword, A pitchfork) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-sapper-pick`, 2 `weapon-chief-pick-lantern`, 3 `weapon-toy-sword`, 4 `weapon-pitchfork`.

1. A pick and a lit charge: A heavy iron pick in the near hand, and a round clay blasting charge with its fuse sparking in the far hand.
2. A pick and a burning lantern: A big two-handed pick held in the near hand, and in the far hand a large iron lantern with its door open and fire roaring inside.
3. A wooden toy sword: In the near hand, a small wooden toy sword.
4. A pitchfork: In the near hand, a wooden pitchfork held upright.
I'll save it as weapon-sheet-m12.png.

#### `weapon-sheet-m13` · Weapon sheet: 2 weapons (A crate, carried, A forging hammer and tongs) · P0

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-crate`, 2 `weapon-hammer-tongs`.

1. A crate, carried: A wooden crate held by a rope handle in the far hand, hanging.
2. A forging hammer and tongs: A forging hammer in the near hand, and a pair of tongs in the far hand.
Leave figures 3 and 4 exactly as they are, with nothing added.
I'll save it as weapon-sheet-m13.png.

#### `weapon-sheet-m14` · Weapon sheet: 4 weapons (Thornwood bow, Sunsteel blade, A mallet, A spade) · P1

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-thornwood-bow`, 2 `weapon-sunsteel-blade`, 3 `weapon-mallet`, 4 `weapon-spade`.

1. Thornwood bow: In the near hand, a strung bow of dark, twisted living wood with thorns along its back and small green leaves at the tips, held upright.
2. Sunsteel blade: In the near hand, a longsword of bright, pale-gold steel with a sunburst on the guard and a faint warm glow, held low.
3. A mallet: In the near hand, a wooden mallet.
4. A spade: In the near hand, a spade held upright.
I'll save it as weapon-sheet-m14.png.

#### `weapon-sheet-m15` · Weapon sheet: 4 weapons (A heron-headed cane, A felling axe, An oil lamp, Varkhul's greatsword) · P1

**Basis:** Edits `weapon-sheet-male`. Cells: 1 `weapon-heron-cane`, 2 `weapon-felling-axe`, 3 `weapon-oil-lamp`, 4 `weapon-varkhul-greatsword`.

1. A heron-headed cane: In the near hand, a slim cane with a silver heron's-head handle.
2. A felling axe: In the near hand, a long felling axe held low, its head forward.
3. An oil lamp: In the near hand, a small brass oil lamp, lit.
4. Varkhul's greatsword: A huge two-handed greatsword of black iron held low in the near hand alone, point forward, a jagged black splinter of crystal set in its hilt pulsing with violet light.
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
2. Sir Corwin's sword, too big: A longsword with a gold hilt and a sun-shaped pommel, too long for the bearer, held upright in the near hand alone.
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
3. An armful of scrolls: An armful of rolled scrolls tied with ribbon and closed with wax seals, held in the near hand.
4. A broom: In the near hand, a straw broom held upright.
I'll save it as weapon-sheet-f02.png.

#### `weapon-sheet-f03` · Weapon sheet: 4 weapons (A coin on the knuckles, A wooden practice sword, A flat cap, held, A covered basket) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-coin`, 2 `weapon-practice-sword`, 3 `weapon-flat-cap-held`, 4 `weapon-basket`.

1. A coin on the knuckles: In the near hand, a copper coin balanced on the knuckles of the near hand.
2. A wooden practice sword: In the near hand, a wooden practice sword held low, point forward.
3. A flat cap, held: An old flat cap held by its brim in the near hand.
4. A covered basket: In the near hand, a wicker basket with a cloth over it, hanging from the near hand by its handle.
I'll save it as weapon-sheet-f03.png.

#### `weapon-sheet-f04` · Weapon sheet: 4 weapons (A flower basket, A pilgrim's staff, A baker's peel and a loaf, A drop spindle) · P0

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-flower-basket`, 2 `weapon-pilgrim-staff`, 3 `weapon-peel`, 4 `weapon-spindle`.

1. A flower basket: A wide wicker basket of pink roses, white lilies and daisies hanging from the far hand by its handle, and a single rose in the near hand.
2. A pilgrim's staff: In the near hand, a tall pilgrim's staff with a small brass bell tied at the top.
3. A baker's peel and a loaf: A long wooden baker's peel in the near hand, and a round loaf held in the far hand.
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
2. A ledger and a pen: A thick leather-bound ledger held in the far hand, and a pen in the near hand.
3. A folded shawl: In the near hand, a folded knitted shawl, held out as if offering it.
4. A sketchbook: A sketchbook held closed in the far hand, and a stick of charcoal in the near hand.
I'll save it as weapon-sheet-f06.png.

#### `weapon-sheet-f07` · Weapon sheet: 4 weapons (A lit lantern, A beekeeper's smoker, A sprig of night-flowers, Vreya's wavy blade) · P2

**Basis:** Edits `weapon-sheet-female`. Cells: 1 `weapon-lantern`, 2 `weapon-smoker`, 3 `weapon-night-flowers`, 4 `weapon-vreya-blade`.

1. A lit lantern: In the near hand, a brass lantern, lit, its warm light on the near side of the face.
2. A beekeeper's smoker: In the near hand, a tin beekeeper's smoker with bellows, a wisp of pale smoke curling from its spout, two or three bees nearby.
3. A sprig of night-flowers: In the near hand, a sprig of pale, star-shaped night-flowers.
4. Vreya's wavy blade: In the near hand, a long, wavy-bladed black sword held low, point forward.
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

Edit the attached blank base image: change ONLY the head: put a charcoal hood worn up, ending in a soft point at the back, the edge of the face in shadow on the bald head. It replaces the hair (hair and helm are one slot), so paint in any hair that should show under it. Where it doesn't cover them, the blank head and face stay exactly as they are. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as helm-kael-hood.png.

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

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long-hafted bearded battleaxe with a heavy steel head, held low, its head forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-battleaxe.png.

#### `weapon-greatsword` · Greatsword · P0

**Basis:** Edits `base-male`. From `Items/ItemCatalog.cs` (`greatsword`).

Edit the attached base image: change ONLY the hands, giving them a two-handed steel greatsword held low in the near hand alone, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-greatsword.png.

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

Edit the attached base image: change ONLY the hands, giving them a slim rapier with a swept hilt in the near hand, held low, point forward, and a parrying dagger held reversed in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-kael-rapier.png.

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

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a crooked wooden walking stick, held upright, its foot near the ground. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-walking-stick.png.

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

Edit the attached base image: change ONLY the hands, giving them a steaming iron stew pot hanging by its handle from the near hand, a cloth wrapped round the handle. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-stew-pot.png.

#### `weapon-silk-bolt` · A bolt of silk · P0

**Basis:** Edits `base-male`. **(derived)** From Osric.

Edit the attached base image: change ONLY the hands, giving them a bolt of rose-coloured silk hanging from the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-silk-bolt.png.

#### `weapon-lute` · A lute · P0

**Basis:** Edits `base-male`. **(derived)** From Lark.

Edit the attached base image: change ONLY the hands, giving them a small round-backed lute held by its neck in the near hand, hanging. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-lute.png.

#### `weapon-scrolls` · An armful of scrolls · P0

**Basis:** Edits `base-female`. **(derived)** From Ilsa.

Edit the attached base image: change ONLY the hands, giving them an armful of rolled scrolls tied with ribbon and closed with wax seals, held in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-scrolls.png.

#### `weapon-broom` · A broom · P0

**Basis:** Edits `base-female`. **(derived)** From Hesper; the maids.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a straw broom held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-broom.png.

#### `weapon-coin` · A coin on the knuckles · P0

**Basis:** Edits `base-female`. **(derived)** From Wren.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a copper coin balanced on the knuckles of the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-coin.png.

#### `weapon-practice-sword` · A wooden practice sword · P0

**Basis:** Edits `base-female`. From `10-classes.md` §5.1 (`train-imra`: a practice sword).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden practice sword held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-practice-sword.png.

#### `weapon-flat-cap-held` · A flat cap, held · P0

**Basis:** Edits `base-female`. **(derived)** From Maud, Pell, the lost boy.

Edit the attached base image: change ONLY the hands, giving them an old flat cap held by its brim in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-flat-cap-held.png.

#### `weapon-staff-ring` · Thessaly's ringed staff · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (a staff with a ring at the top).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a tall staff of dark wood topped with a plain open ring of silver metal, held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-staff-ring.png.

#### `weapon-ledger-pen` · A ledger and a pen · P1

**Basis:** Edits `base-female`. From `03-art-overhaul.md` §7.4 (Gannet: a ledger always under the arm).

Edit the attached base image: change ONLY the hands, giving them a thick leather-bound ledger held in the far hand, and a pen in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-ledger-pen.png.

#### `weapon-folded-shawl` · A folded shawl · P1

**Basis:** Edits `base-female`. From `07-companions.md` (`brannock-hester`: she holds a shawl).

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a folded knitted shawl, held out as if offering it. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-folded-shawl.png.

#### `weapon-mallet` · A mallet · P1

**Basis:** Edits `base-male`. **(derived)** From Tobin; the masons.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden mallet. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-mallet.png.

#### `weapon-spade` · A spade · P1

**Basis:** Edits `base-male`. **(derived)** From the levy, `mira-tobin`.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a spade held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spade.png.

#### `weapon-heron-cane` · A heron-headed cane · P1

**Basis:** Edits `base-male`. **(derived)** From Harrow.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a slim cane with a silver heron's-head handle. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-heron-cane.png.

#### `weapon-felling-axe` · A felling axe · P1

**Basis:** Edits `base-male`. **(derived)** From Hobb.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long felling axe held low, its head forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-felling-axe.png.

#### `weapon-black-box` · The Black Box · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §6.4 (`black-box`: a palm-sized lightless cube, faint seams, a hum ring).

Edit the attached base image: change ONLY the near hand: turn it palm-up, with a small cube about the size of a fist, of perfect lightless black with faint seams on its faces, floating just above the open palm, a thin ring of dull violet light turning slowly round it. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-black-box.png.

#### `weapon-oil-lamp` · An oil lamp · P1

**Basis:** Edits `base-male`. **(derived)** From `maleficio-young`.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a small brass oil lamp, lit. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-oil-lamp.png.

#### `weapon-sketchbook` · A sketchbook · P1

**Basis:** Edits `base-female`. **(derived)** From `mira-young`.

Edit the attached base image: change ONLY the hands, giving them a sketchbook held closed in the far hand, and a stick of charcoal in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-sketchbook.png.

#### `weapon-varkhul-greatsword` · Varkhul's greatsword · P1

**Basis:** Edits `base-male`. From `03-art-overhaul.md` §7.4 (a greatsword with the shard in the hilt).

Edit the attached base image: change ONLY the hands, giving them a huge two-handed greatsword of black iron held low in the near hand alone, point forward, a jagged black splinter of crystal set in its hilt pulsing with violet light. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-varkhul-greatsword.png.

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

Edit the attached base image: change ONLY the hands, giving them a longsword with a gold hilt and a sun-shaped pommel, too long for the bearer, held upright in the near hand alone. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-corwin-sword.png.

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

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wicker basket with a cloth over it, hanging from the near hand by its handle. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-basket.png.

#### `weapon-flower-basket` · A flower basket · P0

**Basis:** Edits `base-female`. **(derived)** From the `flower-seller` sheet.

Edit the attached base image: change ONLY the hands, giving them a wide wicker basket of pink roses, white lilies and daisies hanging from the far hand by its handle, and a single rose in the near hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-flower-basket.png.

#### `weapon-spear-lantern` · A spear and a lantern · P2

**Basis:** Edits `base-male`. From `08-random-encounters.md` (`enc-thrall-patrol`: a lantern).

Edit the attached base image: change ONLY the hands, giving them a plain spear held upright in the near hand, and a lit iron lantern hanging from the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-spear-lantern.png.

#### `weapon-vesk-sword` · Vesk's black sword · P2

**Basis:** Edits `base-male`. **(derived)** From Vesk.

Edit the attached base image: change ONLY the hands, giving them a long black sword held in front, point down, both gauntleted hands crossed on the pommel. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-vesk-sword.png.

#### `weapon-vreya-blade` · Vreya's wavy blade · P2

**Basis:** Edits `base-female`. **(derived)** From Vreya.

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a long, wavy-bladed black sword held low, point forward. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-vreya-blade.png.

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

Edit the attached base image: change ONLY the near hand: close its fingers naturally round a wooden pitchfork held upright. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-pitchfork.png.

#### `weapon-crate` · A crate, carried · P0

**Basis:** Edits `base-male`. **(derived)** From the `porter` sheet.

Edit the attached base image: change ONLY the hands, giving them a wooden crate held by a rope handle in the far hand, hanging. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-crate.png.

#### `weapon-peel` · A baker's peel and a loaf · P0

**Basis:** Edits `base-female`. **(derived)** From the `baker` sheet.

Edit the attached base image: change ONLY the hands, giving them a long wooden baker's peel in the near hand, and a round loaf held in the far hand. Draw the grip clearly in the fingers. Change nothing else: keep the pose, the body, the framing, the canvas size and position, the background and the line style exactly the same. Do not move, rescale or redraw anything else. Return it as a PNG at the same 1024×1536 size, on the same solid black background. I'll save it as weapon-peel.png.

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

Every person and humanoid foe, as the parts they wear, their morph and their colours. The faces are the shared set (§4): many people wear the same one, and the Colours column recolours each (the skin, the eyebrows, beard and hair, the eyes) with the game's swatches (`Characters/Parts/Swatches.cs`), so no two who share a face look alike. A head that isn't a face (§4.3) keeps its own colours. The Helm / hair column is what's on the head, one slot: a style from the hair library (§4.8), or a helm in its place. The Extras column is what they wear on the head besides (§4.10): a beard, jewellery, scars, a people's ears or horns, any number. `—` is an empty slot. A variant id (`elara.act3`) is the same character at another point in the story; `swap` cues in the scripts switch between them. Names with ids in backticks in the notes are other entries.

| Id | Name | Face | Armor | Cape | Helm / hair | Extras | Weapon | Morph (h × w) | Colours (skin, hair, eyes) | Priority | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `you-male` | You, the male default | `face-m03` | `armor-traveller` | — | `hair-03` | — | `weapon-longsword` | 1.00 × 1.00 | skin PEACH, hair CHESTNUT, eyes BROWN | P0 | the player swaps any slot on the creation screen |
| `you-female` | You, the female default | `face-f03` | `armor-traveller-f` | — | `hair-22` | `skin-01` | `weapon-longsword` | 1.00 × 1.00 | skin PEACH, hair CHESTNUT, eyes BROWN | P0 |  |
| `soulbound` | {sb}, the default | `face-f04` | `armor-sb-traveller` | `cape-sb` | `hair-31` | — | `weapon-sb-blades` | 1.02 × 1.00 | skin PEACH, hair BLACK, eyes BROWN | P0 | the player can change every slot |
| `you-male.champion` | You, Champion of Fable | `face-m03` | `armor-champion` | — | `hair-03` | — | `weapon-longsword` | 1.00 × 1.00 | skin PEACH, hair CHESTNUT, eyes BROWN | P1 | Act 1's end |
| `you-female.champion` | You, Champion of Fable | `face-f03` | `armor-champion-f` | — | `hair-22` | `skin-01` | `weapon-longsword` | 1.00 × 1.00 | skin PEACH, hair CHESTNUT, eyes BROWN | P1 |  |
| `soulbound.champion` | {sb}, Champion of Fable | `face-f04` | `armor-champion-f` | `cape-sb` | `hair-31` | — | `weapon-sb-blades` | 1.02 × 1.00 | skin PEACH, hair BLACK, eyes BROWN | P1 |  |
| `soulbound.lord-protector` | {sb}, Lord Protector | `face-f04` | `armor-lord-protector` | — | `hair-31` | — | `weapon-longsword` | 1.02 × 1.00 | skin PEACH, hair BLACK, eyes BROWN | P2 | Act 2's end |
| `mc-child` | You, aged eight | `face-m01` | `armor-child-cove` | — | `hair-14` | `skin-01` | — | 0.62 × 0.66 | skin PEACH, hair CHESTNUT, eyes BROWN | P0 |  |
| `sb-child` | {sb}, aged eight | `face-f01` | `armor-child-cove-f` | — | `hair-22` | — | — | 0.62 × 0.66 | skin PEACH, hair BLACK, eyes BROWN | P0 |  |
| `sera` | Sera Vantis | `face-f05` | `armor-sera` | `cape-sera` | `hair-33` | `ears-01` | `weapon-sera-longbow` | 1.04 × 0.90 | skin PORCELAIN, hair HONEY, eyes GREEN | P0 |  |
| `mira` | Mira Ashgrove | `face-f06` | `armor-mira` | — | `hair-38` | `skin-01` | `weapon-quarterstaff` | 1.01 × 0.90 | skin FAIR, hair COPPER, eyes BROWN | P0 | her healing glow is an effect |
| `mira-young` | Mira, aged thirteen | `face-f01` | `armor-mira-apprentice` | — | `hair-35` | `skin-01` | `weapon-sketchbook` | 0.80 × 0.80 | skin FAIR, hair COPPER, eyes BROWN | P1 | drawn as an echo |
| `kael` | Kael Duskmantle | `face-m04` | `armor-kael` | `cape-tail-ember` | `helm-kael-hood` | `horns-02` | `weapon-kael-knives` | 1.00 × 0.95 | skin TAN, hair BLACK, eyes AMBER | P0 | the horns come through the hood |
| `brannock` | Brannock Ironvein | `face-m09` | `armor-brannock` | — | `hair-04` | `beard-10`, `skin-03` | `weapon-brannock` | 0.85 × 1.36 | skin GOLDEN, hair COPPER, eyes BROWN | P0 | shield `weapon-brannock-1`, `-41`, `-42` as the names come |
| `brannock.clasps-gone` | Brannock, the clasps gone | `face-m09` | `armor-brannock` | — | — | `beard-10`, `skin-03` | `weapon-brannock-42` | 0.85 × 1.36 | skin GOLDEN, hair COPPER, eyes BROWN | P2 | from A3-C2-05 |
| `valencia` | Valencia Morvaine | `face-f04` | `armor-valencia` | `cape-valencia` | `helm-circlet-silver` | — | `weapon-dagger` | 1.09 × 0.90 | skin PORCELAIN, hair SILVER, eyes GREY | P2 |  |
| `valencia.sister-vale` | Sister Vale | `face-f04` | `armor-abbey-habit` | — | `helm-abbey-hood` | — | — | 1.09 × 0.90 | skin PORCELAIN, hair SILVER, eyes GREY | P2 | `valencia-unhooded`: the same without the helm, eyes red (an expression) |
| `corra` | Corra | `face-f08` | `armor-corra` | — | `hair-38` | — | `weapon-netting-needle` | 0.95 × 1.00 | skin TAN, hair CHESTNUT, eyes GREY | P0 |  |
| `corra.flashback` | Corra, sixteen years younger | `face-f07` | `armor-corra` | — | — | — | `weapon-netting-needle` | 0.95 × 1.00 | skin TAN, hair CHESTNUT, eyes GREY | P0 | soaked by rain (an effect) |
| `lookout` | The lookout | `face-m06` | `armor-sailor` | — | `helm-knit-cap` | `beard-12` | `weapon-telescope` | 1.00 × 0.95 | skin BROWN, hair COPPER, eyes GREY | P0 |  |
| `odelle` | Captain Odelle Marsh | `face-f07` | `armor-odelle` | — | `helm-tricorn` | `jewel-01` | `weapon-cutlass` | 1.00 × 1.00 | skin UMBER, hair BLACK, eyes BROWN | P0 |  |
| `tomas` | Old Tomas | `face-m10` | `armor-fisher` | — | `helm-knit-cap` | `beard-01` | `weapon-gaff` | 0.95 × 1.00 | skin TAN, hair WHITE, eyes SKY | P0 |  |
| `red-gull` | Red Gull Thug | `face-m06` | `armor-red-gull` | — | — | `beard-01`, `skin-05` | `weapon-cudgel` | 1.08 × 1.15 | skin UMBER, hair COPPER, eyes BROWN | P0 | also the enemy `red-gull-thug` |
| `red-gull.b` | Red Gull Thug, second head | `face-m06` | `armor-red-gull` | — | `helm-bandana-red` | `beard-01`, `skin-05` | `weapon-cudgel` | 1.08 × 1.15 | skin FAIR, hair COPPER, eyes GREY | P0 |  |
| `red-gull.c` | Red Gull Thug, third head | `face-m04` | `armor-red-gull` | — | `hair-09` | `beard-01`, `skin-05` | `weapon-cudgel` | 1.06 × 1.12 | skin TAN, hair BLACK, eyes BROWN | P0 |  |
| `grell` | Bosun Grell | `face-m06` | `armor-grell` | — | — | `beard-08`, `skin-02` | `weapon-cutlass` | 0.85 × 1.55 | skin TAN, hair COPPER, eyes BROWN | P0 | also the enemy `bosun-grell` |
| `hale` | Captain Roderic Hale | `face-m09` | `armor-watch-captain` | `cape-watch-captain` | `helm-watch-crest` | `beard-01` | `weapon-cudgel` | 1.03 × 1.14 | skin FAIR, hair SILVER, eyes GREY | P0 |  |
| `watch` | City Watch | `face-m05` | `armor-watch` | `cape-watch` | `helm-watch` | `beard-01` | `weapon-watch-kit` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes GREY | P0 | other watchmen wear `face-m09` and `face-m05` |
| `fennick` | Questmaster Fennick | `face-m07` | `armor-clerk` | — | `hair-05` | `beard-03`, `jewel-08`, `jewel-11` | `weapon-tack-hammer` | 1.00 × 1.08 | skin FAIR, hair SILVER, eyes GREY | P0 |  |
| `hedda` | Hedda (also `refugee-mother`) | `face-f07` | `armor-refugee-woman` | `cape-blanket` | `hair-26` | — | `weapon-loaf` | 0.99 × 0.95 | skin TAN, hair CHESTNUT, eyes BROWN | P0 |  |
| `refugee-child` | Lise | `face-f01` | `armor-refugee-child` | — | `hair-35` | — | `weapon-rag-doll` | 0.75 × 0.75 | skin TAN, hair CHESTNUT, eyes BROWN | P0 |  |
| `brisk` | Sergeant Brisk | `face-m07` | `armor-watch` | `cape-watch` | `helm-watch` | `beard-04` | `weapon-cudgel` | 1.00 × 1.06 | skin BROWN, hair BLACK, eyes BROWN | P0 |  |
| `refugee-1` | Old Aurek | `face-m10` | `armor-refugee-man` | `cape-blanket` | `hair-12` | `beard-09` | `weapon-walking-stick` | 0.93 × 0.95 | skin TAN, hair WHITE, eyes GREY | P0 |  |
| `refugee-2` | Teodor | `face-m02` | `armor-refugee-man` | `cape-blanket` | `hair-03` | `beard-01` | `weapon-tin-cup` | 0.95 × 0.90 | skin BROWN, hair BLACK, eyes BROWN | P0 |  |
| `refugee-3` | Yselle the Glassblower | `face-f06` | `armor-glassblower` | — | `hair-34` | `skin-01`, `skin-06` | — | 0.82 × 1.25 | skin GOLDEN, hair COPPER, eyes AMBER | P0 |  |
| `elara` | Princess Elara | `face-f05` | `armor-elara` | — | `helm-circlet` | — | — | 1.08 × 0.88 | skin FAIR, hair BLACK, eyes GREY | P0 | the slim sword is part of the armor |
| `elara-hooded` | Princess Elara, hooded | `face-f05` | `armor-elara-disguise` | `cape-elara-hooded` | `helm-hood-charcoal` | — | — | 1.08 × 0.88 | skin FAIR, hair BLACK, eyes GREY | P0 |  |
| `leon` | King Leon | `face-m10` | `armor-leon` | `cape-leon` | `helm-crown` | `beard-07` | — | 0.97 × 1.16 | skin FAIR, hair SILVER, eyes GREY | P0 | his sword is part of the armor |
| `crane` | Lord Chancellor Crane | `face-m08` | `armor-crane` | — | `hair-06` | `beard-02` | `weapon-quill-letter` | 1.10 × 0.85 | skin PORCELAIN, hair BLACK, eyes GREY | P0 |  |
| `royal-messenger` | Royal Messenger | `face-m02` | `armor-royal-livery` | `cape-royal` | `helm-beret` | `skin-01` | `weapon-letter` | 0.99 × 0.95 | skin FAIR, hair DARK BROWN, eyes GREEN | P0 |  |
| `royal-guard` | Royal Guard | `face-m09` | `armor-royal-guard` | `cape-royal` | `helm-royal-guard` | `beard-07` | `weapon-halberd` | 1.04 × 1.08 | skin UMBER, hair BLACK, eyes BROWN | P0 | the captain and lieutenant too |
| `anvil-barkeep` | Hessa the Barkeep | `face-f08` | `armor-tavern` | `cape-tail-bear` | `hair-26` | `skin-02`, `ears-03` | `weapon-tankard` | 1.08 × 1.20 | skin GOLDEN, hair BLACK, eyes BROWN | P0 |  |
| `netty` | Netty the Net-Mender | `face-f08` | `armor-villager-woman` | — | `hair-40` | `skin-01` | `weapon-netting-needle` | 0.98 × 1.00 | skin TAN, hair SILVER, eyes GREEN | P0 |  |
| `maud` | Maud Tolley | `face-f10` | `armor-elder-woman` | — | `hair-37` | — | `weapon-flat-cap-held` | 0.93 × 0.95 | skin FAIR, hair SILVER, eyes GREY | P0 |  |
| `pip` | Pip the Potboy | `face-m02` | `armor-tavern-m` | — | `hair-03` | `skin-01` | `weapon-stew-pot` | 0.72 × 0.80 | skin FAIR, hair COPPER, eyes BROWN | P0 |  |
| `osric` | Osric Penhallow | `face-m07` | `armor-merchant` | `cape-short-lavender` | `hair-05` | `beard-03` | `weapon-silk-bolt` | 1.00 × 1.00 | skin PORCELAIN, hair BLACK, eyes BROWN | P0 |  |
| `lark` | Lark the Busker | `face-m04` | `armor-busker` | `cape-short-lavender` | `hair-21` | `ears-01` | `weapon-lute` | 1.04 × 0.90 | skin PORCELAIN, hair COPPER, eyes VIOLET | P0 |  |
| `ilsa` | Ilsa Fenwright | `face-f03` | `armor-villager-woman` | — | `hair-34` | — | `weapon-scrolls` | 0.99 × 0.95 | skin GOLDEN, hair COPPER, eyes GREEN | P0 |  |
| `oswin` | Guardsman Oswin | `face-m03` | `armor-royal-guard` | `cape-royal` | `helm-royal-guard` | — | `weapon-halberd` | 1.00 × 1.00 | skin FAIR, hair COPPER, eyes BLUE | P0 |  |
| `hesper` | Sister Hesper | `face-f10` | `armor-temple` | — | `hair-27` | — | `weapon-broom` | 0.99 × 1.00 | skin UMBER, hair SILVER, eyes BROWN | P0 |  |
| `wren` | Wren | `face-f01` | `armor-urchin-f` | — | `hair-26` | `skin-01` | `weapon-coin` | 0.62 × 0.66 | skin BROWN, hair BLACK, eyes AMBER | P0 |  |
| `imra-vell` | Master Imra Vell | `face-f07` | `armor-fencer` | — | `hair-13` | `skin-03` | `weapon-practice-sword` | 0.99 × 1.00 | skin UMBER, hair BLACK, eyes BROWN | P0 | trainer |
| `tamsin` | Ranger Tamsin | `face-f07` | `armor-lodge-ranger` | `cape-tamsin` | `hair-26` | `skin-02`, `ears-05` | `weapon-shortbow` | 1.00 × 1.00 | skin TAN, hair CHESTNUT, eyes AMBER | P0 | trainer; her hawk is the creature `lodge-hawk` |
| `garrick` | Garrick the caravan master | `face-m09` | `armor-caravan` | — | `helm-wide-brim` | `beard-03`, `skin-02` | `weapon-light-crossbow` | 1.00 × 1.00 | skin TAN, hair BLACK, eyes BROWN | P1 |  |
| `aldric` | Commander Aldric Voss | `face-m09` | `armor-aldric` | `cape-campaign` | `hair-01` | `beard-01` | `weapon-longsword` | 1.00 × 1.05 | skin FAIR, hair SILVER, eyes SKY | P1 |  |
| `recruit` | The recruit | `face-m02` | `armor-recruit` | — | `helm-kettle` | — | `weapon-spear` | 0.96 × 0.92 | skin FAIR, hair GOLD, eyes BLUE | P1 |  |
| `thessaly` | Archmage Thessaly Varn | `face-f09` | `armor-thessaly` | — | `hair-01` | — | `weapon-staff-ring` | 1.00 × 1.00 | skin FAIR, hair SILVER, eyes GREY | P1 | trainer |
| `thessaly.thessaly-crystal` | Thessaly, frozen in crystal | `face-f09` | `armor-thessaly` | — | `hair-01` | — | `weapon-staff-ring` | 1.00 × 1.00 | skin FAIR, hair SILVER, eyes GREY | P2 | the crystal round her is a prop, not a part |
| `brenna` | Brenna of Ashfall | `face-f07` | `armor-brenna` | `cape-ash` | `hair-42` | `skin-04`, `skin-06` | `weapon-battleaxe` | 1.01 × 1.00 | skin PEACH, hair DARK BROWN, eyes GREY | P1 | trainer |
| `brenna.protectorate` | Brenna, Act 3 | `face-f07` | `armor-brenna-protectorate` | `cape-ash` | `hair-42` | `skin-04`, `skin-06` | `weapon-battleaxe` | 1.01 × 1.00 | skin PEACH, hair DARK BROWN, eyes GREY | P2 | also her boss fight |
| `gannet` | Mother Gannet | `face-f08` | `armor-gannet` | — | `helm-lace-cap` | — | `weapon-ledger-pen` | 0.99 × 1.20 | skin FAIR, hair SILVER, eyes BROWN | P1 |  |
| `hester` | Hester Hollis | `face-f10` | `armor-elder-woman` | — | `hair-37` | — | `weapon-folded-shawl` | 0.90 × 0.90 | skin PORCELAIN, hair WHITE, eyes SKY | P1 |  |
| `tobin` | Tobin Ashgrove | `face-m07` | `armor-cider-presser` | — | `hair-11` | `beard-07`, `skin-01` | `weapon-mallet` | 1.00 × 1.00 | skin FAIR, hair COPPER, eyes BROWN | P1 |  |
| `tobin.levy` | Tobin in the levy | `face-m07` | `armor-levy` | — | `hair-11` | `beard-07`, `skin-01` | `weapon-spade` | 1.00 × 0.95 | skin FAIR, hair COPPER, eyes BROWN | P1 | how he looks when you meet him |
| `harrow` | Lord Harrow | `face-m08` | `armor-harrow` | — | `hair-10` | `beard-11` | `weapon-heron-cane` | 1.00 × 1.05 | skin FAIR, hair SILVER, eyes GREY | P1 |  |
| `hobb` | Hobb the woodcutter | `face-m09` | `armor-woodcutter` | — | `helm-knit-cap` | `beard-08`, `skin-01` | `weapon-felling-axe` | 1.08 × 1.15 | skin FAIR, hair COPPER, eyes BLUE | P1 |  |
| `maleficio` | Maleficio | `face-m10` | `armor-maleficio` | — | `hair-10` | `beard-07` | `weapon-black-box` | 1.01 × 0.95 | skin FAIR, hair SILVER, eyes BROWN | P1 |  |
| `maleficio-young` | Maleficio, remembered | `face-m09` | `armor-scholar` | — | — | `beard-07` | `weapon-oil-lamp` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes BROWN | P1 | drawn as an echo |
| `relief-column-echo` | The relief column | `face-m05` | `armor-old-pattern` | — | `helm-nasal` | `beard-01` | `weapon-relief-kit` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes GREY | P1 | drawn pale |
| `lost-boy` | The woodcutter's boy | `face-m01` | `armor-urchin` | — | `hair-08` | `skin-01` | `weapon-flat-cap-held` | 0.62 × 0.66 | skin FAIR, hair COPPER, eyes BLUE | P1 |  |
| `mags` | Pedlar Mags | `face-f08` | `armor-pedlar` | `cape-pedlar-pack` | `helm-straw-hat` | — | `weapon-walking-stick` | 0.82 × 1.25 | skin TAN, hair SILVER, eyes BROWN | P1 | trainer; Duchess is a creature |
| `leon.ill` | King Leon, ill | `face-m10` | `armor-leon-furs` | `cape-leon` | `helm-crown` | `beard-07` | `weapon-walking-stick` | 0.95 × 1.10 | skin STONE, hair COPPER, eyes GREY | P2 |  |
| `elara.act3` | Elara, leading the resistance | `face-f05` | `armor-elara-field` | `cape-travelling` | `hair-16` | — | `weapon-map` | 1.08 × 0.88 | skin FAIR, hair BLACK, eyes GREY | P2 |  |
| `elara.crowned` | Elara, crowned | `face-f05` | `armor-elara-queen` | — | `helm-crown` | — | — | 1.08 × 0.88 | skin FAIR, hair BLACK, eyes GREY | P2 |  |
| `hale.protectorate` | Captain Hale, Act 3 | `face-m09` | `armor-protectorate` | `cape-watch-captain` | `helm-watch-crest` | `beard-01` | `weapon-cudgel` | 1.03 × 1.14 | skin FAIR, hair SILVER, eyes GREY | P2 |  |
| `refugee-child.aged` | Lise, aged by the tear | `face-f07` | `armor-refugee-child` | — | — | — | — | 0.98 × 0.95 | skin TAN, hair CHESTNUT, eyes BROWN | P2 | the child's smock, tight on her |
| `anselle` | Mother Superior Anselle | `face-f10` | `armor-anselle` | — | `hair-37` | — | `weapon-lantern` | 0.98 × 1.05 | skin FAIR, hair SILVER, eyes HAZEL | P2 | trainer |
| `orla` | Sister Orla | `face-f08` | `armor-beekeeper` | — | `helm-bee-veil` | `skin-01` | `weapon-smoker` | 0.98 × 1.10 | skin PEACH, hair SILVER, eyes BLUE | P2 | trainer |
| `corwin` | Sir Corwin Halbeck | `face-m05` | `armor-corwin` | `cape-corwin` | `helm-corwin` | — | `weapon-corwin-kit` | 1.04 × 1.05 | skin FAIR, hair SILVER, eyes BLUE | P2 | trainer |
| `severin` | Lord Severin Morvaine | `face-m08` | `armor-severin` | `cape-severin` | `helm-crown-thorns` | — | — | 1.10 × 0.90 | skin PORCELAIN, hair SILVER, eyes RED | P2 |  |
| `severin.severin-cell` | Severin, a prisoner | `face-m08` | `armor-prisoner` | — | — | — | — | 1.10 × 0.85 | skin STONE, hair SILVER, eyes RED | P2 |  |
| `isadora` | Lady Isadora | `face-f06` | `armor-isadora` | — | `hair-41` | — | `weapon-night-flowers` | 1.00 × 0.95 | skin FAIR, hair DARK BROWN, eyes BROWN | P2 | drawn as an echo |
| `valencia-child` | Valencia as a child | `face-f01` | `armor-child-dress` | — | `hair-21` | — | — | 0.62 × 0.66 | skin PORCELAIN, hair SILVER, eyes GREY | P2 | drawn as an echo |
| `pell` | Pell | `face-m02` | `armor-morvaine-man` | — | `hair-17` | `skin-01` | `weapon-flat-cap-held` | 0.95 × 0.90 | skin PORCELAIN, hair COPPER, eyes BROWN | P2 |  |
| `ide` | Ide | `face-f04` | `armor-shift` | — | `hair-47` | — | — | 0.97 × 0.88 | skin PORCELAIN, hair COPPER, eyes BROWN | P2 |  |
| `maren-elder` | Maren, Hollowmere's elder | `face-f09` | `armor-morvaine-woman` | — | `hair-40` | — | `weapon-walking-stick` | 0.98 × 0.92 | skin PORCELAIN, hair WHITE, eyes GREY | P2 |  |
| `blood-house-steward` | The blood-house steward | `face-m08` | `armor-morvaine-livery` | — | `hair-06` | — | `weapon-ledger-pen` | 1.00 × 0.92 | skin PORCELAIN, hair SILVER, eyes GREY | P2 |  |
| `hollowmere-child` | A Hollowmere child | `face-m01` | `armor-morvaine-man` | — | `hair-03` | — | `weapon-toy-sword` | 0.62 × 0.66 | skin PORCELAIN, hair COPPER, eyes GREY | P2 |  |
| `hollowmere-farmer` | A Hollowmere farmer | `face-f07` | `armor-morvaine-woman` | — | `helm-kerchief` | — | `weapon-basket` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes BROWN | P2 |  |
| `ruck` | Ruck, the bosun | `face-m09` | `armor-sailor` | — | `hair-11` | `beard-05` | `weapon-cutlass` | 0.95 × 1.10 | skin BROWN, hair SILVER, eyes GREY | P2 | trainer |
| `davin` | Davin | `face-m02` | `armor-davin` | — | `hair-03` | — | `weapon-corwin-sword` | 0.98 × 0.92 | skin TAN, hair DARK BROWN, eyes BROWN | P2 | trainer |
| `vorlaith` | Vorlaith, the Unmade King | `face-vorlaith` | `armor-vorlaith` | — | `helm-vorlaith` | — | — | 1.20 × 0.95 | — | P2 | `vorlaith-shape`: the same at 40% opacity |
| `guild-apprentice` | A Guild apprentice | `face-f02` | `armor-guild-apprentice` | — | `hair-22` | `skin-01` | `weapon-glass-lamp` | 0.94 × 0.88 | skin FAIR, hair HONEY, eyes GREEN | P2 |  |
| `mason` | A mason | `face-m09` | `armor-smith` | — | `helm-flat-cap` | `beard-08` | `weapon-mallet` | 1.00 × 1.05 | skin OLIVE, hair BLACK, eyes BROWN | P2 |  |
| `elder` | The Elder (sheet) | `face-m10` | `armor-elder-coat` | — | `hair-11` | `beard-11` | `weapon-walking-stick` | 0.97 × 0.95 | skin FAIR, hair SILVER, eyes GREY | P0 |  |
| `trader` | The Trader (sheet) | `face-f03` | `armor-trader` | — | `helm-wide-brim` | — | `weapon-coin-purse` | 1.00 × 1.00 | skin TAN, hair DARK BROWN, eyes GREEN | P0 | also the Chandler |
| `baker` | The Baker (sheet) | `face-f06` | `armor-baker` | — | `helm-kerchief` | — | `weapon-peel` | 0.98 × 1.10 | skin FAIR, hair DARK BROWN, eyes BROWN | P0 | also the Innkeeper |
| `smith` | The Smith (sheet) | `face-m09` | `armor-smith` | — | `hair-04` | `beard-07` | `weapon-hammer-tongs` | 1.00 × 1.10 | skin UMBER, hair BLACK, eyes BROWN | P0 |  |
| `flower-seller` | The Flower Seller (sheet) | `face-f03` | `armor-villager-woman` | — | `helm-kerchief` | — | `weapon-flower-basket` | 0.98 × 0.92 | skin TAN, hair DARK BROWN, eyes GREEN | P0 |  |
| `traveller` | A Traveller (sheet) | `face-m05` | `armor-road` | `cape-travelling` | `hair-02` | `beard-01` | `weapon-walking-stick` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes GREY | P0 | also the Harbourmaster, a Regular |
| `seamstress` | The Seamstress (sheet) | `face-f03` | `armor-villager-woman` | — | `hair-29` | `skin-01` | — | 0.98 × 0.92 | skin FAIR, hair COPPER, eyes BLUE | P0 |  |
| `courier` | A Courier (sheet) | `face-m02` | `armor-courier` | — | `helm-flat-cap` | `skin-01` | `weapon-letters` | 0.96 × 0.90 | skin FAIR, hair DARK BROWN, eyes GREEN | P0 | also `COURIER` in Act 3 |
| `pilgrim` | A Pilgrim (sheet) | `face-f06` | `armor-pilgrim` | — | `hair-26` | — | `weapon-pilgrim-staff` | 1.00 × 1.00 | skin BROWN, hair BLACK, eyes BROWN | P0 | also `PILGRIM` |
| `weaver` | The Weaver (sheet) | `face-f06` | `armor-villager-woman` | — | `hair-38` | — | `weapon-spindle` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes BROWN | P0 |  |
| `farmhand` | A Farmhand (sheet) | `face-m03` | `armor-villager-man` | — | `helm-straw-hat` | — | `weapon-pitchfork` | 1.00 × 1.00 | skin FAIR, hair GOLD, eyes BLUE | P0 | also a Dock Hand, `FARMER` |
| `old-man` | An Old Man (sheet) | `face-m10` | `armor-elder-coat` | — | `hair-11` | `beard-11` | `weapon-walking-stick` | 0.94 × 1.00 | skin FAIR, hair SILVER, eyes GREY | P0 |  |
| `maid` | A Maid (sheet) | `face-f03` | `armor-maid` | — | `hair-29` | `skin-01` | `weapon-feather-duster` | 0.97 × 0.92 | skin FAIR, hair COPPER, eyes BLUE | P0 |  |
| `porter` | A Porter (sheet) | `face-m05` | `armor-porter` | — | `hair-03` | `beard-01` | `weapon-crate` | 1.00 × 1.08 | skin OLIVE, hair BLACK, eyes BROWN | P0 | also a Carrier, a Warehand |
| `old-woman` | An Old Woman (sheet) | `face-f10` | `armor-elder-woman` | `cape-old-woman` | `hair-37` | — | `weapon-basket` | 0.90 × 0.95 | skin FAIR, hair SILVER, eyes BROWN | P0 | also a Drinker |
| `child` | A Child (sheet) | `face-m01` | `armor-urchin` | — | `helm-flat-cap` | — | `weapon-toy-sword` | 0.62 × 0.66 | skin TAN, hair DARK BROWN, eyes BROWN | P0 |  |
| `youth` | A Youth (sheet) | `face-m02` | `armor-villager-man` | — | `hair-03` | `skin-01` | — | 0.90 × 0.88 | skin FAIR, hair DARK BROWN, eyes GREEN | P0 |  |
| `pool-questeria` | The city pool (sample) | `face-f06` | `armor-villager-woman` | — | `helm-kerchief` | — | `weapon-basket` | 1.00 × 1.00 | skin FAIR, hair DARK BROWN, eyes BROWN | P0 | the generator mixes the town faces and armors by seed |
| `pool-court` | The court (sample: a courtier) | `face-m04` | `armor-noble` | — | `hair-21` | `ears-01` | — | 1.00 × 0.95 | skin PORCELAIN, hair COPPER, eyes GREEN | P0 | the courtiers and the scribe in the throne room |
| `pool-court-lady` | The court (sample: a lady) | `face-f03` | `armor-noble-gown` | — | `hair-29` | `skin-01` | — | 0.98 × 0.92 | skin FAIR, hair COPPER, eyes BLUE | P0 |  |
| `pool-saltwind` | The docks pool (sample) | `face-m09` | `armor-fisher` | — | `helm-knit-cap` | `beard-08` | — | 1.00 × 1.05 | skin OLIVE, hair BLACK, eyes BROWN | P0 | also `NET-MENDER`, `SAILOR` |
| `pool-doomward` | The Doomward pool (sample) | `face-m05` | `armor-refugee-man` | `cape-blanket` | `hair-02` | `beard-01` | `weapon-walking-stick` | 1.00 × 0.95 | skin FAIR, hair DARK BROWN, eyes GREY | P0 |  |
| `pool-morvaine` | The Morvaine pool (sample) | `face-f07` | `armor-morvaine-woman` | — | `hair-33` | — | `weapon-lantern` | 1.00 × 0.92 | skin PORCELAIN, hair BLACK, eyes BROWN | P2 |  |
| `pool-abbey` | The abbey pool (sample) | `face-f06` | `armor-abbey-lay` | — | `helm-straw-hat` | — | `weapon-basket` | 0.72 × 0.80 | skin FAIR, hair DARK BROWN, eyes HAZEL | P2 |  |
| `red-gull-thug` | Red Gull Thug | `face-m06` | `armor-red-gull` | — | — | `beard-01`, `skin-05` | `weapon-cudgel` | 1.08 × 1.15 | skin UMBER, hair COPPER, eyes BROWN | P0 | the same as `red-gull`, with its two other heads |
| `bosun-grell` | Bosun Grell | `face-m06` | `armor-grell` | — | — | `beard-08`, `skin-02` | `weapon-cutlass` | 0.85 × 1.55 | skin TAN, hair COPPER, eyes BROWN | P0 | the same as `grell` |
| `hollowed-footman` | Hollowed Footman | `face-hollowed` | `armor-hollowed-footman` | — | `helm-hollowed-kettle` | — | `weapon-hollowed-sword` | 0.97 × 1.00 | — | P0 | hunched (a clip) |
| `hollowed-hexer` | Hollowed Hexer | `face-hollowed` | `armor-hollowed-hexer` | — | `helm-hollowed-hood` | — | `weapon-hexer-staff` | 1.01 × 0.90 | — | P0 |  |
| `hollowed-sapper` | Hollowed Sapper | `face-hollowed` | `armor-hollowed-sapper` | — | `helm-sapper-goggles` | — | `weapon-sapper-pick` | 0.95 × 1.05 | — | P0 |  |
| `sapper-chief` | Sapper Chief | `face-hollowed` | `armor-hollowed-chief` | — | `helm-sapper-faceplate` | — | `weapon-chief-pick-lantern` | 1.06 × 1.20 | — | P0 |  |
| `vault-construct` | Vault Construct | `face-construct-vault` | `armor-construct-stone` | — | — | — | — | 1.20 × 1.35 | — | P0 |  |
| `kael-duel` | Kael, duelling | `face-m04` | `armor-kael` | `cape-tail-ember` | `helm-kael-hood` | `horns-02` | `weapon-kael-rapier` | 1.00 × 0.95 | skin TAN, hair BLACK, eyes AMBER | P0 |  |
| `hollowed-climber` | Hollowed Climber | `face-hollowed` | `armor-hollowed-climber` | — | — | — | `weapon-climber-hook` | 1.02 × 0.88 | — | P1 | crouched (a clip) |
| `varkhul` | Varkhul, the Hollow Knight | `face-m10` | `armor-varkhul` | `cape-varkhul` | `helm-varkhul` | `beard-07` | `weapon-varkhul-greatsword` | 1.20 × 1.25 | skin STONE, hair SILVER, eyes AMBER | P1 | boss |
| `varkhul.varkhul-unmasked` | Varkhul, unmasked | `face-m10` | `armor-varkhul-split` | `cape-varkhul` | `hair-47` | `beard-07` | — | 1.20 × 1.25 | skin STONE, hair SILVER, eyes AMBER | P1 | kneels (a clip) |
| `maleficio-boss` | Maleficio, the Sovereign | `face-m10` | `armor-maleficio-ascendant` | — | `hair-10` | `beard-07` | `weapon-black-box` | 1.01 × 0.95 | skin FAIR, hair SILVER, eyes BROWN | P1 | floats (the game lifts it); swaps to the creature `hungering` |
| `cube-cultist` | Cube Cultist | `face-m05` | `armor-cultist` | — | `hair-01` | `skin-06` | `weapon-cultist-dagger` | 1.00 × 1.00 | skin FAIR, hair COPPER, eyes GREY | P1 |  |
| `bandit` | Bandit | `face-m05` | `armor-bandit` | — | `helm-bandit-hood` | `beard-01` | `weapon-shortsword` | 1.00 × 1.00 | skin OLIVE, hair BLACK, eyes BROWN | P1 | a new id |
| `thrall` | Thrall | `face-m05` | `armor-thrall` | — | `hair-01` | — | `weapon-spear-lantern` | 1.00 × 1.00 | skin PORCELAIN, hair COPPER, eyes RED | P2 |  |
| `vesk` | Vesk | `face-m05` | `armor-thrall-knight` | `cape-thrall-knight` | `helm-vesk-wolf` | — | `weapon-vesk-sword` | 1.08 × 1.15 | skin PORCELAIN, hair COPPER, eyes RED | P2 |  |
| `vreya` | Vreya | `face-f04` | `armor-thrall-knight-f` | `cape-thrall-knight` | `helm-vreya-crown` | — | `weapon-vreya-blade` | 1.06 × 1.00 | skin PORCELAIN, hair WHITE, eyes RED | P2 |  |
| `mordain` | Mordain | `face-m05` | `armor-thrall-knight` | `cape-thrall-knight` | `helm-mordain-beak` | — | `weapon-mordain-glaive` | 1.08 × 0.90 | skin PORCELAIN, hair COPPER, eyes RED | P2 |  |
| `gilded-knight` | Gilded Knight | `face-m05` | `armor-gilded-knight` | `cape-gilded` | `helm-gilded` | `beard-01` | `weapon-gilded-kit` | 1.02 × 1.05 | skin FAIR, hair DARK BROWN, eyes GREY | P2 | the `order` pool |
| `gilded-sergeant` | Gilded Sergeant | `face-m09` | `armor-gilded-knight` | `cape-gilded` | `hair-02` | `beard-08` | `weapon-mace-shield` | 1.00 × 1.12 | skin OLIVE, hair BLACK, eyes BROWN | P2 |  |
| `drowned` | Drowned | `face-drowned` | `armor-drowned` | — | — | — | — | 0.97 × 1.05 | — | P2 |  |
| `blood-wight` | Blood-wight | `face-blood-wight` | `armor-blood-wight` | — | — | — | — | 1.10 × 0.85 | — | P2 |  |
| `sun-scorched` | Sun-scorched | `face-sun-scorched` | `armor-sun-scorched` | — | — | — | — | 1.00 × 1.00 | — | P2 |  |
| `thornling` | Thornling | `face-thornling` | `armor-thornling` | — | — | — | — | 0.68 × 0.70 | — | P2 |  |
| `priest-construct` | Priest-construct | `face-construct-priest` | `armor-construct-priest` | — | — | — | — | 1.20 × 1.30 | — | P2 |  |
| `severin-boss` | Severin, unbound | `face-m08` | `armor-severin` | `cape-severin-wings` | `helm-crown-thorns` | — | — | 1.10 × 0.90 | skin PORCELAIN, hair SILVER, eyes RED | P2 | floats; claws and blood magic are effects |
| `smuggler` | Smuggler | `face-f03` | `armor-smuggler` | — | `helm-bandana` | `skin-01` | `weapon-cutlass` | 1.00 × 1.00 | skin FAIR, hair COPPER, eyes BLUE | P2 | a new id |
| `protectorate-watchman` | Protectorate watchman | `face-m05` | `armor-protectorate` | `cape-watch` | `helm-watch` | `beard-01` | `weapon-protectorate-kit` | 1.00 × 1.00 | skin OLIVE, hair BLACK, eyes BROWN | P2 | also `PROTECTORATE GUARD`; the `protectorate` pool |
| `protectorate-warden` | Protectorate warden | `face-m09` | `armor-protectorate-warden` | — | `helm-warden` | `beard-07` | `weapon-warden-kit` | 1.08 × 1.18 | skin UMBER, hair BLACK, eyes BROWN | P2 |  |
| `crown-hollowed` | Crown-raised Hollowed | `face-crown-hollowed` | `armor-crown-hollowed` | — | `helm-hollowed-kettle` | — | `weapon-spear` | 1.00 × 1.00 | — | P2 |  |
| `crown-construct` | Crown-construct | `face-construct-crown` | `armor-construct-crown` | — | — | — | — | 1.25 × 1.20 | — | P2 |  |
| `kael-shadow` | Kael, the Protectorate Shadow | `face-m04` | `armor-kael-shadow` | `cape-tail-ember` | `hair-03` | `horns-02` | `weapon-kael-knives` | 1.00 × 0.95 | skin TAN, hair BLACK, eyes AMBER | P2 | boss |
| `sb-sovereign` | {sb}, the Sovereign Protector | `face-f04` | `armor-sovereign` | — | `helm-crown-dusk-dawn` | — | `weapon-longsword` | 1.02 × 1.00 | skin PEACH, hair BLACK, eyes BROWN | P2 | the final boss; the player's own face and look; floats in phase 2 |
| `unmade` | The Unmade (ancient) | `face-unmade` | `armor-unmade-bronze` | — | `helm-bronze-crest` | — | `weapon-unmade-spear-shield` | 1.00 × 1.00 | — | P2 |  |
| `unmade.b` | The Unmade (a knight) | `face-unmade` | `armor-old-pattern` | — | `helm-nasal` | — | `weapon-longsword` | 1.00 × 1.00 | — | P2 | the game greys it |
| `unmade.c` | The Unmade (a pikeman) | `face-unmade` | `armor-pikeman` | — | `helm-morion` | — | `weapon-pike` | 1.00 × 1.00 | — | P2 |  |
| `unmade.d` | The Unmade (a watchman) | `face-unmade` | `armor-watch` | `cape-watch` | `helm-watch` | — | `weapon-cudgel` | 1.00 × 1.00 | — | P2 | the game greys it |
| `ashfall-raider` | Ashfall raider | `face-m09` | `armor-ashfall-raider` | `cape-ash` | `hair-02` | `beard-08` | `weapon-raider-kit` | 1.00 × 1.00 | skin OLIVE, hair BLACK, eyes BROWN | P2 |  |

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
| `face-m01` | Man 1, the boy | face | §4 | P0 |
| `face-m02` | Man 2, the lad | face | §4 | P0 |
| `face-m03` | Man 3, the hero | face | §4 | P0 |
| `face-m04` | Man 4, the rogue | face | §4 | P0 |
| `face-m05` | Man 5, the soldier | face | §4 | P0 |
| `face-m06` | Man 6, the bruiser | face | §4 | P0 |
| `face-m07` | Man 7, the kindly man | face | §4 | P0 |
| `face-m08` | Man 8, the noble | face | §4 | P0 |
| `face-m09` | Man 9, the veteran | face | §4 | P0 |
| `face-m10` | Man 10, the elder | face | §4 | P0 |
| `face-f01` | Woman 1, the girl | face | §4 | P0 |
| `face-f02` | Woman 2, the maiden | face | §4 | P0 |
| `face-f03` | Woman 3, the heroine | face | §4 | P0 |
| `face-f04` | Woman 4, the quick wit | face | §4 | P0 |
| `face-f05` | Woman 5, the lady | face | §4 | P0 |
| `face-f06` | Woman 6, the gentle soul | face | §4 | P0 |
| `face-f07` | Woman 7, the fighter | face | §4 | P0 |
| `face-f08` | Woman 8, the matron | face | §4 | P0 |
| `face-f09` | Woman 9, the matriarch | face | §4 | P0 |
| `face-f10` | Woman 10, the grandmother | face | §4 | P0 |
| `face-vorlaith` | Vorlaith, the Unmade King | face | §4 | P2 |
| `face-hollowed` | The Hollowed | face | §4 | P0 |
| `face-crown-hollowed` | Crown-raised Hollowed | face | §4 | P2 |
| `face-construct-vault` | Vault construct's head | face | §4 | P0 |
| `face-construct-priest` | Priest-construct's head | face | §4 | P2 |
| `face-construct-crown` | Crown-construct's head | face | §4 | P2 |
| `face-drowned` | Drowned | face | §4 | P2 |
| `face-blood-wight` | Blood-wight | face | §4 | P2 |
| `face-sun-scorched` | Sun-scorched | face | §4 | P2 |
| `face-thornling` | Thornling | face | §4 | P2 |
| `face-unmade` | The Unmade | face | §4 | P2 |
| `hair-01` | Hair 1, the buzz | hair | §4.8 | P0 |
| `hair-02` | Hair 2, the crop | hair | §4.8 | P0 |
| `hair-03` | Hair 3, short and messy | hair | §4.8 | P0 |
| `hair-04` | Hair 4, short curls | hair | §4.8 | P0 |
| `hair-05` | Hair 5, the side parting | hair | §4.8 | P0 |
| `hair-06` | Hair 6, slicked back | hair | §4.8 | P0 |
| `hair-07` | Hair 7, the undercut | hair | §4.8 | P0 |
| `hair-08` | Hair 8, spiky | hair | §4.8 | P0 |
| `hair-09` | Hair 9, the short topknot | hair | §4.8 | P0 |
| `hair-10` | Hair 10, receding | hair | §4.8 | P0 |
| `hair-11` | Hair 11, the bald crown | hair | §4.8 | P0 |
| `hair-12` | Hair 12, bald with wisps | hair | §4.8 | P0 |
| `hair-13` | Hair 13, the pixie cut | hair | §4.8 | P0 |
| `hair-14` | Hair 14, the mop | hair | §4.8 | P0 |
| `hair-15` | Hair 15, the fringed bob | hair | §4.8 | P0 |
| `hair-16` | Hair 16, the choppy cut | hair | §4.8 | P0 |
| `hair-17` | Hair 17, shaggy | hair | §4.8 | P0 |
| `hair-18` | Hair 18, wavy to the jaw | hair | §4.8 | P0 |
| `hair-19` | Hair 19, shoulder curls | hair | §4.8 | P0 |
| `hair-20` | Hair 20, the afro | hair | §4.8 | P0 |
| `hair-21` | Hair 21, straight to the shoulders | hair | §4.8 | P0 |
| `hair-22` | Hair 22, the short tail | hair | §4.8 | P0 |
| `hair-23` | Hair 23, windswept | hair | §4.8 | P0 |
| `hair-24` | Hair 24, swept back | hair | §4.8 | P0 |
| `hair-25` | Hair 25, the half-up knot | hair | §4.8 | P0 |
| `hair-26` | Hair 26, pulled back tight | hair | §4.8 | P0 |
| `hair-27` | Hair 27, long and straight | hair | §4.8 | P0 |
| `hair-28` | Hair 28, long, over one shoulder | hair | §4.8 | P0 |
| `hair-29` | Hair 29, long and wavy | hair | §4.8 | P0 |
| `hair-30` | Hair 30, long curls | hair | §4.8 | P0 |
| `hair-31` | Hair 31, the high ponytail | hair | §4.8 | P0 |
| `hair-32` | Hair 32, the low tail | hair | §4.8 | P0 |
| `hair-33` | Hair 33, the shoulder braid | hair | §4.8 | P0 |
| `hair-34` | Hair 34, the back braid | hair | §4.8 | P0 |
| `hair-35` | Hair 35, two plaits | hair | §4.8 | P0 |
| `hair-36` | Hair 36, two bunches | hair | §4.8 | P0 |
| `hair-37` | Hair 37, the low bun | hair | §4.8 | P0 |
| `hair-38` | Hair 38, the messy bun | hair | §4.8 | P0 |
| `hair-39` | Hair 39, the high bun | hair | §4.8 | P0 |
| `hair-40` | Hair 40, the crown braid | hair | §4.8 | P0 |
| `hair-41` | Hair 41, the braided updo | hair | §4.8 | P0 |
| `hair-42` | Hair 42, the warrior's topknot | hair | §4.8 | P0 |
| `hair-43` | Hair 43, the mohawk | hair | §4.8 | P0 |
| `hair-44` | Hair 44, locs | hair | §4.8 | P0 |
| `hair-45` | Hair 45, many braids | hair | §4.8 | P0 |
| `hair-46` | Hair 46, to the waist | hair | §4.8 | P0 |
| `hair-47` | Hair 47, long and wild | hair | §4.8 | P0 |
| `hair-48` | Hair 48, long and wispy | hair | §4.8 | P0 |
| `beard-01` | Stubble | extra | §4.10 | P0 |
| `beard-02` | The thin moustache | extra | §4.10 | P0 |
| `beard-03` | The moustache | extra | §4.10 | P0 |
| `beard-04` | The drooping moustache | extra | §4.10 | P0 |
| `beard-05` | The goatee | extra | §4.10 | P0 |
| `beard-06` | The chin strap | extra | §4.10 | P0 |
| `beard-07` | The short beard | extra | §4.10 | P0 |
| `beard-08` | The full beard | extra | §4.10 | P0 |
| `beard-09` | The long beard | extra | §4.10 | P0 |
| `beard-10` | The braided beard | extra | §4.10 | P0 |
| `beard-11` | The side-whiskers | extra | §4.10 | P0 |
| `beard-12` | The scruffy beard | extra | §4.10 | P0 |
| `jewel-01` | The small hoop | extra | §4.10 | P0 |
| `jewel-02` | The big hoop | extra | §4.10 | P0 |
| `jewel-03` | The stud | extra | §4.10 | P0 |
| `jewel-04` | The drop | extra | §4.10 | P0 |
| `jewel-05` | The ear rings | extra | §4.10 | P0 |
| `jewel-06` | The feather | extra | §4.10 | P0 |
| `jewel-07` | The nose ring | extra | §4.10 | P0 |
| `jewel-08` | The spectacles | extra | §4.10 | P0 |
| `jewel-09` | The eyepatch | extra | §4.10 | P0 |
| `jewel-10` | The monocle | extra | §4.10 | P0 |
| `jewel-11` | The quill | extra | §4.10 | P0 |
| `jewel-12` | The pearl | extra | §4.10 | P0 |
| `skin-01` | Freckles | extra | §4.10 | P0 |
| `skin-02` | The brow scar | extra | §4.10 | P0 |
| `skin-03` | The nose scar | extra | §4.10 | P0 |
| `skin-04` | The burn | extra | §4.10 | P0 |
| `skin-05` | The tattoo | extra | §4.10 | P0 |
| `skin-06` | The ash | extra | §4.10 | P0 |
| `ears-01` | Pointed ears | extra | §4.10 | P0 |
| `ears-02` | Wolf's ears | extra | §4.10 | P0 |
| `ears-03` | Bear's ears | extra | §4.10 | P0 |
| `ears-04` | Fox's ears | extra | §4.10 | P0 |
| `ears-05` | Feather tufts | extra | §4.10 | P0 |
| `ears-06` | Short points | extra | §4.10 | P0 |
| `horns-01` | Ram's horns | extra | §4.10 | P0 |
| `horns-02` | Swept horns | extra | §4.10 | P0 |
| `horns-03` | Short horns | extra | §4.10 | P0 |
| `horns-04` | Bull's horns | extra | §4.10 | P0 |
| `horns-05` | Antlers | extra | §4.10 | P0 |
| `horns-06` | Curled horns | extra | §4.10 | P0 |
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
| `armor-clerk-f` | A clerk's waistcoat, on the woman's base | armor | §5 | P1 |
| `armor-refugee-woman` | A refugee woman's clothes | armor | §5 | P0 |
| `armor-refugee-man` | A refugee man's clothes | armor | §5 | P0 |
| `armor-refugee-man-f` | A refugee's coat, on the woman's base | armor | §5 | P1 |
| `armor-refugee-child` | A refugee child's smock | armor | §5 | P0 |
| `armor-glassblower` | A glassblower's apron | armor | §5 | P0 |
| `armor-elara` | Elara's riding coat | armor | §5 | P0 |
| `armor-elara-disguise` | Elara in disguise | armor | §5 | P0 |
| `armor-leon` | King Leon's robes | armor | §5 | P0 |
| `armor-crane` | Crane's long coat | armor | §5 | P0 |
| `armor-crane-f` | A long plum coat, on the woman's base | armor | §5 | P1 |
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
| `weapon-flat-cap-held` | A flat cap, held | weapon | §6.3 | P0 |
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
| `weapon-crate` | A crate, carried | weapon | §6.3 | P0 |
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
