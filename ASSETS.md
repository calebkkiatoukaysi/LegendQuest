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

Content/character-palettes.png - built by tools/generate_paperdoll.py from tools/source/npc-townsperson.png: every preset the palette swap can recolour a sheet to, one ramp a row, the youth's own keys first in each block (skin, hair, eyes, shirt, vest, legs, cloak); the other presets were chosen by Caleb Kiatoukaysi for Legend Quest

## Tiles and props

Content/Tiles/questeria.png - created by Caleb Kiatoukaysi for Legend Quest, Questeria's and the harbour's tileset (plaza, terrace, cobble, grass, sand, water, planks), seen straight down and lit from the top left, with the edges the tilemap draws where one meets another (a lawn's kerb, the quay wall, the boardwalk's beam), drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/ship.png - created by Caleb Kiatoukaysi for Legend Quest, the ship you arrive on, side on with its cabin at the stern, its sail full and then furled, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/gangplank.png - created by Caleb Kiatoukaysi for Legend Quest, the ship's gangplank down to the boardwalk, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Props/rowboat.png - created by Caleb Kiatoukaysi for Legend Quest, a rowing boat moored by the jetty, drawn procedurally by tools/generate_harbour.py, released under public domain

Content/Tiles/crypt.png - created by Caleb Kiatoukaysi for Legend Quest, the barrow's tileset: floors, steps, walls from above and from the front, rubble, bones and a cobweb, drawn procedurally by tools/generate_world.py, released under public domain

Content/Tiles/forest.png - created by Caleb Kiatoukaysi for Legend Quest, the old road's tileset: dead leaves, the road, undergrowth, moss, grass, ferns, a rock, a stump, a fallen log, a tree trunk, bushes, and the nine pieces of a tree's crown, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/barrel.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/crate.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/urn.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sarcophagus.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sconce.png - created by Caleb Kiatoukaysi for Legend Quest, the iron sconce a torch stands in, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/campfire.png - created by Caleb Kiatoukaysi for Legend Quest, a ring of stones round two burning logs, a prop (the game draws the flame on it), drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/bedroll.png - created by Caleb Kiatoukaysi for Legend Quest, a blanket laid out by the campfire, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/chest.png - created by Caleb Kiatoukaysi for Legend Quest, a chest shut and open, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/door.png - created by Caleb Kiatoukaysi for Legend Quest, a double door in its stone frame, shut and open, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/gate.png - created by Caleb Kiatoukaysi for Legend Quest, a stone door in its frame, shut and sunk into the floor, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/lever.png - created by Caleb Kiatoukaysi for Legend Quest, a lever on a wall plate, up and down, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sign.png - created by Caleb Kiatoukaysi for Legend Quest, a fingerpost, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/waypoint.png - created by Caleb Kiatoukaysi for Legend Quest, a waypoint stone with its mark dark and then lit, drawn procedurally by tools/generate_world.py, released under public domain

## UI

Content/button.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron plate every button is drawn with and the light in its groove, drawn procedurally by tools/generate_ui.py, released under public domain

Content/panel.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron panel the forms sit on, drawn procedurally by tools/generate_ui.py, released under public domain

Content/map-marks.png - created by Caleb Kiatoukaysi for Legend Quest, the marks on the map screen (places, where you are, waypoints, roads), drawn procedurally by tools/generate_ui.py, released under public domain

Content/cursors.png - created by Caleb Kiatoukaysi for Legend Quest, the mouse cursors: an arrow, a ring to walk, a hand to use, a speech bubble to talk, and a crossed circle, drawn procedurally by tools/generate_ui.py, released under public domain

## Maps

Content/Maps/questeria.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria, the capital and the hub map, released under public domain

Content/Maps/docks.map - written by Caleb Kiatoukaysi for Legend Quest, Questeria's harbour, where a new game opens with the ship coming in, released under public domain

Content/Maps/road.map - written by Caleb Kiatoukaysi for Legend Quest, the old road through the forest, at dusk, released under public domain

Content/Maps/barrow.map - written by Caleb Kiatoukaysi for Legend Quest, the barrow, a dark map lit by torches, released under public domain

## Music

Content/Music/embers.wav - created by Caleb Kiatoukaysi for Legend Quest, "Embers", the title theme, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/hearthsmoke.wav - created by Caleb Kiatoukaysi for Legend Quest, "Hearthsmoke", for walking about on a lit map, composed and synthesised by tools/generate_audio.py, released under public domain

Content/Music/cold-stone.wav - created by Caleb Kiatoukaysi for Legend Quest, "Cold Stone", for walking about underground, composed and synthesised by tools/generate_audio.py, released under public domain

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

## Text

Content/Spectral/Spectral-Light.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/Spectral-Medium.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/OFL.txt - the SIL Open Font License text and copyright notice for Spectral, redistributed alongside the font as that licence requires (copied into the build output, so it ships with the game)

Content/spectral-title.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; names Spectral Light at 92pt by relative path, so the build uses the font shipped in Content/Spectral/ rather than whatever is installed

Content/spectral-ui.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Medium at 26pt, letterspaced

Content/spectral-detail.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Light at 17pt, letterspaced

## The scripts

tools/pixels.py, tools/generate_ui.py, tools/generate_world.py, tools/generate_harbour.py, tools/generate_audio.py - every asset above that says "drawn procedurally" or "synthesised" comes out of these; pixels.py is the PNG writer and colour helpers the art scripts share. They need nothing but CPython (3.12 or newer for generate_audio.py), and every one is seeded, so a rerun gives the same files

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
