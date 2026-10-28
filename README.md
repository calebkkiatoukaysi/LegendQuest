# Legend Quest

A MonoGame project for CIS 580 - Foundations of Game Programming, Kansas State University.

Legend Quest is a top-down, turn-based fantasy RPG. You make a character, gather a party of up to
four, walk the maps in real time, talk your way through conversations where the risky options
roll a d20 on screen, and fight turn by turn when talking doesn't work. What each character wears
shows on them as they walk. The rules underneath will be 5e's, from the SRD 5.1.

This is the skeleton: every system working end to end on a little content, built so maps,
characters, items and dialogue can keep being added without rewriting anything. It's being built
in milestones, and right now the first four are done: the screens around the game, the sound, the
saves and the proof run; the maps, walking round them, and the dark; the three maps joined up, with
things on them to open and pull, the map screen and fast travel; and the party, following you about,
with the mouse to walk, point and click.
[Still to come](#still-to-come) lists the rest. The look has changed since the fourth: the title
and the capital, Questeria, are cut from pictures I made in a brighter style (see
[Supplied art](#supplied-art)). And the story has started. I've written the whole of it, three acts,
and it's going in from the beginning in its own order: so far that's the Soul-Bound, the
conversations the story needs (choices, dice, approval), the Prologue, Act 1's first chapter, and
buildings you can go into. [The story](#the-story) says what's in.

## Running it

Everything is drawn on a 1600x900 canvas (`ScreenCanvas`), and the canvas is blown up to fill the
window, keeping its shape. The window opens as big as fits on the screen it's on, can be dragged to
any size, and **F11** (or DISPLAY on the options) makes it full screen. A whole number of times
bigger is point sampled; in between, it's blown up to the next whole number and shrunk the rest of
the way smoothly, so the pixels stay even. On the canvas the art is drawn small and blown up by a
whole number so the pixels stay square: 3x for the buttons, the panels and the world, 4x for the
picture behind the title, which is 416x277 art pixels, a little bigger than the canvas so its
layers can sway. The mouse wheel zooms the world between 2x, 3x and 4x, never anything in between.

```
dotnet run
```

**NEW GAME** opens the save slots, and picking one starts the Prologue: you make yourself, then
the Soul-Bound, your oldest friend, then you sail for Questeria (see [The story](#the-story)).
**LOAD** opens them to carry a game on, **CONTINUE** carries on whichever slot was saved last (it's
dead when there isn't one), **OPTIONS** has the volumes and the text speed, and **EXIT** and **Esc**
close the game. On a map, clicking the ground walks you there, clicking someone or something walks you up to
them, E uses whatever you're standing next to, Tab changes who you're controlling, M is the map, and
Esc is the pause menu.

One switch exists for working on the game, and it isn't the game:

```
dotnet run -- --proof shots       # play through the screens, save a picture of each to shots/, and quit
```

It's the same harness as The Black Box's. It goes through the title, the options, the save slots
for loading (and erases a broken slot the way a player would, ERASE then SURE?), the save slots for
a new game, then the Prologue: its card, the creation screen (you as you start, then named, and
each of its tabs), the question that picks your voice, the Soul-Bound as they start and then named,
and the two paintings, the pier and the red moon. On the deck of the Gull's Wager the voyage plays
and gets read through, answering its questions, past the sea-wraiths' card to Questeria at dawn. On
the harbour the arrival plays itself: the ship sailing in, tied up, the walk down the gangplank, and
Act 1's card. Then the Red Gulls: it tries talking first, the Persuade check comes up on the dice,
it calls the Soul-Bound off Grell, and Captain Hale arrives. Then up the street into Questeria: the Soul-Bound talking as you come in,
the plaza and the same view a few seconds on with the townsfolk moved, round behind a house, both
zooms, the mouse over the far side of a statue (the path goes round it) and a click to walk it, the
mouse on the elder and a click to walk up and talk to them, the mouse on the canal where there's no
way, Tab, a click on a card, Fennick and the quest board, you sent off on your own while the
Soul-Bound finds their way after you, the waypoint crystal, the map, and in through the Gilded
Flagon's door. Inside it takes the innkeeper's work and goes back out the door, which leaves you on
the street outside it, then out through the city gate. On the old road it walks into the trigger before the camp, opens the chest,
reads the sign, finds the second waypoint and fast travels back to Questeria, then straight back to
the road by it, walks into the
trigger at the blocked end twice (it goes off every time), and up the path into the barrow. In the
barrow it tries the locked door and the stone door, pulls the lever that opens the stone door, goes
through it to the chest, opens the map, stands under a torch, pauses, and goes out through QUIT TO
TITLE, where the slots show the autosave it made on the way. Moving and using things go in through
the same code the keyboard does, and the mouse is a pretend one inside `InputState` that the screen
reads the same as the real one; it only jumps about between scenes. It uses a scratch save folder
inside `shots/` that it fills with one of every kind of slot, so it never touches a real one, and
it ignores the real mouse and keyboard so nothing I do while it runs changes the pictures. It's
silent: every sound it would have played goes in `shots/audio.log` with the frame it was asked for
on, which is how I check the footsteps change from grass to stone and the door grinds when the
lever's pulled. A second switch, `--simulate`, comes with combat.

### Controls

| Where | Keys |
| --- | --- |
| Menus and forms | **Arrows** or **WASD** to move, **Enter** or **Space** to pick, **Esc** to go back. The mouse works too |
| Options | **Left** and **right** change the selected row |
| Save slots | **Left** and **right** move between a slot and its ERASE |
| On a map | **WASD** or the **arrows** to walk, **E**, **Space** or **Enter** to use or talk to what you're next to, **M** for the map, the **mouse wheel** to zoom, **Esc** to pause |
| On a map, the mouse | **Left click** the ground to walk there (the path shows while you point), or someone or something to walk up to them and talk or use it. Pointing at anything says what it is |
| The party | **Tab** for the next in line, **F1** to **F4** for one in particular, or click their card or them |
| Making a character | **Up** and **down** pick a row, **left** and **right** change it, **Q** and **E** change the tab. On NAME, **Enter** and then type |
| Conversations | **Enter**, **Space**, **E** or a click shows the rest of the line, then the next. At a question, **1** to **4**, a click, or the **arrows** and **Enter** |
| The dice | **Space** or a click rolls, and the same again goes on |
| The ship coming in | **Esc** skips it |
| Anywhere | **F11** switches full screen on and off |
| The map | **Tab** switches between the map you're on and the world. Opened from a waypoint, the **arrows** (or the mouse) pick where to go and **Enter** (or a click) goes. **M** or **Esc** closes it |

A gamepad works anywhere the keyboard does: the d-pad or left stick, A to use, B to back out, Y for
the map, the right shoulder for the next in the party (and both shoulders to switch the map's sides),
and Start.

## Screens

Every screen is a `GameScreen` on a `ScreenManager`, the way the game state management tutorial
does it (`StateManagement/` is that tutorial's code, carried over from The Black Box). Each has its
own `TransitionOnTime` and `TransitionOffTime`, so everything fades instead of cutting.

| Screen | What it is |
| --- | --- |
| `TitleBackgroundScreen` | Questeria at twilight behind the title and every form on it, moving. It doesn't transition off when something covers it, so it stays up behind all of them |
| `TitleScreen` | The title, with NEW GAME, CONTINUE, LOAD, OPTIONS and EXIT |
| `OptionsScreen` | Master, music and sound volume, and how fast text types out. Saved to `settings.json` |
| `SaveSlotScreen` | The save slots, for a new game or for loading one, with ERASE beside each slot that has something in it |
| `CharacterCreationScreen` | Making you, then the Soul-Bound: rows for a name, a people, a class and every part of a look, on three tabs, with them walking on the spot and turning round beside the rows, and their face blown up over that. RANDOM, BEGIN and BACK. The Prologue's first lines play between the two |
| `DialogueScreen` | Popup. A conversation a line at a time, over the dimmed map or a painting: the speaker's face and name, the line typing out at the text speed, and a question's options numbered under it, with an icon for a check, a flirt or a locked one. Approval, gold and items come up as toasts |
| `DiceRollOverlay` | Popup, over a conversation. A check: the d20 with the DC and every modifier, a press to roll, the tumble, then the total and SUCCESS or FAILURE |
| `ExploreScreen` | A map being walked round: the tiles, the props, the things you can use, the people, the party, and the dark if it's a dark map. The party's cards are along the bottom left, toasts come up near the top (what was in a chest, what a sign says), and whatever the mouse is on gets a tooltip and its own cursor |
| `MapScreen` | Popup. The map you're on, as much as you've seen of it, and the world: every place you've been and the roads between them. Opened from a waypoint, it's where you pick somewhere to travel to |
| `PauseMenuScreen` | RESUME, OPTIONS or QUIT TO TITLE. Nothing under it moves while it's up. SAVE and LOAD go on it once there's a game to save |
| `LoadingScreen` | From the tutorial. Waits for everything to fade out, then adds the next screens. Going onto a map puts its name on the black in between |

## The title

Questeria across a lake at twilight, under a blue and pink sky, from a terrace with blossom trees
either side. It's layers like the parallax tutorial's, except nothing scrolls: the whole view sways
slowly from side to side and each layer moves by its own share of that, the sky hardly at all and
the terrace the most, so it reads as near and far. Clouds drift across, an island floats and bobs
with a waterfall falling off it, a banner blows either side of the terrace, the crystal on a tower
pulses, and stars twinkle over the castle. Petals come off the trees (`PetalParticleSystem`) and
motes of light rise off the water (`SparkleParticleSystem`), both on the particle system tutorial's
engine. Every picture in it is cut by `tools/cut_supplied.py` (see [Supplied art](#supplied-art)),
and drawn at 4x.

## The story

The story is mine, written out in full before any of it went in: a prologue and three acts, every
scene with an id (`P-01`, `A1-C1-01`). It goes into the game in its own order, each step playable
before the next, and each conversation is named after its scene (`a1-c1-01` is A1-C1-01), the same
as the proof shots. What's in so far:

- **The Soul-Bound.** You don't arrive alone. Your oldest friend comes with you: the one you met on
  a pier in a storm when you were eight, and swore an oath with under a red moon at sixteen. You
  make them too, on the same screen, and they're whoever you make them: their name, their pronouns
  (he, she or they), their people, their class and their look. They took Valen's place.
- **The Prologue.** P-01 is a card, then the creation screen makes you. In P-02 the narrator names
  you and asks what drives you: doing what's right, getting the job done, or a good story. That's
  your voice (Honorable, Pragmatic or Witty), and some lines and options later depend on it. In P-03
  the creation screen makes the Soul-Bound, with the pier you met on behind it, then the narrator
  remembers that day and the oath, each over a painting, and the game is written to its slot. P-04
  and P-05 are the voyage: the deck of the Gull's Wager at night, two days out, the Soul-Bound
  talking about the new king, the bell from the crow's nest, the sea-wraiths (a card until there's
  combat), and Questeria at dawn.
- **Act 1, Chapter 1: Saltwind Welcome.** The ship comes into Saltwind Docks and Act 1's card comes
  up. The Red Gulls are shaking old Tomas down for a dock tax that doesn't exist. You can wade in,
  try talking first (a Persuade check, with advantage for a Skald or an Honorable voice), or say it's
  not your business, in which case the Soul-Bound goes in anyway. Then their boss, Grell, is on his
  knees with the Soul-Bound's blade at his throat a beat too long, and what happens to him is up to
  you. Captain Hale's Watch arrives, and Tomas gives you his lucky hook. Up in the city the
  Soul-Bound points the way, Fennick talks you through the quest board in the Lantern Market (the
  Rangers' contract for the Mirewood glows on it), and the innkeeper in the Gilded Flagon has work
  if you want it.

Everyone in it who isn't in the party is in `Cast`, with the name they go by, which townsfolk sheet
or paperdoll look they wear, and who's only ever a voice. Captain Odelle, the Red Gulls, Grell,
Captain Hale, the Watch and Fennick are drawn the party's way, so they can wear bandanas, a tricorn
or a helm; Old Tomas is one of the townsfolk. The
story's bookkeeping is in `Story/`: the meters (`Meters`: the bond between you and the Soul-Bound,
their ambition, each companion's approval, fame, and how far the eclipse has gone, each with where
it starts and its range), the key items, the paintings and the quest board's postings. Every table
there throws at startup if a row's wrong.

The fights in the story are cards until combat exists ("The Red Gulls are beaten."), with the
conversation round them, and each keeps its encounter's id so the fight can go in where the card
is. The barks in them (the Soul-Bound's one-liners mid-fight) are lines for now too.

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
world = 860 110          ; where it is on the world map
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
[overhead]               ; roofs and canopies, drawn over everyone; . is an empty cell

[objects]                ; kind id x y, in tiles, then any settings as key=value
spawn start 19.5 27 facing=up
exit road 18 29 w=4 to=road at=barrow
trigger cold 18 19 w=4 say="The air in here is colder."
prop sconce 8 13
door tomb 19.5 13.5 locked=yes name="Tomb door"
door side 4.5 13.5 look=gate flag=side-gate name="Stone door"
lever side-lever 30 13 flag=side-gate
chest side-chest 4.5 8 gold=40
```

Something at x, y stands with its feet on the middle of that tile, and halves are fine. Exits and
triggers are rectangles instead: x, y is their top left tile and `w` and `h` their size. A value
with spaces in it goes in double quotes, and it can't have a `;` in it, since that's a comment.
`fog = no` in `[map]` has the map screen show all of a map from the start, which Questeria does,
since it's the capital and everyone knows their way round it.

| Kind | What it is | Settings |
| --- | --- | --- |
| `spawn` | Somewhere you can arrive. Every map needs one called `start` | `facing` (down if it isn't set) |
| `prop` | Anything out of `PropCatalog`; the id is which one | none |
| `npc` | Someone standing there (see People, under [The party](#the-party)) | `name`, `sheet` (one of `Townsfolk`), `facing`, `say` |
| `light` | A pool of light with nothing drawn for it, like daylight down the barrow's steps | `reach` in tiles, `strength` 0 to 1 |
| `exit` | Walk into it and you go to another map | `to` the map, `at` a spawn there (`start` if it isn't set), `w`, `h` |
| `trigger` | Walk into it and it says something or sets a flag | `say`, `set`, `once` (yes if it isn't set), `w`, `h` |
| `chest` | Opens once, and gives up what's in it | `gold`, and `look` and `name` like all of these |
| `door` | Opens and stays open, unless it's locked or a flag works it | `locked`, or `flag` (it's open while the flag is set, and won't move by hand) |
| `lever` | Sets its flag, or clears it if it's set | `flag` |
| `sign` | Has something written on it | `text` |
| `waypoint` | A stone you travel between. One a map at most | none |

The loader keeps the two fixes from the exercise: it trims every line (the exercise left the `\r`
from Windows line endings on each one), and it checks every row against the map's size (the
exercise took the id list on trust, and a short one read straight past the end). It also refuses a
tile that isn't in the legend, a legend tile that isn't in the tileset, a prop that isn't in the
catalogue, an object off the edge of the map, `ambient` on a map that isn't dark, a setting a kind
doesn't have (so `glod=12` is caught, and so is `gold=12` on a door), two objects with the same id,
a trigger that does nothing, a spawn inside an exit, a waypoint with no room beside it, and a quote
that's never closed. Once every map is read, `MapCatalog` checks every exit goes to a map that
exists and a spawn on it that does too.

- **Tiles.** Ids count from 1 and 0 is empty, the same as the exercise. A tileset lists its tiles
  in order, and each one says how many variants it has, whether it's solid and whether it sounds
  like grass or stone. The map picks a variant per cell from where the cell is, so a field of grass
  doesn't repeat, and it's the same every time. A tile can be a block of several tiles each way,
  where each cell shows its part of the block and the variant is picked per block. A tile can also
  be *edged*: Questeria's lawns, water and boardwalk have four more frames for their north, east,
  south and west sides, and the tilemap draws them wherever one meets ground of another kind, so a
  lawn gets its kerb and the water its quay wall without a tile for every corner.
- **Walls.** Collision comes from the tiles: a cell is solid if its ground or detail tile is, and
  each row's runs of solid cells merge into one `BoundingRectangle` (the collision tutorial's).
  There's a wall all the way round outside the edge too. Walking moves the feet one axis at a time
  and pushes them back out of anything they hit, exactly like the lobby, so a wall stops you one way
  and lets you slide along it the other. The feet box is 10 by 4 art pixels, narrower than a tile,
  so a one-tile gap can be walked through without lining up perfectly.
- **Props.** A prop stands on its foot like a person does and blocks the floor under it (its
  footprint). Floor props sort with the people; wall props, like the torch sconces, go behind
  everyone. A sconce or a campfire carries a light: where its flame goes and where its pool lands.
  The things you use are props too, with a picture for each way they can be: shut and open, up
  and down, asleep and awake. An arch, like Questeria's gate, has a gap down the middle of its
  footprint to walk through, and a patch of flowers lies flat under everyone and blocks nothing.
- **The camera** is The Black Box's `Camera2D`, except the world is in art pixels now and the
  camera does the scaling, so the zoom is just a whole number. It eases after you, stops at the
  edge of the map (or centres the map if it's smaller than the view), and rounds itself to whole
  screen pixels. The walker snaps to whole screen pixels too, or it crawls.
- **Sorting.** One world batch, back to front. The ground and detail tiles are at the back, wall
  props in front of those, then everyone on the floor sorted by how far down the map their feet
  are, then the overhead tiles. Questeria's buildings use that: each one's footprint is only its
  front few tiles, so walking along the back of one, it's drawn in front of you and you go behind
  the roof.
- **The dark.** A dark map gets The Black Box's lobby trick: one low resolution texture of darkness
  with the pools of light cut out of it, built once when the map loads, stretched over the map with
  smooth sampling and drawn after everything, so you go dark when you walk out of the light. What's
  new is that light stops at walls. Each texel walks the tiles between itself and the light and
  looks for one that blocks it: an opaque tile (a wall; rubble, fences and tree trunks are solid
  but you can see over them), or a door that's shut. A wall's face takes its light from the floor
  in front of it, two rows down at most, because my first go walked the line from the face itself,
  every face shut its own torch out of the floor beside it, and the pools came out square. The
  dark only gets built again when a door opens or shuts. The torches flicker without rebuilding
  anything: the glow texture goes over each pool additively, with a flicker made of three sine
  waves at speeds that never line up.
- **Using things.** Whatever's nearest your feet and still has something to do (an open chest
  doesn't) gets its name over it and `E · OPEN` or `PULL` or `READ` or `TRAVEL` under that. Reach
  is a tile and a half, a bit more for a door, which is two tiles wide, and more for a sign, which
  you can read from further off. `WorldState` has the rules: it reads and changes the save and hands
  back what happened, a chest's lid coming up and then the gold, say, and the screen plays a sound
  and puts up a line for each, the same as The Black Box's `RoundEngine` and `TableCues`. A door
  doesn't get its sound from the rules: the screen plays it when it sees a door move, since a lever
  across the room moves one too. Locked doors stay locked for now; keys and forcing them with a
  roll come with the inventory and the dice.
- **Triggers and exits** go off as your feet walk into them, not while you stand in one, and not
  if you arrive standing in one. A trigger that goes off once is kept in the save as used. An exit
  writes the autosave, then the loading screen takes you to the other map with its name on the
  black between.
- **What's been seen.** Every map keeps one bit per tile for whether it's been in sight: nine tiles
  round your feet, cut off by walls and shut doors with the same line the light uses. It only looks
  again when you step into another tile. A map with `fog = no` is seen all over from the start. The
  map screen draws the tiles and the props at one screen pixel per art pixel, browned like an old
  map, and lays one texel per tile of dark over what hasn't been seen, smooth sampled like the dark
  on a dark map, so the edge is soft. Where an exit's been seen, it says where it goes.
- **The world.** Each map says where it is on the world map, and the roads between them are wherever
  the exits go, so the two can't disagree. A map you've been on is a place with its name; a map
  one of those leads to, that you haven't been on yet, is a mark with no name. Using a waypoint finds it, then opens the
  world to travel from it to any other waypoint you've found. Fast travel autosaves the same as an
  exit, and puts you beside the stone you've travelled to, since in front of it you'd be standing
  in the way of it.

| Map | What it is |
| --- | --- |
| `questeria.map` | The capital and the hub, lit, with no fog. The palace on its terrace to the north with the temple and the guildhall either side, the plaza in the middle with its fountain and the waypoint crystal, houses and gardens to the west, the market to the east, the canals, and the avenue south to the city gate and the old road. Seventeen townsfolk |
| `road.map` | The old road through the forest, at dusk. In from Questeria on the west; a camp off to the south with its fire still going and a chest; a fork where a path goes north to the barrow, with a sign and the second waypoint; and a tree down across the road further east. The trees are trunks with their crowns overhead, so you walk under the edges of them |
| `barrow.map` | Dark, seven torches and a little daylight down the steps. In up the steps, through a short dark passage into the hall, with the locked tomb door in the middle of its north wall, the stone door into the side room at the west end, and the lever that opens it at the east end |

The road is meant to be lit, at dusk. I've made it a dark map with a lot of ambient light
(`ambient = 0.55`) so the campfire throws a pool, rather than giving lit maps a tint setting of
their own. It reads as dusk, and if it needs to be warmer or bluer that's a colour on the dark.

## The party

Up to four: you, and whoever's with you. A new game starts you with the Soul-Bound, and the story
recruits the rest: Sera, Kael, Mira and Brannock are in the data with their peoples, classes and
looks, ready for the chapters they join in. When there are four of you already, anyone else who
joins waits at camp.

| File | What it is |
| --- | --- |
| `PartyMember` | Someone in the party, as the save keeps them: name, pronouns, class, level, ability scores, hit points, look, and where they're standing |
| `Origins` | Everyone who can be in the party and how they start: class, scores (5e's standard array, 15 14 13 12 10 8, where their class recommends) and look. You and the Soul-Bound start from theirs and the creation screen does the rest |
| `ClassCatalog` | The eight classes: the SRD's names in the code and the story's on screen (a Fighter is a Blademaster, a Rogue a Shadowstep, a Cleric a Lightbearer, a Wizard an Arcanist, a Paladin a Warden, a Druid a Wildcaller, a Warlock a Hexblade and a Bard a Skald), each with its role, its hit die and where its scores go |
| `AbilityCheck` | A check: a d20 plus a modifier against a DC, with or without advantage, and a natural 20 or 1 deciding it whatever the total |
| `AbilityScores` | The six scores, and 5e's modifier: half of (score minus 10), rounded down |

Hit points at level 1 are 5e's: the whole hit die plus the Constitution modifier. Everyone stays
level 1 in the skeleton.

- **Following.** The others walk your trail: wherever your feet have been, a point every few
  pixels, and each aims for the spot their own distance back along it, 22 art pixels apart, so they
  go through doors and round corners in single file without thinking about it. If one falls too far
  behind, or pushes against something for half a second without getting anywhere, they find a path
  to their spot with A* and go back to the trail when they're there. They never stand in your feet,
  and you can walk through them. When you arrive on a map, they're put round you, behind you if
  there's room.
- **Who you control.** Tab, F1 to F4, clicking a card or clicking one of them. The camera eases over
  and whoever you were falls in behind. Only your footsteps make a sound, since four sets at once
  was a lot.
- **The strip.** A card each along the bottom left with their face, their name and a bar for their
  hit points; yours sits higher with a torchlit edge. The face is the head and shoulders of their
  own walking sheet blown up three times, so it always matches how they look. Conditions go on the
  cards when there's fighting.
- **The mouse.** Pointing at the ground shows the path you'd walk, a dotted line to a ring, and
  the walk cursor; where there's no way it's a crossed circle instead. Pointing at someone gives
  the speech bubble, at something you can use a hand, and either gets a soft pool of light at its
  feet, which shows in the dark too. A tooltip by the cursor says what it is, or for one of the party,
  their level, class and hit points. Clicking walks you there, or up to them and then talks or uses
  it, the same as E. The keys always take over from a path. The game draws its own cursor, so it can
  change shape.
- **A\* and the grid.** `NavGrid` knows which tiles can be walked: not solid, and not covered in the
  middle by a footprint (a prop, a shut door, someone standing there). It's built again when a door
  opens. `Pathfinder` is plain A*: eight directions, never cutting a corner past a blocked tile,
  1 for a straight step and √2 for a diagonal with octile distance for the guess, and a
  `BinaryHeap` for the open list. It can aim for a tile or for anywhere within reach of a point,
  which is how clicking a chest works. Then the path's pulled straight wherever the feet could walk
  a straight line, so you don't zigzag across an open square.
- **People.** `npc` in a map file puts someone there: a name, which of the townsfolk's sheets
  they're drawn from, which way they face, and a line. They stand, look round every few seconds (never at the wall), and turn to face you
  when you talk to them; the line goes up over their head on a dark smudge. Conversations take over
  from the line when they have one. Questeria has seventeen of them, and some of the townsfolk
  stroll: every so often one with `wander` set walks off to somewhere within that many tiles of
  where they were put, rests, looks round, and goes somewhere else. They give up on a stroll if
  anything's in the way, and stop and face you when you talk to them.

## Conversations

A conversation is a `DialogueScript`, written in C# in `Dialogue/Scripts/`, a file per chapter. It
says who it's with, the flags it `Needs` and the ones that stop it (`Unless`), and its lines.
`DialogueCatalog.For` picks what someone says: the first of theirs that can start. Talking to them
when there isn't one gets their `say` line over their head instead.

- **A line** has who says it (one of the cast, you, the Soul-Bound, a companion, the narrator in a
  colour of its own, or a card big in the middle of the screen), what they say, and a stage direction under
  their name. It can have conditions (it's skipped if they don't hold), effects for when it's said,
  where it goes next (the next line if it doesn't say), a painting or the quest board behind it, and
  a sound.
- **A question** is a line with two to four options. Each has its text, conditions, effects and
  where it goes. One whose conditions don't hold isn't shown, unless it's `Locked`, when it's greyed
  with what it needs (WITTY, WARDEN). A `Flirt` option gets a heart.
- **A check** is an option that rolls: a d20 plus an ability's modifier, against a DC of 12, or 15 when
  the odds are against you, with advantage when any of the things it names are true (a class, a
  voice, a flag). A natural 20 always passes and a natural 1 always fails, and it goes one way on a
  pass and another on a fail. `AbilityCheck` rolls it; the dice on screen only show what it rolled.
- **A clock.** A question can have seconds on it, and where it goes if they run out. A bar runs
  down under the options.
- **Conditions** look at a flag being set or not, a meter at or over a number or under it, your
  voice, who's in the party, or your class. **Effects** set or clear a flag, add to a meter or set
  it, hand over gold, give or take a key item, change what you're to do next, have someone join (or
  wait at camp if there are four of you already), pick your voice, or take you to another map when
  it's over.
- **Words in braces** get filled in by `TextTokens`: `{name}` is yours, `{sb}` is the Soul-Bound's,
  `{sb_they}`, `{sb_them}` and `{sb_their}` are their pronouns, `{race}`, `{class}`, `{sb_race}` and
  `{sb_class}` are what the two of you are, and `{romance}` is whoever you're with. A capital on the
  token's first letter capitalises the word it gives.

`DialogueRunner` plays a script against the save the way The Black Box's discussion did: it draws
nothing and takes its `Random`, and hands back what happened as events (a meter moved, a check
rolled, someone joined) for `DialogueScreen` to turn into toasts, dice and sounds, and for the map
under it to catch up with, like the Red Gulls leaving the docks the moment they're beaten. When a
companion's approval moves while they're in the party, a toast says they approve or disapprove.
Every script is checked when the game starts, after the maps: every line going somewhere that
exists, every flag, meter, item, painting and person being real, every check having somewhere to go
on a pass and a fail, every clock having somewhere to go when it runs out, and every map and spawn
a conversation travels to existing. A mistake stops the game right there and says which line.

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
The footstep is grass or stone depending on the ground tile under you. For now the party is just
the body, in smallclothes; the paperdoll puts hair, a face and clothes on it later. The townsfolk
have sheets of their own, cut from my pictures already dressed, and since they only stand and
walk, theirs stop after the first five columns.

The party is all one body: the youth off my townsperson sheet, the best drawn walker I have, so
everyone looks like they belong with the townsfolk. `tools/generate_paperdoll.py` takes him apart:
every pixel of him gets sorted into skin, hair, eyes, shirt, vest, breeches or something that never
changes (his outline, belt and boots), each part gets cut down to a few key shades, and his hair
comes off onto a layer of its own. The other hair styles (cropped, long, tied back, braided), the
faces, the beards, the skirt and the cape are drawn onto his frames from where his head, eyes, chin,
waist and legs turn out to be in each one, and so are what makes the peoples look like themselves:
a Sylvan's pointed ears, a Wildkin's wolf, hawk or bear ears and a wolf's tail, an Emberkin's
horns (curled or swept) and burning tail, and the hats (a bandana, a tricorn, the Watch's helm). A
tail comes in two halves, like the cape, since it's behind you facing down and in front of you
facing up. The Stoneborn and the Hearthling are shorter, so every layer is drawn again for each
build, with rows taken out of the torso and legs: `Paperdoll/Stout/` and `Paperdoll/Small/`. `Paperdoll.Compose` lays the sheets someone's look calls
for over each other in `PaperdollOrder`, and `CharacterPalettes` swaps each key shade for the same
shade of the preset their look picks, out of `character-palettes.png`: six skins, eight hair
colours, six eye colours, five shirts, eight vests, six for the legs and six cloaks. That's The Black
Box's palette swap, done once per look with `GetData` and `SetData`, not every frame, and two
people who look alike share one sheet. For now his sheets only go as far as the fifth column; the
fighting columns come with combat.

## Saving

Five slots, one JSON file each, under `%LOCALAPPDATA%\LegendQuest\Saves`: three numbered ones
(`slot0.json` to `slot2.json`), `quicksave.json` and `autosave.json`. Not next to the executable,
because `bin/` gets deleted by every clean build.

| File | What it is |
| --- | --- |
| `SaveData` | The whole save: the map (and where you came in, while you're inside a building), the party (each with where they're standing), who's waiting at camp and which of the party you're controlling, the play time, the gold, the key items, what you're to do next, your voice, who you're with, the story's meters and flags, the chests and doors opened, the triggers used, the waypoints found, and what's been seen of each map. It grows with the game: each system adds its field when it exists |
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
  forward when a field changes, one version at a time. Version 2 is the party: version 1 had one X,
  Y and Facing for you, and upgrading gives it the starting party with you standing there. Version
  3 is the Soul-Bound: Valen becomes them, keeping his name and look, the placeholders go, and the
  Prologue counts as done, since those games started on the harbour. Those old fields only make it
  as far as `Upgrade` because `SaveData.Extra` keeps whatever this build doesn't have a property
  for.

Enums go in by name (`"Facing": "Up"`), so reordering one is safe and renaming one breaks saves; a
number where a name should be makes the slot unreadable. Things on a map are kept as `map/id`, like
`road/camp` for the chest at the camp, so two maps can each have a chest called chest. What's been
seen of a map is its bits packed eight to a byte, which JSON writes as base64. A list I've nulled by
hand comes back empty instead of crashing, a save with nobody in the party gets the starting party,
hit points get pulled back between 0 and the most someone can have, and a position off the map or
inside a wall puts you on the map's start. A class the game doesn't have makes the slot unreadable.

A new game only goes in a numbered slot, and it's written as soon as you pick one, so if the slot
can't be written the form can say so. Picking one that's full asks first (PICK IT AGAIN TO START
OVER IT). ERASE asks too: the button turns into SURE?. While you play, the game writes the autosave
every time you go from one map to another, by an exit or by fast travel, before anything fades, so
it has exactly what the next map starts from. If it can't be written the game carries on and the
next map says so. Saving where you like, and F5 and F9, come with the rest of saving at the end.

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
menu, a footstep on stone and one on grass, a heavy door dragging open (a creak built the way a
hinge makes one, out of a run of tiny slips, over stone grinding), a latch rattling in a lock that
won't give, a chest lid creaking up and knocking back, an iron lever clicking over its teeth and
clunking down with a chain taking up somewhere, a handful of coins, and a waypoint stone waking up
with a low hum and something like glass ringing over it. The story brought seven more: the d20
thrown down and tumbling, a check passing (two bright notes a fifth apart, like a small bell) and
failing (a dull knock and a note sliding down), a companion approving (a soft note rising) and
disapproving (the same, lower and turning down), a heartbeat for the Prologue's card, one on its own
and then two in step, and a ship's bell rung three times for all hands.

`Audio/AudioManager` is where all of it lives: the same `Content.Load<Song>`, `MediaPlayer` and
`SoundEffect.Play` calls as the audio tutorial, in one game service any screen can reach, with a
master, a music and a sound volume. Each screen asks for its song as it comes on, and the manager
fades the old one out and the new one in.

## What is on screen

| Sprite | Source | Motion |
| --- | --- | --- |
| `Title/sky.png`, `Title/city.png`, `Title/foreground.png` | `TitleBackgroundScreen` | The three layers, 416x277 each, at 4x, swaying against each other |
| `Title/clouds.png`, `Title/island.png` | `TitleBackgroundScreen` | Six clouds, four of them drifting across, and the floating island, which bobs |
| `Title/waterfall.png`, `Title/spire.png`, `Title/banner.png` | `TitleBackgroundScreen` | Loops: 6 frames of the island's waterfall, 8 of the tower's crystal pulsing, 8 of a banner blowing |
| `Title/petal.png` | `PetalParticleSystem` | One petal, 5x4, at 4x, swinging from side to side as it falls |
| `torch-flame.png` | `TorchSprite` | 6 frames, 12x20, the base at the bottom middle. Each torch starts at its own frame. In every sconce and on the campfire |
| `glow.png` | `TorchSprite`, `WalkerSprite` | A white circle fading to nothing, drawn additively and tinted, so it's whatever colour of light it needs to be. Squashed flat, it's the shadow under the walker |
| `mote.png` | `SparkleParticleSystem`, `TitleBackgroundScreen` | A soft dot. The motes rising off the title's lake, and its stars |
| `button.png` | `ButtonSprite` | Nine-sliced iron plate; a slow smoulder idle, kindles on hover, sinks on press. The bottom band is the groove's light, tinted torch, ember or bone |
| `panel.png` | `FormPanel` | Nine-sliced iron slab the forms are built on, with a bronze line inside the lip |
| `Tiles/questeria.png` | `Tilemap` | 16x16 tiles, eight to a row, in `TilesetCatalog` order, each block of 3x3 tiles one after another: plaza paving, flagstones, cobbles, grass, sand and the canal |
| `Tiles/crypt.png` | `Tilemap` | 16x16 tiles, eight to a row, in `TilesetCatalog` order: floors, the floor in a wall's shadow, steps, the tops of walls, the two rows of a wall's face (one with a burial niche), rubble, bones and a cobweb |
| `Tiles/forest.png` | `Tilemap` | The same layout: dead leaves, the road, the undergrowth under the deep forest, moss and grass in lumps that don't reach the edge of their tile, ferns, a rock, a stump, a fallen log in two halves, a tree trunk, bushes, and the nine pieces of a tree's crown |
| `Props/*.png` | `PropSprite` | Standing on the middle of the bottom edge: a barrel, a crate, an urn, a sarcophagus, the iron sconce a torch stands in, a campfire, a bedroll, and the things you use, each a strip of two pictures (shut then open): a chest, a double door and a stone door in their frames, a lever, and the waypoint stone with its mark dark then lit. The sign is one picture |
| Questeria's `Props/*.png` | `PropSprite` | The same, cut from my pictures: the palace, the temple, the guildhall, the city gate, four houses, the fountain, statues, lamps, banners, benches, market stalls, trees, bushes and flowers, the notice board Questeria's sign looks like, and the waypoint crystal, dim then lit |
| `People/*.png` | `WalkerSprite` | Seventeen townsfolk, cut from my pictures, in the first five columns of the 10x4 layout: standing, and walking |
| `map-marks.png` | `MapScreen`, `ExploreScreen` | 7 marks, 12x12, at 3x on the map screen: a place, a place you only know of, the ring round where you are, a waypoint stone, the brackets round where you're going, you on the local map, and one dot of a road. The map you're walking borrows the dot and the ring, at their own size, for the path you'd take |
| `Paperdoll/*.png` | `Paperdoll` | The body, six hair styles (one of them bald), five faces, four beards, the skirt, the cape and the two tails in halves, four kinds of ears, two of horns, and three hats, all in the youth's 5x4 layout and his key shades, so they stack and recolour. `Stout/` and `Small/` are every one of them again for the shorter builds. The cards on the strip and the faces in conversations are the head and shoulders of the standing frame, blown up |
| `character-palettes.png` | `CharacterPalettes` | Five pixels by forty-five: one ramp per row, dark to light. Skins, hair, eyes, shirts, vests, legs and cloaks, each block with its key first |
| `cursors.png` | `CursorSprite` | 5 cursors, 16x16, at 2x: an arrow, a ring to walk, a hand to use something, a speech bubble to talk, a crossed circle for no way |
| `dice.png` | `DiceRollOverlay` | The d20, 9 frames of 40x40 at 4x: eight tumbling, then the face it lands on with the number drawn over it |
| `dialogue-icons.png` | `DialogueScreen` | 6 icons, 12x12, at 2x, beside an option: a die for a check, a heart for a flirt, a lock, and the three voices |
| `Paintings/*.png` | `PaintingSprite` | The story's paintings, each three or four layers on the title's 416x277 canvas at 4x, swaying against each other like the title: the pier the day you met, the oath under the red moon, and Questeria at dawn from the ship's rail |
| `Tiles/ship.png` | `Tilemap` | The Gull's Wager's deck: planks, the bulwark round it, the hatch, and the sea, with foam where it meets the hull |
| `Tiles/interior.png` | `Tilemap` | Inside the buildings: wood and stone floors, a rug with its edges, the back wall's face (plaster over wainscoting, two tiles tall) and the tops of the walls |
| The ship's and the inside's `Props/*.png` | `PropSprite` | The mast with its sail, the ship's lanterns (which give off light), the wheel and coils of rope; and the bar, round tables, stools, the hearth and candle stands (lit), kegs, a bed, a wardrobe, bookshelves and a shop counter |

Text is drawn with `spectral-title` (Spectral Light, 92pt), `spectral-ui` (Spectral Medium, 26pt)
and `spectral-detail` (Spectral Light, 17pt), letterspaced, the same faces as The Black Box. The
`.spritefont` files name their TTF by relative path, so the build always uses the font shipped in
`Content/Spectral/` rather than whatever is installed on the machine.

## Regenerating the art

Every PNG in `Content/` is made by a script:

```
python tools/generate_ui.py           # button.png, panel.png, map-marks.png, cursors.png, dice.png and dialogue-icons.png
python tools/generate_world.py        # the crypt and forest tilesets, their props, the torch flame, glow.png and mote.png
python tools/generate_harbour.py      # Questeria's and the harbour's tiles, the ship and its deck, its gangplank, mast, lanterns, wheel and rope, and the rowing boat
python tools/generate_interiors.py    # the inside of the buildings: the interior tileset and the furniture
python tools/generate_paintings.py    # the story's paintings (after cut_supplied.py, for the title's city)
python tools/cut_supplied.py          # the title, Questeria's and the harbour's props, and the townsfolk, out of tools/source/
python tools/generate_paperdoll.py    # the paperdoll sheets for all three builds and character-palettes.png, off the youth (after cut_supplied.py's people)
```

They need nothing but CPython. The PNG writer is in `tools/pixels.py` (copied from The Black Box's
`generate_assets.py`), and every drawing is seeded, so running a script again produces the same
bytes; only what you changed changes. Everything on a map is drawn as if a torch were right on it,
lit from the top left, and the game does the dark itself. Tiles that lie next to each other (the
leaves, the floors) only vary in ways that meet cleanly at the edges. A tree's crown is a grid of
round clumps of leaves that wraps round the tile, so one tile's clumps carry on into the next, the
lower clump in front where two overlap, and its edge and corner pieces leave out the clumps on the
outside, so a crown of any size has a round edge. The paintings follow the title: skies dither
between a few colours instead of blending, anything lit is lit from that picture's own light and
goes lavender in shadow, and the people in them are silhouettes edged with the light behind them.

## Supplied art

After the fourth milestone the look changed to a brighter, more mystical fantasy, from pictures I
made with ChatGPT: the title, the capital, Questeria, and the people in it. They're in
`tools/source/`, and `tools/cut_supplied.py` cuts them into the game's layouts, the way The Black
Box's character sheet became its walker. The pictures aren't on a pixel grid (I measured, and
there's no pitch to find), so every cut says how big it should come out. It finds each thing on its
sheet, keys out the flat dark backdrop by flooding in from the edge of its box, so a dark outline
inside it stays, and samples it down to art pixels by taking the middle colour of each cell by
brightness, which keeps the picture's own colours instead of blurring them together. There's nothing
random in it, so a rerun writes the same bytes.

- Questeria's ground was painted as square cells about three tiles across, so each one is a block
  of 3x3 tiles, trimmed of the light line round each cell.
- Everything standing in Questeria is a prop, buildings included.
- The townsfolk come off three sheets: twenty people drawn once each way, six walking each way, and
  one more standing and walking. Three of the twenty turn into someone else when they turn round, so
  they're left out.
- The old road and the barrow are still the generated art from before. The ship's deck, the insides
  of the buildings and the paintings are generated too, drawn to match the pictures (the city in
  Questeria at dawn is the title's), and the party and the story's people are the youth, taken
  apart and dressed again by the paperdoll.

`python tools/check_assets.py` checks that `ASSETS.md`, `Content/Content.mgcb` and the files on
disk agree, and fails with a list if they don't. I run it before every commit.

## Still to come

In the order they're being built:

1. ~~The screens around the game, the sound, the saves and the proof run.~~
2. ~~One map and one walker: the map file format, collision, the camera, walking, the dark with lights in it.~~
3. ~~The three maps joined up: exits, triggers, chests, doors, levers, the map screen and fast travel.~~
4. ~~The party: four members, switching between them, followers, click-to-move with A*.~~
5. The paperdoll and gear: armour you can see, the inventory and the character sheet.
6. Character creation. ~~Your name, people, class and look, and the Soul-Bound's~~; point buy is still to come.
7. Dialogue with dice rolls, approval, the quest and the journal. ~~Conversations, choices, checks
   on the dice, clocks, approval~~; the journal is still to come.
8. Combat, and `--simulate`.
9. Resting, full saving, and the polish.

The story goes in alongside them, in its own order:

1. ~~Valen out and the Soul-Bound in: their origin, their pass on the creation screen, pronouns and
   the tokens, and the meters in the save.~~
2. ~~The conversations the story needs, and the Prologue and Act 1's first chapter rebuilt on
   them. Buildings to go into.~~
3. Act 1, chapters 2 to 4: the Mirewood and the quarry, Thornbridge, the Old Crown Vault, Sunspire
   Keep and the Rusty Anvil, and the companions joining. This is next: chapter 1 ends with the
   Rangers' contract taken, and the old road is where the Mirewood will be.
4. Act 1, chapters 5 to 8, to the end of Act 1.
5. Act 2, with the eclipse.
6. Act 3.
7. Every fight card swapped for its fight, as combat comes in.

The Soul-Link (the two of you fighting as one) comes with combat, and so do the barks in fights.
The Soul-Bound's idle remarks while you walk come with Act 2, when their ambition starts to show
in them. The ship's deck borrows the title's song until the sea has one of its own.

The maps could go through the content pipeline with an importer of their own (that's chapter 11 of
the textbook); for now they're copied as text and read at startup, which is enough to catch mistakes.

## Where things are

- `LegendQuestGame.cs` reads the maps, sets up the window, the screen manager and the sound, and puts the title up. `LegendQuestGame.Proof.cs` takes the proof shots.
- `StateManagement/` is the game state management tutorial: `GameScreen`, `ScreenManager`, `MenuScreen` and `MenuEntry`, `LoadingScreen`, and the input classes from the advanced input tutorial. `InputState` is the only thing that reads the keyboard, mouse or pads.
- `Screens/` is every screen. The explore screen is in partials: `ExploreScreen.cs` for walking and drawing, `ExploreScreen.Party.cs` for the party and the followers, `ExploreScreen.Pointer.cs` for the mouse, `ExploreScreen.World.cs` for using things, triggers, exits, doors in and out and travel, `ExploreScreen.Story.cs` for conversations, scenes, the townsfolk strolling and the ship coming in, and `ExploreScreen.Proof.cs` for its part of the proof schedule.
- `World/` is the maps: `MapLoader` (with `MapObjectLine` for the `[objects]`) and `MapCatalog` to read them, `MapDefinition` and the `Map*` records for what's in one (`MapPresence` for `when` and `unless`), `Tilemap` to draw one, build its walls and see across it, `LightMap` for the dark, `Discovery` for what's been seen, `WorldState` for the rules of using things, `NavGrid`, `Pathfinder` and `BinaryHeap` for A*, `Camera2D`, and the tileset and prop catalogues.
- `Characters/` is the walking sprite, the party (`PartyMember`, `Origin` and `Origins`), people on maps (`Npc`, and their sheets in `Townsfolk`), and how everyone looks (`CharacterLook`, `LookOptions`, `Paperdoll` and `CharacterPalettes`).
- `Dialogue/` is conversations: the script's parts (`DialogueScript`, `DialogueNode`, `DialogueOption`, `DialogueCheck`, `DialogueCondition`, `DialogueEffect`), `DialogueRunner` to play one, `TextTokens` for the words in braces, `DialogueCatalog` for every one of them, and the scripts themselves in `Dialogue/Scripts/`, a file per chapter.
- `Story/` is the story's tables: `Meters`, `KeyItems`, `Paintings`, `Cast` and the quest board's `Postings`.
- `Rules/` is the 5e parts that draw nothing: `AbilityScores`, `AbilityCheck`, and the classes in `ClassCatalog`.
- `Components/` is every screen element, the torches, the toasts, the tooltip, the party's cards, the cursor, the paintings and the quest board, each with `LoadContent`, `Update` and `Draw` the way the tutorials do it.
- `Collisions/` is the collision tutorial's rectangle and helper.
- `Particles/` is the particle engine, and the title's petals and motes built on it.
- `Audio/` is the `AudioManager`. `Save/` is the save file, the slots, and the settings.
- `Enums/` is every enum in the game, one file each.
- `Layers.cs` is the draw order, `Palette.cs` is the colours more than one screen uses, `Text.cs` draws a line of text with its shadow, and `Blends.cs` is the additive blend for anything that glows.
- `Content/Maps/` is the map files. `tools/` is the art and sound generators, the cutting script and the asset check, and `tools/source/` is the pictures it cuts.

## Assets

Every picture, song and sound in the game is original: drawn or synthesised by the scripts in
`tools/`, or cut by one of them from pictures I made. The only outside work is the Spectral font, the MonoGame template files, and a few of
5e's rules from the SRD 5.1, which is credited in there word for word as its licence asks. See
[ASSETS.md](ASSETS.md) for every file.

## Author's Note

