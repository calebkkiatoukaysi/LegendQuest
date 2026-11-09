# Assets

Every file the game ships or reads, and where it came from. Nothing here was downloaded or recorded:
the art is drawn by `tools/generate_ui.py`, `tools/generate_world.py` and
`tools/generate_paperdoll.py`, `tools/generate_harbour.py`, or cut by `tools/cut_supplied.py` from pictures I made (the title,
Questeria and its people), the music and sound effects are synthesised by `tools/generate_audio.py`,
the maps are written by hand, and the only outside work is the font, the MonoGame template files,
and a few rules from the SRD 5.1 (credited at the end).
`python tools/check_assets.py` fails if this list and the files on disk ever disagree.

## Sprites

Content/torch-flame.png - created by Caleb Kiatoukaysi for Legend Quest, six frames of a torch flame, drawn procedurally by tools/generate_world.py, released under public domain

Content/glow.png - created by Caleb Kiatoukaysi for Legend Quest, the soft white light every glow is drawn with, drawn procedurally by tools/generate_world.py, released under public domain

Content/mote.png - created by Caleb Kiatoukaysi for Legend Quest, the small soft dot the title's stars and sparkles are drawn with, drawn procedurally by tools/generate_world.py, released under public domain

## Characters and armour

Content/Paperdoll/body.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the youth with his hair taken off, every part of him (skin, eyes, shirt, vest, breeches) cut down to a few key shades for the palette swap; standing, four steps and the five fighting frames each way (posed from his standing frame), 32x48 frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-tousled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: his own hair, on a layer of its own, in the hair's key shades; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-cropped.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: his hair cut back to a cap round his skull and edged again; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-long.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: his hair with long locks drawn onto it, framing the face from the front and down the back otherwise; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-tail.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: his hair with a tail drawn onto the back of it, tied at the nape; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-braid.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: his hair with a braid drawn onto it, over the shoulder from the front; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/face-gentle.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the gentle face, only the pixels round his eyes it changes, drawn from where his eyes are in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/face-lashed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the lashed face, only the pixels round his eyes it changes, drawn from where his eyes are in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/face-stern.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the stern face, only the pixels round his eyes it changes, drawn from where his eyes are in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/face-freckled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the freckled face, only the pixels round his eyes it changes, drawn from where his eyes are in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/face-scarred.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the scarred face, only the pixels round his eyes it changes, drawn from where his eyes are in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/beard-stubble.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a stubble, in the hair's key shades so it's always the colour of the hair, drawn from where his chin is in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/beard-moustache.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a moustache, in the hair's key shades so it's always the colour of the hair, drawn from where his chin is in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/beard-beard.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a beard, in the hair's key shades so it's always the colour of the hair, drawn from where his chin is in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/beard-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a full beard, in the hair's key shades so it's always the colour of the hair, drawn from where his chin is in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/skirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a long skirt over his breeches, in their key shades, as wide as his stride in each frame; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/cape-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the part of a cape that hangs behind whoever's wearing it, in the cloak's key shades; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/cape-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the part of a cape that goes over whoever's wearing it: the drape and clasp from the front, all of it from behind; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hair-bald.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a bald head: a smooth crown of skin where his hair was, in the skin's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/ears-pointed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a Sylvan's long pointed ears, in the skin's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/ears-wolf.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a Wildkin wolf line's ears on the crown, in the hair's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/ears-hawk.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a Wildkin hawk line's feathered tufts, in the hair's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/ears-bear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a Wildkin bear line's round ears, in the hair's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/horns-curl.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: an Emberkin's horns, curled round like a ram's, in bone that never changes colour; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/horns-swept.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: an Emberkin's horns, swept back; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/tail-ember-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the half of an Emberkin's tail that hangs behind them: thin, crimson, its tip burning; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/tail-ember-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the half of an Emberkin's tail that shows over them from behind; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/tail-wolf-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the half of a Wildkin wolf line's bushy tail that hangs behind them, in the hair's key shades; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/tail-wolf-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the half of a wolf tail that shows over them from behind; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-bandana.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a bandana knotted at the back, in the vest's key shades (the Red Gulls wear crimson); drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-tricorn.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a captain's tricorn, black with a gold edge; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-helm.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the City Watch's steel helm; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-crown.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: King Leon's small gold crown, three points and a ruby in front, sat on top of the head over the hair, in gold that never changes colour; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-hood.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a deep travelling hood in the cloak's key shades, open on the face and falling onto the shoulders; Paperdoll leaves the hair and ears off under it; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/hat-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a slim gold circlet over the hair at the hairline, a sapphire on the brow; drawn procedurally by tools/generate_paperdoll.py onto the youth's frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/character-palettes.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: every preset the palette swap can recolour a sheet to, one ramp a row, the youth's own keys first in each block (skin, hair, eyes, shirt, vest, legs, cloak); the other presets were chosen by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/body.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/body.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-tousled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-tousled.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-cropped.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-cropped.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-long.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-long.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-tail.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-tail.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-braid.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-braid.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hair-bald.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-bald.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/face-gentle.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-gentle.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/face-lashed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-lashed.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/face-stern.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-stern.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/face-freckled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-freckled.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/face-scarred.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-scarred.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/beard-stubble.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-stubble.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/beard-moustache.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-moustache.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/beard-beard.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-beard.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/beard-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-full.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/skirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/skirt.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/cape-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/cape-back.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/cape-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/cape-front.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/ears-pointed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-pointed.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/ears-wolf.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-wolf.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/ears-hawk.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-hawk.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/ears-bear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-bear.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/horns-curl.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/horns-curl.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/horns-swept.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/horns-swept.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/tail-ember-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-ember-back.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/tail-ember-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-ember-front.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/tail-wolf-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-wolf-back.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/tail-wolf-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-wolf-front.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-bandana.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-bandana.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-tricorn.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-tricorn.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-helm.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-helm.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-crown.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-crown.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-hood.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-hood.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/hat-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-circlet.png redrawn for the Stoneborn's stout body: a head shorter and broader, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/body.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/body.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-tousled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-tousled.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-cropped.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-cropped.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-long.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-long.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-tail.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-tail.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-braid.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-braid.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hair-bald.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hair-bald.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/face-gentle.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-gentle.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/face-lashed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-lashed.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/face-stern.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-stern.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/face-freckled.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-freckled.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/face-scarred.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/face-scarred.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/beard-stubble.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-stubble.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/beard-moustache.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-moustache.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/beard-beard.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-beard.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/beard-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/beard-full.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/skirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/skirt.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/cape-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/cape-back.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/cape-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/cape-front.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/ears-pointed.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-pointed.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/ears-wolf.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-wolf.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/ears-hawk.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-hawk.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/ears-bear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/ears-bear.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/horns-curl.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/horns-curl.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/horns-swept.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/horns-swept.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/tail-ember-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-ember-back.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/tail-ember-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-ember-front.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/tail-wolf-back.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-wolf-back.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/tail-wolf-front.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/tail-wolf-front.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-bandana.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-bandana.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-tricorn.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-tricorn.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-helm.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-helm.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-crown.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-crown.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-hood.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-hood.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/hat-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/hat-circlet.png redrawn for the Hearthling's small body, rows taken out of the torso and legs by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-padded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a quilted padded jacket over the body and sleeves in the vest's key shades, a stitched line every third row and a high collar; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a leather jerkin in leather's key shades, laced up the front, with shoulder caps and a belt with a brass buckle; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-studded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the leather jerkin with steel rivets in a diamond pattern; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-hide.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a hide jerkin in leather's key shades with fur round the collar and a ragged fur hem; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-chain-shirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a mail shirt in metal's key shades, to the elbows and just past the hips; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-scale.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: scale armour in metal's key shades, rows of scales each set over by one; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-chain.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a belted mail hauberk in metal's key shades, to the wrists and the knees; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-plate.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: plate in metal's key shades: a ridged breastplate with a glint, pauldrons, mail at the upper arm, vambraces, tassets over the hips, brass at the neck and wrists; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/armour-robe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a robe to the ankles in the vest's key shades, gold at the hem, the cuffs and down the front, and a sash; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/helm-cap.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a leather skullcap with a darker band, a seam and a button; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/helm-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a great helm in metal's key shades that covers the whole head, a visor slot across the eyes and down the middle, and a brass rim; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/helm-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a slim silver circlet in metal's key shades with a violet stone on the brow; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/gloves.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: leather gloves with a cuff over the wrist; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/gauntlets.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: steel gauntlets over the hands and wrists, a glint on the knuckles; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/boots-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: tall leather boots to the middle of the calf with a turned-down cuff; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/boots-iron.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: iron greaves from the knee, a strap at the ankle and plated feet; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py onto the youth's frames and posed for the fights like the rest; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/shield-round.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a round shield of wooden planks with a steel rim and boss on the arm without the weapon: its face from the front, its back and strap from behind, its edge out in front from the side; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/shield-heater.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a heater shield, its face in the vest's key shades with a steel rim and a brass stud, on the arm without the weapon; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-dagger.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a dagger in the weapon hand, lifted back over the shoulder winding up and driven straight in; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-shortsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a shortsword in the weapon hand, lifted over the shoulder winding up and brought down through striking; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-longsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a longsword with a brass guard in the weapon hand, lifted and brought down; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-rapier.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a rapier, long and thin with a brass guard, lifted back and driven straight in; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-scimitar.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a scimitar curving back near the tip, lifted and brought down; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-axe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: an axe on a wooden haft, its broad head out in front with a bright edge; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-greatsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a greatsword three pixels broad, with a long grip and a wide guard; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-hammer.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a warhammer, a steel block on a wooden haft; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-mace.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a mace, a flanged steel ball on an iron haft; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-staff.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a staff with a violet crystal at its top: held upright, swung from its foot, and held up high to cast; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-spear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a spear: upright, levelled behind the head winding up, and thrust out at its full reach; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-bow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a bow, carried in the other hand; winding up an arrow's taken from the quiver, and striking it's drawn to the full with the arrow on the string; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/weapon-crossbow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: a light crossbow, carried in the other hand; winding up a bolt's taken from the quiver, and striking it's levelled and loosed; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_paperdoll.py on each frame where the hand holding it is; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/gear-palettes.png - created by Caleb Kiatoukaysi for Legend Quest, the gear's tints, one ramp a row: metal's keys and then iron, blackened steel, bronze and gold, and leather's keys and then dark, black, tan and red leather, written by tools/generate_paperdoll.py, released under public domain

Content/Paperdoll/Stout/armour-padded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-padded.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-leather.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-studded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-studded.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-hide.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-hide.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-chain-shirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-chain-shirt.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-scale.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-scale.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-chain.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-chain.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-plate.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-plate.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/armour-robe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-robe.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/helm-cap.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-cap.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/helm-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-full.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/helm-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-circlet.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/gloves.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/gloves.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/gauntlets.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/gauntlets.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/boots-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/boots-leather.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/boots-iron.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/boots-iron.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/shield-round.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/shield-round.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/shield-heater.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/shield-heater.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-dagger.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-dagger.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-shortsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-shortsword.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-longsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-longsword.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-rapier.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-rapier.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-scimitar.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-scimitar.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-axe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-axe.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-greatsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-greatsword.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-hammer.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-hammer.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-mace.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-mace.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-staff.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-staff.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-spear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-spear.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-bow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-bow.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Stout/weapon-crossbow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-crossbow.png drawn again for the Stoneborn's stout body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-padded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-padded.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-leather.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-studded.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-studded.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-hide.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-hide.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-chain-shirt.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-chain-shirt.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-scale.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-scale.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-chain.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-chain.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-plate.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-plate.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/armour-robe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/armour-robe.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/helm-cap.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-cap.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/helm-full.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-full.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/helm-circlet.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/helm-circlet.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/gloves.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/gloves.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/gauntlets.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/gauntlets.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/boots-leather.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/boots-leather.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/boots-iron.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/boots-iron.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/shield-round.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/shield-round.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/shield-heater.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/shield-heater.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-dagger.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-dagger.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-shortsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-shortsword.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-longsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-longsword.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-rapier.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-rapier.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-scimitar.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-scimitar.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-axe.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-axe.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-greatsword.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-greatsword.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-hammer.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-hammer.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-mace.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-mace.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-staff.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-staff.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-spear.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-spear.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-bow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-bow.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paperdoll/Small/weapon-crossbow.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: Content/Paperdoll/weapon-crossbow.png drawn again for the Hearthling's small body by tools/generate_paperdoll.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Creatures/sea-wraith.png - created by Caleb Kiatoukaysi for Legend Quest, a sea-wraith, a drowned thing gone sea-green and half see-through, kelp for hair, empty sockets with a cold light in them and rags trailing off where its legs were; it drifts rather than walks, lunges claws first, flickers when it's hurt and comes apart into a puddle when it's down; 32x48 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/gloomfang.png - created by Caleb Kiatoukaysi for Legend Quest, the gloomfang, a lean near-black wolf of the Mirewood, violet eyes with black smoke leaking up off them; standing, four steps, wind-up, strike, howl, hurt and downed each way, 48x32 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/thornmother.png - created by Caleb Kiatoukaysi for Legend Quest, the Thornmother, a huge corrupted she-bear with brambles grown through her fur, black crystal out of her shoulders and haunch and violet veins under her coat; standing, four steps, rearing, slam, roar, hurt and downed each way, 96x80 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/hollowed-footman.png - created by Caleb Kiatoukaysi for Legend Quest, a Hollowed footman, ash-grey with empty pale eyes, in cracked Doomward plate and a crimson tabard with a kettle helm and a short sword; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/hollowed-hexer.png - created by Caleb Kiatoukaysi for Legend Quest, a Hollowed hexer in a faded scholar's robe and a deep hood, a staff of black crystal in its hand; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/hollowed-sapper.png - created by Caleb Kiatoukaysi for Legend Quest, a stooping Hollowed sapper in a leather cap and jerkin with one cracked pauldron, a pick over its shoulder and a lit lantern; standing, four steps and the five fighting frames each way, 32x48 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

Content/Creatures/vault-construct.png - created by Caleb Kiatoukaysi for Legend Quest, a vault construct, a guardian of dressed stone banded with gold, the Old Crown's points on its helm and a gold sun in its chest; standing, four steps and the five fighting frames each way, 48x64 frames, drawn procedurally by tools/generate_creatures.py, released under public domain

<!-- character parts: written by tools/import_parts.py; don't edit between these two lines -->
## Characters (parts)

The characters' parts made in ChatGPT from Story/13-art-prompts.md, each an edit of one of the two bases (tools/source/characters/base-male.png and base-female.png), cut by `tools/import_parts.py` from the pictures it keeps in tools/source/characters: keyed off the black, a base split at the jaw seam into its body and its bald head, a face cut to its head and the hair below its jaw, shrunk to half size and packed into atlas pages. The game puts each character together from their recipe at the base's sockets (Characters/Parts).

tools/import_parts.py - written by Caleb Kiatoukaysi for Legend Quest, cuts the parts into layers at the bases' sockets, packs them into atlas pages and writes parts.json

tools/vectorize.py - written by Caleb Kiatoukaysi for Legend Quest, redraws a picture as vector art (flat cel colours, smooth lines, Bezier outlines) and draws it back at any size; the importer runs the parts through it when VECTOR is on

Content/Characters/Parts/parts.json - the sockets of each base, where every part is on its page and which socket it hangs from, and every recipe from Story/13-art-prompts.md §7; written by tools/import_parts.py

