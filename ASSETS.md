# Assets

Every file the game ships or reads, and where it came from. Nothing here was downloaded or
recorded: the art is drawn by `tools/generate_ui.py`, `tools/generate_world.py` and
`tools/generate_characters.py`, the music and sound effects are synthesised by
`tools/generate_audio.py`, the maps are written by hand, and the only outside work is the font and
the MonoGame template files. `python tools/check_assets.py` fails if this list and the files on
disk ever disagree.

## Sprites

Content/title-scene.png - created by Caleb Kiatoukaysi for Legend Quest, the gate at night behind the title screen, lit by two torches, drawn procedurally by tools/generate_world.py, released under public domain

Content/torch-flame.png - created by Caleb Kiatoukaysi for Legend Quest, six frames of a torch flame, drawn procedurally by tools/generate_world.py, released under public domain

Content/glow.png - created by Caleb Kiatoukaysi for Legend Quest, the soft white light every glow is drawn with, drawn procedurally by tools/generate_world.py, released under public domain

Content/mote.png - created by Caleb Kiatoukaysi for Legend Quest, the small soft dot an ember is drawn with, drawn procedurally by tools/generate_world.py, released under public domain

## Characters and armour

Content/Paperdoll/body-lean.png - created by Caleb Kiatoukaysi for Legend Quest, the lean body in smallclothes, the bottom layer of the paperdoll, in the 10x4 sheet layout and painted in the skin keys, drawn procedurally by tools/generate_characters.py, released under public domain

## Tiles and props

Content/Tiles/village.png - created by Caleb Kiatoukaysi for Legend Quest, the village tileset: grass, dirt, cobbles, fences, house fronts and thatch, drawn procedurally by tools/generate_world.py, released under public domain

Content/Tiles/crypt.png - created by Caleb Kiatoukaysi for Legend Quest, the barrow's tileset: floors, steps, walls from above and from the front, rubble, bones and a cobweb, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/barrel.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/crate.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/well.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/urn.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sarcophagus.png - created by Caleb Kiatoukaysi for Legend Quest, a prop, drawn procedurally by tools/generate_world.py, released under public domain

Content/Props/sconce.png - created by Caleb Kiatoukaysi for Legend Quest, the iron sconce a torch stands in, a prop, drawn procedurally by tools/generate_world.py, released under public domain

## UI

Content/button.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron plate every button is drawn with and the light in its groove, drawn procedurally by tools/generate_ui.py, released under public domain

Content/panel.png - created by Caleb Kiatoukaysi for Legend Quest, the nine-sliced iron panel the forms sit on, drawn procedurally by tools/generate_ui.py, released under public domain

## Maps

Content/Maps/village.map - written by Caleb Kiatoukaysi for Legend Quest, the village, the hub map, released under public domain

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

## Text

Content/Spectral/Spectral-Light.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/Spectral-Medium.ttf - Spectral font created by Production Type, released on Google Fonts (https://fonts.google.com/specimen/Spectral) under the SIL Open Font License

Content/Spectral/OFL.txt - the SIL Open Font License text and copyright notice for Spectral, redistributed alongside the font as that licence requires (copied into the build output, so it ships with the game)

Content/spectral-title.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; names Spectral Light at 92pt by relative path, so the build uses the font shipped in Content/Spectral/ rather than whatever is installed

Content/spectral-ui.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Medium at 26pt, letterspaced

Content/spectral-detail.spritefont - written by Caleb Kiatoukaysi for Legend Quest, from the MonoGame SpriteFont template; Spectral Light at 17pt, letterspaced

## The scripts

tools/pixels.py, tools/generate_ui.py, tools/generate_world.py, tools/generate_characters.py, tools/generate_audio.py - every asset above that says "drawn procedurally" or "synthesised" comes out of these; pixels.py is the PNG writer and colour helpers the art scripts share. They need nothing but CPython (3.12 or newer for generate_audio.py), and every one is seeded, so a rerun gives the same files

tools/check_assets.py - written by Caleb Kiatoukaysi for Legend Quest, checks this file, Content/Content.mgcb and the files on disk against each other

## From the MonoGame template

MonoGame.Framework.DesktopGL and MonoGame.Content.Builder.Task - the framework the game is built on and the content pipeline that builds Content/, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

Content/Content.mgcb - the content pipeline's list of what to build and copy, started from the MonoGame DesktopGL project template and filled in by Caleb Kiatoukaysi for Legend Quest

Icon.ico - MonoGame project icon, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

Icon.bmp - MonoGame project icon, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)

app.manifest - the Windows application manifest, unmodified from the MonoGame DesktopGL project template, created by the MonoGame Team (https://www.monogame.net/) and distributed under the Microsoft Public License (https://licenses.nuget.org/MS-PL)
