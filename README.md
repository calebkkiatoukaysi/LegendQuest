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
conversations the story needs (choices, dice, approval), the Prologue, Act 1's first four chapters,
the companions who join in them, and buildings you can go into. Outside the walls the country is
bright, green farmland now, the way a capital's fields should look at noon. [The story](#the-story)
says what's in, and [COMPANIONS.md](COMPANIONS.md) is the companions' bible.

## For the assignment

The project's requirements, and where each one is met:

- **A tilemap rendered for some elements of the game.** Every room's floor is a tilemap. The room
  file in `Content/Zones/` lays out the rows of ground under the floor band (and an interior's back
  wall) as characters, each a 16x16 tile in the zone's tileset (`World/TilesetCatalog.cs`, one sheet a
  set in `Content/Tiles/`). `World/Zones/ZoneLoader.cs` reads them, and `World/Tilemap.cs` draws them
  at 4x under everyone: a variant picked per cell, edges where one kind of ground meets another, and
  animated water. The footsteps read the tile under your feet. All 19 zones, 37 rooms, have one;
  [Rooms](#rooms) has the format and the layers.
- **Serialization of game state to create and load a save file.** `Save/SaveSystem.cs` writes the
  whole game, `Save/SaveData.cs`, to a JSON file with `System.Text.Json`, and reads it back: the map
  and where everyone's standing, the party and what they're wearing, the bag, gold, the story's flags
  and meters, the chests opened and the maps seen. There are three slots, a quicksave and an
  autosave, written safely (a temporary file, then swapped in, the old one kept as a backup) and
  upgraded from older versions; a broken file shows as UNREADABLE rather than crashing. NEW GAME
  writes one, and LOAD and CONTINUE read one. [Saving](#saving) has the details.

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
```

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
run to itself (`ExploreScreen.ProofFights.cs`), EnemyMind plays the party's turns as well as the
foes' at three times the speed, and there's a shot of each fight's opening and its end. The
tutorial's first turn is played by hand, for the movement preview past the budget, the target's
tooltip, the attack and the log. The zones' dice are seeded under the run, so it fights the same way
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
| Walking a room | **A** and **D** or **left** and **right** to walk along it, **W** and **S** or **up** and **down** to walk deeper or nearer on the floor, **E**, **Space** or **Enter** to use or talk to what you're next to or go through the door you're at, **M** for the zone map, **Esc** to pause. Walk off either end, or into a stair, to go on. Everyone faces left or right |
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
| In a fight | **Click** a tile to walk there, a foe to attack it (walking into reach first if it's in range of a walk), or a friend who's down to help them up. **1** to **0**, then **-** and **=**, or a **click** picks a hotbar slot: one used on yourself happens at once, anything else waits for a target. **Tab** goes through the targets and **Enter** uses it on the one picked out. **Right click** or **Esc** puts it back. **WASD** or the **arrows** step a tile, **Space**, **End** or END TURN ends the turn, **L** shows and hides the log (the **wheel** scrolls it with the mouse on it), and **Esc** with nothing picked pauses. Pointing at anything says what it is and what a click would do |
| The end of a fight | **Enter**, **Space** or a **click** on the panel goes on to what happens next |
| The inventory | **I** for the bag, **C** for the character sheet, or **Esc** and then INVENTORY. **Q** and **E** (or **F1** to **F4**, or a **click** on a face) pick who you're looking at, **Tab** turns between the bag and the sheet, and **I**, **C** or **Esc** closes it. On a pad, the shoulders pick who, Y turns the page, A uses and B closes |
| In the inventory | The **arrows** move round the slots, the bag and the key items. **Enter** or a **click** puts on what's in the bag (whatever was there goes back in it), drinks a potion, or takes off what's in a slot; on an empty slot it finds something in the bag for it. The **mouse wheel** scrolls the bag. On the sheet, **up** and **down** pick an ability to read |
| A shop | Talk to the keeper. **Enter** or a **click** buys one or sells one, and with **Shift** held it sells the whole stack. **Tab** crosses the counter, **Q** and **E** pick who the cards measure against, **Esc** leaves |

A gamepad works anywhere the keyboard does: the d-pad or left stick, A to use, B to back out, Y for
the zone map, the right shoulder for the next in the party, the left shoulder held for what Alt
shows, X to put a tip away, and Start.

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
| `DiceRollOverlay` | Popup, over a conversation. A check: the d20 with the DC and every modifier, a press to roll, the tumble, then the total and SUCCESS or FAILURE |
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
  rail: the first fight, and a guided one, where a wraith at a time teaches you to move, attack, use
  an ability, fill and spend the Soul-Link, and end your turn. Then Questeria at dawn.
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
`Content/Rooms/<zone>/<room>/`, each at its own scale so its pixels stay square:

| Layer | Screen pixels to an art pixel | Parallax | What's in it |
| --- | --- | --- | --- |
| `sky.png` | 4 | 0.1 | The title's blue going pink (twilight in the Mirewood, night at sea, the dark underground), two or three of its clouds, and over Questeria one of its floating islands |
| `far.png` | 2 | 0.3 | The skyline: the palace, the temple, rooftops, trees, the city wall, the windmill, the title's city across the water and its mountains behind everything, cooled toward the sky with a few windows lit |
| the tile strip | 4 | 1 | The floor, drawn by `Tilemap` from the room file |
| `mid.png` | 3 | 1 | The buildings, each on its door, the street dressed the way the maps were (trees and garden walls, lamps, banners, stalls, crates, strings of lanterns, washing lines), or indoors the wall: beams and plaster, the keep's marble and gallery, the crypt's arches |
| `fore.png` | 4 | 1.15 | What's between you and the room, dark, never more than 12% of the floor band |

An interior has no sky or far layer and no parallax. In a dark room the dark (`LightMap`, with the
lights cut out of it) goes over the floor, the mid layer, the people and the foreground, but not the
sky or the far layer: a painted night sky already has its light. `doors.txt` says where each door's
doorway is painted, which is what the door's outline and badge go round; what you click is the
door's bigger box. `thumb.png` is the room for the zone map. The life loops are the title's clouds,
its motes, sparkles and petals, the torch flame and its glow, and gulls and flags drawn a pixel at
a time.

The layers are painted by `tools/rooms/generate_room_placeholders.py`, out of the game's own art,
until the rooms are painted by hand. It moves each building so its painted door is on its door
line, fills the street in, and then gives everything a pass of detail: two-tone shadows where it
stands, shadows under the eaves, light along the roofs' top and left edges (it's lit from the top
left, and nothing's flipped), window boxes, posters, and moss and cracks, the moss cut out of the
room's own tileset. It's seeded, so a rerun writes the same bytes. `--preview` writes what the game
will show into a folder outside `Content/`, with the tile strip, the room's props and two townsfolk
at their real size. For painting a room for real, `tools/rooms/make_room_template.py` writes its
guide (the bands, the floor, every door's box, where the foreground may go), and
`tools/rooms/cut_room.py` cuts the painting into the layers.

## The party

Up to four: you, and whoever's with you. A new game starts you with the Soul-Bound, and the story
recruits the rest: Sera in the Mirewood, Mira at Thornbridge, Kael in the Old Crown Vault and
Brannock in the Rusty Anvil, each with their people, class and look. When there are four of you
already, anyone else who joins waits at camp, which is the Gilded Flagon.

| File | What it is |
| --- | --- |
| `PartyMember` | Someone in the party, as the save keeps them: name, pronouns, class, level, experience, ability scores, hit points, what they're wearing (item ids by slot, changed with `Equip`, `Unequip` and `PutOn`), what they've spent since they last rested, look, and where they're standing. The most hit points they can have includes anything worn that adds to it |
| `Origins` | Everyone who can be in the party and how they start: class, scores (5e's standard array, 15 14 13 12 10 8, where their class recommends), look and kit. A companion the story arms sets out in their own (Sera's shortbow, Kael's twin daggers, Brannock's round shield); everyone else in their class's. You and the Soul-Bound start from theirs and the creation screen does the rest |
| `ClassCatalog` | The eight classes: the SRD's names in the code and the story's on screen (a Fighter is a Blademaster, a Rogue a Shadowstep, a Cleric a Lightbearer, a Wizard an Arcanist, a Paladin a Warden, a Druid a Wildcaller, a Warlock a Hexblade and a Bard a Skald), each with its role, its hit die, where its scores go, the armour, shields and weapons it's trained with (every class has the simple weapons), the ability it casts with, its speed, and its starting kit: its weapon, shield and armour, and a travelling cloak and boots. A kit it couldn't wear stops the game at startup |
| `AbilityCheck` | A check: a d20 plus a modifier against a DC, with or without advantage, and a natural 20 or 1 deciding it whatever the total |
| `AbilityScores` | The six scores, and 5e's modifier: half of (score minus 10), rounded down |
| `DiceExpression` | Dice written 5e's way (`2d6+3`, `1d8`, `3`), parsed once and rolled into a `RollResult` with every die and every modifier by where it came from; a critical rolls the dice twice and the bonus once. `Dice` rolls one die |
| `AttackMath` | The d20 with advantage or disadvantage, whether a roll hits (a natural 20 always, a natural 1 never), the exact chance it will for the tooltip, and how several advantages and disadvantages cancel |
| `ArmourClass` | AC from what's worn: the armour's base, Dexterity up to its cap (all for light, +2 for medium, none for heavy), and every worn bonus, a shield's +2 among them. 10 plus Dexterity in nothing |

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
their class's abilities out of `AbilityCatalog`, each with what it costs and what's left of
anything it spends. Picking one reads out what it does under the list.

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

Fights are turn-based 5e from the SRD 5.1, on the map you're on, a tile at a time (a tile is 1.5 m,
and a diagonal step costs the same as a straight one). `Combat/` is the rules and draws nothing:
`CombatEngine` takes the encounter, who's in it and where, the ground (`ICombatGrid`) and a
`Random`, and every command (`Begin`, `Move`, `Use`, `EndTurn`, `RunAiTurn`) hands back the
`CombatEvent`s it caused, in order, each with its whole log line ("SERA attacks GLOOMFANG: 14 + 5 =
19 vs AC 13 - HIT. 1d8+3 = 9 piercing."), for the screen to play out at its own pace, the way The
Black Box's table did. Nothing in it waits on time.

- **Turns.** Initiative is d20 + Dexterity (ties to the higher Dexterity, then the party). A turn
  has 9 m of movement (less in heavy armour you aren't strong enough for, 3 m less when Slowed, none
  when Restrained), an action, a bonus action and a reaction.
- **Moving.** Foes block you, allies you can walk through but not stop on. Leaving a foe's reach
  without Disengaging gives it an opportunity attack, which spends its reaction.
- **Attacks.** Advantage from Help, a restrained or prone target, a Downed one in reach, and Pack
  Tactics; disadvantage from Dodge, being restrained, prone or mocked, armour you aren't trained
  for, a ranged attack with a foe beside you, and a Warden's raised shield. A natural 20 doubles the
  dice, and so does a hit on someone Downed from beside them.
- **Kits.** Every class's level-1 kit is in `AbilityCatalog`, with the actions everyone has (Attack,
  Off-hand, Dash, Disengage, Dodge, Help, Drink Potion, Drink Elixir, Alchemist's Fire). What's spent
  goes in `PartyMember.Spent` by `ResourceCatalog`'s ids (`spell-slot-1`, `pact-slot`,
  `second-wind`, `lay-on-hands`, `bardic-inspiration`), and `ResourceCatalog.Recharge` gives it back
  on a short or long rest. Hex and Entangle need concentration, which damage can break.
- **Going down.** A party member at 0 is Downed and rolls death saves on their turn; a hit on them is
  a failure (two for a critical), a 20 gets them up on 1 HP, and healing or Help gets them up any time.
  Foes at 0 die. A fight to yield (Kael's duel) or a brawl (the Red Gulls) knocks people out
  instead; the duel ends when Kael gives up.
- **The Soul-Link.** When you and the Soul-Bound both hit the same foe between your turns it
  charges; at two charges either of you can spend it on the Soul-Link Strike, both weapons and 1d8
  radiant on a sure hit, and the Soul-Bound says their P-05 lines as bark events.
- **Waves.** An encounter can bring more foes on a later round, sooner if everyone before them is
  beaten, and guests who fight on your side (Mira at Thornbridge, Kael against the sappers). It can
  also sit party members out (`SitsOut`: Sera, while she draws the rot out of the crystal).
- **The AI.** `EnemyMind` floods out everywhere the one whose turn it is could walk, scores every
  ability on everyone from there (hit chance times damage, the chance of dropping them, how hurt and
  how dangerous they are, what the walk risks in opportunity attacks, staying out of reach for
  anyone who fights from range), does the best thing, and goes again. Getting someone up is worth
  less each time they've already gone down that fight, so nobody spends every turn hauling up a
  friend who's knocked straight back down. Every weight is a named constant. It has no randomness
  of its own, so a seed replays a fight exactly.

| File | What it is |
| --- | --- |
| `CombatEngine` | The rules, in partials: turns and waves, movement, abilities, attacks, health, and the odds for the tooltip |
| `Combatant`, `TurnBudget`, `ActiveCondition` | Someone in a fight, what's left of their turn, and what's on them |
| `AbilityCatalog`, `ResourceCatalog`, `CombatStats` | What everyone can do, what it spends, and a party member's numbers from their kit |
| `EnemyCatalog`, `EncounterCatalog` | The foes' stat blocks, and every fight by the story's id |
| `EnemyMind`, `CombatSimulator` | The AI, and `--simulate` |
| `ShownFight`, `TurnPlan` | The fight as far as the screen's shown it, and what a click on your turn would do |
| `World/Rooms/RoomCombatGrid` | The room's floor a fight's on, as the engine sees it, until M4's rows |
| `Screens/ExploreScreen.Combat*` | The fight on the map: starting and ending it, playing it out, its cues, your turn, the cards, drawing it, its tips, and the proof run's hooks |
| `Components/CombatHotbar`, `BudgetBar`, `InitiativeBar`, `UnitFrame`, `SoulLinkSigil` | The hotbar and END TURN, what's left of the turn, the turn order, the frame of whoever's turn it is, and the Soul-Link's meter |
| `Components/CombatLog`, `CombatCard`, `TurnBanner`, `DamagePopups`, `CombatResult` | The log, the card by whatever's pointed at, the banner when a turn starts, the numbers that pop, and the panel at the end |
| `Components/CombatEffects`, `ScreenShake`, `CombatStyle`, `CombatArt` | What's thrown about (sparks, motes, what flies), the shake, the colours and sounds of each kind of damage, and the fight's sheets |

```
dotnet run -- --simulate a1-c1-01-gulls 2000   # fight it 2000 times headless, AI on both sides
dotnet run -- --simulate all                   # every encounter, 1000 times each
```

`--simulate` opens no window. The party is the default level-1 one (you as a Blademaster, the
Soul-Bound as a Shadowstep, and the companions who'd be there, with the two potions you start with)
on open ground a move away from the foes, and it prints the win rate, the rounds, and each member's
hit points left and how often they went down. A fight that runs into the 30-round cap is counted
as unfinished, and `StallWatch` says why from its last five rounds: a revive treadmill, a
stand-off, one side unable to hurt the other, or just a slow fight. The targets are constants in `CombatSimulator`: the
tutorial 98% or more, story fights 80 to 92%, bosses 65 to 80%. These are the simulator's own
numbers, `--simulate all 1000` with its seed, after tuning; nothing ran into the round cap.

| Encounter | Target | Won | Rounds | Who goes down most |
| --- | --- | --- | --- | --- |
| `p-05-wraiths` | 98%+ | 100% | 4.0 | nobody |
| `a1-c1-01-gulls` | 80-92% | 88.3% | 6.1 | the Soul-Bound, 71% |
| `a1-c2-01-gloomfangs` | 80-92% | 89.6% | 4.8 | the Soul-Bound, 49% |
| `a1-c2-02-thornmother` | 65-80% | 78.4% | 8.8 | the Soul-Bound, 98% |
| `a1-c2-02-guard-wave` | 80-92% | 83.9% | 6.0 | the Soul-Bound, 61% |
| `a1-c3-02-raid` | 80-92% | 81.4% | 8.3 | the Soul-Bound, 78% |
| `a1-c4-02-kael` | 80-92% | 88.3% | 6.2 | the Soul-Bound, 91% |
| `a1-c4-02-sappers` | 80-92% | 81.3% | 5.1 | the Soul-Bound, 81% |

Getting there took two things the first runs showed up. A fighter in heavy armour kept picking the
same Downed friend up every turn and never hit back, so a handful of fights never ended; picking
someone up is now worth less each time they've gone down in the fight. And the Soul-Bound, a
Shadowstep in leather, died in nearly a quarter of the Red Gulls fights, which the story can't
have, so that fight is a brawl with cudgels now: nobody dies in it, and Grell is alive to beg after.
In the game nobody in the party dies for good in any fight: whoever's fallen gets up on 1 HP once
it's won.

How I'd tune one that's off: hit points first, since they change how long a fight lasts without
changing what each turn feels like; then the to-hit, which changes how often anyone goes down; damage
last, because a level-1 party has so few hit points that one more point a hit is a lot. A boss gets
its AC and hit points moved, not its attacks, since they're what makes it frightening. Waves are the
last thing to move, since they change the shape of the fight. `EnemyMind`'s weights are how foes
behave, not how hard they are, so they stay put unless a foe's doing something silly. The leather
Shadowstep and Wildcaller are the party's soft spots, so most fights are tuned round how quickly
they go down.

What the story asks of a fight comes before any of that. The wraiths at sea take two hits each and
only chill you, so the Soul-Link fills against them and nobody goes down while they learn. The Red
Gulls are a brawl, so nobody dies on the docks and Grell's alive to beg. The Thornmother frightens
by lasting (her hit points and a thorn-hard hide) and by knocking you flat (Trip), not by hitting
harder. The guard wave comes in three, the pack, the courier's dead escort, then a straggler, while
Sera sits it out at the crystal. The sappers are diggers, a point worse with a pick than a soldier
would be.

### On screen

A fight happens on the map you're on. A conversation that ends with `DialogueEffect.Fight` starts
it when it's over, the way a shop or a journey waits for the talking to end, and the game's
autosaved first, so a lost fight can be fought again. Everyone steps onto the middle of their tile
(the nearest free one, if theirs is taken or under a shelf), foes who were standing about turn,
foes who turn up fade in where they arrive, guests join you at your side, and anyone the encounter
`SitsOut` stays where they are. The battle's song comes in, the fight's name goes across the
screen, and the initiative dice fill the turn order in along the top a portrait at a time.

The engine works each command out whole, and `ExploreScreen.CombatPlayback` shows what it caused a
beat at a time, the way The Black Box's table read its round a line at a time. `ShownFight` keeps
up with what's been shown, so the bars, the turn order and the death saves never get ahead of the
picture. `ExploreScreen.CombatCues` is what each beat looks and sounds like:

- **A step** is a slide from tile to tile, walking.
- **An attack** winds up, then lunges, or lets fly (an arrow, a fire bolt, a frost ray, an eldritch
  blast, a flask lobbed in an arc, each turned to where it's going and trailing its colour). Its
  log line goes in as it lands, with the sparks in the damage's colour, the damage's sound, the
  target flashing red, recoiling and hurt, and a small shake (a bigger one, gold sparks and
  CRITICAL! for a critical). Then the number pops in the big Spectral digits as the bar drains. A
  miss is a sidestep and MISS.
- **A spell** lifts the caster's hands in a cloud of its colour; a save shows radiant light falling,
  a burst, or the vines, and SAVED or FAILED over whoever made it. Healing rises in warm motes with
  its number.
- **Going down** is the downed pose and DOWN!, then the death saves over them and on their portrait:
  three sockets for successes, three for failures. Help or healing gets them up. A foe who dies fades
  off the map in grey motes; one of yours who runs out of death saves has fallen, faint on the
  ground, for the rest of the fight.
- **The Soul-Link** flickers violet as it charges, gold motes rise off the two of you, and full it
  glows and says READY; the strike is the two of you winding up and lunging together in a fall of
  gold. The Soul-Bound's lines come up over their head the way banter does.
- **A turn** starts with its banner (YOUR TURN, or whose it is) and the camera going to them. A
  foe's turn waits a moment so its banner can be read, then EnemyMind plays it, at the same pace.

On your turn the hotbar along the bottom is whoever's turn it is: the fighting slots (the weapon
and the class's own), the Soul-Link Strike, moving about, and the bag, each with its key, what's
left of what it spends, and END TURN after them. Over it are the action's circle, the bonus
action's triangle, the reaction and the movement bar, each pale for what the thing you're pointing
at would spend. Pointing at the ground shows the tiles you can reach, the path there and what it
costs, the part past your movement in the other colour, and an ember ring under every foe it'd let
swing at you. Pointing at someone shows the card: their health and armour, what's on them, and what
a click would do, with the engine's own odds ("72% TO HIT", "4 TO 11 SLASHING", "DC 13 DEX SAVE  ·
58% THEY FAIL"), the range, and the walk into reach if there is one. Anything that can't be done
says why, in the engine's words. Everything on the screen has a card: a slot (what it costs, how
far it reaches, its dice and its DC, what's left of it until which rest), the budget's parts, the
sigil, a condition, an always-on ability. The frame in the bottom left is whoever's turn it is,
the log is up the left side over it, and the tips come up on the right.

When the last foe's beaten the panel comes up with the experience, the gold and what the fallen
carried (each foe's `LootTable` and the encounter's, rolled into the gold and the bag), the
encounter's `Victory` flag is set, everyone who's down or fallen gets up on 1 HP, and its `Then`
conversation plays. Nobody in the party dies for good. Lose a fight with a `Lost` conversation and
that plays instead, everyone back on 1 HP; lose any other and it's `GameOverScreen`. A foe who died
stays off the map for the rest of the visit, and its presence flag keeps it off after that. There's
no saving mid-fight, and no inventory: what's worn can't change in the middle of one.

The first fight, the wraiths at sea, is marked `Tutorial`: the combat tips come a lesson at a time
on your turns (moving, attacking, the bonus action once the action's spent, ending the turn,
abilities from your second turn, the Soul-Link the first time it charges), and with tips on,
whatever each is about pulses gold until you've done it: the ground you can reach, the foes, the
slot, END TURN. In any other fight the same tips come once, the first time their moment does.

The proof run has `ProofAutoplay(on, speed)` to have EnemyMind play the party too (the panel at the
end goes by itself), `ProofAimAtFighter(id)` and `ProofAimAtCell(cell)` to point,
`ProofPressAbility(id)` to pick a slot, `ProofClick()` to click, `ProofShowLog(shown)` and
`ProofContinue()`, and `ProofInFight`, `ProofYourTurn` and `ProofFightOver` to wait on.

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
| `TutorialTip` | One tip: its id, its title, its words, the keys it teaches, where its card goes (`TipAnchor`), what it's about (`TipTopic`), and whether it holds everything until it's read (only the first fight's does) |
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
| The fight's and the bag's | Written, and waiting for the screens they're about: `combat-start` to `combat-victory`, `inventory-open`, `inventory-equip`, `inventory-potion` and `loot` |

The conversation's tips (going on, answering, the dice) and the wheel are only in HOW TO PLAY: a
conversation has the whole screen, and there's no single moment to say the wheel zooms. The map
screen notices the moments in `ExploreScreen.Tutorial.cs`, since nearly all of them are things it
already knows every frame (who's nearest, who's marked, what you're to do next), and the few that
are something you do (talking, using something, opening the map) tell it from where they happen.
The proof run keeps the tips quiet (`Tutorials.Quiet`), so its shots are the same as they were and
nothing waits on a press it can't make; `ProofTip` puts one up for a shot of its card.

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

None of it was recorded or downloaded. `tools/generate_audio.py` builds every song and sound from
sines, wavetables, noise and a few filters, in CPython (3.12 or newer) and nothing else. The
oscillators, filters, reverb and song mixer are The Black Box's.

```
python tools/generate_audio.py                 # everything (about five minutes)
python tools/generate_audio.py music           # the songs
python tools/generate_audio.py sfx             # every sound effect
python tools/generate_audio.py embers pause    # just these
```

| Song | Where | What it is |
| --- | --- | --- |
| *Embers* | Title, options, save slots | 56 BPM in A minor, 12 bars. A lute picking slow broken chords over a bowed drone (Am, F, Dm, Em, then F, G and an E major that pulls back round to the A), a melody on the lute an octave up for the second half, a quiet choir, a frame drum on the downbeats, wind, and a fire crackling close by |
| *Hearthsmoke* | Lit maps | 66 BPM in D Dorian, 16 bars. Folk rather than dread: the lute rolls through the chords in eighths, a wooden flute plays a tune over the second half, and the frame drum keeps a slow walk. The major IV is what makes it Dorian, and keeps it from sounding as sad as the title |
| *Cold Stone* | Underground | 48 BPM in C Phrygian, 10 bars. Hardly a tune: a low drone on C and G, a choir that leans on the flat second, a few low lute notes far apart, one deep boom, cold air, and water dripping somewhere in a long reverb |
| *Meadowlark* | The Old Road and the fields round Questeria | 136 BPM in G major, in three, 48 bars. The only waltz: the lute goes low-strum-strum under a flute tune that leaps up a sixth and skips back down like a lark going up, the middle climbs to the high E over held strings, and the last time round chimes ring an octave over the tune with a tambourine. A frame drum on the downbeats, small birds either side and a breeze |
| *Sunspire* | Sunspire Keep | 72 BPM in D major leaning Mixolydian, 18 bars. Far-off bells and two horns calling while the timpani roll in, then a trumpet plays a stately tune in dotted steps over held horn chords with a harp running up every chord; the second time the violins take it to the high D over a choir, chimes and a field drum walking it like a procession. Ends C to A, back round to D |
| *Salt and Lanterns* | The Gull's Wager | 60 BPM in E minor with every beat in three, so it rocks like a hull, 16 bars. The lute picks low, high, middle in each beat, a flute tune that keeps lifting to the G comes in at bar 5, a frame drum knocks like the hull on a swell, and the sea rolls in and out under it all, four long waves to a loop |
| *Steel* | Fights | 136 BPM in D minor, 32 bars. The low strings drive eighths on the bass in octaves the whole way, leaning on a 3+3+2 with the timpani, and the horns play Steel's call in octaves: D, the A a fifth up, the D an octave up. The trumpet takes it an octave higher with a choir and a field drum, the middle turns to E flat against the D with the violins climbing over stabbing horns and the lute picking sixteenths, and the trumpet and the horns throw the call out once more before the field drum rolls it round. A cymbal crashes in every section |
| *The Hunger* | Boss fights | 96 BPM in A Phrygian, 24 bars. Steel turned inside out: the black crystal's hum throbbing under it all, the strings grinding sixteenths that catch on the semitone above, a great bell and the war drum pounding. The low brass play Steel's call bent (the fifth a tritone, and it falls onto the semitone above home instead of home), the trumpet tries Steel's theme as it was and its last note is dragged flat under them, then the choir, the violins shrieking a semitone apart, the trumpet bending the call too and the crystal's glass ringing it high |

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
| `dice.png` | `DiceRollOverlay` | The d20, 9 frames of 40x40 at 4x: eight tumbling, then the face it lands on with the number drawn over it |
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
python tools/generate_ui.py           # button.png, panel.png, map-marks.png, cursors.png, dice.png, dialogue-icons.png, markers.png and keycaps.png
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
python tools/generate_combat_art.py   # the fight's ability and condition icons, and the pieces of its screen
python tools/cut_supplied.py          # the title, Questeria's and the harbour's props, the houses, the furniture and the townsfolk, out of tools/source/
python tools/cut_supplied.py buildings  # just the houses (or name props to cut just those)
python tools/generate_paperdoll.py    # the paperdoll sheets and the gear for all three builds, character-palettes.png and gear-palettes.png, off the youth (after cut_supplied.py's people)
python tools/rooms/generate_room_placeholders.py   # every room's sky, far, mid and foreground layers, its thumbnail and doors.txt, out of the props, tiles and title pictures above (last)
python tools/rooms/generate_room_placeholders.py --preview <folder>   # and what each room looks like in the game, outside Content
python tools/rooms/make_room_template.py --out <folder>   # the guide each room is painted over
python tools/rooms/cut_room.py <zone>/<room> background.png foreground.png   # a painted room cut into its layers (--test tries it on a made-up one)
```

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
   Mirewood, and a room at the Gilded Flagon, all back to full); short rests and the camp screen
   are still to come.

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
- `Combat/` is the fights, which draw nothing: `CombatEngine` and its partials, `Combatant`, `TurnBudget`, `ActiveCondition`, `CombatEvent`, the catalogs (`AbilityCatalog`, `ResourceCatalog`, `EnemyCatalog`, `EncounterCatalog`), `EnemyMind` for the AI, `CombatSimulator` for `--simulate`, and the grid it all happens on (`ICombatGrid`, `OpenGround`, `Measure`).
- `Items/` is everything that can be carried, worn or sold: `ItemDefinition` and `ItemCatalog`, the bag (`Inventory` over the save's `ItemStack`s), the shops (`ShopDefinition`, `ShopCatalog`) and what foes drop (`LootTable`, `LootResult`, `LootTableCatalog`). The inventory's and the shops' rules are here too, drawing nothing: `Wardrobe`, `Trade`, `GearComparison`, `ItemReadout` and `BagOrder`.
- `Components/` is every screen element, the torches, the toasts, the tooltip, the party's cards, the cursor, the paintings, the quest board, the chapters' card, the tutorial's card and key caps, and the inventory's icons, grids, item cards, faces, tiles and character sheet, each with `LoadContent`, `Update` and `Draw` the way the tutorials do it.
- `Collisions/` is the collision tutorial's rectangle and helper.
- `Particles/` is the particle engine, and the title's petals and motes built on it.
- `Audio/` is the `AudioManager`. `Save/` is the save file, the slots, and the settings.
- `Enums/` is every enum in the game, one file each.
- `Layers.cs` is the draw order, `Palette.cs` is the colours more than one screen uses, `Text.cs` draws a line of text with its shadow, and `Blends.cs` is the additive blend for anything that glows.
- `Content/Zones/` is the zone files. `tools/` is the art and sound generators, the cutting script and the asset check, `tools/source/` is the pictures it cuts, and `tools/rooms/` is the room tools, with the old tile maps they were converted from in `tools/rooms/legacy/`.

## Assets

Every picture, song and sound in the game is original: drawn or synthesised by the scripts in
`tools/`, or cut by one of them from pictures I made. The only outside work is the Spectral font, the MonoGame template files, and a few of
5e's rules from the SRD 5.1, which is credited in there word for word as its licence asks. See
[ASSETS.md](ASSETS.md) for every file.

## Author's Note

