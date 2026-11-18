# Asset prompts: props, icons, interface, title and story paintings, for ChatGPT

The owner's request (2026-11-18): a prompt for every art asset, so all of the game's art can be made in ChatGPT in one look. The rooms' backgrounds, interiors, foregrounds and floor tiles are in `14-background-prompts.md`; the characters' parts and the creatures are in `13-art-prompts.md` (§8 for the creatures); armors painted whole are in the armor brief. This file has what was left: the props that stand in the rooms, the item, ability and condition icons, the interface, the title screen and the story paintings, and the small moving things in a room.

Each block is complete on its own: the style, the picture's list and the avoid list. What each thing is comes from `ASSETS.md`'s line for the picture the game has now, from `ItemCatalog.cs`, and from `AbilityId.cs` and `ConditionId.cs`. No prompt names a game, a studio or a brand; keep it that way when you edit one.

Nothing imports these pictures yet: the props, icons and interface the game has are drawn by its own tools or cut from earlier sheets. Send the pictures to Claude with the names given and say they're from this file; the cutting comes with the first of them.

---

## 1. How to use this file

1. One picture per message. Paste the whole block.
2. Nothing to attach, unless the block says so.
3. Check it: the right number of things, in the order listed, none touching, flat magenta behind them, no text.
4. Send the PNG to Claude with the file name the entry gives.

To fix a picture, ask for one fix at a time and say what stays: *"Keep everything else exactly the same; only ..."*

---

## 2. The shared blocks

Every block below already contains these. They're here to edit: change one, and ask Claude to update the blocks.

### 2.1 The prop style block

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.
```

### 2.2 The prop avoid list

```text
AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### 2.3 The icon style block

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text.
```

### 2.4 The icon avoid list

```text
AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### 2.5 The painting style block

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.
```

### 2.6 The painting avoid list

```text
AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

---

## 3. Props

The 223 things the game stands in its rooms, six to a picture, in 8 groups. The game makes 43 colour variants itself (a house with a plum roof, say), listed at the end of this section: they need no picture.

### Buildings and shop fronts

#### `props-01` · Buildings and shop fronts · sheet 1

- **Send to Claude as:** `props-01.png`
- **Holds:** `bakery`, `chapel`, `city-gate`, `city-wall`, `cottage`, `door`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. bakery: the baker's: a shop window under a pink striped awning, a loaf on its hanging sign.
2. chapel: a little chapel: a gothic door under a gable, stained glass under a gable each side.
3. city gate: the city gate, with a way through the middle.
4. city wall: a stretch of the city wall with flowers along it at Thornbridge.
5. cottage: a one-floor cottage under a hipped roof, a gabled porch door between window boxes.
6. door: a double door in its stone frame, shut and open.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-02` · Buildings and shop fronts · sheet 2

- **Send to Claude as:** `props-02.png`
- **Holds:** `east-gate`, `farmhouse`, `forge`, `fountain`, `garden-wall`, `gate`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. east gate: Questeria's east gate, the city gate with the road through it cleared so whoever walks it shows at Thornbridge.
2. farmhouse: a farmhouse: a one-floor cottage with a red roof and a chimney.
3. forge: the smithy: an open arch with the forge glowing inside, a chimney, the anvil sign on its post.
4. fountain: the plaza's fountain.
5. garden wall: a length of garden wall.
6. gate: a stone door in its frame, shut and sunk into the floor.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-03` · Buildings and shop fronts · sheet 3

- **Send to Claude as:** `props-03.png`
- **Holds:** `gilded-flagon-sign`, `gilded-flagon`, `guildhall`, `healer-station`, `house-awning`, `house-ivy`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. gilded flagon sign: the Gilded Flagon's hanging sign on its own post: a gold flagon.
2. gilded flagon: the Gilded Flagon inn: two floors, a front gable over its door, a gold flagon on its hanging sign.
3. guildhall: the guildhall.
4. healer station: Mira's table of bottles, bandages and herbs.
5. house awning: a house with an awning.
6. house ivy: a house with ivy up it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-04` · Buildings and shop fronts · sheet 4

- **Send to Claude as:** `props-04.png`
- **Holds:** `house-rose`, `house-shop`, `inn`, `keep-arch`, `palace`, `pillar-marble`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. house rose: a house with roses round it.
2. house shop: a house with a shop in the front.
3. inn: the inn: the shop's house, a size bigger.
4. keep arch: the arch from the great hall into the throne room, its gilded doors standing open.
5. palace: the palace.
6. pillar marble: a marble column with a gilded capital.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-05` · Buildings and shop fronts · sheet 5

- **Send to Claude as:** `props-05.png`
- **Holds:** `quarry-mouth`, `rusty-anvil`, `shop-counter`, `statue`, `stone-bridge`, `temple`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. quarry mouth: the timbered way into the old quarry.
2. rusty anvil: the Rusty Anvil, a dockside tavern: two gables, a wide door, its anvil sign on a post.
3. shop counter: a shop counter with pots and a cloth on it.
4. statue: a statue on its plinth.
5. stone bridge: the old stone bridge that takes the road over the stream.
6. temple: the temple.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-06` · Buildings and shop fronts · sheet 6

- **Send to Claude as:** `props-06.png`
- **Holds:** `townhouse`, `turret-house`, `warehouse`, `watch-post`, `well-flowers`, `well-roofed`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. townhouse: a tall narrow townhouse: a gabled garret, a floor of flower boxes, a door beside a slit window.
2. turret house: a gabled house with a round turret up its corner.
3. warehouse: a twin-gabled warehouse for the docks, wide doors and an open doorway.
4. watch post: the Watch's post: a battlemented guardroom with a blue door, a corner tower, the Watch's banner on its stand.
5. well flowers: a roofed well grown over with flowers.
6. well roofed: a well with a roof over it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-07` · Buildings and shop fronts · sheet 7

- **Send to Claude as:** `props-07.png`
- **Holds:** `windmill`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. windmill: the farm's windmill, its sails across its front.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Harbour and ship

#### `props-08` · Harbour and ship · sheet 8

- **Send to Claude as:** `props-08.png`
- **Holds:** `bollard`, `cargo-barrel`, `cargo-crate`, `fish-stall`, `fishmonger`, `gangplank`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. bollard: a mooring post.
2. cargo barrel: a barrel.
3. cargo crate: a crate.
4. fish stall: the fishmonger's stall: a blue striped stall with the day's catch laid out on ice.
5. fishmonger: the fishmonger's: a timber shop front under a blue striped awning, a fish on its sign.
6. gangplank: the ship's gangplank down to the boardwalk.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-09` · Harbour and ship · sheet 9

- **Send to Claude as:** `props-09.png`
- **Holds:** `harbour-bell`, `mast`, `net-rack`, `rope-coil`, `rope-posts`, `ship-wheel`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. harbour bell: the harbour bell on its gallows post.
2. mast: the Gull's Wager's mainmast, its sail full and its crow's nest at the top.
3. net rack: a rack of fishing net hung up to dry between two posts, cork floats on it.
4. rope coil: a coil of rope on the deck.
5. rope posts: two posts with a rope between them.
6. ship wheel: the ship's wheel.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-10` · Harbour and ship · sheet 10

- **Send to Claude as:** `props-10.png`
- **Holds:** `ship`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. ship: the ship you arrive on, side on, its cabin at the stern and its sail full.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Market, street and festival

#### `props-11` · Market, street and festival · sheet 11

- **Send to Claude as:** `props-11.png`
- **Holds:** `banner-blue`, `banner-pink`, `barrel-flowers`, `barrel-salt`, `barrel-water`, `barrel`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. banner blue: a blue banner on its pole.
2. banner pink: a pink banner on its pole.
3. barrel flowers: a barrel of flowers.
4. barrel salt: an open barrel of salt.
5. barrel water: a water barrel with a blue lid.
6. barrel: a barrel.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-12` · Market, street and festival · sheet 12

- **Send to Claude as:** `props-12.png`
- **Holds:** `bench-iron`, `bench-plain`, `bench`, `booth`, `cask`, `crate-apples`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. bench iron: a bench with iron arms.
2. bench plain: a wooden bench.
3. bench: a bench.
4. booth: a high-backed tavern booth with a crimson cushion.
5. cask: a cask.
6. crate apples: a crate of apples.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-13` · Market, street and festival · sheet 13

