# Script: Prologue and Act 1

This is the source of truth for every line in the Prologue and Act 1. Chapters 1 to 4 are already built in `Dialogue/Scripts/`. Rewrite those scripts' lines to match this file, keeping their ids and flags. Chapters 5 to 8 are new.

**Fights.** Where a scene describes a fight's layout (tiles, circles on the field, walking into a zone), `11-combat.md` §5 is what to build: fights are side-view rows in the room now, and that section restates every special fight. Who helps in each fight is `11-combat.md` §4.1.

## How to read it

The notation maps one-to-one onto `DialogueScript`. The full mapping, with a worked example, is in `01-build-plan.md` §4. In short:

| Notation | Means |
| --- | --- |
| `#### script a1-c1-01` | A `DialogueScript` with this id. A header line under it gives `with`, `needs`, `unless`, `marker`, the map and what starts it |
| `SPEAKER (expr) [direction]: Line.` | A node. `SPEAKER` is a cast or origin id in capitals (`YOU` = `you`, `SB` = `soulbound`, `RED GULL` = `red-gull`). `(expr)` is the face the portrait wears (new, see the art spec). `[direction]` is the existing `Direction` |
| `#label` before the speaker | The node's `Id` |
| `{if a, b}` before the speaker | The node's `Conditions`, all of them. `!flag` is not-set. `bond>=70`, `with:sera`, `voice:witty`, `class:bard` |
| `~ a · b` at the end | `Effects`: `set f`, `clear f`, `bond +3`, `ambition =100`, `gold +40`, `give item`, `take item`, `join id`, `fight id`, `objective "…"`, `travel map spawn`, `rest`, `voice honorable` |
| `→ label` at the end | `Next`. `→ end` ends the script |
| `?` | The node above is a question. Its options follow, numbered. `? timer 5 → label` puts it on a clock |
| `[PERSUADE 12 · adv class:bard, voice:honorable]` | A check on the option. The label, the DC (12 if left out), the ability (Charisma if left out) and what gives advantage. Then `→ pass x · fail y` |
| `[needs cond]` / `[locked cond]` | Option conditions; locked shows it greyed |
| `♥` | A flirt option |
| `! cue …` | A stage cue that runs as the next node comes up (new; `01-build-plan.md` §5) |
| `— rejoin —` | A reading aid only: the branches above all lead back to the next line |

Text tokens are written exactly as the code has them: `{name}`, `{sb}`, `{sb_them}`, `{sb_their}`, and a capital first letter capitalises (`{Sb_their}`).

---

## Prologue · "Two Names, One Oath"

### P-01 · Title card

#### script p-01 · plays first, before the creation screen

```
! fade in 2 · sfx heartbeat
CARD: Every legend begins with a promise.
CARD: Most end with one broken.  ~ act =1
```

**Build.** The heartbeat is `heartbeat`, the sound the game has: one beat on its own, then two in step, so the one cue plays both. The second cue this had after the card, `heartbeat-two`, named a sound there isn't, and it's gone.

### P-02 · Creation: you

#### script p-02 · plays when the creation screen confirms you

```
NARRATOR: {name}. Nobody in Fable has heard that name yet.
NARRATOR: What brings you?
?
  1. Doing right, even when it costs.  ~ voice honorable
  2. Getting paid, and getting home.  ~ voice pragmatic
  3. A good story, and somewhere warm to tell it.  ~ voice witty
```

### P-03 · Creation: the Soul-Bound

#### script p-03 · plays after the creation screen's second pass, for `soulbound`

The second pass of the creation screen starts with the name field empty, and won't confirm until it's filled. The pier painting is behind it.

```
! backdrop pier-memory
NARRATOR: You were eight. A squall came in off the reef and turned your boat over.
NARRATOR: The grown-ups on the pier were still arguing about rope when {sb} jumped in.
! backdrop pier-fire
CORRA: Well. Now you've got each other.
?
  1. {sb} never once let me quit.  ~ bond +5
  2. {sb} always wanted more than the cove had.  ~ ambition +5
  3. {sb} could make me laugh with nothing to laugh at.  ~ bond +5
! backdrop red-moon
NARRATOR: At sixteen, under a red moon, you cut your palms with Bram's gutting knife and shook on it.
SB [sixteen, palm out]: Never leave the other behind.   [CALLBACK oath]
! sigil both on
NARRATOR: The cuts healed into a mark. It's never faded.
```

### P-04 · The voyage

#### script p-04 · map `gulls-wager` · trigger at the bow rail, where {sb} is (the map marks it)

The deck is walkable first, with the crew's barks as built. The scene starts at the bow.

```
! place soulbound at bow facing right · anim soulbound lean-rail
SB: Can't sleep either?   [CALLBACK sleep]
YOU: Too quiet. Something's going to try to eat us.
SB (smile): Give it till Fable. Odelle says the whole kingdom's crawling with work. Wolves, bandits, rings down wells.
SB: And the king pays in gold and thanks. Her word. Thanks. When did anyone with a title ever thank us for anything?
?
  1. Don't get your hopes up. A king's a king.  → hopes
  2. Maybe this one's different.  ~ bond +2  → different
  3. [needs voice:witty] I'll take the gold. Thanks don't keep.  → gold
#hopes SB: Spoken like a cove kid.  → doggett
#different SB: Maybe. I'd like to see one, just once.  → doggett
#gold SB (laugh): This is why you do the counting.  → doggett
#doggett SB [looking at the water]: You remember Doggett's cart? The nets going up on it?
YOU: I remember.
SB: Mam on her knees in the mud of the landing. Harrow talking to his reeve about the weather.
SB: He never looked at her once. I think about that more than I'd like.
SB (grin): So here's the plan. Fable. We make a name. Then a bigger one.
SB: And one day, a kingdom. Only a small one. Somewhere nobody kneels in the mud.   [CALLBACK mud]
?
  1. I'll be right there.  ~ bond +5  → there
  2. Let's start with a roof that doesn't leak.  → roof
  3. People who want crowns usually lose things on the way.  ~ set sb-warned-p  → crowns
#there SB: I know. That's most of the plan, really.  → herring
#roof SB: Fine. Roof first. Kingdom after.  → herring
#crowns SB (smile): Then I'll hold on tighter than they did.  → herring
#herring SB: Here. Hold your hand out.
! anim soulbound hold-out · hold soulbound hand_front herring-charm
NARRATOR: A herring, whittled from driftwood, on a waxed cord. {sb} is wearing its twin.   [CALLBACK herring]
SB: For luck. Don't lose it. I'm not carving another one on a boat this size.  ~ give herring-charm
! hold you neck herring-charm
! sfx ship-bell
LOOKOUT: Wraiths! Port side! They're coming up the hull!  ~ set wraiths-sighted · fight p-05-wraiths
```

The two herrings are live items (art spec §6). Yours stays on your neck socket for the whole game unless you take it off in the inventory; {sb}'s stays on theirs until A3-C2-04.

**Build.** The header has no `with`: the marked trigger at the bow starts this, and a `with` on {sb} that nothing stops would replay the voyage every time you talked to {sb}. The bell is `ship-bell`, the sound the game has. The lookout's line sets `wraiths-sighted`, which brings the wraiths up over the rail on the `gulls-wager` map.

