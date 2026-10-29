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

Content/Paperdoll/body.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: the youth with his hair taken off, every part of him (skin, eyes, shirt, vest, breeches) cut down to a few key shades for the palette swap; standing and four steps each way, 32x48 frames; the picture was supplied by Caleb Kiatoukaysi for Legend Quest

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

## Tiles and props

Content/Tiles/questeria.png - created by Caleb Kiatoukaysi for Legend Quest, Questeria's and the harbour's tileset (plaza, terrace, cobble, grass, sand, water, planks), seen straight down and lit from the top left, with the edges the tilemap draws where one meets another (a lawn's kerb, the quay wall, the boardwalk's beam), drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/ship.png - created by Caleb Kiatoukaysi for Legend Quest, the ship you arrive on, side on with its cabin at the stern, its sail full and then furled, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/gangplank.png - created by Caleb Kiatoukaysi for Legend Quest, the ship's gangplank down to the boardwalk, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/rowboat.png - created by Caleb Kiatoukaysi for Legend Quest, a rowing boat moored by the jetty, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Tiles/crypt.png - created by Caleb Kiatoukaysi for Legend Quest, the barrow's tileset: cold slate floors of every size of slab, worn and in shadow, steps, the walls from above and from the front with carvings and niches, rubble, bones and cobwebs, drawn procedurally by tools/generate_world.py, released under public domain

Content/Tiles/forest.png - created by Caleb Kiatoukaysi for Legend Quest, the old road's tileset: grass, dead leaves, the road with its ruts, the undergrowth under the wood, and moss, tufts, ferns, flowers, pebbles, roots and litter to lie on them, drawn procedurally by tools/generate_world.py, released under public domain

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

Content/Tiles/ship.png - created by Caleb Kiatoukaysi for Legend Quest, the Gull's Wager's deck (boards, the bulwark, the hatch, the sea round it with foam at the hull), drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/mast.png - created by Caleb Kiatoukaysi for Legend Quest, the Gull's Wager's mainmast, its sail full and its crow's nest at the top, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/lantern-hanging.png - created by Caleb Kiatoukaysi for Legend Quest, a ship's lantern on a post, a prop that gives off light, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/ship-wheel.png - created by Caleb Kiatoukaysi for Legend Quest, the ship's wheel, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/rope-coil.png - created by Caleb Kiatoukaysi for Legend Quest, a coil of rope on the deck, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Tiles/interior.png - created by Caleb Kiatoukaysi for Legend Quest, inside Questeria's buildings (wood and stone floors, the rug and its border, the back wall's face and its panelling, the walls' tops), drawn procedurally by tools/generate_interiors.py, released under public domain

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

## UI

Content/button.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron plate every button is drawn with and the light in its groove, drawn procedurally by tools/generate_ui.py, released under public domain

Content/panel.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron panel the forms sit on, drawn procedurally by tools/generate_ui.py, released under public domain

Content/map-marks.png - created by Caleb Kiatoukaysi for Legend Quest, the marks on the map screen (places, where you are, waypoints, roads), drawn procedurally by tools/generate_ui.py, released under public domain

Content/markers.png - created by Caleb Kiatoukaysi for Legend Quest, the gold and red "!" markers over people with a quest or an event and the arrows to them at the screen's edge, drawn procedurally by tools/generate_ui.py, released under public domain

Content/dice.png - created by Caleb Kiatoukaysi for Legend Quest, the d20 for checks, eight frames tumbling and one landed, drawn procedurally by tools/generate_ui.py, released under public domain

Content/dialogue-icons.png - created by Caleb Kiatoukaysi for Legend Quest, the icons beside options in a conversation (a die, a heart, a lock, and the three voices), drawn procedurally by tools/generate_ui.py, released under public domain

Content/cursors.png - created by Caleb Kiatoukaysi for Legend Quest, the mouse cursors: an arrow, a ring to walk, a hand to use, a speech bubble to talk, and a crossed circle, drawn procedurally by tools/generate_ui.py, released under public domain

## Maps

Content/Maps/questeria.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria, the capital and the hub map, released under public domain

Content/Maps/docks.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria's harbour, where a new game opens with the ship coming in, released under public domain

