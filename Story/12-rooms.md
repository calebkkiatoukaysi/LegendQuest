# Rooms: AQW-style maps

The world moves from top-down tile maps to side-view **rooms**, the way AdventureQuest Worlds builds its maps. A place, a **zone**, is a chain of painted rooms. In a room you walk left and right, and a little up and down in depth along the floor. Arrows at the edges take you to the next room, and doors in the painting take you inside. Fights happen in the room you're in (`11-combat.md`).

AQW is the reference for the *shape* of this (rooms, edge arrows, painted scenes, names over heads) and nothing else. Every painting is LegendQuest's own.

This file covers the format, the indicators you asked for (shops and places you can enter are marked), how detailed the painting should be, and the room list for every zone. It replaces the tile-map parts of `03b-world-art.md` (§1 and §2): its building, prop, effect, UI and music lists still apply, drawn into rooms instead of onto tiles.

---

## 1. What a room is

```text
 ┌───────────────────────────────────────── 1600 × 900 ──────────────────────────────────────────┐
 │  sky ............................................................................  (parallax 0.1) │
 │  far: the Sunspire, rooftops ......................................................  (0.3)      │
 │  mid: the street's buildings, doors, shop signs .................................... (0.7)      │
 │                       [ 🛍 The Chandlery ]             [ ⚔ The Proving Hall ]                    │
 │                            ▐█ door █▌                         ▐█ door █▌                            │
 │  ════ floor band (where feet can go) ═══════════════════════════════════════════════════  y 600 │
 │  ◀◀ Saltwind Docks       Tomas           (you) ({sb})        Netty                The Lantern ▶▶│
 │  ════════════════════════════════════════════════════════════════════════════════════  y 860 │
 │  foreground: a cart wheel, a lamp post (drawn over everyone)                          (1.1)      │
 └────────────────────────────────────────────────────────────────────────────────────────────────┘
```

- **Size.** 900 tall, always. 1600 wide for most rooms (one screen); up to 4800 for streets, fields and halls, which scroll sideways. Never vertically: a tall place (the frozen falls, the Sunspire) is a stack of rooms joined by stairs.
- **Floor band.** A polygon, usually from y 600 to 860, where feet can stand. It can curve and have holes (a fountain, a well). Characters sort by their feet's y, as now.
- **No scaling with depth.** A character is the same size anywhere on the floor (AQW does the same). The band is shallow enough that this reads fine.
- **Layers.** Sky, far, mid, floor, and foreground (§3). The mid layer is where doors and shop fronts are.
- **Exits.** Edge exits at the left and right; doors, stairs and ladders anywhere (§2).
- **Camera.** In a one-screen room, fixed. In a wider room, it follows you sideways and stops at the edges.

### 1.1 Moving

| Input | Does |
| --- | --- |
| Click on the floor | Walk there (pathing round any blocking footprints) |
| A / D, ← / → | Walk left and right |
| W / S, ↑ / ↓ | Walk deeper or nearer, within the band |
| Click a door, sign, person or thing | Walk to it, then use it |
| E or Space | Use what you're next to |
| Click an edge arrow, or walk into it | Go to the next room |

Followers trail you as now. Facing is left or right only (`03-art-overhaul.md` §1).