### P-05 · The sea-wraiths

The guided fight as built. {sb}'s barks:

| When | Bark |
| --- | --- |
| First Soul-Link | `SB: Same as the lighthouse! Go!` |
| You below half health | `SB: I've got you. Get up.` |
| The last wraith falls | `SB: That's the lot. That's the lot, isn't it?` |

#### script p-05-wraiths-after · plays when the fight is won

```
! fade out 1 · backdrop questeria-dawn · fade in 1
ODELLE: There she is. Questeria. Pretty from out here.
ODELLE: Keep a hand on your purses once you're in.
SB (smile): Look at the size of it.
CHAPTER: Prologue  ·  Two Names, One Oath  ~ rest · set prologue-done · travel docks start
```

**Build.** The card is spaced like the game's other chapter cards, and carries what the as-built dawn did: `rest`, since the fight leaves everyone on 1 HP; `prologue-done`, which the ship's arrival in the harbour waits for; and the travel to the docks.

---

## Act 1 · "The Hunger at the Heart"

## Chapter 1 · "Saltwind Welcome"

### A1-C1-01 · Saltwind Docks

#### script a1-c1-01 · map `docks` · trigger at the Red Gulls once you've landed (the map marks it)

```
RED GULL: Dock tax, grandfather. It's doubled since last week.
TOMAS (afraid): There's no dock tax. Fifty years I've fished this harbour.
RED GULL: There is now. Hand it over.
SB (angry) [low]: Are you seeing this?
?
  1. Let's handle it.  ~ fame +1  → fight
  2. [PERSUADE · adv class:bard, voice:honorable] Talk first.  → pass talked · fail not-talked
  3. Not our business. We only just landed.  ~ bond -3 · ambition +2  → charges
#talked NARRATOR: The thugs look at each other, then at the two of you, and back off down the pier.
NARRATOR: A minute later they're back with their bosun.  → fight
#not-talked RED GULL: Talk's free. The tax isn't.  → fight
#charges SB: It's exactly our business. That's Doggett in a different hat.
NARRATOR: {sb} is already walking over.  → fight
#fight NARRATOR: Cudgels come out. A Stoneborn with a hook for a hand shoulders to the front.
GRELL: Two off the boat, and already telling me how my harbour works. Gulls. Collect.  ~ fight a1-c1-01-gulls
```