- **Send to Claude as:** `props-13.png`
- **Holds:** `crate-onions`, `crate-open`, `crate-stack`, `crate`, `flower-cart`, `handcart`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. crate onions: a crate of onions.
2. crate open: an open crate.
3. crate stack: two crates stacked.
4. crate: a crate.
5. flower cart: a flower cart.
6. handcart: a handcart of flowers.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-14` · Market, street and festival · sheet 14

- **Send to Claude as:** `props-14.png`
- **Holds:** `hay-bale`, `haystack`, `keg-stack`, `lamp-arm`, `lamp-double`, `lamp`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. hay bale: a bale of hay.
2. haystack: a haystack with a pitchfork stuck in it.
3. keg stack: kegs, one behind two.
4. lamp arm: a street lamp, its lantern hung from an arm.
5. lamp double: a street lamp with two lanterns.
6. lamp: a street lamp.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-15` · Market, street and festival · sheet 15

- **Send to Claude as:** `props-15.png`
- **Holds:** `lantern-hanging`, `lantern-post`, `lantern-small`, `notice-board`, `planter-long`, `planter-pink`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. lantern hanging: a ship's lantern on a post.
2. lantern post: a lantern hung from a post.
3. lantern small: a lantern on the ground.
4. notice board: a notice board, what Questeria's sign looks like.
5. planter long: a long planter of flowers and lavender.
6. planter pink: a planter of pink flowers.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-16` · Market, street and festival · sheet 16

- **Send to Claude as:** `props-16.png`
- **Holds:** `planter-stone`, `planter`, `potted-plant`, `sack-pile`, `sacks`, `sign`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. planter stone: a round stone planter.
2. planter: a planter of flowers.
3. potted plant: a potted lily.
4. sack pile: a pile of grain sacks.
5. sacks: a heap of sacks.
6. sign: a fingerpost.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-17` · Market, street and festival · sheet 17

- **Send to Claude as:** `props-17.png`
- **Holds:** `signpost-arrows`, `signpost`, `stall-blue`, `stall-cloth`, `stall-pink`, `stall-teal`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. signpost arrows: a signpost with painted arrows.
2. signpost: a signpost.
3. stall blue: a market stall with a blue awning.
4. stall cloth: a cloth stall.
5. stall pink: a market stall with a pink awning.
6. stall teal: a market stall with a teal awning.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-18` · Market, street and festival · sheet 18

- **Send to Claude as:** `props-18.png`
- **Holds:** `sun-banner`, `topiary`, `urn-blue`, `urn`, `washing-line`, `waypoint`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. sun banner: a banner of the Aurelian sun hanging on a wall.
2. topiary: a clipped bush.
3. urn blue: a blue funerary urn in the barrow.
4. urn: a funerary urn in the barrow.
5. washing line: a washing line with washing on it.
6. waypoint: a waypoint stone with its mark dark and then lit.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Trees, plants and rocks

#### `props-19` · Trees, plants and rocks · sheet 19

- **Send to Claude as:** `props-19.png`
- **Holds:** `blossom-tree`, `bush-deep`, `bush-flowers`, `bush-pink`, `bush-small`, `bush`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. blossom tree: a tree in white blossom.
2. bush deep: a bush, deepened for the wood.
3. bush flowers: a flowering bush.
4. bush pink: a pink bush.
5. bush small: a small bush.
6. bush: a bush.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-20` · Trees, plants and rocks · sheet 20

- **Send to Claude as:** `props-20.png`
- **Holds:** `cherry-tree-small`, `cherry-tree`, `cypress-pot`, `cypress`, `dead-tree`, `fallen-tree`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. cherry tree small: a small cherry tree.
2. cherry tree: a cherry tree in blossom.
3. cypress pot: a cypress in a pot.
4. cypress: a cypress.
5. dead tree: a bare dead tree.
6. fallen tree: a tree down across the road.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-21` · Trees, plants and rocks · sheet 21

- **Send to Claude as:** `props-21.png`
- **Holds:** `fence-iron`, `fence-wood-post`, `fence-wood-side`, `fence-wood`, `fern`, `fir-deep`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. fence iron: an iron railing, blue and gold.
2. fence wood post: a farm fence's post on its own, for the end of a run.
3. fence wood side: a tile of farm fence running up the screen.
4. fence wood: two tiles of farm fence, a post and two split rails.
5. fern: a fern.
6. fir deep: a fir, deepened for the wood.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-22` · Trees, plants and rocks · sheet 22

- **Send to Claude as:** `props-22.png`
- **Holds:** `fir`, `flowers-mixed`, `flowers-pink`, `flowers-white`, `gloom-mushroom-small`, `gloom-mushroom`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. fir: a fir.
2. flowers mixed: a patch of flowers.
3. flowers pink: a patch of pink flowers.
4. flowers white: a patch of white flowers.
5. gloom mushroom small: a few gloom mushrooms.
6. gloom mushroom: ink caps and pale gloom mushrooms in a cluster.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-23` · Trees, plants and rocks · sheet 23

- **Send to Claude as:** `props-23.png`
- **Holds:** `grass-tall`, `log`, `mushrooms-pale`, `mushrooms`, `oak-amber`, `oak-deep`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. grass tall: a tuft of long grass.
2. log: a fallen log by the road.
3. mushrooms pale: pale blue toadstools.
4. mushrooms: red toadstools.
5. oak amber: an oak in autumn.
6. oak deep: the oak, deepened for the wood.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-24` · Trees, plants and rocks · sheet 24

- **Send to Claude as:** `props-24.png`
- **Holds:** `oak-light`, `oak-pale`, `oak-turning`, `oak`, `rocks-mossy`, `rocks`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. oak light: a green oak.
2. oak pale: an oak with pale bark.
3. oak turning: the deep oak beginning to turn, a vein up its trunk in the Mirewood.
4. oak: an oak.
5. rocks mossy: mossy rocks.
6. rocks: a few rocks.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-25` · Trees, plants and rocks · sheet 25

- **Send to Claude as:** `props-25.png`
- **Holds:** `root-wall`, `snag`, `stump`, `tree-blackvein-bare`, `tree-blackvein-pale`, `tree-blackvein`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. root wall: roots grown across a way through, tangled and then parted, what the quarry's root walls look like.
2. snag: a broken trunk.
3. stump: a hollow stump.
4. tree blackvein bare: a bare dead tree, black-veined in the Mirewood.
5. tree blackvein pale: an oak with pale bark gone grey, black-veined in the Mirewood.
6. tree blackvein: an oak gone grey with black veins through its bark and its leaves in the Mirewood.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-26` · Trees, plants and rocks · sheet 26

- **Send to Claude as:** `props-26.png`
- **Holds:** `wildflowers-blue`, `wildflowers-yellow`, `wildflowers`, `woodcutter-stump`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. wildflowers blue: a clump of blue flowers in the meadows.
2. wildflowers yellow: tall cream and yellow flowers in the meadows.
3. wildflowers: lupins in the meadows.
4. woodcutter stump: a felled oak's stump with the axe left in it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Indoors: furniture and fittings

#### `props-27` · Indoors: furniture and fittings · sheet 27

