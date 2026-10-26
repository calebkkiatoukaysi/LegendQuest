# Legend Quest

A MonoGame project for CIS 580 - Foundations of Game Programming, Kansas State University.

Legend Quest is a top-down, turn-based fantasy RPG. You make a character, gather a party of up to
four, walk the maps in real time, talk your way through conversations where the risky options
roll a d20 on screen, and fight turn by turn when talking doesn't work. What each character wears
shows on them as they walk. The rules underneath will be 5e's, from the SRD 5.1.

This is the skeleton: every system working end to end on a little content, built so maps,
characters, items and dialogue can keep being added without rewriting anything. It's being built
in milestones, and right now only the first one is done: the screens around the game, the sound,
the saves, and the proof run. [Still to come](#still-to-come) lists the rest.

## Running it

The window is 1600x900. The art is drawn small and blown up by a whole number so the pixels stay
square: 3x for the buttons and panels (and the world, once there is one), 4x for the gate behind
the title, which is 400x225 art pixels and fills the window exactly.

```
dotnet run
```

**NEW GAME** and **LOAD** open the save slots, **CONTINUE** carries on whichever slot was saved
last (it's dead when there isn't one), **OPTIONS** has the volumes and the text speed, and
**EXIT** and **Esc** close the game. There are no maps yet, so picking a slot goes through the
loading screen and comes back to the title.

One switch exists for working on the game, and it isn't the game:

```
dotnet run -- --proof shots       # play through the screens, save a picture of each to shots/, and quit
```

It's the same harness as The Black Box's. It goes through the title, the options, the save slots
for loading (and erases a broken slot the way a player would, ERASE then SURE?), the save slots
for a new game, the pause menu, and out through QUIT TO TITLE. It uses a scratch save folder
inside `shots/` that it fills with one of every kind of slot, so it never touches a real one, and
it ignores the real mouse and keyboard so nothing I do while it runs changes the pictures. It's
silent: every sound it would have played goes in `shots/audio.log` with the frame it was asked for
on. A second switch, `--simulate`, comes with combat.

### Controls

| Where | Keys |
| --- | --- |
| Menus and forms | **Arrows** or **WASD** to move, **Enter** or **Space** to pick, **Esc** to go back. The mouse works too |
| Options | **Left** and **right** change the selected row |
| Save slots | **Left** and **right** move between a slot and its ERASE |

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
| `PauseMenuScreen` | RESUME, OPTIONS or QUIT TO TITLE. Nothing under it moves while it's up. SAVE and LOAD go on it once there's a game to save |
| `LoadingScreen` | From the tutorial. Waits for everything to fade out, then adds the next screens, with a line of text on the black in between if there is one |

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
  quietly drop the fields this build doesn't know about.
- **Every file carries a `Version`.** `SaveSystem.Upgrade` is where an old file gets brought
  forward when a field changes. Nothing has needed it yet.

A new game only goes in a numbered slot, and picking one that's full asks first (PICK IT AGAIN TO
START OVER IT). ERASE asks too: the button turns into SURE?.

The volumes and the text speed aren't in a slot. They're in `%LOCALAPPDATA%\LegendQuest\settings.json`,
since turning the music down shouldn't depend on which game is loaded.

## Music and sound

None of it was recorded or downloaded. `tools/generate_audio.py` builds every song and sound from
sines, wavetables, noise and a few filters, in CPython (3.12 or newer) and nothing else. The
oscillators, filters, reverb and song mixer are The Black Box's.

```
python tools/generate_audio.py                 # everything (about a minute)
python tools/generate_audio.py music           # the songs
python tools/generate_audio.py sfx             # every sound effect
python tools/generate_audio.py embers pause    # just these
```

| Song | Where | What it is |
| --- | --- | --- |
| *Embers* | Title, options, save slots | 56 BPM in A minor, 12 bars. A lute picking slow broken chords over a bowed drone (Am, F, Dm, Em, then F, G and an E major that pulls back round to the A), a melody on the lute an octave up for the second half, a quiet choir, a frame drum on the downbeats, wind, and a fire crackling close by |