Content/Characters/Parts/armor-0.png - an atlas page of 11 parts (armor-champion, armor-courier, armor-elder-coat, armor-leather, armor-leon, armor-noble, armor-road, armor-scale, armor-smith, armor-studded, armor-villager-man) and 11 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/armor-1.png - an atlas page of 12 parts (armor-busker, armor-chain-mail, armor-chain-shirt, armor-crane, armor-hide, armor-mage-robes, armor-merchant, armor-padded, armor-porter, armor-refugee-man, armor-watch, armor-watch-captain) and 11 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/armor-2.png - an atlas page of 12 parts (armor-brannock, armor-child-cove, armor-clerk, armor-fisher, armor-grell, armor-kael, armor-plate, armor-red-gull, armor-royal-guard, armor-royal-livery, armor-sera, armor-urchin) and 12 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/armor-3.png - an atlas page of 12 parts (armor-elara, armor-elara-disguise, armor-elder-woman, armor-glassblower, armor-mira, armor-odelle, armor-sailor, armor-sb-traveller, armor-temple, armor-traveller, armor-traveller-f, armor-villager-woman) and 12 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/armor-4.png - an atlas page of 12 parts (armor-chain-mail-f, armor-chain-shirt-f, armor-corra, armor-hide-f, armor-maid, armor-padded-f, armor-pilgrim, armor-plate-f, armor-refugee-child, armor-refugee-woman, armor-studded-f, armor-tavern) and 13 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/armor-5.png - an atlas page of 9 parts (armor-baker, armor-champion-f, armor-fencer, armor-leather-f, armor-lodge-ranger, armor-mage-robes-f, armor-noble-gown, armor-scale-f, armor-trader) and 9 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/base-0.png - an atlas page of 4 parts (base-female.body, base-female.head, base-male.body, base-male.head) and 4 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/face-0.png - an atlas page of 78 parts (face-anselle, face-anvil-barkeep, face-brannock, face-brenna, face-brisk, face-corra, face-crane, face-elara, face-fennick, face-gannet, face-grell, face-guild-apprentice, face-hale, face-hedda, face-hesper, face-hester, face-hollowmere-farmer, face-ide, face-ilsa, face-imra-vell, face-isadora, face-kael, face-lark, face-leon, face-lookout, face-mags, face-maren-elder, face-maud, face-mc-child, face-mira, face-mira-young, face-netty, face-odelle, face-orla, face-osric, face-oswin, face-pip, face-player-1, face-player-2, face-player-3, face-player-4, face-player-5, face-player-6, face-player-7, face-player-8, face-red-gull, face-red-gull-2, face-red-gull-3, face-refugee-1, face-refugee-2, face-refugee-3, face-refugee-child, face-sb-child, face-sera, face-tamsin, face-thessaly, face-tomas, face-town-1, face-town-10, face-town-11, face-town-12, face-town-13, face-town-14, face-town-15, face-town-16, face-town-18, face-town-2, face-town-3, face-town-4, face-town-5, face-town-6, face-town-7, face-town-8, face-town-9, face-valencia, face-valencia-child, face-vreya, face-wren) and 78 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

Content/Characters/Parts/helm-0.png - an atlas page of 78 parts (hair-anselle, hair-anvil-barkeep, hair-brannock, hair-brenna, hair-brisk, hair-corra, hair-crane, hair-elara, hair-fennick, hair-gannet, hair-grell, hair-guild-apprentice, hair-hale, hair-hedda, hair-hesper, hair-hester, hair-hollowmere-farmer, hair-ide, hair-ilsa, hair-imra-vell, hair-isadora, hair-kael, hair-lark, hair-leon, hair-lookout, hair-mags, hair-maren-elder, hair-maud, hair-mc-child, hair-mira, hair-mira-young, hair-netty, hair-odelle, hair-orla, hair-osric, hair-oswin, hair-pip, hair-player-1, hair-player-2, hair-player-3, hair-player-4, hair-player-5, hair-player-6, hair-player-7, hair-player-8, hair-red-gull, hair-red-gull-2, hair-red-gull-3, hair-refugee-1, hair-refugee-2, hair-refugee-3, hair-refugee-child, hair-sb-child, hair-sera, hair-tamsin, hair-thessaly, hair-tomas, hair-town-1, hair-town-10, hair-town-11, hair-town-12, hair-town-13, hair-town-14, hair-town-15, hair-town-16, hair-town-18, hair-town-2, hair-town-3, hair-town-4, hair-town-5, hair-town-6, hair-town-7, hair-town-8, hair-town-9, hair-valencia, hair-valencia-child, hair-vreya, hair-wren) and 78 masks, cut from tools/source/characters; supplied by Caleb Kiatoukaysi, made in ChatGPT; cut by tools/import_parts.py

<!-- end character parts -->

<!-- part sheets: written by tools/part_sheet.py; don't edit between these two lines -->
## Part sheets

Several parts at a time: ChatGPT paints one face, hairstyle, outfit, helm, cape or weapon into each cell of a sheet made from a base (Story/13-art-prompts.md §4.0, §4.9, §5.0 and §6.0), and `tools/part_sheet.py` puts each cell back on its base's canvas.

tools/part_sheet.py - written by Caleb Kiatoukaysi for Legend Quest, makes the blank face, hair, armor, helm, cape and weapon sheets from the bases and cuts a painted sheet back into one picture per part, lined up with its base

tools/source/characters/face-sheet-female.png - the blank face sheet: eight copies of its head and shoulders at full size, four across and two down, from tools/source/characters/base-female-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/face-sheet-male.png - the blank face sheet: eight copies of its head and shoulders at full size, four across and two down, from tools/source/characters/base-male-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/hair-sheet-female.png - the blank hair sheet: four copies of it from the head to the hips at full size, side by side, from tools/source/characters/base-female-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/hair-sheet-male.png - the blank hair sheet: four copies of it from the head to the hips at full size, side by side, from tools/source/characters/base-male-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/armor-sheet-female.png - the blank armor sheet: four copies of the whole figure at 0.625 scale, side by side, from tools/source/characters/base-female.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/armor-sheet-male.png - the blank armor sheet: four copies of the whole figure at 0.625 scale, side by side, from tools/source/characters/base-male.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/helm-sheet-female.png - the blank helm sheet: four copies of it from the top of the canvas, room for a crown, to the hips, at full size, side by side, from tools/source/characters/base-female-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/helm-sheet-male.png - the blank helm sheet: four copies of it from the top of the canvas, room for a crown, to the hips, at full size, side by side, from tools/source/characters/base-male-blank.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/cape-sheet-female.png - the blank cape sheet: four copies of the whole figure at 0.56 scale, side by side, from tools/source/characters/base-female.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/cape-sheet-male.png - the blank cape sheet: four copies of the whole figure at 0.56 scale, side by side, from tools/source/characters/base-male.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/weapon-sheet-female.png - the blank weapon sheet: four copies of the whole figure at 0.56 scale, side by side, from tools/source/characters/base-female.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/weapon-sheet-male.png - the blank weapon sheet: four copies of the whole figure at 0.56 scale, side by side, from tools/source/characters/base-male.png, with grey lines between them; built by tools/part_sheet.py

tools/source/characters/sheets/armor-sheet-f01.png - armor sheet armor-sheet-f01 (armor-traveller-f, armor-sb-traveller, armor-sera, armor-mira), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f02.png - armor sheet armor-sheet-f02 (armor-corra, armor-odelle, armor-refugee-woman, armor-refugee-child), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f03.png - armor sheet armor-sheet-f03 (armor-glassblower, armor-elara, armor-elara-disguise, armor-tavern), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f04.png - armor sheet armor-sheet-f04 (armor-villager-woman, armor-elder-woman, armor-temple, armor-fencer), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f05.png - armor sheet armor-sheet-f05 (armor-lodge-ranger, armor-noble-gown, armor-trader, armor-baker), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f06.png - armor sheet armor-sheet-f06 (armor-pilgrim, armor-maid, armor-padded-f, armor-leather-f), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f07.png - armor sheet armor-sheet-f07 (armor-studded-f, armor-hide-f, armor-chain-shirt-f, armor-chain-mail-f), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-f08.png - armor sheet armor-sheet-f08 (armor-scale-f, armor-plate-f, armor-mage-robes-f, armor-champion-f), painted on tools/source/characters/armor-sheet-female.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m01.png - armor sheet armor-sheet-m01 (armor-traveller, armor-child-cove, armor-kael, armor-brannock), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m02.png - armor sheet armor-sheet-m02 (armor-sailor, armor-fisher, armor-red-gull, armor-grell), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m03.png - armor sheet armor-sheet-m03 (armor-watch, armor-watch-captain, armor-clerk, armor-refugee-man), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m04.png - armor sheet armor-sheet-m04 (armor-leon, armor-crane, armor-royal-livery, armor-royal-guard), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m05.png - armor sheet armor-sheet-m05 (armor-villager-man, armor-merchant, armor-busker, armor-urchin), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m06.png - armor sheet armor-sheet-m06 (armor-noble, armor-elder-coat, armor-smith, armor-road), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m07.png - armor sheet armor-sheet-m07 (armor-courier, armor-porter, armor-padded, armor-leather), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m08.png - armor sheet armor-sheet-m08 (armor-studded, armor-hide, armor-chain-shirt, armor-chain-mail), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/armor-sheet-m09.png - armor sheet armor-sheet-m09 (armor-scale, armor-plate, armor-mage-robes, armor-champion), painted on tools/source/characters/armor-sheet-male.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-traveller-f.png - armor-traveller-f on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-sb-traveller.png - armor-sb-traveller on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-sera.png - armor-sera on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-mira.png - armor-mira on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-corra.png - armor-corra on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-odelle.png - armor-odelle on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-refugee-woman.png - armor-refugee-woman on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-refugee-child.png - armor-refugee-child on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-glassblower.png - armor-glassblower on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-elara.png - armor-elara on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-elara-disguise.png - armor-elara-disguise on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-tavern.png - armor-tavern on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-villager-woman.png - armor-villager-woman on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-elder-woman.png - armor-elder-woman on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-temple.png - armor-temple on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-fencer.png - armor-fencer on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-lodge-ranger.png - armor-lodge-ranger on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-noble-gown.png - armor-noble-gown on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-trader.png - armor-trader on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-baker.png - armor-baker on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-pilgrim.png - armor-pilgrim on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-maid.png - armor-maid on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-padded-f.png - armor-padded-f on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-leather-f.png - armor-leather-f on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-studded-f.png - armor-studded-f on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-hide-f.png - armor-hide-f on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-chain-shirt-f.png - armor-chain-shirt-f on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-chain-mail-f.png - armor-chain-mail-f on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-scale-f.png - armor-scale-f on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-f08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-plate-f.png - armor-plate-f on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-f08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-mage-robes-f.png - armor-mage-robes-f on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-f08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-champion-f.png - armor-champion-f on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-f08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-traveller.png - armor-traveller on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-child-cove.png - armor-child-cove on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-kael.png - armor-kael on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-brannock.png - armor-brannock on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m01.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-sailor.png - armor-sailor on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-fisher.png - armor-fisher on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-red-gull.png - armor-red-gull on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-grell.png - armor-grell on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m02.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-watch.png - armor-watch on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-watch-captain.png - armor-watch-captain on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-clerk.png - armor-clerk on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-refugee-man.png - armor-refugee-man on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m03.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-leon.png - armor-leon on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-crane.png - armor-crane on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-royal-livery.png - armor-royal-livery on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-royal-guard.png - armor-royal-guard on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m04.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-villager-man.png - armor-villager-man on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-merchant.png - armor-merchant on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-busker.png - armor-busker on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-urchin.png - armor-urchin on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m05.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-noble.png - armor-noble on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-elder-coat.png - armor-elder-coat on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-smith.png - armor-smith on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-road.png - armor-road on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m06.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-courier.png - armor-courier on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-porter.png - armor-porter on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-padded.png - armor-padded on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-leather.png - armor-leather on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m07.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-studded.png - armor-studded on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-hide.png - armor-hide on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-chain-shirt.png - armor-chain-shirt on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-chain-mail.png - armor-chain-mail on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m08.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-scale.png - armor-scale on its base's canvas, cut by tools/part_sheet.py from cell 1 of tools/source/characters/sheets/armor-sheet-m09.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-plate.png - armor-plate on its base's canvas, cut by tools/part_sheet.py from cell 2 of tools/source/characters/sheets/armor-sheet-m09.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-mage-robes.png - armor-mage-robes on its base's canvas, cut by tools/part_sheet.py from cell 3 of tools/source/characters/sheets/armor-sheet-m09.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/armors/armor-champion.png - armor-champion on its base's canvas, cut by tools/part_sheet.py from cell 4 of tools/source/characters/sheets/armor-sheet-m09.png; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f01.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f02.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f03.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f04.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f05.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-f06.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-m01.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-m02.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-m03.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/sheets/head-and-hair/face-sheet-m04.png - a face sheet painted the old way, eight heads with their hair on the face sheet as it was then; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-anselle.png - face-anselle's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-anvil-barkeep.png - face-anvil-barkeep's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-brannock.png - face-brannock's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-brenna.png - face-brenna's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-brisk.png - face-brisk's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-corra.png - face-corra's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-crane.png - face-crane's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-elara.png - face-elara's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-fennick.png - face-fennick's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-gannet.png - face-gannet's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-grell.png - face-grell's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-guild-apprentice.png - face-guild-apprentice's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-hale.png - face-hale's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-hedda.png - face-hedda's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-hesper.png - face-hesper's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-hester.png - face-hester's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-hollowmere-farmer.png - face-hollowmere-farmer's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-ide.png - face-ide's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-ilsa.png - face-ilsa's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-imra-vell.png - face-imra-vell's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-isadora.png - face-isadora's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-kael.png - face-kael's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-lark.png - face-lark's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-leon.png - face-leon's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-lookout.png - face-lookout's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-mags.png - face-mags's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-maren-elder.png - face-maren-elder's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-maud.png - face-maud's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-mc-child.png - face-mc-child's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-mira-young.png - face-mira-young's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-mira.png - face-mira's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-netty.png - face-netty's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-odelle.png - face-odelle's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-orla.png - face-orla's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-osric.png - face-osric's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-oswin.png - face-oswin's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-pip.png - face-pip's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-1.png - face-player-1's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-2.png - face-player-2's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-3.png - face-player-3's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-4.png - face-player-4's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-5.png - face-player-5's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-6.png - face-player-6's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-7.png - face-player-7's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-player-8.png - face-player-8's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-red-gull-2.png - face-red-gull-2's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-red-gull-3.png - face-red-gull-3's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-red-gull.png - face-red-gull's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-refugee-1.png - face-refugee-1's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-refugee-2.png - face-refugee-2's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-refugee-3.png - face-refugee-3's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-refugee-child.png - face-refugee-child's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-sb-child.png - face-sb-child's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-sera.png - face-sera's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-tamsin.png - face-tamsin's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-thessaly.png - face-thessaly's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-tomas.png - face-tomas's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-1.png - face-town-1's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-10.png - face-town-10's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-11.png - face-town-11's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-12.png - face-town-12's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-13.png - face-town-13's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-14.png - face-town-14's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-15.png - face-town-15's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-16.png - face-town-16's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-18.png - face-town-18's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-2.png - face-town-2's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-3.png - face-town-3's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-4.png - face-town-4's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-5.png - face-town-5's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-6.png - face-town-6's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-7.png - face-town-7's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-8.png - face-town-8's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-town-9.png - face-town-9's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-valencia-child.png - face-valencia-child's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-valencia.png - face-valencia's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-vreya.png - face-vreya's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

tools/source/characters/faces/head-and-hair/face-wren.png - face-wren's head and hair on its base's canvas, painted the old way and cut by tools/part_sheet.py from a sheet in tools/source/characters/sheets/head-and-hair; tools/import_parts.py splits it into its face and its hair; supplied by Caleb Kiatoukaysi, made in ChatGPT

