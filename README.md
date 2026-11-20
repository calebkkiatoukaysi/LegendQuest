# Legend Quest

A MonoGame project for CIS 580 - Foundations of Game Programming, Kansas State University.

Legend Quest is a side-on fantasy RPG. You make a character, gather a party of up to four, walk the
maps in real time, talk your way through conversations where the risky options roll a d20 on screen,
and fight in real time, where you stand, when talking doesn't work. What each character wears
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
conversations the story needs (choices, dice, approval), the Prologue, Act 1's first four chapters,
the companions who join in them, and buildings you can go into. Outside the walls the country is
bright, green farmland now, the way a capital's fields should look at noon. [The story](#the-story)
says what's in, and [COMPANIONS.md](COMPANIONS.md) is the companions' bible.

## For the assignment

The project's requirements, and where each one is met:

- **A tilemap rendered for some elements of the game.** Every room's floor is a tilemap. The room
  file in `Content/Zones/` lays out the rows of ground under the floor band (and an interior's back
  wall) as characters, each a tile 16 world pixels square in the zone's tileset (`World/TilesetCatalog.cs`,
  one sheet a set in `Content/Tiles/`, painted 64 texels a tile by `tools/rooms/paint_tiles.py`).
  `World/Zones/ZoneLoader.cs` reads them, and `World/Tilemap.cs` cuts the sheet by its `TileTexels` and
  draws them one texel to a screen pixel under everyone: a variant picked per cell, edges where one kind
  of ground meets another, and animated water. The footsteps read the tile under your feet. All 19 zones, 37 rooms, have one;
  [Rooms](#rooms) has the format and the layers.
- **Serialization of game state to create and load a save file.** `Save/SaveSystem.cs` writes the
  whole game, `Save/SaveData.cs`, to a JSON file with `System.Text.Json`, and reads it back: the map
  and where everyone's standing, the party and what they're wearing, the bag, gold, the story's flags
  and meters, the chests opened and the maps seen. There are three slots, a quicksave and an
  autosave, written safely (a temporary file, then swapped in, the old one kept as a backup) and
  upgraded from older versions; a broken file shows as UNREADABLE rather than crashing. NEW GAME
  writes one, and LOAD and CONTINUE read one. [Saving](#saving) has the details.
- **At least one rendered 3D object, with the 2D.** The d20 a check rolls is a real 3D model, drawn
  among the 2D. `Components/DieModel.cs` builds it in code, nothing loaded: an icosahedron, twenty
  triangles from twelve corners, each face flat with its own normal, numbered so opposite faces add
  up to 21 like a real one, and the numbers painted onto a texture of twenty triangles as it loads.
  It's drawn with a `BasicEffect`: a perspective camera, a lamp at the top left with a shine on it,
  a cool fill light, and a depth buffer. `Components/DiceTable.cs` throws it. It floats over the
  panel slowly turning till you press, then goes up towards the camera (so it grows), tumbles,
  bounces and rolls to a stop with the number the rules rolled on top: the throw is worked out
  backwards from the turn that puts that face towards you. It's drawn on a target of its own at
  twice the size and shrunk onto the 2D canvas, with an ink outline and a shadow under it, so it sits
  in the painted panel. With advantage two are thrown and the lower one fades. The first check is
  at Saltwind Docks, talking the Red Gulls round (`DiceRollOverlay`).

## Running it

Everything is drawn on a 1600x900 canvas (`ScreenCanvas`), and the canvas is blown up to fill the
window, keeping its shape. The window opens as big as fits on the screen it's on, can be dragged to
any size, and **F11** (or DISPLAY on the options) makes it full screen. A whole number of times
bigger is point sampled; in between, it's blown up to the next whole number and shrunk the rest of
the way smoothly, so the pixels stay even. On the canvas the art is drawn small and blown up by a
whole number so the pixels stay square: 3x for the buttons and the panels, 4x for the picture
behind the title, which is 416x277 art pixels, a little bigger than the canvas so its layers can
sway, and 4x for a room, which is exactly the canvas's height. The world doesn't zoom.

```
dotnet run
```

**NEW GAME** opens the save slots, and picking one starts the Prologue: you make yourself, then
the Soul-Bound, your oldest friend, then you sail for Questeria (see [The story](#the-story)).
**LOAD** opens them to carry a game on, **CONTINUE** carries on whichever slot was saved last (it's
dead when there isn't one), **OPTIONS** has the volumes and the text speed, and **EXIT** and **Esc**
close the game. In a room, clicking the floor walks you there, clicking someone, something or a door
walks you up to it, E uses whatever you're standing next to (a door too), holding Alt shows everything
at once, Tab changes who you're controlling, M is the zone map, and Esc is the pause menu.

One switch exists for working on the game, and it isn't the game:

```
dotnet run -- --proof shots       # play through the screens, save a picture of each to shots/, and quit
dotnet run -- --proof shots Docks # the same, but stop after that part of the run (a ProofStage)
dotnet run -- --proof shots Hud   # only the real-time HUD, drawn from made-up views, and its icons (see The real-time HUD)
dotnet run -- --proof shots Arena   # story fights from the gulls to the sappers, and the fields, each a fresh game (Combat, On screen)
dotnet run -- --proof shots Boss    # the Thornmother through her three phases, her Thorn Burst filmed frame by frame
dotnet run -- --proof shots Classes # six classes in your place, each pressing its skills, two ultimates filmed
```

Set `LQ_ARENA_ONLY` to part of a bout's shot path (`field/`, `sappers`) to run only those bouts of
Arena, Boss or Classes.

The story has four checks of its own, run before every commit with `dotnet build` and
`python tools/check_assets.py` (`Story/01-build-plan.md` §0):

```
python tools/story/compile_scripts.py          # Story/04 to 08 into Dialogue/Scripts/Generated/
python tools/story/compile_scripts.py --check  # exit 1 if the generated C# is out of date, a script won't read, or an id is in both
python tools/story/compile_scripts.py --diff a1-c3-02  # a hand-written script beside the one generated for it
python story/tools/lint_dialogue.py            # the writing guide's mechanical rules
python story/tools/build_registry.py --check   # rewrites Story/09-registry.md; exit 1 if a flag is read but never set
```

It's the same harness as The Black Box's. It goes through the title, the options, the save slots
for loading (and erases a broken slot the way a player would, ERASE then SURE?), the save slots for
a new game, then the Prologue: its card, the creation screen (you as you start, then named, and
each of its tabs), the question that picks your voice, the Soul-Bound as they start and then named,
and the two paintings, the pier and the red moon. On the deck of the Gull's Wager it walks along to
the red "!" at the bow, and the voyage gets read through, answering its questions, past the lookout
seeing the sea-wraiths and the fight to Questeria at dawn. On the harbourside the arrival plays
itself, its shots on its own clock: the ship sailing in, the sail coming in, tied up, the plank
sliding out, the walk down it and along the quay, and Act 1's card. Then the red "!" over the Red
Gulls, and a walk east into them: it tries talking first, the Persuade check comes up on the dice,
it calls the Soul-Bound off Grell, and Captain Hale arrives; then through the fish market and up
into the city. Questeria goes room by room. In the gate plaza, the Soul-Bound talking as you come
in, and the label at the plaza's east end. In the festival square, the square and the same view a
few seconds on with the townsfolk moved, the mouse over the far side of the fountain (the path goes
round its ellipse) and a click to walk it, the mouse on the elder and a click to walk up and talk to
them, the mouse up over the painting where there's no way, Tab, a click on a card, the mouse on the
city gate (its outline, its plaque and the door cursor), Alt held, the stair's chevrons under the
mouse and a click that walks up them and back down, and the waypoint crystal and the zone map's
world. In the Lantern Market, the camera along it, Fennick and the quest board, you sent off on your
own while the Soul-Bound finds their way after you, the Gilded Flagon's tankard with the
innkeeper's "!" over it, the zone map and a jump from it back to the square, and in through the
Flagon's door. Inside it takes the innkeeper's work and goes back out the door, which leaves you on
the street outside it, then out through the city gate. On the old road it walks in, walks into the
trigger in the farmland, then at the fork opens the chest, reads the sign, looks at the zone map,
finds the second waypoint and fast travels back to Questeria, then straight back to the road by it
and east into the Mirewood. From there it plays Act 1's chapters 2 to 4 through: the gloomfangs and
Sera, the quarry's roots, the courier's letter, the Thornmother and the crystal, the night camp and
a word with Sera, Fennick paying out, Thornbridge's gate (talked open on the dice), Mira and her
errands, the Hollowed raid at the river, the royal messenger, the hooded woman in the Flagon, the
Old Crown Vault's plates and Kael, the audience with the King and Crane in the great hall, Brannock
in the Rusty Anvil, and Kael and Brannock waiting at the Flagon, with Kael swapped in for Sera. Every
visit to a zone is a queue of beats (walk into this, talk to them, read it through, go out that way)
rather than frames, since a conversation's length depends on its answers and a room change on its
fade (`ExploreScreen.Proof.cs`, `ExploreScreen.ProofBeats.cs` and `ExploreScreen.ProofChapters.cs`).
The beats find people, triggers, doors, ways out, things and spawns by their ids in the zone file,
and change room first, through the same fade a player gets, when one's in another room of the zone.
Every fight on the way is fought, from the sea-wraiths to the sappers: whenever one's on, it has the
run to itself (`ExploreScreen.ProofFights.cs`): the party's AI drives the leader as well as everyone
else, in real time on the run's lockstep clock, and there's a shot of each fight in the thick of it
and of its end (Combat, On screen). The tutorial at sea starts with the pretend mouse clicking a
wraith, for the target picked and later the log. The zones' dice are seeded under the run, so it fights the same way
each time, and a part's clock stops while a fight's on. A fight the party loses goes to game over
like anyone's, and the run loads the save from just before it and fights it again on the next seed,
five times at most before it says one's too hard. In the Flagon it opens the bag, the character
sheet, the forge's shop, the help's first page and its page on fights, and a tip. Then back to the
road and in at the barrow's gate. In the barrow it walks into the dark and the cold, tries the
locked tomb door and the stone door, pulls the lever that opens the stone door, goes through it to
the side room's chest, opens the zone map, stands under a torch, pauses, and goes out through QUIT
TO TITLE, where the slots show the autosave it made on the way. Moving and using things go in
through the same code the keyboard does, and the mouse is a pretend one inside `InputState` that the
screen reads the same as the real one; it only jumps about between scenes. It uses a scratch save
folder inside `shots/` that it fills with one of every kind of slot, so it never touches a real one,
and it ignores the real mouse and keyboard so nothing I do while it runs changes the pictures. It's
silent: every sound it would have played goes in `shots/audio.log` with the frame it was asked for
on, which is how I check the footsteps change from grass to stone and the door grinds when the
lever's pulled. A second switch, `--simulate`, fights an encounter headless (see [Combat](#combat)).

### Controls

| Where | Keys |
| --- | --- |
| Menus and forms | **Arrows** or **WASD** to move, **Enter** or **Space** to pick, **Esc** to go back. The mouse works too |
| Options | **Left** and **right** change the selected row |
| Save slots | **Left** and **right** move between a slot and its ERASE |
| Walking a room | **A** and **D** or **left** and **right** to walk along it, **W** and **S** or **up** and **down** to walk deeper or nearer on the floor, **E**, **Space** or **Enter** to use or talk to what you're next to or go through the door you're at, **M** for the zone map, **R** to rest (everyone kneels and gets 5% of their hit points back a second until they're full or you move; walking about gives 1% every 2 s anyway), **Esc** to pause. Walk off either end, or into a stair, to go on. Everyone faces left or right |
| Walking a room, the mouse | **Left click** the floor to walk there (the path shows while you point), someone, something or a door to walk up to it and talk, use it or go through, or a way out's chevrons to walk into it. Above the floor there's only the painting. Pointing at anything says what it is |
| The party | **Tab** for the next in line, **F1** to **F4** for one in particular, or click their card or them |
| Talking to the party | **E** (with nobody and nothing else in reach) or a **click** on one of them talks to them, when they've something to say: the cursor's a speech bubble, the tooltip ends TALK, and a gold "!" floats over anyone with something new |
| Making a character | **Up** and **down** pick a row, **left** and **right** change it, **Q** and **E** change the tab. On NAME, **Enter** and then type |
| Conversations | **Enter**, **Space**, **E** or a click shows the rest of the line, then the next. At a question, **1** to **4**, a click, or the **arrows** and **Enter** |
| The dice | **Space**, **Enter** or a click rolls, and the same again goes on |
| The ship coming in | **Esc** skips it |
| Anywhere | **F11** switches full screen on and off |
| In a room | Hold **Alt** to see everything at once: every door's badge (a plain door's only shows under the mouse otherwise), every name, every way out's label and every glint on something still to use. A way out's label also shows once you're near it |
| The zone map | **M** in a room shows the zone's rooms laid out the way they join, with their badges and "!"s. The **arrows** or the mouse pick a room, and **Enter** or a click jumps to it if you've been there, but not in a fight or a scene. **Tab** switches to the world, which is where it opens from a waypoint, to pick somewhere to travel to. **M** or **Esc** closes it |
| A tip | **Enter** or a **click** on its card puts it away. It never stops you walking, and goes by itself once there's been time to read it twice |
| How to play | **Esc**, then HOW TO PLAY. **Left** and **right** (or **Q** and **E**) turn the page, a **click** on a tab jumps to it, and **Esc** goes back |
| In a fight | **Click** a foe to target it (your auto attack comes on and you walk into its reach), a friend to target them (and help them up if they're down), or the floor to walk there; **WASD** or the **arrows** walk too. **1** to **6** or a **click** use the bar: **1** the auto attack on and off, **2** to **5** your skills on your target (walking into reach first), **6** a healing potion; **7** or the sigil is the Soul-Link Strike. **Tab** picks the next foe, nearest first (a run of presses keeps the order it started with), **T** your target's target, **Esc** clears your target (and with none, pauses), **L** or a click on its tab shows and hides the log (the **wheel** scrolls it), and **Alt** shows everyone's numbers. Pointing at anything says what it is |
| The end of a fight | **Enter**, **Space** or a **click** on the panel goes on to what happens next |
| A foe in a room | A red name is a foe you can fight (a field, Combat, On screen): **click** it and the fight starts, with it your target. Some come at you when you come near |
| Debug builds only | **F9** puts whoever you're controlling in the next class (all twenty-five), dressed in its kit; **F10** gives them the next rank in it. For trying a class's skills, in a scratch game |
| The inventory | **I** for the bag, **C** for the character sheet, or **Esc** and then INVENTORY. **Q** and **E** (or **F1** to **F4**, or a **click** on a face) pick who you're looking at, **Tab** turns between the bag and the sheet, and **I**, **C** or **Esc** closes it. On a pad, the shoulders pick who, Y turns the page, A uses and B closes |
| In the inventory | The **arrows** move round the slots, the bag and the key items. **Enter** or a **click** puts on what's in the bag (whatever was there goes back in it), drinks a potion, or takes off what's in a slot; on an empty slot it finds something in the bag for it. The **mouse wheel** scrolls the bag. On the sheet, **up** and **down** pick an ability to read |
| A shop | Talk to the keeper. **Enter** or a **click** buys one or sells one, and with **Shift** held it sells the whole stack. **Tab** crosses the counter, **Q** and **E** pick who the cards measure against, **Esc** leaves |

A gamepad works anywhere the keyboard does outside a fight: the d-pad or left stick, A to use, B to
back out, Y for the zone map, the right shoulder for the next in the party, the left shoulder held
for what Alt shows, X to put a tip away, and Start. In a fight a pad can walk and turn through the
foes, but the bar (1 to 7), assist (T), the log (L) and resting (R) are the keyboard's and the
mouse's for now.

## Screens

Every screen is a `GameScreen` on a `ScreenManager`, the way the game state management tutorial
does it (`StateManagement/` is that tutorial's code, carried over from The Black Box). Each has its
own `TransitionOnTime` and `TransitionOffTime`, so everything fades instead of cutting.

| Screen | What it is |
| --- | --- |
| `TitleBackgroundScreen` | Questeria at twilight behind the title and every form on it, moving. It doesn't transition off when something covers it, so it stays up behind all of them |
| `TitleScreen` | The title, with NEW GAME, CONTINUE, LOAD, OPTIONS and EXIT |
| `OptionsScreen` | Master, music and sound volume, how fast text types out, and whether the tutorial's tips come up. Saved to `settings.json` |
| `SaveSlotScreen` | The save slots, for a new game or for loading one, with ERASE beside each slot that has something in it |
| `CharacterCreationScreen` | Making you, then the Soul-Bound: rows for a name, a people, a class and every part of a look, on three tabs, with them walking on the spot and turning round beside the rows, and their face blown up over that. RANDOM, BEGIN and BACK. The Prologue's first lines play between the two |
| `DialogueScreen` | Popup. A conversation a line at a time, over the dimmed map or a painting: the speaker's face and name, the line typing out at the text speed, and a question's options numbered under it, with an icon for a check, a flirt or a locked one. Approval, gold and items come up as toasts |
| `DiceRollOverlay` | Popup, over a conversation. A check: the d20 (in 3D, `DiceTable`) with the DC and every modifier, a press to throw it, the tumble, then the total and SUCCESS or FAILURE |
| `ExploreScreen` | A zone's room being walked along: its painted layers and tile strip, the props, the things you can use, the people, the party, and the dark if it's a dark room. The party's cards are along the bottom left, toasts come up near the top (what was in a chest, what a sign says), and whatever the mouse is on gets a tooltip and its own cursor |
| `ZoneMapScreen` | Popup, from **M** or a waypoint. The zone you're in as its rooms, laid out as they join, with their badges and markers, and the world: every place you've been and the roads between them. Any room you've walked into can be jumped to outside a fight or a scene; opened from a waypoint, it's where you pick somewhere to travel to |
| `InventoryScreen` | Popup, from **I**, **C** or the pause menu. The party's faces down the left, whoever's picked turning on the spot in what they're wearing with their nine slots round them, and the bag, the gold and the key items or their character sheet on the right (see [Items and inventory](#items-and-inventory)) |
| `ShopScreen` | Popup, from a shopkeeper's conversation. Their shelf at their prices on the left and the bag at half on the right; a click buys or sells |
| `PauseMenuScreen` | RESUME, INVENTORY, OPTIONS, HOW TO PLAY or QUIT TO TITLE. Nothing under it moves while it's up. SAVE and LOAD go on it once there's a game to save. In a fight there's no INVENTORY, and its line says there's no saving mid-fight |
| `GameOverScreen` | Popup, over the fight that was lost. LOAD LAST SAVE (the autosave from just before the fight, unless you've saved since) or QUIT TO TITLE |
| `HelpScreen` | Popup, from HOW TO PLAY. Every control on the keyboard, the mouse and a pad, then every tutorial tip under its topic, with the keys as key caps (see [The tutorial](#the-tutorial)) |
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
  and P-05 are the voyage: the deck of the Gull's Wager at night, two days out, where you can walk
  about and hear the crew before you go to the rail at the bow (the red "!"), then the Soul-Bound
  talking about the new king, the bell from the crow's nest, and the sea-wraiths up over the port
  rail: the first fight, and a guided one, where the wraiths teach you a step at a time to pick a
  foe, let the auto attack swing, press your first skill, read the cooldowns and Focus, drink the
  potion, and fill and spend the Soul-Link. Then Questeria at dawn.
- **Act 1, Chapter 1: Saltwind Welcome.** The ship comes into Saltwind Docks and Act 1's card comes
  up. Across the boardwalk, under a red "!", the Red Gulls are shaking old Tomas down for a dock tax
  that doesn't exist, and it starts when you walk up to them. You can wade in,
  try talking first (a Persuade check, with advantage for a Skald or an Honorable voice), or say it's
  not your business, in which case the Soul-Bound goes in anyway. Every way in ends in a brawl with
  the three of them and their bosun, Grell: cudgels and fists, and nobody dies. Then Grell is on his
  knees with the Soul-Bound's blade at his throat a beat too long, and what happens to him is up to
  you. Captain Hale's Watch arrives, and Tomas gives you his lucky hook. Up in the city the
  Soul-Bound points the way, Fennick talks you through the quest board in the Lantern Market (the
  Rangers' contract for the Mirewood glows on it), and the innkeeper in the Gilded Flagon has work
  if you want it.
- **Act 1, Chapter 2: The Rot in the Mirewood.** The Rangers' contract takes you east down the old
  road, past where the woodcutters have sawn through the fallen tree, into the Mirewood: green at
  its edge, and rotting further in, with black in the bark and tar in the stream. Gloomfangs with
  smoke leaking from their eyes, then Sera's arrow, and Sera, who joins you. In the Old Quarry she
  talks two walls of roots apart, a dead Doomward courier's letter says an army is coming, and the
  Thornmother guards the black crystal. Then the party's first argument: purify it Sera's way,
  smash it the Soul-Bound's, or hold the quarry while Sera works. A night camp after, where Sera
  watches the Soul-Bound from the dark, and Fennick pays triple back in the city.
- **Act 1, Chapter 3: Ashes at Thornbridge.** Out of the east gate, a tent camp of Doomward
  refugees has swollen overnight in the meadows, and Sergeant Brisk won't let them in. Mira
  Ashgrove is healing the sick: help her with her errands, hear about her teacher Maleficio, then
  the Hollowed come out of the trees. After the raid the Soul-Bound can calm down, or give the camp a
  speech it won't forget. Mira joins.
- **Act 1, Chapter 4: The Sunheart.** A royal messenger sends you into the Gilded Flagon, where a
  hooded Princess Elara wants the Sunheart back from the Old Crown Vault before anyone knows it's
  gone. Down there are pressure plates, dart traps, dormant constructs, and Kael Duskmantle up a
  relic shelf; then Hollowed sappers break in from below and it's a truce. Kael's black-iron coin
  says someone near the crown is paid by Doomward. King Leon thanks you in the Sunspire, the
  Soul-Bound speaks out of turn, the King sends you to Greywatch with a caravan, and Chancellor Crane
  has a quiet word with the Soul-Bound on the way out. Brannock Ironvein, drinking in the Rusty Anvil,
  makes six.
- **The companions.** Sera, Mira, Kael and Brannock each have their own conversations as the
  chapters go: talk to them (a gold "!" says when there's something new) and you learn who they are,
  why, and what they'd want from you; each ends this stretch with their personal quest's hook. With
  four of you already, the rest wait at the Gilded Flagon and can swap in there. They talk among
  themselves too while you walk, once everyone in the conversation is with you.
  [COMPANIONS.md](COMPANIONS.md) is their bible: the research behind them, each one's want, need,
  wound and lie, how they talk, what they approve of, their arcs through all three acts, their
  personal quests and romances, and what each does when the Soul-Bound turns.
- **Round the edges.** The city and the harbour are full of people with something to say: a dozen
  you can stop and talk to (`Townspeople`), each with a few choices and something new the second
  time, and four with work for you, marked in gold, that are the side quests' hooks (rats with black
  eyes in the Flagon's cellar, a merchant's stolen ledger, highwaymen on the Mirewood road, a
  widow's ring in the harbour). Forty-odd more say things as you pass, and the two of you talk about
  wherever you are as you walk in.

Everyone in it who isn't in the party is in `Cast`, with the name they go by, which townsfolk sheet
or paperdoll look they wear, and who's only ever a voice. Captain Odelle, the Red Gulls, Grell,
Captain Hale, the Watch and Fennick are drawn the party's way, so they can wear bandanas, a tricorn
or a helm; Old Tomas is one of the townsfolk. The
story's bookkeeping is in `Story/`: the meters (`Meters`: the bond between you and the Soul-Bound,
their ambition, each companion's approval, fame, and how far the eclipse has gone, each with where
it starts and its range), the key items, the paintings and the quest board's postings. Every table
there throws at startup if a row's wrong.

Every fight in the story is fought where it happens, on that map. The conversation before it ends
by starting it (`DialogueEffect.Fight`, with the encounter's id), the foes are the people already
standing there or the ones who come over a rail, down a passage or up out of a hole, and winning it
sets the flag the scene always set, so the map changes, and plays the conversation after it (the
encounter's `Then`, called `<encounter>-after`), so the scene carries on where it left off. The
Soul-Bound's barks are the fight's own: the Soul-Link's two lines from P-05, and "Where's the
Watch?! Where's the king's army?!" when Thornbridge's second wave comes out of the trees. Kael's duel
ends when he gives up, the sappers break in, and the truce goes straight into the fight with them;
holding the quarry while Sera purifies the crystal is a fight of its own, and she sits it out.

## Walking a room

`ExploreScreen` is one visit to a zone: one of its rooms at a time ([Rooms](#rooms) has the zone file
and the painting). Going into another room of the same zone fades it out over a quarter of a second,
swaps the room in, fades back in over another, and writes the autosave; there's no loading screen.
Going to another zone is a new screen through the loading screen, with the autosave written before
anything fades, as an exit always did. The party comes with you either way.

- **The floor.** Feet are a point with a 10 by 4 box over it, the same feet box the maps had. The
  point has to stay on the room's floor band, inside its polygon and out of its ellipses (a
  fountain, a well), and the box out of the footprints of whatever's standing in the room: the props,
  the things you use, and anyone standing still. `World/Rooms/RoomWalker.cs` does the moving, one
  axis at a time like The Black Box's lobby: a footprint pushes the feet back out right against its
  edge, and a step that would leave the floor looks a little either side of itself for floor (up to
  half as far again as the step), so walking along a sloping back edge or into a fountain slides you
  along it instead of stopping you dead. Feet a scene left off the floor move freely until they're
  back on it, so nobody's ever stuck. A/D walk along the room and W/S deeper and nearer; there's no
  scaling with depth, the band's too shallow to need it.
- **Facing.** Everyone faces left or right, and only ever left or right: `WalkerSprite.SideOnly` makes
  setting up or down do nothing and turns someone to a point by which side of them it's on. Walking
  mostly deeper or nearer keeps the way you face.
- **Paths.** Clicking the floor walks there by A*, over a grid of 8 by 8 world-pixel cells (32 canvas
  pixels) laid over the room: a cell's walkable if its middle is on the floor and no footprint, grown
  by the feet box, covers it, so a path's feet clear everything and a straight stretch only has to be
  looked along. `NavGrid` and `Pathfinder` take any cell size, corner and rule for what's solid. A click just past the edge of the band goes
  to the nearest cell a path reaches, then the point itself if feet fit there. Above the floor
  there's only the painting, so a click there that isn't on a door or a way out's chevrons goes
  nowhere, and the cursor says so.
- **People.** The townsfolk who wander stroll along the floor, never further deeper or nearer than
  15% of how far they wander (`RoomWalker.Stroll` picks somewhere they fit). A patrol walks from one
  end of its stretch to the other and back, resting at each end, and turns back early if something's
  in its way. Everyone glances left and right. Someone walking about doesn't block you, the way
  AQW's townsfolk don't, and stops short of you instead; someone standing does, and the paths go
  round them. The Gull's Wager's crew stand on her deck, wherever she is, sorted between her and her
  near rail.
- **Followers** walk your trail as they always have, find a path if they fall behind or get stuck,
  and come with you into the next room, where they're put behind you a party's spacing apart, along
  the room before deeper or nearer (`RoomWalker.SpotsNear`).
- **Ways out.** An edge exit is the last 12 world pixels (48 canvas pixels) at either end of the
  floor: walk off the end, or click its chevrons, and you go. A stair is a strip at the back of the
  floor (up) or the front (down) at its foot. A door is a box in the painting standing on the back of
  the floor: E uses it when you're under it near the back of the floor, and a click on the doorway
  or its badge walks you there and goes through. Each goes to another room of the zone, to a spawn in
  another zone, or back out to wherever you came in. A locked one (the twin of a door or exit that's
  there once a flag's set) plays the locked door and says why. Going through a door into another
  zone keeps where you came in, in front of the door, so the building's way out puts you back there.
- **Triggers** span the whole depth of the floor, so you can't walk round one. They and the ways out
  go off as your feet walk into them, not while you stand in one, and not if you arrive standing in
  one. A trigger that goes off once is kept in the save as used.
- **Using things.** Whatever's nearest your feet and still has something to do (an open chest
  doesn't) gets its name over it and `E · OPEN` or `PULL` or `READ` or `TRAVEL` under that, and a
  door you're at gets `E · ENTER` (or `E · LOCKED`). `WorldState` has the rules for things: it reads
  and changes the save and hands back what happened, a chest's lid coming up and then the gold, say,
  and the screen plays a sound and puts up a line for each, the same as The Black Box's `RoundEngine`
  and `TableCues`. Using a waypoint finds it, then opens the zone map's world to travel from it to any
  other waypoint you've found; travel puts you beside the stone, since in front of it you'd be in the
  way of it.
- **The camera** is locked to the room at 4x (`Camera2D.LockTo`), exactly the screen's height, so it
  only slides along the room after you and stops at its ends. The wheel doesn't zoom.
- **Drawing**, in order: `RoomScene.DrawBack` (the sky, the far layer, the tile strip and the mid
  layer), one world batch sorted back to front by how far down the floor feet are (`RoomScene.Depth`;
  a prop by its foot, less its `SortRise`), `RoomScene.DrawFront` (the foreground), `RoomScene.DrawDark`
  in a dark room, then the light added over it (each torch's flicker, a scene's glows, the pool under
  whatever the mouse is on), the path you'd walk, `IndicatorLayer` (badges, chevrons and their
  labels, names over heads, glints; all at once while **Alt** is held), and the screen: the "!"s, the
  lines over heads, the prompt, the cards and the heading with the zone's and the room's names. The
  "!"s and the lines go over anyone's name plate, not on it. A marker off the screen gets an arrow at
  the side it's off.
- **Tint** is the colour of the light in a room. The room's layers, the props and everyone standing
  in it, the party too, are multiplied by it, and nothing on the screen over them is: the heading, the
  prompts, the markers and the cards stay their own colours. The flames aren't tinted either, since
  they're the light and not what it falls on. It goes on a dark room as well as a lit one: the tint is
  the colour of the light, and the dark is how much of it there is.

| Zone | Rooms, left to right | |
| --- | --- | --- |
| `gulls-wager` | deck | The Gull's Wager at night, two days out. Where a new game starts, and the voyage |
| `docks` | pier, harbourside, fish-market | Saltwind Docks. The ship comes in at the harbourside, where the Red Gulls are shaking down Old Tomas; the Rusty Anvil's door is there too |
| `questeria` | gate-plaza, festival-square, sunspire-steps, lantern-market, thornbridge-gate | The capital: the plaza in from the docks, the festival square with the waypoint and the city gate, the steps up to the keep and the vault (a stair up from the square), the Lantern Market with the Gilded Flagon, Fennick and the shops, and the east gate |
| `road` | farmland, fork | The old road: the farms, then the fork with the camp, the barrow's gate and the way east to the Mirewood |
| `barrow` | steps, hall, side-room | Down the steps into the hall, the tomb door, and the stone door to the side room |
| `mirewood`, `mirewood-quarry` | edge, deep, camp; lip, cut, floor | The Mirewood and its Old Quarry, the roots Sera opens between the quarry's rooms |
| `thornbridge` | camp-gate, tents, river-edge | The refugee camp outside the east gate |
| `old-crown-vault`, `sunspire-keep` | entrance, halls, strongroom, treasury; great-hall | Under the Sunspire, and the King's hall |
| `gilded-flagon`, `rusty-anvil` | taproom; bar | The inns. The camp waits in the Flagon's taproom |
| `bakery`, `chapel`, `cottage-a`, `cottage-b`, `forge`, `shop`, `warehouse` | inside | One room each, through a door |

The old top-down tile maps the zones were converted from are kept in `tools/rooms/legacy/`, as
`tools/rooms/convert_maps.py`'s input. The game doesn't read them: it walks, draws, fights and saves
by the rooms alone.

## Rooms

Since the third milestone the world is side on, the way AdventureQuest Worlds lays out its maps: a
place is a zone, a chain of painted rooms you walk along, left and right and a little way in depth
along the floor. The arrows at the ends take you to the next room, and the doors in the painting
take you inside. Everything in a room is my own art, the props, buildings and tilesets the top-down
maps were made of, stood up side on and drawn in more detail.

A zone is a text file in `Content/Zones/`, one `[room]` section a room, left to right. Its numbers are
canvas pixels: a room is 1600 to 4800 wide and always 900 tall, and the game divides everything by 4
(`ZoneCatalog.RoomScale`) once, so a walker, a prop or a reach keeps the size it had on the maps.
Each room has its floor band (a polygon where feet can go, with ellipses it can't), its tile strip
(`tiles`, `tiletop`, a `legend` and a `row` per row of tiles, and `detail` over them), what stands in
its painted layers (`scenery far|mid|fore <prop> <x>`), the little things that move in it (`life` boxes
of gulls, clouds, smoke, flags, shimmer, leaves, motes, ash or candles), its exits and doors (a door's
box stands on the band's back edge, sized by its badge), and its spawns, people, props, chests,
signs, triggers, scenes, camp seats and lights. `tools/rooms/zonefile.py` describes every line.

What you see is `World/Rooms/RoomScene.cs` drawing the room's layers, from
`Content/Rooms/<zone>/<room>/`. Every layer is the screen's height, and is drawn at the scale its
height gives: a painted layer is 900 texels tall, one texel to a screen pixel, so nothing is blown
up; a room not painted yet still has its placeholder's layers, 225, 450 or 300 tall, drawn 4, 2 or
3 times. A layer wider than the widest texture (4096) is written in pieces, `mid-0.png`,
`mid-1.png` and on, drawn side by side.

| Layer | Parallax | What's in it |
| --- | --- | --- |
| `sky.png` | 0.1 | A painted sky, blue going pink by day (dusk, night or twilight where the room wants it), its clouds, the sun or the moon |
| `far.png` | 0.3 | The distance, softer and cooler: mountains, hills, the city's roofs, the Sunspire, the sea |
| the tile strip | 1 | The floor, drawn by `Tilemap` from the room file |
| `mid.png` | 1 | The buildings, each on its door, and the street's dressing (trees, stalls, lanterns, banners, crates), or indoors the wall |
| `fore.png` | 1.15 | What's between you and the room, dark, never more than 12% of the floor band |

An interior has no sky or far layer and no parallax. In a dark room the dark (`LightMap`, with the
lights cut out of it) goes over the floor, the mid layer, the people and the foreground, but not the
sky or the far layer: a painted night sky already has its light. `doors.txt` says where each door's
doorway is painted, which is what the door's outline and badge go round; what you click is the
door's bigger box. `thumb.png` is the room for the zone map. The life loops (`LifeLoops`) are
painted too, in `Content/Rooms/Life`: drifting clouds, gulls, waving flags, torch and candle flames
with their glow, and the smoke, motes, ash, sparkles and petals the particles draw with.

The rooms are painted in the game's illustrated style (crisp dark outlines, cel shading lit from
the top left, soft gradients only in the sky and the distance) by `tools/rooms/paint_all.py`: one
recipe a room, in a module a family (`paint_rooms_questeria.py` and the rest), each built out of
`tools/rooms/paint.py`, the drawing library (skies, mountains, castles, houses put together from
walls, roofs, windows and doors sized to the room file's door boxes, towers, gates, stairs, trees,
props and interiors), on the rig's `vector.py` and `style.py`. It paints the floor tilesets too
(`paint_tiles.py`, 64 texels a tile, so `Tilemap` draws a tile one texel to a screen pixel:
`TilesetDefinition.TileTexels`), the floor props (`paint_props.py`, into `Content/Props/Painted`,
which `PropSprite` draws in place of the old picture, 4 texels to a world pixel), and the life
loops (`paint_life.py`), and keeps `Content.mgcb` and `ASSETS.md` up to date. It's seeded, so a
rerun writes the same bytes, and `--preview` writes what the game will show into a folder outside
`Content/`, with the tile strip, the props and two of the cast at their real size.

A room with no recipe yet still has the layers `tools/rooms/generate_room_placeholders.py` put
together out of the game's old pixel art.

I'm painting the rooms myself, in ChatGPT, and these tools turn the pictures into rooms
(`tools/rooms/sourcekit.py` has the details):

```
python tools/rooms/make_room_template.py      # the guide for every panel of every room, in shots/art/room-templates/
python tools/rooms/cut_room.py                # every room with its pictures: the flat mid layer, foreground, doors.txt, thumbnail
python tools/rooms/cut_tiles.py               # the floor textures into the tilesets, and tiles-<tileset>.txt, what each needs
python tools/rooms/check_room_sources.py      # what's painted and what's still to do
```

A room W canvas pixels wide is painted in panels, ChatGPT's 1536 x 1024 landscapes: one for a room
1600 wide, else ceil((W - 1600) / 1400) + 1, each 1600 pixels of the room, overlapping the next by
200 and the last against the room's end. Only the middle 1536 x 864 of each (16:9) is used; the top
and bottom 80 rows are bleed. Each panel's guide, `<zone>-<room>-<i>.png` (from 1), marks the bleed,
the horizon or the back wall's foot, the floor band to keep plain, every door's box with its badge
and name, the ways out, and the overlaps that have to carry on from the next panel. The pictures go
in `tools/source/rooms/<zone>/<room>/`: `bg-<i>.png`, the whole scene, and `fg-<i>.png` if a panel
has a foreground strip (on transparency or a flat #FF00FF). `cut_room.py` resizes each band to
1600 x 900, cross-fades the panels across their overlaps, and writes the room flat: the one picture
as its mid layer, opaque down to the floor band, where it fades into the tile strip, so the floor
stays a tilemap; the foreground keyed off its backdrop; no sky or far layer, which is what tells
`RoomScene` the room is flat, nothing sliding. It writes doors.txt from the zone file's door boxes and
the thumbnail, and puts it all in `Content.mgcb` and `ASSETS.md`. The floor textures are
`tools/source/tiles/<tileset>/<tile>.png`, 1024 x 1024 and seamless, and `cut_tiles.py` resamples them
to 64 texels onto the sheet in `TilesetCatalog`'s order, keeping any tile there's no texture for. A
room with no pictures keeps the art it has, and the painters leave a room with pictures alone.

## The party

Up to four: you, and whoever's with you. A new game starts you with the Soul-Bound, and the story
recruits the rest: Sera in the Mirewood, Mira at Thornbridge, Kael in the Old Crown Vault and
Brannock in the Rusty Anvil, each with their people, class and look. When there are four of you
already, anyone else who joins waits at camp, which is the Gilded Flagon.

| File | What it is |
| --- | --- |
| `PartyMember` | Someone in the party, as the save keeps them: name, pronouns, class, level, experience, ability scores, hit points, what they're wearing (item ids by slot, changed with `Equip`, `Unequip` and `PutOn`), what they've spent since they last rested, look, and where they're standing. The most hit points they can have includes anything worn that adds to it |
| `Origins` | Everyone who can be in the party and how they start: class, scores (5e's standard array, 15 14 13 12 10 8, where their class recommends), look and kit. A companion the story arms sets out in their own (Sera's shortbow, Kael's twin daggers, Brannock's round shield); everyone else in their class's. You and the Soul-Bound start from theirs and the creation screen does the rest |
| `ClassCatalog` | The 25 classes, each with its kit (`SkillCatalog`): the eight base ones have the SRD's names in the code and the story's on screen (a Fighter is a Swordhand, a Rogue a Shadowstep, a Cleric a Lightbearer, a Wizard an Arcanist, a Paladin a Warden, a Druid a Wildcaller, a Warlock a Hexblade and a Bard a Skald), each with its role, its hit die, where its scores go, the armour, shields and weapons it's trained with (every class has the simple weapons), the ability it casts with, its speed, and its starting kit: its weapon, shield and armour, and a travelling cloak and boots. A kit it couldn't wear stops the game at startup |
| `AbilityCheck` | A check: a d20 plus a modifier against a DC, with or without advantage, and a natural 20 or 1 deciding it whatever the total |
| `AbilityScores` | The six scores, and 5e's modifier: half of (score minus 10), rounded down |
| `DiceExpression` | Dice written 5e's way (`2d6+3`, `1d8`, `3`), parsed once and rolled into a `RollResult` with every die and every modifier by where it came from; a critical rolls the dice twice and the bonus once. `Dice` rolls one die |
| `AttackMath` | The d20 with advantage or disadvantage, whether a roll hits (a natural 20 always, a natural 1 never), the exact chance it will for the tooltip, and how several advantages and disadvantages cancel |
| `ArmourClass` | AC from what's worn: the armour's base, Dexterity up to its cap (all for light, +2 for medium, none for heavy), and every worn bonus, a shield's +2 among them. 10 plus Dexterity in nothing |

Hit points follow 10-classes.md §1.4: 10 plus the Constitution modifier at level 1, and 7 plus it
for every level after, with the class's HpModifier on top (+10% for the Swordhand, -10% for the
Arcanist), and whatever anything worn adds. Everyone levels up as Difficulty and levels says.

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
- **Talking to them.** One of the party with something to say has a conversation in
  `DialogueCatalog` that's with their origin id (`soulbound`, `sera`, `mira`, `kael`, `brannock`)
  and can start. Then E talks to them when they're the nearest and there's nobody and nothing else
  in reach (they're always at your elbow, so they'd take E off the chest you're standing at
  otherwise), and clicking them talks instead of taking control: the cursor's the speech bubble and
  the tooltip ends TALK. Someone with something new, a conversation marked `MarkerKind.Quest`, has
  the gold "!" over their head as they walk. Never whoever you're controlling.
- **The camp.** Anyone recruited with four of you already goes to wait at the Gilded Flagon, and a
  toast says so. They stand at the Flagon's `camp` places, dressed as themselves, the first to wait
  at the first, and you can talk to them. Their own conversation plays if they have one that can
  start; if not, they offer to take the place of a companion with you (never you or the
  Soul-Bound), or to come along if there's room, or to wait. A swap is an effect like any other a
  conversation has, so it's in the save the moment it's picked: the two of them change places in
  the party and the camp, the one coming in stands where the one going stood, and the strip
  changes with them.
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
- **People.** `npc` in a map file puts someone there: a name, what they're drawn from (one of the
  townsfolk's sheets, one of the story's cast, or a number the paperdoll dresses someone at random
  from, which is how the crowds are made), which way they face, and their lines. They stand, look
  round every few seconds (never at the wall), and turn to face you when you talk to them; a line
  goes up over their head on a dark smudge, the next one each time you talk to them. Conversations
  take over from the lines when they have one. Some stroll: every so often one with `wander` set
  walks off to somewhere within that many tiles of where they were put, rests, looks round, and
  goes somewhere else. They give up on a stroll if anything's in the way, and stop and face you
  when you talk to them. `talk=` gives someone a conversation of their own by id, which wins over
  any that's with them while it can start.
- **Creatures.** `npc ... creature=gloomfang` puts down one of `Creatures`: the Gloomfang wolves, the
  Thornmother, the Hollowed (footman, hexer and sapper) and the vault's constructs. A creature is
  someone on the map like anyone else, so it looks round, faces you, can wander, can be clicked and
  talked to, and its tooltip is its name; it's only bigger. Each has its own frame size, where its
  feet are, the box you point at, its shadow and the floor it blocks, and it can be talked to from
  as much further off as it's broader than a person.
- **Chatter.** When you're within five tiles of someone with lines, now and then they say one
  without being asked, each on their own clock of nine to eighteen seconds, and never more than two
  at once, so a market sounds busy without the screen filling up. Bubbles that would overlap move up
  out of each other's way, and none of it shows while a conversation's up.
- **Markers.** Someone whose conversation is marked as work for you has a gold "!" bobbing over
  their head, someone with an event has a red one, and so does a trigger marked `mark=yes` while
  it's waiting for you. They draw over the roofs and the dark, they show on the map screen, and one
  that's off the screen gets an arrow at its edge pointing the way. Who's marked is only worked out
  again when a flag changes, the party does, or you take control of someone else.

## Items

Forty-nine items in `ItemCatalog`, in the order their icons are drawn: the SRD's weapons and armour
with its damage, ranges, properties, AC, Dexterity caps, Strength and prices, shields, helms,
gloves, boots, cloaks, an amulet and a ring, potions, alchemist's fire, an elixir that gives back a
spell slot, and the pelts and black crystal shards foes drop for selling. The few magic ones are the
team's: the ember dagger, the thornwood bow, the sunsteel blade, the Warden's shield, and the circlet,
gauntlets, boots, cloak, amulet and ring that each add one thing. Every row is checked against its
kind when the game starts, so a sword with no damage or a potion with a paperdoll layer stops it there.

| File | What it is |
| --- | --- |
| `ItemDefinition` | One item: name, kind, description, value, rarity, icon, the paperdoll layer it draws, and whichever of the weapon, armour, worn and consumable settings its kind uses. `Slot`, `Fits` and `SlotOn` say where it goes |
| `ItemCatalog` | Every item, by id |
| `Inventory` | The party's bag, over the save's list of `ItemStack`s: potions and materials stack, anything worn has a place each |
| `ShopCatalog` | The shops: the Trader's stall in the Lantern Market, Halden's Forge, the apothecary and the Chandler's, each with its stock and markup. Selling gets half an item's value anywhere |
| `LootTableCatalog` | What each foe drops: some gold, anything it always has, and weighted draws. Wolves leave pelts, the Hollowed a few coins and the odd shard, the Thornmother the thornwood bow, the Old Crown Vault's fight its gold and the sunsteel blade |

Chests hold items too (`items=` in the map files), which go in the bag with a toast naming them.

## Items and inventory

**I** opens the inventory over the map, **C** opens it on the character sheet, and so does
INVENTORY on the pause menu. Down the left are the party's faces, then whoever's waiting at camp
under AT CAMP. Whoever's picked stands big in the middle, turning on the spot in exactly what
they're wearing (baked off the paperdoll with their gear), with their nine slots round them: head,
cloak, body and main hand down the left, the side their sword hand's on when they face you; amulet,
ring, hands and off hand down the right; and feet under them. An empty slot shows a faint outline
of what goes in it. Their HP, AC and speed are on the tiles under them.

On the right is the party's shared bag, shown in the catalogue's order (weapons, then armour and
what's worn with it, then potions, then what's for selling) whatever order it was filled in, with
how many of anything that stacks in the corner, then the party's gold and the key items the story's
handed over. A click on something in the bag (or Enter) puts it on whoever's picked, in the slot it
goes in, and whatever was there goes back in the bag: a shield takes a two-handed weapon off with
it, and a second light weapon goes in the off hand. The slot it'll go in lights up gold while it's
pointed at. They swing a new weapon once and a glow goes up behind them, so you see the change, and
the party on the map behind the veil is already wearing it. A click on a slot takes off what's in
it, and on an empty one finds something in the bag for it. A healing potion is drunk the same way,
by whoever's picked, and an elixir of focus gives back a spent spell slot. Anything that can't be
used says why along the bottom of the screen: a pelt's for selling, alchemist's fire is for a fight,
and nobody drinks a potion they don't need. Whoever's at camp can be looked at but not changed;
their kit's at the Flagon with them.

Pointing at anything, or the keys landing on it, brings up its card: its name in its rarity's
colour (white, green for uncommon, blue for rare, like the rim round its slot), what it is and
where it's worn, its numbers the way the SRD writes them, what it says about itself, what putting
it on would change against what's worn there now (AC +1 in green, average damage -1.5 in red), a
warning if they aren't trained with it or strong enough for it, and what it's worth. The
comparison is worked out by dressing a copy of them in it and asking `CombatStats`, so a shield
that costs a longsword its two-handed die says so, and the card can't disagree with a fight.

The character sheet (**Tab**, or **C**) has their level, class and experience, their people and
what their class is for, the six scores with their modifiers and the save under each (gold for
their class's own), their proficiency bonus, how they attack with each hand and with spells, and
their class's kit out of `SkillCatalog`, each with its icon from the fight's hotbar and what it
costs in a fight, and the ones their rank hasn't opened yet dimmed with the rank that does. Over the
kit go their rank in the class, its class points, and how many more the next rank wants. Picking one
reads out what it does under the list.

Shops open from their keepers' conversations, once the talking's done (`DialogueEffect.Shop`): the
Trader at the blue stall in the Lantern Market, the smith at Halden's Forge (inside, or by its
door), the Chandler down by the south canal, and the spice seller at the teal stall, who keeps her
sister's apothecary shelf. Each has something to say the first time and something short after.
The shelf is on the left at the shop's prices and the bag on the right at half what things are
worth. A click buys or sells one, and Shift sells the whole stack. Whatever's sold stays on the end
of the shelf at what it fetched until you leave, so a slip of the finger can be bought back, and
what the party can't afford is greyed with its price in red. Only the bag can be sold from: what
someone's wearing comes off in the inventory first.

| File | What it is |
| --- | --- |
| `Items/Wardrobe` | Putting things on out of the bag, taking them off into it, and drinking from it, and why something can't be used |
| `Items/Trade` | Buying and selling across a counter, and why a deal can't be done |
| `Items/GearComparison` | What putting something on would change (each a `GearChange`), and what's worth knowing first |
| `Items/ItemReadout` | An item in words: its kind and slot, and its numbers a line each |
| `Items/BagOrder` | The order the bag's shown in |
| `Components/ItemIcons`, `ItemGrid`, `ItemTooltip` | The icons and the slots they sit in, a scrolling grid of them, and the card |
| `Components/PortraitRail`, `StatTiles`, `CharacterSheet` | The faces down the left, the tiles with a number on, and the sheet |
| `Dialogue/Scripts/Shops` | The four shopkeepers' conversations |

Anything out of a chest goes in the bag with a toast that says so, and the first time anything
does, a tip says where it's gone; the next time, if the bag hasn't been opened by then, another
says I and C.

## Combat

Fights are in real time, the way `Story/11-combat.md` lays it out: you trade blows where you
stand, your auto attack swings on its own timer, and four class skills and a potion sit on the bar.
`Combat/Realtime/` is the engine (below), and the fight screen plays it in the room you're in (On
screen). There is one combat system: the turn-based engine and its screen are gone.

### The real-time engine

`Combat/Realtime/Battle` is the fight, and draws nothing. It takes the encounter, who's in it
(`CombatantSetup`s, which `BattleRoster` builds from the party, the guests and the starting foes), a
`BattleRules` (the difficulty, the story's flags, the leader the player drives, the bag) and a seed,
and an optional `BattleArena` (the room's floor band, where each side starts, where waves walk in;
`BattleArena.Default` is a 400-pixel room like the game's). Everyone has a position in world pixels
on the floor; there are no rows. Time is whole milliseconds, moved on by `Step()` one 20 ms tick at a
time, and every roll comes from the fight's own `Pcg32`, never `System.Random`, so the game (which
calls `Step` from `Update` with an accumulator) and the simulator play the same fight from the same
seed and the same commands. `Command(BattleCommand)` queues an order (Target, MoveTo, Use a skill on
someone, Potion, SoulLink, AutoAttack on or off, Help, Stop, Concede) for the next tick; `Step()`
hands back that tick's `BattleEvent`s: a clip starting (with when its blows land), a telegraph and
its ground marker, a cast, a projectile, hits, misses, dodges, blocks, damage, healing, auras,
movement, downs, death saves, joins, supports acting, phases, barks, the Soul-Link, and the fight
won or lost. Damage lands on the tick of its clip's `hit` (or arrives after its `release` and
flight), and its events carry that tick, so the number shows on the frame the blow lands.

- **Timers.** The auto attack swings every 2.0 s (light and finesse weapons 1.8, heavy and two-handed
  2.4, crossbows 2.6; foes 2.5, quick beasts 2.0, big ones 3.0), and only lands if you're in reach
  when it comes round. An action skill puts every action skill on a 1.5 s GCD; a bonus skill is off
  it, with a 0.5 s lockout of its own; a reaction is armed for 4 s and fires on its trigger
  (Riposte, Shield, Hellish Rebuke). Each skill has its own cooldown in seconds: the class tables' CD
  1 to 4 are 3, 6, 9 and 12 s, a turn each, but a heal comes round no faster than every 6 s and a few
  skills are tuned off the rule ("Tuning" below; `10-classes.md` §8 has every one). Haste (3% a point of
  Dexterity, or the casting ability, plus what you carry, to 50%) shortens swings, cooldowns, casts
  and the GCD, to 1.0 s at the least. A cast roots you, and moving, a Stun, a knockdown or failing
  the Con save a hit forces stops it.
- **The sheet underneath.** Every attack is still d20 + to-hit against AC with advantage and
  disadvantage; a miss that would have hit without the target's Dexterity is a dodge, one that would
  have hit without their shield a block. Crit chance is 5% (the 20) plus 2% a point of the attack's
  ability, to 40%, and doubles the dice. Saves, DCs, resistance, vulnerability and temporary hit
  points (a shield aura hits come off first) are 5e's; ⁺ dice go up at levels 5 and 9.
- **Auras.** Every condition, buff, debuff, and damage or healing over time is an aura with a
  duration, stacks, a source and, if it ticks, a tick every 3 s; the same aura from the same source
  refreshes; cleansing takes the named conditions off; concentration breaks on a failed Con save,
  which being hurt forces at most once a turn (3 s), as often as a round at the table would.
- **Focus.** 0 to 100, 1 a second for everyone and each class's model on top (`FocusModels`:
  Momentum, Opportunist, Bulwark, Wellspring, Devotion, Wild, Pact, Tempo, Blood); a fight starts on
  the difficulty's (Story 60, Balanced 40, Tactician 30; Tempo 80).
- **Kits.** `SkillCatalog` has every class's seven skills by id (`<class code>-<kebab name>`) as data
  (`SkillDefinition`: kind, Focus, cooldown, cast or telegraph, reach, target, §4.3's area shape, and
  `Effect`s in order); the eight base classes' are in `SkillCatalog.Base.cs`, the sixteen advanced
  classes' and the Hourwarden's in `SkillCatalog.ActOne.cs` and `SkillCatalog.ActTwo.cs` (gathered,
  with what they share, in `SkillCatalog.Advanced.cs`), the foes' moves in `SkillCatalog.Foes.cs`,
  and the guide at the top of `SkillCatalog.cs` says how to add a class. Every class has its own kit
  now. The
  advanced kits brought their own kinds of effect: summons that fight on your side for the rest of
  the fight (the Watchman, a knight of the Order), Broadside's guns outdoors and Grapeshot indoors
  (`BattleRules.Outdoors`), gold taken as data for the result panel (`Battle.Stolen`,
  `Battle.GoldBonusPct`), decoys (Mirror Image, Rooftops), redirected attacks (Cover, Refract,
  Nobody Left Behind), Sanctuary's save to pick a target, Reveal's dispel, Rewind's hit points from a
  few seconds back (`Fighter.HpHistory`), Stop's Frozen, a random draught (Rummage), a thrown flask
  from the bag (Throw Anything), a free Strike from every ally, Echo Strike's repeat, and passives
  that keep someone up, stop a foe's big wind-up or shake off a foe's control. `10-classes.md` §8
  says how each table line reads in real time. A class's rank (1 to 10, from its
  class points, kept per person per class in the save) opens its slots: skill 2 at 2, skill 3 at 3,
  the passives at 4 and 10, the ultimate at 5 (`ClassRanks`).
- **Clips.** Each move plays a clip (`auto-<kind>-n` for a weapon auto, `skill-<id>` for a class
  skill, `foe-<enemy>-<move>` for a rigged foe's move), and `MotionTimings` reads when its blows land
  from `Content/Characters/Parts/clips.json`'s `hit` and `release` events, with fallbacks (an auto
  0.30 s, a melee skill 0.45 s, a cast 0.40 s, an ultimate 0.70 s) where a clip's missing;
  `--validate` warns about each missing one.
- **Foes.** Each stat block has an auto attack (a Multiattack is a combo on the one swing) and its
  telegraphed moves (Trip, Hook Pull, Thorn Burst, Uprooting Charge, Blasting Charge, Ashen Bolt's
  cast, Hex, Feint, Smoke Bomb, Cunning Disengage, Overhead Chop), each with a cooldown and a wind-up
  everyone can see, an area's ground marker filling until it lands. A threat table picks its target
  (damage a point, healing half a point shared out, double for the defenders' classes, 60% for the
  Shadowstep and 85% for the Lanerunner, a taunt on top; it switches at 110%, 130% for a ranged foe).
  In real time a foe's blow lands surer and lighter than on its sheet (`HitShift`, `DamageScale`, and
  more hit points, `HpScale`: "Tuning" below). A boss changes at shares of its health
  (`FoePhase`): the Thornmother calls whelps and brings in Thorn Burst at two thirds, then enrages
  with the Uprooting Charge at a third; Kael brings out his smoke at 60%.
- **The party.** You drive the leader; everyone else, guests and summons too, is the party AI
  (`Battle.Brains`): every 0.3 s a free fighter scores every skill on every target worth thinking
  about, healers heal first, defenders taunt what's hitting someone else, everyone assists the
  leader's target, ranged fighters step away from melee, a hurt fighter backs off behind someone
  sturdier, and anyone steps out of a ground marker. A defence is worth more the more foes are on
  whoever it guards (and half again under half health), so Smoke Step or a Shield goes up when it's
  needed and not before. Like a player, it saves its Focus for its
  ultimate while that's off cooldown and would be worth pressing (it still heals, guards and taunts
  in the meantime), and it picks pockets when there's a pocket left to pick. Down at 0 is Downed, with death saves every 3 s;
  healing or Help (a 2 s channel beside them) gets them up. The Soul-Link charges when you and the
  Soul-Bound hit the same foe within 3 s.
- **The encounter.** Supports act every few seconds from their start (`EncounterSupport.FromSeconds`,
  `EverySeconds`), waves walk in from the room's edge at their second (`EncounterWave.AtSeconds`) or
  sooner if the field's clear, guests join at theirs, and a fight you only have to last ends at
  `EndsAfterSeconds`. The old rounds are kept beside them in the data.
- **For the HUD.** `Battle.Bar(fighter)` (the seven slots with cooldowns, the GCD, costs, reach and
  why not), `WhyNot`, `InReach`, `Threat(foe)`, `Phase(boss)`, `Supports`, `SurviveShare`,
  `SoulLinkCharge`; every `Fighter` has its position, facing, hit points, temporary hit points,
  Focus, auras, cast, swing, target, weapon kind and current clip. `ApplyToParty()` writes the
  party's hit points back when it's over.

### Difficulty and levels

`Settings.Difficulty` picks one of `11-combat.md` §6.1's modes, changeable any time outside a fight;
`DifficultyRules` holds what each changes, for foes only, never a guest:

| | Story | Balanced (default) | Tactician |
| --- | --- | --- | --- |
| Foe hit points | -35% | as tuned | +25% |
| Foe to-hit and save DCs | -2 | as tuned | +1 |
| Your side's Focus at the start | 60 | 40 | 30 |
| After a story fight | Full heal | Heal half of what's missing | Nothing |
| Losing | Retry, and "Win this fight" after two losses | Retry, or retry on Story for this fight | Retry |

Levels come from experience: `LevelTable.Expected` is the level the story has the party at for a
fight (§6.3: level 1 in the Prologue and A1-C1, 2 in A1-C2 and A1-C3, 3 in A1-C4), a story fight
gives about a third of a level at that level, less above it and more below, and `Leveling` brings
the hit points with each.

### The simulator

```
dotnet run -- --simulate a1-c1-01-gulls 2000      # fight it 2000 times headless, AI on both sides
dotnet run -- --simulate all 200                  # every encounter, 200 times each, on Balanced
dotnet run -- --simulate all 200 story            # the same on Story (or tactician)
dotnet run -- --simulate classes 200              # every class in your place in every fight
dotnet run -- --simulate classes:rogue,wizard 200 # only those classes
dotnet run -- --simulate numbers                  # every class's real-time numbers, as Markdown
dotnet run -- --simulate selftest                 # the engine's own checks
dotnet run -- --simulate trace:a1-c3-02-raid 3    # run 3 of a fight, its whole log
dotnet run -- --simulate trace:a1-c4-02-sappers@hourwarden   # the same with a class in your place
dotnet run -- --simulate a1-c1-01-gulls@rogue 400 # one fight with a class in your place, a line a member
dotnet run -- --simulate skill:glasswright-refract # the self-test's scripted fight for one skill, its whole log
```

`--simulate` opens no window and drives the real-time engine (`Combat/Realtime/BattleSimulator`).
Each run lines up the party the story would have there (you as a Swordhand, the Soul-Bound as a
Shadowstep, and the companions who'd be with you) at `LevelTable.Expected` and the class rank the
story would have them at (`ClassRanks.ExpectedRank`), fresh, with two healing potions, through
`BattleRoster` as the game does, with the fight's guests, supports and waves, on the difficulty asked
for, and plays it on the fixed 20 ms step with the AI driving everyone, you included, with the human
handicap (`BattleRules.HumanHandicap`: every party decision 0.4 s late, and its second-best choice
15% of the time, drawn from the fight's own dice). Run r of a fight is seeded from the base seed, the
fight's id and r (`Pcg32.Mix`), so it prints the same whether it's fought alone or with the rest, and
`trace:` replays any one run line by line. An ally who only comes on a story flag is fought both
ways: the verdict is on the fight without them. A fight still going at 600 s is unfinished.

Each fight prints how it went (win rate, mean seconds to win, unfinished, who went down most), a line
a member (how often they went down, damage and healing a second, the share of the fight a foe was on
them, the share of the standing foes' time they held one under control, hit points left) and a line
of pace; then the report, one line a fight, with its verdict against §9.2's targets, and exits 1 if
anything fails:

| Kind | Won, on Balanced | Length | Most downed may be |
| --- | --- | --- | --- |
| The tutorial | 99%+ | 20 to 50 s | 40% |
| Random fights | 97%+ | 15 to 45 s | 40% |
| Story fights | 92 to 97% | 25 to 75 s | 40% |
| Mini-bosses | 88 to 94% | 45 to 120 s | 70% |
| Bosses | 82 to 90% | 75 to 180 s | 70% |
| The Sovereign | 78 to 88% | 120 to 240 s | 70% |

On Story every fight has to be won 99% of the time; Tactician's verdicts are information. Under the
report comes the pace (`BattleSimulator.Pace`, information, never a failure), for how a fight feels
against 11-combat.md's AQW-style rhythm: your auto's heartbeat (about every 2 s); how often your bar
lets you press a skill (each skill but the auto and the ultimate as often as its cooldown or the GCD
allows, or as the Focus you actually earn pays for, whichever is slower; 1.5 to 3 s is the aim) and
how often you did; the share of your time a skill was ready, and your mean Focus and how often it sat
full (full means cooldowns, not Focus, hold you back); how long an ordinary foe lasts from its first
wound and how long the whole party's fire takes to drop one (its hit points over the party's damage
a second: 8 to 15 s is the aim); and a boss's life from its first wound, its seconds in each phase,
and the share of its falls in which every phase lasted 8 s or more.

`classes` puts each class in your place (with its starting kit and recommended scores) in every
fight, a quarter of the runs each, and reports its win rate, length, damage and healing a second,
threat held, control held, how often its bar lets you press a skill and how often it did, its mean
Focus, and in how many fights it led everyone on its side at damage, healing by its skills (potions
are nobody's role), threat or control. The verdict asks that it's within 5 points of the median class
either way and leads its party at something in at least half the fights. Then come each class's win
rate fight by fight, and every skill the AI used in your place and any skill open at the ranks reached
that it never used. `classes:watchblade,rogue` runs only those classes (the median is then of those
alone), which is how the tuning ran it in eight processes at once. `numbers` prints every class's
real-time numbers from the catalog as Markdown (`10-classes.md` §8.1 is its output).

`selftest` checks that every fight replays exactly from its seed (the same seed gives the same hash
of every event's every field, another seed a different one), that every skill of every class with a
kit, its form's too, fires in a scripted fight at level 5 and rank 10 and does what its kind says
(damage, healing, an aura, an armed reaction, a transform...; a skill that was used but whose effect
the dice kept from landing, a rider on a hit missed three times running, gets two more fights on
fresh seeds), that the advanced kits' own kinds of effect do the right thing in fights set up for
them (`SelfTests.Mechanics`: Broadside outdoors and in, both summons fighting, Arrest's taking out,
Pick Pocket once a foe, Captain's Share, Throw Anything using up the flask, Lucky Stone's natural
20, Cover, Hold, Counterspell, Mirror Image's three images, Refract, Sanctuary, Reveal, Rewind,
Bleed the Clock and Echo Strike), that every foe's move fires and lands, and that no fight stalls on
any difficulty. `skill:` prints the scripted fight the every-skill check uses for one skill.

The tuned pass, `--simulate all 200` on Balanced (5 of 8 pass; on Story all 8 pass, every one won
every time):

| Encounter | Kind | Lv | Won | Seconds | Who goes down most | Verdict |
| --- | --- | --- | --- | --- | --- | --- |
| `p-05-wraiths` | Tutorial | 1 | 100% | 29 | you, 10% | pass |
| `a1-c1-01-gulls` | Story | 1 | 97% | 30 | you, 14% | pass |
| `a1-c2-01-gloomfangs` | Story | 2 | 97% | 34 | you, 21% | pass |
| `a1-c2-02-thornmother` | Boss | 2 | 87% (91% with the wardens) | 93 | the Soul-Bound, 54% | pass |
| `a1-c2-02-guard-wave` | Story | 2 | 97% | 44 | the Soul-Bound, 18% | pass |
| `a1-c3-02-raid` | Story | 2 | 100% | 59 | Sera, 23% | too easy |
| `a1-c4-02-kael` | MiniBoss | 3 | 100% | 48 | you, 42% | too easy |
| `a1-c4-02-sappers` | Story | 3 | 98% | 58 | you, 37% | too easy |

The raid, Kael and the sappers are still over their bands: each is won all but every time while whoever
takes the blows already goes down about as often as §9.2 allows, so more damage or hit points only
downs more people (the tuning tried both). They want something other than numbers: a support that
comes later, a wave more, or a foe that goes for the back line. Its pace, the same run:

| Encounter | Your auto | A skill to press | Pressed | Focus | A foe under the party's fire | The boss |
| --- | --- | --- | --- | --- | --- | --- |
| `p-05-wraiths` | 2.1 s | 3.0 s (rank 1) | 3.7 s | 69 | 9.6 s | |
| `a1-c1-01-gulls` | 2.1 s | 2.8 s | 3.1 s | 51 | 7.1 s | |
| `a1-c2-01-gloomfangs` | 2.1 s | 2.9 s | 3.1 s | 49 | 8.8 s | |
| `a1-c2-02-thornmother` | 2.3 s | 4.1 s | 4.0 s | 26 | (whelps) | 91 s: 26 + 30 + 35, every phase seen |
| `a1-c2-02-guard-wave` | 2.1 s | 2.6 s | 2.5 s | 32 | 9.4 s | |
| `a1-c3-02-raid` | 2.2 s | 2.6 s | 2.7 s | 53 | 8.4 s | |
| `a1-c4-02-kael` | 2.3 s | 3.7 s | 3.5 s | 22 |  | 47 s: 26 + 21 (he yields) |
| `a1-c4-02-sappers` | 2.4 s | 3.0 s | 3.0 s | 28 | 14.5 s | |

Against a single big foe the Swordhand's Focus runs low (Cleave catches only one), so the bar's
4 s there is Focus, not cooldowns: what to spend it on is the player's choice.

`--simulate classes 200` (50 runs a fight, each class in your place at the story's levels, ranks 1
to 5). The median class wins 89% of the Act 1 fights; the Swordhand, the story's own pick, 96%:

| Class | Won | Dmg/s | Heal/s | Threat | Control | A skill every | Leads its party (of 8 fights) |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Swordhand | 96% | 4.6 | 0.4 | 81% | 0% | 3.2 s | healing 3, threat 8 |
| Shadowstep | 81% | 5.9 | 0.1 | 60% | 0% | 2.8 s | damage 5, threat 2 |
| Lightbearer | 94% | 1.9 | 0.6 | 54% | 0% | 6.3 s | healing 4, threat 1 |
| Arcanist | 82% | 5.3 | 0.2 | 76% | 0% | 2.6 s | damage 1, threat 7 |
| Warden | 86% | 3.6 | 0.3 | 83% | 0% | 3.7 s | healing 3, threat 8 |
| Wildcaller | 96% | 2.7 | 0.5 | 64% | 8% | 5.6 s | healing 4, threat 2, control 7 |
| Hexblade | 84% | 5.0 | 0.1 | 74% | 1% | 3.7 s | threat 6, control 1 |
| Skald | 89% | 1.8 | 0.6 | 56% | 0% | 4.9 s | healing 4 |
| Watchblade | 88% | 2.3 | 0.2 | 82% | 16% | 8.3 s | threat 8, control 8 |
| Netcaster | 97% | 2.0 | 0.6 | 53% | 19% | 5.4 s | healing 4, control 8 |
| Deckhand | 95% | 2.9 | 0.5 | 58% | 5% | 4.9 s | healing 3, threat 2, control 8 |
| Lodge Ranger | 80% | 5.4 | 0.2 | 74% | 0% | 2.6 s | damage 4, threat 5 |
| Guild Battlemage | 98% | 3.7 | 0.0 | 67% | 0% | 6.5 s | threat 5 |
| Ashwalker | 92% | 5.0 | 0.1 | 76% | 9% | 4.7 s | damage 2, threat 7, control 8 |
| Ironvein Bulwark | 84% | 2.4 | 0.1 | 79% | 0% | 4.6 s | threat 7 |
| Ember Mender | 98% | 2.2 | 0.8 | 58% | 0% | 4.9 s | healing 7, threat 1 |
| Lanerunner | 84% | 5.4 | 0.1 | 76% | 0% | 4.0 s | damage 3, threat 6 |
| Wayfarer | 84% | 4.6 | 0.2 | 68% | 0% | 2.9 s | healing 1, threat 3 (nothing in half) |
| Gilded Knight | 88% | 4.6 | 0.1 | 67% | 3% | 4.4 s | damage 1, threat 3, control 4 |
| Hemomancer | 96% | 3.2 | 1.1 | 72% | 0% | 4.3 s | healing 8, threat 3 |
| Hivekeeper | 94% | 2.3 | 0.8 | 60% | 0% | 5.6 s | healing 6, threat 2 |
| Glasswright | 86% | 3.7 | 0.1 | 78% | 7% | 5.3 s | threat 8, control 5 |
| Lamplighter | 94% | 2.1 | 0.7 | 54% | 0% | 4.4 s | healing 5 |
| Crownsworn | 86% | 2.9 | 0.5 | 80% | 0% | 4.6 s | healing 2, threat 6 |
| Hourwarden | 93% | 2.6 | 0.0 | 46% | 5% | 5.7 s | healing 4, threat 1, control 5 |

Eleven classes are within 5 points of the median; the rest are 6 to 9 points off, so §9.2's "within 5
points" isn't met yet. Almost all of the gap comes from the fights you and the Soul-Bound fight
alone at rank 2 (the gulls, the gloomfangs and the guard wave) and from the Thornmother: there the
Swordhand's plate, Cleave across the pack and Second Wind carry the duo, while a light class without
an area skill or a way to keep a foe off yet wins 26 to 80% of them (the Shadowstep 16% against the
Thornmother). The Skald and the Lamplighter win the tutorial only 76% and 80% of the time. Every
class wins the four-member fights. The fix is the light classes' rank 1 and 2 kits for those fights,
not the fights' numbers, and it's still to do. The AI used every skill open at rank 5 except the
Skald's Inspiration.

The turn-based engine and its simulator are gone; `--simulate` is the real-time one.

### Tuning

How to tune one that's off, in this order (§9.2): the supports (their dice, their start and how
often); foe hit points, for how long a fight lasts; foe to-hit, for how often anyone goes down;
telegraph lengths, for how fair the big hits are; damage; and waves and time limits last. Tune on 600
runs or more, since 200 is about three points either way, and confirm on 200. The tuning ran with the
anim stream's `clips.json` copied into the build's output, so blows landed when the real clips say.

In real time a party lands several blows for every one it landed in a turn, and a foe swings two or
three times a round, while the party's hit points stay what the sheet says. So every stat block has
three real-time levers, read by nothing turn-based:
- `EnemyDefinition.HpScale`: its hit points as a multiple of its hit dice's average (4.5 by default).
- `DamageScale`: the share of what its blows roll that they do (0.23 by default).
- `HitShift`: what it adds to its attack rolls (+10 by default).

A surer, lighter blow is the point of the last two. Against a stream of swings, armour decides almost
everything: at +4 a wolf hits AC 13 1.7 times as often as the Swordhand's AC 19, but at +14 only 1.2
times as often. So a fight a plate-wearer finds fair is no longer a rout for someone in leather. Blows
that light mostly do 1 (the least a hit does), so at these levels hit points and to-hit are the levers
that move a fight; the damage scale only starts to matter above about a third.

| Foe | Hit points × | Damage × | To-hit + |
| --- | --- | --- | --- |
| Sea-wraith | 9 | 0.23 | 10 |
| Red Gull thug | 7.5 | 0.24 | 8 |
| Bosun Grell | 8 | 0.25 | 8 |
| Gloomfang wolf | 7 | 0.25 | 10 |
| Gloomfang whelp | 3 | 0.18 | 10 |
| The Thornmother | 10.5 | 0.22 | 10 |
| Hollowed footman | 7 | 0.36 | 1 |
| Hollowed hexer | 7 | 0.48 | -2 |
| Hollowed sapper | 9 | 0.8 | 3 |
| Sapper chief | 9 | 0.8 | 2 |
| Kael | 9 | 0.45 | 8 |

This second pass started from engine-core's first:
- 2 of 8 fights were in band on Balanced; the rest were won 98 to 100% of the time.
- The Soul-Bound went down in half the fights.
- You pressed a skill every 4 to 8 s, though one was ready only 10 to 70% of the time.
- A foe lasted 14 to 27 s from its first wound to its fall.
- The classes spread from 68 to 100%, and the damage classes dealt less a second than the front line.

What moved, and why:

- **The conversion.** The tables' CD 1 to 4 are now 3, 6, 9 and 12 s, a turn each (`SkillCatalog.Cd1`
  to `Cd4`), not 6, 8, 12 and 16. AQW's fillers come round every 2 to 6 s and its big skills every 10
  to 20, and at the old numbers a full bar offered a press only every 4 to 6 s.
  - Heals stayed at 6 s (Cure Wounds, both Healing Words, Mend, Cauterise, Honey Salve) and Second
    Wind at 16; otherwise the healers in your place won everything.
  - The Swordhand's Cleave became a 3 s filler (CD 1), so the story's own class has a skill to
    press every 2.5 to 3 s from rank 1.
- **Concentration** is saved against at most once a turn. Hit two or three times as often as at the
  table, Hex, Hunter's Mark and Moonbeam lasted seconds, and the AI paid for them again and again.
- **The party AI** values a defence by how many foes are on whoever it guards, and half again under
  half health. Smoke Step had been worth less than its price, so it was never used; Action Surge's
  worth went up to 14 for the same reason.
- **Threat.** The Shadowstep makes 60% and the Lanerunner 85%. At the front with Sneak Attack, the
  Soul-Bound took every boss's attention and went down in 76 to 90% of the Thornmother's and Kael's
  fights; now the Swordhand holds them.
- **Focus.** Opportunist pays +8 a hit (from +5), and Pact +2 a second and +8 a cursed hit (from +1
  and +6). The Shadowstep, Lodge Ranger, Lanerunner and Hexblade sat at 16 to 25 Focus with skills
  ready and nothing to pay for them.
- **The glass cannons** hit harder, so they kill before they're worn down:
  - Shadowstep: Sneak Attack 2d6⁺ (from 1d6⁺, as 5e has it by third level); Backstab strikes with
    its weapon dice twice for 15 Focus; Smoke Step's disadvantage lasts 6 s for 15 Focus.
  - Arcanist: Magic Missile's darts 1d4 + 2 for 20 Focus, Burning Hands 20; its hit points are no
    longer 10% under.
  - Hexblade: Hex adds 1d8 for 15 Focus; Hellish Rebuke costs 20.
  - Lodge Ranger: Hunter's Mark adds 2d4 for 10 Focus; Hawk Strike is free, every 3 s, 2d6⁺.
  - Wayfarer: the empty-handed throw is 2d10; Duchess Kicks costs 20 Focus every 6 s; +10% hit points.
  - Others: Second Hand 1d10⁺, Shield Bash 1d10⁺, Mirror Image 20 Focus every 9 s; Rooftops costs 10,
    Command 15, Sunburst 20, Vespers 20.
- **The sturdy and the self-healing** came down a little:
  - Guild Battlemage: Ward is 1d6 + Int + level for 30 Focus every 12 s; Force Bolt is 2d6⁺ instead,
    so it leads at damage rather than at not dying.
  - Hemomancer: Blood Lash heals 1, Drain costs 30 Focus, Blood Shield is 2d6 + level every 12 s.
  - Cauterise is 2d4⁺ for 30 Focus; Honey Salve and Cure Wounds cost 30.
- **The fights.** The table above, from 4.5 to 10.5 times their hit dice and blows at about a quarter
  of what they roll.
  - The Thornmother is a Boss now, as 11-combat.md §6.1 calls her ("the first"). Her class points
    were already a boss's, and she lasts 85 to 95 s through all three phases.
  - The sappers' Blasting Charge fills for 2 s (from 1.5), so the back row can step out of it.

What's left: the raid, the sappers and Kael are won every time.
- With four or five of you, two healers and Help, a wipe is rare however hard the foes hit.
- Pushing harder only puts whoever heals on the floor: healing's threat (§5.1) draws the melee foes to
  Sera and Mira.
- Kael's duel is nonlethal, so he'd have to down all four at once.

Either a taunt in the early party or a lighter healing threat would change that; both are design
decisions. The class spread comes from the duo fights (above).

The turn-based retune, for the turn-based engine before it was replaced (kept here for its reasons, which the real-time numbers inherited), started from every story fight won 100% of the time
and the Soul-Bound, the AC 13 Shadowstep, going down in up to 99% of the Thornmother's. Three things
were silly first, and are `EnemyMind`'s: nobody ever left the front row, so whoever had the least
armour took every swing; a hurt rogue stepped back past a foe and then drank its potion, so the swing
dropped it first; and the wraiths dodged rather than hit, since every foe in front counted as one
beside you. With those fixed, the numbers that moved:

- **Supports:** Tomas's net and Sera's arrows from the trees from round 3, not 2, and the Watch from
  round 4, not 3.
- **Kits:** Backstab has no cooldown, so the rogue can fight from the back row every turn it has the
  Focus; Cunning Step's cooldown is 2, not 3.
- **The Gulls:** thugs 2d6+2 hit points (from 2d4) and +2 (from +1); Grell +5 (from +3).
- **The wolves:** 3d6+3 hit points (from 2d4+1), and a bite at +4 for 1d6 (from +3 for 1d4).
- **The Thornmother:** AC 15 (from 14) and 12d10+24 hit points (from 10d10+20), with her bite and
  claws at +5 (from +7): she lasts, and knocks you flat, and hits less surely.
- **The Hollowed:** footmen 4d8+4 hit points (from 2d8+2) with a 1d10+1 blade (from 1d8+1); hexers
  3d8 (from 2d8).
- **The sappers:** 4d8+4 hit points (from 2d8+2), still +3 with a pick; the chief 7d8+14 (from
  5d8+10); Blasting Charge 2d6+2 (from 2d6).
- **Kael:** 16d8+16 hit points (from 11d8+11), +8 (from +5), a 2d8+3 rapier and 1d6+3 off-hand and
  feint (from 1d8+3 and 1d4+3): one duellist against four only ever reaches whoever holds the front.

What the story asks of a fight comes before any of that. The wraiths at sea only chill you, so the
Soul-Link fills against them and nobody goes down while they learn. The Red Gulls are a brawl, so
nobody dies on the docks and Grell's alive to beg. The Thornmother frightens by lasting (her hit
points and a thorn-hard hide) and by knocking you flat (Trip), not by hitting harder. The guard wave
comes in three, the pack, the courier's dead escort, then a straggler, while Sera sits it out at
the crystal. The sappers are diggers, a point worse with a pick than a soldier would be.

### On screen

A fight happens in real time in the room you're in, side-on, where everyone stands
(`Story/11-combat.md` §2). A conversation that ends with `DialogueEffect.Fight` starts it when it's
over, the way a shop or a journey waits for the talking to end, and the game's autosaved first, so a
lost fight can be fought again. Nobody walks to a slot: `ExploreScreen.Combat` hands `BattleRoster`'s
setups to the engine with each fighter's feet where they're standing (the party, the room's people who
turn, the guests who are here), and `World/Rooms/BattleFloor` builds the `BattleArena` from the room's
floor band across its fight range: the widest rectangle of floor that stays on the band all the way
along, the encounter's places for foes who turn up (ahead of the party, round the foes who are
there), the far end of the range on the foes' side for waves to walk in from, and the back edge
behind the party for the supports. `--validate` checks every room a fight is fought in has floor
enough for one. The Battle is made with the difficulty, the story's flags, you as the leader (whoever
of the party's first if you're sitting it out), the bag, the room's dark (or the eclipse's) and sky,
and a seed (the proof run's own). While it's on, the room's ways out, doors, triggers, scenes and
banter wait; anyone standing about who isn't in it steps back to the far edge to watch a fight to
yield, or leaves by the nearest edge for one to the death, and comes back after. The battle's music
comes in with its stinger (`AudioManager.BeginBattle`) and the fight's name goes across the screen.

**The loop** (`ExploreScreen.CombatLoop`). The screen keeps the fight's shown time and steps the
engine in its 20 ms ticks until it's a tick ahead, so every figure is drawn between its last two ticks
and every `BattleEvent` is played when the shown time reaches the tick it happened on. Under the proof
run the shown time moves exactly a sixtieth of a second for each drawn frame (lockstep), so its
captured frames are evenly spaced. Every fighter's walker is put where the engine has them each frame,
turned the way it has them facing, walking while they move, in the ready stance (`InFight`) with the
engine's weapon kind; a crowd standing almost on one spot is drawn eased a few pixels apart across the
room so it doesn't heap up (only the drawing moves). The camera follows you. A blocking tip, the pause
menu and the panel at the end hold the fight.

**The cues** (`ExploreScreen.CombatCues`) are played on whoever they happen to, all at once, never in
a queue:

- **A move** (`ActionStarted`) plays its clip on the mover (`WalkerSprite.PlayMove`), the swing's
  whoosh timed to peak on its blow (`CombatSounds.SwingLeadMs`), or for a spell the school's cast
  sound and a glow in the hand until it leaves. The engine times every blow to its clip's impact, so
  the hit lands on that frame. A hit-stop holds a figure's clip for a few frames while the engine goes
  on; afterwards the clip plays a little faster until it's back in step, so a combo's next blow still
  lands on its frame.
- **A telegraph** holds the foe's wind-up with a glint and the warning sound, and puts the move's
  ground marker (`CombatEffects.Mark`: a ring, a line or a cone the size of its area) on the floor,
  filling until the blow and resolving on it; an interrupted one goes. A cast with a cast time plays
  the caster's channel and its glow.
- **What flies** (`ProjectileLaunched`): an arrow or bolt, a school's bolt, or a beam of lightning,
  from the hand to the target, timed to arrive on the engine's tick.
- **A blow landing** (`Damaged`): the hit-stop on both, the white flash, the squash and the knock
  (`WalkerSprite.TakeHit`), sparks where the weapon meets the body or the school's burst, the weapon's
  or the school's sound with the critical's over it, the number (white, gold and bigger for a
  critical, red on the party, small and tinted for a tick), the flinch unless they're mid-move, and a
  shake only for a heavy skill, a critical, a kill, a big blow on you or a boss's slam, never an auto
  attack. A miss, a dodge or a block is MISS, DODGE (a sidestep) or BLOCK (a clash and the block clip)
  with its sound; a save is RESIST. Healing is the green number, a glow and a green wash. An aura going
  on is a small glow on the figure; the HUD shows the aura itself.
- **Life and death.** Knocked down and up, Downed (the collapse, staying on the floor), death saves,
  back up, and a foe's death: its die clip, a held beat, then a fade in motes. One of the party fallen
  for the rest of the fight stays on the floor, faint. A guest who falls back or a foe who yields
  fades out.
- **The rest.** Someone joining fades in where the engine has them; a support swings or shoots from
  behind the party (its banner lit, its ring filling to its next turn, its arrow carrying the blow); a
  boss's phase puts its name across the screen with a flash, a burst, the boss sound and a slam; a
  bark is a bubble over whoever said it; the Soul-Link charging is gold round the two of you; the
  win is everyone's victory pose.

**The HUD** (`ExploreScreen.CombatHudView`, drawn by `Components/Realtime/CombatHud`, below) is built
from the engine every frame: your frame (the leader), the party's rows, your target's frame with its
cast bar and auras, the boss's wide frame while one's in the fight, the hotbar from `Battle.Bar(you)`
with its cooldown and GCD sweeps, reach, cost and why not, your cast and swing over it, the Soul-Link
sigil on 7, and a nameplate over everyone else with a foe's telegraph filling over its head and the
ring under your target. The card for whoever the mouse is on in the room is the screen's own
(`ExploreScreen.CombatCards`); the HUD draws its own for a slot, the sigil or an aura.

**Input** (`ExploreScreen.CombatInput`, §2.3 and §7.1). Everything you do is a `BattleCommand` for
the leader, so the engine owns where you are. A click on a foe targets it, which turns the auto attack
on and walks you into its reach; a click on one of your side targets them (and helps them up if
they're Downed); a click on the floor walks you there; WASD and the arrows walk you while they're
held. 1 to 6 or a click on a slot use the bar (1 the auto attack on and off, 2 to 5 a skill on your
target or its own choice, walking into reach first if it's out of reach, 6 the bag's best healing
potion), 7 or the sigil the Soul-Link Strike; Tab the next foe, nearest first; T your target's
target; Escape clears your target, and with none it's the pause menu; L the log; Alt everyone's
numbers. The HUD is hit-tested before the room. Something that can't be done says why over the bar.

**The end.** When the engine's won or lost and everything's been shown, the panel comes up after a
moment: the experience, the class points (`ClassRanks.AwardFight`) and any rank or level gained, the
gold (with what was lifted mid-fight and the party's share) and the loot into the bag;
`Battle.ApplyToParty` writes the hit points back, the jingle plays (`AudioManager.EndBattle`), and
dismissing it sets the fight's `Victory` flag, gets anyone down up on 1, heals what the difficulty
says after a story fight, and plays the `Then` (or `Lost`) conversation. A loss with no `Lost` script
is the game-over screen with its retries; WIN THIS FIGHT hands the fight to the party.

**The options** (`OptionsScreen`, §7.3): the screen's shake (full, reduced, off), hit flashes and
damage numbers, taken as the fight starts and again every frame, so changing them from the pause
menu mid-fight works at once.

**The lessons** (`ExploreScreen.CombatTutorial`, §7.2) come one at a time, each once a game, at its
moment: what a fight is (holding everything while it's up), then picking a foe while you've none,
the auto attack once you've one, your first skill once your first swing lands, the cooldowns and the
global cooldown just after the skill goes off, Focus and your class's way of filling it a moment
later (or as soon as a skill's too dear), the potion the first time you're hurt (or a while after),
a foe's first telegraph, the Soul-Link when it first fills, a friend down, the allies, walking and
the win. The tutorial fight at sea is where they nearly all come; any other fight gives the ones
not yet seen the first time their moment comes. Each lesson's card holds the fight
(`TutorialTip.HoldsFight`) and puts a gold pulse round what it's about (the foe, slot 1 and the swing
bar, slot 2, the skills' sweeps, the Focus bar, the potion, the foe winding up, the sigil), but the
keys and the mouse still work: doing what it says (clicking the foe, pressing 2, drinking) puts it
away and the fight goes on, and so does GOT IT. The Focus lesson's body has a slot, `{focus}`, that
the fight fills with the leader's model ("Yours fills as your blows land, and more on a critical.").

**The fields** (`Combat/EncounterCatalog.Field`, `ExploreScreen.Field`, §2.1) are foes who keep a
wild room once the story's past them: three gloomfangs in the Mirewood's deep wood once the scene with
the pack at its edge is over (`a1-c2-01`), two Hollowed footmen at Thornbridge's river once Chapter 3's
done (`a1-c3`; not straight after the raid, while Mira's still talking there), and the Old
Crown Vault's four constructs, each a fight of its own, asleep until they're woken. Their names are
red. Clicking one starts its fight where everyone stands, with it as your target, so your auto
attack walks you in; the wolves and the Hollowed come at you by themselves when you come within
120 world pixels (not in the first two seconds in the room or after a fight, so a room that's just
filled doesn't pull you straight back in). Each is a `Random` encounter worth a random fight's
experience and 15 class points, with its foes' own loot. A field that's beaten is back a minute of
play later, or at once if you leave the room and come back. Its foes are the room's npcs: the vault's
constructs are in the zone file already, and the others the table puts down (`FieldGround`), since
the zone files are written by `tools/rooms`; `--validate` checks every field against its room and its
floor. Losing one is the game-over screen with its retries, as any fight without a `Lost` script.

**The proof run** plays every story fight in real time: the party's AI drives the leader too
(`Battle.LeaderAutoplay`), at 1x on the lockstep clock. It takes `fight-<id>.png` at the first busy
moment from a few seconds in (several people mid-move and numbers in the air) and `fight-<id>-won.png`
on the panel, and for the Red Gulls and the Thornmother films three seconds of every second frame into
`rt/`. The tutorial at sea puts its first three lessons up even though the run's tips are quiet,
takes each card's shot (`lesson-target.png`, `lesson-attack.png`, `lesson-ability.png`) and does what
it says, as a player would: the pretend mouse clicks the nearest wraith (`combat-target.png` is the
target picked), GOT IT, and 2 for the first skill; then the AI plays on, and the log's shown later
(`combat-log.png`). A fight the run loses is retried like a player would: from the autosave, and
after two losses, on Story.

**The fight stages** (`LegendQuestGame.ProofArena`, `ExploreScreen.ProofArena`) are run on their own:
`--proof <dir> Arena`, `Boss` or `Classes`. Each bout is a fresh game the way the story has it at
that fight: you (a Swordhand, Caleb), the Soul-Bound (Wren) and the companions who've joined by
then, at `LevelTable`'s level and the class rank the story's fights before it give, in the room the
fight's fought in, with the story's flags up to it, and the save made as the fight starts, so the
room starts it as it loads. The party's AI fights for everyone. Each bout saves, once each, the
fight's start, the thick of it, the bar's cooldowns just after your first skill, a telegraph's
marker part filled, each boss phase's banner, a friend downed and being helped up, the Soul-Link
Strike, and the panel. **Arena** fights the Red Gulls (filming their melee from Grell's first Hook Pull, every second frame,
into `seq/`, to see how a crowd stands round its targets), the gloomfangs, the raid (filming the
Hexer's first Hex), Kael's duel (filming his Smoke Bomb and Cunning Disengage) and the sappers, then
the fields: the Mirewood's pack coming at the party on the path, the room cleared, the party resting
(`field/wolves-resting.png`: the run takes them to half first, and writes their hit points before and
after), and the pack back, on its own spots,
a minute later; and, after a look at your character sheet (`field/construct-sheet.png`: the kit's icons,
the rank locks, the class points), a vault construct pointed at (its red name, the hand) and clicked. **Boss** fights
the Thornmother through all three phases with the Lodge's wardens on the lip, and films her first
Thorn Burst every second frame into `seq/`. **Classes** puts six classes in your place (Swordhand,
Arcanist, Lightbearer, Warden, Hemomancer, Lodge Ranger) at rank 5 in the raid on Thornbridge (the
longest of Act 1's fights), presses each of their four skills as soon as it can go, through the same
code keys 2 to 5 use, with a shot of each, and
films the first two classes' ultimates into `seq/`. It also films Riposte, Second Wind and Raise
Shield for 1.5 s from the moment each starts (`ArenaPlan.FilmSkills`). When somebody stood level with
or in front of the leader, drawn over the move, the next go with nobody there is filmed too (`-clear`). A foe move with no
telegraph is filmed the same way from when a foe starts it (`ArenaPlan.FilmFoeSkills`). A bout that's lost is fought again on the next
seed, three times at most, and one that runs past three minutes is given up on, each with a line on
the error stream.

| File | What it is |
| --- | --- |
| `Screens/ExploreScreen.Combat` | Starting a fight in place and ending it: the roster, the arena, the Battle, the bystanders, the panel and the rewards |
| `Screens/ExploreScreen.CombatLoop` | The fight's clock, stepping the engine, placing everyone, keeping clips in step |
| `Screens/ExploreScreen.CombatCues` | What each event looks and sounds like, on whoever it happens to |
| `Screens/ExploreScreen.CombatHudView`, `CombatDraw`, `CombatCards` | The HUD's views from the engine, drawing the fight, the card for someone in the room |
| `Screens/ExploreScreen.CombatInput` | The mouse and the keys as BattleCommands |
| `Screens/ExploreScreen.CombatAllies`, `CombatTutorial`, `CombatProof`, `ProofFights` | The supports, the lessons and their gold pulse, and the proof run's fights |
| `Screens/ExploreScreen.Field`, `Combat/EncounterCatalog.Field` | The fields: their foes put down, red names, a click or coming near to fight, and the room filling again |
| `Screens/ExploreScreen.ProofArena`, `LegendQuestGame.ProofArena` | The Arena, Boss and Classes proof stages |
| `Screens/ExploreScreen.Debug` | F9 and F10 in debug builds: the next class, the next rank |
| `World/Rooms/BattleFloor` | The arena from a room's floor band, and its check |
| `Combat/FightRoster`, `Slot`, `CombatStats` | A fight's starting foes and their ids, a foe's place when it isn't in the room, and a party member's numbers from what they wear |
| `Components/CombatLog`, `CombatCard`, `TurnBanner`, `CombatResult`, `SupportBanner` | The log, the card, the banner for the fight's name and a boss's phase, the panel at the end, and a support's banner with its ring |
| `Components/CombatEffects`, `DamagePopups`, `ScreenShake`, `CombatStyle`, `CombatArt` | What's thrown about, the numbers, the shake, the colours of each kind of damage, and the fight's sheets |

## Conversations

A conversation is a `DialogueScript`. The story's are written in `Story/04` to `08` in the notation
`Story/01-build-plan.md` §4 gives, and `tools/story/compile_scripts.py` turns them into C# in
`Dialogue/Scripts/Generated/` (nobody edits those: a wrong line is fixed in the `.md` and compiled
again). The rest are written in C# in `Dialogue/Scripts/`, a file per chapter, beside the constants
that name every chapter's flags. A script says who it's with, the flags it `Needs` and the ones that
stop it (`Unless`), and its lines.
`DialogueCatalog.For` picks what someone says: the first of theirs that can start. Talking to them
when there isn't one gets their `say` line over their head instead.

- **A line** has who says it (one of the cast, you, the Soul-Bound, a companion, the narrator in a
  colour of its own, a card big in the middle of the screen, or a chapter's card, the chapter's name
  under the act the way the ship coming in ends on Chapter 1's), what they say, and a stage direction under
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
- **Stage cues** are what happens on the map around a line before it shows: someone put at a spawn
  or beside someone, walking somewhere, turning, a clip, something held, a glow, the camera, a fade,
  a shake, a tint, a sound, the music. A story script writes them on `!` lines over the line they're
  for (`! place sera at treeline facing left · anim sera bow-lowered`), and the ones after the last
  line are its `Closing`, played once that line's put away.

How the cues play is `Story/01-build-plan.md` §5. `StageDirection` reads a cue's words, the same
way for the game and for the start-up check. `CutsceneDirector` plays a line's cues as the line comes
up: everyone has a queue of their own, so a cue waits for the last one on the same person (walking
and clips take time; the rest is at once) while cues on different people go together; a fade, a
wait, a backdrop, the camera, a travel-actor and a slow wait for everything before them and hold
up everything after; and the line shows once nothing's left. The panel's down while they play, and
Escape or a click puts everything at its end at once. A cue line's `{if …}` is looked at as the line
comes up. The director keeps the clock, the fades (drawn by the conversation, over the painting and
under the panel), the sounds, the music and the slow; what happens to the world goes through
`ICutsceneStage`, which takes actor ids, spawn names and world positions and nothing about tiles, so
the side-on rooms can be one. `ExploreScreen` is the stage, in its room.
Actor ids are `you`, `soulbound`, `party`, `both`, a companion by their origin id, anyone on the map
by their id, and a cast id, which is everyone the map puts down as that one of the cast and every
copy numbered after it; the plural is the same, so `royal-guards` is every royal guard in the keep,
and `place royal-guard x4 at throne` makes `royal-guard-1` to `-4` round the throne. Someone a cue
puts on the stage who isn't there, but is one of the cast or the creatures, is brought on.

Legacy, which is how everything's drawn for now, does what build plan §5.3's Legacy column says. A
spawn's spot is exactly where the map has it (the spawns a scene uses are in the map files beside
who stands there), and beside, before, over and the crowd are the nearest spots the feet fit. Walking
follows the mouse's A* paths at a walk. A clip shows one of the sheet's five poses where
`LegacyClips` has one for it (a pommel strike is a strike, a heal is a cast, a fall is downed) and
nothing where it doesn't, which isn't a fault; a pose is held through its line, and downed till a
clip gets them up. What someone's holding goes in the save's sockets and isn't drawn. A light is a
glow round whoever it's on (or whoever's holding it). The sigil comes up on the corner of the party's
cards. A swap changes one of the cast to another of their looks in `Cast`. An echo is someone at
half strength, a reflection and a trail are nothing, a tint fades the map's light to one of `Tints`
over 0.6 s, a shake is the fights' screen shake, and backdrop and board are the line's painting and
quest board, which the compiler has already put on it, so they aren't drawn twice. A travel-actor
to another map takes the party when the conversation's over, since the map can't change under a
scene that's still on it. The creation screen's scripts aren't on a map, so there their cues on the
world do nothing; their fades, sounds and backdrops still play.

`DialogueRunner` plays a script against the save the way The Black Box's discussion did: it draws
nothing and takes its `Random`, and hands back what happened as events (a meter moved, a check
rolled, someone joined) for `DialogueScreen` to turn into toasts, dice and sounds, and for the map
under it to catch up with, like the Red Gulls leaving the docks the moment they're beaten. When a
companion's approval moves while they're in the party, a toast says they approve or disapprove.
Every script is checked when the game starts, after the maps: every line going somewhere that
exists, every flag, meter, item, painting and person being real, every check having somewhere to go
on a pass and a fail, every clock having somewhere to go when it runs out, and every map and spawn
a conversation travels to existing. A mistake stops the game right there and says which line. So
does a cue that couldn't play, in a debug build: its words wrong, or a spawn or someone it names
that can't be on its script's map (the header's `map`, or the last one named over it in the story
file); a release build writes those to the error log instead and skips the cue when it comes. A
script can be marked as work (`MarkerKind.Quest`) or an event (`MarkerKind.Event`), which puts the
gold or red "!" over whoever has it.

**Banter** is the talk that doesn't stop you: `Story/Banters.cs` has the short exchanges, a few lines
each, that play in bubbles over the speakers' heads while you keep walking, when you walk into a
trigger with `bark=` on it. The party comments on where they are (the fish on the docks, the
fountain, the board, the Flagon, someone's cottage you've walked into uninvited), and townsfolk talk
to each other: sailors, fishwives, two of the Watch, gossiping stall-keepers, an old couple. A
banter can need flags or be stopped by them, so the docks sound different once the Red Gulls are
gone. A speaker who isn't there is skipped, only one plays at a time with one more waiting, and one
that can play again (`once=no`) waits a while first. `Dialogue/Scripts/Townspeople.cs` has the
people you can stop and talk to round the city, each with a few choices, a check or two, and
something different to say the second time, and the side-quest hooks the board's postings hint at.

## The tutorial

Nobody reads a page of controls before they've walked anywhere, so the game teaches itself as you
play: a tip comes up the first time you need it, says one thing in a sentence or two with its keys
drawn as key caps, and never comes up again in that game. It doesn't stop anything. You can walk
while a tip's up, and it goes by itself once there's been time to read it twice, or straight away
with **Enter** (X on a pad) or a click on it. Tips that come at the same moment wait their turn, so
they come one at a time with a breath between, and while a conversation or a menu's over the map
the card fades out and its clock stops, so it's all there to read after. If you do the thing before
its tip has come up (you talk to someone before the tip about talking), it never comes, and if it's
up when you do it, it goes, since it's done its job. **TUTORIAL TIPS** on the options switches them
off and on, and with them off the controls go up as one toast when the ship comes in, the way
they did before there were tips.

| File | What it is |
| --- | --- |
| `TutorialTip` | One tip: its id, its title, its words, the keys it teaches, where its card goes (`TipAnchor`), what it's about (`TipTopic`), whether it holds everything until it's read (only the first fight's does), whether it holds a fight while the keys still work (the fight's lessons, `HoldsFight`, which never go by themselves), and what a slot in its words says when the game has nothing more particular (`Fill`; HOW TO PLAY shows that) |
| `TutorialCatalog` | Every tip, checked when the game starts like the other tables: no two with one id, a title that fits on its card, no more than 120 letters of words and six keys, and every id the game calls by name in the table. A wall of text stops the game before it can turn up mid-play |
| `Tutorials` | The tips for one game: which have been seen (kept in the save, in `SaveData.Tips`, so it's once a game and a new game on another slot teaches it all again), the one that's up and the ones waiting. A tip counts as seen the moment it comes up, so quitting with one on screen doesn't bring it back. It draws nothing; `Show` puts one in line, `Complete` says you've done what it teaches |
| `TutorialPanel` | The card: cut from the forms' panel at the party cards' size, the title in gold, the words in bone, the keys on ivory caps (`KeyCaps`), and ENTER · GOT IT in the corner. A gold edge flares as it comes in and settles, so the eye goes to it once. It rises into place, or drops for the ones along the top |
| `HelpScreen` | HOW TO PLAY, from the pause menu. A page of every control on the keyboard, the mouse and a pad, then every tip under its topic, laid out down two columns and onto another page if they need one. The tips are the manual, so one put away too fast can always be found again, and the two can't say different things |

| Tip | When it comes |
| --- | --- |
| Finding your feet | The first moment you can walk: on the Gull's Wager's deck in a new game. It goes once you've walked a few seconds |
| A red mark, A gold mark | The first time there's a red "!", and the first gold one over someone outside the party |
| Someone to talk to | The first time someone's close enough to talk to |
| Chests, doors and levers, Waypoint stones, The quest board | The first time one's in reach |
| Going inside | The first time you're by the door into a building |
| What's next | The first time the line under the map's name changes, pointing at it from under it |
| The map | The first time you're somewhere that's on the world map, under the M on the top right |
| Your party | Once there are two of you and you've walked twenty seconds on one map, over the party's cards. It goes the first time you change who you're controlling |
| Something to say | The first time one of the party has a gold "!" over them, or something to say with you beside them |
| Waiting at the Flagon | The first time anyone's waiting at camp |
| Every control | Once you've had five tips: Esc, then HOW TO PLAY |
| The fight's lessons | `combat-start`, `combat-target`, `combat-attack`, `combat-ability`, `combat-cooldown`, `combat-focus`, `combat-potion`, `combat-telegraph`, `combat-soul-link`, `combat-downed`, `combat-allies`, `combat-move` and `combat-victory`, at the moments Combat, On screen gives |
| The bag's | `inventory-open`, `inventory-equip`, `inventory-potion` and `loot` |

The conversation's tips (going on, answering, the dice) and the wheel are only in HOW TO PLAY: a
conversation has the whole screen, and there's no single moment to say the wheel zooms. The map
screen notices the moments in `ExploreScreen.Tutorial.cs`, since nearly all of them are things it
already knows every frame (who's nearest, who's marked, what you're to do next), and the few that
are something you do (talking, using something, opening the map) tell it from where they happen.
The proof run keeps the tips quiet (`Tutorials.Quiet`), so its shots are the same as they were and
nothing waits on a press it can't make; `ProofTip` puts one up for a shot of its card, and the
tutorial fight puts its first three lessons up the same way.

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

A creature's sheet (`Content/Creatures/`) is the same ten columns and four rows in a frame of its
own size, with its feet in the middle of the bottom, two pixels up: a Gloomfang is 48x32, the
Thornmother 96x80, a vault construct 48x64, and the Hollowed are 32x48 like a person. `WalkerShape`
says how big someone's frames are, where their feet are in them, and the box, shadow and footprint
that go with them; everyone who's a person has the one shape, and `Creatures` has the rest.

`WalkerSprite` is The Black Box's on the new layout: it cuts frames from a sheet, walks through
columns 1 to 4, and fires `Stepped` on 1 and 3, when a foot lands, so the footsteps match the feet.
The footstep is grass or stone depending on the tile in the room's strip under you. In the side-on
rooms only rows 1 and 2 ever show, facing left and right (`WalkerSprite.SideOnly`): setting someone to
face up or down does nothing, and turning to a point goes by which side of them it's on, for the
party, the townsfolk's glances, a fight and a scene's cues alike. The sheets keep their four rows. A
room's drawn at 4x, so a person's 48-pixel frame is 192 screen pixels tall, every pixel of it a 4 by
4 square. The party's sheets are
baked by the paperdoll, below. The townsfolk have sheets of their own, cut from my pictures already
dressed, and since they only stand and walk, theirs stop after the first five columns.

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
build, with rows taken out of the torso and legs: `Paperdoll/Stout/` and `Paperdoll/Small/`.
`Paperdoll.Compose` lays the sheets someone's look and gear call for over each other, in the order
`PaperdollOrder` gives for each way they face, and `CharacterPalettes` swaps each key shade for the
same shade of the preset their look picks, out of `character-palettes.png`: six skins, eight hair
colours, six eye colours, five shirts, eight vests, six for the legs and six cloaks. That's The Black
Box's palette swap, done once per change with `GetData` and `SetData`, not every frame, and two
people who look alike and are dressed alike share one sheet (`Paperdoll.Key`).

He was only ever drawn standing and walking, so the five fighting columns are his standing frame
posed like a paper puppet. Each frame has one map of where every pixel in it comes from, and every
layer goes through the same map, so a hat stays on his head and a sleeve on his arm and the layers
still stack. His arms come off at the shoulder and go where the frame wants them, he leans from his
feet, sinks or comes up at the knee, and his feet go apart. Winding up, the weapon arm goes up over
his head; striking, it comes across in front of him from the front, straight out from the side, and
stays up for a blow over the head from behind; casting, both arms go up from the front, and from
the side he pushes the spell out with both hands; hurt, he's knocked back and his knees go; downed,
the whole of him is turned onto his back across the frame, with rows out of his legs and body if
he's too long to fit. From the side his near arm is the back of him, so when it moves the gap is
filled from his body in front of it. A cape's back half and a tail's hang behind him and don't go
with his arms.

Gear is more layers, named the way the items name them: nine body armours (padded, leather,
studded, hide, a chain shirt, scale, a mail hauberk, plate and a robe), three helms (a leather cap,
a great helm that hides the hair, and a circlet), gloves and gauntlets, leather boots and iron ones,
the cloak (which is the cape), two shields and thirteen weapons. What's worn is drawn onto his frames
from the same measurements as the hats, so it's made into the other builds and posed like the rest.
Cloth gear is in his vest's keys, so a robe comes out whatever his vest is dyed; metal and leather
have keys of their own, and `gear-palettes.png` has those and a few tints (iron, blackened steel,
bronze and gold; dark, black, tan and red leather), so `weapon-longsword:gold` is the same sheet in
gold. What's held has to swing, so it isn't posed: each weapon and shield is drawn on every build's
own frames from wherever the hand holding it has got to. The weapon is in his right hand from the
front and from behind and the hand nearer you from the side (the two side rows are each other in a
mirror), lifted over the shoulder to wind up and brought down or driven in to strike. A bow or a
crossbow is carried in the other hand; winding up he takes an arrow from the quiver on his back,
and striking it's in his bow hand at full draw. A staff is held up high to cast. The shield is on
the other arm, over him facing you and behind him every other way, with its rim showing past him.
Anything long enough to go off the edge of a frame is cut short, as if it's pointing a little
towards you or away. `ExploreScreen` dresses each party member in what their `Equipment` holds
(`Paperdoll.GearOf` reads each item's `Layer`), and `Paperdoll` checks every layer is there for
every build when it loads, and that every item's `Layer` is one it has.

### Fighting moves and impacts

The real-time fight drives a figure by moves rather than poses. `WalkerSprite.PlayMove(name,
restart, speed, hold)` plays any clip by name (`auto-two-hand-2`, `skill-wizard-fire-bolt`, `hurt`,
`knockdown`, `die`), starts it again if it's the one playing, and returns the clip, whose events
(`Clip.Events`, `FirstEvent("hit")`) say when its blows land. As playback crosses each event the
figure fires `ClipEvent` (once each time round a loop), so the screen lands the number, the flash
and the sound on the frame the clip does. A clip that's played out goes back to `ready` in a fight
(`InFight`) or `idle` out of one, unless it holds (a death, a knockdown until its get-up). A missing
clip falls back to the nearest one there is, so a fight never stands still for want of one.
`WeaponKind` is the kind of weapon the engine says they swing. The old `Play(pose)` still works.

`ClipTrack` eases each key as it says (`smooth`, `in`, `out`, `back`, `hold`, `linear`), with the
same maths as `tools/rig/clips.py` to the fourth decimal place, and `Skeleton.Solve` carries a
bone's stretch to its children's joints the way `skeleton.world()` does.

`Characters/Motion` stages each blow on the figures themselves, with the numbers in `Feel` (the
feel spec): a hit-stop that freezes the attacker and the target alone for 3 to 14 frames
(`Feel.HitStopFrames`), the target shaking a pixel or two and tapering off, the attacker creeping on,
the world going on round them; a white flash in the figure's own shape, 2 frames (4 on a critical),
then an element's tint for a spell (`Silhouettes` makes a white copy of each piece once); a squash
that springs back, and a knock away from the blow (`TakeHit` does all of these at once). Through a
clip's `trail` windows the weapon smears longer along its blade and leaves a fading ribbon from its
base to its tip, sampled four times a frame from the clip so a fast swing stays a smooth arc
(`WeaponTrail`, drawn additively by `RibbonBatch`). The skirt and a cape swing after the body on
damped springs (`Spring`), held still through a hit-stop. Hair is painted into the head piece, so
it moves with the head. A creature drawn from a sheet (a sea wraith, a Gloomfang, the Thornmother)
plays the same names as procedural motion on its frames (`CreatureMotion`): a wind-up that crouches
and pulses red, a lunge that stretches toward its target, a squash when it's hit, and a death that
flashes, is knocked back, tips over, holds and fades into motes.

`Components/CombatEffects` draws what flies about: sparks along the swing and the arc of the blow
(a crescent for a blade, a ring for a blunt blow, a streak for a thrust), a burst for each school
(fire, frost, lightning, radiant, necrotic, force, psychic, nature, thunder, poison, blood), bolts
with ribbons, beams, a charge in the hand, dust when a body lands, death motes, and ground markers
(a ring, a line and a cone that fill over their time and flash when they resolve). Its shapes are
made when it loads (`Particles/EffectTextures`), so there's no art for them. `ScreenShake` adds
trauma by cause (`ShakeCause`: none for an auto attack, 0.25 for a heavy skill, 0.4 for a kill, 0.2
more for a critical), kicks the picture a few pixels the way a heavy blow went, and has a strength
(full, reduced, off). `DamagePopups` colours its numbers by what they are: white dealt, gold and
bigger with a bounce for a critical, red taken, green with a "+" for healing, grey words for MISS,
DODGE and BLOCK, smaller tinted ticks, and at most six over anyone, the rest merged into a running
total. `Settings` has `ScreenShake`, `HitFlashes` and `DamageNumbers`, all on to start with.

`--proof <dir> Moves` films all of it in a room of its own, a sixtieth of a second for each frame
drawn whatever the real clock does, into `<dir>/moves/<take>-NNN.png` (every second frame): each
weapon kind's auto attacks, some skills, a critical, a block, a dodge, a knockdown and get-up, a
death, each creature's wind-up, attack and death, each ground marker, every school's bolt and the
numbers. It also writes `moves/samples.tsv`, the clips sampled every frame, to check the easing
against the Python's.

### The fight's clips

`tools/rig/combat_clips.py` makes the fight's clips, and `tools/rig/clips.py` adds them to
everything else it writes into `clips.json`. There are three auto attacks for each of the nine kinds
of weapon (`auto-<kind>-1` to `-3`: for a sword, a chop over the top, a rising backhand and a
lunge), and a `skill-<id>` for every class skill that isn't passive or a weapon auto. There are also
`telegraph-<kind>` (a held coil that loops), the rigged foes' moves (`foe-<enemy>-<move>`) and the
reactions: `block`, `dodge`, `hurt`, `hurt-big`, `knockdown` and `getup`, `downed`, `die`, `revive`,
`victory-<kind>`, `ready` (with `ready-<kind>` for each weapon's guard), `cast`, `channel`,
`drink-potion` and `soul-link-strike`.

Each skill clip comes from an archetype with its own numbers. The archetypes are a heavy blow, a
combo, a leap and slam, a spin, a dash, one- and two-handed casts, a throw, a war cry, a shield
bash or slam, a reach to an ally, a transformation and so on.

Where an archetype would make two skills look alike, the skill has a move of its own that says what
it does. Raise Shield raps the sword on the shield and lifts it high. Hold the Line stamps down
into a wide stance behind the shield, and Vigil kneels with the sword planted. Rally sweeps the
blade up from low and pumps it, while Call the Order is a knight's salute. Rum Ration is a swig and
a toss, Honey Salve a dip in the pot and a smear, and Angry Hive the smoker shaken overhead. Haste
winds the free arm round in a big circle like a clock's key. Read the Wound hunches over with the
head down and the hand at the chin. Cauterise presses down low and snatches the hand up to the
shoulder, and Warm Hands flings the arms wide, one forward and one back. Ward is traced from low to
high and set up over the ally, and the Hexer's Hex is thrust out and wrenched back to the chest. The
casts that shared a style have their own: Stop is raised high and flat, Eldritch Blast is drawn back
by the ear and driven out, Cinder comes from the lips, Moonbeam is pulled down from the sky, and
Sanctuary, Vespers and Sunburst set the lantern low, lift it to the bowed head and sweep the blade up
from the hip. A hand is always drawn as a fist, so a move says what it does with the arm and the
body, never with the fingers. These moves keep the archetype's frames up to their `hit` or
`release`, and its length, so the fight's timing doesn't change.

A pose gives each hand's turn between -180 and 180 degrees, so `seq()` takes each hand the shortest
way round over the whole clip (`unwound()`). A spear or a lantern doesn't spin a full turn between
two keys that are only a few degrees apart. A swing that is meant to go the long way round is listed
in `OVER_THE_TOP` and keeps its keys as they are: the dagger's third, which brings the blade forward
over the head and down (the third auto, Backstab and the last blow of Shadow Dance).

Each blow eases out into its coil and holds there a moment. It strikes in two or three frames at 60
a second (ease `in`), overshoots on a `back` ease and settles. The hips and torso lead the shoulder,
and the head comes late. On the contact frames the weapon hand is stretched into a smear, inside a
`trail` window, and the skirt swings after the hips on a spring.

Events: `hit` (one for each blow of a multi-hit), `release` (with the hand), `recover`, `land`,
`telegraph`, `step` and `fx`, whose ids are only for show:

- a charge's `glow-hand`, `glow-hands`, `glow-body` and `glow-weapon` start as the wind-up does;
- `glint` marks the start of a hold;
- the others are `burst-self`, `burst-ground`, `dust`, `smoke`, `spark`, `ring`, `coin` and `echo`.

`python tools/rig/clips.py --check` fails on any of these:

- a foot through the floor;
- a joint bent backward;
- a clip the fight needs that is missing;
- a blow with no `hit` or `release`;
- two skill or foe clips that look alike (`likeness()`). Each clip is sampled at 41 points through
  its length, the hands and the scales left out. How far apart two poses are is the widest gap of
  any channel. The two clips' samples are lined up by time warping, so two clips that go through the
  same poses in the same order are as far apart as their poses are, however differently they're
  timed: the same archetype and style with other timing counts as the same move. A pair whose
  matched gaps average under 5 degrees fails, and so does a pair whose body tracks are the same
  keys. When two foes share one ability, one clip serves both through an alias:
  `foe-sapper-chief-blasting-charge` is `foe-hollowed-sapper-blasting-charge`.

`tools/rig/clip_strip.py` draws a clip's frames at 30 a second on a dressed figure, holding what
that kind or class fights with. Each event is marked under its frame, and an onion skin of the
whole move closes the band. With `--gif` it also writes the clip as an animated GIF.

## Saving

Five slots, one JSON file each, under `%LOCALAPPDATA%\LegendQuest\Saves`: three numbered ones
(`slot0.json` to `slot2.json`), `quicksave.json` and `autosave.json`. Not next to the executable,
because `bin/` gets deleted by every clean build.

| File | What it is |
| --- | --- |
| `SaveData` | The whole save: the zone and the room of it you're in (and where you came in, while you're inside a building), the party (each with where they're standing in the room and what they're wearing), who's waiting at camp and which of the party you're controlling, the play time, the gold, the key items, the party's bag, the tutorial tips already shown, what you're to do next, your voice, who you're with, the story's meters and flags, the chests and doors opened, the triggers used, the waypoints found, every room that's been walked into, and what everyone's holding in their sockets, as the scenes' hold and drop cues left it. It grows with the game: each system adds its field when it exists |
| `SaveSlot` | One slot as the form sees it: empty, occupied or unreadable, plus the line printed on it |
| `SaveSystem` | Reads, writes and erases slots. The only thing in the project that touches a save file |

The same three decisions as The Black Box:

- **Writes are atomic.** A save goes to a `.tmp` first and only then replaces the real file, so a
  crash mid-write leaves the old save intact. The one it replaced is kept as a `.bak`.
- **Nothing throws.** A missing, locked or corrupt file comes back as a slot that says UNREADABLE
  and can be erased. So does a save from a newer build of the game, since writing it back would
  quietly drop the fields this build doesn't know about, and so does a save in a zone this build
  doesn't have.
- **Every file carries a `Version`.** `SaveSystem.Upgrade` is where an old file gets brought
  forward when a field changes, one version at a time. Version 2 is the party: version 1 had one X,
  Y and Facing for you, and upgrading gives it the starting party with you standing there. Version
  3 is the Soul-Bound: Valen becomes them, keeping his name and look, the placeholders go, and the
  Prologue counts as done, since those games started on the harbour. Version 4 is items: everyone in
  the party and at camp gets dressed in their kit, and the bag gets the two healing potions a new
  game starts with. Version 5 is the act as a meter: Act 1 unless Act 1's last card has been seen.
  Version 6 is the sockets: the herring and Tomas's hook, if they've been given, go round your neck
  and on your belt, where the voyage and the docks put them. Version 7 had the rooms' fields
  beside the tile maps'. Version 8 is the rooms: a game finds its zone (its map's id) and its room
  (the one whose old tiles the leader stood on, by the zone file's `legacy` rectangles, at the same
  share of the way along it and halfway down its floor; the zone's start if none), the whole party
  stands there and falls in behind you once the room's up, the way back out of the last building gets
  the same, and every room of every map it had seen any of counts as visited. Those old fields only
  make it as far as `Upgrade` because `SaveData.Extra` keeps whatever this build doesn't have a
  property for, and the tile maps' map, way back and fog are read out of the file on their own.

Enums go in by name (`"Facing": "Up"`), so reordering one is safe and renaming one breaks saves; a
number where a name should be makes the slot unreadable. Things in a zone are kept as `zone/id`, like
`road/camp` for the chest at the camp, so two zones can each have a chest called chest; a zone has
its old map's id and keeps its objects' ids, so those needed no upgrading. A list I've nulled by
hand comes back empty instead of crashing, a save with nobody in the party gets the starting party,
anything in the bag or worn that this build hasn't got is dropped (something worn where it doesn't
go goes back in the bag), hit points get pulled back between 0 and the most someone can have, a party
in a room that isn't there, or with its leader off the floor, goes to the zone's start, and feet that
land inside something go to the nearest floor that's clear, or the room's first spawn. A class the game doesn't have makes the slot unreadable.

A new game only goes in a numbered slot, and it's written as soon as you pick one, so if the slot
can't be written the form can say so. Picking one that's full asks first (PICK IT AGAIN TO START
OVER IT). ERASE asks too: the button turns into SURE?. While you play, the game writes the autosave
every time you go into another room or zone, by a way out, a door or fast travel: before anything
fades for another zone, so it has exactly what the next one starts from, and once you're in for
another room of the same one. If it can't be written the game carries on and says so. Saving where you like, and F5 and F9, come with the rest of saving at the end.

The volumes and the text speed aren't in a slot. They're in `%LOCALAPPDATA%\LegendQuest\settings.json`,
since turning the music down shouldn't depend on which game is loaded.

## Music and sound

The sound effects are built by `tools/generate_audio.py` from sines, wavetables, noise and a few
filters, in CPython (3.12 or newer); the oscillators, filters, reverb and song mixer are The Black
Box's. Five of the real-time fight's sounds also have a recording layered under the synthesis, from
the Versilian Community Sample Library below (ASSETS.md names each one). The music is composed in `tools/score/` and played on recordings of real
instruments, from two sample libraries by Versilian Studios released under CC0 1.0 (VSCO 2 Community
Edition and the Versilian Community Sample Library). `tools/score/fetch_samples.py` fetches only the
instrument folders the score plays, pinned to one commit of each, into a cache outside the repo
(`%LOCALAPPDATA%/LegendQuest-samples`, or `LQ_SAMPLES`); the recordings are never committed or
shipped, and `compose.py` stops and says how to fetch them if they're missing. The rest is
`tools/score`'s own, in numpy: a reader for the libraries' SFZ files, a sampler (re-pitching by
resampling, velocity layers, round robin, an envelope, each part's timing and dynamics humanised
from a seed), a synthesised choir for the voices the libraries don't have, a mixer with a synthetic
hall, and mastering to a loudness with a limiter that never lets a peak past -1 dBFS. Every piece is
written note by note on one theme, LegendQuest's: up a fifth, a step down and back, lifting to the
sixth, and home through the flat seventh and the flat sixth.

```
python tools/generate_audio.py sfx             # every sound effect
python tools/generate_audio.py pause           # just these
python tools/score/fetch_samples.py            # once: the sample libraries (about 1.6 GB to download, 4 GB on disk)
python tools/score/compose.py                  # every song and jingle (about five minutes)
python tools/score/compose.py steel engage     # just these
python tools/score/check.py                    # their length, loudness, true peak and loop seams
```

| Song | Where | What it is |
| --- | --- | --- |
| *Embers* | Title, options, save slots | The theme stated in full: 80 BPM in D major, 32 bars. The harp spreads D, B flat and C over a held D while a flute calls the theme's head from far off; the horns play the theme over the strings, then the violins take it an octave up with a choir, the trumpet joining for the flat-six, flat-seven, one cadence. A bridge calls the head in B flat, G minor and E flat and climbs to the dominant over the timpani, everyone plays the theme's second half with a cymbal, and it falls quiet over the D again |
| *Hearthsmoke* | Towns, lit maps and houses | The theme as a country dance: 116 BPM in G Mixolydian (the F natural makes it old and homely), in three, 48 bars. A strumstick strums oom-pah-pah over plucked cellos and a frame drum; an alto recorder plays the theme, the folk harp and the violins join it, a solo violin and then a clarinet play a second tune in E minor, and everyone plays the theme with a tambourine |
| *Meadowlark* | The Old Road and the fields | The theme as a journey: 108 BPM in A major with a Lydian lift (B major in place of D), 40 bars. The violas run eighths and the basses walk in pizzicato under a light snare march; two horns play the theme, the flute has a tune of its own over the oboe, then flute and violins take the theme an octave up, the cellos and horns turn the head over, and everyone plays the theme's second half |
| *Cold Stone* | Underground | Hardly a tune: 60 BPM in C Phrygian, 24 bars. The basses and cellos hold C under a low choir moving between C minor and the flat second; cold air, water dripping far off in a long hall. The theme's head turns up in pieces: a far horn, a low harp and the violas, the bells over tremolo cellos, a low flute coming down to C |
| *Salt and Lanterns* | The Gull's Wager | The theme as a shanty: E Dorian in 6/8, a dotted quarter at 56, 40 bars. The folk harp rocks through each chord, the basses pluck the big beats and a frame drum knocks like the hull, with the sea rolling in and out under it, ten waves a loop. The clarinet plays the theme, the recorder takes it an octave up over clarinet and bassoon chords like a squeezebox, the cellos sing a second tune in G, and violins and recorder end it together |
| *Sunspire* | Sunspire Keep | The theme as a royal chorale: 76 BPM in B flat major with the old modal chords (A flat and G flat), 28 bars. A fanfare of trumpets and horns over a timpani roll, then a trumpet plays the theme over the brass in chorale with the organ and the harp; the violins take it, darker, with a choir; and everyone plays it with the full organ and a cymbal |
| *Steel* | Fights | The theme in D minor, driven: 150 BPM, 64 bars. Spiccato cellos and basses hammer eighths on each root, leaping the octave in a 3+3+2 that the bass drum and the timpani lean on. The horns play the theme, the trumpets take it an octave up over violins in sixteenths and a snare, the violins sing it long in G minor over trombone stabs, a build calls the head a step higher every two bars over tremolo strings and a snare roll, everyone plays it with cymbals, the low brass throw the head down and the violins answer in runs, and the horns play it once more under a high violin line |
| *The Hunger* | Boss fights | The theme bent: 132 BPM in D Phrygian with the tritone (A flat where the fifth should be), 56 bars, in Steel's D so the stinger serves both. The strings grind eighths on D that catch on the E flat above, the bass drum and toms pound in half time, the timpani answer D with A flat. The low brass play the theme bent in octaves, the trumpets join over a gong, the organ and choir move through chords a third apart that shouldn't follow each other, the bass falls a semitone at a time from D to A while bells toll D and A flat, everyone plays it, and a trumpet tries the theme as it should be, in D major, and is answered bent |
| *Victory* (`Music/victory`, once) | A fight won | About 6.5 s at 112 BPM: the brass throw the theme's head out in D major, then climb B flat, C, D (the flat six and seven home, Steel's minor turned major) to a held chord with the timpani rolling, a cymbal, a harp sweep and the glockenspiel |
| *Defeat* (`Music/defeat`, once) | A fight lost | About 7.7 s at 72 BPM in D minor: a solo cello sings the theme's head falling instead of rising over low horns and a choir, the harmony sinking D minor, B flat, G minor to a bare D |
| *Engage* (`Music/engage`, once) | A fight starting | About 2.7 s, in Steel's key and tempo: the strings rip up an octave and a half over a timpani roll and a tom ruff, and on the next downbeat (1.6 s in) the brass hit D minor with the timpani, the bass drum and a cymbal |

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
and then two in step, and a ship's bell rung three times for all hands. The road east brought five more:
a gloomfang howling and another answering, the black crystal's hum (two notes rubbing a semitone
apart, throbbing three times and sagging flat at each throb), war horns a long way off grinding a
semitone apart, a bow loosed and the arrow hissing away, and a coin flipped, caught and set down
on another. The fights brought nineteen more, and five of them borrow Steel's call so a fight
sounds like one piece: a blade landing (a swish in, the edge biting, the blade ringing), a miss
whooshing past, a critical hit with a boom under it and the call flaring on chimes, a blunt thud, a
blow taken on a shield, a bow loosed quicker and tighter than the story's, a spell let go, fire
catching and roaring, frost cracking as it spreads, a heal going up a major chord on soft bells, a
potion uncorked and gulped, the Soul-Link (two heartbeats falling into step and two voices sliding
onto one note), your turn (the call run up on plucked strings), someone going down with one
heartbeat after, something dying, the victory (the call on the trumpet over the horns), the defeat
(the call upside down over a choir and a great bell), gear buckled on, and a purse set down on a
shop counter.

`Audio/AudioManager` is where all of it lives: the same `Content.Load<Song>`, `MediaPlayer` and
`SoundEffect.Play` calls as the audio tutorial, in one game service any screen can reach, with a
master, a music and a sound volume. Each screen asks for its song as it comes on, and the manager
fades the old one out and the new one in.

### The real-time fight's sound

The real-time fight (`Story/11-combat.md` §8) has its own sounds, 65 of them, each built three times
from three seeds, in `Content/Sfx/Combat/<key>-<take>.wav`: `python tools/generate_audio.py combat`
builds them all (under a minute), `combat/hit-axe` one of them. A blow is three layers, the click of
contact (over in 5 ms), a body between 80 and 200 Hz, and the tail of what it's made of; a critical
hit is a separate layer played over the blow, louder and longer, with a sub dropping under it. Five
of them have a real recording layered under the synthesis: an anvil in `hit-heavy-blade`, `block` and
`crit`, a gong in `boss-phase` and a bass drum in `impact-thunder`, from the CC0 Versilian Community
Sample Library that `tools/score/fetch_samples.py` keeps outside the repo (each line in `ASSETS.md`
names the recordings). Without that cache those five are left as they are and the rest still build.

The fight drives the music and the sounds through four calls on `AudioManager`:

| Call | What it does |
| --- | --- |
| `BeginBattle(Track battle)` | The fight's starting. Plays the engage stinger (`Music/engage`), fades the map's song out in 0.4 s and brings the fight's song (`encounter.Music`: `Battle` or `Boss`) in under the stinger, dipped until the stinger's done. Remembers which map song was on and how far it had got. A second call before `EndBattle` (a retry) keeps the first one's map song |
| `EndBattle(bool won)` | The fight's over. The fight's song goes in 0.3 s, the victory or defeat jingle (`Music/victory`, `Music/defeat`) plays once, and when it ends the map's song fades back in from where it was. Without the jingle files it plays the old `Victory` or `GameOver` sound instead. A `PlaySong` for that map's song while the jingle plays doesn't cut it off |
| `PlayCombat(string key, float pan = 0, float volume = 1)` | One combat sound. Turns through the key's three takes, nudges the pitch at random (up to 0.08 of an octave for swings and hits, 0.05 for the rest, 0.02 for the bar's clicks, none for the sounds tuned to the battle song), and pans it (-1 left to 1 right, from where it happened on screen). It drops the sound if the same key has played twice in the last 40 ms, or ten combat sounds have, and the big ones dip the music for 0.2 s so they land. Returns whether it played |
| `Duck(float level, float hold)` | Dips the music to `level` (0.65 is about 4 dB down) for `hold` seconds, for anything else that has to land |

`Audio/CombatSounds` has the keys and the helpers to pick one: `FamilyOfWeapon(item.Base ?? item.Id)`
and `FamilyOfKind(weaponKind)` give a weapon family, `SchoolOf(damageType)` a spell school (cold is
frost, acid is poison, and the three weapon damage types have none), and `Swing`, `Hit`, `Cast` and
`Impact` make the key. `SwingLeadMs(family)` is how long before the blow lands to start the swing, so
its whoosh peaks on the impact. When the proof run has the game silent, every call is written to
`audio.log` (`battle begin Boss`, `combat hit-axe take 2 pitch +0.03 pan -0.40`, `jingle Victory`,
`song Exploring from 41.2 s`).

| Key | When it plays |
| --- | --- |
| `swing-<family>` | A weapon swung: `SwingLeadMs` before the `hit` event of an auto or a weapon skill (at once if the clip's `hit` comes sooner). For `bow` and `crossbow` it's the shot itself, played on the `release` event |
| `hit-<family>` | That weapon's blow landing, on the `hit` event, when the roll hits (every hit of a multi-hit skill; the voice limit thins them) |
| | Families: `light-blade` (dagger, shortsword, rapier, scimitar), `heavy-blade` (longsword, greatsword), `axe` (handaxe, battleaxe), `blunt` (mace, warhammer, quarterstaff), `polearm` (spear), `bow`, `crossbow`, `unarmed`, and for creatures `claw` and `bite` |
| `crit` | Over the hit or impact sound, on the same frame, when the hit is a critical one |
| `block` | Instead of the hit, when a blow is blocked (the shield's share of AC turned it) |
| `dodge` | Instead of the hit, when a blow is dodged |
| `miss` | Instead of the hit, when a blow simply misses |
| `cast-<school>` | A spell's wind-up: at the start of its clip or cast bar |
| `impact-<school>` | The spell landing on its target, on the `hit` event (a projectile's arrival); also a damage-over-time's first bite |
| | Schools: `fire`, `frost`, `lightning`, `radiant`, `necrotic`, `force`, `psychic`, `nature`, `thunder`, `poison`, `blood` |
| `heal` | Someone healed by a skill (not a tick) |
| `buff` | A beneficial aura put on someone (a blessing, a stance, haste) |
| `debuff` | A harmful aura put on someone (a curse, slowed, marked, poisoned) |
| `aura-tick` | An aura ticking every 3 s (a burn, a bleed, a mending). Very quiet, meant for the leader and their target only |
| `telegraph` | A foe starting a telegraphed wind-up, with its warning marker |
| `boss-phase` | A boss changing phase |
| `knockdown` | Someone knocked down, on the clip's `land` event |
| `body-fall` | A body hitting the floor on a death clip's `land` event, after `death` |
| `death` | A foe or an ally dying |
| `downed` | A party member going down (down, not dead) |
| `revive` | A downed party member brought back up |
| `potion` | A potion drunk |
| `soul-link` | The Soul-Link strike |
| `ui-press` | A skill pressed on the bar and accepted |
| `ui-denied` | A skill pressed that can't be used yet (cooldown, Focus, reach) |
| `cooldown-ready` | The leader's skill coming off cooldown (not the global cooldown) |
| `level-up` | A level gained, on the fight's result panel |
| `victory` | The last foe falling, just before `EndBattle(true)` |
| `defeat` | The last of the party falling, just before `EndBattle(false)` |

## The real-time HUD

The real-time fight's HUD is in `Components/Realtime/`, and it draws nothing but plain view records
(`Views.cs`): `FighterHudView` for someone in the fight, `AuraView`, `CastView`, `SlotView` for a
hotbar slot, `SoulLinkView` for the sigil on 7, `BossView`, `NameplateView`, and `HudView` for the
lot. The fight screen builds a `HudView` from the battle each frame, so the HUD never sees the
engine; every time in them is in seconds and every share is 0 to 1. `CombatHud` lays the pieces out
and draws them together: `Update` once a frame, `DrawRing` in a point sampled batch before the
figures (the target's ring is on the floor), `DrawArt` in a point sampled batch after them, and
`DrawWords` in a linear sampled batch last. `At(mouse, view)` says what the mouse is on (a slot, the
sigil, an aura, a party row) for clicks, and the card for it is drawn by `DrawWords`.

- **`PlayerFrame`** (top left): the face, the name and class, health with temporary hit points as a
  pale shield on its end and a lag that drains after a hit, Focus (lit paler for what the slot under
  the mouse would spend), and the aura row under it.
- **`PartyFrames`**: a row each under your auras, with their auras beside it; a downed one greys,
  says DOWN, and shows its death saves.
- **`TargetFrame`** (top centre): the same for your target, with its armour class and its cast bar
  (grey with a lock when it can't be stopped). With a boss up it drops under the boss's frame, and
  while your target is the boss itself it isn't drawn at all, since the boss's frame already shows
  its health, cast and auras; an add you target keeps its own.
- **`BossFrame`**: wide, across the top, with the boss's title, a tick on the bar and a pip for each
  phase, its cast bar under it and its auras.
- **`Hotbar`**: slots 1 to 6 and the Soul-Link sigil on 7, each clickable. A cooldown is a dark veil
  sweeping round clockwise with a pale hand on its edge and the seconds left; the global cooldown is
  a lighter one over every slot on it. Out of reach is tinted red, too little Focus is grey with the
  cost in red, a press flashes, a slot coming off its cooldown catches a glint, and an armed
  reaction glows gold. Your cast bar and the auto attack's swing sit over it.
- **`AuraRow`**: each aura's icon in a green (good) or red (bad) edge, its stacks, its time left, the
  share gone veiled, and a pulse in its last three seconds.
- **`Nameplates`** and **`TargetMarker`**: a name and a thin health bar over each head, a foe's
  telegraph as its move's icon and a warning bar that fills until it lands, and the ring under the
  target's feet with a chevron over its head.

Its icons are `Content/Combat/skills.png` and `Content/Combat/auras.png`, 24x24, drawn by
`tools/generate_combat_art.py`: one for every skill in `Story/10-classes.md` (a passive's with a gold
rim), the bar's own, the foes' telegraphed moves, and every condition and aura the combat design
names. Each is a glyph on a tile in its school's colour. `Content/Combat/icons.json` lists each
sheet's ids in cell order, and `HudIcons` finds an icon by id, not by any enum's order, so adding one
never moves the rest; an id with no icon falls back on the plain buff or debuff. Timers are written
in `spectral-timer` (the cooldowns' seconds) and `spectral-small` (everything small); both carry the
point, so "2.5" and "12s" can be drawn, and `spectral-number` has the point, the slash, the colon,
the percent and m, s and x besides its digits.

`--proof <dir> Hud` opens `HudPreviewScreen` over the farmland with a party and foes standing in it
and saves `hud-fight.png` (cooldowns mid-sweep, an armed reaction, too little Focus, a foe winding
up and casting), `hud-card.png` (a slot's card), `hud-reach.png` (out of reach, the global cooldown,
a press, your cast, an aura's card), `hud-boss.png` (a boss in its second phase casting what can't
be stopped, someone down with their death saves, the sigil lit, a glint), `hud-icons-1.png` and on
(every icon at twice its size with its id), and `hud-checks.txt`, the sums a picture can't show:
the hit-testing, the timers' wording, the fonts' marks and the sweep's frames.

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
| `Tiles/questeria.png` | `Tilemap` | 16x16 tiles, eight to a row, in `TilesetCatalog` order: flagstones of every size with moss in the joints, the odd crack and a petal or two, the terrace's slabs, rounded cobbles, grass with tufts and tiny flowers, sand, the boardwalk's planks with their grain and nails, and the water, four frames of cyan ripples and glints over deep blue, with the quay's edges |
| `Tiles/crypt.png` | `Tilemap` | 16x16 tiles, eight to a row, in `TilesetCatalog` order: cold slate floors in blocks of 2x2 tiles, slabs of every size, worn ones and ones in a wall's shadow, steps, the tops of walls, the two rows of a wall's face (with a burial niche, and with a carving), rubble, bones and cobwebs |
| `Tiles/forest.png` | `Tilemap` | The same layout: grass, dead leaves, the road (its edges where it meets the rest), the undergrowth under the deep wood, and what lies on them: moss, tufts, ferns, flowers, pebbles, roots, ruts and litter |
| `Props/*.png` | `PropSprite` | Standing on the middle of the bottom edge. The old road's wood: oaks (green, pale, amber, and deepened), firs, bushes, a stump, a snag, a dead tree, ferns, long grass, mossy rocks, a log, a fallen tree, toadstools, the campfire and a bedroll. The barrow's: urns, a sarcophagus, the iron sconce a torch stands in, and the things you use, each a strip of two pictures (shut then open): a chest, a double door and a stone door in their frames, a lever, and the waypoint stone with its mark dark then lit. The sign is one picture |
| Questeria's `Props/*.png` | `PropSprite` | The same, cut from my pictures: the palace, the temple, the guildhall, the city gate, the fountain, statues, lamps, banners, benches, market stalls, trees, bushes and flowers, the quest board, and the waypoint crystal, dim then lit. The houses: the four I painted whole, and townhouses, cottages, turret houses, a bakery, a chapel, a forge, a Watch post, warehouses, the Gilded Flagon with its sign, the Rusty Anvil and a fishmonger's, put together by `cut_supplied.py` from the walls, roofs, doors and shop fronts on my sheets, each in blue, pink, teal and plum roofs. The docks' fish stalls, net racks and harbour bell, crates, sacks and barrels, wells, pumps, washing lines, carts and planters |
| `People/*.png` | `WalkerSprite` | Seventeen townsfolk, cut from my pictures, in the first five columns of the 10x4 layout: standing, and walking |
| `map-marks.png` | `ZoneMapScreen`, `ExploreScreen` | 7 marks, 12x12, at 3x on the map screen: a place, a place you only know of, the ring round where you are, a waypoint stone, the brackets round where you're going, you on the local map, and one dot of a road. The map you're walking borrows the dot and the ring, at their own size, for the path you'd take |
| `Paperdoll/*.png` | `Paperdoll` | The body, six hair styles (one of them bald), five faces, four beards, the skirt, the cape and the two tails in halves, four kinds of ears, two of horns, and the hats; and the gear: nine body armours, three helms, gloves, gauntlets, two kinds of boots, two shields and thirteen weapons. All in the youth's 10x4 layout and his key shades (metal and leather in keys of their own), so they stack and recolour. `Stout/` and `Small/` are every one of them again for the shorter builds. The cards on the strip and the faces in conversations are the head and shoulders of the standing frame, blown up |
| `character-palettes.png` | `CharacterPalettes` | Five pixels by forty-five: one ramp per row, dark to light. Skins, hair, eyes, shirts, vests, legs and cloaks, each block with its key first |
| `gear-palettes.png` | `Paperdoll` | Five pixels by ten: metal's keys and its four tints, then leather's keys and its four, one ramp a row, dark to light |
| `cursors.png` | `CursorSprite` | 5 cursors, 16x16, at 2x: an arrow, a ring to walk, a hand to use something, a speech bubble to talk, a crossed circle for no way |
| `keycaps.png` | `KeyCaps` | 4 frames, 14x14, at 3x: an ivory key cap with a deeper lip along its bottom, nine-sliced so it's as wide as what's written on it, then the mouse with its left button, its right button and its wheel lit in torchlight |
| `dialogue-icons.png` | `DialogueScreen` | 6 icons, 12x12, at 2x, beside an option: a die for a check, a heart for a flirt, a lock, and the three voices |
| `markers.png` | `ExploreScreen`, `ZoneMapScreen` | Two rows of 14x14 cells at 3x, gold then red: the "!" that bobs over someone with work for you (gold) or an event (red), and the arrows at the edge of the screen pointing to one that's off it, which the game turns a quarter at a time for every direction |
| `Paintings/*.png` | `PaintingSprite` | The story's paintings, each three to five layers on the title's 416x277 canvas at 4x, swaying against each other like the title: the pier in the storm the day you met, the oath under the red moon, and Questeria at dawn from the ship's rail. `weather.png`, `bolts.png`, `sigil.png`, `glint.png` and `lantern.png` are what moves over them: the rain and the lightning, the Soul-Bond's glow, glints on the water, and the pier's lantern swinging. Under the red moon the two figures are your own sheet and the Soul-Bound's, drawn black with the moon on their edges |
| `Tiles/ship.png` | `Tilemap` | The Gull's Wager's deck: planks, the rails, the hull, the hatch, the bow and the stern in blocks, the bowsprit, a lantern on the rail that swings, and the night sea, animated |
| `Rooms/<zone>/<room>/*.png` | `RoomScene`, the zone map | Each room's sky (4x), far layer (2x), mid layer (3x) and foreground (4x), at art resolution and drawn at their scales with their parallax, and its 200x113 thumbnail. `doors.txt` beside them is where each door's doorway is painted, in canvas pixels |
| `Tiles/interior.png` | `Tilemap` | Inside the buildings: wood and stone floors, a rug with its edges, the back wall's face (plaster over wainscoting, two tiles tall) and the tops of the walls |
| The ship's and the inside's `Props/*.png` | `PropSprite` | The ship you arrive on, seven frames: under full sail with its wake, the sail hauling up, furled, and moored with its lines ashore. Its near rail with the gangplank, six frames of the gate opening and the plank sliding out. The rowing boat, the mast with its sails furled and the crow's nest, the lanterns, the wheel and coils of rope. Inside: the bar, tables, chairs and stools, the hearth and candle stands (lit), kegs, a wardrobe, bookshelves, a desk, an anvil and tool racks, tapestries and plants, cut from my props sheet, and a bed |

Text is drawn with `spectral-title` (Spectral Light, 92pt), `spectral-ui` (Spectral Medium, 26pt)
and `spectral-detail` (Spectral Light, 17pt), letterspaced, the same faces as The Black Box. The
`.spritefont` files name their TTF by relative path, so the build always uses the font shipped in
`Content/Spectral/` rather than whatever is installed on the machine.

## Regenerating the art

Every PNG in `Content/` is made by a script:

```
python tools/generate_ui.py           # button.png, panel.png, map-marks.png, cursors.png, dialogue-icons.png, markers.png and keycaps.png
python tools/generate_world.py        # the crypt tileset (and the forest one, which no map uses now), their props (cut or drawn), the torch flame, glow.png and mote.png
python tools/generate_harbour.py      # Questeria's and the harbour's tiles, the ship and its deck, its gangplank, mast, lanterns, wheel and rope, and the rowing boat
python tools/generate_interiors.py    # the inside of the buildings: the interior tileset, the bed, and the taverns' tables and booth
python tools/generate_meadow.py       # the meadow tileset and the farmland's props, the refugee camp, and the east gate from outside
python tools/generate_mirewood.py     # the Mirewood's and the quarry's tilesets, the black-veined trees, the roots, the crystal and the rest
python tools/generate_keep.py         # Sunspire Keep's tileset, the throne and the court's props
python tools/generate_vault.py        # the Old Crown Vault's tileset, its doors, shelves, gold, traps and the Sunheart's pedestal
python tools/generate_creatures.py    # the creatures' sheets: gloomfangs, the Thornmother, the Hollowed and the vault's constructs
python tools/generate_paintings.py    # the story's paintings and what moves over them, out of the title's pictures
python tools/generate_items.py        # the item icons and the inventory's slots
python tools/generate_combat_art.py   # the fight's ability and condition icons, the pieces of its screen, and the real-time HUD's skill and aura icons
python tools/cut_supplied.py          # the title, Questeria's and the harbour's props, the houses, the furniture and the townsfolk, out of tools/source/
python tools/cut_supplied.py buildings  # just the houses (or name props to cut just those)
python tools/generate_paperdoll.py    # the paperdoll sheets and the gear for all three builds, character-palettes.png and gear-palettes.png, off the youth (after cut_supplied.py's people)
python tools/rooms/generate_room_placeholders.py   # every room's sky, far, mid and foreground layers, its thumbnail and doors.txt, out of the props, tiles and title pictures above (last)
python tools/rooms/generate_room_placeholders.py --preview <folder>   # and what each room looks like in the game, outside Content
python tools/rooms/make_room_template.py   # the 1536x1024 guide each panel of each room is painted over, in shots/art/room-templates/
python tools/rooms/cut_room.py             # my paintings of the rooms, in tools/source/rooms/, cut into them (Rooms above)
python tools/rooms/cut_tiles.py            # my floor textures, in tools/source/tiles/, cut into the tilesets
python tools/rig/clips.py                  # every clip the figures play, into Content/Characters/Parts/clips.json (the fight's from tools/rig/combat_clips.py)
python tools/rig/clips.py --check          # fails if a foot goes through the floor, a joint bends backward, a clip the fight needs is missing, or two skills' clips look alike
python tools/rig/clip_strip.py <out.png> <clip> ... [--gif <out.gif>]   # a clip's frames on a dressed figure, a contact sheet to check it by
```

A clip's keys can each say how they ease to the next (`[t, v, "back"]`: `smooth`, `in`, `out`, `back`,
which overshoots and settles, `hold` and `linear`), so a blow can ease into its coil, snap through
the contact in two or three frames and overshoot after it; the maths is in `clips.eased()` and
the game's `ClipTrack` matches it. A fighting clip also carries its `blend` in, the `trail`
windows its weapon smears over, and events: `hit` on the frame each blow lands, `release` when
something leaves the hand, `recover` when the next action may cut it, `telegraph`, `land` and `fx`.

They need nothing but CPython. The PNG writer is in `tools/pixels.py` (copied from The Black Box's
`generate_assets.py`), and every drawing is seeded, so running a script again produces the same
bytes; only what you changed changes. Everything on a map is drawn as if a torch were right on it,
lit from the top left, and the game does the dark itself. Tiles that lie next to each other (the
leaves, the floors) only vary in ways that meet cleanly at the edges. The trees on the old road are
props, cut from my trees sheet and some of them recoloured deeper, so the wood's crowns overlap and
hang over the road the way the city's trees do.

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

- The ground isn't cut: my ground sheet painted its cobbles on the diagonal, which reads as a
  different camera to everything standing on it, so `generate_harbour.py` draws the ground straight
  on in the sheet's colours, and the cut props stand on it.
- Everything standing in Questeria is a prop, buildings included. Past the five houses I painted
  whole, the rest are put together from the parts on my houses and grand sheets (walls with their
  windows, roofs, doors, shop fronts, signs), laid out back to front and then sampled once, so the
  joins don't show. Their roofs are recoloured for variety; nothing's flipped, since that would put
  the light on the wrong side.
- The townsfolk come off three sheets: twenty people drawn once each way, six walking each way, and
  one more standing and walking. Three of the twenty turn into someone else when they turn round, so
  they're left out.
- The paintings are built from the title's pictures: `generate_paintings.py` samples the sky, the
  clouds, the mountains and the city and recolours them with gradient maps (each pixel's brightness
  looked up on a new ramp, with some of its own colour kept), so all of the detail survives in the
  new light: sunrise, a crimson night, a slate storm. What isn't in them (the ship's rail, the waves,
  the pier) is drawn to match.
- The furniture inside is cut from my props sheet too, and so are the old road's trees and bushes
  and the barrow's urns and chest. The ground under them, the ship, its deck and the water are
  drawn to sit beside the cut props. The party and the story's people are the youth, taken apart
  and dressed again by the paperdoll.

`python tools/check_assets.py` checks that `ASSETS.md`, `Content/Content.mgcb` and the files on
disk agree, and fails with a list if they don't. I run it before every commit.

## Still to come

In the order they're being built:

1. ~~The screens around the game, the sound, the saves and the proof run.~~
2. ~~One map and one walker: the map file format, collision, the camera, walking, the dark with lights in it.~~
3. ~~The three maps joined up: exits, triggers, chests, doors, levers, the map screen and fast travel.~~
4. ~~The party: four members, switching between them, followers, click-to-move with A*.~~
5. ~~The paperdoll and gear: armour you can see, the inventory and the character sheet.~~
6. Character creation. ~~Your name, people, class and look, and the Soul-Bound's~~; point buy is still to come.
7. Dialogue with dice rolls, approval, the quest and the journal. ~~Conversations, choices, checks
   on the dice, clocks, approval~~; the journal is still to come.
8. ~~Combat, and `--simulate`.~~
9. Resting, full saving, and the polish. ~~A long rest~~ (the night at sea, the camp in the
   Mirewood, and a room at the Gilded Flagon, all back to full), and ~~hit points coming back
   between fights~~ (1% every 2 s as you go about, 5% a second resting on R); the camp screen is
   still to come.

The story goes in alongside them, in its own order:

1. ~~Valen out and the Soul-Bound in: their origin, their pass on the creation screen, pronouns and
   the tokens, and the meters in the save.~~
2. ~~The conversations the story needs, and the Prologue and Act 1's first chapter rebuilt on
   them. Buildings to go into.~~
3. ~~Act 1, chapters 2 to 4: the Mirewood and the quarry, Thornbridge, the Old Crown Vault, Sunspire
   Keep and the Rusty Anvil, and the companions joining.~~
4. Act 1, chapters 5 to 8, to the end of Act 1. This is next: chapter 4 ends with Brannock in the
   party and the caravan for Greywatch leaving from the east gate at dawn.
5. Act 2, with the eclipse.
6. Act 3.
7. ~~Every fight card swapped for its fight, as combat comes in.~~

The Soul-Bound's remarks turning colder as their ambition grows come with Act 2. The side quests'
hooks are in, and the quests behind them come with the chapters they belong to.
The companions' personal quests have their hooks set in this stretch (see COMPANIONS.md), and
the quests themselves come with the chapters they belong to.

The maps could go through the content pipeline with an importer of their own (that's chapter 11 of
the textbook); for now they're copied as text and read at startup, which is enough to catch mistakes.

## Where things are

- `LegendQuestGame.cs` reads the maps, sets up the window, the screen manager and the sound, and puts the title up. `LegendQuestGame.Proof.cs` takes the proof shots.
- `StateManagement/` is the game state management tutorial: `GameScreen`, `ScreenManager`, `MenuScreen` and `MenuEntry`, `LoadingScreen`, and the input classes from the advanced input tutorial. `InputState` is the only thing that reads the keyboard, mouse or pads.
- `Screens/` is every screen. The explore screen is in partials: `ExploreScreen.cs` for walking and drawing, `ExploreScreen.Party.cs` for the party and the followers, `ExploreScreen.Pointer.cs` for the mouse, `ExploreScreen.World.cs` for using things, triggers, exits, doors in and out and travel, `ExploreScreen.Story.cs` for conversations, scenes, the townsfolk strolling and the ship coming in, `ExploreScreen.Banter.cs` for banter, chatter and the bubbles, `ExploreScreen.Markers.cs` for the markers, `ExploreScreen.Camp.cs` for whoever's waiting at camp and swapping places with them, `ExploreScreen.Tutorial.cs` for the tutorial's tips, `ExploreScreen.Items.cs` for I and C, the shops and what goes in the bag, `ExploreScreen.Stage.cs` for the map as a conversation's stage, where its cues play (`ICutsceneStage`, played by `CutsceneDirector`), and `ExploreScreen.Proof.cs`, `ExploreScreen.ProofBeats.cs` and `ExploreScreen.ProofChapters.cs` for its part of the proof schedule.
- `World/` is the places: `World/Zones/` (`ZoneLoader` and `ZoneCatalog` to read the zones, `ZoneDefinition`, `RoomDefinition` and the `Room*` records), the `Map*` records the rooms reuse for what's in one (`MapPresence` for `when` and `unless`), `World/Rooms/` to draw, walk and fight in a room, `Tilemap` to draw a room's tile strip, `LightMap` for the dark, `WorldState` for the rules of using things, `NavGrid`, `Pathfinder` and `BinaryHeap` for A*, `Camera2D`, and the tileset and prop catalogues.
- `Characters/` is the walking sprite and the shape of its frames (`WalkerSprite` and `WalkerShape`), the party (`PartyMember`, `Origin` and `Origins`), people on maps (`Npc`, and their sheets in `Townsfolk`), the beasts (`Creature` and `Creatures`), and how everyone looks (`CharacterLook`, `LookOptions`, `Paperdoll` and `CharacterPalettes`), and `LegacyClips` for what a scene's clips come to on a sheet.
- `Dialogue/` is conversations: the script's parts (`DialogueScript`, `DialogueNode`, `DialogueOption`, `DialogueCheck`, `DialogueCondition`, `DialogueEffect`), `DialogueRunner` to play one, `TextTokens` for the words in braces, `DialogueCatalog` for every one of them, `StageCue` and `Expressions` for the cues and faces the story's scripts carry (`StageDirection`, `CueSpot` and `CueFacing` for what a cue's words mean), the scripts compiled from `Story/` in `Dialogue/Scripts/Generated/`, and the hand-written ones in `Dialogue/Scripts/`, a file per chapter.
- `Story/` is the story's tables (`Meters`, `KeyItems`, `Paintings`, `Tints`, `Cast`, the quest board's `Postings`, and `Banters`) and the plan the rest of the game is built from: the build plan, the writing guide, the scripts, the specs for rooms, combat, classes and art, and the generated registry (`Story/README.md` says what each is), with their two tools in `Story/tools/`.
- `Tutorial/` is the tutorial's tips, which draw nothing: `TutorialTip`, every one of them in `TutorialCatalog`, and `Tutorials` for which a game has seen and which are waiting. `TutorialPanel` and `KeyCaps` in `Components/` show them.
- `Rules/` is the 5e parts that draw nothing: `AbilityScores`, `AbilityCheck`, `Dice`, `DiceExpression` and `RollResult`, `AttackMath`, `ArmourClass`, and the classes in `ClassCatalog`.
- `Combat/` is the fights, which draw nothing: the real-time engine in `Combat/Realtime/` (`Battle` and its partials, `Fighter`, `BattleEvent`, `BattleCommand`, `BattleArena`, `BattleRoster`, `BattleRules`, the brains, and `BattleSimulator` for `--simulate`), the kits (`SkillCatalog`), the catalogs (`EnemyCatalog`, `EncounterCatalog`, with the encounters' `EncounterGuest`s, `EncounterSupport`s and `EncounterWave`s), a fight's starting foes (`FightRoster`, `Slot`), the difficulty (`DifficultyRules`), class ranks (`ClassRanks`) and a party member's numbers (`CombatStats`).
- `Items/` is everything that can be carried, worn or sold: `ItemDefinition` and `ItemCatalog`, the bag (`Inventory` over the save's `ItemStack`s), the shops (`ShopDefinition`, `ShopCatalog`) and what foes drop (`LootTable`, `LootResult`, `LootTableCatalog`). The inventory's and the shops' rules are here too, drawing nothing: `Wardrobe`, `Trade`, `GearComparison`, `ItemReadout` and `BagOrder`.
- `Components/` is every screen element, the torches, the toasts, the tooltip, the party's cards, the cursor, the paintings, the quest board, the chapters' card, the tutorial's card and key caps, the 3D d20 (`DieModel`) and the panel it's thrown on (`DiceTable`), and the inventory's icons, grids, item cards, faces, tiles and character sheet, each with `LoadContent`, `Update` and `Draw` the way the tutorials do it. `Components/Realtime/` is the real-time fight's HUD (see [The real-time HUD](#the-real-time-hud)).
- `Collisions/` is the collision tutorial's rectangle and helper.
- `Particles/` is the particle engine, and the title's petals and motes built on it.
- `Audio/` is the `AudioManager`. `Save/` is the save file, the slots, and the settings.
- `Enums/` is every enum in the game, one file each.
- `Layers.cs` is the draw order, `Palette.cs` is the colours more than one screen uses, `Text.cs` draws a line of text with its shadow, and `Blends.cs` is the additive blend for anything that glows.
- `Content/Zones/` is the zone files. `tools/` is the art and sound generators, the cutting script and the asset check, `tools/source/` is the pictures it cuts, and `tools/rooms/` is the room tools, with the old tile maps they were converted from in `tools/rooms/legacy/`.

## Assets

Every picture, song and sound in the game is made for it: drawn or synthesised by the scripts in
`tools/`, or cut by one of them from pictures I made. The only outside work is the Spectral font, the
MonoGame template files, a few of 5e's rules from the SRD 5.1 (credited in there word for word as its
licence asks), and the instrument recordings the music is played on and a few combat sounds are
layered with: two sample libraries by Versilian Studios (VSCO 2 Community Edition and VCSL),
released under CC0 1.0, which `tools/score/fetch_samples.py` fetches to a cache outside the repo and
which are never committed or shipped. See
[ASSETS.md](ASSETS.md) for every file.

## Author's Note