**Built in M3, to the author's direction (2026-11-04).** Rooms are composed from the game's existing art (its tilesets, buildings, props and the title's paintings), side-on and more detailed, by `tools/rooms/generate_room_placeholders.py`; nobody, enemies included, faces up or down; townsfolk still stroll along the band and patrols walk end to end. The course requires a tilemap rendered for some elements of the game, so every room keeps a tile strip: the ground under the floor band, and an interior's back wall, drawn from the zone's 16x16 tileset by `World/Tilemap`. Characters are the current walker sheets at 4x until the rig (M11).

## 2. Ways out, and how they're marked

You asked for shops and places to enter to have indicators. Every way out of a room, and everything you can use, is marked. Nothing that matters relies on the painting alone.

### 2.1 The kinds of exit

| Kind | Where | Marked by |
| --- | --- | --- |
| **Edge exit** | The left or right edge of the floor | Animated chevrons on the floor at the edge (`◀◀` / `▶▶`), gently pulsing. On hover, or when you're within 300 px, a label with where it goes: "Saltwind Docks ▶" |
| **Door** | A door, gate, arch or tent flap in the painting | A **badge** floating over it (§2.2), and a soft outline on the doorway on hover. The cursor turns to a door |
| **Stairs and ladders** | Up or down to a room above or below | Chevrons pointing up or down on the step, and a label on hover |
| **World exit** | The edge of a town or the end of a road | The edge chevrons, with the label "Travel" and a small compass. Opens the world map |
| **Locked** | Any of these, while locked | The same marker, greyed, with a padlock; the label says why ("Barred from inside", "The Watch is guarding it") |

### 2.2 Badges

A badge is a small round icon with a plaque under it naming the place, floating 30 px above the doorway. They're always shown for shops, trainers, inns and story places; plain doors only show theirs on hover or when you hold **Alt**.

| Badge | For | Colour |
| --- | --- | --- |
| Coin purse | A shop | Gold |
| Anvil | A smith or armourer | Gold |
| Bottle | An alchemist or apothecary | Gold |
| Tankard | An inn or tavern (rest, rumours, rooms) | Amber |
| Crossed swords | A class trainer (`10-classes.md`) | Blue |
| Scroll | The Guild, an archive, a library | Blue |
| Shield | The Watch House, a barracks | Blue |
| Sun | A temple, an abbey, a shrine | White |
| Door | A home or anywhere else you can go in | White, only on hover or Alt |
| Gold "!" over the badge | Someone in there has work for you | The existing marker, small |
| Red "!" over the badge | A story event in there | The existing marker, small |

So the Lantern Market shows, at a glance: a coin purse over the chandlery, a bottle over the apothecary, crossed swords over the Proving Hall, a tankard over the Gilded Flagon, and a gold "!" over whichever has a quest.

### 2.3 People and things

- **Names over heads**, AQW-style: every named NPC has their name above them in small caps. White to talk to; gold with "!" for a quest; blue with the shop or trainer badge beside the name for anyone you can buy from or train with. Generic townsfolk have no name until hovered.
- **Things you can use** (chests, signs, examine spots, levers, a herring on a table) glint: a small star of light every few seconds, plus an outline on hover and the hand cursor.
- **Alt** shows everything at once: every badge, every name, every exit label, every glint.
- **The zone map** (M) shows the zone's rooms as a strip of thumbnails in their real arrangement, with each room's badges and quest markers on it, and you on the one you're in. A room you've visited can be jumped to from there, outside fights and story scenes (AQW lets you jump between cells; this is the same idea, earned by having walked there).

### 2.4 Shops themselves

A shop is a room (`interior` kind): its counter, shelves of what it sells, the keeper behind the counter. Talking to the keeper opens the shop screen as now. Small shops (a market stall, a pedlar) are just a person in a street room with a badge, no interior.

## 3. How detailed the painting is

You asked for more detail, backgrounds included. These are the rules for every room, and the art spec's character rules (`03-art-overhaul.md` §2) are raised to match (§3.4 below).

### 3.1 Layers

| Layer | Parallax | What's in it | Detail |
| --- | --- | --- | --- |
| Sky | 0.1 | Sky, clouds, sun or moon | Soft gradients; 2 to 3 cloud shapes; drifts |
| Far | 0.3 | Skyline, mountains, distant towers, the sea | Low contrast, cooler, simplified shapes with a few lit windows |
| Mid | 1.0 (built: 0.7 would slide doors off their click boxes) | The buildings you can enter, walls, trees, shop fronts, stalls | Full detail: timber, stone courses, roof tiles, hanging signs, window boxes, washing lines, posters, cracks, moss |
| Floor | 1.0 | Cobbles, planks, grass, ash, with a calmer pattern in the band where people stand | Medium detail, lower contrast than the mid layer so characters read against it |
| Foreground | 1.1 to 1.3 | Lamp posts, railings, crates, foliage along the bottom edge, drawn over everyone | Dark and crisp, never covering more than 12% of the floor band |

### 3.2 Life

Every room has at least three moving things, drawn as small looping sprites or particles: smoke from a chimney, a flag, water shimmer, gulls, lantern sway, leaves, ash fall, candle flicker, a cat on a wall. Lights glow additively and flicker. The world tint (`03b-world-art.md` §5) lies over everything.

### 3.3 Style

Painted like the characters: crisp dark outlines on the mid and foreground layers, cel shading with two shadow tones and a highlight, and soft painted gradients only in the sky and far layer. Saturated where the story wants the eye (a shop sign, a door, the Sunspire), muted elsewhere. Lit from the top left, as the tiles were: rooms are never mirrored, so they keep that rule.

### 3.4 Characters, to match

To sit in rooms at this detail, characters go up in size and detail:

- A standard-build character is **200 px tall** on the 1600×900 screen, about 22% of its height, close to AQW's proportion. The rig is authored at that size (`03-art-overhaul.md` §2.2, updated).
- Cel shading gets a second shadow tone and a thin rim light along the top-back edge, so they read against detailed backgrounds.
- Costumes get one more layer of detail: trims, buckles, stitching, a pouch or charm, wear on leather. The parts list stays the same; the detail goes into the paintings of each part.

### 3.5 Making the paintings

The same way as the characters (`03-art-overhaul.md` §12):

- **Built to the author's direction (2026-11-06): the author paints every room in ChatGPT.** A room W wide is painted in panels, ChatGPT's 1536×1024 landscapes: one for W ≤ 1600, else ceil((W − 1600) / 1400) + 1, each covering 1600 of the room, overlapping the next by 200, the last right-aligned to the room's end. Only each picture's middle 1536×864 (16:9) is used; the top and bottom 80 px are bleed. `Story/14-background-prompts.md` has the prompts.
- `tools/rooms/make_room_template.py` writes each panel's guide from the room file, `shots/art/room-templates/<zone>-<room>-<i>.png` (from 1): the bleed and the band, the horizon or the back wall's foot, the floor band ("keep open: plain ground"), every door's box with its badge and name, the ways out, the overlaps ("must continue the neighbour panel"), and the panel's number, in light outlines to paint over.
- The pictures go in `tools/source/rooms/<zone>/<room>/`: `bg-<i>.png`, the whole scene sky to floor, and `fg-<i>.png` if a panel has a foreground strip (on transparency or a flat #FF00FF). `tools/rooms/cut_room.py` resizes each band to 1600×900, cross-fades the panels across their overlaps and writes the room flat: one opaque mid layer, no sky or far layer and so no parallax, faded out from the floor band's back edge so the tile strip is the floor; the foreground keyed off its backdrop; doors.txt from the door boxes; the thumbnail; and the files' lines in `Content.mgcb` and `ASSETS.md`. `tools/rooms/check_room_sources.py` says what's painted and what isn't.
- The floor stays a tilemap. Its textures are `tools/source/tiles/<tileset>/<tile>.png`, 1024×1024 and seamless; `tools/rooms/cut_tiles.py` resamples them to 64 texels onto each sheet in `TilesetCatalog`'s order, keeping any tile without one, and writes `shots/art/room-templates/tiles-<tileset>.txt`, the files each tileset needs.
- Until a room is painted, `tools/rooms/generate_room_placeholders.py` builds it from the zone's existing tileset and props at 3x: a gradient sky, a far silhouette, the props from `03b` §3 standing along the back of the band as the mid layer, and the tileset's ground as the floor. They're plain, but they're the right shape, the doors are in the right places, and everything works.
- The prompts, one a panel with its guide attached, are in `Story/14-background-prompts.md`. Don't name another game or feed it another game's art.

## 4. The room file

Rooms replace the tile maps with `Content/Zones/<zone>.txt`, read by `ZoneCatalog` at start-up, failing on the first bad line with the file and the line, as the maps' loader did. `tools/rooms/convert_maps.py` writes them from the old maps and `tools/rooms/plan.py`; nobody edits one by hand. The old `.map` files now live in `tools/rooms/legacy/`, as the converter's input; the game doesn't read them. `tools/rooms/zonefile.py` is the parser every tool shares, and its docstring is the full reference.

Everything is in canvas pixels (a room is 1600 to 4800 wide, a multiple of 64, and 900 tall), and the game divides by `ZoneCatalog.RoomScale` (4) once, on loading. A line is `head = body · field · field`, split at each middle dot; a field is a word (`mark`) or a key and a value (`gold 15`), and a value in double quotes has them taken off. `;` outside quotes starts a comment.

```text
[zone]
id = docks
name = Saltwind Docks
world = 80 190                       ; optional: on the world map
music = Exploring
region = saltwind                    ; optional, for random encounters
danger = 0                           ; towns and insides are 0

[room harbourside]                   ; rooms run left to right; settings first, then objects
name = The Harbourside
kind = street                        ; or interior: no sky, far layer or parallax
width = 3200
dark = yes                           ; optional; a dark room has ambient = 0..1
tint = 214 186 164                   ; optional
floor = 0 600, 3200 600, 3200 860, 0 860   ; a simple polygon, closed implicitly
block = 1500 700 90 40               ; an ellipse feet can't enter: centre, radii
fight = 400 2200                     ; optional: where fights stand (default the middle 1600)
legacy = 18 0 44 36                  ; the old map's tiles it came from, x0 y0 x1 y1 exclusive (saves migrate by these; they don't overlap)
tiles = questeria                    ; the tile strip: its tileset,
tiletop = 576                        ;   its top (an interior's is 0, with the back wall),
legend = + planks                    ;   a character per tile name,
row = ++++++++...                    ;   ceil((900 - tiletop) / 64) rows of width/64,
detail = ....f....                   ;   and optional detail rows ('.' is empty)
when protectorate = act3             ; a room state (reserved)
scenery mid rusty-anvil-plum 300     ; far|mid|fore, a prop, x: the painter's input
life gulls = 0 40 3200 260           ; gulls clouds smoke flag shimmer leaves motes ash candle
exit west = pier pier-east · id harbourside-west
exit east = locked · id blocked · why "No way through to the east." · unless a1-c1-02
exit up 1000 = sunspire-steps sunspire-steps-down · id festival-square-up · label "The Sunspire Steps"
door 441 = rusty-anvil start · inn · "The Rusty Anvil" · id anvil · when a1-c4-03
door 441 = locked · inn · "The Rusty Anvil" · id anvil-shut · why "Barred from the inside." · unless a1-c4-03
spawn start = 560 780 · facing right
npc gull-1 = 1137 762 · cast red-gull · facing right · unless red-gulls-beaten
npc dock-hand = 2864 706 · sheet farmhand · name "A Dock Hand" · wander 192 · say "a|b"
npc trader = 1445 663 · sheet trader · name "The Trader" · badge shop     ; or look <seed>, creature, patrol l r, aboard <prop>, talk
prop ship = 1246 600                 ; a prop on the floor (or hung on the back wall), with optional id, when, unless
chest glade = 760 660 · gold 15 · items healing-potion                   ; sign (text), lever (flag), waypoint; look and name
trigger red-gulls = 773 600 1718 860 · talk a1-c1-01 · mark · when arrived ; say, set, bark, once no
scene a1-c4-03 = talk a1-c4-03 · when a1-c4-02 · unless a1-c4-03
camp seat-1 = 870 750 · facing right
light corner-candle = 1378 700 160 0.8   ; x y reach [strength]; the id is optional
```

- **Ways out.** An exit or a door goes to `<room> <spawn>` in the zone, `<zone> <spawn>` for another (a room's looked for first), `back`, or `locked` with a `why`. Locked twins replace the old `*-shut` triggers: the same exit or door twice, one `when` a flag and the other `unless` it. West and east exits go off in the 48 px at that end; up and down stand at an x; `label` changes what an exit's label says and `travel` makes it open the world map.
- **Doors** stand on the floor's back edge at x. The badge is one of shop, anvil, bottle, inn, trainer, scroll, shield, sun, crown, door, gate, stair-up and stair-down, and sizes the box (120×200; gate and crown 300×300; stairs 160×120) unless `size w h` says.
- **Ids.** Spawn names are unique in a zone, and so are the ids of everything else, which keep the old maps' ids, so saves' zone/id keys still mean what they did. Every room has a spawn and every zone has `start`. An exit's id defaults to `<room>-<side>` and a door's to where it goes, but the converter always writes them.
- **People** are one of `cast`, `sheet` (with `name`), `look` (a seed, with `name`) or `creature`, facing left or right (toward the room's middle if it isn't set). Every `place x at <spawn>` in the scripts names a spawn; `tools/check_spawns.py` (build plan §8) checks it against the room the script plays in.
- `tools/rooms/check_rooms.py` checks everything `ZoneCatalog` does, and walks every room to be sure each trigger, door, exit, person and thing in it can be reached.

## 5. Fights in rooms

When a fight starts, the slots (`11-combat.md` §1) are laid out on the room's floor inside its `fight` range: your side on the left third, theirs on the right, front rows at the middle. Anyone not in the fight (townsfolk, a merchant) steps back to the band's far edge and watches, or leaves by the nearest edge if the fight's lethal. In a wide room the camera centres on the fight. A fight across several rooms (`11-combat.md` §5.3) moves the party through the room's exit between waves.

## 6. Every zone's rooms

Each map from `03b-world-art.md` §1 and §3b becomes a zone of these rooms, left to right unless a room is above or below another. The one-line description is the painting's brief. Spawn names the scripts use (`09-registry.md`) go in the room their scene plays in.

### Questeria and the coast

| Zone | Rooms |
| --- | --- |
| `gulls-wager` | **deck** (the main deck at sea, rigging, a grey sky and a long swell) · **bow** (the bow rail, the city on the horizon when docking) · **hold** (below, hammocks and crates) |
| `docks` | **pier** (the long pier, boats moored, gulls on every post) · **harbourside** (Tomas's net-rack, the customs house, the *Wager*'s berth with a gangway) · **fish-market** (stalls, ice, a crane hoisting crates) |
| `questeria` | **gate-plaza** (the city gate and the Watch's notice board) · **lantern-market**, 3200 wide (the market under strings of lanterns; Chandlery, Proving Hall, Gilded Flagon, apothecary) · **sunspire-steps** (the broad steps up to the keep, the Sunspire filling the far layer) · **festival-square** (the square of the Purging Dawn, a fountain, the balcony above) · **thornbridge-gate** (the arch down to Thornbridge) |
| `low-ring` | **tallow-lane** (narrow, washing lines, Duskmantle's laundry) · **counting-house** (Gannet's, a ledger room, interior) · **sewer-grate** (an alley ending in a grate) · **sewers** (below: brick tunnels, a channel of black water) · **rooftops** (above: chimneys and slate, the city at night, for SQ3-Hale) |
| `gilded-flagon` | **taproom** (interior, a long bar, a fire, a balcony) · **upstairs** (a private room with a window over the market) |
| `rusty-anvil` | **forge** (interior, the anvil, racks of shields, the back door to the yard) |
| `proving-hall` | **hall-floor** (new; interior, a sanded floor, weapon racks, practice dummies, a gallery) |
| `watch-house` | **front-desk** (interior, Hale's desk, the roster board) · **yard** (the training yard, for `trial-watch`) |
| `thornbridge` | **camp-gate** (the refugee camp's gate, Hedda's post) · **tents**, 3200 wide (rows of tents, the clinic tent, cooking fires) · **river-edge** (the bridge itself and the river below) |
| `sunspire-keep` | **courtyard** (the keep's courtyard, the gate, the Gilded knights' muster in Act 2) · **great-hall** (the throne, banners, the gallery) · **study** (Leon's study, interior) · **war-room** (a map table) · **council-room** (the small council room, a window over the square) · **balcony** (over the festival square) · **quarters** (your rooms: the bed for every "plays at your next rest" scene) · **chapel** (the royal chapel; the crypt stair goes down to `aurelian-crypt`) · **grand-stair** (the stair to the roof, for A3-C4-05) |
| `sunspire-roof` | **roof** (the parapet at night, the city's lights below) |
| `sunspire-summit` | **summit** (the throne room, roof torn off, the time wall at the door). After Act 3: **ruins** (the same, broken, the scar in the sky) |
| `arcanum-spire` | **foyer** (the Guild's hall of glass and brass) · **late-stacks** (shelves into the dark) · **lectern-hall** (Thessaly's) · **vault-stair** (down to the Deepvault) |
| `deepvault` | **seal-ring** (the twelve seals in a ring of light) · **relics** (the Stillglass reliquary and the Eye's case) |
| `pier-memory` | **pier** (the Kettle Cove landing in a storm; used by P-03 and A3-C5-07) |
| `questeria-still` | The same rooms as `questeria`, in their `still` state (every NPC frozen, black sky) |
| `questeria-hills` | **hilltop** (grass, one tree, the walls in the far layer at dawn) |
| `questeria-plain` | **plain**, 4800 wide (farmland and the road to the gate; the siege's field) · **siege-camp** (the muster, carts, tents, Elara's cart) |

### The Mirewood and the east

| Zone | Rooms |
| --- | --- |
| `mirewood` | **edge** (the forest's edge, the road) · **deep** (old trees, moss, mist, black-veined in Act 1) · **silverbark** (the clearing with the young silverbark) · **camp** (the resistance camp in Act 3: tents, the stump table) |
| `rangers-lodge` | **lodge** (interior, a long table, antlers, maps; Tamsin's hawk on a perch) |
| `mirewood-quarry` | **lip** (the quarry's edge) · **floor** (the quarry floor, the crystal, the Thornmother's den) |
| `old-crown-vault` | **entrance** · **halls** · **strongroom** (as built, repainted as three rooms) |
| `kestrel-ford` | **road** (the east road through low hills) · **ford** (the river crossing, the wagons) · **village** (Hester's cottage, the inn) |
| `greywatch` | **road** (the switchbacks up) · **gate** (the fortress gate) · **wall-walk**, 3200 wide (the walls, the siege below) · **keep** (Aldric's bier) |

### Doomward

| Zone | Rooms |
| --- | --- |
| `ashen-strait` | **shore** (grey sand, a black sea, the ferry) |
| `ashfall` | **street** (the muster board, Brenna's people) · **ash-pits** (for `trial-ash`) · **garrison** (Act 3: the Protectorate's hold, the Hollowed line) |
| `thornwall` | **path** (the walk through the briar wall) · **levy-camp** (Tobin's fire) |
| `ember-archive` | **stacks**, 3200 wide (the archive's halls) · **lowest-stacks** (the Index's shelf) |
| `well-of-embers` | **rim** (the edge of the well) · **floor** (the bottom, the circle of battlemages, Maleficio) |

### Act 2

| Zone | Rooms |
| --- | --- |
| `aurelian-crypt` | **stair** · **tombs** · **lectern** (the sun-scrolls) |
| `saint-lumen` | **landing** (the island jetty) · **cloister**, 3200 wide (the shady side and the sunny side, the well) · **hives** (Orla's garden) · **cliff-top** (the raid's room) |
| `abbey-chapel` | **chapel** (interior, the altar steps, stained glass) |
| `strait-cave` | **cave** (tide pools, the dry ledge) |
| `gilded-chapterhouse` | **hall** (the altar, banners) · **cells** (below) · **crypt** (for `trial-vigil`) |
| `morvaine-pass` | **pass** (snow, black pines, a pale sun) · **camp** |
| `hollowmere` | **village**, 3200 wide (neat houses, the elder's porch, the far gate) · **blood-house-road** |
| `blood-house` | **gate** · **halls** · **cellar** |
| `glasswood` | **woods** (trees of black glass) · **conservatory** (the glasshouse, the prisms, the desk) |
| `sunscar-waste` | **waste**, 4800 wide (red sand, glass dunes, an ember sun) |
| `sunfall-temple` | **mural-hall** (the Purging Dawn mural) · **sanctum** (the altar, the Eye's pedestal) |
| `noxhallow-falls` | **base** · **ice-ledges** (above) · **falls-top** (above). Stacked rooms joined by stairs |
| `noxhallow` | **courtyard** (the Order pinned) · **library** (the hanging library) · **treasury** (Crane) |
| `blood-orrery` | **orrery** (the clockwork orrery, the Sun Mirrors in the mid layer) |
| `sunward-cell` | **cell** (a sunny stone room) |

### Act 3

| Zone | Rooms |
| --- | --- |
| `stopped-clocks` | **hall** (Vorlaith's hall of stopped clocks) |
| Every Act 1 and 2 zone | A `protectorate` state where Act 3 changes it: violet banners, checkpoints, the granaries. `thornbridge` gets a `frozen` state |

## 7. Other systems this touches

| | Change |
| --- | --- |
| Scripts | `travel <map> <spawn>` becomes `travel <zone> <spawn>`; the spawn says which room. No script needs editing: map ids become zone ids, and the spawn names stay. `camera north` becomes `camera <spawn>` |
| Stealth (SQ3-Hale, the blood-house, the thrall patrols) | Patrols walk left and right along the band with a sight cone along the floor, drawn as a translucent wedge. Same rules as before, side-on |
| Random encounters (`08` §2) | On entering a wild room, roll (the danger meter now counts rooms entered, not steps). If it fires, the encounter is *in the room*: foes standing with name plates, visible before they see you. Walk within 400 px or click them to fight. Slip past by keeping to the band's far edge and passing a Stealth check. Back off by leaving the room. An ambush jumps out when you reach the room's middle |
| The world map | Unchanged: zones are its places |
| Saving | You're saved by zone, room and position. Old saves: by the old map's start room |

## 8. Code

| New | Does |
| --- | --- |
| `ZoneCatalog`, `ZoneDefinition`, `RoomDefinition` | Read and check `Content/Zones/*.txt` |
| `RoomScene` | Replaces `MapScene` for drawing: the layers with parallax, animated details, lights, the floor band, sorted actors, foreground |
| `RoomWalker` | Movement on the band: click-to-walk with simple pathing round `block` ellipses, keys, followers |
| `IndicatorLayer` | Badges, name plates, exit chevrons and labels, glints, Alt to show all |
| `ZoneMapScreen` | The M map: room thumbnails, badges, jump to a visited room |
| `ExploreScreen` | Keeps its partials (conversations, banter, markers, camp, items, proof), with the tile-specific parts swapped for `RoomScene` and `RoomWalker` |

The tile maps' code went at the end of M3. Their `.map` files stay in `tools/rooms/legacy/` as the converter's input, and the tilesets stay, since the rooms' tile strips are drawn from them. Zones the later acts add are written as zones from the start.