<!-- end part sheets -->

## Tiles and props

Content/Tiles/questeria.png - created by Caleb Kiatoukaysi for Legend Quest, Questeria's and the harbour's floor tiles (plaza, terrace, cobble, grass, sand, water, planks) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/ship.png - created by Caleb Kiatoukaysi for Legend Quest, the ship you arrive on, side on with its cabin at the stern, its sail full and then furled, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/gangplank.png - created by Caleb Kiatoukaysi for Legend Quest, the ship's gangplank down to the boardwalk, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/rowboat.png - created by Caleb Kiatoukaysi for Legend Quest, a rowing boat moored by the jetty, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Tiles/crypt.png - created by Caleb Kiatoukaysi for Legend Quest, the barrow's floor tiles (floor, floor worn, floor shadow, steps, wall top, wall face, wall niche, wall carving, wall base, rubble, bones, cobweb, cobweb ne) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Tiles/forest.png - created by Caleb Kiatoukaysi for Legend Quest, the old road's, under the trees, floor tiles (grass, leaves, road, undergrowth, moss, tuft, fern, flowers, pebbles, roots, ruts, litter) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/barrel.png - built by tools/generate_world.py from tools/source/questeria-props-2.png: a barrel, a prop, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crate.png - built by tools/generate_world.py from tools/source/questeria-props-2.png: a crate, a prop, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/urn.png - built by tools/generate_world.py from tools/source/questeria-props-2.png: a funerary urn in the barrow, a prop, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/sarcophagus.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sconce.png - created by Caleb Kiatoukaysi for Legend Quest, the iron sconce a torch stands in, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/campfire.png - created by Caleb Kiatoukaysi for Legend Quest, a ring of stones round two burning logs, a prop (the game draws the flame on it), drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/bedroll.png - created by Caleb Kiatoukaysi for Legend Quest, a blanket laid out by the campfire, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/chest.png - built by tools/generate_world.py from tools/source/questeria-props-2.png: a chest, shut and open, a prop, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/door.png - created by Caleb Kiatoukaysi for Legend Quest, a double door in its stone frame, shut and open, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/gate.png - created by Caleb Kiatoukaysi for Legend Quest, a stone door in its frame, shut and sunk into the floor, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/lever.png - created by Caleb Kiatoukaysi for Legend Quest, a lever on a wall plate, up and down, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sign.png - created by Caleb Kiatoukaysi for Legend Quest, a fingerpost, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/waypoint.png - created by Caleb Kiatoukaysi for Legend Quest, a waypoint stone with its mark dark and then lit, drawn procedurally by tools/generate_world.py, released under public domain

Content/Tiles/ship.png - created by Caleb Kiatoukaysi for Legend Quest, the Gull's Wager's deck's floor tiles (sea, deck, rail n, rail s, hull, hatch, bow, stern, bowsprit, lantern) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/mast.png - created by Caleb Kiatoukaysi for Legend Quest, the Gull's Wager's mainmast, its sail full and its crow's nest at the top, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/lantern-hanging.png - created by Caleb Kiatoukaysi for Legend Quest, a ship's lantern on a post, a prop that gives off light, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/ship-wheel.png - created by Caleb Kiatoukaysi for Legend Quest, the ship's wheel, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/rope-coil.png - created by Caleb Kiatoukaysi for Legend Quest, a coil of rope on the deck, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Tiles/interior.png - created by Caleb Kiatoukaysi for Legend Quest, the floors inside Questeria's buildings, floor tiles (floor wood, floor stone, rug, wall top, wall face, wall base) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/bar-counter.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: the Gilded Flagon's bar: the market stall's counter twice end to end, a prop put together from parts of the sheet, sampled at 3 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/round-table.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a round tavern table, a prop, keyed off its backdrop and sampled to 24 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stool.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a stool, a prop, keyed off its backdrop and sampled to 13 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/hearth.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a stone oven-hearth with its fire, giving off light, a prop, keyed off its backdrop and sampled to 44 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/keg-stack.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: kegs, one behind two, a prop put together from parts of the sheet, sampled at 3 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bed.png - unchanged entry; regenerated by tools/generate_interiors.py (its outline colour comes from tools/generate_harbour.py's OUTLINE, which the harbour agent changed)

Content/Props/wardrobe.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a cupboard of jars with flowers on top, a prop, keyed off its backdrop and sampled to 29 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bookshelf.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a shelf of jars and bottles, a prop, keyed off its backdrop and sampled to 44 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/shop-counter.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a shop counter, the market stall's counter with pots and a cloth on it, a prop, keyed off its backdrop and sampled to 60 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/candle-stand.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a candelabra with three candles, giving off light, a prop, keyed off its backdrop and sampled to 18 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/questeria-dawn-sky.png - built by tools/generate_paintings.py from tools/source/title-sky.png: its sky, warmed to sunrise, the sun coming up over the mountains and the stars and moons fading; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/questeria-dawn-city.png - built by tools/generate_paintings.py from tools/source/title-city.png: the title's city and its lake, warmed to sunrise; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/questeria-dawn-rail.png - built by tools/generate_paintings.py from tools/source/questeria-props-2.png: the ship's rail in front of it all (varnished wood, brass caps, a rope coil and a pennant, drawn here) with the lantern off my props sheet; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/red-moon-sky.png - built by tools/generate_paintings.py from tools/source/title-sky.png: its sky, recoloured to a crimson night; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/red-moon-moon.png - built by tools/generate_paintings.py from tools/source/title-sky.png: its big moon, turned blood red, with a halo; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/pier-memory-sky.png - built by tools/generate_paintings.py from tools/source/title-sky.png and title-clouds.png: the storm the day you met, the sky and its clouds recoloured slate; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/pier-memory-sea.png - built by tools/generate_paintings.py from tools/source/title-city.png: the rough sea, its lake's water recoloured with the waves, the capsized boat and the coast drawn over it; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/pier-memory-pier.png - built by tools/generate_paintings.py from tools/source/npc-villagers.png: the pier in the storm, drawn here, and a child off my villagers sheet running down it; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/questeria-dawn-clouds.png - built by tools/generate_paintings.py from tools/source/title-clouds.png: two of its clouds, lit gold by the dawn; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/questeria-dawn-gulls.png - created by Caleb Kiatoukaysi for Legend Quest, three gulls over the sea at dawn, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/red-moon-shore.png - built by tools/generate_paintings.py from tools/source/title-far.png: the mountains and the lake at night under the red moon, with its reflection and mist; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Paintings/red-moon-headland.png - created by Caleb Kiatoukaysi for Legend Quest, the headland the two of them stand on, rocks and pines as silhouettes against the moon, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/weather.png - created by Caleb Kiatoukaysi for Legend Quest, a white flash and the near and far raindrops for the storm, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/bolts.png - created by Caleb Kiatoukaysi for Legend Quest, two lightning bolts, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/sigil.png - created by Caleb Kiatoukaysi for Legend Quest, the Soul-Bond's glow and its mark, between the two of them under the red moon, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/glint.png - created by Caleb Kiatoukaysi for Legend Quest, three frames of a glint on the water, drawn procedurally by tools/generate_paintings.py, released under public domain

Content/Paintings/lantern.png - built by tools/generate_paintings.py from tools/source/questeria-props-2.png: the pier's lantern, five frames of it swinging; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Tiles/keep.png - created by Caleb Kiatoukaysi for Legend Quest, Sunspire Keep's floor tiles (marble, marble gold, carpet, dais, steps, wall top, wall high, wall face, wall base, window high, window) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/sun-throne.png - created by Caleb Kiatoukaysi for Legend Quest, King Leon's throne, gold and royal blue with the sun blazing behind it, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/sun-banner.png - created by Caleb Kiatoukaysi for Legend Quest, a banner of the Aurelian sun hanging on a wall, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/pillar-marble.png - created by Caleb Kiatoukaysi for Legend Quest, a marble column with a gilded capital, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/guard-statue.png - created by Caleb Kiatoukaysi for Legend Quest, a knight of the old war in marble on a plinth, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/brazier.png - created by Caleb Kiatoukaysi for Legend Quest, a gold brazier of coals on three legs, giving off light (the game draws the flame on it), a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/keep-arch.png - created by Caleb Kiatoukaysi for Legend Quest, the arch from the great hall into the throne room, its gilded doors standing open, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/light-shaft.png - created by Caleb Kiatoukaysi for Legend Quest, the sun through a pair of tall windows lying across the floor, see-through, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/sun-inlay.png - created by Caleb Kiatoukaysi for Legend Quest, the Aurelian sun laid into the floor in gold before the dais, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/war-table.png - created by Caleb Kiatoukaysi for Legend Quest, the war table with the map of Fable spread across it and the markers on it, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/royal-desk.png - created by Caleb Kiatoukaysi for Legend Quest, a gilded walnut writing desk with letters, a book and an inkwell on it, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/wine-table.png - created by Caleb Kiatoukaysi for Legend Quest, a little marble table with a decanter of wine and two goblets, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/vault-stair-shut.png - created by Caleb Kiatoukaysi for Legend Quest, the stair down to the Old Crown Vault in Questeria's terrace with its iron gate locked, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/vault-stair.png - created by Caleb Kiatoukaysi for Legend Quest, the stair down to the Old Crown Vault with its gate standing open, a prop, drawn procedurally by tools/generate_keep.py, released under public domain

Content/Props/tankard-table.png - created by Caleb Kiatoukaysi for Legend Quest, an oak tavern table with tankards on it, one knocked over, and a candle, a prop, drawn procedurally by tools/generate_interiors.py, released under public domain

Content/Props/booth.png - created by Caleb Kiatoukaysi for Legend Quest, a high-backed tavern booth with a crimson cushion, a prop, drawn procedurally by tools/generate_interiors.py, released under public domain

Content/Tiles/vault.png - created by Caleb Kiatoukaysi for Legend Quest, the Old Crown Vault's floor tiles (floor, floor shadow, floor cracked, inlay, sun crest, pressure plate, pit, earth, tunnel, steps, wall top, wall face, wall crest, wall niche, wall base, earth top, earth face, coins, rubble, debris, darts, bones, cobweb, cobweb ne) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/vault-sconce.png - created by Caleb Kiatoukaysi for Legend Quest, a gilded lamp on the wall, a prop (the game draws the flame on it), drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/vault-brazier.png - created by Caleb Kiatoukaysi for Legend Quest, a gold brazier of glowing coals on three legs, a prop (the game draws the flame on it), drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/vault-door.png - created by Caleb Kiatoukaysi for Legend Quest, the vault's great doors of blued steel and gold in their gilded portal, shut and open, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/vault-lever.png - created by Caleb Kiatoukaysi for Legend Quest, a gilded lever on a stone wall plate, up and down, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/vault-chest.png - created by Caleb Kiatoukaysi for Legend Quest, a royal strongbox bound in gold, shut, and open with its gold showing, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/gold-pile.png - created by Caleb Kiatoukaysi for Legend Quest, a heap of the crown's gold with a goblet, a sword and gems in it, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/gold-pile-small.png - created by Caleb Kiatoukaysi for Legend Quest, a small heap of gold coins, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/relic-shelf.png - created by Caleb Kiatoukaysi for Legend Quest, a cabinet of the old kings' relics, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/relic-case.png - created by Caleb Kiatoukaysi for Legend Quest, a crown on a cushion under glass, on a plinth, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/relic-case-orb.png - created by Caleb Kiatoukaysi for Legend Quest, a gold orb on a cushion under glass, on a plinth, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/dart-trap.png - created by Caleb Kiatoukaysi for Legend Quest, a dart trap in a wall, carved as the sun, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/sunheart-pedestal.png - created by Caleb Kiatoukaysi for Legend Quest, the empty pedestal where the Sunheart sat, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/sun-mosaic.png - created by Caleb Kiatoukaysi for Legend Quest, the sun of the old kings inlaid in a floor, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/rubble-heap.png - created by Caleb Kiatoukaysi for Legend Quest, a heap of broken stone and earth where a roof came down, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Props/sapper-hole.png - created by Caleb Kiatoukaysi for Legend Quest, the hole the Hollowed sappers broke up through a floor, a prop, drawn procedurally by tools/generate_vault.py, released under public domain

Content/Tiles/mirewood.png - created by Caleb Kiatoukaysi for Legend Quest, the Mirewood's floor tiles (grass, grass wilt, grass rot, leaves, leaves blackvein, gloom moss, trail, tar stream, undergrowth, undergrowth rot, cliff top, cliff face, cliff base, tuft, flowers, fern, pebbles, roots, litter, veins, tuft dead, toadstools, tar spatter, brambles, tar slick) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Tiles/quarry.png - created by Caleb Kiatoukaysi for Legend Quest, the Old Quarry's floor tiles (floor, floor shadow, black vein floor, root floor, tar seep, ramp, wall top, wall face, wall roots, wall crystal, wall base, rubble, gravel, roots, shards, fungus, tools) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/tar-pool.png - created by Caleb Kiatoukaysi for Legend Quest, a pool of tar in the Mirewood, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/gloom-mushroom.png - created by Caleb Kiatoukaysi for Legend Quest, ink caps and pale gloom mushrooms in a cluster, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/gloom-mushroom-small.png - created by Caleb Kiatoukaysi for Legend Quest, a few gloom mushrooms, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/ranger-sign.png - created by Caleb Kiatoukaysi for Legend Quest, the rangers' waymark with its antlers and green ribbon, a prop and what the Mirewood's signs look like, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/woodcutter-stump.png - created by Caleb Kiatoukaysi for Legend Quest, a felled oak's stump with the axe left in it, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/ranger-bridge.png - created by Caleb Kiatoukaysi for Legend Quest, the rangers' footbridge over the stream, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/quarry-mouth.png - created by Caleb Kiatoukaysi for Legend Quest, the timbered way into the old quarry, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/bones.png - created by Caleb Kiatoukaysi for Legend Quest, an animal's skull and bones, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/root-wall.png - created by Caleb Kiatoukaysi for Legend Quest, roots grown across a way through, tangled and then parted, what the quarry's root walls look like, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/black-crystal.png - created by Caleb Kiatoukaysi for Legend Quest, the black crystal spike, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/black-crystal-shattered.png - created by Caleb Kiatoukaysi for Legend Quest, the black crystal smashed, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/black-crystal-purified.png - created by Caleb Kiatoukaysi for Legend Quest, the black crystal made clean, white and gold, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/courier-body.png - created by Caleb Kiatoukaysi for Legend Quest, the dead Doomward courier, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Props/satchel.png - created by Caleb Kiatoukaysi for Legend Quest, the courier's satchel with his letter in it, a prop, drawn procedurally by tools/generate_mirewood.py, released under public domain

Content/Tiles/meadow.png - created by Caleb Kiatoukaysi for Legend Quest, the meadows', the old road's and Thornbridge's floor tiles (grass, long grass, path, road, soil, stream, ford, hedge, wall, flowers, clover, tuft, pebbles, ruts, reeds, lilies, crops, stepping, petals) for the room's tile strip, 64 texels a tile in the illustrated style, lit from the top left, painted procedurally by tools/rooms/paint_tiles.py with tools/rooms/paint.py, released under public domain

