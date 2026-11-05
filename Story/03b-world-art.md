# World art: places, buildings, props, effects, UI and music

Carried over from the earlier plan (`STORY.md` Part 2), and now read alongside `12-rooms.md`.

- **The world is rooms now.** `12-rooms.md` replaces tile maps with AQW-style painted rooms and lists every zone's rooms (§6). This file's §1 (maps) and §3b (new maps) stay as the *brief for each place*: what it's for, which scenes use it, what's in it, how dark it is, its music. §2 (tilesets) now only feeds the placeholder rooms until the painted ones arrive.
- **Buildings and props** (§3) are painted into rooms' mid and foreground layers, or placed as separate sprites when they're interactive (a door, a chest, a sign). Every enterable building gets a badge (`12-rooms.md` §2.2).
- **Characters, creatures and backdrops** are in `03-art-overhaul.md`, which supersedes the old §2.4, §2.5 and §2.8.

The repo rules still hold: every PNG made by a seeded script or cut from `tools/source/`, and every file listed in `ASSETS.md` and `Content/Content.mgcb` so `check_assets.py` passes.

**Source column.** `G` = drawn by a `tools/` generator. `S` = a supplied picture in `tools/source/`, cut by `cut_supplied.py`.

**Engine additions the art depends on:**

- **Room states.** `when=<flag>` and `unless=<flag>` on any room line (`12-rooms.md` §4), so one zone can show its Act 1, Act 3 and ruined states (Questeria under the Protectorate, Thornbridge frozen, Greywatch after the siege) without copying the zone.
- **World tint.** A per-map or per-cue colour multiply for the eclipse stages, the violet stopped-time look, and the time-distortion eras (§5).
- **Region and danger** in the zone header, for random encounters (`08-random-encounters.md` §2.1).

## 1. Maps

Existing maps are reused where the story passes through them.