- **Send to Claude as:** `props-27.png`
- **Holds:** `anvil`, `armchair`, `bar-counter`, `bed`, `bedroll`, `book-crate`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. anvil: an anvil on its block.
2. armchair: a blue armchair.
3. bar counter: a long tavern bar counter with pots and a cloth on it.
4. bed: a simple wooden bed with a pillow and a blanket.
5. bedroll: a blanket laid out by the campfire.
6. book crate: a crate with books on it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-28` · Indoors: furniture and fittings · sheet 28

- **Send to Claude as:** `props-28.png`
- **Holds:** `bookshelf`, `candle-stand`, `chair`, `chalkboard`, `chest`, `cot`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. bookshelf: a shelf of jars and bottles.
2. candle stand: a candelabra with three candles.
3. chair: a chair with a pink cushion.
4. chalkboard: a café's chalkboard.
5. chest: a chest, shut and open.
6. cot: a camp bed.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-29` · Indoors: furniture and fittings · sheet 29

- **Send to Claude as:** `props-29.png`
- **Holds:** `desk`, `hearth`, `relic-case-orb`, `relic-case`, `relic-shelf`, `round-table`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. desk: a desk with a map and a candle.
2. hearth: a stone oven-hearth with its fire.
3. relic case orb: a gold orb on a cushion under glass, on a plinth.
4. relic case: a crown on a cushion under glass, on a plinth.
5. relic shelf: a cabinet of the old kings' relics.
6. round table: a round tavern table.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-30` · Indoors: furniture and fittings · sheet 30

- **Send to Claude as:** `props-30.png`
- **Holds:** `royal-desk`, `sconce`, `side-table`, `stool`, `sun-mosaic`, `sun-throne`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. royal desk: a gilded walnut writing desk with letters, a book and an inkwell on it.
2. sconce: the iron sconce a torch stands in.
3. side table: a small table with a candle.
4. stool: a stool.
5. sun mosaic: the sun of the old kings inlaid in a floor.
6. sun throne: King Leon's throne, gold and royal blue with the sun blazing behind it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-31` · Indoors: furniture and fittings · sheet 31

- **Send to Claude as:** `props-31.png`
- **Holds:** `table`, `tankard-table`, `tapestry`, `tool-rack`, `utensil-rack`, `war-table`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. table: a table.
2. tankard table: an oak tavern table with tankards on it, one knocked over, and a candle.
3. tapestry: a pink tapestry, hung on a wall.
4. tool rack: a smith's rack of tongs.
5. utensil rack: a rack of ladles and tongs over a basin.
6. war table: the war table with the map of Fable spread across it and the markers on it.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-32` · Indoors: furniture and fittings · sheet 32

- **Send to Claude as:** `props-32.png`
- **Holds:** `wardrobe`, `wine-table`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. wardrobe: a cupboard of jars with flowers on top.
2. wine table: a little marble table with a decanter of wine and two goblets.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Camps, the wild and the road

#### `props-33` · Camps, the wild and the road · sheet 33

- **Send to Claude as:** `props-33.png`
- **Holds:** `bones`, `brazier`, `campfire`, `cookfire`, `courier-body`, `ranger-bridge`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. bones: an animal's skull and bones.
2. brazier: a gold brazier of coals on three legs (the game draws the flame on it).
3. campfire: a ring of stones round two burning logs (the game draws the flame on it).
4. cookfire: a cooking fire with a pot hung over it (the game draws the flame on it).
5. courier body: the dead Doomward courier.
6. ranger bridge: the rangers' footbridge over the stream.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-34` · Camps, the wild and the road · sheet 34

- **Send to Claude as:** `props-34.png`
- **Holds:** `ranger-sign`, `refugee-bundle`, `rubble-heap`, `sapper-hole`, `satchel`, `tar-pool`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. ranger sign: the rangers' waymark: a post with antlers and a green ribbon.
2. refugee bundle: a refugee's bundle, rolled blanket and pot.
3. rubble heap: a heap of broken stone and earth where a roof came down.
4. sapper hole: the hole the Hollowed sappers broke up through a floor.
5. satchel: the courier's satchel with his letter in it.
6. tar pool: a pool of tar in the Mirewood.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-35` · Camps, the wild and the road · sheet 35

- **Send to Claude as:** `props-35.png`
- **Holds:** `tent-ragged-2`, `tent-ragged`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. tent ragged 2: a lower patched tent with a blanket over its door.
2. tent ragged: a patched tent in the refugees' camp.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### The barrow, the vault and things of power

#### `props-36` · The barrow, the vault and things of power · sheet 36

- **Send to Claude as:** `props-36.png`
- **Holds:** `black-crystal-purified`, `black-crystal-shattered`, `black-crystal`, `crystal-pillar`, `crystal-stone`, `dart-trap`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. black crystal purified: the black crystal made clean, white and gold.
2. black crystal shattered: the black crystal smashed.
3. black crystal: the black crystal spike.
4. crystal pillar: a crystal on a pillar.
5. crystal stone: a waypoint, a crystal on a plinth, drawn twice side by side: dim, and glowing.
6. dart trap: a dart trap in a wall, carved as the sun.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-37` · The barrow, the vault and things of power · sheet 37

- **Send to Claude as:** `props-37.png`
- **Holds:** `gold-pile-small`, `gold-pile`, `guard-statue`, `lever`, `sarcophagus`, `sunheart-pedestal`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. gold pile small: a small heap of gold coins.
2. gold pile: a heap of the crown's gold with a goblet, a sword and gems in it.
3. guard statue: a knight of the old war in marble on a plinth.
4. lever: a lever on a wall plate, up and down.
5. sarcophagus: a sarcophagus.
6. sunheart pedestal: the empty pedestal where the Sunheart sat.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-38` · The barrow, the vault and things of power · sheet 38

- **Send to Claude as:** `props-38.png`
- **Holds:** `vault-brazier`, `vault-chest`, `vault-door`, `vault-lever`, `vault-sconce`, `vault-stair-shut`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. vault brazier: a gold brazier of glowing coals on three legs (the game draws the flame on it).
2. vault chest: a royal strongbox bound in gold, shut, and open with its gold showing.
3. vault door: the vault's great doors of blued steel and gold in their gilded portal, shut and open.
4. vault lever: a gilded lever on a stone wall plate, up and down.
5. vault sconce: a gilded lamp on the wall (the game draws the flame on it).
6. vault stair shut: the stair down to the Old Crown Vault in Questeria's terrace with its iron gate locked.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-39` · The barrow, the vault and things of power · sheet 39

- **Send to Claude as:** `props-39.png`
- **Holds:** `vault-stair`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. vault stair: the stair down to the Old Crown Vault with its gate standing open.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Everything else

#### `props-40` · Everything else · sheet 40