**Build.** The header has no `with`: nobody on the docks map is called `red-gull` (the thugs are `gull-1` to `gull-3`, in the Red Gulls' look), and the marked trigger round them starts the scene.

#### script a1-c1-01-gulls-after · plays when the fight is won

```
! anim grell kneel · anim soulbound blade-at-throat
NARRATOR: Grell is on his knees. {sb} keeps a blade at his throat a little longer than the fight needed.
GRELL (afraid): Mercy. It was only coin.
SB (cold): That's what they always say. Only coin. Only the nets. Only a tax.
?
  1. {sb}. Enough. He's done.  ~ bond +3  → enough
  2. (Say nothing.)  ~ ambition +3 · set sb-dock-cruelty  → kicked
  3. Let the Watch decide what he's owed.  → watch
#enough NARRATOR: {sb} steps back and sheathes the blade.
SB (smile) [breathing out]: Right. Sorry. Old ghosts.  → hale
#kicked NARRATOR: {sb} puts a boot on Grell's chest and shoves him off the pier. He comes up spitting.  ~ set grell-gone  → hale
#watch SB: The Watch. Fine. Let's see what the Watch is worth here.  → hale
#hale NARRATOR: Whistles. The City Watch comes down the boardwalk at a run, Captain Hale at the front.  ~ set grell-gone
HALE: Red Gulls, the lot of them, and my lads never got their boots wet. Who are you?
YOU: {name}. And {sb}. We came in on the Gull's Wager.
HALE (tired): Then welcome to Questeria. I'm short of men. Half the Watch went east to Greywatch and the Low Ring's noticed.
HALE: There's honest work on the board in the Lantern Market.
SB: Why east? What's at Greywatch?
HALE: Orders. Mine come from people who don't explain them. Go and find some work.
TOMAS: Here. It isn't much. It's kept me off the rocks forty years.  ~ give tomas-hook
! hold you belt_front tomas-hook
SB (smile) [walking away]: Did you see his face? That's the pay. The coin's extra.  ~ set a1-c1-01 · objective "Find the quest board in the Lantern Market, up in the city" · xp +100
```

**Changed (M4).** The last line gives `xp +100`, an effect and no new words. The wraiths and the Gulls are worth 100 each at level 1, so that's the 300 that takes the party to level 2 for Chapter 2 (`11-combat.md` §6.3).

### A1-C1-02 · The Lantern Market

#### script a1-c1-02 · map `questeria` · with `fennick` · unless `a1-c1-02` · marker quest

```
! board on
FENNICK: New faces. Good. Contracts go up here: the city's, the guilds', anyone's with coin and a problem.
FENNICK: Monsters on the left, bandits in the middle, lost property on the right. People will put a dog in bandits if you let them.
FENNICK: That one's the Rangers' Lodge. Something's wrong in the Mirewood. Animals biting that never bit, two woodcutters not home. They're paying triple.
SB: Triple. We'll take it.
FENNICK: You haven't asked what it is.
SB: You said triple.  ~ set a1-c1-02 · objective "Take the Rangers' contract to the Mirewood, east down the old road"
```

---

## Chapter 2 · "The Rot in the Mirewood"

### A1-C2-01 · The Mirewood's edge

#### script a1-c2-01 · map `mirewood` · trigger at the black-veined trees

```
! tint twilight
CHAPTER: Chapter 2  ·  The Rot in the Mirewood
SB: Smell that? Like a forge somebody let go out.
{if voice:honorable} YOU: Smells like someone's in trouble.  → honorable
{if voice:pragmatic} YOU: Smells like triple pay.  → pragmatic
{if voice:witty} YOU: Smells like your cooking.  → witty
#honorable SB: Then let's not be late.  → wolves
#pragmatic SB: That's my practical one.  → wolves
#witty SB: I'll remember that next time you're hungry.  → wolves
! sfx wolf-howl
#wolves NARRATOR: Something moves low between the trunks. Then three more somethings.  ~ fight a1-c2-01-gloomfangs
```

**Build.** Restored from the build: the chapter's card, which opens every chapter after the first, and the howl as the wolves come.

#### script a1-c2-01-gloomfangs-after

```
! sfx bowshot
! place sera at treeline facing left · anim sera bow-lowered
NARRATOR: An arrow takes the last wolf behind the ear. A Sylvan woman steps out of the trees, bow still drawn.
SERA: You're loud. Both of you. The whole wood heard you at the road.
SB (grin): You waited till we'd done the hard part, though.
SERA: I waited to see if you'd make it worse.
?
  1. The Rangers' Lodge sent us. We want to help.  ~ affinity-sera +3 · set lodge-told  → name
  2. ♥ Nice shot. Do you do that for everyone?  ~ affinity-sera +2 · set sera-flirt-1  → flirt
  3. We had it handled.  → handled
#flirt SERA: Only for people worth an arrow. I haven't decided about you.  → name
#handled SERA: You had a wolf on your back.  → name
#name SERA: Sera Vantis. I keep this reach.
SERA: The wolves aren't mad. Something in the old quarry is leaking into the roots, and anything that drinks from the stream goes wrong.
SB: So we find it and break it.
SERA: We find it and find out what it is. Then we decide.
SB (smile) [to you]: I like her. She's wrong, but I like her.  ~ join sera · set sera-recruited · set a1-c2-01 · objective "Find the old quarry deep in the Mirewood"
```

**Build.** Restored from the build: the bowshot, and on the last line `a1-c2-01` (the edge is over; the trigger at the black-veined trees checks it) and the objective for the quarry.

**Changed (M4).** Option 1 sets `lodge-told`. Telling Sera the Lodge sent you is what brings the Lodge's wardens to the quarry lip in the Thornmother's fight (`11-combat.md` §4.1), and no line had set it. It's a flag only; no line changes.

### A1-C2-02 · The Old Quarry

#### script a1-c2-02-letter · map `mirewood-quarry` · trigger at the courier's body

The game calls this scene `a1-c2-02-letter`, and ids don't change.

```
SERA: Doomward cloth. This far west, on foot.
NARRATOR: There's a letter in the satchel, sealed with a black cube.
YOU [reading]: "The Master's shards seed the ground ahead of the army. Let the rot soften Fable before we come."  ~ give courier-letter · set courier-letter-read
SB [low]: An army. Coming here. And the palace is planning a festival.  ~ objective "Go into the den at the bottom of the quarry"
```

**Build.** Restored from the build: `courier-letter-read`, which opens the den (the `mirewood-quarry` trigger waits for it) and stops the courier's trigger playing again, and the objective for the den. The den, and the Thornmother's fight it starts, are as built (`a1-c2-02`), and so are the two root walls Sera parts on the way down (`a1-c2-02-roots-1`, `a1-c2-02-roots-2`).

#### script a1-c2-02-thornmother-after · plays when `a1-c2-02-thornmother` is won

The game calls this `a1-c2-02-thornmother-after`, the scene the Thornmother's fight plays when it's won, and ids don't change.

```
! anim sera kneel
SERA: If we break it, the rot goes into the stream all at once. Everything downstream drinks it.
SERA: If I draw it out slowly, it takes two days. Maybe three.
SB: Two days of wolves eating woodcutters. Break it now and clean the stream after.
?
  1. Sera's way. We purify it.  ~ affinity-sera +5 · set crystal-purified  → purify
  2. {sb}'s way. Break it.  ~ affinity-sera -3 · bond +3 · set crystal-shattered  → shatter
  3. Both. Sera works. {sb} and I hold the quarry.  ~ affinity-sera +3 · bond +2 · set crystal-guarded  → guard
#purify SB (cold): Your wood. Your call.
! walk soulbound to quarry-mouth
NARRATOR: {sb} walks off to the quarry mouth and stays there.  ~ ambition +3 · set crystal-dealt-with · set a1-c2-02 · objective "Rest at the camp in the Mirewood" · travel mirewood camp  → end
#shatter ! anim soulbound pommel-strike · sfx crystal-pulse
NARRATOR: The crystal splits. Black water runs out of the stone toward the stream.
SB: Done. We'll fix the stream. Fast was right.
SERA (cold): Fast was fast.  ~ set crystal-dealt-with · set a1-c2-02 · objective "Rest at the camp in the Mirewood" · travel mirewood camp  → end
#guard SB (grin): Now that's a plan.  ~ fight a1-c2-02-guard-wave
```

**Build.** The crystal splitting is `crystal-pulse`, the sound the game has for the crystal. The guard wave's fight is `a1-c2-02-guard-wave`, its id in the game. Winning it plays the as-built `a1-c2-02-guard-wave-after`, which turns the crystal white and takes you to camp. The other two ways end on what the build's last line did: `a1-c2-02` (the quarry's done; the camp scene waits for it), the objective, and the travel to the Mirewood's camp.

### A1-C2-03 · Camp in the Mirewood

#### script a1-c2-03 · map `mirewood` · camp · plays when you rest at the camp

```
! place soulbound at fire · anim soulbound sit
SB: An army, {name}. Marching on a kingdom that's busy stringing lanterns. Somebody ought to be ready.
YOU: Somebody like us?
SB (smile): Who else is there?
! place sera at camp-edge facing left · camera sera
NARRATOR: Past the firelight, Sera is watching {sb}. She doesn't say anything.  ~ set sera-noticed-sb · set a1-c2-03 · rest · objective "Go back to Questeria and collect from Questmaster Fennick"
```

**Build.** Restored from the build: the objective back to Fennick.

### A1-C2-04 · Triple pay

#### script a1-c2-04 · as-built

As built (Fennick pays out). One line changes: Fennick's pay line, the one with the coin, is now `FENNICK: Triple, as promised. I'd like it noted that you didn't ask what it was.`

---

## Chapter 3 · "Ashes at Thornbridge"

### A1-C3-01 · The east gate

#### script a1-c3-01 · map `thornbridge` · plays on arrival at the east gate, once Fennick's paid

```
CHAPTER: Chapter 3  ·  Ashes at Thornbridge
BRISK: Chancellor's orders. Nobody from Doomward inside the walls. Could be sickness. Could be spies.
HEDDA (afraid): We walked three weeks. Lise hasn't eaten since yesterday.
SB: Look at her, Sergeant. That's your spy.
BRISK: I don't write the orders, friend.
SB (cold) [quietly]: No. A man in a warm room writes them, and you stand out here and read them aloud.
?
  1. Then we'll bring the food out to them.  ~ fame +1  → food
  2. [PERSUADE] Let the sick through. We'll answer for it.  → pass let · fail shove
  3. [needs voice:pragmatic] He's right. We don't know who's out there.  ~ bond -5 · ambition +2  → right
#food SB: Fine. Somebody's going to answer for this one day, though.  ~ set a1-c3-01 · objective "Find the healer down by the river"  → end
#let BRISK (tired): The sick. Only the sick. And I never saw you.  ~ fame +2 · bond +3 · set brisk-persuaded · set a1-c3-01 · objective "Find the healer down by the river"  → end
#shove NARRATOR: {sb} shoves the sergeant into the gatepost. A guard draws. You get between them before it goes any further.  ~ ambition +3 · set brisk-shoved · set a1-c3-01 · objective "Find the healer down by the river"  → end
#right SB (shock): Who are you right now?  ~ set a1-c3-01 · objective "Find the healer down by the river"  → end
```

**Build.** The header has no `with`: the scene plays on arrival at the gate (the `thornbridge` map's scene), and talking to Brisk plays his own lines. Restored from the build: the chapter's card; `brisk-persuaded` when the gate's talked open, which Brisk and Hedda read afterwards; and the objective for the healer.

### A1-C3-02 · Thornbridge Camp

#### script a1-c3-02 · map `thornbridge` · with `mira` · needs `a1-c3-01` · unless `mira-supplies`, `mira-supplies-done`, `mira-recruited` · marker quest

```
! anim mira heal · hold mira hand_front light-glow
MIRA: Hold still. It'll feel warm, then it'll feel better.
MIRA: You're not Watch, and you're not running. So you're brave or you're new.
SB: Bit of both.
MIRA: Mira Ashgrove. I was an archivist in Doomward, at the Ember Archive. Before.
?
  1. Before what?  → before
  2. You're dead on your feet. Let us help first.  ~ affinity-mira +5  → errands
  3. ♥ An archivist and a healer. That hardly seems fair.  ~ affinity-mira +2 · set mira-flirt-1  → flirt
#flirt MIRA (smile) [tired]: Flattery won't feed anyone. Thank you, though.  → before
#errands MIRA: Then the farmer's cough, the boy's fever and the glassblower's hands, in that order. They're down by the water.  ~ set mira-supplies · objective "Take Mira's remedies to the three sick refugees by the river"  → end
#before MIRA: Before my teacher opened the Box. Maleficio. He wanted to end the famines in the east, and for a while he did.
MIRA: First he needed enough power to fix the harvest. Then enough to fix the council. Then everything after that.
MIRA: Every step made sense when he took it. That's the part I can't get past.
! camera soulbound
SB: The council, though. Maybe he wasn't wrong about the council.
MIRA (angry): He burned the council. In their hall. With their children at the windows.
SB (worried) [hands up]: I didn't mean that. I meant the arguing. I'm sorry.
! sfx war-horn
NARRATOR: Horns in the treeline. Grey shapes come out of the trees toward the tents.  ~ set mira-supplies-done · objective "Stop the Hollowed at the treeline before they reach the camp"
```

The errands version is as built: once the last of the three remedies is handed over (`supplies-delivered`), talking to Mira plays `a1-c3-02-story`, which ends on the `#before` lines.

**Build.** Restored from the build: the `needs` and `unless`, without which talking to Mira would play this again once she's joined, and `mira-supplies` on the errands, which opens the remedies. The header names its map, `thornbridge`, as the raid's after-scene below does by coming under it, so the game's start-up check knows who their cues can name. The errands line names the errands the game has (`a1-c3-02-supplies-1` to `-3`); it named water barrels and blankets, which aren't there. The horns set `mira-supplies-done` and the objective instead of starting the fight: that flag brings the Hollowed out at the treeline on the `thornbridge` map, and walking up to them plays the as-built `a1-c3-02-raid` (two lines, then the fight, with Mira beside you). Started from here, the fight would put the party at the healer's station, twenty tiles from the raiders, and leave that trigger with nothing to do. The horn is `war-horn`, the sound the game has.

Barks in the raid:

| When | Bark |
| --- | --- |
| Second wave | `SB: Where's the Watch? Where's anybody?` |
| Mira heals someone | `MIRA: Stay down. You're not done bleeding.` |

#### script a1-c3-02-raid-after

```
NARRATOR: The Watch comes out of the gate as the last of the Hollowed falls.
SB (angry): You stood on your wall and watched.
BRISK: We had orders—
SB: Say "orders" one more time.
?
  1. (Put a hand on {sb}'s shoulder.)  ~ bond +5  → hand
  2. Let them hear it. They've earned it.  ~ ambition +5 · set sb-crowd-loved · fame +2  → speech
#hand ! anim you hand-on-shoulder soulbound
SB (tired): I'm all right. I'm all right.  → mira
#speech ! anim soulbound address-crowd
SB: Nobody's coming for you. Not the Watch. Not the king. So we will.
NARRATOR: Somebody starts saying {sb}'s name. Then the whole camp is.  → mira
#mira MIRA [quietly, to you]: {sb} runs hot. I knew someone else who ran hot like that.
MIRA: Watch {sb_them} for me?  ~ join mira · set mira-recruited · set a1-c3 · objective "Go back into the city; the Gilded Flagon's a good place to be found"
```

**Build.** Restored from the build: the objective back into the city.

---

## Chapter 4 · "The Sunheart"

### A1-C4-00 · The messenger

#### script a1-c4-00 · as-built

As built.

### A1-C4-01 · The Gilded Flagon

#### script a1-c4-01 · map `gilded-flagon` · with `elara` · needs `a1-c3` · unless `a1-c4-01` · marker quest

```
{if !a1-c4-00} CHAPTER: Chapter 4  ·  The Sunheart
ELARA: Sit. Don't bow, people are watching. You're the two from Thornbridge.
ELARA: Three nights ago someone got into the Old Crown Vault under the city and took the Sunheart. It powers the old ward-walls. Without it, the walls are just walls.
ELARA: My father wants it kept quiet. I want it back before anyone outside this booth knows it's gone.
SB: Why us? You've got guards.
ELARA: I've got guards. I don't know which of them took it.  ~ set a1-c4-01 · objective "Find the Sunheart in the Old Crown Vault, below the Sunspire"
```

**Build.** Restored from the build: `with elara` (her id on the `gilded-flagon` map; `elara-hooded` is her look there), the `needs` and `unless` that keep this to the booth and to once (without them, talking to Elara at court would play it again), the chapter's card for when the messenger hasn't found you first, and "below the Sunspire" in the objective, since nothing in the booth says where the vault's door is.

### A1-C4-02 · The Old Crown Vault

#### script a1-c4-02 · map `old-crown-vault` · with `kael` · unless `kael-truce`, `a1-c4-02` · marker event

```
! place kael at relic-shelf · hold kael belt_front sunheart · light kael sunheart on
KAEL: Oh. Company. I'd say make yourselves at home, but it's the king's home, and I'm robbing it.
SB: Put it down.
KAEL: Can't. I've been paid half, and I'm very attached to the other half.  ~ fight a1-c4-02-kael
```

**Build.** Restored from the build: the `unless`, without which talking to Kael would start the duel again once he's joined.

#### script a1-c4-02-kael-after · plays when Kael yields

```
! shake 0.6 · sfx thud
NARRATOR: The wall behind the shelves cracks open. Grey hands come through it.
KAEL (shock): Those aren't mine. Truce?
?
  1. Truce. We argue after.  ~ affinity-kael +3 · set kael-truce  → fight
  2. ♥ Truce, if you stop being so distracting.  ~ affinity-kael +2 · set kael-flirt-1 · set kael-truce  → flirt
  3. (Let {sb} answer.)  ~ affinity-kael-sb +3 · set kael-truce  → sb
#flirt KAEL (smirk): No promises, gorgeous.  → fight
#sb SB: Truce. I'm watching you, thief.
KAEL (smirk): Watch away. This is my good side.  → fight
#fight NARRATOR: The sappers come through the wall.  ~ fight a1-c4-02-sappers
```

**Build.** The wall cracking is `thud`, the sound the game has for something heavy landing.

#### script a1-c4-02-sappers-after

```
! anim kael flip-coin · hold kael hand_front black-iron-coin · sfx coin
KAEL: Here's something for you. My buyer paid half up front, in these.
KAEL: Doomward iron. Somebody in your nice clean city is on the other side's payroll.
MIRA (worried): Maleficio's agents pay in those. Whoever hired you is close to the crown.
KAEL: Never saw a face. Hood, good perfume, talked like a man who's never carried his own bags.  ~ set spy-seed-1 · give black-iron-coin
! drop kael belt_front
SB: You're good. You're wasted on pockets. Come with us.
KAEL: Is that a job or a friendship? I'm only any good at one of them.
SB: Both. You'll pick it up.  ~ give sunheart · join kael · set kael-recruited · set a1-c4-02 · objective "Take the Sunheart to the Sunspire Keep"
```

**Build.** Restored from the build: the coin's sound, and the objective to the keep.

### A1-C4-03 · Sunspire Keep

#### script a1-c4-03 · map `sunspire-keep` · trigger at the throne

```
! place leon at throne · place elara at throne-right · place crane at throne-left
LEON: The heroes of the docks, the Mirewood and Thornbridge. And now, I'm told, my own vault.
LEON (smile): My daughter keeps secrets badly when she likes someone.
ELARA: Father.  ~ take sunheart
! anim leon bow-head
LEON: Fable is in your debt, and so am I. Thank you.
SB [whispering, to you]: He said it. He actually said it.
MIRA: Your Majesty. I'm Mira Ashgrove, of the Ember Archive. Maleficio's army is real. His Hollowed hit Thornbridge, and his agents are inside your walls.
MIRA: He'll come through Greywatch, and soon.
CRANE: A refugee's fears, Majesty. Doomward has always tended to drama.
LEON: I fought the last war, Vesper. I know how wars start. Men in warm rooms calling the danger dramatic.
LEON: I also know how they end. I won't empty Fable's villages of their sons on one frightened scholar's word.
SB: With respect, Majesty, people are already dying. They just aren't yours yet.
! anim royal-guards step-forward
?
  1. Forgive my friend. It's been a long week.  ~ bond -2  → forgive
  2. {sb}'s right. Waiting costs lives.  ~ bond +5 · set mc-backed-sb-court  → backed
  3. (Say nothing.)  → nothing
#forgive LEON: There's nothing to forgive. I'd rather be told the truth rudely than lies politely.   [CALLBACK rudely]
SB [muttering]: I didn't need you to apologise for me.  → decision
#backed LEON: Then show me.  → decision
#nothing ELARA: Let them go to Greywatch, Father. Let them see it, and come back and tell you.  ~ affinity-elara +3  → decision
#decision LEON: Here is what I'll do. A supply caravan leaves for Greywatch Pass at dawn. Escort it, and bring me Commander Aldric's report in his own hand.
LEON: If it's as bad as you say, I'll call the banners. And when you come back, you'll kneel in this room and get up as Knights of Fable.
SB: Knights.
LEON: One more thing. Brannock Ironvein knows that pass better than anyone alive. He was my best captain. These days he's my most expensive drunk. Take him with you.  ~ set a1-c4-03 · objective "Find Brannock Ironvein at the Rusty Anvil, on the docks"
```

### A1-C4-04 · After court

#### script a1-c4-04 · map `sunspire-keep` · trigger in the long gallery

```
! place crane beside soulbound · camera soulbound · place you at gallery-end
NARRATOR: {sb} has stopped by a window. The Lord Chancellor has stopped beside {sb_them}.
CRANE: A bracing speech. The king's been surrounded by careful men so long, I think he's forgotten what conviction sounds like.
SB: He seems like a good man.
CRANE: Oh, the very best. Good men make lovely kings in peacetime.   [CALLBACK lovely-kings]
CRANE (smile): Wars want a different sort of man. Something to think about, Sir {sb}.
SB: I'm not a knight.
CRANE: Not yet.  ~ ambition +5 · set crane-contact · set a1-c4-04
```

**Build.** The header names its map, `sunspire-keep`, whose great hall is the long gallery, so the game's start-up check finds `gallery-end` and Crane there.

### A1-C4-05 · The Rusty Anvil

#### script a1-c4-05 · map `rusty-anvil` · with `brannock` · needs `a1-c4-03` · unless `brannock-recruited` · marker quest

```
! place brannock at corner-table · anim brannock slump
BRANNOCK: If the king sent you, tell him I'm busy. If you're buying, sit down.
SB: Greywatch Pass. Dawn. The king says you know it.
BRANNOCK: Greywatch.
BRANNOCK: I held the inner gate there in the last war. Six days, forty men, waiting on a relief everyone swore was coming.
BRANNOCK: The gate held. The men didn't.
?
  1. Then come and make sure it doesn't happen again.  ~ affinity-brannock +3  → join
  2. Stay and drink, then. We'll manage.  → manage
  3. (Let {sb} answer.)  → sb
#manage BRANNOCK (angry): Like hell you will.  ~ affinity-brannock +2  → join
#sb SB: You lost them because someone above you wouldn't send enough. Come with us. We'll bring enough.
BRANNOCK [studying {sb}]: Careful, youngling. Every warlord I ever fought started out talking like that.  ~ set brannock-wary-sb  → join
#join ! anim brannock stand · hold brannock arm_back brannock-shield
BRANNOCK: Dawn, then. I'll be the one who isn't sober.  ~ join brannock · set brannock-recruited · set a1-c4 · objective "Meet the caravan at the east gate at dawn"
```

**Build.** Restored from the build: the `needs` and `unless`, without which talking to Brannock would play this before the audience, and again once he's joined.

---

## Chapter 5 · "The Road to Greywatch"

New from here on. The chapter opens on the `kestrel-ford` map, which is the highland road east with the ford in the middle of it.

### A1-C5-01 · The caravan

#### script a1-c5-01 · map `questeria` · trigger at the east gate · needs `a1-c4`

```
! place garrick by lead-wagon
CHAPTER: Chapter 5  ·  The Road to Greywatch
GARRICK: Six wagons, two days and one bad ford. If you hear me shout, get between the wagons and the trees.
KAEL: What if we hear you scream?
GARRICK: Then it's already too late, and you can do as you like.  ~ set a1-c5-01 · travel kestrel-ford start
```

#### script a1-c5-01-ford · map `kestrel-ford` · trigger at the ford

```
! anim garrick point
GARRICK: Riders on the far bank. Red scarves.
SB: Red Gulls? This far out?  ~ fight a1-c5-01-ambush
```

#### script a1-c5-01-ambush-after

```
! hold kael hand_front black-iron-blade · anim kael inspect
KAEL: Red Gulls can't afford steel like this. Look at the edge on it. Somebody bought their kit for them.
SB: Somebody who knew which morning we'd leave.
BRANNOCK: Only the court knew the route.
BRANNOCK: Last time a relief sat at this ford, it was on a Crane's orders.
{if with:mira} MIRA: Aubrey Crane. The chancellor's father. It's in the war records.
SB: Then I'd like to meet the son properly.  ~ set spy-seed-2 · set a1-c5-01-done · objective "Get the caravan to Greywatch"
```

Brannock's line is new. It's the first hint for the player that the relief at Greywatch forty-one years ago was a Crane decision (COMPANIONS.md), and it points {sb} at Crane by name. Hester's cottage opens in the village beside the ford (`07-companions.md`, Forty Letters).

### A1-C5-02 · The night camp

The camp is free to walk. Each companion has a camp talk in `07-companions.md` (`<id>-camp-c5`). {sb}'s is the last of them, and it's here.

#### script a1-c5-02 · with `soulbound` · needs `a1-c5-01-done` · marker quest

```
! place soulbound apart-from-fire · anim soulbound whittle · hold soulbound hand_front whittling-knife
SB: Remember the red moon? Bram's gutting knife, and you went white as a sail.
YOU: You went white as a sail.
SB (laugh): I was being supportive.
SB: I meant it, you know. Every word. Whatever's waiting at that pass, whatever I've got to turn into to get us through it, I'm still yours.
SB: Soul-Bound. Never leave the other behind.   [CALLBACK oath]
?
  1. You don't have to turn into anything.  ~ bond +5  → anything
  2. Whatever it takes. Together.  ~ bond +5 · ambition +3  → together
  3. Turn into what, exactly?  ~ set sb-reveals-dream  → what
#anything SB [not looking at you]: No. Course not.  → end
#together ! anim both clasp-hands · sigil both glow
SB: Together.  → end
#what SB: Big enough. Big enough that the next Thornbridge doesn't happen.
SB: Big enough that nobody sits in a warm room deciding whether to save people.  → end
```

### A1-C5-03 · Greywatch

#### script a1-c5-03 · map `greywatch` · trigger at the inner gate

```
! place aldric at inner-gate
ALDRIC: Supplies and an escort. That's more than I've had since spring.
ALDRIC (smile): Brannock? You old rock. I heard you'd drowned in a barrel.
BRANNOCK: I tried. They don't make them deep enough.
ALDRIC: Scouts put the Hollowed past the ridge. Thousands of them. They'll come tomorrow night.
ALDRIC: I've sent four riders to the king, lad. Not one of them's come back.
SB: Because somebody's stopping them on the road.
SB: Four riders, and the palace thinks it's quiet out here.
ALDRIC: Then you're all the help I'm getting. Eat. Sleep if you can. Tomorrow we hold or we don't.  ~ set a1-c5-03 · rest
```

---

## Chapter 6 · "The Hollow Knight"

### A1-C6-01 · The siege

#### script a1-c6-01 · map `greywatch` · plays on waking · needs `a1-c5-03`

```
! tint night · sfx war-horn
CHAPTER: Chapter 6  ·  The Hollow Knight
ALDRIC: Walls! Everyone who can hold a spear, on the walls!  ~ fight a1-c6-01-walls
```

The siege is three fights on the one map, each one's `Then` starting the next: `a1-c6-01-walls` (ladders and climbers, three wall sections), `a1-c6-01-gate` (the ram beast breaks the outer gate; fight in the courtyard), `a1-c6-01-varkhul` (the boss).

#### script a1-c6-01-gate-after · plays when the courtyard is cleared

```
! place aldric at inner-gate · anim aldric kneel
ALDRIC (tired) [to Brannock]: Hold the gate, old friend. Same as last time.
BRANNOCK (angry): Not forty-one. Do you hear me, Aldric? Not forty-one.
! place varkhul at outer-gate · tint torches-dim
NARRATOR: Every torch in the courtyard goes low as the knight walks through the broken gate.
VARKHUL: I held a gate once. For a king. He did not come either.
VARKHUL: The Master came. The Master gives. Kneel, and be given.  ~ fight a1-c6-01-varkhul
```

Barks in the Varkhul fight:

| When | Bark |
| --- | --- |
| The shard in his hilt pulses | `VARKHUL: More. It gives more.` |
| At half health, turning on {sb} | `VARKHUL: You. You are hungry too. I can smell it on you.` |
| At a quarter | The screen flickers black for two frames and {sb}'s card on the strip glitches. No line |

#### script a1-c6-01-varkhul-after

```
! anim varkhul downed · swap varkhul varkhul-unmasked
NARRATOR: The armour splits along its seams. Inside is a grey-haired man.
VARKHUL [his own voice]: Thank you.
VARKHUL: My daughter. Ashfall. Tell her her father was a knight. Once.
! anim varkhul die · drop varkhul hand_front · show shard-on-floor · sfx shard-hum
NARRATOR: The sword breaks. Something black skids across the stones and stops against {sb}'s boot.  ~ set varkhul-beaten
```

### A1-C6-02 · The shard

#### script a1-c6-02 · plays straight after `a1-c6-01-varkhul-after`

```
! slow 0.5 · camera soulbound · anim soulbound kneel · hold soulbound hand_front shard · light soulbound shard on
VOICE(sb): They were late. They're always late. You weren't.
VOICE(sb): You held this gate. Not the king. Not his court. You. What could you hold with more?
! backdrop maleficio-sky
MALEFICIO: Greywatch holds. How brave of it.
MALEFICIO: Don't be frightened. That was a knock on the door. I wanted to see who'd answer it.
MALEFICIO: And now I know.
MIRA: Teacher. Stop this. You wanted to feed people.
MALEFICIO (smile): I am feeding them, Mira. Doomward hasn't had a famine in ten years.
MALEFICIO: No famine, no war, no council arguing over grain while children die waiting for a vote. One will. Is that so terrible?
MALEFICIO [to {sb}]: You understand me. I watched you at Thornbridge through my soldiers' eyes. You know what it is to be small, and to watch a man in a good coat decide what happens to you.
SB (afraid): I'm nothing like you.
MALEFICIO: Not yet.
MALEFICIO: Tell King Leon he has until the thaw. When the snow goes, I come for Questeria. He can kneel or he can burn. I'm not particular.
! backdrop off · tint dawn
NARRATOR: The clouds come apart. It's morning.
```

### A1-C6-03 · Aftermath

#### script a1-c6-03 · plays straight after `a1-c6-02`

```
MIRA: {sb}, the shard. Give it to me. It has to be unmade inside an Archive circle. Please don't keep holding it.
SB (smile): Here. Ugly little thing.
! anim soulbound hand-over · hold mira hand_front shard-sliver · hold soulbound pouch shard · light soulbound shard off
NARRATOR: {sb} drops a black splinter into Mira's palm. You don't see {sb_their} other hand.
{if bond>=70} NARRATOR: {Sb_their} other hand is shaking.
?
  1. Your hand's shaking.  ~ set mc-noticed-shard  → shaking
  2. (Let it go.)  → aldric
{if bond<70, sb-tell-me} NARRATOR: {sb} doesn't look at you.
?
  1. You asked me to tell you. You sounded like him.  ~ bond +3  → agreed
  2. (Let it go.)  → aldric
! place brannock at aldric-bier · anim brannock kneel
#aldric BRANNOCK: Forty-one.  ~ set a1-c6-03  → end
#shaking SB (laugh): I just fought a nightmare in a tin can. Give me a minute.  → aldric
#agreed SB [quietly]: I asked, didn't I.
SB: I don't agree with him. I'm frightened that I could. That's different. It has to be.  → aldric
```

Sitting with Brannock is in `07-companions.md` (`brannock-greywatch`). It's where he scratches the first name into his shield, which makes the shield's names a live item (art spec §6).

### A1-C6-04 · The knighting

#### script a1-c6-04 · map `sunspire-keep` · trigger at the throne · needs `a1-c6-03`

```
! hold leon hand_front leon-sword
LEON: You warned me, and I doubted you. Aldric Voss paid for my caution. I won't pay that price twice.
LEON: From this hour, the banners of Fable are called.
LEON: Kneel.
! anim you kneel · anim soulbound kneel · anim leon dub you · anim leon dub soulbound
LEON: Rise, Sir {name}, Knight of Fable. Rise, Sir {sb}, Knight of Fable.
! sfx crowd-chant
NARRATOR: Outside, the crowd is chanting. It's {sb}'s name.
{if sb-crowd-loved} NARRATOR: The Thornbridge refugees are the loudest of them.
ELARA [quietly, to you]: They love {sb_them}. That's a strong thing to have in a war.
CRANE [applauding slowly]: Very strong.
SB (laugh) [to you]: Knights. Two cove kids. Do you remember what I said on the boat?
?
  1. A kingdom. You're a step closer.  ~ ambition +3 · bond +3  → step
  2. Not today. No crowns today.  ~ bond +3  → today
  3. [needs sb-warned-p] I remember what I said back.  ~ set sb-reminded  → back
#step SB: One step.  → end
#today SB (laugh): No crowns today.
NARRATOR: {sb} glances at the throne anyway.  → end
#back SB (smile): Then keep saying it. You're the only one I listen to.  → end
```

### A1-C6-05 · {sb}'s window

#### script a1-c6-05 · map `sunspire-keep` · {sb}'s quarters · plays as {sb} · needs `a1-c6-04`

Control switches to {sb} (Tab locked). {sb} walks to the window and the scene plays.

```
! tint night · hold soulbound hand_front shard · light soulbound shard on
NARRATOR: The larger piece. Mira got a splinter.
SB: I'll give it to her tomorrow. I want to know what it is first. That's all.
VOICE(sb): You told {name} nobody you love would ever kneel again. The king called his banners after a good man died for his caution.
VOICE(sb): How many more Aldrics are you prepared to wait for?
SB: Leon's a good king.
VOICE(sb): Good men make lovely kings in peacetime.   [CALLBACK lovely-kings]
! reflection soulbound delay 0.5
NARRATOR: In the glass, {sb}'s reflection smiles a moment after {sb} does.
SB: Just until I understand it.
! sigil soulbound flicker-violet · hold soulbound pouch shard
CHAPTER: End of Chapter 6  ·  The Knock at the Door  ~ ambition +10 · set sb-kept-shard
```

From now on {sb}'s belt pouch gives off a faint violet light on dark maps (art spec §6). Nobody remarks on it until Valencia.

---

## Chapter 7 · "The Snowmelt"

The winter is the first stretch of the game where you choose the order. Four quests open together: The Heron's Due ({sb}), The Last Silverbark (Sera), A Name on a Door (Kael, through the mole), and Forty Letters (Brannock). They're scripted in `07-companions.md`. Mira's Ember Index starts here and finishes at the Archive. Doomward (A1-C7-04) opens once the mole is found and two of the other three are resolved.

### A1-C7-01 · Winter

#### script a1-c7-01 · map `questeria-plain` · plays on arrival · needs `sb-kept-shard`

```
! tint snow · place leon reviewing-troops
CHAPTER: Chapter 7  ·  The Snowmelt
LEON: Fable hasn't called its banners in forty years. Most of these men have never seen a war. I'd like it to stay that way for them.
! place soulbound at training-yard · anim soulbound strike · trail soulbound violet
RECRUIT: Sir {sb}, how do you hit that hard?
SB (grin): Practice. And something to hit for.
NARRATOR: The snow will go in about eight weeks.  ~ set a1-c7-01 · objective "Winter in Questeria: Lord Harrow, the timber writ, the coin trail, and Brannock's letters"
```

The winter balcony with your romance partner is `<id>-romance-winter` in `07-companions.md`.

### A1-C7-02 · The Arcanum Spire

#### script a1-c7-02 · map `arcanum-spire` · with `thessaly` · marker quest

```
THESSALY: Mira Ashgrove. Your notes on the Black Box are the only useful thing to come out of Doomward in ten years. Sit.
THESSALY: The rest of you, don't touch anything.
MIRA: The Box draws on the Well of Embers under the Archive. While Maleficio's sitting on it he's close to unkillable.
MIRA: When his army marches, he'll stay behind to feed it.
THESSALY: So we cut off the head while the body's marching on us. A small team through Doomward. The Guild opens a gate to bring you home, and seals the Box when it's done.
THESSALY: One rule. Nobody touches the Box. Not to move it, not to study it, not to see if it's warm.
THESSALY: I've buried three apprentices who thought the rule was for other people.
SB: Understood.  ~ set a1-c7-02
```

### A1-C7-03 · The mole

#### script a1-c7-03 · map `low-ring` · trigger at Gannet's counting house · needs `a1-c7-02`

The coin trail runs through Mother Gannet's counting house in the Low Ring. Kael's quest resolves here (`kael-gannet` in `07-companions.md`), and then {sb} goes on alone.

```
KAEL: Every black-iron mark in the Low Ring goes through Gannet's back room and comes out as honest silver. Her ledger says "C" every time.
SB: Then I'll go and see C. Wait here.
YOU: Alone?
SB (grin): Two of us is a visit. One of us is a misunderstanding.  ~ set sb-went-alone
! fade out 0.5 · travel-actor soulbound sunspire-keep cranes-study · fade in 0.5
! place crane at desk · hold crane hand_front cube-seal-ring
CRANE: Ah. Sir {sb}. I did wonder which of you would get here first.
SB: You sold the caravan's route. Aldric's riders. Thornbridge.
CRANE: I sold information to whoever looked likely to win. That isn't treason. It's forecasting.
CRANE: You could have me hanged. Or I go on writing to Maleficio, and from now on I write what you tell me to.
NARRATOR: {sb} doesn't answer for some time.
! fade out 0.5 · travel-actor soulbound low-ring gannets · fade in 0.5
?
  1. Find anything?  → found
  2. [needs mc-noticed-shard] You went in alone again.  ~ bond +3  → alone
#alone SB (smile): Somebody's got to be quick.
SB: I'm telling you, though. That has to count for something.  → found
#found SB: Crane. He's the mole. I've turned him. He'll feed Maleficio a false date for the attack, and we tell Leon after.
?
  1. We tell Leon now.  ~ bond -3  → now
  2. Smart. Do it.  ~ ambition +5 · set mc-complicit-crane  → smart
#now SB: And Maleficio hears Crane's gone quiet the same day. Trust me with this one.
NARRATOR: {sb} does it anyway.  ~ set sb-controls-crane · set a1-c7-03  → end
#smart SB (grin): That's why I keep you.  ~ set sb-controls-crane · set a1-c7-03  → end
```

{sb} keeps Crane's secret because it works. The false date holds, and Maleficio's army marches a week late into Fable's prepared lines. That's one more lesson for {sb} that secrets pay.

### A1-C7-04 · Across the Ashen Strait

#### script a1-c7-04 · map `ashen-strait` · plays on arrival · needs `a1-c7-03`, `winter-two-done` · a gate set by code: `winter-two-done`, once two of `sb-heron-done`, `sera-silverbark-done` and `brannock-letters-done` are set

```
! tint ash
CARD: The banners march for the plain. Six of you sail east.
MIRA [quietly]: This was the orchard road. I walked it to the Archive every morning for nine years.
MIRA: Apricots both sides. You could smell them from the river.  ~ set a1-c7-04
```

### A1-C7-05 · Ashfall

#### script a1-c7-05 · map `ashfall` · with `brenna` · marker event

```
BRENNA: Fablers. Either you're very brave or you've taken a wrong turn.
YOU: Your father was Varkhul. He asked us to find you.
BRENNA (cold): My father died twelve years ago. What came out of that hall wore his armour.
?
  1. He remembered at the end. He said he was a knight once.  ~ fame +2  → truth
  2. He died a knight.  ~ affinity-mira +3  → softer
  3. (Let {sb} answer.)  → sb
#truth BRENNA [looking away]: He was.  → path
#softer BRENNA: Then he died better than he lived these last years. Good.  → path
#sb SB: He died because nobody came for him. We came for you.
BRENNA: Then I'll follow you, Fabler. You and nobody else.  ~ ambition +3 · set ashfall-loyal-sb  → path
#path BRENNA: The Thornwall's the only way to the Archive that isn't watched. My people know a path through it.  ~ set thornwall-guided · set a1-c7-05
```

Mira's father may be among Brenna's fighters (`mira-index` in `07-companions.md`). If you skip Ashfall, the Thornwall map is a dungeon instead of a walk.

---

## Chapter 8 · "The Hunger at the Heart"

### A1-C8-01 · The Ember Archive

#### script a1-c8-01-echo · map `ember-archive` · trigger at the first memory spot

```
! echo maleficio-young · echo mira-young
ECHO-MALEFICIO: If we could feed every child in Doomward, Mira, what wouldn't we do?
MIRA [to the echo]: Anything. That was the trouble.  ~ set a1-c8-01
```

Mira's Ember Index resolves in the Archive's lowest stacks (`mira-index-end` in `07-companions.md`).

### A1-C8-02 · The Well of Embers

#### script a1-c8-02 · map `well-of-embers` · trigger at the rim

```
! place maleficio at pit-edge · hold maleficio hand_front black-box · light maleficio box on
CHAPTER: Chapter 8  ·  The Hunger at the Heart
MALEFICIO: Mira. You came home.
MALEFICIO: And you've brought my favourite Fabler. You've carried a piece of me all winter, haven't you? Every time you held it, I felt you want something.
! face party soulbound
SB: He's lying. That's all he does.
?
  1. I believe {sb}.  ~ bond +5  → believe
  2. [needs mc-noticed-shard] Your hand shook at Greywatch. I saw it.  ~ set shard-semi-exposed  → shook
  3. (Draw your weapon.)  → draw
#shook SB [quietly]: After. Please. After we win.  → fight
#draw MALEFICIO: Loyal. I had loyal students once.  → fight
#believe {if with:sera} SERA: Of course you do.  → fight
#fight NARRATOR: Maleficio stands up from the edge of the pit.  ~ fight a1-c8-02-maleficio
```

The fight's three phases are as the old draft had them: the Scholar (Mira can interrupt his channels), the Sovereign (four Ember Conduits), and the Hunger, which ends the fight at a fifth of his health and plays the next scene.

### A1-C8-03 · Consumed

#### script a1-c8-02-maleficio-after

```
! anim maleficio stagger · anim maleficio lift-item
MALEFICIO: More. I need more. Give me all of it.
VOICE(maleficio): There's nothing left in you to give.
! anim maleficio consumed · swap maleficio hungering · sfx box-open
NARRATOR: The Box opens. It pours into him, and then it pulls, and he folds into it like paper into a fire.
MIRA (shock): It's eating him. It was always going to.  ~ fight a1-c8-03-hungering
```

{if shard-semi-exposed}, Sera or Brannock (whoever's in the party) barks during the Hungering fight: `{sb}, your light's wrong!`

### A1-C8-04 · Last words

#### script a1-c8-03-hungering-after

```
! show maleficio-face
MALEFICIO [his own voice]: Mira. I only wanted nobody to go hungry.
MIRA (sad): I know, teacher. I know you did.
! hide maleficio-face · show black-box · anim black-box spin · sfx shard-hum
! walk soulbound toward black-box slowly
VOICE(sb): He wanted bread. You want a world.
? timer 5 → tackled
  1. (Grab {sb}'s wrist.)  ~ bond +10 · set mc-stopped-sb-box  → wrist
  2. {sb}! Don't.  ~ bond +5  → called
#wrist ! anim you grab-wrist soulbound
SB (afraid): I'm all right. It's loud, {name}. It's so loud.  → end
#called NARRATOR: {sb} stops with {sb_their} fingers a hand's width from it.  → end
! anim brannock tackle soulbound
#tackled {if with:brannock} NARRATOR: Brannock hits {sb} at the waist. The two of them go down well clear of the Box.
{if with:brannock} SB (angry): Get off me! I was going to kick it into the pit!  ~ affinity-brannock +3 · ambition +5 · set brannock-saw-sb-reach  → end
{if !with:brannock} NARRATOR: {sb} stops with {sb_their} fingers a hand's width from it.  ~ ambition +5  → end
```

If Brannock isn't in the party, the clock running out still stops {sb}, but nobody stops {sb_them}: Ambition goes up and Bond doesn't.

### A1-C8-05 · Containment

#### script a1-c8-05 · plays straight after

```
! show guild-gate · place thessaly at guild-gate · show battlemages · sfx gate-open
THESSALY: Nobody move. Nobody breathe on it.
! anim battlemages chant · show stillglass-reliquary · anim black-box lift-into reliquary · sfx reliquary-seal
THESSALY: Contained. It goes to the Deepvault under the Spire, and it doesn't come out again.
THESSALY: Procedure. Did anyone touch it? Any part of it?
! camera soulbound
SB: Nobody touched the Box.
NARRATOR: That's true, as far as it goes.
{if shard-semi-exposed} NARRATOR: Mira looks at {sb}, and then away.  ~ set mira-suspects
THESSALY: The gate closes in one minute. Anyone who'd like to stay in Doomward, say so now.  ~ set a1-c8-05 · travel sunspire-keep throne
```

### A1-C8-06 · Victory

#### script a1-c8-06 · map `sunspire-keep` · plays on arrival

```
! sfx bells
LEON: Fable stands, and Doomward can breathe again. Archmage Varn's Guild will keep the Box for as long as there's a Fable to keep it in.
LEON: Sir {name}. Sir {sb}. I made you knights. Today I make you Champions of Fable, sworn to the crown and to each other.
! sfx crowd-chant
NARRATOR: The crowd shouts {sb}'s name first. Up in the gallery, Crane catches {sb}'s eye, and {sb} gives him the smallest nod.  ~ set a1-c8-06
```

### A1-C8-07 · The roof

#### script a1-c8-07 · map `sunspire-roof` · with `soulbound` · marker quest

```
! tint night · place soulbound at parapet
SB: Two cove kids and a sorcerer-king. Nobody back home's going to believe a word of it.
YOU: You nearly touched it.
SB: I know. It sounded like me. The best of me. The one who's never frightened of anything.
SB: But you were there. You're always there.
! anim soulbound show-palm · sigil soulbound glow
SB: That's why I won't end up like him.
! anim both clasp-hands · sigil both glow · sigil soulbound flicker-violet 1 frame
! camera north · show noxhallow-silhouette
NARRATOR: Far to the north, under a cold moon, a black castle with red windows.
CHAPTER: End of Act 1  ·  The Hunger at the Heart  ~ ambition +5 · set act1-done
```