The song is an exact number of bars long and wraps its own echoes round to the start, so it loops
under `MediaPlayer.IsRepeating` without a seam. The sound effects so far are the menu ones: a soft
tick for moving, a low plucked string for picking, a knock played backwards for going back, two
small ticks for changing a value, and an iron latch for the pause menu.

`Audio/AudioManager` is where all of it lives: the same `Content.Load<Song>`, `MediaPlayer` and
`SoundEffect.Play` calls as the audio tutorial, in one game service any screen can reach, with a
master, a music and a sound volume. Each screen asks for its song as it comes on, and the manager
fades the old one out and the new one in.

## What is on screen

| Sprite | Source | Motion |
| --- | --- | --- |
| `title-scene.png` | `TitleBackgroundScreen` | Still. The gate, the wall and the flagstones, lit by the torches, at 4x |
| `torch-flame.png` | `TorchSprite` | 6 frames, 12x20, the base at the bottom middle. Each torch starts at its own frame |
| `glow.png` | `TorchSprite` | A white circle fading to nothing, drawn additively and tinted, so it's whatever colour of light it needs to be |
| `mote.png` | `EmberParticleSystem` | One ember. Coloured yellow to red as it cools |
| `button.png` | `ButtonSprite` | Nine-sliced iron plate; a slow smoulder idle, kindles on hover, sinks on press. The bottom band is the groove's light, tinted torch, ember or bone |
| `panel.png` | `FormPanel` | Nine-sliced iron slab the forms are built on, with a bronze line inside the lip |

Text is drawn with `spectral-title` (Spectral Light, 92pt), `spectral-ui` (Spectral Medium, 26pt)
and `spectral-detail` (Spectral Light, 17pt), letterspaced, the same faces as The Black Box. The
`.spritefont` files name their TTF by relative path, so the build always uses the font shipped in
`Content/Spectral/` rather than whatever is installed on the machine.

## Regenerating the art

Every PNG in `Content/` is made by a script:

```
python tools/generate_ui.py       # button.png and panel.png
python tools/generate_world.py    # the title gate, the torch flame, glow.png and mote.png
```

They need nothing but CPython. The PNG writer is in `tools/pixels.py` (copied from The Black Box's
`generate_assets.py`), and every drawing is seeded, so running a script again produces the same
bytes; only what you changed changes.

`python tools/check_assets.py` checks that `ASSETS.md`, `Content/Content.mgcb` and the files on
disk agree, and fails with a list if they don't. I run it before every commit.

## Still to come

In the order they're being built:

1. ~~The screens around the game, the sound, the saves and the proof run.~~
2. One map and one walker: the map file format, collision, the camera, walking, the dark with lights in it.
3. The three maps joined up: exits, triggers, chests, doors, levers, the map screen and fast travel.
4. The party: four members, switching between them, followers, click-to-move with A*.
5. The paperdoll and gear: armour you can see, the inventory and the character sheet.
6. Character creation.
7. Dialogue with dice rolls, approval, the quest and the journal.
8. Combat, and `--simulate`.
9. Resting, full saving, and the polish.

## Where things are

- `LegendQuestGame.cs` sets up the window, the screen manager and the sound, and puts the title up. `LegendQuestGame.Proof.cs` takes the proof shots.
- `StateManagement/` is the game state management tutorial: `GameScreen`, `ScreenManager`, `MenuScreen` and `MenuEntry`, `LoadingScreen`, and the input classes from the advanced input tutorial. `InputState` is the only thing that reads the keyboard, mouse or pads.
- `Screens/` is every screen, one per file.
- `Components/` is every sprite and screen element, each with `LoadContent`, `Update` and `Draw` the way the tutorials do it.
- `Particles/` is the particle engine and the embers built on it.
- `Audio/` is the `AudioManager`. `Save/` is the save file, the slots, and the settings.
- `Enums/` is every enum in the game, one file each.
- `Layers.cs` is the draw order, `Palette.cs` is the colours more than one screen uses, `Text.cs` draws a line of text with its shadow, and `Blends.cs` is the additive blend for anything that glows.
- `tools/` is the art and sound generators and the asset check.

## Assets

Every picture, song and sound in the game is original: drawn or synthesised by the scripts in
`tools/`. The only outside work is the Spectral font and the MonoGame template files. See
[ASSETS.md](ASSETS.md) for every file.

## Author's Note