Content/Props/windmill.png - created by Caleb Kiatoukaysi for Legend Quest, the farm's windmill, its sails across its front, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/hay-bale.png - created by Caleb Kiatoukaysi for Legend Quest, a bale of hay, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/haystack.png - created by Caleb Kiatoukaysi for Legend Quest, a haystack with a pitchfork stuck in it, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/fence-wood.png - created by Caleb Kiatoukaysi for Legend Quest, two tiles of farm fence, a post and two split rails, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/fence-wood-post.png - created by Caleb Kiatoukaysi for Legend Quest, a farm fence's post on its own, for the end of a run, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/fence-wood-side.png - created by Caleb Kiatoukaysi for Legend Quest, a tile of farm fence running up the screen, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/stone-bridge.png - created by Caleb Kiatoukaysi for Legend Quest, the old stone bridge that takes the road over the stream, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/wheat.png - created by Caleb Kiatoukaysi for Legend Quest, two tiles of ripe wheat, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/scarecrow.png - created by Caleb Kiatoukaysi for Legend Quest, a scarecrow, with a crow on his arm, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/tent-ragged.png - created by Caleb Kiatoukaysi for Legend Quest, a patched tent in the refugees' camp, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/tent-ragged-2.png - created by Caleb Kiatoukaysi for Legend Quest, a lower patched tent with a blanket over its door, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/cookfire.png - created by Caleb Kiatoukaysi for Legend Quest, a cooking fire with a pot hung over it (the game draws the flame on it), a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/cot.png - created by Caleb Kiatoukaysi for Legend Quest, a camp bed, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/healer-station.png - created by Caleb Kiatoukaysi for Legend Quest, Mira's table of bottles, bandages and herbs, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/refugee-bundle.png - created by Caleb Kiatoukaysi for Legend Quest, a refugee's bundle, rolled blanket and pot, a prop, drawn procedurally by tools/generate_meadow.py, released under public domain

Content/Props/Painted/anvil.png, Content/Props/Painted/armchair.png, Content/Props/Painted/banner-blue.png, Content/Props/Painted/banner-pink.png, Content/Props/Painted/bar-counter.png, Content/Props/Painted/barrel-flowers.png, Content/Props/Painted/barrel-salt.png, Content/Props/Painted/barrel-water.png, Content/Props/Painted/barrel.png, Content/Props/Painted/bed.png, Content/Props/Painted/bedroll.png, Content/Props/Painted/bench.png, Content/Props/Painted/black-crystal-purified.png, Content/Props/Painted/black-crystal-shattered.png, Content/Props/Painted/black-crystal.png, Content/Props/Painted/blossom-tree.png, Content/Props/Painted/bollard.png, Content/Props/Painted/bones.png, Content/Props/Painted/book-crate.png, Content/Props/Painted/bookshelf.png, Content/Props/Painted/booth.png, Content/Props/Painted/brazier.png, Content/Props/Painted/bush-deep.png, Content/Props/Painted/bush-flowers.png, Content/Props/Painted/bush-pink.png, Content/Props/Painted/bush-small.png, Content/Props/Painted/bush.png, Content/Props/Painted/campfire.png, Content/Props/Painted/candle-stand.png, Content/Props/Painted/cargo-barrel.png, Content/Props/Painted/cargo-crate.png, Content/Props/Painted/cask.png, Content/Props/Painted/chair.png, Content/Props/Painted/chalkboard.png, Content/Props/Painted/cherry-tree-small.png, Content/Props/Painted/chest.png, Content/Props/Painted/cookfire.png, Content/Props/Painted/cot.png, Content/Props/Painted/courier-body.png, Content/Props/Painted/crate-apples.png, Content/Props/Painted/crate-onions.png, Content/Props/Painted/crate-open.png, Content/Props/Painted/crate-stack.png, Content/Props/Painted/crate.png, Content/Props/Painted/crystal-stone.png, Content/Props/Painted/cypress-pot.png, Content/Props/Painted/cypress.png, Content/Props/Painted/dart-trap.png, Content/Props/Painted/dead-tree.png, Content/Props/Painted/desk.png, Content/Props/Painted/fallen-tree.png, Content/Props/Painted/fence-wood-post.png, Content/Props/Painted/fence-wood.png, Content/Props/Painted/fern.png, Content/Props/Painted/fir-deep.png, Content/Props/Painted/fir.png, Content/Props/Painted/fish-stall.png, Content/Props/Painted/flower-cart.png, Content/Props/Painted/flowers-mixed.png, Content/Props/Painted/flowers-pink.png, Content/Props/Painted/flowers-white.png, Content/Props/Painted/fountain.png, Content/Props/Painted/gangplank.png, Content/Props/Painted/gloom-mushroom-small.png, Content/Props/Painted/gloom-mushroom.png, Content/Props/Painted/gold-pile-small.png, Content/Props/Painted/gold-pile.png, Content/Props/Painted/grass-tall.png, Content/Props/Painted/guard-statue.png, Content/Props/Painted/handcart.png, Content/Props/Painted/harbour-bell.png, Content/Props/Painted/hay-bale.png, Content/Props/Painted/hearth.png, Content/Props/Painted/keep-arch.png, Content/Props/Painted/keg-stack.png, Content/Props/Painted/lamp-arm.png, Content/Props/Painted/lamp-double.png, Content/Props/Painted/lamp.png, Content/Props/Painted/lantern-hanging.png, Content/Props/Painted/lantern-post.png, Content/Props/Painted/lantern-small.png, Content/Props/Painted/lever.png, Content/Props/Painted/log.png, Content/Props/Painted/mast.png, Content/Props/Painted/mushrooms-pale.png, Content/Props/Painted/mushrooms.png, Content/Props/Painted/net-rack.png, Content/Props/Painted/notice-board.png, Content/Props/Painted/oak-deep.png, Content/Props/Painted/oak-light.png, Content/Props/Painted/oak-pale.png, Content/Props/Painted/oak-turning.png, Content/Props/Painted/oak.png, Content/Props/Painted/pillar-marble.png, Content/Props/Painted/planter.png, Content/Props/Painted/potted-plant.png, Content/Props/Painted/ranger-sign.png, Content/Props/Painted/refugee-bundle.png, Content/Props/Painted/relic-case-orb.png, Content/Props/Painted/relic-case.png, Content/Props/Painted/relic-shelf.png, Content/Props/Painted/rocks-mossy.png, Content/Props/Painted/rocks.png, Content/Props/Painted/root-wall.png, Content/Props/Painted/rope-coil.png, Content/Props/Painted/rope-posts.png, Content/Props/Painted/round-table.png, Content/Props/Painted/royal-desk.png, Content/Props/Painted/rubble-heap.png, Content/Props/Painted/sack-pile.png, Content/Props/Painted/sacks.png, Content/Props/Painted/sapper-hole.png, Content/Props/Painted/satchel.png, Content/Props/Painted/sconce.png, Content/Props/Painted/ship-wheel.png, Content/Props/Painted/ship.png, Content/Props/Painted/shop-counter.png, Content/Props/Painted/side-table.png, Content/Props/Painted/sign.png, Content/Props/Painted/signpost-arrows.png, Content/Props/Painted/signpost.png, Content/Props/Painted/snag.png, Content/Props/Painted/stall-cloth.png, Content/Props/Painted/stall-teal.png, Content/Props/Painted/statue.png, Content/Props/Painted/stool.png, Content/Props/Painted/stump.png, Content/Props/Painted/sun-banner.png, Content/Props/Painted/sun-mosaic.png, Content/Props/Painted/sun-throne.png, Content/Props/Painted/sunheart-pedestal.png, Content/Props/Painted/table.png, Content/Props/Painted/tankard-table.png, Content/Props/Painted/tapestry.png, Content/Props/Painted/tar-pool.png, Content/Props/Painted/tool-rack.png, Content/Props/Painted/topiary.png, Content/Props/Painted/tree-blackvein-bare.png, Content/Props/Painted/tree-blackvein-pale.png, Content/Props/Painted/tree-blackvein.png, Content/Props/Painted/urn-blue.png, Content/Props/Painted/urn.png, Content/Props/Painted/utensil-rack.png, Content/Props/Painted/vault-brazier.png, Content/Props/Painted/vault-chest.png, Content/Props/Painted/vault-lever.png, Content/Props/Painted/vault-sconce.png, Content/Props/Painted/vault-stair-shut.png, Content/Props/Painted/vault-stair.png, Content/Props/Painted/war-table.png, Content/Props/Painted/wardrobe.png, Content/Props/Painted/washing-line.png, Content/Props/Painted/waypoint.png, Content/Props/Painted/wildflowers-blue.png, Content/Props/Painted/wildflowers-yellow.png, Content/Props/Painted/wildflowers.png, Content/Props/Painted/wine-table.png, Content/Props/Painted/woodcutter-stump.png - created by Caleb Kiatoukaysi for Legend Quest, the floor props in the illustrated style, each the prop of the same name in Content/Props repainted at four times its size with the same frames, foot and footprint: trees, bushes, flowers and plants; boxes, barrels, sacks and bundles; furniture; lamps, torches and candles (the game burns their flames); banners, boards and signs; the docks' and the ship's pieces; market stalls and carts; stone, statues, the fountain and the vaults' treasures, painted procedurally by tools/rooms/paint_props.py with tools/rooms/paint.py, released under public domain

## UI

Content/button.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron plate every button is drawn with and the light in its groove, drawn procedurally by tools/generate_ui.py, released under public domain

Content/panel.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron panel the forms sit on, drawn procedurally by tools/generate_ui.py, released under public domain

Content/map-marks.png - created by Caleb Kiatoukaysi for Legend Quest, the marks on the map screen (places, where you are, waypoints, roads), drawn procedurally by tools/generate_ui.py, released under public domain

Content/markers.png - created by Caleb Kiatoukaysi for Legend Quest, the gold and red "!" markers over people with a quest or an event and the arrows to them at the screen's edge, drawn procedurally by tools/generate_ui.py, released under public domain

Content/dice.png - created by Caleb Kiatoukaysi for Legend Quest, the d20 for checks, eight frames tumbling and one landed, drawn procedurally by tools/generate_ui.py, released under public domain

Content/dialogue-icons.png - created by Caleb Kiatoukaysi for Legend Quest, the icons beside options in a conversation (a die, a heart, a lock, and the three voices), drawn procedurally by tools/generate_ui.py, released under public domain

Content/cursors.png - created by Caleb Kiatoukaysi for Legend Quest, the mouse cursors: an arrow, a ring to walk, a hand to use, a speech bubble to talk, a crossed circle, and a door for a room's doors, drawn procedurally by tools/generate_ui.py, released under public domain

Content/indicators.png - created by Caleb Kiatoukaysi for Legend Quest, what marks a room's ways out and the things in it: the badges over its doors (a coin purse, an anvil, a bottle, a tankard, crossed swords, a scroll, a shield, a sun, a crown, a door, a gate, and stairs up and down), each in colour and greyed for a locked one, the padlock and the compass, the chevrons at the ends of a room and on its stairs, a glint's four frames, the plaque a badge's name sits on, and the outline round a doorway under the mouse, drawn procedurally by tools/generate_ui.py, released under public domain

Content/Items/icons.png - created by Caleb Kiatoukaysi for Legend Quest, an icon for every item, 24x24, in ItemCatalog's order (the weapons, the armour by what it's made of, the shields, helms, gloves, boots and cloaks, the amulet and the ring, the potions and the rest), with a glint on the magic ones, drawn procedurally by tools/generate_items.py, released under public domain

Content/Items/slots.png - created by Caleb Kiatoukaysi for Legend Quest, the inventory's slots (empty, under the mouse and picked), the faint outline of what goes in each equipment slot, and the rims of an uncommon and a rare thing's slot, drawn procedurally by tools/generate_items.py, released under public domain

Content/Combat/abilities.png - created by Caleb Kiatoukaysi for Legend Quest, an icon for every ability in a fight, in AbilityId's order, each on a tile the colour of its kind, and the End Turn and bow attack icons, drawn procedurally by tools/generate_combat_art.py, released under public domain

Content/Combat/conditions.png - created by Caleb Kiatoukaysi for Legend Quest, an icon for every condition, in ConditionId's order, drawn procedurally by tools/generate_combat_art.py, released under public domain

Content/Combat/hud.png - created by Caleb Kiatoukaysi for Legend Quest, the pieces of the fight's screen: the hotbar's slots, the initiative portraits, the action, bonus action, reaction and Shift pips, the Focus bar, death saves, the Soul-Link sigil in its five states, the target rings, the plates at the places in the rows and the chevron over one you can Shift to, a support's banner and its progress ring, and the arrow, fire bolt, frost ray, radiant mote, eldritch blast, magic missile and flask in flight, drawn procedurally by tools/generate_combat_art.py, released under public domain

Content/keycaps.png - created by Caleb Kiatoukaysi for Legend Quest, the key cap the tutorial tips and HOW TO PLAY write keys on, and the mouse with its left button, right button and wheel lit, drawn procedurally by tools/generate_ui.py, released under public domain

## Old maps

The top-down tile maps the zones were converted from. The game doesn't read them any more;
they're tools/rooms/convert_maps.py's input, kept in tools/rooms/legacy/.

tools/rooms/legacy/questeria.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria, the capital and the hub map, released under public domain

tools/rooms/legacy/docks.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria's harbour, where a new game opens with the ship coming in, released under public domain

tools/rooms/legacy/road.map - written by Caleb Kiatoukaysi for Legend Quest, the old road through the meadows at noon: the farm, the stream and its bridge, the camp, the fork to the barrow, and the wood thickening toward the Mirewood, released under public domain

tools/rooms/legacy/barrow.map - written by Caleb Kiatoukaysi for Legend Quest, the barrow, a dark map lit by torches, released under public domain

tools/rooms/legacy/gulls-wager.map - written by Caleb Kiatoukaysi for Legend Quest, the deck of the Gull's Wager at night, where the Prologue's voyage plays, released under public domain

tools/rooms/legacy/gilded-flagon.map - written by Caleb Kiatoukaysi for Legend Quest, inside the Gilded Flagon, the Lantern Market's tavern, released under public domain

tools/rooms/legacy/cottage-a.map - written by Caleb Kiatoukaysi for Legend Quest, inside a cottage: a bed, a wardrobe, a bookshelf and a table, released under public domain

tools/rooms/legacy/cottage-b.map - written by Caleb Kiatoukaysi for Legend Quest, inside another cottage, with a hearth, released under public domain

tools/rooms/legacy/shop.map - written by Caleb Kiatoukaysi for Legend Quest, inside the chandler's shop, released under public domain

tools/rooms/legacy/warehouse.map - written by Caleb Kiatoukaysi for Legend Quest, inside a warehouse on the docks, released under public domain

tools/rooms/legacy/bakery.map - written by Caleb Kiatoukaysi for Legend Quest, inside the bakery in the Lantern Market: its counters, the shelves of loaves, the sacks of flour and the oven, released under public domain

tools/rooms/legacy/forge.map - written by Caleb Kiatoukaysi for Legend Quest, inside the forge in the Lantern Market: the anvil, the tool racks and the forge fire, released under public domain

tools/rooms/legacy/chapel.map - written by Caleb Kiatoukaysi for Legend Quest, inside the chapel by the temple: benches for pews, candles and a tapestry, released under public domain

tools/rooms/legacy/sunspire-keep.map - written by Caleb Kiatoukaysi for Legend Quest, inside Sunspire Keep: the throne room and the great hall on the way out, released under public domain

tools/rooms/legacy/rusty-anvil.map - written by Caleb Kiatoukaysi for Legend Quest, inside the Rusty Anvil, Brannock's bar on the docks, released under public domain

tools/rooms/legacy/old-crown-vault.map - written by Caleb Kiatoukaysi for Legend Quest, the Old Crown Vault under the Sunspire: the antechamber and its constructs, the trapped hall, the sappers' tunnel and the warden's room, the treasury, and the relic gallery where Kael is, a dark map lit by lamps and braziers, released under public domain