- **Send to Claude as:** `props-40.png`
- **Holds:** `flower-arch`, `flower-box`, `great-oak`, `light-shaft`, `quest-board`, `rowboat`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. flower arch: an arch of flowers to walk through.
2. flower box: a small box of flowers.
3. great oak: a great old oak, three crowns deep, with a very wide trunk.
4. light shaft: the sun through a pair of tall windows lying across the floor, see-through.
5. quest board: the quest board, roofed and hung with flowers.
6. rowboat: a rowing boat moored by the jetty.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-41` · Everything else · sheet 41

- **Send to Claude as:** `props-41.png`
- **Holds:** `scarecrow`, `sewer-grate`, `sun-inlay`, `trough`, `washtub`, `water-barrel`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. scarecrow: a scarecrow, with a crow on his arm.
2. sewer grate: a sewer grate, lying on the ground.
3. sun inlay: the Aurelian sun laid into the floor in gold before the dais.
4. trough: a stone water trough with a spout.
5. washtub: a wooden washtub.
6. water barrel: a water barrel.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `props-42` · Everything else · sheet 42

- **Send to Claude as:** `props-42.png`
- **Holds:** `water-pump`, `wheat`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, standing on the bottom of its cell.

PROPS, in this order:
1. water pump: a water pump with a bucket.
2. wheat: two tiles of ripe wheat.

OUTPUT. One landscape PNG, 1536 by 1024, on perfectly flat magenta #FF00FF with no shading or texture. Lay the props out in a grid of 3 columns and 2 rows, one prop to a cell, in the order listed (left to right, top row first), each centred in its cell with clear magenta all round it and its base on the bottom of the cell. Draw only these props.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

### Colour variants the game makes itself

| Variant | Made from | Change |
| --- | --- | --- |
| `bakery-pink` | `bakery` | a pink roof |
| `bakery-plum` | `bakery` | a plum roof |
| `bakery-teal` | `bakery` | a teal roof |
| `chapel-pink` | `chapel` | a pink roof |
| `chapel-plum` | `chapel` | a plum roof |
| `chapel-teal` | `chapel` | a teal roof |
| `cottage-pink` | `cottage` | a pink roof |
| `cottage-plum` | `cottage` | a plum roof |
| `cottage-teal` | `cottage` | a teal roof |
| `fishmonger-pink` | `fishmonger` | a pink roof |
| `fishmonger-plum` | `fishmonger` | a plum roof |
| `fishmonger-teal` | `fishmonger` | a teal roof |
| `forge-pink` | `forge` | a pink roof |
| `forge-plum` | `forge` | a plum roof |
| `forge-teal` | `forge` | a teal roof |
| `gilded-flagon-pink` | `gilded-flagon` | a pink roof |
| `gilded-flagon-plum` | `gilded-flagon` | a plum roof |
| `gilded-flagon-teal` | `gilded-flagon` | a teal roof |
| `house-awning-pink` | `house-awning` | a pink roof |
| `house-awning-plum` | `house-awning` | a plum roof |
| `house-awning-teal` | `house-awning` | a teal roof |
| `house-ivy-pink` | `house-ivy` | a pink roof |
| `house-ivy-plum` | `house-ivy` | a plum roof |
| `house-ivy-teal` | `house-ivy` | a teal roof |
| `house-rose-blue` | `house-rose` | a blue roof |
| `house-rose-plum` | `house-rose` | a plum roof |
| `house-rose-teal` | `house-rose` | a teal roof |
| `house-shop-blue` | `house-shop` | a blue roof |
| `house-shop-pink` | `house-shop` | a pink roof |
| `house-shop-plum` | `house-shop` | a plum roof |
| `house-shop-teal` | `house-shop` | a teal roof |
| `rusty-anvil-pink` | `rusty-anvil` | a pink roof |
| `rusty-anvil-plum` | `rusty-anvil` | a plum roof |
| `rusty-anvil-teal` | `rusty-anvil` | a teal roof |
| `townhouse-pink` | `townhouse` | a pink roof |
| `townhouse-plum` | `townhouse` | a plum roof |
| `townhouse-teal` | `townhouse` | a teal roof |
| `turret-house-pink` | `turret-house` | a pink roof |
| `turret-house-plum` | `turret-house` | a plum roof |
| `turret-house-teal` | `turret-house` | a teal roof |
| `warehouse-pink` | `warehouse` | a pink roof |
| `warehouse-plum` | `warehouse` | a plum roof |
| `warehouse-teal` | `warehouse` | a teal roof |

---

## 4. Item icons

An icon for each of the 49 items, in the catalog's order, sixteen to a picture. A magic item gets a small glint.

#### `item-icons-1` · items 1 to 16

- **Send to Claude as:** `item-icons-1.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text.

ICONS, in this order (the words after each name only say what it is: draw the object, write nothing):
1. Dagger (weapon): A short, sharp blade that fits a boot, a sleeve or a palm. Quick in the hand, and true when it's thrown.
2. Shortsword (weapon): A straight blade a forearm long, light enough to swing all day. The Watch carries them, and so do the people the Watch is after.
3. Longsword (weapon): A knight's blade with a long grip, for one hand or two. Every hero in every song Lark knows carries one.
4. Rapier (weapon): A slender duelling blade from the Sunspire's fencing yards. It wins on speed, not weight.
5. Scimitar (weapon): A curved blade from the southern isles, wicked on the draw. Sailors swear by it, and at it.
6. Handaxe (weapon): A woodcutter's hatchet, balanced for the hand. Good for kindling; better thrown.
7. Battleaxe (weapon): A broad, bearded axe head on an ash haft. One hand or two, it settles arguments.
8. Greatsword (weapon): A blade as tall as its wielder that wants both hands and all of your back. Whatever it hits stays hit.
9. Warhammer (weapon): A square iron head on a steel-bound haft, made for the dents armour can't shrug off.
10. Mace (weapon): A flanged iron head on a short handle. Lightbearers favour it: it doesn't care what it's hitting.
11. Quarterstaff (weapon): Seasoned oak, shod with iron at both ends. A walking stick, until it isn't.
12. Spear (weapon): An ash shaft with a leaf-shaped head. It keeps trouble at arm's length, or further if you throw it.
13. Shortbow (weapon): A curved yew bow, short enough to draw among the trees. A Mirewood ranger's first friend.
14. Longbow (weapon): A tall yew bow with a heavy draw. It reaches across a field and through a gambeson.
15. Light Crossbow (weapon): A stock, a steel bow and a crank. Slow to load, easy to aim, and it doesn't miss by much.
16. Ember Dagger (weapon): An Emberkin blade that never quite cools: its edge glows like a coal in the dark. It burns what it cuts.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `item-icons-2` · items 17 to 32

- **Send to Claude as:** `item-icons-2.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text.

ICONS, in this order (the words after each name only say what it is: draw the object, write nothing):
1. Thornwood Bow (weapon): Cut from the briar that grew through the Thornmother's den, with the rot burned out of it. Its string hums when you nock an arrow.
2. Sunsteel Blade (weapon): A longsword from the Old Crown's armoury, its steel folded with captured sunlight. The darker the room, the brighter it shines.
3. Travelling Clothes (armour): Wool, linen and a good belt: what anyone wears on the road. They keep off the weather and nothing else.
4. Padded Armour (armour): Quilted layers of cloth and wadding. It stops a knife, and it rustles with every step.
5. Leather Armour (armour): A breastplate and shoulders of boiled leather, oiled supple. Quiet, light, and a great deal better than a shirt.
6. Studded Leather (armour): Tough leather set with close rows of iron rivets. The best a rogue can wear and still go unheard.
7. Hide Armour (armour): Thick furs and hides stitched one over another. Warm in the Mirewood, and heavier than it looks.
8. Chain Shirt (armour): A shirt of interlocking rings, worn under a tunic. It turns a blade and hides under your clothes.
9. Scale Mail (armour): A coat of overlapping steel scales on leather, down past the knee. It glitters, and it clanks.
10. Chain Mail (armour): Steel rings from shoulder to knee over a quilted jack. Without the strength for it, you'll trudge.
11. Plate Armour (armour): Shaped steel plates from collar to boot, buckled over mail. A knight's armour, at a knight's price.
12. Mage Robes (armour): Deep blue robes with silver thread at the hems, cut in the Mages Guild's fashion. Room to move, room to cast.
13. Wooden Shield (shield): A round shield of banded oak with an iron boss. The battered ones have the best stories.
14. Steel Shield (shield): A heater shield of hammered steel, enamelled Questeria blue. It rings like a bell when it's struck.
15. Warden's Shield (shield): A Royal Warden's shield, blessed in the Sunspire's chapel. Blows slide off it as if it knew they were coming.
16. Leather Cap (head): A padded leather cap. It keeps off the rain and the worst of a low branch.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `item-icons-3` · items 33 to 48

- **Send to Claude as:** `item-icons-3.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text.

