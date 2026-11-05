# Art overhaul

The game moves from 32x48 four-direction pixel sheets to high-resolution, side-view characters built from cut-out parts on a skeleton, with held and worn items that are real objects on the body. Everyone shares one base body and one set of drawing rules, so the cast reads as one world; the people who matter get parts nobody else has, so you can tell them apart in silhouette.

This file is the spec for characters, items, faces and backdrops. The world itself is now AQW-style painted rooms: `12-rooms.md` is their spec, and `03b-world-art.md` the list of buildings, props, effects, UI and music to paint into them. The build plan (`01-build-plan.md`, milestones M11 to M14) says in what order to build it and how to check each step.

---

## 1. The decisions

| | Now | After |
| --- | --- | --- |
| Characters | 32x48 sheets, ten columns, four facing rows, paperdoll layers baked per look | A 2D cut-out rig: about thirty parts on a skeleton, animated by keyframed clips, facing right and mirrored for left |
| Facings | Down, left, right, up | Right and left only. Walking up or down keeps the last horizontal facing |
| Resolution | 1 texel per world unit, point sampled | Characters authored 200 px tall for the 1600×900 screen and drawn 1:1, linear sampled. The world becomes painted rooms (`12-rooms.md`) |
| Poses | Five puppet-posed fighting columns | As many clips as the story needs (§4), shared by everyone |
| Items | Weapons and shields drawn into each build's frames | Any item can be put in a socket on the body (§6), so what someone carries in the story is what you see them carry |
| Faces | The standing frame's head, blown up | Twelve expressions from swappable eyes, brows and mouths (§5); portraits are the rig's bust, rendered live |
| Variety | One body, palette swaps | One base body in four builds, part kits per character, and four tiers of distinctness (§7) |
| Detail | Pixel shading, one tone of shadow | Painted parts with two shadow tones, a highlight and a rim light, and costume detail (trims, buckles, stitching, wear) to sit against detailed painted rooms |

**The author's direction (2026-11-04).** The artwork stays the game's current artwork, drawn in more detail, not a new style: the rig's parts are drawn from today's characters' designs, palettes and proportions, with this file's detail rules raised on top. Characters and enemies face only right or left; there's no front or back view to draw. NPCs keep walking. Where this file's style rules disagree with that, the direction wins.

Everything sits behind a switch, `ArtSettings.Characters = Legacy | Rig` and `ArtSettings.World = Legacy | Rooms`, read at start-up from `settings.json` and toggled in the options screen's debug page. Until M14 is done, both paths work, and the proof run (`--proof`) runs once in each.

## 2. Style

### 2.1 The angle

Every character is drawn once, in three-quarter view facing right. This is the angle Adventure Quest Worlds uses for its characters, and that game is the reference for the *base pose and proportions only*. Look at it to understand the angle. Never trace it, sample it, feed its images to an image generator, name it in a prompt, or ship anything derived from it. Everything in `tools/source/rig/` is made fresh.

What the angle means, measured on the standard build (§3.5):

- **Body.** Turned about 45° from the camera toward the right. The near shoulder is a little lower and further forward than the far one; the chest's centre line sits a third of the way across it from the near side.
- **Head.** Turned further toward the camera than the body, about 30° off full profile. Both eyes show. The far eye is narrower and partly behind the bridge of the nose. The near ear shows; the far one doesn't.
- **Hips and legs.** Closer to profile than the chest. Near leg in front and slightly forward, far leg behind. Feet point right and a little toward the camera.
- **Arms.** The near arm hangs in front of the body's near edge. The far arm is mostly behind the torso, with the hand showing past the hip.
- **Stance.** Feet about one head-width apart, weight on the back foot, a slight forward lean. Relaxed, ready.

### 2.2 Proportions