tools/rooms/legacy/mirewood.map - written by Caleb Kiatoukaysi for Legend Quest, the Mirewood, bright at its west edge and rotten toward the old quarry, at twilight, released under public domain

tools/rooms/legacy/mirewood-quarry.map - written by Caleb Kiatoukaysi for Legend Quest, the Old Quarry under the Mirewood, a dark map lit by fungus and the black crystal, down to the Thornmother's den, released under public domain

tools/rooms/legacy/thornbridge.map - written by Caleb Kiatoukaysi for Legend Quest, Thornbridge, the meadow outside Questeria's east gate, the old bridge over the river, and the refugees' camp, released under public domain

## Zones

The places, each a chain of side-on rooms (12-rooms), read by World/Zones/ZoneLoader.cs. Each was
turned out of its old map by tools/rooms/convert_maps.py, with the hand work (where the rooms split,
their doors and floors) in tools/rooms/plan.py.

Content/Zones/bakery.txt - written by Caleb Kiatoukaysi for Legend Quest, The Bakery: The Bakery, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/barrow.txt - written by Caleb Kiatoukaysi for Legend Quest, The Barrow: The Barrow Steps, The Barrow Hall, The Side Room, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/chapel.txt - written by Caleb Kiatoukaysi for Legend Quest, The Chapel: The Chapel, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/cottage-a.txt - written by Caleb Kiatoukaysi for Legend Quest, A Cottage: A Cottage, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/cottage-b.txt - written by Caleb Kiatoukaysi for Legend Quest, A Cottage: A Cottage, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/docks.txt - written by Caleb Kiatoukaysi for Legend Quest, Saltwind Docks: The Pier, The Harbourside, The Fish Market, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/forge.txt - written by Caleb Kiatoukaysi for Legend Quest, The Smithy: The Smithy, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/gilded-flagon.txt - written by Caleb Kiatoukaysi for Legend Quest, The Gilded Flagon: The Taproom, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/gulls-wager.txt - written by Caleb Kiatoukaysi for Legend Quest, The Gull's Wager: The Main Deck, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/mirewood.txt - written by Caleb Kiatoukaysi for Legend Quest, The Mirewood: The Mirewood's Edge, The Deep Wood, The Rangers' Camp, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/mirewood-quarry.txt - written by Caleb Kiatoukaysi for Legend Quest, The Old Quarry: The Quarry's Lip, The Second Cut, The Quarry Floor, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/old-crown-vault.txt - written by Caleb Kiatoukaysi for Legend Quest, The Old Crown Vault: The Antechamber, The Long Hall, The Relic Gallery, The Treasury, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/questeria.txt - written by Caleb Kiatoukaysi for Legend Quest, Questeria: The Gate Plaza, The Festival Square, The Sunspire Steps, The Lantern Market, The Thornbridge Gate, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/road.txt - written by Caleb Kiatoukaysi for Legend Quest, The Old Road: The Farmland, The Fork, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/rusty-anvil.txt - written by Caleb Kiatoukaysi for Legend Quest, The Rusty Anvil: The Bar, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/shop.txt - written by Caleb Kiatoukaysi for Legend Quest, The Chandler's: The Chandler's, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/sunspire-keep.txt - written by Caleb Kiatoukaysi for Legend Quest, Sunspire Keep: The Great Hall, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/thornbridge.txt - written by Caleb Kiatoukaysi for Legend Quest, Thornbridge: The Camp Gate, The Tents, The River's Edge, converted from its map by tools/rooms/convert_maps.py, released under public domain

Content/Zones/warehouse.txt - written by Caleb Kiatoukaysi for Legend Quest, A Warehouse: A Warehouse, converted from its map by tools/rooms/convert_maps.py, released under public domain

## Rooms

Every room's painted layers, at art resolution, and where its doorways are painted. Until the rooms
are painted by hand, tools/rooms/generate_room_placeholders.py puts them together out of the props,
tiles and title pictures above, stood up side on and given more detail.

Content/Rooms/bakery/inside/mid.png, Content/Rooms/bakery/inside/fore.png, Content/Rooms/bakery/inside/thumb.png, Content/Rooms/bakery/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Bakery in The Bakery, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: The Rising Loaf (inside): warm yellow plaster, the ovens' wall of brick under one long hood (the two ovens are the game's props), shelves of loaves over the counter, the bakery's board with its loaf, sheaves of wheat, a sunny window, strings of onions. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/barrow/steps/sky.png, Content/Rooms/barrow/steps/far.png, Content/Rooms/barrow/steps/mid.png, Content/Rooms/barrow/steps/fore.png, Content/Rooms/barrow/steps/thumb.png, Content/Rooms/barrow/steps/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Barrow Steps in The Barrow, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Barrow Steps: the way down into the barrow; the bright day outside through the portal at the west end, big lintelled stones carved with spirals along the passage, roots through the cracks over it, burial niches, and at the east end an arch on the dark going down to the hall. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/barrow/hall/sky.png, Content/Rooms/barrow/hall/far.png, Content/Rooms/barrow/hall/mid.png, Content/Rooms/barrow/hall/fore.png, Content/Rooms/barrow/hall/thumb.png, Content/Rooms/barrow/hall/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Barrow Hall in The Barrow, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Barrow Hall: a long hall of grey-violet stone, bays between square piers under round arches, burial niches with skulls and urns, carved roundels, roots hanging through the vault, cobwebs in the arches; the stone door to the side room on its door, the tomb door carved with the sun on its own, and bays open on the dark of the barrow going on beyond. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/barrow/side-room/sky.png, Content/Rooms/barrow/side-room/far.png, Content/Rooms/barrow/side-room/mid.png, Content/Rooms/barrow/side-room/fore.png, Content/Rooms/barrow/side-room/thumb.png, Content/Rooms/barrow/side-room/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Side Room in The Barrow, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The barrow's side room: a small burial chamber, its stone door back to the hall on the zone file's door, a sarcophagus with a knight's effigy on its dais under a carved roundel, walls of burial niches, and roots through the vault. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/chapel/inside/mid.png, Content/Rooms/chapel/inside/fore.png, Content/Rooms/chapel/inside/thumb.png, Content/Rooms/chapel/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Chapel in The Chapel, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: The Chapel of the Dawn (inside): warm stone, wall shafts carrying pointed arches under the roof, a rose window of the dawn's colours over the altar's niche (the altar and its cloth are the game's props), tall lancets either side over the hangings, sconces lit. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/cottage-a/inside/mid.png, Content/Rooms/cottage-a/inside/fore.png, Content/Rooms/cottage-a/inside/thumb.png, Content/Rooms/cottage-a/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, A Cottage in A Cottage, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: A cottage (the one with the bed by the door): cream plaster over a wainscot, dark beams, two curtained windows on the day with flowers in their boxes, a shelf of crockery and a picture of the sea, herbs drying from a beam, a sconce. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/cottage-b/inside/mid.png, Content/Rooms/cottage-b/inside/fore.png, Content/Rooms/cottage-b/inside/thumb.png, Content/Rooms/cottage-b/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, A Cottage in A Cottage, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: A cottage (the one with the hearth): blue-grey plaster, a brick chimney breast behind the hearth (the game's prop), a curtained window and a small one over the bed, a picture of hills, a shelf, onions and herbs hung from the beams. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/docks/pier/sky.png, Content/Rooms/docks/pier/far.png, Content/Rooms/docks/pier/mid.png, Content/Rooms/docks/pier/fore.png, Content/Rooms/docks/pier/thumb.png, Content/Rooms/docks/pier/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Pier in Saltwind Docks, painted procedurally by tools/rooms/paint_rooms_docks.py with tools/rooms/paint.py: The Pier: the long pier, boats moored, gulls on every post. Netty's teal fisherman's cottage at the west end under an oak, nets drying on its wall and lobster pots by its door; then the open harbour behind the quay's edge with a fishing boat and a dinghy tied up and gulls on the piles; the big warehouse with its hoist; and the harbour running out east to the mole and its beacon, the lighthouse on the headland beyond. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/docks/harbourside/sky.png, Content/Rooms/docks/harbourside/far.png, Content/Rooms/docks/harbourside/mid.png, Content/Rooms/docks/harbourside/fore.png, Content/Rooms/docks/harbourside/thumb.png, Content/Rooms/docks/harbourside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Harbourside in Saltwind Docks, painted procedurally by tools/rooms/paint_rooms_docks.py with tools/rooms/paint.py: The Harbourside: Tomas's net rack, the customs house, the Wager's berth with a gangway. West to east: a stack of cargo by a lamp, the Rusty Anvil (a shabby brick and timber dive bar under a plum roof, one window boarded, its anvil sign out); Tomas's nets drying; the long berth, open water behind the quay's edge with a pile at each end, where the Wager (the game's prop) ties up; the customs house with its clock and flag; a fishmonger's stall and a blossom tree; the teal warehouse with its loft and hoist; and the harbour again at the east end with a boat moored. Across the water the mole and its beacon, ships at anchor, and the headland. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/docks/fish-market/sky.png, Content/Rooms/docks/fish-market/far.png, Content/Rooms/docks/fish-market/mid.png, Content/Rooms/docks/fish-market/fore.png, Content/Rooms/docks/fish-market/thumb.png, Content/Rooms/docks/fish-market/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Fish Market in Saltwind Docks, painted procedurally by tools/rooms/paint_rooms_docks.py with tools/rooms/paint.py: The Fish Market: stalls, ice, a crane hoisting crates. West to east: the pink townhouse; the market on the quay, two fish stalls either side of a crane lifting crates out of a lighter; the Chandler's (the shop: an anchor on its sign); the ice store with blocks of ice at its hatch; a rack of fish drying; the Watch post; then the street climbs east toward the city, a blossom tree over the wall, lamps either side of the steps up, and Questeria on its hill across the far layer with the Sunspire. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/forge/inside/mid.png, Content/Rooms/forge/inside/fore.png, Content/Rooms/forge/inside/thumb.png, Content/Rooms/forge/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Smithy in The Smithy, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: Halden's forge (inside): grey stone gone sooty, a great brick hood over the hearth (the hearth and the anvil are the game's props) up through the roof with its glow, the bellows hung by it, boards of tools either side, horseshoes nailed in an arch, a shield on crossed swords for the trade, a window on the day. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/gilded-flagon/taproom/mid.png, Content/Rooms/gilded-flagon/taproom/fore.png, Content/Rooms/gilded-flagon/taproom/thumb.png, Content/Rooms/gilded-flagon/taproom/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Taproom in The Gilded Flagon, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: The Gilded Flagon's taproom: warm yellow plaster over a panelled wainscot under dark beams. The bar end at the left: the inn's gilded board over it, two long shelves of bottles over the kegs, a garland of hops, a wheel chandelier; a curtained window at the far left. Then the stair up to the balcony, which runs east along the wall either side of the great stone chimney breast (the hearth is the game's prop, in front of it), the rooms' doors and a window behind its balustrade. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/gulls-wager/deck/sky.png, Content/Rooms/gulls-wager/deck/far.png, Content/Rooms/gulls-wager/deck/mid.png, Content/Rooms/gulls-wager/deck/fore.png, Content/Rooms/gulls-wager/deck/thumb.png, Content/Rooms/gulls-wager/deck/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Main Deck in The Gull's Wager, painted procedurally by tools/rooms/paint_rooms_docks.py with tools/rooms/paint.py: The Main Deck of the Gull's Wager, two days out at night. A moon and stars over a long dark swell with the moon's path on it; the far bulwark along the deck's back edge, navy and gold, its pin rails hung with coiled falls and a lamp lit at each of the zone's rail lights; the forecastle and the bowsprit at the left (the bow), the fore and mizzen masts with their sails furled and the shrouds and stays of all three (the mainmast is the game's prop), and the stern cabin at the right with its windows lit, the big stern lantern and the ensign. The room is dark: the wood is moonlit, and the game's dark and the lamps' pools do the rest. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/mirewood-quarry/lip/sky.png, Content/Rooms/mirewood-quarry/lip/far.png, Content/Rooms/mirewood-quarry/lip/mid.png, Content/Rooms/mirewood-quarry/lip/fore.png, Content/Rooms/mirewood-quarry/lip/thumb.png, Content/Rooms/mirewood-quarry/lip/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Quarry's Lip in The Old Quarry: its sky at 4x, its far layer at 2x (the skyline, cooled and hazed), its mid layer at 3x (the buildings on their doors, or the wall, and the dressing), its foreground at 4x, its 200x113 thumbnail for the zone map, and doors.txt, where each door's doorway is painted, put together procedurally by tools/rooms/generate_room_placeholders.py from the game's own props, tiles and title pictures, released under public domain

Content/Rooms/mirewood-quarry/cut/sky.png, Content/Rooms/mirewood-quarry/cut/far.png, Content/Rooms/mirewood-quarry/cut/mid.png, Content/Rooms/mirewood-quarry/cut/fore.png, Content/Rooms/mirewood-quarry/cut/thumb.png, Content/Rooms/mirewood-quarry/cut/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Second Cut in The Old Quarry: its sky at 4x, its far layer at 2x (the skyline, cooled and hazed), its mid layer at 3x (the buildings on their doors, or the wall, and the dressing), its foreground at 4x, its 200x113 thumbnail for the zone map, and doors.txt, where each door's doorway is painted, put together procedurally by tools/rooms/generate_room_placeholders.py from the game's own props, tiles and title pictures, released under public domain

Content/Rooms/mirewood-quarry/floor/sky.png, Content/Rooms/mirewood-quarry/floor/far.png, Content/Rooms/mirewood-quarry/floor/mid.png, Content/Rooms/mirewood-quarry/floor/fore.png, Content/Rooms/mirewood-quarry/floor/thumb.png, Content/Rooms/mirewood-quarry/floor/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Quarry Floor in The Old Quarry: its sky at 4x, its far layer at 2x (the skyline, cooled and hazed), its mid layer at 3x (the buildings on their doors, or the wall, and the dressing), its foreground at 4x, its 200x113 thumbnail for the zone map, and doors.txt, where each door's doorway is painted, put together procedurally by tools/rooms/generate_room_placeholders.py from the game's own props, tiles and title pictures, released under public domain