ICONS, in this order (the words after each name only say what it is: draw the object, write nothing):
1. Iron Helm (head): A full iron helm with a hinged visor. It hides your hair and most of your face, which some people prefer.
2. Circlet of Focus (head): A thin silver circlet set with a blue stone that hums while a spell takes shape. Spells find their mark a little more often.
3. Leather Gloves (hands): Soft doeskin gloves, good for reins, ropes and cold mornings.
4. Gauntlets of Might (hands): Iron gauntlets with Stoneborn runes worked into the knuckles. Every weapon blow you land hits a little harder.
5. Leather Boots (feet): Stout leather boots, well broken in. Every adventure starts in a pair.
6. Iron Boots (feet): Boots plated with iron at the shin and toe. Loud on stone, and very hard to stamp on.
7. Boots of Striding (feet): Soft boots with a silver wing stitched at each heel. Your stride lengthens without you trying.
8. Travelling Cloak (cloak): A hooded wool cloak, waxed against the rain. It doubles as a blanket.
9. Cloak of Protection (cloak): A grey-green cloak that stirs when there's no wind. Blows that should land somehow don't.
10. Amulet of Vigour (amulet): A red stone on a gold chain, warm against the skin. You feel steadier with it on, and harder to put down.
11. Ring of Protection (ring): A plain silver band with a ward cut inside it. A faint glimmer turns the edge of every blow.
12. Healing Potion (consumable): A small bottle of red tonic with a glow in it. It tastes of cherries and closes wounds.
13. Greater Healing Potion (consumable): A bigger, brighter draught, gold flecks turning in the red. It mends what a lesser potion can't.
14. Alchemist's Fire (consumable): A clay flask of sticky liquid that bursts into flame when it breaks. Throw it; don't drop it.
15. Elixir of Focus (consumable): A blue elixir that fizzes on the tongue and clears the head. Drunk in a fight, it gives back 30 Focus.
16. Wolf Pelt (material): A thick grey pelt from a Mirewood wolf, the black smoke long gone out of it. Traders pay for good fur.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `item-icons-4` · items 49 to 49

- **Send to Claude as:** `item-icons-4.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text.

ICONS, in this order (the words after each name only say what it is: draw the object, write nothing):
1. Black Crystal Shard (material): A splinter of the black crystal that grows wherever the land's gone wrong. It's cold, it hums, and somebody always wants to buy it.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

---

## 5. Ability icons

An icon for each of the 36, in the game's order, sixteen to a picture. Each ability's icon is a white or pale symbol with a dark outline, with no tile behind it: the game puts it on a tile the colour of its kind.

#### `ability-icons-1` · 1 to 16

- **Send to Claude as:** `ability-icons-1.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text. Each ability's icon is a white or pale symbol with a dark outline, with no tile behind it: the game puts it on a tile the colour of its kind.

ICONS, in this order (the words after each name say what it does in a fight, to pick a symbol by: write nothing):
1. Attack: One attack with the weapon in the main hand, or a foe's attacks.
2. Off Hand Attack: A bonus-action attack with a light weapon in the off hand, after attacking with a light one in the main hand.
3. Dash: In no kit since fights went to rows: kept for its icon's place.
4. Disengage: Leaving the front row doesn't give anyone an opportunity attack for the rest of the turn. In no kit since fights went to rows.
5. Dodge: Attacks against you have disadvantage until your next turn.
6. Help: Gets a Downed ally back up with 1 HP, or gives an ally advantage on their next attack.
7. Drink Potion: A healing potion out of the party's bag, as a bonus action.
8. Throw Alchemists Fire: A flask of alchemist's fire out of the party's bag, thrown.
9. Soul Link Strike: You and the Soul-Bound strike one foe together: a sure hit with both weapons and radiant light.
10. Second Wind: A bonus-action breather that heals 1d10 + level, once a short rest.
11. Defence: The Defence fighting style: +1 AC while wearing armour.
12. Sneak Attack: An extra 1d6 once a turn with a finesse or ranged weapon, with advantage, or on a target an ally's hit this round, or one that's Marked or Prone.
13. Cunning Dash: Cunning Action: Dash as a bonus action. In no kit since fights went to rows: kept for its icon's place.
14. Cunning Disengage: Cunning Step: a free Shift nobody gets a swing at, and Dodging, as a bonus action.
15. Sacred Flame: Radiant fire on a foe who fails a Dexterity save. A cantrip.
16. Healing Word: A bonus-action heal at range.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ability-icons-2` · 17 to 32

- **Send to Claude as:** `ability-icons-2.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text. Each ability's icon is a white or pale symbol with a dark outline, with no tile behind it: the game puts it on a tile the colour of its kind.

ICONS, in this order (the words after each name say what it does in a fight, to pick a symbol by: write nothing):
1. Cure Wounds: A bigger heal, by touch.
2. Fire Bolt: A mote of fire thrown at a foe. A cantrip.
3. Ray Of Frost: A beam of cold that slows its target. A cantrip.
4. Magic Missile: Three darts of force that never miss.
5. Lay On Hands: A pool of healing by touch, spent a hit point at a time.
6. Protection: The Protection fighting style: the reaction that turns an attack on someone next to you to disadvantage, with a shield up.
7. Produce Flame: A flame in the palm, thrown. A cantrip.
8. Entangle: Grasping weeds and vines that restrain everyone in a patch of ground.
9. Eldritch Blast: A crackling beam of force. A cantrip.
10. Hex: A curse: an extra 1d6 necrotic on every hit against its target.
11. Vicious Mockery: An insult laced with magic: psychic damage and disadvantage on the target's next attack. A cantrip.
12. Bardic Inspiration: A d6 given to an ally to add to an attack or a save.
13. Multiattack: The Attack action makes every one of the foe's attacks, not just one.
14. Pack Tactics: Advantage on an attack on someone in the front when another of the foe's side is standing in its own front row.
15. Trip: A melee hit knocks the target Prone unless they make a Strength save.
16. Smoke Bomb: A bonus-action cloud of smoke: Dodging and Disengaged until the foe's next turn.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ability-icons-3` · 33 to 36

- **Send to Claude as:** `ability-icons-3.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text. Each ability's icon is a white or pale symbol with a dark outline, with no tile behind it: the game puts it on a tile the colour of its kind.

ICONS, in this order (the words after each name say what it does in a fight, to pick a symbol by: write nothing):
1. Blasting Charge: A lit charge of blasting powder thrown into a crowd: fire on everyone near where it lands.
2. Thorn Burst: Thorns burst out of the Thornmother's hide into the row in front of her.
3. Drink Elixir: An elixir of focus out of the party's bag, as a bonus action: a spent spell slot back.
4. Backstab: A weapon strike on any foe, front row or back.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

---

## 6. Condition icons

An icon for each of the 21, in the game's order, sixteen to a picture. Each condition's icon is a small, bold symbol in one or two colours.

#### `condition-icons-1` · 1 to 16

- **Send to Claude as:** `condition-icons-1.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text. Each condition's icon is a small, bold symbol in one or two colours.

ICONS, in this order (the words after each name say what it does in a fight, to pick a symbol by: write nothing):
1. Downed: At 0 hit points and out of the fight: a party member rolls death saves until they're healed, helped up, stable or dead.
2. Prone: On the ground: melee attacks against them have advantage, ranged ones disadvantage, and their own attacks have disadvantage. Getting up takes their Shift.
3. Slowed: They can't Shift on their next turn (Ray of Frost).
4. Disengaged: Leaving the front row doesn't give anyone an opportunity attack, for the rest of this turn.
5. Dodging: Attacks against them have disadvantage, and their Dexterity saves advantage, until the start of their next turn.
6. Restrained: Held where they stand: no Shift, attacks against them have advantage, theirs have disadvantage. A Strength save at the end of each of their turns breaks it (Entangle).
7. Hexed: Cursed: whoever hexed them does an extra 1d6 necrotic every time they hit them.
8. Inspired: Holding a bardic inspiration die, added to the first attack roll or save that would otherwise fail by no more than the die can make up.
9. Stable: Downed but no longer dying: no more death saves, but still out until healed.
10. Helped: Someone's helping: advantage on their next attack roll before the end of their next turn.
11. Mocked: Rattled by Vicious Mockery: disadvantage on their next attack roll before the end of their next turn.
12. Marked: The next attack on them has advantage, and uses the mark up.
13. Taunted: Their single-target attacks have to go at whoever taunted them (the Source) whenever they can.
14. Burning: On fire: the condition's die in fire damage at the start of each of their turns.
15. Bleeding: Bleeding: the condition's die in damage at the start of each of their turns.
16. Frozen: Stopped in time: they skip their turns, and only an area (any shape but one foe or one ally) can reach them.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `condition-icons-2` · 17 to 21