| | Standard build |
| --- | --- |
| Height, feet to crown | 200 px on the 1600×900 screen (about 22% of its height, close to the reference's proportion) |
| Heads tall | 5.5 |
| Shoulder width (as drawn, at the angle) | 1.6 heads |
| Hand length | 0.8 of a head. Hands and feet are drawn large, the way the reference does: they carry most of the acting |
| Foot length | 1.1 heads |
| Eye height | Half way down the head |

The other builds are bone scales of this one (§3.5), not separate drawings.

### 2.3 Rendering rules

1. **Outline.** Every part has a 4 px outer line in the part's darkest shade (never pure black), and a 2 px inner line where forms overlap inside the part.
2. **Shading.** Cel shading with hard edges: base, two shadow tones (a form shadow and a deeper occlusion shadow in creases and under overlaps), one highlight tone, and a thin rim light along the top-back edge of each part so characters separate from detailed backgrounds. No gradients on bodies or clothes. Glows, magic and light sources may use soft gradients.
3. **Light.** From above and slightly in front of the character, toward the camera. Never from the left or right. Shadow falls on the undersides of forms (under the chin, under the hat brim, the bottom of each sleeve), and highlight sits on the tops. This is what lets a right-facing drawing be mirrored for left without the light jumping sides. Note that it's a change from the current rule ("lit from the top left"), and that tiles keep their own rule (§11).
4. **Colour.** Each character's palette is no more than six hues plus skin and hair. Saturation tops out at the reserved hues (§7.3). Shadows shift toward violet-blue, highlights toward warm yellow, by the same amounts everywhere.
5. **Detail density.** The face and hands are the most detailed things on a character. Then the costume: every outfit has trims, seams, buckles or clasps, a pouch, charm or strap, and wear (scuffed leather, frayed hems, dents in plate). Cloth folds are few but drawn with both shadow tones. Armour gets a line per plate edge, a highlight per plate, rivets, and one engraved or etched detail on notable characters. Detail stays inside the silhouette: it never breaks the outline's read.
6. **Ground contact.** Every character casts the existing squashed-glow shadow under the feet, scaled with the build.

### 2.4 How it sits in the world

Characters stand on a room's floor band (`12-rooms.md` §1) at 200 px tall, with no scaling for depth. Each has a footprint ellipse (60×18 px for the standard build, scaled by build) used for sorting, click-picking and the shadow. In Legacy mode, on the old tile maps, the rig is drawn at 138 px so it stands three tiles tall as the sheets did; `WalkerShape` gets `RigScale` for that.

## 3. The rig

### 3.1 Space

- Rig space is in screen pixels at 1:1 on the 1600×900 canvas (the standard build is 200 px tall), origin at the point between the feet, x to the right, y down (the same as the screen).
- A character facing left is drawn with the root's x scale at −1.
- Every part is a rectangle from an atlas, drawn with `SpriteBatch.Draw` at its bone's world transform, with the part's pivot as the origin. Nothing is deformed or meshed: parts rotate, move and scale, and that's all.

### 3.2 Bones

Twenty-four bones. "Near" is the side nearer the camera; at this angle, facing right, it's the character's left. "Far" is their right.

| Bone | Parent | Pivot (A-pose, standard build, px from root) | Notes |
| --- | --- | --- | --- |
| `root` | — | (0, 0) | Placement, facing, the build's overall scale |
| `hips` | root | (0, −96) | Bob, sway, crouch |
| `spine` | hips | (0, −107) | Lower torso |
| `chest` | spine | (3, −136) | Breathing (scale y 1.00 to 1.02) |
| `neck` | chest | (6, −159) | |
| `head` | neck | (9, −168) | |
| `hair_back` | head | (−9, −183) | Hair behind the head; sways |
| `cape_1`, `cape_2`, `cape_3` | chest, cape_1, cape_2 | (−14, −154), (−17, −122), (−20, −87) | A three-link chain; lags behind movement |
| `arm_far_up` | chest | (−14, −151) | Far shoulder |
| `arm_far_low` | arm_far_up | (−17, −122) | Far elbow |
| `hand_far` | arm_far_low | (−17, −96) | Far wrist |
| `arm_near_up` | chest | (20, −148) | Near shoulder |
| `arm_near_low` | arm_near_up | (23, −119) | Near elbow |
| `hand_near` | arm_near_low | (26, −93) | Near wrist |
| `leg_far_up` | hips | (−9, −93) | Far hip |
| `leg_far_low` | leg_far_up | (−12, −49) | Far knee |
| `foot_far` | leg_far_low | (−12, −9) | Far ankle |
| `leg_near_up` | hips | (12, −90) | Near hip |
| `leg_near_low` | leg_near_up | (14, −46) | Near knee |
| `foot_near` | leg_near_low | (17, −7) | Near ankle |
| `tail_1`, `tail_2` | hips | (−14, −90), (−32, −72) | Wildkin and Emberkin only |

In the A-pose template the arms are out at 30° below horizontal and the legs a little apart, so every part can be painted whole and separate. The pivots above are where the joints are in that pose; `make_rig_template.py` draws them.

### 3.3 Parts and layer order

A **part** is one picture on one bone. A **slot** is a named place a part goes, so a slot can swap pictures (an open hand for a fist, a smile for a frown). Every slot has a fixed layer. Drawn back to front, facing right:

| Layer | Slot | Bone |
| --- | --- | --- |
| 0 | `cape_back_1..3` | cape_1..3 |
| 1 | `hair_back` | hair_back |
| 2 | `tail_1`, `tail_2` | tail |
| 3 | `item_hand_far` (a shield or off-hand item, under the hand) | hand_far, via the `hand_back` socket |
| 4 | `hand_far` | hand_far |
| 5 | `arm_far_low`, `arm_far_up` | |
| 6 | `item_arm_back` (a shield on the forearm) | arm_far_low |
| 7 | `foot_far`, `leg_far_low`, `leg_far_up` | |
| 8 | `item_back` (a sheathed sword, a quiver, a bow on the back) | chest |
| 9 | `foot_near`, `leg_near_low`, `leg_near_up` | |
| 10 | `hips` (with the skirt or coat tail) | hips |
| 11 | `spine`, `chest` | |
| 12 | `item_belt_back`, `item_belt_front`, `item_pouch` | hips |
| 13 | `neck` | |
| 14 | `item_neck` | chest |
| 15 | `head`, `ear_near` | head |
| 16 | `eyes`, `brows`, `mouth`, `face_extra` (beard, scar, spectacles) | head |
| 17 | `hair_front` | head |
| 18 | `headwear` | head, via the `head` socket |
| 19 | `arm_near_up`, `arm_near_low` | |
| 20 | `item_hand_front` (what the near hand holds) | hand_near |
| 21 | `hand_near` (the fingers drawn over the grip) | hand_near |
| 22 | `item_float` (something floating above the near hand) | hand_near |
| 23 | `cape_front` (a cloak's near edge, over the near arm) | cape_1 |

A clip can move a slot to another layer for its length (`"layer": {"arm_near_up": 6}`), for poses where the near arm goes behind the body, like the bow's draw.

### 3.4 Sockets

A socket is a named point on a bone, with an angle, where an item can be attached. These names are what the scripts' `hold <actor> <socket> <item>` cue uses.

| Socket | Bone | Where | Used for |
| --- | --- | --- | --- |
| `hand_front` | hand_near | the palm, grip axis along the fingers | weapons, letters, relics, lamps, cups |
| `hand_back` | hand_far | the palm | off-hand weapons, a lantern, the Eye in A3-C2-02 |
| `arm_back` | arm_far_low | outside of the forearm | shields |
| `back` | chest | between the shoulder blades, angled 30° | sheathed weapons, quivers, a bow, Leon's sword on Elara |
| `belt_front` | hips | near hip | keys, the outer key, a dagger |
| `belt_back` | hips | far hip | a vial, a flask, a scroll case |
| `pouch` | hips | front of the belt | the shard, the herring you took, small things |
| `neck` | chest | the collar | charms on cords: the herrings |
| `head` | head | the crown of the head | crowns, circlets, hats as items |
| `shoulder` | chest | the near shoulder | Burr; a pauldron trophy |
| `float` | hand_near | 26 px above the open palm | a relic held up by magic (Maleficio's Box, the shard rising in A3-C2-02) |

Props have sockets too, defined in the prop's catalog entry: `top` on a pedestal or a table, `seat` on a throne, `arm` on a throne's arm.

### 3.5 Builds

A build is a set of bone length and part scales over the standard drawing, so every kit fits every build without being redrawn.

| Build | Who | Legs | Torso | Arms | Head | Width |
| --- | --- | --- | --- | --- | --- | --- |
| `standard` | Humans, Sylvans, Emberkin, Wildkin | 1.00 | 1.00 | 1.00 | 1.00 | 1.00 |
| `stout` | Stoneborn | 0.78 | 0.92 | 0.90 | 1.00 | 1.25 |
| `small` | Hearthlings, children | 0.70 | 0.78 | 0.75 | 0.92 | 0.90 |
| `tall` | Severin, Crane, Varkhul, Vorlaith, constructs | 1.14 | 1.08 | 1.10 | 0.96 | 0.94 |

Width scales each part's x. A part may have a build-specific override (`torso@stout`) where scaling looks wrong; the kit loader prefers an override when one exists.

### 3.6 Peoples

Race parts are kits like any other: `ears-sylvan`, `ears-wildkin-wolf`, `ears-wildkin-hawk`, `ears-wildkin-bear`, `horns-emberkin-curled`, `horns-emberkin-swept`, `tail-wildkin-wolf`, `tail-emberkin` (with a looping ember particle at the tip). They sit in the `ear_near`, `headwear`-under and `tail` slots.

### 3.7 Equipment

`ItemDefinition` gets `Visuals`, replacing `Layer`:

```csharp
public sealed record ItemVisuals(
    IReadOnlyDictionary<string, string> Parts,   // slot -> part name in its kit, e.g. "chest" -> "chain-shirt/chest"
    string Socket = null,                        // for held items: "hand_front", "arm_back", "back"...
    string Grip = "fist",                        // the hand_near part it needs: fist, grip-wide, pinch, open
    string HideSlots = null,                     // comma list: a great helm hides "hair_front,hair_back,ear_near"
    string Tint = null);                         // the gear-palettes ramp, as now: "gold", "blackened"
```

The nine armours, three helms, gloves, gauntlets, two boots, the cloak, two shields and thirteen weapons all become kits under `Content/Rig/Gear/`. Cloth gear keeps the vest's tint, metal and leather keep their own, exactly as now (§3.8).

### 3.8 Colour and tints

Painted parts can't be palette-swapped key by key the way the pixel sheets were: anti-aliased edges blend the keys. So each tintable part comes as two pictures:

- `<part>.png`, the full-colour part, with every tintable area painted in a neutral grey ramp.
- `<part>.mask.png`, the same size, where the red channel says which tint group a texel belongs to (skin 1, hair 2, eyes 3, shirt 4, vest 5, legs 6, cloak 7, metal 8, leather 9; 0 means keep the painted colour) and alpha is the coverage.

`RigTint` bakes a look's parts once per change: for each texel in a tint group, its grey's brightness looks up the chosen ramp in `character-palettes.png` or `gear-palettes.png`, as the gradient-map does in `generate_paintings.py`. The ramps are widened from five to nine steps for the softer shading. The result is cached by (kit, part, ramps), so two people dressed alike share textures, as `Paperdoll.Key` does now.

## 4. Animation

### 4.1 Clips

A clip is a JSON object: a length, whether it loops, keyframes per bone (rotation in degrees, offset in texels, scale), part swaps at times, layer changes, and events.

```json
{
  "name": "walk",
  "length": 0.8,
  "loop": true,
  "bones": {
    "hips":        { "y":   [[0, 0], [0.2, -3], [0.4, 0], [0.6, -3], [0.8, 0]] },
    "leg_near_up": { "rot": [[0, -24], [0.4, 22], [0.8, -24]] },
    "leg_far_up":  { "rot": [[0, 22], [0.4, -24], [0.8, 22]] },
    "arm_near_up": { "rot": [[0, 14], [0.4, -12], [0.8, 14]] }
  },
  "swaps":  [[0, "hand_near", "relaxed"]],
  "events": [[0.2, "step"], [0.6, "step"]],
  "ease": "sine"
}
```

Clips are written as Python in `tools/rig/clips.py`, which writes `Content/Rig/clips.json`, so they're readable, diffable and seeded like every other generator. A clip only lists the bones it moves; the rest hold the rest pose. Playback blends 0.12 s between clips. `step` events fire the footstep sound, as `WalkerSprite.Stepped` does now.

A clip can be **two-actor**: it names a partner and a meeting offset (`"partner": {"offset": [34, 0], "clip": "shake-hands-b"}`), and the cue runner walks the two actors to that offset before playing both halves together.

### 4.2 Combat clips

| Clip | Weapon classes | Notes |
| --- | --- | --- |
| `ready` | all | The fight idle. Replaces column 0 in a fight |
| `windup` / `strike` | `one-hand`, `two-hand`, `polearm`, `dagger`, `unarmed` | Per class, as `windup-two-hand`. The strike's `hit` event is when damage numbers show |
| `draw` / `loose` | `bow`, `crossbow` | Takes an arrow from the `back` socket's quiver |
| `cast` / `channel` | all | Both arms forward; `channel` loops |
| `block` | shield, weapon | |
| `hurt` | all | Knocked back a step |
| `downed` | all | On the back, the arms out. Replaces column 9 |
| `die` | all | `downed` and a fade, for foes |
| `revive` | all | Up on one knee, then standing |
| `yield` | all | Kneeling, the weapon down. For nonlethal fights |
| `victory` | all | Brief; one per class |

### 4.3 Cutscene clips

Every `anim <actor> <name>` in the scripts needs a clip. This is the full list, grouped. Where a name is an alias, the script name plays the listed clip. `tools/rig/check_clips.py` reads every script file and fails if a name isn't here.

**Standing, moving, falling**

| Clip | Aliases | Notes |
| --- | --- | --- |
| `idle` | `stand-still`, `hold` | Breathing, a blink every 3 to 6 s |
| `walk`, `walk-slow`, `run` | `run-in`, `leave` (walk off screen), `stand-beside` (walk to the partner's side), `step-forward` (one step) | |
| `sit` | `sit-fire` (sitting on the ground), `sit-up` (from lying to sitting) | Needs a `seat` or the ground |
| `stand` | `rise` | From sitting or kneeling |
| `kneel` | `fall-kneel` (fast, from standing), `kneel-smoking` (with smoke particles) | |
| `lean` | `lean-rail` (on a prop's edge) | |
| `turn` | | Flips facing over 0.15 s with a squash |
| `bow` | `bow-head` (a nod only) | |
| `look-up` | `face-sun` (eyes shut, chin up), `stand-sun` | |
| `look-back` | | Head and chest turn over the far shoulder |
| `hide` | | Crouched |
| `leap` | `dive` (head first) | |
| `land` | `drop-in` | |
| `stagger` | | |
| `fall` | `slip` (losing a grip), `slump` | |
| `slide-to` | | Pulled along the ground toward a point, arms out. The point is the cue's target |
| `hold-throne` | | Holding a prop's leg, lying down |
| `scream` | | Head back, hands to the head |
| `wake-gasp` | | From lying to sitting, fast |
| `march-in-place` | | For the relief column's echo |
| `frozen` | | Holds the current pose, stops the blink, tints violet (§8). Used on every stopped-time citizen |
| `chained` | `break-chains` | A pose with the arms up and out; `break-chains` brings them down |
| `arm-sling` | | An idle with the near arm in a sling part swap |

**Hands and items** (these all expect an item in a socket and move it)

| Clip | Aliases | Notes |
| --- | --- | --- |
| `point` | | |
| `raise-hand` | `stop` (palm out at someone) | |
| `show-palm` | `look-palm` (looking at your own) | Open near hand, palm up, at chest height. Sigil cues draw on the palm here |
| `close-fist` | | |
| `offer-hand` | | |
| `reach` | `reach-crown`, `touch-crown`, `crown-self` (both hands to the head, then down) | |
| `give-item` | `hand-over` | Two-actor. Moves the item from the giver's `hand_front` to the taker's at the `pass` event |
| `pick-up` | `grab-item`, `snatch` (fast) | Takes the cue's item from a prop socket or the floor into `hand_front` |
| `place-item` | `lift-into` (into a prop socket that's higher) | |
| `lift-item` | `raise-item`, `lift-ledger`, `hold-out` | Raises `hand_front` to head height |
| `lift-both` | | Both hands up, an item in each |
| `throw` | `throw-fire`, `burn-book` | Releases the item at the `release` event; the item flies on an arc to the target |
| `uncork` | `fill-vial` (with the other hand), `cut-palm` (the knife in `hand_front`) | |
| `untie-cord` | | Both hands to the neck socket; the item comes off at the end |
| `drink-flask` | `pour-wine` (into a cup on a prop) | |
| `read` | `read-roll`, `inspect`, `look-at-shrine` | |
| `sign` | | Writing at a desk |
| `bind-book` | | |
| `mend` | `whittle`, `clean-blade`, `scratch-shield`, `stuff-satchel` | Seated or standing small-hands loop; the item is whatever's in the hands |
| `flip-coin` | | |
| `open-parcel` | | |
| `unlock` | | |
| `pickpocket` | | Two-actor, small build |
| `hood-back` | | Swaps the headwear part at the half-way point |
| `touch-burn` | `cough`, `breathe-in` | A hand to the face or chest |
| `heal` | `cast` | The gold glow particles on both hands |
| `drain` | | `channel` with red particles toward the caster |
| `chant` | | `channel` for a group |
| `address-crowd` | `climb-cart` (first steps up onto a prop) | Arms open, chin up |
| `pound-wall` | | Fists on an invisible wall |
| `bar-door` | `set-shield`, `shield-cover` (with a partner behind) | Shoulder into a prop or shield up |
| `blade-at-throat` | | Two-actor with the partner kneeling |
| `pommel-strike` | `strike` | |
| `draw` | | Weapon from `back` or belt to `hand_front` |
| `lower-weapon` | `lower-knives`, `bow-lowered` (Sera's pose with the bow at the hip) | |
| `dub` | | Two-actor, the sword tapping each shoulder |
| `yield`, `hurt`, `downed`, `die`, `catch` | | The combat clips; `catch` is the hurt clip reversed with the arms forward |
| `burn` | | `hurt` with fire particles |
| `ash` | `consumed` | `die`, then the parts dissolve upward as ash; `consumed` pulls them toward a target instead |

**Two actors**

| Clip | Aliases | Notes |
| --- | --- | --- |
| `shake-hands` | | |
| `take-hand` | `catch-hand`, `touch-fingers`, `touch-hand` | The near hands meet. `touch-fingers` stops short |
| `clasp-hands` | | Forearm clasp. Plays the sigil glow on both palms |
| `hand-on-shoulder` | `hand-on-bark` (on a prop) | |
| `hug` | | |
| `grab-arm` | `grab-wrist` | |
| `tackle` | | |
| `cloak-around` | | Wraps the cape parts round the partner; both fade |
| `let-go` | | The end of `take-hand`, with the far actor sliding away |
| `hesitate` | | For the child on the pier: a step forward and a step back |

**Props and things that aren't people.** These play on a prop's own animation strip, not a rig: `crystal-break`, `split` (the crown), `shrink` (the tear), `pull` (the tear's suction, a loop), `float-free` (the Eye rising), `lean-toward` (the relic cases tilting), `spin` (the Box), `cross` (the leviathan's shadow), `land` on Burr (a small flier rig, §9).

### 4.4 The cue verbs that touch art

| Cue | Art it needs |
| --- | --- |
| `place <actor> at <spawn>` | The map's spawn points; nothing new |
| `walk`, `face`, `travel-actor` | Clips above |
| `hold <actor> <socket> <item>` / `drop <actor> <socket>` | The item's HD picture and its grip (§6) |
| `light <thing> <state>` | The item's or actor's glow states (§6.3) |
| `sigil <actor> <state>` | The palm sigil's states: `on`, `off`, `glow`, `gold`, `violet`, `grey`, `cold`, `flicker-gold`, `flicker-violet`, with `1 frame` meaning a single 1/30 s flash. Drawn on the near hand in `show-palm`, and as the HUD icon |
| `swap <actor> <look>` | A whole alternative look for the actor (Sister Vale to Valencia unhooded, Thessaly to crystal, Maleficio to the Hungering, Varkhul unmasked) or a prop's alternative picture (Brannock's shield with 1 and 41 names) |
| `echo <actor>` | The memory shader (§8) |
| `reflection <actor> delay <s>` | A second copy of the actor in the nearest `mirror` prop, its pose `s` seconds behind |
| `trail <actor> <colour>` | Afterimages of the actor's last 6 poses at 0.05 s intervals, fading |
| `backdrop <id>` / `backdrop off` | A painted backdrop (§10) |
| `tint <id>` | A world tint (§8) |

## 5. Faces

### 5.1 Parts

Each head kit has these slots and pictures:

- `eyes`: `open`, `half`, `closed`, `wide`, `narrow`, `down`, `up`
- `brows`: `level`, `raised`, `knit`, `sad`, `one-up`
- `mouth`: `closed`, `smile`, `grin`, `open`, `frown`, `grit`, `smirk`, `flat`, plus `talk-a` and `talk-b` for each of `closed`, `smile` and `frown`

### 5.2 The twelve expressions

These are every `(expr)` used in the scripts. `afraid` and `worried` are kept separate: worried is for someone else; afraid is for yourself.

| Expression | Eyes | Brows | Mouth | Notes |
| --- | --- | --- | --- | --- |
| `neutral` | open | level | closed | Default |
| `smile` | open | level | smile | |
| `grin` | half | raised | grin | |
| `laugh` | closed | raised | open | Head bob, 2 cycles |
| `smirk` | narrow | one-up | smirk | |
| `sad` | down | sad | frown | |
| `worried` | open | knit, sad | flat | |
| `afraid` | wide | sad | open | |
| `angry` | narrow | knit | grit | |
| `cold` | narrow | level | flat | The {sb} of Act 3. No blink while it's held |
| `tired` | half | level | flat | |
| `shock` | wide | raised | open | |

A missing picture falls back down this chain: the expression's own, then `neutral`'s for that slot. A kit that only has `open`, `closed`, `level`, `closed` (mouth) and `smile` still works.

### 5.3 Talking and blinking

While a line types, the mouth alternates the current shape with its `talk-a` and `talk-b` at 10 Hz, closing on punctuation. A blink is `half`, `closed`, `half` over 0.12 s, every 3 to 6 s, suppressed by `cold`, `shock` and `frozen`.

### 5.4 Portraits

The dialogue box's portrait is the speaker's rig, drawn live into a 320x320 render target, cropped from the top of the head to mid-chest, at the expression the line asks for, with talking and blinking. Speakers on the left of the box face right; on the right, they're mirrored. Held items show if they're in the crop (the shard in {sb}'s hand on the roof, the letter in Corra's). `VOICE(x)` uses `x`'s portrait tinted violet at 70% with a 2-texel echo offset and no name. A speaker with no rig on the map (`CORRA [letter]`, `NARRATOR`) uses their rig if they have one and no portrait if they don't.

## 6. Live items

### 6.1 The rule

A story item that has a picture is a real object. If someone holds it in a scene, it's in their hand. If it's in a party member's bag and it has a home socket, they're wearing it: on the map, in fights (unless the socket is a hand and the fight needs both), in cutscenes, and in their portrait. When it changes hands in the story, it changes hands on screen.

### 6.2 How

- `ItemDefinition` gets `Live` (bool), `HomeSocket` (string, optional), `Picture` (the HD sprite, 3x), `Grip` (as in §3.7) and `States` (§6.3).
- Each `Actor` (party member, NPC or rig prop) has a `Sockets` dictionary: socket name to item id.
- `hold a s i` puts `i` in `a`'s socket `s`. `drop a s` empties it. If `a` is a party member, `give`/`take` effects move the bag item, and the sockets follow: a `give` of a live item with a home socket puts it there unless a `hold` says otherwise in the same scene.
- After a scene, every party member's sockets are recomputed from their bag and equipment, except items marked `Sticky` by a `hold` during the scene (the outer key on {sb}'s belt from A2-C6-06 on, the crown on {sb}'s head in Act 3), which stay until another cue moves them.
- The save stores each actor's sockets.

### 6.3 States

Some items change: `light <item> <state>` sets a state, which picks a picture and a glow (a tinted `glow.png` drawn additively, with a pulse rate).

### 6.4 The catalog

Every item a script holds. Sizes are in px at character scale (a standard-build character is 200 px tall).

| Item | Home socket | Size | States | Look | Scenes |
| --- | --- | --- | --- | --- | --- |
| `herring-charm` | neck | 20x12 | — | Driftwood herring, knife-cut scales, waxed cord. You wear one, {sb} wears its twin | P-04 on; {sb}'s comes off at A3-C2-04 |
| `herring-new` | neck | 20x12 | — | The same, the cuts paler and newer | A3 birthday |
| `herring-pair` | neck | 26x14 | — | Two herrings on one cord, knocking together (a 2-frame sway) | A3 birthday, if worn |
| `sb-herring` | pouch | 20x12 | — | {sb}'s herring, the cord knotted twice | A3-C2-04 to A3-C5-04 |
| `tomas-hook` | belt_front | 12x17 | — | Brass fish hook on a cord | A1-C1-01 on |
| `whittling-knife` | belt_back | 26x9 | — | Short knife, a cord through the handle | {sb}'s, A1 and A2 |
| `shard` | pouch | 14x20 | `on`, `off` | Black splinter, violet edge glow when on; a faint violet light from the pouch on dark maps from A1-C6-05 | A1-C6 to A3-C2-02 |
| `shard-sliver` | — | 9x12 | — | A smaller splinter | A1-C6-03 |
| `black-box` | — | 32x32 | `on`, `off` | Palm-sized lightless cube, faint seams, a hum ring when on | A1-C8, A3-C2-02 |
| `black-iron-coin` | — | 9x9 | — | Black coin stamped with a cube | A1-C4 |
| `black-iron-blade` | — | weapon | — | Doomward black iron, a cube on the pommel | A1 |
| `cube-seal-ring` | — | 9x9 | — | Crane's signet, a cube | A1-C7-03 |
| `sunheart` | — | 20x20 | `on` | Gold gem, warm inner light | A1-C4 |
| `leon-sword` | back (on Elara) | weapon, unique | — | Long, plain, very old. Blue leather grip worn pale. A small gold sun on the crossguard | A1-C4-03 on Leon; A3-C2-03 you; A3-C2-05 on Elara to the end |
| `brannock-shield` | arm_back | 58x64 | pictures `0`, `1`, `41`, `42` names | Battered round shield, iron rim. Names scratched round the rim, more each version | Always; texture by the name count |
| `outer-key` | belt_front | 12x26 | — | Heavy gold key, a sun bow | A2-C6-06 on {sb}, sticky |
| `eye-of-solvaris` | — | 26x26 | `asleep`, `gold`, `gold-violet`, `red-black` | Fist-sized gold lens in a cage of old gold | A2-C4 to A3-C2-02 |
| `crown-of-thorns` | head | 38x17 | — | Severin's iron thorn circlet | Act 2 |
| `crown-dusk-dawn` | head | 38x20 | `on`, `violet-flare` | Black iron braided with gold glass, the shard at the front | A3-C2-02 to A3-C5-03, sticky |
| `crown-box-half`, `crown-eye-half` | — | 20x20 | `gold` (the eye half) | The crown split down the middle, jagged | A3-C5-03/04 |
| `valencia-vial` | belt_back | 9x20 | `gold` | Small glass vial of dark blood with a gold light in it | A3-C3-02 to A3-C5-02 |
| `corra-letter-1..3` | pouch | 17x14 | — | Folded sheet, a fishing-net watermark | A1-C7, A2, A3 |
| `harrow-petition`, `harrow-notice`, `timber-writ`, `royal-writ`, `protectorate-writ`, `wardens-charter`, `curfew-roster`, `sun-scroll`, `abbey-letter` | — | 20x26 | — | Documents. One shared picture with six seal variants: heron, admiralty anchor, Aurelian sun, Protectorate violet, the Lodge's leaf, the abbey's bee | various |
| `ledger`, `hollowing-ledger`, `roll-of-living`, `index-half`, `sealed-index`, `notes`, `letter-bundle` | — | 23x26 | — | Books and bundles. Four shared pictures, tinted | various |
| `smoke-pot` | — | 14x20 | `on` | Beekeeper's smoker, a wisp loop | Orla |
| `lamp`, `lantern` | hand_front | 14x23 | `on` | | Kael in the sewers; thralls |
| `kael-knives` | hand_front | weapon pair | — | Twin long knives, red cord on the grips | Kael |
| `corwin-sword` | hand_front | weapon | — | Gilded Morning longsword | Corwin, then Davin |
| `apple`, `wine-bottle`, `honey-cakes`, `grain-sack`, `tithe-cup`, `crate`, `parcel`, `whetstone`, `ink-pen`, `shawl`, `wooden-sword`, `sun-token`, `blade` | — | small | — | Generic small props for hands | various |
| `light-glow` | hand_front | — | — | Not an item; Mira's healing glow, a particle emitter on the socket | Mira |

### 6.5 What the player sees

At least these must be visible, and are the M12 check (build plan): the herring on your neck in every scene from P-04; the shard's violet glow in {sb}'s pouch on dark maps from A1-C6-05 (and nobody remarking on it until Valencia); the outer key on {sb}'s belt from A2-C6-06; the crown in A3; Leon's sword on Elara's back from A3-C2-05 to the coronation; Brannock's shield's names; the vial on your belt until the summit.

## 7. Who looks like what

### 7.1 Tiers

| Tier | Who | Distinctness |
| --- | --- | --- |
| **Hero** | You and {sb} | The player's look, built from the creation screen's choices on the base kits. Outfits change by act (§7.2) |
| **Notable** | The six companions, Elara, and every character in §7.4 | At least one unique kit (a head, a hairstyle, a costume piece), a reserved hue or hue pair (§7.3), and a **silhouette element**: something you can recognise in a black fill at portrait size. Full expression set |
| **Named minor** | Anyone named who speaks more than twice: Tomas, Fennick, Brisk, Odelle, Hedda, Tamsin, Hobb, Garrick, Maren, Pell, Ide, Wren, Mags, the recruit | Pool kits, one unique item or hat, a fixed palette. Six expressions (neutral, smile, sad, worried, angry, shock) |
| **Generic** | Townsfolk, guards, soldiers, cultists, pilgrims | Generated from a pool per culture (§7.5), seeded by the NPC's id so they're the same every time. Neutral, smile, worried |

### 7.2 You and {sb}, by act

| | Act 1 | Act 1 end (Champions) | Act 2 end | Act 3 |
| --- | --- | --- | --- | --- |
| You | Traveller: whatever the creation screen picked | Champion's tabard over it, gold Soul-Bond trim | Same | Same, with mud on it from the camp after A3-C2-05; the tabard's gold trim goes grey with the sigil |
| {sb} | Traveller | Champion's tabard | Lord Protector's coat: long, high-collared, deep blue with violet lining that shows when {sb} moves | Sovereign's armour: dark plate with violet-gold edging, the coat's collar kept, the crown |

{sb}'s violet increases in steps, and nothing else in the game uses violet for a person until the Protectorate's tabards in Act 3, which take it from {sb}.

### 7.3 Reserved hues

| Hue | Reserved for |
| --- | --- |
| Violet (#7a3fb0 family) | The shard, the Box's glow, {sb} from A1-C6-05, the Voice, the crown, the Protectorate in Act 3, stopped time |
| Sun gold (#e8b84a) | The Aurelian crown: Leon, Elara, the Eye's good light, the Soul-Bond sigil |
| Wine red (#7d1f2e) | Morvaine: Severin, Valencia (after the habit), thralls |
| Guild cobalt (#2c4fa3) | The Mages Guild: Thessaly, battlemages |
| Watch navy and brass | Hale and the City Watch (Act 1 and 2) |
| Ember orange (#d9662b) | Maleficio, the Hollowed's eye light, Kael's tail |
| Moss green | Sera, the Rangers' Lodge |
| Heron green and grey | Harrow and his men |

Generic NPCs are generated without these hues. The generator rejects a palette that comes within 12 ΔE of a reserved hue.

### 7.4 Notable characters

| Character | Build | Silhouette element | Hue | Unique kits | Variants |
| --- | --- | --- | --- | --- | --- |
| **Sera** | standard, Sylvan | One long braid to the waist; a longbow on her back taller than her head | Moss green, undyed leather | Head, braid, short ranger's cape | — |
| **Mira** | standard | A satchel with three scroll tubes; hair tied up with a pen through it | Ink blue, parchment | Head, hair, robe, satchel | Healing glow on both hands |
| **Kael** | standard, Emberkin | Swept horns through a pointed hood; ember tail | Charcoal, ember-red sash | Head, hood, sash, coat | `kael-shadow` (Protectorate colours, Act 3) |
| **Brannock** | stout, Stoneborn | The shield; a braided beard to the belt with two gold clasps (none after A3-C2-05) | Iron grey, tarnished gold | Head, beard (with and without clasps), plate | `arm-sling` part |
| **Valencia** | standard | Act 2 early: the abbey habit's square hood. After: a high, stiff collar and long gloves | Bone white, wine red | Head, habit, coat, collar | `sister-vale`, `valencia-unhooded` |
| **Elara** | standard | A short cape on one shoulder; Leon's sword on her back in Act 3 | Royal blue, sun gold | Head, cape, riding coat | `elara-hooded` (A1-C4-01), crowned (A3-C5-06) |
| **Leon** | standard | Broad ermine collar; a small plain gold crown | White, sun gold | Head, beard, robes | Ill, in furs (Act 3) |
| **Crane** | tall | Very long coat, very thin; rings that catch light | Plum, silver | Head, coat, slicked hair | — |
| **Hale** | standard, broad (width 1.1) | Watch helm with a crest brush | Watch navy and brass; Protectorate violet in Act 3 | Head, helm | Protectorate tabard |
| **Thessaly** | standard | Cropped grey hair; a staff with a ring at the top; two fingers missing on the near hand | Guild cobalt | Head, robe, staff, hand | `thessaly-crystal` |
| **Maleficio** | standard | Scholar's robes with a stiff high back; the Box floating over his palm | Charcoal with ember edges | Head, robes | `maleficio-young` (echo), `hungering` (swap) |
| **Varkhul** | tall | Hollowed plate with a cracked visor; greatsword with the shard in the hilt | Ash, ember eyes | Whole kit | `varkhul-unmasked` |
| **Brenna** | standard | One cracked knight's pauldron; an ash-grey cloak | Ash grey, Doomward black | Head, pauldron, cloak | Act 3 Protectorate sash if `ashfall-loyal-sb` |
| **Aldric** | standard | Worn plate, an old campaign cloak, grey stubble | Steel, faded red | Head, cloak | — |
| **Severin** | tall | Silver hair to the shoulders; the thorn crown; a long black velvet coat with a cape that spreads like wings in fights | Black, wine red | Whole kit | `severin-boss`, `severin-cell` (thin, grey, plain shirt) |
| **Isadora** | standard | A long gown with a train | Midnight blue (echo-tinted always) | Whole kit | Echo only |
| **Corwin** | standard | Gilded plate; a sun tabard; a tall plume | Gold-white | Head, plate | — |
| **Davin** | standard, young | Corwin's tabard cut down to fit; no plume | Gold-white | Head | Carries `corwin-sword` |
| **Anselle** | standard | Abbey habit with no hood; a heavy key ring | Grey, white | Head, habit | — |
| **Orla** | standard | A bee veil on a wide hat | Honey yellow, brown | Hat, veil | — |
| **Vesk, Vreya, Mordain** | standard | Black plate with red trim; helms shaped as a wolf (Vesk), a crown (Vreya), a beak (Mordain) | Black, wine red | Helms | — |
| **Gannet** | standard, wide (1.2) | Heavy shawl; a ledger always under the arm | Mustard, brown | Head, shawl | — |
| **Corra** | standard | A net-mender's shawl and a ball of twine at the belt | Cove blue, oiled brown | Head, shawl | Flashback version, 16 years younger |
| **Hester** | standard | Grey bun, a shawl over both shoulders | Heather | Head | — |
| **Tobin** | standard | A cider-presser's apron; the burn across the back of his near hand | Apple green, brown | Head, hand | Levy coat |
| **Harrow** | standard | Green coat with a grey heron on the breast | Heron green and grey | Head, coat | — |
| **Grell** | standard, wide (1.25) | Bald; a hook for a hand | Red Gull red | Head, hook hand | — |
| **Vorlaith** | tall (×1.2) | Body of cracked glass with light moving inside it; crown of broken clock-hands | Colourless glass, white light | Whole kit | `vorlaith-shape` (A3-C1-01: the same, at 40% opacity) |
| **The children on the pier** | small | Fishing-town kids | Cove blue | Two heads | `mc-child`, `sb-child`: the player's and {sb}'s looks, at eight, on the small build |

### 7.5 The generic pools

Seven cultures, each a set of kits: 6 heads, 6 hairstyles, 4 upper bodies, 3 lower bodies, 4 hats, with the culture's palette ramps.

| Culture | Where | Feel |
| --- | --- | --- |
| `questeria` | The city, the docks | Bright, layered cloth; aprons, waistcoats |
| `saltwind` | The docks, the coast | Oilskins, caps, rolled sleeves |
| `doomward` | Ashfall, the levies | Grey and black, patched, practical |
| `morvaine` | Hollowmere, the pass | Dark wool, high collars, pale skin ramps |
| `abbey` | Saint Lumen's | Habits, aprons, sun hats |
| `order` | The Gilded Morning | Gilded plate and tabards (knights, sergeant) |
| `protectorate` | Act 3 everywhere | Violet-and-gold tabards over the Watch's kit |

`tools/rig/generate_pool_npcs.py` picks a head, hair, body, legs, hat and palette for each NPC id, seeded, and writes `Content/Rig/npcs.json`. A map NPC with no entry gets one picked at load the same way.

### 7.6 The lineup proof

`--lineup` renders every notable character and a sample of each pool, side by side, standing, at portrait scale and at map scale, to `shots/lineup.png`, then the same in black silhouette to `shots/lineup-silhouette.png`. `tools/rig/check_lineup.py` measures each pair of notable silhouettes' overlap (intersection over union, after aligning the feet) and fails if any pair is above 0.88. The two images are the review: in the silhouette one, every notable character must be nameable.

## 8. Effects on characters

All of these are shader-free CPU or `SpriteBatch` tricks, so they run on every platform MonoGame does:

- **Frozen** (stopped time): a violet multiply at 60% and a 1-texel violet outline; no animation, no blink. Snow particles near them stop.
- **Echo** (memories): drawn twice, at 55% opacity in midnight blue, and again offset 2 texels at 25%, with a 0.3 s flicker every few seconds.
- **The Voice**: the listener's portrait at 70% violet, the line's text in italic with a 2-texel shadow copy.
- **Trail**: §4.4.
- **Ash** and **consumed**: each part becomes 6 particles of its average colour, drifting up (ash) or toward the target (consumed).
- **World tints** (`tint <id>`): a full-screen colour multiply over the world layer and characters, under the UI. The list is in `03b-world-art.md` §5, World tints.

## 9. Creatures

Four creature rigs share the same machinery with their own bones:

| Rig | Bones | Who |
| --- | --- | --- |
| `quadruped` | root, body, neck, head, jaw, tail ×2, four legs of two segments and a foot | Gloomfang, rot-boar, ash-hound, Duchess the donkey; the Thornmother on it at scale 2.2 with a hump |
| `flier` | root, body, head, two wings of two segments, tail | Dire bat, Burr (small), the leviathan (scale 12, only as a shadow) |
| `arthropod` | root, body, tail ×4, two claws, six legs of one segment | Glass scorpion |
| `humanoid` | The person rig | Hollowed (footman, hexer, sapper, climber, crown-raised), thralls, drowned, blood-wights, sun-scorched, constructs (on `tall`), the Unmade |

Some things aren't rigged: the sea-wraiths and the Hungering are layered sprites that move as a whole with a wobble, and the Hollowed ram beast is a 4-frame sheet. They keep the old sheets, redrawn at 3x.

New enemies the encounters and scripts need, with their look in one line each:

| Id | Rig | Look |
| --- | --- | --- |
| `rot-boar` | quadruped | Bristled boar, black veins under the hide, red eyes |
| `thornling` | humanoid, small | Bark-skinned sprite with thorn fingers |
| `rockjaw` | quadruped, stout | A boulder with a jaw; moss on its back |
| `ash-hound` | quadruped | Grey dog the colour of the ground, ember eyes |
| `cube-cultist` | humanoid | Black robe, a cube pendant, ash on the face |
| `drowned` | humanoid | Grey-green, kelp hair, waterlogged clothes |
| `dire-bat` | flier | Wingspan of a tall man, ragged ears |
| `tithe-rider` | humanoid on quadruped | Morvaine rider in dark wool on a black horse |
| `blood-wight` | humanoid, tall | Thin, grey, a red glow in the chest |
| `sun-scorched` | humanoid | Old priest's robes burned to the skin |
| `glass-scorpion` | arthropod | Red glass shell, a sun-bright sting |
| `protectorate-watchman`, `protectorate-warden` | humanoid | Pool `protectorate`; the warden in heavy plate |
| `crown-hollowed` | humanoid | Hollowed in Protectorate sashes, violet eye light |
| `crown-construct` | humanoid, tall | Violet-gold metal, a crown motif for a face |
| `unmade` | humanoid | A soldier from any age with no face: smooth dark glass where it would be (4 armour variants) |

## 10. Backdrops

A backdrop is a full-screen painting shown behind (or instead of) the map for a moment. The current paintings are 416x277 at 4x; the new ones are 1600x900 at 1x, in three to five layers that drift against each other as the title's do. Every `backdrop <id>` in the scripts:

| Id | Scene | Layers |
| --- | --- | --- |
| `pier-memory` | P-03, A3-C5-07: the pier in the storm, a boat over, a child in the water | storm sky, sea, pier, the two children |
| `pier-fire` | P-03: Corra wrapping two soaked children in one blanket by a fire | room, fire, figures |
| `red-moon` | P-03: two teenagers under a red moon, palms cut | sky, moon, figures, glow |
| `questeria-dawn` | P-05: the city from the ship's rail | sky, sea, city, spire, rail |
| `maleficio-sky` | A1-C6-02: Maleficio's face in the clouds over Greywatch | sky, clouds, face, fortress |
| `eclipse` | A2-C1-01: the grey ring over the sun at the festival | sky, sun, ring, lanterns |
| `purging-dawn` | A2-C4-02: the Sunfall mural | one aged mural |
| `purging-dawn-mirage` | Sunscar mirage: the same scene shimmering, half-transparent | heat haze, mural figures |
| `eye-wakes` | A2-C4-03: the Eye's first gold light | sanctum, rays, figures |
| `sunfall-beam` | A2-C5-04: Valencia's memory of the beam, red-tinted | beam, figures |
| `morning-montage` | A2-C6-05: sunrise over Morvaine, Fable and Doomward | three panels sliding past |
| `starless-flash` | A3-C1-04: one frame of the vision | black sky, frozen street, glass figure |
| `crown-forged` | A3-C2-02: Box, Eye and shard melting together | dark, relics, light |
| `coup` | A3-C2-04: {sb} on the balcony over the crowd | sky, balcony, crowd. {sb} is the player's rig, drawn into it |
| `time-tear` | A3-C5-03: the sky over the Sunspire tearing | sky, tear, light, spire |
| `era-purging-dawn` | The tear's first era: the square burning, a boy watching | |
| `era-ruins` | The Sunspire in ruins under a dead sun | |
| `era-gulls-wager` | The deck at night, two young figures at the rail, one laughing | the two are the player's and {sb}'s rigs |
| `era-crowd-aging` | The square's crowd, ageing and back (a 12-frame loop) | |
| `era-leviathan` | A winged leviathan crossing the tear | |
| `vorlaith-hall` | A3-C5-07: the hall of stopped clocks | hall, clocks, throne, the Unmade |

Where a backdrop includes you or {sb}, the painting has a hole and the rig is drawn into it in the right pose (`coup`, `era-gulls-wager`, `pier-memory`'s children on the small build), so the player's character is always theirs.

## 11. Rooms and props (M13)

The world is painted rooms now, not tiles: `12-rooms.md` is their spec, with their layers, detail rules, indicators, templates and placeholders. Props from `03b-world-art.md` §3 are recut from their source pictures at room scale by `cut_supplied.py --scale 3` (the source pictures are larger than the current cuts, so nothing is upscaled) and placed in rooms' mid and foreground layers by the placeholder generator, until the painted rooms replace them. Rooms are lit from the top left and never mirrored, so they keep that rule; characters keep theirs (§2.3).

Until M13, Rig characters in placeholder rooms is the supported combination, and Rig characters on Legacy tiles still works at 138 px (§2.4).

## 12. Tools

All CPython, seeded, writing the same bytes on every run, like the rest of `tools/`.

| Script | Writes | What it does |
| --- | --- | --- |
| `tools/rig/rig_spec.py` | `Content/Rig/base.rig.json` | The bones, slots, layers, sockets and builds above, as data. The single source of truth for them |
| `tools/rig/vector.py` | (library) | Anti-aliased polygons, ellipses, strokes and fills, by 4x supersampling into `pixels.py` images |
| `tools/rig/make_rig_template.py` | `tools/source/rig/templates/<build>.png` | The A-pose template, 2048x2048: a labelled box for every part, the pivot as a crosshair, the outline weights, and a grey figure showing proportions. This is what a painted kit is drawn over |
| `tools/rig/cut_rig.py` | `Content/Rig/Kits/<kit>.png`, `<kit>.mask.png`, `<kit>.kit.json` | Reads a filled template from `tools/source/rig/<kit>.png`, keys out the backdrop by flooding from each box's edge (as `cut_supplied.py` does), trims each part, reads the pivot from the template, and packs the parts into one atlas with its mask |
| `tools/rig/generate_rig_placeholders.py` | the same files | Draws every kit procedurally with `vector.py`: flat-shaded shapes in the right colours and silhouettes. Everything works with these before a single painted kit exists |
| `tools/rig/clips.py` | `Content/Rig/clips.json` | Every clip in §4 |
| `tools/rig/generate_pool_npcs.py` | `Content/Rig/npcs.json` | §7.5 |
| `tools/rig/check_clips.py` | — | Fails if a script's `anim` name has no clip or alias |
| `tools/rig/check_lineup.py` | — | §7.6 |
| `tools/check_assets.py` | — | Extended to `Content/Rig/` |

### 12.1 Painting a kit

The author makes kits the way the current supplied art was made: with an image generator, then cut. The template is the brief. A prompt that works, given the template image as a reference:

```text
A character part sheet for a 2D game. Paint over the reference sheet exactly: one part in each
labelled box, nothing outside the boxes, flat dark grey background. Three-quarter view facing right.
Cartoon fantasy style, thick dark outlines, flat cel shading with one shadow and one highlight,
lit from above. Character: [description from §7.4]. Every part separate and complete, including
the parts hidden behind others when assembled.
```

Don't name another game or artist in the prompt and don't give it another game's art as reference. Describe the angle and style in words, as above.

A painted kit doesn't need tint masks if it's for one character in one look (most notable characters). Hero and pool kits need them; `cut_rig.py --mask` builds a first mask from the template's colour-keyed regions, which is then touched up.

### 12.2 ASSETS.md

Every file gets a line, as now. The shapes:

```text
| `Rig/base.rig.json` | `RigLibrary` | The skeleton, slots, layers, sockets and builds (tools/rig/rig_spec.py) |
| `Rig/clips.json` | `RigAnimator` | Every animation clip (tools/rig/clips.py) |
| `Rig/Kits/sera.png` | `RigKit` | Sera's head, braid, cape and bow; cut from tools/source/rig/sera.png |
| `Rig/Kits/pool-questeria-head-3.png` | `RigKit` | A Questeria townsperson's head; generated |
| `Rig/Items/eye-of-solvaris.png` | `LiveItem` | The Eye: asleep, gold, gold-violet and red-black, 18x18 each at 3x |
| `Backdrops/coup/*.png` | `BackdropSprite` | A3-C2-04's balcony, four layers, 1600x900 |
```

## 13. Runtime classes

In a new `Rig/` folder:

| Class | Does |
| --- | --- |
| `RigLibrary` | Loads `base.rig.json`, `clips.json`, every kit and item picture; checks every slot a kit fills exists |
| `RigKit` | One kit's atlas, mask and part list |
| `RigLook` | Which kits fill which slots for one character in one outfit, with tints; built from `CharacterLook` and equipment for party members, from `npcs.json` or a notable's definition for everyone else |
| `RigTint` | §3.8. Bakes and caches |
| `RigPose` | Bone transforms for a moment of a clip |
| `RigAnimator` | Plays and blends clips; fires events |
| `RigActor` | A placed character: look, animator, facing, sockets, expression, effects |
| `RigRenderer` | Draws a `RigActor` (and its held items and glows) with `SpriteBatch`, in layer order, with the facing's x scale |
| `PortraitRenderer` | §5.4 |
| `LiveItem` | An item's picture, grip, states and glow |

`WalkerSprite` stays for Legacy. `ExploreScreen`, the fight screens and `DialogueScreen` draw through an `IActorView` interface that both `WalkerSprite` and `RigActor` implement, which is how the switch works without forking the screens.
