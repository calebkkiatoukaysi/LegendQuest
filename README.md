# Legend Quest

A MonoGame project for CIS 580 - Foundations of Game Programming, Kansas State University.

Legend Quest is a top-down, turn-based fantasy RPG. You make a character, gather a party of up to
four, walk the maps in real time, talk your way through conversations where the risky options
roll a d20 on screen, and fight turn by turn when talking doesn't work. What each character wears
shows on them as they walk. The rules underneath will be 5e's, from the SRD 5.1.

This is the skeleton: every system working end to end on a little content, built so maps,
characters, items and dialogue can keep being added without rewriting anything. It's being built
in milestones, and right now the first two are done: the screens around the game, the sound, the
saves and the proof run, and then the maps, walking round them, and the dark.
[Still to come](#still-to-come) lists the rest.

## Running it

The window is 1600x900. The art is drawn small and blown up by a whole number so the pixels stay
square: 3x for the buttons, the panels and the world, 4x for the gate behind the title, which is
400x225 art pixels and fills the window exactly. The mouse wheel zooms the world between 2x, 3x and
4x, never anything in between.

```
dotnet run
```

**NEW GAME** opens the save slots, and picking one starts you in the village. **LOAD** opens them
to carry a game on, **CONTINUE** carries on whichever slot was saved last (it's dead when there
isn't one), **OPTIONS** has the volumes and the text speed, and **EXIT** and **Esc** close the
game. On a map, Esc is the pause menu.

One switch exists for working on the game, and it isn't the game:

```
dotnet run -- --proof shots       # play through the screens, save a picture of each to shots/, and quit
```

It's the same harness as The Black Box's. It goes through the title, the options, the save slots
for loading (and erases a broken slot the way a player would, ERASE then SURE?), the save slots for
a new game, then a new game in the village: up the road, round behind a roof, both sides of a
barrel, into a wall at an angle, and both zooms. Then the barrow: the way in, the dark passage, the
hall, right under a torch, the pause menu, and out through QUIT TO TITLE. It uses a scratch save
folder inside `shots/` that it fills with one of every kind of slot, so it never touches a real one,
and it ignores the real mouse and keyboard so nothing I do while it runs changes the pictures. It's
silent: every sound it would have played goes in `shots/audio.log` with the frame it was asked for
on, which is how I check the footsteps change from grass to stone. A second switch, `--simulate`,
comes with combat.

### Controls

| Where | Keys |
| --- | --- |
| Menus and forms | **Arrows** or **WASD** to move, **Enter** or **Space** to pick, **Esc** to go back. The mouse works too |
| Options | **Left** and **right** change the selected row |
| Save slots | **Left** and **right** move between a slot and its ERASE |
| On a map | **WASD** or the **arrows** to walk, the **mouse wheel** to zoom, **Esc** to pause |

A gamepad works anywhere the keyboard does: the d-pad or left stick, A, B, and Start.

## Screens

Every screen is a `GameScreen` on a `ScreenManager`, the way the game state management tutorial
does it (`StateManagement/` is that tutorial's code, carried over from The Black Box). Each has its
own `TransitionOnTime` and `TransitionOffTime`, so everything fades instead of cutting.

| Screen | What it is |
| --- | --- |
| `TitleBackgroundScreen` | The gate at night behind the title and every form on it. It doesn't transition off when something covers it, so it stays up behind all of them |
| `TitleScreen` | The title, with NEW GAME, CONTINUE, LOAD, OPTIONS and EXIT |
| `OptionsScreen` | Master, music and sound volume, and how fast text types out. Saved to `settings.json` |
| `SaveSlotScreen` | The save slots, for a new game or for loading one, with ERASE beside each slot that has something in it |
| `ExploreScreen` | A map being walked round: the tiles, the props, you, and the dark if it's a dark map |
| `PauseMenuScreen` | RESUME, OPTIONS or QUIT TO TITLE. Nothing under it moves while it's up. SAVE and LOAD go on it once there's a game to save |
| `LoadingScreen` | From the tutorial. Waits for everything to fade out, then adds the next screens. Going onto a map puts its name on the black in between |

## The title

The gate is one picture, `title-scene.png`, drawn by `tools/generate_world.py`: a crenellated
wall against the night, an arched gate with a barred door, flagstones in front, and an iron sconce
either side. The light in it is worked out in 3D (the wall stands up, the floor lies down, the
flames stand a little off the wall) and then cut into dithered bands, so the pools read as pixel
art and not a smooth gradient. A thin line of light gets out under the door.

The flames aren't in the picture. Each torch is a `TorchSprite`: six frames of flame, and the glow
texture drawn additively over the stone with a flicker made of three sine waves at speeds that
never line up. Embers come off the tips (`EmberParticleSystem`, on the particle system tutorial's
engine), rising, wandering, flickering, and cooling from yellow to a dull red.

## Maps

A map is a text file in `Content/Maps`, copied next to the game by `Content.mgcb` and read with
`TitleContainer.OpenStream`. It's my tilemap exercise grown up: that one read a tileset name, a
tile size, a map size and a list of tile ids, and this reads the same things out of sections, with
the tiles laid out as ASCII the way The Black Box's lobby was, so I can see the map while I edit
it. `MapCatalog` reads every map when the game starts, so a mistake in one stops the game right
there with the file and the line, instead of when somebody walks onto it.

```
; A comment. Blank lines don't matter, and every line is trimmed.
[map]
id = barrow              ; has to match the file name
name = The Barrow        ; what the heading and the loading screen say
tileset = crypt          ; from TilesetCatalog
size = 40 30             ; width and height in tiles; every layer has to be exactly this
dark = yes
ambient = 0.07           ; how much light gets everywhere (dark maps only)
music = Underground      ; a Track name

[legend]                 ; one character per tile, by the tile's name in the tileset
, = floor
# = wall-top

[ground]                 ; every cell needs a tile
[detail]                 ; things on the floor; . is an empty cell
[overhead]               ; roofs and such, drawn over everyone; . is an empty cell

[objects]                ; kind id x y, in tiles; things stand on the middle of the tile
spawn start 19.5 27
prop sconce 11 13
```

Those are the two kinds of object so far: `spawn` is somewhere the party can arrive (every map
needs one called `start`), and `prop` is anything out of `PropCatalog`. Halves are fine for x and y.
The loader keeps the two fixes from the exercise: it trims every line (the exercise left the `\r`
from Windows line endings on each one), and it checks every row against the map's size (the
exercise took the id list on trust, and a short one read straight past the end). It also refuses a
tile that isn't in the legend, a legend tile that isn't in the tileset, a prop that isn't in the
catalogue, an object off the edge of the map, and `ambient` on a map that isn't dark.

- **Tiles.** Ids count from 1 and 0 is empty, the same as the exercise. A tileset lists its tiles
  in order, and each one says how many variants it has, whether it's solid and whether it sounds
  like grass or stone. The map picks a variant per cell from where the cell is, so a field of grass
  doesn't repeat, and it's the same every time.
- **Walls.** Collision comes from the tiles: a cell is solid if its ground or detail tile is, and
  each row's runs of solid cells merge into one `BoundingRectangle` (the collision tutorial's).
  There's a wall all the way round outside the edge too. Walking moves the feet one axis at a time
  and pushes them back out of anything they hit, exactly like the lobby, so a wall stops you one way
  and lets you slide along it the other. The feet box is 10 by 4 art pixels, narrower than a tile,
  so a one-tile gap can be walked through without lining up perfectly.
- **Props.** A prop stands on its foot like a person does and blocks the floor under it (its
  footprint). Floor props sort with the people; wall props, like the torch sconces, go behind
  everyone. A sconce carries a light: where its flame goes and where its pool lands.
- **The camera** is The Black Box's `Camera2D`, except the world is in art pixels now and the
  camera does the scaling, so the zoom is just a whole number. It eases after you, stops at the
  edge of the map (or centres the map if it's smaller than the view), and rounds itself to whole
  screen pixels. The walker snaps to whole screen pixels too, or it crawls.
- **Sorting.** One world batch, back to front. The ground and detail tiles are at the back, wall
  props in front of those, then everyone on the floor sorted by how far down the map their feet
  are, then the overhead tiles. The houses use that: the thatch hangs over the two rows of ground
  behind each house, so walking along the back of one, you go under the roof.
- **The dark.** A dark map gets The Black Box's lobby trick: one low resolution texture of darkness
  with the pools of light cut out of it, built once when the map loads, stretched over the map with
  smooth sampling and drawn after everything, so you go dark when you walk out of the light. What's
  new is that light stops at walls. Each texel walks the tiles between itself and the light and
  looks for a solid one. A wall's face takes its light from the floor in front of it, two rows
  down at most, because my first go walked the line from the face itself, every face shut its own
  torch out of the floor beside it, and the pools came out square. The torches flicker without
  rebuilding anything: the glow texture goes over each pool additively, with the same flicker as
  the title.

| Map | What it is |
| --- | --- |
| `village.map` | The hub, lit. The elder's house on the north side of the square, the well in the middle, four more houses, a garden, a road south out of the village and one east toward the old road |
| `barrow.map` | Dark, five torches. In up the steps, through a short dark passage into the hall, the side room off it to the west, and the tomb at the back |

There's no way between the two yet; exits come next. The proof run goes to the barrow by itself.

## The walker

Every character sheet is one layout, so the paperdoll's layers can stack: 32x48 frames (the same
size as The Black Box's, so its cutting scripts still fit), ten columns and four rows, 320x192 in
all, with the feet at (16, 46) in every frame.

| Columns | | Rows |
| --- | --- | --- |
| 0 | Standing | 0 facing down |
| 1-4 | Walking: right foot forward, passing, left foot forward, passing (a pixel higher, which is the bounce) | 1 facing left |
| 5 | Wind-up | 2 facing right |
| 6 | Strike | 3 facing up |
| 7 | Cast | |
| 8 | Hurt | |
| 9 | Downed, lying on their back | |

`WalkerSprite` is The Black Box's on the new layout: it cuts frames from a sheet, walks through
columns 1 to 4, and fires `Stepped` on 1 and 3, when a foot lands, so the footsteps match the feet.
The footstep is grass or stone depending on the ground tile under you. For now you walk round as
just the body, in smallclothes; the paperdoll puts hair, a face and clothes on it later.

The sheets are drawn off one rig in `tools/generate_characters.py`: where the head, shoulders,
elbows, hands, hips, knees and feet are in every frame. A layer is shapes hung off those joints, so
a sleeve or a helmet on another sheet lands on the same arm or head. Facing right isn't facing left
mirrored, the way The Black Box did it: the sword hand is the right one either way, so it's the near
arm facing right and the far one facing left.

## Saving

Five slots, one JSON file each, under `%LOCALAPPDATA%\LegendQuest\Saves`: three numbered ones
(`slot0.json` to `slot2.json`), `quicksave.json` and `autosave.json`. Not next to the executable,
because `bin/` gets deleted by every clean build.

| File | What it is |
| --- | --- |
| `SaveData` | The whole save. Small for now (the map and the play time), and it grows with the game: each system adds its field when it exists |
| `SaveSlot` | One slot as the form sees it: empty, occupied or unreadable, plus the line printed on it |
| `SaveSystem` | Reads, writes and erases slots. The only thing in the project that touches a save file |

The same three decisions as The Black Box:

- **Writes are atomic.** A save goes to a `.tmp` first and only then replaces the real file, so a
  crash mid-write leaves the old save intact. The one it replaced is kept as a `.bak`.
- **Nothing throws.** A missing, locked or corrupt file comes back as a slot that says UNREADABLE
  and can be erased. So does a save from a newer build of the game, since writing it back would
  quietly drop the fields this build doesn't know about, and so does a save on a map this build
  doesn't have.
- **Every file carries a `Version`.** `SaveSystem.Upgrade` is where an old file gets brought
  forward when a field changes. Nothing has needed it yet.

A new game only goes in a numbered slot, and it's written as soon as you pick one, so if the slot
can't be written the form can say so. Picking one that's full asks first (PICK IT AGAIN TO START
OVER IT). ERASE asks too: the button turns into SURE?. Nothing saves while you play yet; that comes
with the autosave between maps.

The volumes and the text speed aren't in a slot. They're in `%LOCALAPPDATA%\LegendQuest\settings.json`,
since turning the music down shouldn't depend on which game is loaded.

## Music and sound

None of it was recorded or downloaded. `tools/generate_audio.py` builds every song and sound from
sines, wavetables, noise and a few filters, in CPython (3.12 or newer) and nothing else. The
oscillators, filters, reverb and song mixer are The Black Box's.

```
python tools/generate_audio.py                 # everything (about three minutes)
python tools/generate_audio.py music           # the songs
python tools/generate_audio.py sfx             # every sound effect
python tools/generate_audio.py embers pause    # just these
```

| Song | Where | What it is |
| --- | --- | --- |
| *Embers* | Title, options, save slots | 56 BPM in A minor, 12 bars. A lute picking slow broken chords over a bowed drone (Am, F, Dm, Em, then F, G and an E major that pulls back round to the A), a melody on the lute an octave up for the second half, a quiet choir, a frame drum on the downbeats, wind, and a fire crackling close by |
| *Hearthsmoke* | Lit maps | 66 BPM in D Dorian, 16 bars. Folk rather than dread: the lute rolls through the chords in eighths, a wooden flute plays a tune over the second half, and the frame drum keeps a slow walk. The major IV is what makes it Dorian, and keeps it from sounding as sad as the title |
| *Cold Stone* | Underground | 48 BPM in C Phrygian, 10 bars. Hardly a tune: a low drone on C and G, a choir that leans on the flat second, a few low lute notes far apart, one deep boom, cold air, and water dripping somewhere in a long reverb |

Each song is an exact number of bars long and wraps its own echoes round to the start, so it loops
under `MediaPlayer.IsRepeating` without a seam. A map's file says which song plays on it. The sound
effects so far: a soft tick for moving through a menu, a low plucked string for picking, a knock
played backwards for going back, two small ticks for changing a value, an iron latch for the pause
menu, and a footstep on stone and one on grass.

`Audio/AudioManager` is where all of it lives: the same `Content.Load<Song>`, `MediaPlayer` and
`SoundEffect.Play` calls as the audio tutorial, in one game service any screen can reach, with a
master, a music and a sound volume. Each screen asks for its song as it comes on, and the manager
fades the old one out and the new one in.

## What is on screen

| Sprite | Source | Motion |
| --- | --- | --- |
| `title-scene.png` | `TitleBackgroundScreen` | Still. The gate, the wall and the flagstones, lit by the torches, at 4x |
| `torch-flame.png` | `TorchSprite` | 6 frames, 12x20, the base at the bottom middle. Each torch starts at its own frame. On the title and in every sconce |
| `glow.png` | `TorchSprite`, `WalkerSprite` | A white circle fading to nothing, drawn additively and tinted, so it's whatever colour of light it needs to be. Squashed flat, it's the shadow under the walker |
| `mote.png` | `EmberParticleSystem` | One ember. Coloured yellow to red as it cools |
| `button.png` | `ButtonSprite` | Nine-sliced iron plate; a slow smoulder idle, kindles on hover, sinks on press. The bottom band is the groove's light, tinted torch, ember or bone |
| `panel.png` | `FormPanel` | Nine-sliced iron slab the forms are built on, with a bronze line inside the lip |
| `Tiles/village.png` | `Tilemap` | 16x16 tiles, eight to a row, in `TilesetCatalog` order: grass, long grass, dirt, cobbles, flowers, tufts, pebbles, fences, stone and plaster house fronts, a window, a door in two halves, and thatch (ridge, roof and eave) |
| `Tiles/crypt.png` | `Tilemap` | The same layout: floors, the floor in a wall's shadow, steps, the tops of walls, the two rows of a wall's face (one with a burial niche), rubble, bones and a cobweb |
| `Props/*.png` | `PropSprite` | One picture each, standing on the middle of its bottom edge: a barrel, a crate, the well, an urn, a sarcophagus, and the iron sconce a torch stands in |
| `Paperdoll/body-lean.png` | `WalkerSprite` | The lean body in smallclothes, in the 10x4 layout above. Painted in the skin keys, so it can be recoloured later |

Text is drawn with `spectral-title` (Spectral Light, 92pt), `spectral-ui` (Spectral Medium, 26pt)
and `spectral-detail` (Spectral Light, 17pt), letterspaced, the same faces as The Black Box. The
`.spritefont` files name their TTF by relative path, so the build always uses the font shipped in
`Content/Spectral/` rather than whatever is installed on the machine.

## Regenerating the art

Every PNG in `Content/` is made by a script:

```
python tools/generate_ui.py           # button.png and panel.png
python tools/generate_world.py        # the tilesets, the props, the title gate, the torch flame, glow.png and mote.png
python tools/generate_characters.py   # the paperdoll sheets
```

They need nothing but CPython. The PNG writer is in `tools/pixels.py` (copied from The Black Box's
`generate_assets.py`), and every drawing is seeded, so running a script again produces the same
bytes; only what you changed changes. Everything on a map is drawn as if a torch were right on it,
lit from the top left, and the game does the dark itself. Tiles that lie next to each other (grass,
cobbles, floors) only vary in ways that meet cleanly at the edges: the cobbles, for one, are a grid
of stones that wraps round the tile, so one tile's stones carry on into the next.

`python tools/check_assets.py` checks that `ASSETS.md`, `Content/Content.mgcb` and the files on
disk agree, and fails with a list if they don't. I run it before every commit.

## Still to come

In the order they're being built:

1. ~~The screens around the game, the sound, the saves and the proof run.~~
2. ~~One map and one walker: the map file format, collision, the camera, walking, the dark with lights in it.~~
3. The three maps joined up: exits, triggers, chests, doors, levers, the map screen and fast travel.
4. The party: four members, switching between them, followers, click-to-move with A*.
5. The paperdoll and gear: armour you can see, the inventory and the character sheet.
6. Character creation.
7. Dialogue with dice rolls, approval, the quest and the journal.
8. Combat, and `--simulate`.
9. Resting, full saving, and the polish.

The maps could go through the content pipeline with an importer of their own (that's chapter 11 of
the textbook); for now they're copied as text and read at startup, which is enough to catch mistakes.

## Where things are

- `LegendQuestGame.cs` reads the maps, sets up the window, the screen manager and the sound, and puts the title up. `LegendQuestGame.Proof.cs` takes the proof shots.
- `StateManagement/` is the game state management tutorial: `GameScreen`, `ScreenManager`, `MenuScreen` and `MenuEntry`, `LoadingScreen`, and the input classes from the advanced input tutorial. `InputState` is the only thing that reads the keyboard, mouse or pads.
- `Screens/` is every screen. The map screen has its part of the proof schedule beside it, in `ExploreScreen.Proof.cs`.
- `World/` is the maps: `MapLoader` and `MapCatalog` to read them, `MapDefinition` for what's in one, `Tilemap` to draw one and build its walls, `LightMap` for the dark, `Camera2D`, and the tileset and prop catalogues.
- `Characters/` is the walking sprite.
- `Components/` is every screen element and the torches, each with `LoadContent`, `Update` and `Draw` the way the tutorials do it.
- `Collisions/` is the collision tutorial's rectangle and helper.
- `Particles/` is the particle engine and the embers built on it.
- `Audio/` is the `AudioManager`. `Save/` is the save file, the slots, and the settings.
- `Enums/` is every enum in the game, one file each.
- `Layers.cs` is the draw order, `Palette.cs` is the colours more than one screen uses, `Text.cs` draws a line of text with its shadow, and `Blends.cs` is the additive blend for anything that glows.
- `Content/Maps/` is the map files. `tools/` is the art and sound generators and the asset check.

## Assets

Every picture, song and sound in the game is original: drawn or synthesised by the scripts in
`tools/`. The only outside work is the Spectral font and the MonoGame template files. See
[ASSETS.md](ASSETS.md) for every file.

## Author's Note