- **Send to Claude as:** `condition-icons-2.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads at 24 pixels: big shapes, no tiny detail, no text. Each condition's icon is a small, bold symbol in one or two colours.

ICONS, in this order (the words after each name say what it does in a fight, to pick a symbol by: write nothing):
1. Poisoned: The condition's die in poison damage at the start of each of their turns, and disadvantage on their attacks.
2. Stunned: They skip their turns, and attacks on them have advantage.
3. Blinded: Disadvantage on their attacks, and attacks on them have advantage.
4. Frightened: Disadvantage on their attacks.
5. Charmed: They can't attack whoever charmed them (the Source).

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF. A grid of 4 columns and 4 rows, one icon to a cell, in the order listed (left to right, top row first), each centred and filling about three quarters of its cell. Leave unused cells empty magenta.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

---

## 7. Interface

The menus' plates, the cursors and the marks, one picture each. The colour ramps the game tints characters and gear with (`character-palettes.png`, `gear-palettes.png`) are data, not art: they aren't painted.

#### `ui-panel`

- **Replaces:** `Content/panel.png`
- **Send to Claude as:** `ui-panel.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: a square panel to stretch behind menus: a dark iron plate with a thin bevelled rim, small rivets at the four corners and a plain, even middle, so its corners, edges and middle can be cut apart and stretched (nine slices). 1024 by 1024, the panel filling the picture. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-button`

- **Replaces:** `Content/button.png`
- **Send to Claude as:** `ui-button.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: two buttons side by side, each a wide iron plate with a groove along its middle: the left one at rest, the right one lit, a warm gold light in its groove. Plain, even middles so they can be stretched. 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-cursors`

- **Replaces:** `Content/cursors.png`
- **Send to Claude as:** `ui-cursors.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: six mouse cursors in a row of six cells, each pointing up and to the left: an arrow; a ring to walk to; a hand to use something; a speech bubble to talk; a crossed circle for no; and a small door for a room's doors. 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-dialogue-icons`

- **Replaces:** `Content/dialogue-icons.png`
- **Send to Claude as:** `ui-dialogue-icons.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: six small icons in a row for the options in a conversation: a twenty-sided die; a heart; a padlock; and three voices as three different speech marks (a calm one, a bold one, a sly one). 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-indicators`

- **Replaces:** `Content/indicators.png`
- **Send to Claude as:** `ui-indicators.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: sixteen badges in a 4 by 4 grid, each a round gold-rimmed token with one symbol on it, for what's behind a door: a coin purse (a shop), an anvil (a smith), a bottle (potions), a tankard (an inn), crossed swords (a fight), a scroll (a quest), a shield (the Watch), a sun (the temple), a crown (the keep), a plain door, a gate, stairs going up, stairs going down, an arrow pointing left, an arrow pointing right, and a sparkle for something to look at. 1024 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-keycaps`

- **Replaces:** `Content/keycaps.png`
- **Send to Claude as:** `ui-keycaps.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: a blank keyboard key cap seen from the front, pale with a darker rim, to write a key's letter on; and beside it four computer mice seen from above: plain, with the left button lit gold, with the right button lit, and with the wheel lit. 1536 by 1024, five cells in a row. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-map-marks`

- **Replaces:** `Content/map-marks.png`
- **Send to Claude as:** `ui-map-marks.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: eight marks for a parchment map in a row: a town (a cluster of roofs), a camp (a tent), a ruin (a broken arch), a wood (three trees), a cave mouth, a 'you are here' marker (a gold pin), a waypoint (a small diamond) and a short piece of dotted road. 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-markers`

- **Replaces:** `Content/markers.png`
- **Send to Claude as:** `ui-markers.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: four markers in a row: a gold exclamation-mark-shaped flame for someone with a quest, a red one for an event, and a gold and a red arrow that point to them from the screen's edge. Draw the marks as bold shapes, not as letters. 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `ui-combat-hud`

- **Replaces:** `Content/Combat/hud.png`
- **Send to Claude as:** `ui-combat-hud.png`

```text
STYLE. Original icons for a 2D fantasy role-playing game's interface, in a clean, outlined, vector-like style: bold outlines in deep brown to near-black, never pure black; flat colour fills with two cel tones and one small highlight; light from the upper left. Each icon is one simple, bold object or symbol that still reads small: big shapes, no tiny detail, no text.

DRAW: the pieces of a fight's screen, laid out in a tidy grid with space between them: a square hotbar slot, empty, and the same slot lit; a round portrait frame for the turn order, plain, and one with a gold rim for whose turn it is; four small pips (a sword for the action, a spark for the bonus action, a shield for the reaction, a boot for the Shift), each lit and unlit; an empty bar with a blue fill for Focus; three small skulls and three small hearts for death saves; a five-pointed linked sigil in five states from dim to blazing gold; a target ring in red and one in gold; a flat oval plate for where someone stands in a row and a chevron to float over it; a ribbon banner. 1536 by 1024. On perfectly flat magenta #FF00FF, each thing centred in its cell with clear magenta round it.

AVOID. Text, letters, numbers, labels, watermarks. Frames, borders, tiles or backgrounds behind an icon unless asked. More than one object in a cell. Icons touching the cell edges or each other. Pixel art, painterly blur, glossy 3D-render shading, gradients, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

---

## 8. The title screen

The title is layers that drift at different speeds: the sky at the back, the far mountains and floating islands, the city on its cliffs, and the terrace and blossom trees at the front, with clouds, a banner, the spire's light, a waterfall and falling petals moving over them. Make them in one chat, in this order, so they match.

#### `title-sky`

- **Send to Claude as:** `title-sky.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: the sky, edge to edge: deep blue at the top going to soft pink at a level horizon, a scatter of faint stars and two pale moons high up, one large and one small. No clouds, no land. 1536 by 1024, the whole picture. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-far`

- **Send to Claude as:** `title-far.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: the far distance on flat magenta: a long range of pale, hazy blue mountains across the bottom third, and above them three floating islands of rock and green, the biggest in the middle with a waterfall pouring off its edge into mist. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-city`

- **Send to Claude as:** `title-city.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: the middle distance on flat magenta: the city of Questeria on its cliffs above a calm lake, seen from far off: cream stone walls and towers, roofs of azure, rose, teal and plum, a tall keep with gold-capped spires, bridges between the cliffs, the lake below reflecting it. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-foreground`

- **Send to Claude as:** `title-foreground.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: the near foreground on flat magenta: a stone terrace with a carved balustrade across the bottom of the picture, and a blossom tree at each side leaning in, heavy with pink blossom. The middle of the picture is empty. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-clouds`

- **Send to Claude as:** `title-clouds.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: six separate clouds in a grid of 3 columns and 2 rows on flat magenta: big, soft, rounded white clouds with flat pale-pink undersides, each a different shape, none touching. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-banner`

- **Send to Claude as:** `title-banner.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: eight frames of one long pennant blowing in the wind, in a grid of 4 columns and 2 rows on flat magenta: navy with a gold sun, on the tip of a pole, the pole's tip in the same place in every cell and the cloth rippling a little further in each. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-spire`

- **Send to Claude as:** `title-spire.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: eight frames of the light at the top of a crystal spire, in a grid of 4 columns and 2 rows on flat magenta: a tall, slim crystal on a gold foot, its glow swelling and fading from one cell to the next, the foot in the same place in every cell. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-waterfall`

- **Send to Claude as:** `title-waterfall.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: six frames of a thin waterfall falling from the edge of a floating island, in a grid of 3 columns and 2 rows on flat magenta: the water's streaks moving down a little further in each cell, ending in mist, the top of the fall in the same place in every cell. 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `title-petal`

- **Send to Claude as:** `title-petal.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