| Map id | Name on screen | Tileset | Size (tiles) | Dark | Music | Used in | What's on it |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `gulls-wager` | The *Gull's Wager* | `ship` (new) | 30x14 | yes, ambient 0.35 | Sea | P-04, P-05, A3-C3-06 | Deck at night, rail, mast, hanging lanterns, crow's nest (overhead), hatch, sea all round |
| `docks` (exists) | Saltwind Docks | `questeria` | as is | no | Exploring | P-05 end, A1-C1-01 | Add fish stalls, net racks, harbour bell, Red Gull thugs, Old Tomas |
| `questeria` (exists) | Questeria | `questeria` | as is | no | Exploring; Protectorate in Act 3 | Hub, A1-C1-02, A1-C3-01, A2-C1-01, A3-C1-02 | Lantern Market = the existing market; the Gilded Flagon = the inn; quest board; east gate; doors to the keep, the Arcanum Spire and the Low Ring. Act 3 state via `when=` objects |
| `low-ring` | The Low Ring | `questeria` + `sewer` tiles | 36x28 | partly | Exploring | A1-C7-03, A3-C2-05, A3-C3-04 | Shabby houses, the fence's shop, sewer grates and a sewer run for the escape |
| `gilded-flagon` | The Gilded Flagon | `interior` (new) | 20x14 | yes, ambient 0.5 | Exploring | A1-C4-01 | Tavern hall, back booth, hearth |
| `rusty-anvil` | The Rusty Anvil | `interior` | 16x12 | yes, ambient 0.4 | Exploring | A1-C4-05 | Dim dockside bar, Brannock's corner table |
| `sunspire-keep` | Sunspire Keep | `keep` (new) | 44x34 | no | Court | A1-C4-03/04, A1-C6-04, A1-C7-03, A1-C8-06, A2-C1-03, A2-C5-01, A2-C6-06, A3-C1-03, A3-C2-04 | Throne room, great hall, Crane's study, Leon's private study, war room, balcony over the square |
| `sunspire-roof` | The Sunspire roof | `keep` | 24x16 | yes, night | Court (soft) | A1-C8-07, A3-C1-04 | Rooftop, parapet, city lights below |
| `sunspire-summit` | The Sunspire, open to the sky | `keep` + `void` | 30x24 | no | SoulBound | A3-C5 | The throne room with its roof torn off, frozen moments hanging in the air, the time-wall at the door |
| `arcanum-spire` | The Arcanum Spire | `arcanum` (new) | 30x30 | yes, ambient 0.45 | Court | A1-C7-02, A3-C4-03 | Mages Guild tower: arcane circle, shelves, astrolabe, stair down |
| `deepvault` | The Deepvault | `arcanum` | 24x24 | yes, ambient 0.1 | Underground | A2-C6-06/07, A3-C2-02, A3-C4-03 | Twelve seal pillars, two pedestals, the Stillglass Reliquary, crystal-sealed Thessaly in Act 3 |
| `thornbridge` | Thornbridge | `questeria` | 40x30 | no | Exploring | A1-C3-01/02, A3-C1-02, A3-C3-05 | Act 1: tent camp outside the east gate. Act 3: the free clinic and granary, then the frozen district |
| `mirewood` | The Mirewood | `forest` (+ corrupted tiles) | 50x40 | no, twilight tint | Exploring | A1-C2-01, A3-C3-01 | Black-veined trees, tar pools, the ranger trail; in Act 3 the resistance camp |
| `mirewood-quarry` | The Old Quarry | `quarry` (new) | 36x32 | yes, ambient 0.08 | Underground | A1-C2-02 | Root puzzles, the black crystal spike, the courier's body, the Thornmother's den |
| `old-crown-vault` | The Old Crown Vault | `crypt` + `vault` tiles | 40x36 | yes, ambient 0.06 | Underground | A1-C4-02 | Traps, collapsed tunnels, relic shelves, vault constructs, the Sunheart pedestal |
| `kestrel-ford` | Kestrel Ford | `highland` (new) | 50x30 | no | Exploring | A1-C5-01/02 | Highland road, the ford, caravan wagons, the night camp |
| `greywatch` | Greywatch Fortress | `fortress` (new) | 44x40 | yes at night, ambient 0.15 | Siege | A1-C5-03, A1-C6-01..03 | Walls, outer and inner gates, courtyard, battlements; post-siege state via `when=` |
| `questeria-plain` | The Questeria Plain | `highland` | 60x36 | no | Siege | A1-C7-01, A3-C4-01/02 | Army muster, Act 3 battle lines |
| `ashen-strait` | The Ashen Strait | `doomward` (new) | 40x24 | no, grey tint | Doomward | A1-C7-04 | Black beach, tar shallows, the orchard road inland |
| `ashfall` | Ashfall | `doomward` | 40x34 | no | Doomward | A1-C7-05, A3-C3-03 | Ruined town, resistance barricades; Act 3 Protectorate garrison via `when=` |
| `thornwall` | The Thornwall | `doomward` | 40x40 | yes, ambient 0.1 | Doomward | A1-C7-05 (skipped with Brenna's help) | Thorn maze |
| `ember-archive` | The Ember Archive | `archive` (new) | 44x44 | yes, ambient 0.12 | Underground | A1-C8-01 | Burned library, floating shelves, frozen fire, memory echoes |
| `well-of-embers` | The Well of Embers | `archive` | 30x30 | yes, ambient 0.2 | Boss | A1-C8-02..05 | Round chamber, black-fire pit, four Ember Conduits |
| `aurelian-crypt` | The Aurelian Crypt | `crypt` + sun motifs | 36x30 | yes, ambient 0.06 | Underground | A2-C1-02 | Royal tombs, sun-crested sarcophagi, scroll lectern |
| `saint-lumen` | Saint Lumen's Abbey | `abbey` (new) | 44x36 | no | Exploring (night: Underground) | A2-C2-01/02 | White abbey on a sea cliff, cloister, gardens, bell tower, cliff paths for the raid |
| `abbey-chapel` | The Chapel | `abbey` | 18x22 | yes, moonlight | Exploring | A2-C2-03 | Altar, pews, stained glass |
| `morvaine-pass` | The Morvaine Pass | `morvaine` (new) | 50x30 | no, eclipse tint | Morvaine | A2-C3-01 | Snow, black pines, a border stone |
| `hollowmere` | Hollowmere | `morvaine` | 40x34 | no | Morvaine | A2-C3-02 | Tidy tithe village, the blood-letting house in the square |
| `glasswood` | The Glasswood | `glass` (new) | 44x40 | yes, ambient 0.25 | Morvaine | A2-C3-03/04 | Frozen forest of glass trees, the Moonlit Conservatory, the night camp |
| `sunscar-waste` | The Sunscar Waste | `desert` (new) | 56x40 | no, ember-sun tint | Sunfall | A2-C4-01 | Red fused-glass dunes, half-buried priest statues |
| `sunfall-temple` | Sunfall Temple | `sunfall` (new) | 44x44 | yes, ambient 0.2 | Sunfall | A2-C4-02..05 | Mirror and lens puzzles, murals of the Purging Dawn, the sanctum and blood altar |
| `noxhallow-falls` | The Frozen Falls | `noxhallow` (new) | 30x56 (tall) | yes, ambient 0.1 | Noxhallow | A2-C5-02 | Ice climb, ledges, thrall patrols, the hanging library |
| `noxhallow` | Castle Noxhallow | `noxhallow` | 44x40 | yes, ambient 0.15 | Noxhallow | A2-C5-03 | Blood-fountain halls, the treasury, the courtyard where Corwin is pinned |
| `blood-orrery` | The Blood Orrery | `noxhallow` | 32x32 | yes, ambient 0.1 → daylight | Boss | A2-C5-04, A2-C6-01..04 | The orrery centrepiece, four Sun Mirrors, the falls behind |
| `sunward-cell` | The Sunward Cell | `morvaine` | 14x12 | no, bright | Morvaine (soft) | A2-C6-04, A3-C3-02 | Sunlit tower room, one window, a bed and a desk |
| `pier-memory` | The pier | `questeria` (docks tiles) | 24x14 | stormy tint | SoulBound | P-03 background, A3-C5-07 | Small fishing-town pier, capsized boat, storm |
| `stopped-clocks` | The Hall of Stopped Clocks | `void` (new) | 34x30 | yes, ambient 0.2 | Vorlaith | A3-C5-07 | Clocks frozen at different hours, a glass throne, ranks of the Unmade |


## 2. Tilesets

Each new set goes in `TilesetCatalog` and `Content/Tiles/<id>.png`. Tile names follow the existing style.

| Tileset | Source | Tiles |
| --- | --- | --- |
| `ship` | G | `deck` (3 variants, planks), `deck-edge` (edged), `rail` (solid), `hatch`, `sea` (3, solid, animated shimmer), `sea-foam` (edged) |
| `interior` | G | `floor-wood` (4), `floor-stone` (3), `rug` (edged), `wall-top`, `wall-face` (3, solid, opaque), `wall-base`, `hearth-wall` |
| `keep` | S | `marble` (3), `marble-gold-inlay` (2), `carpet-royal` (edged, red with gold), `wall-top`, `wall-face` (3), `wall-window` (tall window, block 1x2), `parapet` (solid), `roof-slate` (3), `torn-floor` (Act 3 summit, cracked) |
| `sewer` (add to `questeria`) | G | `sewer-floor` (3), `sewer-water` (edged, solid), `sewer-wall-face`, `grate` |
| `quarry` | G | `quarry-floor` (4), `quarry-wall-face` (3), `rubble` (solid), `root-floor` (2), `black-vein-floor` (2, corruption), `tar-seep` (edged, solid) |
| `vault` (add to `crypt`) | G | `vault-floor` (gold trim, 2), `vault-wall-face`, `pressure-plate`, `pit` (solid), `coin-scatter` |
| `forest` additions | G | `leaves-blackvein` (3), `tar-stream` (edged, solid), `gloom-moss` (2) |
| `highland` | G | `heather` (4, grass), `road-gravel` (3), `ford-water` (edged, shallow, walkable), `ford-deep` (solid), `boulder` (solid) |
| `fortress` | G | `flagstone` (4), `flagstone-cracked` (2), `wall-top` (2), `wall-face` (3), `battlement` (solid, see-over), `gate-arch` (block 3x2), `snow` (3, edged), `scorch` (post-siege), `rubble` |
| `doomward` | G | `ash` (4, grass), `tar` (edged, solid, glossy), `black-sand` (3), `thorn` (2, solid), `dead-orchard-soil` (2), `glass-ruin-floor` (2), `tar-shallow` (walkable) |
| `archive` | G | `charred-floor` (4), `ember-floor` (2, glowing cracks), `archive-wall-face` (3), `burned-shelf-wall`, `black-fire-pit` (block 3x3, solid, animated) |
| `abbey` | S | `white-stone` (3), `cloister` (2), `garden-soil` (2), `grass` (edged), `cliff-edge` (edged, solid), `sea` (solid), `chapel-floor` (2), `wall-face` (3), `stained-window` (block 1x2, 3 colourways) |
| `morvaine` | G | `snow` (4, edged), `black-stone` (3), `pine-needles` (2), `ice` (2, slick), `cobble-black` (3), `wall-face` (2) |
| `glass` | G | `frost-ground` (3), `glass-root` (2), `black-glass-floor` (2, conservatory), `glass-wall` (solid, see-through), `night-flower-bed` |
| `desert` | G | `fused-glass-sand` (4, red), `glass-crust` (2, crunchy surface), `dune-edge` (edged), `sandstone-ruin` (2, solid) |
| `sunfall` | G | `sandstone-floor` (3), `sun-tile` (2, gold inlay), `mural-wall` (block 2x1, 4 scenes), `wall-face` (2), `light-channel` (floor groove beams travel along), `blood-channel` |
| `noxhallow` | G | `ice-wall` (3, solid, opaque, frozen waterfall), `ice-ledge`, `black-stone-floor` (3), `blood-channel` (edged), `carpet-crimson` (edged), `wall-face` (3) |
| `void` | G | `cracked-glass-floor` (4), `clock-floor` (block 3x3, a clock face), `void` (solid, starless), `time-scar` (animated edge) |


## 3. Buildings and props

Every prop is a `PropCatalog` entry with a footprint. A prop with states (shut/open, intact/broken, asleep/awake) gets a frame per state, like the existing door and lever.

**The ship and docks** (G unless noted): `ship-rail`, `mast` (overhead top), `lantern-hanging` (light), `rope-coil`, `ship-hatch`, `ship-wheel`, `crows-nest` (overhead), `fish-stall` (S), `net-rack` (S), `harbour-bell`.

**Questeria, Act 1** (S, to match the existing props): `quest-board` (three columns: monsters, bandits, lost-and-found), `gilded-flagon-sign`, `rusty-anvil` (building), `low-ring-house` (2 variants, shabby), `sewer-grate`, `east-gate` (large arch, open/shut), `watch-post`, `sunspire-keep` (the keep's exterior, if the existing `palace` isn't it), `arcanum-spire` (tall tower of blue glass), `granary` (Act 3), `clinic` (Act 3).

**Questeria, Act 3 Protectorate state** (S): `banner-protectorate` (violet and gold), `curfew-bell`, `checkpoint` (barricade), `protectorate-poster` (sign).

**Sunspire Keep** (S): `sun-throne` (gold, a sun behind it), `sun-banner` (Aurelian sun), `pillar-marble`, `war-table` (map of Fable), `royal-desk`, `wine-table`, `guard-statue`, `balcony-rail`, `throne-ruined` (Act 3 summit), `time-wall` (shimmering barrier across a door, A3-C5-01).

**Taverns** (G): `bar-counter`, `tankard-table`, `booth`, `hearth` (light), `keg-stack`.

**Thornbridge** (S): `tent-ragged` (2), `cookfire` (light), `cot`, `healer-station`, `refugee-bundle`, `supply-crate` (exists as `crate`).

**Mirewood and the quarry** (G): `tree-blackvein`, `tar-pool`, `gloom-mushroom`, `ranger-sign`, `woodcutter-stump`, `root-wall` (puzzle: tangled/parted), `black-crystal` (large spike, pulsing, intact/shattered/purified), `courier-body`, `satchel`.

**Old Crown Vault** (G): `vault-door` (large, shut/open), `relic-shelf`, `gold-pile`, `dart-trap`, `vault-construct` (dormant/awake, becomes an enemy), `sunheart-pedestal` (with/without gem).

**Kestrel Ford** (G): `caravan-wagon` (whole/broken), `ford-stones`, `camp-tent`, `campfire` (exists).

**Greywatch** (G): `siege-ladder`, `outer-gate` (whole/broken), `inner-gate`, `weapon-rack`, `faded-banner`, `catapult`, `brazier` (light), `bier` (Aldric).

**The Arcanum Spire and Deepvault** (G): `arcane-circle` (floor decal, animated, blue), `guild-shelf`, `astrolabe`, `crystal-focus` (light), `seal-pillar` (intact/broken, x12), `relic-pedestal`, `stillglass-reliquary` (clear crystal case, empty/holding the Box), `eye-case` (crystal case for the Eye), `crystal-prison` (Thessaly frozen inside; a prop, not a character).

**Doomward** (G): `thorn-bush` (2), `glass-tower-ruin` (large, clouded black), `dead-orchard-tree` (2), `ash-pile` (where a Hollowed fell), `barricade`, `ruined-house` (3), `knight-statue-broken`.

**The Ember Archive and the Well** (G): `floating-shelf` (bobs), `frozen-fire` (light, still flames), `memory-echo-spot` (marker that plays an echo), `ember-conduit` (x4, intact/destroyed, boss mechanic), `black-box-plinth`.

**The Aurelian Crypt** (G): `royal-sarcophagus` (sun crest), `scroll-lectern`.

**Saint Lumen's Abbey** (S): `abbey-chapel` (building), `cloister-arch`, `garden-bed` (2), `bell-tower` (building), `altar`, `pew`, `abbey-well`.

**Morvaine** (G): `black-pine` (2, large), `red-lamp-post` (light, red glass), `blood-house` (building, the tithe house), `snowdrift`, `border-stone`, `tithe-cottage` (2).

**The Glasswood** (G): `glass-tree` (3), `night-flowers`, `prism` (puzzle, four rotations), `conservatory` (black-glass greenhouse, building), `isadora-journal`.

**Sunscar and Sunfall** (G): `priest-statue-buried`, `obelisk-ruined`, `sun-mirror` (puzzle, four rotations, lit/unlit), `lens-stand`, `priest-construct` (dormant/awake, becomes an enemy), `blood-altar`, `eye-pedestal` (with/without the Eye).

**Noxhallow** (G): `blood-fountain` (light, red), `hanging-shelf`, `treasury-chest`, `chains`, `blood-orrery` (very large animated centrepiece with a cradle for the Eye and a place Valencia is chained), `sun-mirror-tall` (x4, unlit/lit; the boss's mechanic).

**The Sunward Cell** (G): `sun-window` (light, bright), `cell-bed`, `cell-desk`.

**Act 3 camp and battle** (G): `resistance-tent` (2), `stump-table`, `map-table`, `crown-construct` (becomes an enemy).

**Time and the stinger** (G): `stopped-clock` (4 sizes), `glass-throne`, `frozen-moment` (a banner or bird hanging mid-fall; for the summit and frozen district), `pier`, `capsized-boat`.


## 3b. Maps the new scripts add

| Map id | Name on screen | Tileset | Size (tiles) | Dark | Music | Used in | What's on it |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `questeria-still` | Questeria (the vision) | `questeria` | as `questeria` | no, starless tint | none | A3-C1-01 | The city map with every NPC placed mid-step and frozen, snow stopped in the air |
| `questeria-hills` | The hills outside Questeria | `highland` | 30x20 | no, dawn | SoulBound (soft) | A3-C2-05 | A hilltop with a view of the walls, a lone tree for Valencia's shade |
| `watch-house` | The Watch House | `interior` | 16x12 | yes, night | Protectorate | A3-C3-05 | Hale's desk, a roster board, a window onto the roofs |
| `gilded-chapterhouse` | The Gilded Chapterhouse | `abbey` + `keep` tiles | 30x24 | no | Court (soft) | 07 §5.1, A3-C3-04 | The Order's hall: an altar, banners, cells below |
| `rangers-lodge` | The Rangers' Lodge | `interior` (wood) | 18x14 | no | Exploring | 07 §3.2 | A long table, antlers, maps of the Mirewood |
| `blood-house` | The Blood-House | `morvaine` + `interior` | 28x24 | yes, candle | Morvaine | 08 §5.1 | A tidy manor; a cellar with cots |
| `strait-cave` | A sea-cave on the strait | `quarry` (wet variant) | 20x16 | yes, cave | Sea | 08 §5.2 | Tide pools, a dry ledge at the back |
| `noxhallow-falls` | The Frozen Falls | `noxhallow` | 30x56 | yes | Noxhallow | A2-C5-02 | (as listed above; named here because the scripts reach it) |

The scripts also use these spawn names, which each map file needs as named points: every `place <x> at <spawn>` in `04` to `08`. `tools/check_spawns.py` (build plan §8) lists any a map is missing.

## 4. Relics, items and HUD icons

The HD pictures and sockets for these are in `03-art-overhaul.md` §6.4; this table is the inventory icons (16x16 and 32x32) and their looks.

Each needs a world prop where it sits on a map, a 16x16 icon for the inventory, and a larger 32x32 version for cutscene close-ups.

| Item | Look |
| --- | --- |
| Soul-Bond sigil | Two interlocking hands in a circle. HUD icon with five states: gold, gold with a one-frame violet flicker, violet, grey (broken), grey with a gold pulse |
| The Black Box | Palm-sized cube of lightless metal, faint seams, a hum ring when held |
| Black Box shard | Jagged black splinter with a violet edge glow; a "sliver" variant (smaller) |
| Stillglass Reliquary | Clear crystal case; empty and holding-the-Box states |
| The Sunheart | Gold gem with a warm inner light |
| The Eye of Solvaris | Fist-sized gold lens; asleep (dull), shining (gold), corrupted (red-black) |
| The Crown of Dusk and Dawn | Circlet of braided black metal and gold crystal, the shard set in the centre; whole, cracked, and two halves |
| Tomas's Lucky Hook | Brass fish hook on a cord |
| Black-iron coin | Black coin stamped with a cube |
| Varkhul's greatsword | Hollowed blade, shard in the hilt; shattered variant |
| Brannock's shield | Battered round shield; Act 3 variant with forty-one names scratched in, and a forty-second |
| Isadora's journal | Dark leather, a pressed night-flower |
| Sun-scroll | Half a scroll with a sun seal |
| Courier's letter | Charred paper with a cube seal |


## 5. Effects

Particles go in the existing `Particles` system; colours come from `Palette.cs` (add violet, gold and blood red if they aren't there).

- **Soul-Link tether**: a gold thread between you and {sb} that brightens as the combo charges; violet-tinged from A1-C8; gone after A3-C2-04; returns as **Soul-Clash** (two gold beams meeting) in A3-C5-02.
- **Violet afterimage**: {sb}'s strikes leave a fading violet copy from A1-C7.
- **Corruption**: black smoke from eyes (wolves, rats), black veins pulsing in trees, the black crystal's pulse ring.
- **Guild magic**: blue fire gate (A1-C8-05), the reliquary's sealing ring, the crystal blooming over Thessaly (A3-C2-02).
- **Hollowed dissolving** into ash.
- **Black fire**: the Well's column, Maleficio's phase 2 flood, the Box opening and pulling him in (A1-C8-03).
- **Eye beams**: pure gold, gold with a violet edge ({sb} wielding it), red-black (Severin's ritual).
- **Blood mist and bats** for Severin's teleports.
- **Eclipse stages 1-5**: a grey ring sliding over the sun, the world tint darkening each stage, stars at stage 4, total black at stage 5, then sunrise.
- **Stopped time**: violet haze, hanging snowflakes, a frozen pigeon (A3-C2-02, A3-C3-05, A3-C5-01).
- **Time-wall shimmer** at the summit door.
- **Memory echoes**: a blue, translucent, flickering shader for Isadora, young Maleficio and young Mira.
- **The Voice**: on-screen text in italic with a faint echo offset; a violet vignette while it speaks.
- **Time tear**: the sky cracking like paper, light from many eras inside it, the arena flickering between eras (A3-C5-03), glass shattering off the Crown, and the scar left in the sky afterwards.
- **Reflection delay**: {sb}'s reflection in glass moves a half-second late (A1-C6-05, A2-C6-07). Draw the reflection as a second, delayed copy of their sprite.


### World tints

Every `tint <id>` the scripts use. A tint is a multiply colour and a strength over the world and characters, faded in over 0.6 s unless the cue says otherwise. `off` fades back to the map's own.

| Id | Look |
| --- | --- |
| `day`, `daylight` | Neutral. `daylight` is a little warmer, for the end of the eclipse |
| `dawn` | Pink-gold, low |
| `twilight` | Blue-violet, the Mirewood's |
| `night`, `moonlight` | Deep blue at 55%; `moonlight` a paler, silver blue |
| `snow`, `snow-night` | Cool white; the same at night |
| `candle`, `torches-dim`, `fire` | Warm orange at three strengths; `fire` flickers |
| `cave` | Green-grey, dark |
| `ash` | Grey, desaturated by half |
| `ember-sun` | Red-orange, the Sunscar's |
| `eclipse`, `eclipse-1` to `eclipse-5` | Each stage greyer and darker; stars at 4; black at 5 |
| `black` | Nearly black, for Severin's darkness |
| `starless` | Black-blue with no stars, the vision |
| `violet-haze` | Violet at 35% with drifting motes: stopped time |
| `flicker-day-night` | Cycles between `day` and `night` every 2 to 4 s, randomly: after the tear |

## 6. UI

All `G` in `generate_ui.py`, built on the existing `panel` and `button`:

- **Choice panel**: up to four options, a small icon before an option that has a condition (a die for a check, a heart for romance, a lock for a flag-gated option, the voice's icon), a countdown bar for timed choices.
- **Dice roll**: the d20 rolling on screen with the modifier and the DC.
- **Approval toast**: "Sera approves" / "Kael disapproves", with the companion's bust.
- **Soul-Bond HUD**: the sigil in the corner of the party frame, in its five states (see 2.6).
- **Eclipse meter**: a sun disc with the stage shown as a grey ring, Act 2 only.
- **Protectorate map overlay**: the world map tinted violet where {sb} holds a region and gold where it's been liberated (A3-C3).
- **Act and chapter cards**: "Act 1 — The Hunger at the Heart", "Chapter 3 — Ashes at Thornbridge", in the Spectral title font.
- **Quest board screen**: three columns of posted contracts, the main quest's posting glowing.


## 7. Music and sound

New `Track` values, synthesised by `generate_audio.py` in the style of the existing three:

| Track | Name | Where |
| --- | --- | --- |
| `Sea` | "Salt and Lanterns" | The ship, the pier |
| `Court` | "Sunspire" | The keep, the Arcanum Spire |
| `Battle` | "Steel" | Combat |
| `Siege` | "Hold the Gate" | Greywatch, the Questeria plain |
| `Boss` | "The Hunger" | Boss arenas |
| `Doomward` | "Ash Orchard" | Doomward |
| `Morvaine` | "Tithe" | Morvaine, Hollowmere, the Glasswood |
| `Sunfall` | "Red Glass" | The Sunscar Waste and Sunfall |
| `Noxhallow` | "Frozen Falls" | Castle Noxhallow |
| `Protectorate` | "Order" | Questeria in Act 3: Hearthsmoke's melody as a slow march |
| `SoulBound` | "Two Names, One Oath" | {sb}'s theme: two voices in harmony. Played split and dissonant in Act 3, together again at the duel's last clash |
| `Vorlaith` | "Stopped Clocks" | The stinger: ticking that slows and runs backward |

Sound effects (`Sfx`): `shard-hum`, `voice-whisper`, `crystal-pulse`, `sigil-glow`, `sigil-break`, `box-open`, `reliquary-seal`, `eye-wake`, `sunbeam`, `bat-mist`, `freeze-time`, `time-tear`, `clock-stop`, `crowd-chant`, `alarm-bell`, `gate-break`, `heartbeat` (P-01: one heartbeat, then two in rhythm).

---