Content/Rooms/mirewood/edge/sky.png, Content/Rooms/mirewood/edge/far.png, Content/Rooms/mirewood/edge/mid.png, Content/Rooms/mirewood/edge/fore.png, Content/Rooms/mirewood/edge/thumb.png, Content/Rooms/mirewood/edge/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Mirewood's Edge in The Mirewood, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: The Mirewood's edge: the road out of the meadows going in under the first trees at dusk; bright and green at the west end, a blossom tree and lush oaks, the wood growing older, mossier and stranger to the east, the leaves turning, until the last trees have black veins in their bark with a violet light in them and mist lies along the ground. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/mirewood/deep/sky.png, Content/Rooms/mirewood/deep/far.png, Content/Rooms/mirewood/deep/mid.png, Content/Rooms/mirewood/deep/fore.png, Content/Rooms/mirewood/deep/thumb.png, Content/Rooms/mirewood/deep/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Deep Wood in The Mirewood, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: The Deep Wood: old trees with roots like walls and moss up their trunks, black veins splitting their bark with a violet light in them, dead snags, the rangers' footbridge over a gully of tar, mist along the ground, and the old quarry's timbered mouth in an outcrop of grey rock on the zone file's door. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/mirewood/camp/sky.png, Content/Rooms/mirewood/camp/far.png, Content/Rooms/mirewood/camp/mid.png, Content/Rooms/mirewood/camp/fore.png, Content/Rooms/mirewood/camp/thumb.png, Content/Rooms/mirewood/camp/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Rangers' Camp in The Mirewood, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: The Rangers' Camp: a clearing in the Mirewood at night by firelight, the moon over the trees, the rangers' tents, the stump table with its map, a rack of bows and spears, crates and firewood, and lanterns strung between the trunks; the fire itself is the floor's. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/old-crown-vault/entrance/sky.png, Content/Rooms/old-crown-vault/entrance/far.png, Content/Rooms/old-crown-vault/entrance/mid.png, Content/Rooms/old-crown-vault/entrance/fore.png, Content/Rooms/old-crown-vault/entrance/thumb.png, Content/Rooms/old-crown-vault/entrance/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Antechamber in The Old Crown Vault, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Old Crown Vault's antechamber: cream stone dressed with gold under the Sunspire; at the west end the wall broken through where the sappers dug in, then the stair up to the terrace with daylight falling down it, banners of navy and gold, the Aurelian sun in the roundels, and the great vault doors to the long hall and to the treasury. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/old-crown-vault/halls/sky.png, Content/Rooms/old-crown-vault/halls/far.png, Content/Rooms/old-crown-vault/halls/mid.png, Content/Rooms/old-crown-vault/halls/fore.png, Content/Rooms/old-crown-vault/halls/thumb.png, Content/Rooms/old-crown-vault/halls/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Long Hall in The Old Crown Vault, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Old Crown Vault's long hall: an arcade of gilded bays going on and on, the traps' slots in the walls, relics on shelves, banners and roundels of the sun, the vault doors back to the antechamber, and the Crown Door, gold, with the Old Crown across its leaves. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/old-crown-vault/strongroom/sky.png, Content/Rooms/old-crown-vault/strongroom/far.png, Content/Rooms/old-crown-vault/strongroom/mid.png, Content/Rooms/old-crown-vault/strongroom/fore.png, Content/Rooms/old-crown-vault/strongroom/thumb.png, Content/Rooms/old-crown-vault/strongroom/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Relic Gallery in The Old Crown Vault, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Relic Gallery: the vault's long gallery of relics, walls of shelves and alcoves full of crowns, goblets, orbs and old books, and in the middle a great apse with the Aurelian sun in gold over the Sunheart's pedestal (the pedestal and the cases are the floor's), the Crown Door back to the hall at the west end. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/old-crown-vault/treasury/sky.png, Content/Rooms/old-crown-vault/treasury/far.png, Content/Rooms/old-crown-vault/treasury/mid.png, Content/Rooms/old-crown-vault/treasury/fore.png, Content/Rooms/old-crown-vault/treasury/thumb.png, Content/Rooms/old-crown-vault/treasury/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Treasury in The Old Crown Vault, painted procedurally by tools/rooms/paint_rooms_under.py with tools/rooms/paint.py: The Old Crown's treasury: a gilded chamber whose alcoves are heaped with gold, chests spilling coins, crowns and goblets, shelves of plate, the treasury doors back to the antechamber on the zone file's door. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/questeria/gate-plaza/sky.png, Content/Rooms/questeria/gate-plaza/far.png, Content/Rooms/questeria/gate-plaza/mid.png, Content/Rooms/questeria/gate-plaza/fore.png, Content/Rooms/questeria/gate-plaza/thumb.png, Content/Rooms/questeria/gate-plaza/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Gate Plaza in Questeria, painted procedurally by tools/rooms/paint_rooms_questeria.py with tools/rooms/paint.py: The gate plaza: the city gate at the west end with the harbour beyond it, the Watch's notice board under its tower, a turret house, the Chapel of the Dawn with its rose window and bell tower, the ivy house, the Chandler's with candles in its window, the rose house and the awning house, and the arch through to the festival square at the east end. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/questeria/festival-square/sky.png, Content/Rooms/questeria/festival-square/far.png, Content/Rooms/questeria/festival-square/mid.png, Content/Rooms/questeria/festival-square/fore.png, Content/Rooms/questeria/festival-square/thumb.png, Content/Rooms/questeria/festival-square/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Festival Square in Questeria, painted procedurally by tools/rooms/paint_rooms_questeria.py with tools/rooms/paint.py: The festival square, the square of the Purging Dawn: a colonnade on the west, the broad stair up to the Sunspire steps, the palace front with its balcony over the fountain and the sun banners hanging from it, the City Gate out to the road, the ivy house and a tall townhouse; bunting strung across, the Sunspire high behind. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/questeria/sunspire-steps/sky.png, Content/Rooms/questeria/sunspire-steps/far.png, Content/Rooms/questeria/sunspire-steps/mid.png, Content/Rooms/questeria/sunspire-steps/fore.png, Content/Rooms/questeria/sunspire-steps/thumb.png, Content/Rooms/questeria/sunspire-steps/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Sunspire Steps in Questeria, painted procedurally by tools/rooms/paint_rooms_questeria.py with tools/rooms/paint.py: The Sunspire steps: the terrace at the top of the stair, the keep's great door under its towers with the sun over it, a balustrade along the terrace with the way down to the square at its west end, the old iron gate to the Old Crown Vault low in the terrace wall, and the Sunspire itself filling the far layer. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/questeria/lantern-market/sky.png, Content/Rooms/questeria/lantern-market/far.png, Content/Rooms/questeria/lantern-market/mid.png, Content/Rooms/questeria/lantern-market/fore.png, Content/Rooms/questeria/lantern-market/thumb.png, Content/Rooms/questeria/lantern-market/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Lantern Market in Questeria, painted procedurally by tools/rooms/paint_rooms_questeria.py with tools/rooms/paint.py: Questeria's Lantern Market: a street of cream stone and timber under blue and rose roofs, strings of paper lanterns between the eaves, the Gilded Flagon at the west end, the bakery and Halden's forge to the east, and in the middle an open square of stalls with the Sunspire beyond it. Late afternoon: the sun low in the west, the windows starting to glow. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/questeria/thornbridge-gate/sky.png, Content/Rooms/questeria/thornbridge-gate/far.png, Content/Rooms/questeria/thornbridge-gate/mid.png, Content/Rooms/questeria/thornbridge-gate/fore.png, Content/Rooms/questeria/thornbridge-gate/thumb.png, Content/Rooms/questeria/thornbridge-gate/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Thornbridge Gate in Questeria, painted procedurally by tools/rooms/paint_rooms_questeria.py with tools/rooms/paint.py: The Thornbridge gate: the last of the Lantern Market's houses, a turret house and a townhouse with a garden between, and the city's east gate, its arch open on the road down to Thornbridge and the refugees' tents by the river below. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/road/farmland/sky.png, Content/Rooms/road/farmland/far.png, Content/Rooms/road/farmland/mid.png, Content/Rooms/road/farmland/fore.png, Content/Rooms/road/farmland/thumb.png, Content/Rooms/road/farmland/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Farmland in The Old Road, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: The Old Road's farmland: the road out of Questeria running east between hedgerows, the farmhouse with its washing line and blossom tree, gold wheat behind a rail fence, haystacks, the windmill on its rise, and at the east end the road turning back over a little stone bridge toward the fork. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/road/fork/sky.png, Content/Rooms/road/fork/far.png, Content/Rooms/road/fork/mid.png, Content/Rooms/road/fork/fore.png, Content/Rooms/road/fork/thumb.png, Content/Rooms/road/fork/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Fork in The Old Road, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: The fork in the Old Road: the meadow going on from the farmland, a stream at the west end with its anglers' blossom trees, the barrow's grassy mound with its portal of three great stones on the zone file's door, the great oak the young couple walk out to, and east of it the wood thickening, darker and older, toward the Mirewood. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/rusty-anvil/bar/mid.png, Content/Rooms/rusty-anvil/bar/fore.png, Content/Rooms/rusty-anvil/bar/thumb.png, Content/Rooms/rusty-anvil/bar/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Bar in The Rusty Anvil, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: The Rusty Anvil (03b §1: a dim dockside bar, Brannock's corner table), kept as it was built: low and dark, tall wainscot of tarred boards under stained plaster, heavy low beams on posts, the bar at the left with a crooked board over it and a shelf of bottles, a grimy window on the night, sailors' junk on the walls (a wheel, an anchor, a prize fish, a net), notices pinned up, and one candle sconce lit. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/shop/inside/mid.png, Content/Rooms/shop/inside/fore.png, Content/Rooms/shop/inside/thumb.png, Content/Rooms/shop/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Chandler's in The Chandler's, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: The Chandler's (inside): a ship's chandlery in blue plaster over dark boards, the shop's board with its anchor, shelves of jars, lanterns and tins behind where the counter stands, a model ship up on a bracket, hanks of rope on pegs, an anchor and a wheel on the walls, lanterns hung from the beams, and a window on the harbour. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/sunspire-keep/great-hall/mid.png, Content/Rooms/sunspire-keep/great-hall/fore.png, Content/Rooms/sunspire-keep/great-hall/thumb.png, Content/Rooms/sunspire-keep/great-hall/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Great Hall in Sunspire Keep, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: Sunspire Keep's great hall: a long hall of cream ashlar over a blue-grey dado, fluted pilasters where the marble pillars stand, round arches springing between them under the ceiling, tall windows of stained glass. The throne at the left stands before a cloth of estate with a great gold sun under the sun window; a gallery runs along the middle of the hall over the arch; the door out to the terrace is at the east end. Lit by its windows (the room isn't dark); gold chandeliers hang down the hall. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/thornbridge/camp-gate/sky.png, Content/Rooms/thornbridge/camp-gate/far.png, Content/Rooms/thornbridge/camp-gate/mid.png, Content/Rooms/thornbridge/camp-gate/fore.png, Content/Rooms/thornbridge/camp-gate/thumb.png, Content/Rooms/thornbridge/camp-gate/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Camp Gate in Thornbridge, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: Thornbridge's camp gate: Questeria's east gate at the west end, its towers under blue cones and the sunlit city through the arch, the wall running off west; cypresses by the road, Hedda's post with its notice board, and the first of the refugees' tents. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/thornbridge/tents/sky.png, Content/Rooms/thornbridge/tents/far.png, Content/Rooms/thornbridge/tents/mid.png, Content/Rooms/thornbridge/tents/fore.png, Content/Rooms/thornbridge/tents/thumb.png, Content/Rooms/thornbridge/tents/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The Tents in Thornbridge, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: Thornbridge's tents: the refugees' camp in the meadow under the city wall, rows of patched tents in every colour, cooking fires with pots on tripods, washing lines, crates and bundles, the clinic's big bell tent on the zone file's healer station, and blossom at the east end toward the river. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/thornbridge/river-edge/sky.png, Content/Rooms/thornbridge/river-edge/far.png, Content/Rooms/thornbridge/river-edge/mid.png, Content/Rooms/thornbridge/river-edge/fore.png, Content/Rooms/thornbridge/river-edge/thumb.png, Content/Rooms/thornbridge/river-edge/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, The River's Edge in Thornbridge, painted procedurally by tools/rooms/paint_rooms_wild.py with tools/rooms/paint.py: Thornbridge's river edge: the old bridge itself at the west end, three arches of mossy stone standing in the river, the river running across the back of the room under a far bank of meadow and trees, reeds along the near bank, blossom and an oak by the water. Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/warehouse/inside/mid.png, Content/Rooms/warehouse/inside/fore.png, Content/Rooms/warehouse/inside/thumb.png, Content/Rooms/warehouse/inside/doors.txt - created by Caleb Kiatoukaysi for Legend Quest, A Warehouse in A Warehouse, painted procedurally by tools/rooms/paint_rooms_interiors.py with tools/rooms/paint.py: A warehouse on the docks (inside): tall walls of boards between heavy posts under the roof's beams, a loft along the left with sacks and crates stored on it, a hoist's block and chain from the beam, high windows on the day, a lantern hung by the clerk's desk (the crates, barrels and the desk on the floor are the game's props). Its layers are one texel to a screen pixel (RoomScene draws each at the scale its height gives), with its 200x113 thumbnail for the zone map and doors.txt, where each door's doorway is painted, released under public domain

Content/Rooms/Life/clouds.png, Content/Rooms/Life/gull.png, Content/Rooms/Life/flag.png, Content/Rooms/Life/flag-pole.png, Content/Rooms/Life/flame.png, Content/Rooms/Life/candle.png, Content/Rooms/Life/smoke.png, Content/Rooms/Life/mote.png, Content/Rooms/Life/ash.png, Content/Rooms/Life/sparkle.png, Content/Rooms/Life/leaf.png - created by Caleb Kiatoukaysi for Legend Quest, the rooms' life loops in the illustrated style: six drifting clouds, a gull's two wingbeats, a pennant waving and its pole, a torch's and a candle's flames flickering, and the smoke, motes, ash, sparkles and petals the rooms' particles are drawn with, painted procedurally by tools/rooms/paint_life.py with tools/rooms/paint.py, released under public domain

## Music

Content/Music/embers.wav - created by Caleb Kiatoukaysi for Legend Quest, "Embers", the title theme, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/hearthsmoke.wav - created by Caleb Kiatoukaysi for Legend Quest, "Hearthsmoke", for walking about on a lit map, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/cold-stone.wav - created by Caleb Kiatoukaysi for Legend Quest, "Cold Stone", for walking about underground, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/salt-and-lanterns.wav - created by Caleb Kiatoukaysi for Legend Quest, "Salt and Lanterns", for the Gull's Wager at sea, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/meadowlark.wav - created by Caleb Kiatoukaysi for Legend Quest, "Meadowlark", for the sunny green fields round Questeria, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/sunspire.wav - created by Caleb Kiatoukaysi for Legend Quest, "Sunspire", for the royal keep, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/steel.wav - created by Caleb Kiatoukaysi for Legend Quest, "Steel", for fights, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/the-hunger.wav - created by Caleb Kiatoukaysi for Legend Quest, "The Hunger", for boss fights, composed and synthesised by tools/generate_audio.py, released under public domain

## Sound effects

Content/Sfx/menu-move.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/menu-confirm.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/menu-back.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/option-change.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/pause.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/footstep-stone.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/footstep-grass.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/door.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/door-locked.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/chest.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/lever.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/pickup.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/waypoint.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/dice-roll.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/check-pass.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/check-fail.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/approve.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/disapprove.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/heartbeat.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/ship-bell.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/wolf-howl.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/crystal-pulse.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/war-horn.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/bowshot.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/coin.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/sword-hit.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/miss.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/crit.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/thud.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/shield.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/bow.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/cast.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/fire.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/frost.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/heal.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/potion.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/soul-link.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/turn-start.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/downed.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/death.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/victory.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/game-over.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/equip.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

Content/Sfx/buy.wav - created by Caleb Kiatoukaysi for Legend Quest, a sound effect, synthesised by tools/generate_audio.py, released under public domain

## Text

Content/Spectral/Spectral-Light.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/Spectral-Medium.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/OFL.txt - the SIL Open Font License text and copyright notice for Spectral, redistributed alongside the font as that licence requires (copied into the build output, so it ships with the game)

Content/spectral-title.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; names Spectral Light at 92pt by relative path, so the build uses the font shipped in Content/Spectral/ rather than whatever is installed

Content/spectral-ui.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Medium at 26pt, letterspaced

Content/spectral-detail.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Light at 17pt, letterspaced

Content/spectral-number.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Medium at 40pt, only the digits and the plus and minus signs, for the numbers that pop up over someone hit or healed

## The scripts

tools/pixels.py, tools/generate_ui.py, tools/generate_world.py, tools/generate_harbour.py, tools/generate_interiors.py, tools/generate_paintings.py, tools/generate_audio.py - every asset above that says "drawn procedurally" or "synthesised" comes out of these; pixels.py is the PNG writer and colour helpers the art scripts share. They need nothing but CPython (3.12 or newer for generate_audio.py), and every one is seeded, so a rerun gives the same files