LAYER. One layer of a title screen, to be laid over the others. Draw: four single pink blossom petals on flat magenta, each at a different turn, big and simple. 1024 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

---

## 9. Story paintings

The three painted scenes the story shows, each in layers that drift, like the title. Make a scene's layers in one chat, in order. All 1536 by 1024.

### `questeria-dawn` · Questeria at dawn, from the ship

#### `questeria-dawn-sky`

- **Send to Claude as:** `questeria-dawn-sky.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. Questeria at dawn, from the ship. This picture is one layer of it, to be laid over the others. Draw: a sunrise sky, edge to edge: gold at a level horizon going to rose and then pale blue, the sun just coming up over far mountains at the right, the last stars and two pale moons fading high up. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `questeria-dawn-city`

- **Send to Claude as:** `questeria-dawn-city.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. Questeria at dawn, from the ship. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: the city of Questeria on its cliffs across the water, warmed by the sunrise: cream walls gone gold, long shadows, roofs of azure, rose and plum, the lake below it gold and pink. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `questeria-dawn-clouds`

- **Send to Claude as:** `questeria-dawn-clouds.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. Questeria at dawn, from the ship. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: two long, low clouds lit gold underneath by the dawn. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `questeria-dawn-rail`

- **Send to Claude as:** `questeria-dawn-rail.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. Questeria at dawn, from the ship. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: a ship's rail across the bottom of the picture, seen from on deck: varnished wood with brass caps, a coil of rope, a small pennant and a hanging lantern at the left. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

### `pier-memory` · The pier in the storm, the day you met

#### `pier-memory-sky`

- **Send to Claude as:** `pier-memory-sky.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The pier in the storm, the day you met. This picture is one layer of it, to be laid over the others. Draw: a storm sky, edge to edge: slate-grey and heavy, darker at the top, with big torn clouds. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `pier-memory-sea`

- **Send to Claude as:** `pier-memory-sea.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The pier in the storm, the day you met. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: a rough grey-green sea with white-capped waves, a small capsized boat far out, and a dark coast along the horizon. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `pier-memory-pier`

- **Send to Claude as:** `pier-memory-pier.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The pier in the storm, the day you met. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: a long wooden pier running in from the left edge across the lower part of the picture, wet planks and leaning posts, a lantern on a pole at its end; and a small child seen from behind, running down it. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

### `red-moon` · The red moon, where the bond was made

#### `red-moon-sky`

- **Send to Claude as:** `red-moon-sky.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The red moon, where the bond was made. This picture is one layer of it, to be laid over the others. Draw: a crimson night sky, edge to edge: deep red-black at the top going to dark rose at a level horizon, a few stars. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `red-moon-moon`

- **Send to Claude as:** `red-moon-moon.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The red moon, where the bond was made. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: one very large blood-red full moon with a soft halo round it. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `red-moon-shore`

- **Send to Claude as:** `red-moon-shore.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The red moon, where the bond was made. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: dark mountains and a still lake at night, the moon's red reflection in a long path on the water, mist along the far shore. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

#### `red-moon-headland`

- **Send to Claude as:** `red-moon-headland.png`

```text
STYLE. An original painting for a 2D fantasy role-playing game, drawn by hand in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black; flat colour fills with two or three cel tones and hard-edged shadow shapes; soft gradients only in the sky and the far distance. A bright, mystical fantasy palette: blue skies warming to pink at the horizon, warm cream and honey stone, roofs of azure, rose, teal and plum, navy and gold for anything royal. Light from the upper left.

SCENE. The red moon, where the bond was made. This picture is one layer of it, to be laid over the others. Draw: on flat magenta: a rocky headland across the bottom of the picture as a near-black silhouette, with a few wind-bent pines, flat on top in the middle where two people will stand. One landscape PNG, 1536 by 1024. Where it says flat magenta, everything that isn't the layer's own is perfectly flat magenta #FF00FF with no shading.

AVOID. People or animals unless the prompt asks for them. Text, letters, numbers, labels, watermarks, frames, borders. Pixel art, painterly blur, glossy 3D-render shading, photorealism, lens flare. Pure-black outlines. Neon or candy colours. Anything copied from an existing game, film, comic or brand.
```

