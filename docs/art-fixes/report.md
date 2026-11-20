# Art, animation and room integration report

Branch: `codex/art-and-animation-fixes`, based on fetched
`origin/feature/realtime-combat` (`43a6892`). The separate checkout named in the
brief was read; implementation is in the ChatGPT workspace checkout.

## Changes

- `tools/weapon_alone.py`, `tools/import_parts.py` and the parts runtime now retain
  whole transparent held items, independent far-hand halves and shield kinds.
  Repainted damaged weapons, three clipped male hairstyles, a whole travelling
  cloak and two NPC boots. Preserved the female hair counterparts and intended
  stubble. All 127 held atlas parts match their standalone source transforms.
- `tools/rig/skeleton.py` cleans armour islands using joint-connected ownership;
  hand seeds use the wrist rather than the largest shape. Gloomfang and
  Thornmother frames have extra headroom without moving their ground anchors.
- Clips and `WalkerSprite` support timed `behind` windows. Near weapons draw
  before the head. Revised Riposte, Hex, Smoke Bomb and Unbroken Oath silhouettes,
  reached the Oath kneel during anticipation so it reads before the next auto,
  moved the cloak forward for Armour of Shadows, and made staff rotations
  continuous. Hit flash is a two-frame 65% white tint. Telegraphs and the red
  selected-target marker have smooth edges.
- All **74 background panels are fitted into all 37 game rooms**. Exact source
  hashes, reviewed door centres and floor lines are in `art/rooms/runtime-fit.json`.
  `tools/rooms/fit_backgrounds.py` fits them to unchanged gameplay geometry;
  minimum-error seams avoid broad ghosted joins. Painted floors cover the old
  gameplay tile strips. `layers.txt` prevents stale compiled parallax layers from
  appearing after an incremental build. `--proof <folder> Rooms` captures every
  room with a character for scale.
- README, Story/03, Story/11, Story/13, Story/14 and ASSETS document the pipeline,
  source prompts, credits, clip fields and room reproduction command.

## Checker comparison

| Measurement | Before | After |
|---|---:|---:|
| Atlas rectangles | 1179 | 1206 |
| Rig pieces with detached islands | 210 | 0 |
| Fragmented held items | 19 | 0 |
| Flat part edges | 5 | 0 |
| Creature frames touching an edge | 26 | 0 |
| Atlas bleed | 0 | 0 |
| Raw island flags (including head accessories) | 357 | 67 |
| Raw enclosed-gap flags | 65 | 52 |
| Raw colour flags | 732 | 809 |
| Unexpected findings after individual review | baseline unreviewed | 0 |

`baseline.json` and `after.json` contain the measurements. Colour detection also
flags natural dark outlines, leather and painted shadows; a flag is not proof of
a backing card. The remaining accessory islands and painted openings were
reviewed in atlas sheets and actor strips. Every allowance has a written reason
and exact RGBA fingerprint; changed pixels force review. No wildcard allowances.
The audit's older 225 count used another alpha threshold and is not interchangeable
with this baseline's 210.

## Verification and viewed evidence

- `check_parts.py`: 1206 rectangles, zero rig islands, zero fragmented held items,
  zero unexpected findings. Reviewed all held atlas sheets, flagged nonheld sheets,
  standalone weapons, Brannock/Sera/Kael strips, and elder/villager run and die
  strips. The elder's covered far-boot/skirt overlap stays beneath the full skirt.
- `clips.py --check`: all 320 clips, zero feet/joint problems, missing clips,
  missing hit/release events or look-alike pairs. `timing-check.json` confirms
  identical clip lengths and hit/release/recover times to `43a6892`, with LF-only
  JSON. Balance reruns are therefore not required. Five far-hand casts on both
  bases passed the staff continuity check (`staff-continuity.json`).
- `check_assets.py`: 1163 content files, 1767 sources and 62 scripts agree.
  Room validation: 19 zones, 37 rooms, zero problems or warnings.
- C# build, data validation and combat selftest passed. Final validation logs are
  under the ignored `shots/` folder.
- Viewed Voyage `rig-windup`, `rig-strike`, cast and hurt shots: faces stay clear.
  Viewed Moves party/weapon/creature shots: separate far-hand equipment and
  unclipped Gloomfang/Thornmother. Viewed Arena raid staff, gull cudgels and Smoke
  Bomb sequences. Viewed Boss phase/telegraph frames; fresh integrated captures
  show the smooth orange telegraph and red target marker over the new quarry art.
- Viewed all 75 actual runtime room captures, covering all 37 rooms, plus corrected
  full panoramas for the third-panel joins. Fresh Voyage, Boss, Paladin and Warlock
  captures use the integrated backgrounds. Armour of Shadows brings the intact
  cloak across the front. Class crowds can obscure poses; isolated actor strips
  supplement those captures. The focused `LQ_MOVE_ONLY=repaired-` Moves takes
  isolate Oath, Riposte, Hex and Smoke Bomb with the real runtime composer.

Several source-review/import attempts stopped before publication when they
exposed mask, cloak or wrist-seed problems. After the repairs, the ordinary full
parts importer completed successfully. Bounded scratch retries handled transient
OneDrive write locks; scratch runners and proof PNGs are not repository sources.

## Remaining decisions and scope

Hollowed faces retain the existing blank design pending the owner's decision.
Shields remain over the fist; moving them to the forearm needs a design choice.
The audit's HUD overlap, combo hit-stop synchronisation and downed-leader delay
were not changed in this art batch. Crowded battles can still obscure figures.
Illustrated door proportions and perspective are preserved, with centres and
floors fitted rather than repainting every doorway to a rectangular template.
The 22 optional foregrounds and 90 tile sources remain candidates for a separate
fitting pass. Original background sources are preserved.