Content/Maps/road.map - written by Caleb Kiatoukaysi for Legend Quest, the old road through the forest, at dusk, released under public domain

Content/Maps/barrow.map - written by Caleb Kiatoukaysi for Legend Quest, the barrow, a dark map lit by torches, released under public domain

Content/Maps/gulls-wager.map - written by Caleb Kiatoukaysi for Legend Quest, the deck of the Gull's Wager at night, where the Prologue's voyage plays, released under public domain

Content/Maps/gilded-flagon.map - written by Caleb Kiatoukaysi for Legend Quest, inside the Gilded Flagon, the Lantern Market's tavern, released under public domain

Content/Maps/cottage-a.map - written by Caleb Kiatoukaysi for Legend Quest, inside a cottage: a bed, a wardrobe, a bookshelf and a table, released under public domain

Content/Maps/cottage-b.map - written by Caleb Kiatoukaysi for Legend Quest, inside another cottage, with a hearth, released under public domain

Content/Maps/shop.map - written by Caleb Kiatoukaysi for Legend Quest, inside the chandler's shop, released under public domain

Content/Maps/warehouse.map - written by Caleb Kiatoukaysi for Legend Quest, inside a warehouse on the docks, released under public domain

Content/Maps/bakery.map - written by Caleb Kiatoukaysi for Legend Quest, inside the bakery in the Lantern Market: its counters, the shelves of loaves, the sacks of flour and the oven, released under public domain

Content/Maps/forge.map - written by Caleb Kiatoukaysi for Legend Quest, inside the forge in the Lantern Market: the anvil, the tool racks and the forge fire, released under public domain

Content/Maps/chapel.map - written by Caleb Kiatoukaysi for Legend Quest, inside the chapel by the temple: benches for pews, candles and a tapestry, released under public domain

## Music

Content/Music/embers.wav - created by Caleb Kiatoukaysi for Legend Quest, "Embers", the title theme, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/hearthsmoke.wav - created by Caleb Kiatoukaysi for Legend Quest, "Hearthsmoke", for walking about on a lit map, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/cold-stone.wav - created by Caleb Kiatoukaysi for Legend Quest, "Cold Stone", for walking about underground, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/salt-and-lanterns.wav - created by Caleb Kiatoukaysi for Legend Quest, "Salt and Lanterns", for the Gull's Wager at sea, composed and synthesised by tools/generate_audio.py, released under public domain

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

## Text

Content/Spectral/Spectral-Light.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/Spectral-Medium.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/OFL.txt - the SIL Open Font License text and copyright notice for Spectral, redistributed alongside the font as that licence requires (copied into the build output, so it ships with the game)

Content/spectral-title.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; names Spectral Light at 92pt by relative path, so the build uses the font shipped in Content/Spectral/ rather than whatever is installed

Content/spectral-ui.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Medium at 26pt, letterspaced

Content/spectral-detail.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Light at 17pt, letterspaced

## The scripts

tools/pixels.py, tools/generate_ui.py, tools/generate_world.py, tools/generate_harbour.py, tools/generate_interiors.py, tools/generate_paintings.py, tools/generate_audio.py - every asset above that says "drawn procedurally" or "synthesised" comes out of these; pixels.py is the PNG writer and colour helpers the art scripts share. They need nothing but CPython (3.12 or newer for generate_audio.py), and every one is seeded, so a rerun gives the same files

tools/generate_paperdoll.py - written by Caleb Kiatoukaysi for Legend Quest, takes the youth off tools/source/npc-townsperson.png (through cut_supplied.py) apart into the paperdoll: sorts every pixel of him into what it's part of, cuts each part down to key shades, and draws the other hair, the faces, the beards, the skirt and the cape onto his frames. It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/cut_supplied.py - written by Caleb Kiatoukaysi for Legend Quest, cuts the pictures in tools/source/ into the title's layers and loops, Questeria's and the harbour's props and the townsfolk's sheets (see The pictures the generators read). It needs nothing but CPython, and there's nothing random in it, so a rerun gives the same files

tools/check_assets.py - written by Caleb Kiatoukaysi for Legend Quest, checks this file, Content/Content.mgcb and the files on disk against each other

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