The small things over them (the lightning, the rain, the glint on the water, the bond's sigil and glow) the game draws itself.

---

## 10. The small moving things in a room

The clouds, gulls, flags, flames, smoke and leaves that move in the rooms. Two pictures, sixteen cells each.

#### `room-life-1`

- **Send to Claude as:** `room-life-1.png`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, centred in its cell.

ROWS, four cells each, left to right:
Row 1: four soft white clouds with flat pale undersides, each a different shape.
Row 2: a white gull with grey wing tips, flying, seen from the side, in four frames of one wing beat.
Row 3: a small pennant on a pole, in four frames of it rippling, the pole in the same place in each.
Row 4: a torch flame in four frames of it flickering, orange with a yellow heart.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF: a grid of 4 columns and 4 rows, one thing to a cell, centred, none touching.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

#### `room-life-2`

- **Send to Claude as:** `room-life-2.png`

```text
STYLE. Original props for a side-on 2D fantasy role-playing game, drawn in a clean, outlined, vector-like style: crisp outlines in deep brown to near-black, never pure black, heavier round the outer shape and finer inside; flat colour fills with two or three cel tones and hard-edged shadow shapes; light from the upper left. Charming, stylised and well made, with a little wear. A bright fantasy palette: warm cream and honey stone, timber browns, roofs and cloth of azure, rose, teal and plum, navy and gold for anything royal. Each prop is seen side-on at a standing person's eye level, flat to us, with no perspective, centred in its cell.

ROWS, four cells each, left to right:
Row 1: a candle flame in four frames, small and steady.
Row 2: a puff of grey chimney smoke in four frames, rising and thinning.
Row 3: four single leaves, green going to gold, each at a different turn.
Row 4: a four-pointed sparkle in four frames, growing and fading.

OUTPUT. One square PNG, 1024 by 1024, on perfectly flat magenta #FF00FF: a grid of 4 columns and 4 rows, one thing to a cell, centred, none touching.

AVOID. People, animals, text, letters, numbers, labels, watermarks. A floor, ground, cast shadow, scenery or frame. Perspective or a view from above. Objects touching or overlapping each other or the cell edges. Any shading, glow or gradient on the background. Pixel art, painterly blur, glossy 3D-render shading, photorealism. Pure-black outlines. Anything copied from an existing game, film, comic or brand.
```

---

## 11. Index

| Picture | What | Holds |
| --- | --- | --- |
| `props-01.png` | Buildings and shop fronts | bakery, chapel, city-gate, city-wall, cottage, door |
| `props-02.png` | Buildings and shop fronts | east-gate, farmhouse, forge, fountain, garden-wall, gate |
| `props-03.png` | Buildings and shop fronts | gilded-flagon-sign, gilded-flagon, guildhall, healer-station, house-awning, house-ivy |
| `props-04.png` | Buildings and shop fronts | house-rose, house-shop, inn, keep-arch, palace, pillar-marble |
| `props-05.png` | Buildings and shop fronts | quarry-mouth, rusty-anvil, shop-counter, statue, stone-bridge, temple |
| `props-06.png` | Buildings and shop fronts | townhouse, turret-house, warehouse, watch-post, well-flowers, well-roofed |
| `props-07.png` | Buildings and shop fronts | windmill |
| `props-08.png` | Harbour and ship | bollard, cargo-barrel, cargo-crate, fish-stall, fishmonger, gangplank |
| `props-09.png` | Harbour and ship | harbour-bell, mast, net-rack, rope-coil, rope-posts, ship-wheel |
| `props-10.png` | Harbour and ship | ship |
| `props-11.png` | Market, street and festival | banner-blue, banner-pink, barrel-flowers, barrel-salt, barrel-water, barrel |
| `props-12.png` | Market, street and festival | bench-iron, bench-plain, bench, booth, cask, crate-apples |
| `props-13.png` | Market, street and festival | crate-onions, crate-open, crate-stack, crate, flower-cart, handcart |
| `props-14.png` | Market, street and festival | hay-bale, haystack, keg-stack, lamp-arm, lamp-double, lamp |
| `props-15.png` | Market, street and festival | lantern-hanging, lantern-post, lantern-small, notice-board, planter-long, planter-pink |
| `props-16.png` | Market, street and festival | planter-stone, planter, potted-plant, sack-pile, sacks, sign |
| `props-17.png` | Market, street and festival | signpost-arrows, signpost, stall-blue, stall-cloth, stall-pink, stall-teal |
| `props-18.png` | Market, street and festival | sun-banner, topiary, urn-blue, urn, washing-line, waypoint |
| `props-19.png` | Trees, plants and rocks | blossom-tree, bush-deep, bush-flowers, bush-pink, bush-small, bush |
| `props-20.png` | Trees, plants and rocks | cherry-tree-small, cherry-tree, cypress-pot, cypress, dead-tree, fallen-tree |
| `props-21.png` | Trees, plants and rocks | fence-iron, fence-wood-post, fence-wood-side, fence-wood, fern, fir-deep |
| `props-22.png` | Trees, plants and rocks | fir, flowers-mixed, flowers-pink, flowers-white, gloom-mushroom-small, gloom-mushroom |
| `props-23.png` | Trees, plants and rocks | grass-tall, log, mushrooms-pale, mushrooms, oak-amber, oak-deep |
| `props-24.png` | Trees, plants and rocks | oak-light, oak-pale, oak-turning, oak, rocks-mossy, rocks |
| `props-25.png` | Trees, plants and rocks | root-wall, snag, stump, tree-blackvein-bare, tree-blackvein-pale, tree-blackvein |
| `props-26.png` | Trees, plants and rocks | wildflowers-blue, wildflowers-yellow, wildflowers, woodcutter-stump |
| `props-27.png` | Indoors: furniture and fittings | anvil, armchair, bar-counter, bed, bedroll, book-crate |
| `props-28.png` | Indoors: furniture and fittings | bookshelf, candle-stand, chair, chalkboard, chest, cot |
| `props-29.png` | Indoors: furniture and fittings | desk, hearth, relic-case-orb, relic-case, relic-shelf, round-table |
| `props-30.png` | Indoors: furniture and fittings | royal-desk, sconce, side-table, stool, sun-mosaic, sun-throne |
| `props-31.png` | Indoors: furniture and fittings | table, tankard-table, tapestry, tool-rack, utensil-rack, war-table |
| `props-32.png` | Indoors: furniture and fittings | wardrobe, wine-table |
| `props-33.png` | Camps, the wild and the road | bones, brazier, campfire, cookfire, courier-body, ranger-bridge |
| `props-34.png` | Camps, the wild and the road | ranger-sign, refugee-bundle, rubble-heap, sapper-hole, satchel, tar-pool |
| `props-35.png` | Camps, the wild and the road | tent-ragged-2, tent-ragged |
| `props-36.png` | The barrow, the vault and things of power | black-crystal-purified, black-crystal-shattered, black-crystal, crystal-pillar, crystal-stone, dart-trap |
| `props-37.png` | The barrow, the vault and things of power | gold-pile-small, gold-pile, guard-statue, lever, sarcophagus, sunheart-pedestal |
| `props-38.png` | The barrow, the vault and things of power | vault-brazier, vault-chest, vault-door, vault-lever, vault-sconce, vault-stair-shut |
| `props-39.png` | The barrow, the vault and things of power | vault-stair |
| `props-40.png` | Everything else | flower-arch, flower-box, great-oak, light-shaft, quest-board, rowboat |
| `props-41.png` | Everything else | scarecrow, sewer-grate, sun-inlay, trough, washtub, water-barrel |
| `props-42.png` | Everything else | water-pump, wheat |
| `item-icons-1.png` | Item icons | Dagger, Shortsword, Longsword, Rapier, Scimitar, Handaxe, Battleaxe, Greatsword, Warhammer, Mace, Quarterstaff, Spear, Shortbow, Longbow, Light Crossbow, Ember Dagger |
| `item-icons-2.png` | Item icons | Thornwood Bow, Sunsteel Blade, Travelling Clothes, Padded Armour, Leather Armour, Studded Leather, Hide Armour, Chain Shirt, Scale Mail, Chain Mail, Plate Armour, Mage Robes, Wooden Shield, Steel Shield, Warden's Shield, Leather Cap |
| `item-icons-3.png` | Item icons | Iron Helm, Circlet of Focus, Leather Gloves, Gauntlets of Might, Leather Boots, Iron Boots, Boots of Striding, Travelling Cloak, Cloak of Protection, Amulet of Vigour, Ring of Protection, Healing Potion, Greater Healing Potion, Alchemist's Fire, Elixir of Focus, Wolf Pelt |
| `item-icons-4.png` | Item icons | Black Crystal Shard |
| `ability-icons-1.png` | Ability icons | Attack, Off Hand Attack, Dash, Disengage, Dodge, Help, Drink Potion, Throw Alchemists Fire, Soul Link Strike, Second Wind, Defence, Sneak Attack, Cunning Dash, Cunning Disengage, Sacred Flame, Healing Word |
| `ability-icons-2.png` | Ability icons | Cure Wounds, Fire Bolt, Ray Of Frost, Magic Missile, Lay On Hands, Protection, Produce Flame, Entangle, Eldritch Blast, Hex, Vicious Mockery, Bardic Inspiration, Multiattack, Pack Tactics, Trip, Smoke Bomb |
| `ability-icons-3.png` | Ability icons | Blasting Charge, Thorn Burst, Drink Elixir, Backstab |
| `condition-icons-1.png` | Condition icons | Downed, Prone, Slowed, Disengaged, Dodging, Restrained, Hexed, Inspired, Stable, Helped, Mocked, Marked, Taunted, Burning, Bleeding, Frozen |
| `condition-icons-2.png` | Condition icons | Poisoned, Stunned, Blinded, Frightened, Charmed |
| `ui-panel.png` | Interface | panel |
| `ui-button.png` | Interface | button |
| `ui-cursors.png` | Interface | cursors |
| `ui-dialogue-icons.png` | Interface | dialogue-icons |
| `ui-indicators.png` | Interface | indicators |
| `ui-keycaps.png` | Interface | keycaps |
| `ui-map-marks.png` | Interface | map-marks |
| `ui-markers.png` | Interface | markers |
| `ui-combat-hud.png` | Interface | combat-hud |
| `title-sky.png` | Title screen | sky |
| `title-far.png` | Title screen | far |
| `title-city.png` | Title screen | city |
| `title-foreground.png` | Title screen | foreground |
| `title-clouds.png` | Title screen | clouds |
| `title-banner.png` | Title screen | banner |
| `title-spire.png` | Title screen | spire |
| `title-waterfall.png` | Title screen | waterfall |
| `title-petal.png` | Title screen | petal |
| `questeria-dawn-sky.png` | Questeria at dawn, from the ship | sky |
| `questeria-dawn-city.png` | Questeria at dawn, from the ship | city |
| `questeria-dawn-clouds.png` | Questeria at dawn, from the ship | clouds |
| `questeria-dawn-rail.png` | Questeria at dawn, from the ship | rail |
| `pier-memory-sky.png` | The pier in the storm, the day you met | sky |
| `pier-memory-sea.png` | The pier in the storm, the day you met | sea |
| `pier-memory-pier.png` | The pier in the storm, the day you met | pier |
| `red-moon-sky.png` | The red moon, where the bond was made | sky |
| `red-moon-moon.png` | The red moon, where the bond was made | moon |
| `red-moon-shore.png` | The red moon, where the bond was made | shore |
| `red-moon-headland.png` | The red moon, where the bond was made | headland |
| `room-life-1.png` | Room life | clouds, gull, flag, flame |
| `room-life-2.png` | Room life | candle, smoke, leaf, sparkle |