tools/generate_paperdoll.py - written by Caleb Kiatoukaysi for Legend Quest, takes the youth off tools/source/npc-townsperson.png (through cut_supplied.py) apart into the paperdoll: sorts every pixel of him into what it's part of, cuts each part down to key shades, and draws the other hair, the faces, the beards, the skirt and the cape onto his frames. It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/cut_supplied.py - written by Caleb Kiatoukaysi for Legend Quest, cuts the pictures in tools/source/ into the title's layers and loops, Questeria's and the harbour's props and the townsfolk's sheets (see The pictures the generators read). It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/check_assets.py - written by Caleb Kiatoukaysi for Legend Quest, checks this file, Content/Content.mgcb and the files on disk against each other

tools/import_character.py - written by Caleb Kiatoukaysi for Legend Quest, puts a character's picture made in ChatGPT into the game in one go: keys its background off with a soft matte, trims it to the figure, finds the feet and the head, scales it by its build, writes it and its .json to Content/Characters/Art, copies the picture to tools/source/characters, writes its lines here and in Content/Content.mgcb (see Characters (imported art)), and saves a preview to shots/art/import. It needs nothing but CPython and numpy, and nothing in it is random, so a rerun gives the same files

tools/generate_keep.py - written by Caleb Kiatoukaysi for Legend Quest, draws Sunspire Keep: its tileset, its furniture, the sunlight from its windows, and the stair down to the Old Crown Vault in Questeria's terrace. It needs nothing but CPython, and every bit of noise in it is seeded, so a rerun gives the same files

tools/generate_vault.py - written by Caleb Kiatoukaysi for Legend Quest, draws the Old Crown Vault: its tileset, its doors, lamps, braziers and lever, its treasure, relic shelves and cases, the dart traps, the Sunheart's empty pedestal, and the Hollowed sappers' rubble and hole. It needs nothing but CPython, and every bit of noise in it is seeded, so a rerun gives the same files

tools/generate_mirewood.py - written by Caleb Kiatoukaysi for Legend Quest, draws the Mirewood's and the Old Quarry's tilesets and props, and cuts the black-veined trees off tools/source/questeria-trees.png (through cut_supplied.py) and turns them. It needs nothing but CPython, and it's seeded, so a rerun gives the same files

tools/generate_creatures.py - written by Caleb Kiatoukaysi for Legend Quest, draws the creatures' sheets (the sea-wraith, the gloomfang, the Thornmother, the three Hollowed and the vault construct) out of shaded round parts posed frame by frame. It needs nothing but CPython, and every bit of noise in it is seeded on where it is, so a rerun gives the same files

tools/generate_meadow.py - written by Caleb Kiatoukaysi for Legend Quest, draws the meadows' tileset and the farm's, the bridge's and the refugees' camp's props, and cuts the great oak, the wild flowers, the farmhouse, the east gate and the city wall off tools/source/ (through cut_supplied.py). It needs nothing but CPython, and every bit of noise in it is seeded, so a rerun gives the same files

tools/generate_items.py - written by Caleb Kiatoukaysi for Legend Quest, draws the item icons and the inventory's slots: every pixel is given a material and shaded from the top left at the end, with a dark outline round the lot, and it checks Items/ItemCatalog.cs's icons and rarities against its own list before it writes anything. It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/generate_combat_art.py - written by Caleb Kiatoukaysi for Legend Quest, draws the ability and condition icons and the pieces of the fight's screen with generate_items.py's kit, and checks Combat/CombatIcons.cs and the AbilityId and ConditionId enums against its own tables before it writes anything. It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/rooms/roomkit.py, tools/rooms/generate_room_placeholders.py - written by Caleb Kiatoukaysi for Legend Quest, the room placeholder tools: generate_room_placeholders.py paints every room's layers, its thumbnail and its doors.txt into Content/Rooms out of the game's own props, tiles and title pictures, and writes previews of them; roomkit.py is what every room tool shares. They need nothing but CPython, and every one is seeded, so a rerun gives the same files

tools/rooms/sourcekit.py, tools/rooms/make_room_template.py, tools/rooms/cut_room.py, tools/rooms/cut_tiles.py, tools/rooms/check_room_sources.py - written by Caleb Kiatoukaysi for Legend Quest, the tools for the owner's paintings (12-rooms 3.5): make_room_template.py writes the 1536x1024 guide each panel of a room is painted over in ChatGPT, into shots/art/room-templates; cut_room.py cuts a room's panels in tools/source/rooms into its flat mid layer, its foreground, doors.txt and thumbnail; cut_tiles.py cuts the floor textures in tools/source/tiles into the tilesets' sheets and lists what each tileset needs; check_room_sources.py says what's painted and what's still to do; sourcekit.py is what they share (the panels' geometry, PNGs, resizing, keying, and keeping this file and Content.mgcb up to date). They need nothing but CPython and numpy, and nothing in them is random, so a rerun gives the same files

tools/rooms/paint.py, tools/rooms/paint_all.py, tools/rooms/paint_life.py, tools/rooms/paint_props.py, tools/rooms/paint_room.py, tools/rooms/paint_rooms_docks.py, tools/rooms/paint_rooms_interiors.py, tools/rooms/paint_rooms_questeria.py, tools/rooms/paint_rooms_under.py, tools/rooms/paint_rooms_wild.py, tools/rooms/paint_tiles.py - written by Caleb Kiatoukaysi for Legend Quest, the room painters, in the illustrated style: paint.py is the drawing library (skies, the far layer's mountains, roofs and castles, houses built from walls, roofs, windows and doors, towers, gates and stairs, trees, props, interiors) on tools/rig/vector.py and style.py; paint_room.py is what a room's recipe paints into, and writes the room, its thumbnail and doors.txt, with previews; paint_all.py runs everything and keeps Content.mgcb and this file up to date; paint_tiles.py paints every floor tileset, paint_props.py the floor props and paint_life.py the rooms' life loops; and each paint_rooms_(family).py has a family's recipes (paint_rooms_questeria.py, Questeria's streets; paint_rooms_docks.py, the docks and the Gull's Wager; paint_rooms_interiors.py, every room indoors; paint_rooms_wild.py, the road, the Mirewood and its quarry, Thornbridge; paint_rooms_under.py, the barrow and the Old Crown Vault). They need CPython and numpy, and every one is seeded, so a rerun gives the same files, released under public domain

tools/rooms/zonefile.py, tools/rooms/convert_maps.py, tools/rooms/plan.py, tools/rooms/catalogs.py, tools/rooms/check_rooms.py, tools/check_spawns.py - written by Caleb Kiatoukaysi for Legend Quest, the zone tools: zonefile.py reads and writes the zone files, convert_maps.py turns the maps into zones with the hand work in plan.py, catalogs.py reads what the game's C# catalogs hold, check_rooms.py checks the zones against them, and check_spawns.py checks that every spawn the scripts name is in their zone. They need nothing but CPython

tools/rig/style.py - written by Caleb Kiatoukaysi for Legend Quest, the owner's art guide (tools/source/art-guide.png; the alignment notes override the art plan's "Twilight Cel"): clean dark-brown linework, heavier outside and lighter inside; cel shading in two to four hard-edged tones; a key light from the upper left with a soft fill and only a thin neutral rim; subtle texture and wear. Folds (§1.7) and armour details (§1.8) as before, and weave, dirt and scars. All as functions that paint into a vector.Canvas. Everything paints ramp indices into a tint group (theme.tones(..., group=g)), so one drawing tints into any colour; a group of 0 bakes the tones' colours instead. It needs nothing but CPython and numpy, and nothing in it is random, so a rerun gives the same files

tools/rig/theme.py - written by Caleb Kiatoukaysi for Legend Quest, the rig's colours, from the owner's art guide (tools/source/art-guide.png, "2D Art Style & Generation Guide"): muted, earthy, worked out in OKLab by the same formulas for every material. It needs nothing but CPython and numpy, and nothing in it is random, so a rerun gives the same files

tools/rig/vector.py - written by Caleb Kiatoukaysi for Legend Quest, the rig's drawing library: anti-aliased vector shapes into numpy pictures, by 4x4 supersampling. Every part of every kit is drawn with it (03-art-overhaul.md §12, the art plan's §2.7). It needs nothing but CPython and numpy, and nothing in it is random, so a rerun gives the same files

## The pictures the generators read

These are the only sprites the generators don't draw from nothing. Each starts from a picture I
made with ChatGPT, and tools/cut_supplied.py keys it off its backdrop, samples it down to art
pixels and lays it out the way the game reads it.

Content/Title/sky.png - built by tools/cut_supplied.py from tools/source/title-sky.png: the title's sky, blue up top going pink at the horizon, sampled to the title's 416x277 canvas; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/city.png - built by tools/cut_supplied.py from tools/source/title-city.png: Questeria on its cliffs, the middle layer of the title, keyed off its backdrop and sampled to the canvas; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/foreground.png - built by tools/cut_supplied.py from tools/source/title-foreground.png: the terrace and the blossom trees at the front of the title, keyed and sampled to the canvas; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/clouds.png - built by tools/cut_supplied.py from tools/source/title-clouds.png: six clouds, one to a cell, that drift across the title; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/island.png - built by tools/cut_supplied.py from tools/source/title-far.png: the biggest of the floating islands in its sky, waterfall and all, cut out on its own; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/banner.png - built by tools/cut_supplied.py from tools/source/title-banner.png: eight frames of a banner blowing, each lined up on the tip of its pole; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/waterfall.png - built by tools/cut_supplied.py from tools/source/title-waterfall.png: six frames of a floating island's waterfall, lined up on the top of the island; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/spire.png - built by tools/cut_supplied.py from tools/source/title-spire.png: eight frames of the crystal spire's light, keyed off the glow round it and lined up on its foot; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Title/petal.png - built by tools/cut_supplied.py from tools/source/questeria-ground.png: one blossom petal lying on the paving, for the ones that fall across the title; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/palace.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png: the palace, a prop, keyed off its backdrop and sampled to 290 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/temple.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png: the temple, a prop, keyed off its backdrop and sampled to 176 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/city-gate.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png: the city gate, with a way through the middle, a prop, keyed off its backdrop and sampled to 190 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/guildhall.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the guildhall, a prop, keyed off its backdrop and sampled to 220 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-ivy.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a house with ivy up it, a prop, keyed off its backdrop and sampled to 130 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-shop.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a house with a shop in the front, a prop, keyed off its backdrop and sampled to 170 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-awning.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a house with an awning, a prop, keyed off its backdrop and sampled to 148 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-rose.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a house with roses round it, a prop, keyed off its backdrop and sampled to 138 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fountain.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: the plaza's fountain, a prop, keyed off its backdrop and sampled to 128 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/statue.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a statue on its plinth, a prop, keyed off its backdrop and sampled to 48 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crystal-pillar.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a crystal on a pillar, a prop, keyed off its backdrop and sampled to 20 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/lamp.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a street lamp, a prop, keyed off its backdrop and sampled to 16 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/banner-pink.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a pink banner on its pole, a prop, keyed off its backdrop and sampled to 36 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/banner-blue.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a blue banner on its pole, a prop, keyed off its backdrop and sampled to 36 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bench.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a bench, a prop, keyed off its backdrop and sampled to 46 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/planter.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a planter of flowers, a prop, keyed off its backdrop and sampled to 32 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cypress-pot.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a cypress in a pot, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flower-cart.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a flower cart, a prop, keyed off its backdrop and sampled to 50 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/notice-board.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a notice board, what Questeria's sign looks like, a prop, keyed off its backdrop and sampled to 40 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/well-roofed.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a well with a roof over it, a prop, keyed off its backdrop and sampled to 50 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cask.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a cask, a prop, keyed off its backdrop and sampled to 12 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/water-barrel.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a water barrel, a prop, keyed off its backdrop and sampled to 18 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stall-blue.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a market stall with a blue awning, a prop, keyed off its backdrop and sampled to 72 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stall-pink.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a market stall with a pink awning, a prop, keyed off its backdrop and sampled to 70 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stall-teal.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a market stall with a teal awning, a prop, keyed off its backdrop and sampled to 68 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stall-cloth.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a cloth stall, a prop, keyed off its backdrop and sampled to 70 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/garden-wall.png - built by tools/cut_supplied.py from tools/source/questeria-garden.png: a length of garden wall, a prop, keyed off its backdrop and sampled to 84 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cherry-tree.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a cherry tree in blossom, a prop, keyed off its backdrop and sampled to 104 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cherry-tree-small.png - built by tools/cut_supplied.py from tools/source/questeria-garden.png: a small cherry tree, a prop, keyed off its backdrop and sampled to 70 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: an oak, a prop, keyed off its backdrop and sampled to 96 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/blossom-tree.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a tree in white blossom, a prop, keyed off its backdrop and sampled to 112 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cypress.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a cypress, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/topiary.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a clipped bush, a prop, keyed off its backdrop and sampled to 34 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flower-arch.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: an arch of flowers to walk through, a prop, keyed off its backdrop and sampled to 48 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bush-flowers.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a flowering bush, a prop, keyed off its backdrop and sampled to 32 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bush-pink.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a pink bush, a prop, keyed off its backdrop and sampled to 32 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flowers-pink.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a patch of pink flowers, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flowers-white.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a patch of white flowers, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flowers-mixed.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a patch of flowers, a prop, keyed off its backdrop and sampled to 26 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rocks.png - built by tools/cut_supplied.py from tools/source/questeria-trees.png: a few rocks, a prop, keyed off its backdrop and sampled to 30 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/inn.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the inn: the shop's house, a size bigger, a prop, keyed off its backdrop and sampled to 186 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/table.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a table, a prop, keyed off its backdrop and sampled to 46 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cargo-crate.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a crate, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cargo-barrel.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a barrel, a prop, keyed off its backdrop and sampled to 18 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/sacks.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a heap of sacks, a prop, keyed off its backdrop and sampled to 36 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rope-posts.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: two posts with a rope between them, a prop, keyed off its backdrop and sampled to 40 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bollard.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a mooring post, a prop, keyed off its backdrop and sampled to 9 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/lantern-post.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a lantern hung from a post, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/signpost.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a signpost, a prop, keyed off its backdrop and sampled to 40 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crystal-stone.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: the waypoint, a crystal on a plinth, dim and then glowing, each keyed off the glow round it and lined up on the plinth; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/harbour-bell.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: the harbour bell on its gallows post, a prop, keyed off its backdrop and sampled to 26 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crate-open.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: an open crate, a prop, keyed off its backdrop and sampled to 20 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crate-apples.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a crate of apples, a prop, keyed off its backdrop and sampled to 18 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crate-onions.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a crate of onions, a prop, keyed off its backdrop and sampled to 16 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/crate-stack.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: two crates stacked, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/barrel-water.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a water barrel with a blue lid, a prop, keyed off its backdrop and sampled to 19 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/barrel-salt.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: an open barrel of salt, a prop, keyed off its backdrop and sampled to 17 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/sack-pile.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a pile of grain sacks, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/washing-line.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a washing line with washing on it, a prop, keyed off its backdrop and sampled to 40 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/water-pump.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a water pump with a bucket, a prop, keyed off its backdrop and sampled to 22 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/trough.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a stone water trough with a spout, a prop, keyed off its backdrop and sampled to 34 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/washtub.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a wooden washtub, a prop, keyed off its backdrop and sampled to 23 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/well-flowers.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a roofed well grown over with flowers, a prop, keyed off its backdrop and sampled to 56 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/quest-board.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: the quest board, roofed and hung with flowers, a prop, keyed off its backdrop and sampled to 44 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/signpost-arrows.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a signpost with painted arrows, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/planter-stone.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a round stone planter, a prop, keyed off its backdrop and sampled to 23 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/lamp-arm.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a street lamp, its lantern hung from an arm, a prop, keyed off its backdrop and sampled to 36 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/lamp-double.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a street lamp with two lanterns, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/lantern-small.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a lantern on the ground, a prop, keyed off its backdrop and sampled to 14 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/planter-long.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a long planter of flowers and lavender, a prop, keyed off its backdrop and sampled to 38 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/planter-pink.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a planter of pink flowers, a prop, keyed off its backdrop and sampled to 34 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/flower-box.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a small box of flowers, a prop, keyed off its backdrop and sampled to 17 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/barrel-flowers.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a barrel of flowers, a prop, keyed off its backdrop and sampled to 20 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bench-iron.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a bench with iron arms, a prop, keyed off its backdrop and sampled to 41 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bench-plain.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a wooden bench, a prop, keyed off its backdrop and sampled to 41 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/handcart.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a handcart of flowers, a prop, keyed off its backdrop and sampled to 31 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fence-iron.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: an iron railing, blue and gold, a prop, keyed off its backdrop and sampled to 68 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/sewer-grate.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a sewer grate, lying on the ground, a prop, keyed off its backdrop and sampled to 23 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chalkboard.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: a café's chalkboard, a prop, keyed off its backdrop and sampled to 19 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chair.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a chair with a pink cushion, a prop, keyed off its backdrop and sampled to 24 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/armchair.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a blue armchair, a prop, keyed off its backdrop and sampled to 26 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/tapestry.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a pink tapestry, hung on a wall, a prop, keyed off its backdrop and sampled to 34 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/anvil.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: an anvil on its block, a prop, keyed off its backdrop and sampled to 31 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/tool-rack.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a smith's rack of tongs, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/desk.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a desk with a map and a candle, a prop, keyed off its backdrop and sampled to 47 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/side-table.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a small table with a candle, a prop, keyed off its backdrop and sampled to 28 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/utensil-rack.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a rack of ladles and tongs over a basin, a prop, keyed off its backdrop and sampled to 31 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/book-crate.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a crate with books on it, a prop, keyed off its backdrop and sampled to 21 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/potted-plant.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a potted lily, a prop, keyed off its backdrop and sampled to 21 art pixels across; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/townhouse.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a tall narrow townhouse: a gabled garret, a floor of flower boxes, a door beside a slit window, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/townhouse-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/townhouse.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/townhouse-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/townhouse.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/townhouse-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/townhouse.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cottage.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: a one-floor cottage under a hipped roof, a gabled porch door between window boxes, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cottage-pink.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/cottage.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cottage-teal.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/cottage.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/cottage-plum.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/cottage.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/turret-house.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a gabled house with a round turret up its corner, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/turret-house-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/turret-house.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/turret-house-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/turret-house.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/turret-house-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/turret-house.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bakery.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the baker's: a shop window under a pink striped awning, a loaf on its hanging sign, a prop put together from parts of the sheet and painted on, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bakery-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/bakery.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bakery-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/bakery.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bakery-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/bakery.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chapel.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: a little chapel: a gothic door under a gable, stained glass under a gable each side, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chapel-pink.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/chapel.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chapel-teal.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/chapel.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/chapel-plum.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/chapel.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/forge.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: the smithy: an open arch with the forge glowing inside, a chimney, the anvil sign on its post, giving off light, a prop put together from parts of the sheet and painted on, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/forge-pink.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/forge.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/forge-teal.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/forge.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/forge-plum.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: Content/Props/forge.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/watch-post.png - built by tools/cut_supplied.py from tools/source/questeria-grand.png and tools/source/questeria-houses.png: the Watch's post: a battlemented guardroom with a blue door, a corner tower, the Watch's banner on its stand, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/warehouse.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: a twin-gabled warehouse for the docks, wide doors and an open doorway, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/warehouse-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/warehouse.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/warehouse-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/warehouse.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/warehouse-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/warehouse.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/gilded-flagon.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the Gilded Flagon inn: two floors, a front gable over its door, a gold flagon on its hanging sign, a prop put together from parts of the sheet and painted on, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/gilded-flagon-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/gilded-flagon.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/gilded-flagon-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/gilded-flagon.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/gilded-flagon-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/gilded-flagon.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rusty-anvil.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the Rusty Anvil, a dockside tavern: two gables, a wide door, its anvil sign on a post, a prop put together from parts of the sheet, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rusty-anvil-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/rusty-anvil.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rusty-anvil-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/rusty-anvil.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rusty-anvil-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/rusty-anvil.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fishmonger.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the fishmonger's: a timber shop front under a blue striped awning, a fish on its sign, a prop put together from parts of the sheet and painted on, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fishmonger-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/fishmonger.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fishmonger-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/fishmonger.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fishmonger-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/fishmonger.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/gilded-flagon-sign.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: the Gilded Flagon's sign on its own post, a gold flagon painted over the smith's sign, a prop put together from parts of the sheet and painted on, sampled at 2.1 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fish-stall.png - built by tools/cut_supplied.py from tools/source/questeria-props-1.png: the fishmonger's stall: the blue striped stall with the day's catch painted on ice, a prop put together from parts of the sheet and painted on, sampled at 2.4 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/net-rack.png - built by tools/cut_supplied.py from tools/source/questeria-props-2.png: a rack of fishing net hung to dry: the washing line with a net painted over it, a prop put together from parts of the sheet and painted on, sampled at 3 sheet pixels an art pixel; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-ivy-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-ivy.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-ivy-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-ivy.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-ivy-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-ivy.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-awning-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-awning.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-awning-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-awning.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-awning-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-awning.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-rose-blue.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-rose.png with a blue roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-rose-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-rose.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-rose-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-rose.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-shop-blue.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-shop.png with a blue roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-shop-pink.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-shop.png with a pink roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-shop-teal.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-shop.png with a teal roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/house-shop-plum.png - built by tools/cut_supplied.py from tools/source/questeria-houses.png: Content/Props/house-shop.png with a plum roof, recoloured by tools/cut_supplied.py; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak-light.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a green oak, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak-pale.png - built by tools/generate_world.py from tools/source/questeria-trees.png: an oak with pale bark, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak-amber.png - built by tools/generate_world.py from tools/source/questeria-trees.png: an oak in autumn, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak-deep.png - built by tools/generate_world.py from tools/source/questeria-trees.png: the oak, deepened for the wood, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fir.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a fir, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fir-deep.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a fir, deepened for the wood, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/stump.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a hollow stump, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/snag.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a broken trunk, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/dead-tree.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a bare dead tree, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bush.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a bush, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bush-deep.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a bush, deepened for the wood, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/bush-small.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a small bush, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/fern.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a fern, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/grass-tall.png - built by tools/generate_world.py from tools/source/questeria-trees.png: a tuft of long grass, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/rocks-mossy.png - built by tools/generate_world.py from tools/source/questeria-trees.png: mossy rocks, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/urn-blue.png - built by tools/generate_world.py from tools/source/questeria-props-2.png: a blue funerary urn in the barrow, a prop, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/log.png - created by Caleb Kiatoukaysi for Legend Quest, a fallen log by the road, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/fallen-tree.png - created by Caleb Kiatoukaysi for Legend Quest, a tree down across the road, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/mushrooms.png - created by Caleb Kiatoukaysi for Legend Quest, red toadstools, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/mushrooms-pale.png - created by Caleb Kiatoukaysi for Legend Quest, pale blue toadstools, drawn procedurally by tools/generate_world.py, released under public domain

Content/People/elder.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the elder, template D1, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/trader.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the trader, template D3, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/baker.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the baker, template A2, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/smith.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the smith, template B1, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/flower-seller.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the flower seller, template C2, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/traveller.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: a traveller, template A4, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/seamstress.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the seamstress, template A5, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/courier.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: a courier, template B4, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/pilgrim.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: a pilgrim, template C1, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/weaver.png - built by tools/cut_supplied.py from tools/source/npc-customization.png: the weaver, template D2, the one picture each way, in all five columns, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/farmhand.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: a farmhand, set 1 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/old-man.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: an old man, set 2 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/maid.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: a maid, set 3 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/porter.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: a porter, set 4 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/old-woman.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: an old woman, set 5 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/child.png - built by tools/cut_supplied.py from tools/source/npc-villagers.png: a child, set 6 of the six, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/People/youth.png - built by tools/cut_supplied.py from tools/source/npc-townsperson.png: a youth, standing and four steps each way, sampled to 32x48 frames with the feet on the same row (the first five columns of the 10x4 layout); the picture was supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/art-guide.png - the 2D art style and generation guide the characters and creatures are drawn to (proportions, line, cel shading, light, palette, modular parts, the creatures), supplied by Caleb Kiatoukaysi; nothing's cut from it, but tools/rig/theme.py samples its palette
tools/source/characters/base-male.png - the base man every male armor, cape, helm, weapon and face is painted onto as an edit: a bald, cel-shaded figure in a three-quarter stance facing right, fists clenched, in dark shorts, on solid black, 1024x1536; cut from the owner's pair picture (scaled 1.345x with Lanczos3, centred, feet at y 1466); supplied by Caleb Kiatoukaysi for Legend Quest, made in ChatGPT
tools/source/characters/base-female.png - the base woman every female armor, cape, helm, weapon and face is painted onto as an edit: a bald, cel-shaded figure in a three-quarter stance facing right, fists clenched, in dark underwear, on solid black, 1024x1536; cut from the owner's pair picture (scaled 1.345x with Lanczos3, centred, feet at y 1466); supplied by Caleb Kiatoukaysi for Legend Quest, made in ChatGPT
tools/source/characters/base-male-blank.png - the blank-faced base man every male face, hair and helm is painted onto: tools/source/characters/base-male.png with its eyes, brows and mouth taken off, the head's outline, ears and jaw left where they were, and the head everyone's drawn with; supplied by Caleb Kiatoukaysi for Legend Quest, made in ChatGPT as an edit of the base
tools/source/characters/base-female-blank.png - the blank-faced base woman every female face, hair and helm is painted onto: tools/source/characters/base-female.png with its eyes, brows and mouth taken off, the head's outline, ears and jaw left where they were, and the head everyone's drawn with; supplied by Caleb Kiatoukaysi for Legend Quest, made in ChatGPT as an edit of the base
tools/source/style-sheet.png - the palette and a mock-up of Questeria's plaza, the look the rest were made to match, supplied by Caleb Kiatoukaysi for Legend Quest; nothing's cut from it, but the sizes in tools/cut_supplied.py come from it

tools/source/questeria-ground.png - Questeria's ground: paving, flagstones, cobbles, grass, sand and water, a cell each, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts the tileset and the title's petal from it

tools/source/questeria-garden.png - garden pieces: bushes, planters, water, lamps, banners, walls and bridges, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts the garden wall and the small cherry tree from it

tools/source/questeria-houses.png - the houses and the guildhall, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts them from it

tools/source/questeria-grand.png - the palace, the temple and the city gate, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts them from it

tools/source/questeria-trees.png - trees, bushes, flowers and rocks, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts the green from it

tools/source/questeria-props-1.png - the plaza's and the market's things: the fountain, statues, lamps, benches, stalls and the waypoint crystal, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts them from it

tools/source/questeria-props-2.png - chests, doors, crates, barrels and furniture in the same style, supplied by Caleb Kiatoukaysi for Legend Quest; nothing's cut from it yet

tools/source/title-sky.png - the title's sky, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-clouds.png - the clouds that drift across the title, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-far.png - the far distance, floating islands over mountains, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts its biggest island from it

tools/source/title-city.png - Questeria on its cliffs, for the title, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-foreground.png - the terrace and blossom trees at the front of the title, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-banner.png - a banner blowing, in eight pictures, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-waterfall.png - a floating island's waterfall, in six pictures, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/title-spire.png - the crystal spire's light, in eight pictures, supplied by Caleb Kiatoukaysi for Legend Quest

tools/source/npc-villagers.png - townsfolk: heads, hair, clothes, a row of people facing front, and six people walking each way, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts the six from it

tools/source/npc-townsperson.png - a young man standing and walking each way, supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts the youth from it

tools/source/npc-customization.png - skin, faces, hair, clothes and twenty people each way (templates A1 to D5), supplied by Caleb Kiatoukaysi for Legend Quest; tools/cut_supplied.py cuts ten of them from it, and leaves out A1, B5 and D5, who change when they turn round

Content/Props/tree-blackvein.png - built by tools/generate_mirewood.py from tools/source/questeria-trees.png: an oak gone grey with black veins through its bark and its leaves, a prop in the Mirewood, keyed off its backdrop, sampled down and turned; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/tree-blackvein-pale.png - built by tools/generate_mirewood.py from tools/source/questeria-trees.png: an oak with pale bark gone grey, black-veined, a prop in the Mirewood, keyed off its backdrop, sampled down and turned; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/tree-blackvein-bare.png - built by tools/generate_mirewood.py from tools/source/questeria-trees.png: a bare dead tree, black-veined, a prop in the Mirewood, keyed off its backdrop, sampled down and turned; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/oak-turning.png - built by tools/generate_mirewood.py from tools/source/questeria-trees.png: the deep oak beginning to turn, a vein up its trunk, a prop in the Mirewood, keyed off its backdrop, sampled down and turned; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/great-oak.png - built by tools/generate_meadow.py from tools/source/questeria-trees.png: the great oak, the oak with two more crowns behind it and the widest trunk in front, put together from the sheet's parts, a prop on the old road, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/wildflowers.png - built by tools/generate_meadow.py from tools/source/questeria-trees.png: lupins, a prop in the meadows, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/wildflowers-yellow.png - built by tools/generate_meadow.py from tools/source/questeria-trees.png: tall cream and yellow flowers, a prop in the meadows, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/wildflowers-blue.png - built by tools/generate_meadow.py from tools/source/questeria-trees.png: a clump of blue flowers, a prop in the meadows, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/east-gate.png - built by tools/generate_meadow.py from tools/source/questeria-grand.png: Questeria's east gate, the city gate with the road through it cleared so whoever walks it shows, a prop at Thornbridge, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/city-wall.png - built by tools/generate_meadow.py from tools/source/questeria-grand.png: a stretch of the city wall with flowers along it, a prop at Thornbridge, keyed off its backdrop and sampled down; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

Content/Props/farmhouse.png - built by tools/generate_meadow.py from tools/source/questeria-houses.png and tools/source/questeria-grand.png: the farmhouse, the cottage's parts put together with a chimney on its roof, and the roof turned red, a prop on the old road; the pictures were supplied by Caleb Kiatoukaysi for Legend Quest

## SRD 5.1

The rules use a few things from the 5e System Reference Document: the six ability scores and how a
score turns into a modifier, the class names Fighter, Rogue, Cleric and Wizard with their hit dice,
hit points at first level, and the standard array the party's scores are spread from. The names of
the people and places are mine.

This work includes material taken from the System Reference Document 5.1 ("SRD 5.1") by Wizards of the Coast LLC and available at https://dnd.wizards.com/resources/systems-reference-document. The SRD 5.1 is licensed under the Creative Commons Attribution 4.0 International License available at https://creativecommons.org/licenses/by/4.0/legalcode.

## From the MonoGame template

MonoGame.Framework.DesktopGL and MonoGame.Content.Builder.Task - the framework the game is built on and the content pipeline that builds Content/, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

Content/Content.mgcb - the content pipeline's list of what to build and copy, started from the MonoGame DesktopGL project template and filled in by Caleb Kiatoukaysi for Legend Quest

Icon.ico - MonoGame project icon, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

Icon.bmp - MonoGame project icon, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

app.manifest - the Windows application manifest, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)
