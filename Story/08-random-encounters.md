# Random encounters

The road between story beats. Every random encounter is written: there are no blank "3 wolves appear" rolls without a reason, and most of what you meet isn't a fight. The aim is a world that keeps happening while you walk through it, that notices which act it is and who's in your party, and that never makes you grind.

Notation as in `04-script-prologue-act1.md`. C# shapes are in the build plan (§7); this file says what the system does and what every encounter says.

---

## 1. Rules

1. **Rare enough to matter.** In wild zones, about one encounter every four to six rooms you walk into. On roads, half that. Never in towns, camps, interiors or dungeons with scripted fights.
2. **Never on top of the story.** No roll in a room with a story trigger that can still fire, during an `objective` marked urgent (a timer, a chase, an escort), in the five minutes after a scripted fight, or while a companion talk's "!" is waiting on someone in the party.
3. **Mostly not fights.** Target mix across a playthrough: fights 45%, events 30%, merchants 10%, companion moments 10%, rare 5%.
4. **You can usually say no.** A spotted encounter (most fights) offers Fight, Slip past (a Stealth check, advantage with Kael or Sera), or Back off (leave the room; the danger meter is halved, not cleared). Ambushes, which start in the foe's favour, are under a fifth of fights and never in the first ten minutes of a chapter.
5. **It knows where you are in the story.** Every entry has acts, flags and a party condition. Nothing from Act 1 walks around in Act 3 unless it means something.
6. **It's a setting.** Options menu: Encounters `Normal`, `Fewer` (half the rate, fights only on a spotted approach), `Story only` (events, merchants and companion moments; no random fights).

## 2. How it works

### 2.1 The danger meter

This follows the move to rooms (`12-rooms.md` §7). Every wild zone's header gets a region and a rate:

```text
region = mirewood
danger = 20
```

Each time the party walks into a room of a wild zone, the zone's `danger` is added to the party's danger meter (a room marked `road` adds half). When the meter passes the threshold, an encounter is rolled for that room and the meter resets. The threshold is drawn fresh after each encounter, between 80 and 120, so it can't be counted. At `danger 20`, that's every four to six rooms. Rooms you've cleared of an encounter don't roll again for ten minutes. `danger 0` zones (towns, interiors) never roll.

### 2.2 Crossing between regions

Walking from a wild zone into a wild zone of another region rolls once, at 25%, regardless of the meter. If it fires, the encounter is in the first room of the new zone, and the meter resets.

### 2.3 Picking one

1. Gather every entry whose `regions`, `acts`, `needs`, `unless`, `party`, `time` and cooldown pass.
2. If none pass, nothing happens (the meter still resets).
3. Weighted pick by `weight`. `Rare` entries have weight 1 to 3; everything else 6 to 20.
4. `once` entries set `enc-<id>-done` when they finish and never come up again. Repeating entries have a `cooldown` in encounters (not minutes): `cooldown 4` means at least four other encounters before this one can repeat.

### 2.4 Where it happens

In the room you've just walked into, AQW-style. A fight's foes are already standing in it, in a loose group on the far side from where you came in, with name plates (and a level, greyed if they're well below you). An event's people are placed the same way. You see them before they see you.

### 2.5 Approach

A fight entry has an approach: `spotted`, `ambush` or `either` (rolled: Perception check by the party's best, DC 12 by default; pass is spotted). The choice below comes up when you walk within 400 px of them or click them; walking the other way out of the room is also Back off.

**Spotted** plays its entry's opening lines, then:

```text
?
  1. (Fight.)  → fight
  2. [CHECK stealth 12 · adv with:kael, with:sera] (Slip past.)  → pass slipped · fail fight
  3. (Back off.)  → back
```

`#slipped` and `#back` are shared nodes (`NARRATOR: You go round them.` / `NARRATOR: You give them a wide berth.`). Back off halves the meter instead of resetting it.

**Ambush** skips the choice. The foes are hidden until you reach the middle of the room, then jump in; they act first in round one.

### 2.6 Fights

Random fights are `EncounterDefinition`s like any other, id `rnd-<region>-<name>`, fought in rows in the room (`11-combat.md`). They're tuned for one level below the party (`11-combat.md` §6.3) and give XP and loot like any fight, at 60% of a story fight's XP for the same foes, so walking about is worth something but never the best way to level.

A random fight can't be lost to a game over. `Lost` is the shared `rnd-lost` script: you wake at the region's last camp or inn with half health, short 10% of your gold.

#### script rnd-lost · plays instead of game over after a random fight

```
NARRATOR: You wake up by a fire somebody else built. Your purse is lighter.
```

### 2.7 Saving

The save gets: the meter, the current threshold, each entry's cooldown counter, and the `enc-*-done` flags (which are ordinary flags). That's all.

### 2.8 Entry fields

| Field | Meaning |
| --- | --- |
| `id` | `enc-` plus a name |
| `kind` | `fight`, `event`, `merchant`, `companion`, `rare` |
| `regions` | Map regions it can happen in |
| `acts` | 1, 2, 3 (any of) |
| `needs` / `unless` | Flags and conditions, same grammar as scripts |
| `party` | `with:x` conditions on who's in the party |
| `time` | `day`, `night`, `any`; or `eclipse>=n` |
| `weight` | Pick weight |
| `once` / `cooldown n` | Repeat rule |
| `approach` | Fights: `spotted`, `ambush`, `either` |
| `script` | The opening script id (every entry has one) |
| `fight` | The fight id, if any. Its `Then` is the after script |

---

## 3. Regions and foes

| Region | Maps | Act 1 | Act 2 | Act 3 |
| --- | --- | --- | --- | --- |
| `saltwind` | docks road, coast | Red Gull stragglers, gulls | smugglers | Protectorate patrol, deserters |
| `mirewood` | mirewood, quarry road | gloomfangs, rot-boars | thornlings, bandits | re-raised Hollowed, deserters |
| `kestrel` | east road, ford, Greywatch road | bandits, Doomward scouts | bandits | Protectorate patrol |
| `greywatch` | the pass | Hollowed footmen and hexers | rockjaws | Hollowed (crown-raised) |
| `doomward` | ash fields, Thornwall | Hollowed, ash-hounds, cube cultists | ash-hounds | Hollowed (crown-raised), Brenna's raiders if `!ally-ashfall` |
| `strait` | Saint Lumen's isles | — | Order patrols, drowned | — |
| `morvaine` | pass, Glasswood, Hollowmere road | — | thralls, tithe riders, dire bats; at `eclipse>=3`, blood-wights at noon | thrall remnants |
| `sunscar` | the waste | — | sun-scorched, glass scorpions | — |
| `highlands` | the Order's road | — | Order patrols | Order or Protectorate, by `sq3-gilded` |

Enemy ids that don't exist yet (rot-boar, thornling, rockjaw, ash-hound, cube-cultist, drowned, dire-bat, tithe-rider, blood-wight, sun-scorched, glass-scorpion, protectorate-watchman, protectorate-warden, crown-hollowed) are in the art spec's monster list (§9) and need `EnemyCatalog` entries. The build plan's §9 gives their stat targets.

---

## 4. The encounters

Thirty-six, by region. The header line under each is its entry; then its script.

### Saltwind and the city roads

#### enc-wren · event · regions `saltwind` · acts 1 · party `with:kael` · once · weight 10

```
! place wren at road · anim wren pickpocket you
NARRATOR: A girl of about ten bumps into you, says sorry, and is three steps away before you feel your belt.
KAEL: Wren. Put it back.
WREN: Kael! I didn't know they were yours.
KAEL: Everybody's mine till they're not. Put it back and say something nice.
WREN: Your boots are very clean.  ~ affinity-kael +2
```

#### enc-gull-stragglers · fight · regions `saltwind` · acts 1 · needs `grell-gone` · once · approach spotted · weight 8

```
! place red-gull ×3 at road-ahead
RED GULL: That's them. The two off the boat. Grell's in the harbour because of them.
RED GULL: There's three of us this time. And no Watch.  ~ fight rnd-saltwind-gulls
```

#### enc-net-mender · event · regions `saltwind` · acts 1, 2 · time day · cooldown 6 · weight 8

```
! place net-mender at roadside · anim net-mender mend
NET-MENDER: Mind the floats. I've laid them out to count.
{if with:soulbound} SB [crouching]: You're doing the edge wrong. Here. Over, under, then twice round. My mother's knot.
{if with:soulbound} NET-MENDER: Huh. That holds better. Where's she from?
{if with:soulbound} SB: Kettle Cove. You won't have heard of it.  ~ bond +1
```

#### enc-checkpoint · event · regions `saltwind`, `kestrel`, `mirewood` · acts 3 · needs `soul-bond-broken` · cooldown 3 · weight 16

```
! place protectorate-watchman ×2 at road-ahead · show checkpoint-barrier
PROTECTORATE WATCHMAN: Papers. Everyone on the road has papers now. For your safety.
?
  1. [needs with:kael] (Let Kael handle it.)  → kael
  2. [PERSUADE 13 · adv voice:witty] Left them in my other coat.  → pass waved · fail fight
  3. (Go back the way you came.)  → back
#kael KAEL: Papers. Here. Signed by Captain Hale himself, look at that flourish.
PROTECTORATE WATCHMAN: That's not Hale's hand.
KAEL: Well, it's somebody's.  ~ fight rnd-checkpoint  → end
#waved PROTECTORATE WATCHMAN: Go on. Get one by the next post.  → end
#fight PROTECTORATE WATCHMAN: Right. Hands where I can see them.  ~ fight rnd-checkpoint  → end
#back NARRATOR: You give them a wide berth.  → end
```

`rnd-checkpoint` is `Nonlethal`: they're the Watch.

#### enc-statue · event · regions `saltwind` · acts 3 · needs `a3-c2-04` · once · weight 10

```
! show statue-scaffold · place mason at scaffold
NARRATOR: Masons are putting up a statue at the crossroads. It's half done: a cloak, a raised hand, no face yet.
MASON: The Lord Protector. They're going up in every town on the coast. We're doing the face last. It's the hard part.
{if with:sera} SERA: Somebody should tell them the hand's wrong. {sb} never held a hand up like that in {sb_their} life.
```

### The Mirewood

#### enc-woodcutters-boy · event · regions `mirewood` · acts 1 · needs `a1-c2-01` · unless `crystal-dealt-with` · once · weight 14

```
! place lost-boy at tree-hollow · anim lost-boy hide
LOST BOY: Are you the Lodge? My da went out for timber. He said he'd be back for tea. That was Tuesday.
?
  1. We'll take you to the Lodge.  ~ fame +1 · affinity-sera +2  → lodge
  2. What's your da's name?  → name
#name LOST BOY: Hobb. He's big. He's got a red beard. You'd know him.  ~ set know-hobb  → lodge
#lodge ! walk lost-boy to party
NARRATOR: He walks close enough to step on your heels the whole way.  ~ give lodge-token
```

Hobb comes home after the crystal (he's one of the two woodcutters not home). If `know-hobb`, he's the foreman at Sera's table (§3.2 of `07`) and says: `HOBB: You're the one who walked my lad home. Sit there. Best seat.` Write that line as a conditional on `sera-table`.

#### enc-otters · companion · regions `mirewood` · acts 1, 2, 3 · party `with:sera` · cooldown 8 · weight 8

```
! place sera at stream-bank · anim sera kneel
SERA: Otters. Four. Up from two in the spring.
{if crystal-shattered} SERA: Two in the spring because the stream was black. That was us. Don't make that face. It was us.
{if !crystal-shattered} SERA: They've come back up the stream. They don't hold grudges. Lucky for us.
```

#### enc-honey-pedlar · event · regions `mirewood` · acts 1 · party `with:sera` · once · weight 6

```
! place pedlar at road · hold pedlar hand_front honey-cakes
PEDLAR: Honey cakes from the city! Two for a copper, and I'll throw in the directions to the old quarry for nothing.
! camera sera · anim sera stand-still
SERA: Keep your cakes.
PEDLAR: No offence meant, madam.
SERA: None taken. Keep walking.
?
  1. (Ask her later.)  ~ affinity-sera +3 · set sera-honey-seen  → end
  2. What was that about?  → about
#about SERA: Not today.  → end
```

`sera-honey-seen` unlocks one extra line in `sera-talk-2`'s honey-cakes story: `SERA: You saw the pedlar. That's what they looked like. Exactly that pleasant.`

#### enc-rot-boars · fight · regions `mirewood` · acts 1 · unless `crystal-dealt-with` · cooldown 3 · approach either · weight 14

```
! place rot-boar ×2 at undergrowth
NARRATOR: Two boars come out of the brambles with black veins under their skin and blood in their eyes.  ~ fight rnd-mirewood-boars
```

#### enc-deserters · fight · regions `mirewood`, `kestrel` · acts 3 · needs `a3-c3-01` · cooldown 4 · approach spotted · weight 12

```
! place protectorate-watchman ×3 at clearing · anim protectorate-watchman sit-fire
PROTECTORATE WATCHMAN: Easy. Easy. We're not looking for a fight. We walked off our post three days ago.
PROTECTORATE WATCHMAN: We heard about Thornbridge. My sister's in Thornbridge.
?
  1. The camp's that way. Tell them I sent you.  ~ set deserters-sent  → camp
  2. [needs voice:pragmatic] Your boots and swords stay with us.  → kit
  3. How do I know you're not scouts?  → scouts
#camp PROTECTORATE WATCHMAN: Thank you, Sir. If we see anyone following us, we'll lose them in the bog.  → end
#kit PROTECTORATE WATCHMAN: Fair. Fair. You'll want them more than we will.  ~ give watch-swords  → end
#scouts PROTECTORATE WATCHMAN: You don't. That's the trouble with all of it now, isn't it.  → end
```

Each `deserters-sent` adds one rank of Watch to the siege muster (A3-C4-01), up to three.

#### enc-crown-hollowed · fight · regions `mirewood`, `greywatch`, `doomward` · acts 3 · needs `crown-forged` · cooldown 3 · approach either · weight 14

```
! place crown-hollowed ×3 at road · light crown-hollowed violet
NARRATOR: Three Hollowed in Protectorate sashes, walking in step down the road. Their eyes have violet in them now.
{if with:mira} MIRA: The crown's raised them. They're answering it, not him. Not my teacher.  ~ fight rnd-crown-hollowed
{if !with:mira} NARRATOR: They turn toward you all at once.  ~ fight rnd-crown-hollowed
```

### The Kestrel road and Greywatch

#### enc-relief-ghost · rare · regions `kestrel` · acts 1, 2, 3 · party `with:brannock` · time night · once · weight 2

```
! tint night · place relief-column-echo at ford · light relief-column-echo pale · anim relief-column-echo march-in-place
NARRATOR: At the ford there are campfires that weren't there an hour ago. Men round them in old-pattern armour. Nobody moves.
BRANNOCK [quietly]: That's the relief. Forty-one years ago. That's exactly where they sat.
BRANNOCK: Go on. Go up there and tell them. Go on, the lot of you.
! fade out 0.5 · hide relief-column-echo · fade in 0.5
NARRATOR: The fires go out one by one. The ford's dark.
{if brannock-letters-delivered} BRANNOCK: Funny. I thought I'd mind more.  ~ affinity-brannock +3
{if !brannock-letters-delivered} BRANNOCK: Don't ask me. Not tonight.
```

In Act 3 the echo's armour is the colour of the time-scar (art spec §7), and Thessaly, if she's heard, says one line at the next camp: `THESSALY: A relief column at Kestrel Ford, forty-one years out of place? That's the scar. It's leaking already.`

#### enc-doomward-scouts · fight · regions `kestrel` · acts 1 · needs `a1-c4` · unless `a1-c6-03` · cooldown 3 · approach spotted · weight 14

```
! place cube-cultist at road · place hollowed-footman ×2 at road
CUBE CULTIST: Fablers. The Master's compliments. Hold still and this'll be brief.  ~ fight rnd-kestrel-scouts
```

#### enc-pilgrims · event · regions `kestrel`, `highlands` · acts 1, 2 · cooldown 6 · weight 10

```
! place pilgrim ×3 at road · hold pilgrim hand_front sun-token
PILGRIM: Sun's blessing on you. We walk to Sunfall every year, to see where the old priests lit the world.
{if sb-knows-purge} SB [low, to you]: Somebody ought to tell them what the old priests lit.
?
  1. Let them walk.  ~ bond +1  → walk
  2. Go on, then. Tell them.  ~ ambition +2  → tell
PILGRIM: Walk with us a while if you're going east. We've bread.  → end
#walk SB: Fine. Let them walk.  → end
#tell SB: Excuse me. The square at Sunfall. Do you know what happened in it?
PILGRIM: A miracle, Sir. The sun came down.
SB: Yes. It did.  → end
```

#### enc-rockjaws · fight · regions `greywatch` · acts 2 · cooldown 3 · approach either · weight 12

```
NARRATOR: The scree moves. Then it stands up.  ~ fight rnd-greywatch-rockjaws
```

### Doomward

#### enc-apricot · companion · regions `doomward` · acts 1, 2, 3 · party `with:mira` · once · weight 10

```
! place apricot-tree at ash-field · light apricot-tree green · place mira at apricot-tree
NARRATOR: In the middle of a grey field, one apricot tree in leaf.
MIRA: That shouldn't be possible. The roots must go down past the ash, to the old river.
MIRA: I'm going to sit here for a minute. You don't have to wait.
?
  1. (Sit with her.)  ~ affinity-mira +3  → sit
  2. (Wait at the road.)  → road
#sit NARRATOR: Neither of you says anything. A bird you can't name lands in the branches and leaves again.  → end
#road NARRATOR: She's there a quarter of an hour. When she comes back she's holding a leaf.  → end
```

#### enc-ash-hounds · fight · regions `doomward` · acts 1, 2 · cooldown 3 · approach either · weight 14

```
! place ash-hound ×3 at ridge
NARRATOR: Grey dogs on the ridge, the colour of the ground. You only see them when they move.  ~ fight rnd-doomward-hounds
```

#### enc-black-iron · event · regions `doomward`, `kestrel` · acts 1 · needs `mira-talk-2-done` · once · weight 8

```
! place dead-courier at roadside · show black-iron-purse
NARRATOR: A dead man in a ditch, a day old. Wolves haven't touched him. His purse is full of black-iron marks.
{if with:mira} MIRA: My drawing. On every one. I never get used to it.
?
  1. (Take the marks for evidence.)  ~ give black-iron-marks  → take
  2. (Bury him.)  ~ affinity-mira +2 · affinity-sera +2  → bury
#take NARRATOR: Thirty-one marks. Someone was going to be paid a lot.  → end
#bury NARRATOR: You bury him with the purse. It's what Doomward does, Brenna told you. Money goes in the ground with whoever earned it.  → end
```

`black-iron-marks` gives advantage on the `kael-gannet` scene's opening if you show them (an extra option, `[needs black-iron-marks]`, `GANNET: Where did you get those?`).

#### enc-brenna-raiders · fight · regions `doomward` · acts 3 · needs `ashfall-loyal-sb` · unless `ally-ashfall` · cooldown 4 · approach spotted · weight 10

```
! place ashfall-raider ×3 at ridge · anim ashfall-raider draw
ASHFALL RAIDER: Fablers in Doomward. The Protector's orders are nobody crosses. Turn round.
?
  1. Brenna knows us.  → brenna
  2. (Fight.)  → fight
#brenna ASHFALL RAIDER: Brenna knows a lot of people. She knows the Protector best.  ~ fight rnd-doomward-raiders  → end
#fight ASHFALL RAIDER: Your choice.  ~ fight rnd-doomward-raiders  → end
```

Nonlethal. They're Brenna's people, and A3-C3-03 goes better if none died (a `rnd-doomward-raiders` won without a knockout sets nothing; this is flavour, not a penalty).

### The strait and Saint Lumen's

#### enc-drowned · fight · regions `strait` · acts 2 · time night · cooldown 3 · approach ambush · weight 10

```
! tint night · place drowned ×3 at tideline · anim drowned rise
NARRATOR: Something comes up out of the shallows on hands and knees. Then two more.  ~ fight rnd-strait-drowned
```

#### enc-bees · companion · regions `strait` · acts 2 · party `with:valencia` · needs `valencia-orla-saved` · once · weight 10

```
! place orla at hives · hold orla hand_front smoke-pot · place valencia at shade-edge
ORLA: You look thin.
VALENCIA: Orla, you've said that every day for thirty years.
ORLA: And I've been right every day. Come here. The bees want to see you.
! walk valencia to hives · anim valencia stand-sun
NARRATOR: Valencia stands in the sun by the hives. The bees don't mind her at all.  ~ affinity-valencia +3
```

### Morvaine

#### enc-tithe-riders · event · regions `morvaine` · acts 2 · unless `a2-c6-04` · cooldown 4 · weight 14

```
! place tithe-rider ×2 at road · place hollowmere-farmer at road · hold tithe-rider hand_front tithe-cup
TITHE RIDER: The cup, goodwife. It's the new moon.
HOLLOWMERE FARMER: I gave last month. It's my boy's turn. He's eleven.
?
  1. (Step between them.)  → step
  2. [needs with:valencia] (Let Valencia speak.)  → val
  3. (Keep walking.)  ~ affinity-valencia -3  → walk
#step TITHE RIDER: Southerners. Lord Severin will hear of this.  ~ fight rnd-morvaine-riders  → end
#val VALENCIA: Do you know who I am?
TITHE RIDER: My lady.
VALENCIA: Go home. Tell my father the boy's cup was paid by me.  ~ affinity-valencia +5  → end
#walk NARRATOR: Behind you, the boy holds out his arm.  → end
```

#### enc-noon-wights · fight · regions `morvaine`, `kestrel`, `mirewood` · acts 2 · time `eclipse>=3` · cooldown 3 · approach ambush · weight 16

```
! tint eclipse · place blood-wight ×2 at road
NARRATOR: Noon, and the sun's the colour of a bruise. Something that should only walk at night is walking.
{if with:valencia} VALENCIA: Blood-wights. They can stand the light now. I'd like it noted that I can too, and I'm not happy about it either.  ~ fight rnd-noon-wights
{if !with:valencia} NARRATOR: They've seen you.  ~ fight rnd-noon-wights
```

#### enc-hollowmere-child · event · regions `morvaine` · acts 2 · once · weight 8

```
! place hollowmere-child at roadside · hold hollowmere-child hand_front wooden-sword
HOLLOWMERE CHILD: Are you the southerners? Mam says southerners burn people.
?
  1. Not us.  → not
  2. Some did. A long time ago.  → some
#not HOLLOWMERE CHILD: That's what a southerner would say.  → end
#some HOLLOWMERE CHILD: Mam says that too. Then she says, but they're far away.  → end
```

#### enc-thrall-patrol · fight · regions `morvaine` · acts 2 · unless `a2-c6-04` · time night · cooldown 3 · approach either · weight 14

```
! tint night · place thrall ×3 at road · hold thrall hand_front lantern
THRALL: Halt in the Sovereign's name. Show your throats.  ~ fight rnd-morvaine-thralls
```

### The Sunscar

#### enc-mirage · rare · regions `sunscar` · acts 2 · once · weight 3

```
! tint ember-sun · backdrop purging-dawn-mirage · wait 2 · backdrop off
NARRATOR: For a moment, on the horizon, a city square full of people with their hands up against a falling light.
NARRATOR: Then it's heat over sand.
{if with:soulbound} SB: You saw it too. Tell me you saw it too.
{if with:valencia} VALENCIA: He was nine. He was standing about there.
```

#### enc-glass-scorpions · fight · regions `sunscar` · acts 2 · cooldown 3 · approach ambush · weight 14

```
NARRATOR: The sand clicks.  ~ fight rnd-sunscar-scorpions
```

### Act 3 only

#### enc-granary · event · regions `saltwind`, `kestrel`, `mirewood` · acts 3 · needs `a3-c3-01` · cooldown 6 · weight 10

```
! place farmer at cart · hold farmer hand_front grain-sack · show protectorate-granary
FARMER: Free grain at the Protector's granary. First time in my life I've had more than I needed in winter.
FARMER: You lot are the resistance, aren't you. I won't tell anyone. I just don't understand what you're resisting.
?
  1. Thornbridge.  → thornbridge
  2. You'll find out.  → out
  3. (Say nothing.)  → nothing
#thornbridge FARMER: I heard. I heard. But the grain, though.  → end
#out FARMER: That's what my da said about the last war. He was right, too.  → end
#nothing FARMER: Suit yourself. Good luck, I suppose.  → end
```

This entry is the one that should make the player uncomfortable. It's meant to. Don't give it a right answer.

#### enc-stopped-hare · rare · regions any · acts 3 · needs `a3-c3-06` · once · weight 2

```
! place hare at field · anim hare frozen · light hare violet
NARRATOR: A hare in mid-leap, a foot off the ground. It isn't moving. A little violet light is caught in its fur.
{if with:mira} MIRA: Like Thornbridge. Just a hare. Somehow that's worse.
```

#### enc-leviathan-shadow · rare · regions `saltwind`, `strait` · acts 3 · needs `a3-c5-04` · once · weight 3

Plays only in the days between the tear and the coronation (A3-C5-04 to A3-C5-06), if the player walks the coast.

```
! tint flicker-day-night · show leviathan-shadow at sea · anim leviathan-shadow cross
NARRATOR: A shadow crosses the sea from one horizon to the other, too big to be a cloud. It has fins.
NARRATOR: The water underneath it goes calm, then goes on as if nothing happened.  ~ set saw-leviathan
```

### Merchants

#### enc-mags · merchant · regions any wild · acts 1, 2, 3 · cooldown 5 · weight 8

Pedlar Mags: a Stoneborn woman with a donkey called Duchess. She's the same merchant in every act; her stock and her one line change.

```
! place mags at roadside · place duchess beside mags
{if act:1} MAGS: Rope, salt, liniment, and a lucky stone that's never once been lucky. What'll it be?  → ask
{if act:2} MAGS: Lanterns are dear this year. Something about the sun. Don't ask me, I only sell the things.  → ask
MAGS: Protectorate paper says I'm to charge you double. I can't read, so you'll have to tell me what it says.  → ask
#ask MAGS: Well? Duchess hasn't got all day. She has, actually, but she doesn't like it known.
?
  1. Show me your wares.  ~ shop mags  → end
  2. Teach me to travel like you.  → teach
#teach MAGS: Four hundred. It's mostly knowing which end of the donkey to stand at.  ~ train mags
```

`shop <id>` is the existing effect; `mags` and `wager-crate` are new `ShopCatalog` entries (build plan §3.5). `act:n` is a new condition reading the current act.

#### enc-odelle-crew · merchant · regions `saltwind` · acts 1, 2 · once per act · weight 8

```
! place gull-wager-sailor at road · hold gull-wager-sailor hand_front crate
SAILOR: The two off the Wager. You've gone up in the world. Odelle says if you want southern goods, we've a crate going cheap. It fell off the ship.
SAILOR: It fell off the ship in the harbour at Questeria, in front of the customs house, but still.  ~ shop wager-crate
```

### Companions on the road

#### enc-burr · companion · regions `mirewood`, `kestrel`, `highlands` · party `with:sera` · cooldown 10 · weight 6

```
! show burr at sky · anim burr land sera
NARRATOR: A grey-winged kestrel drops onto Sera's glove with something in its beak.
SERA: Burr. What've you got. A key? Where did you get a key?
SERA: He won't say. He never says.  ~ give burr-trinket
```

`burr-trinket` is drawn from a small loot table (a key that opens one chest somewhere in the region, a ring, a copper button).

#### enc-kael-prices · companion · regions any wild · party `with:kael` · cooldown 10 · weight 6

```
! place kael at roadside-shrine · anim kael look-at-shrine
KAEL: That shrine's got a silver bowl in it. I'm not going to take it. I just want everyone to know I noticed.
{if with:brannock} BRANNOCK: Noted.
{if !with:brannock, with:soulbound} SB: Noted.
```

#### enc-brannock-drink · companion · regions any wild · party `with:brannock` · acts 1 · unless `brannock-letters-done` · cooldown 10 · weight 6

```
! place brannock at roadside · anim brannock drink-flask
BRANNOCK: Want some?
?
  1. (Take a drink.)  → drink
  2. Not on the road.  → not
#drink BRANNOCK: Good. I hate drinking alone. I do it all the time, but I hate it.  ~ affinity-brannock +1  → end
#not BRANNOCK: Fair. More for me. That's not the point you were making, I know.  → end
```

#### enc-sb-whittle · companion · regions any wild · party `with:soulbound` · acts 1, 2 · cooldown 10 · weight 6

```
! place soulbound at roadside · anim soulbound whittle · hold soulbound hand_front whittling-knife
SB: Sit down a minute. Your face says your feet hurt.
{if act:1} SB: I'm doing you a gull. It's going to look like a herring with ideas. I only know how to do herring.
{if act:2} SB: I haven't whittled anything since the abbey. I keep starting a crown by accident. Look. That's a crown, isn't it.  ~ ambition +1
```

That Act 2 line is the only place the random table touches {sb}'s slide. It's small, and it's on purpose.

---

## 5. Side quests these open

### 5.1 SQ-Pell: the blood-house

Opens at `sq-pell-started` (A2-C3-02). The blood-house is a manor on the Hollowmere road (map `blood-house`). Ide is in the cellar, a "reserve" past her month.

#### script sq-pell-door · map `blood-house` · trigger at the gate · needs `sq-pell-started`

```
! place blood-house-steward at gate · hold blood-house-steward hand_front ledger
BLOOD-HOUSE STEWARD: The house isn't receiving. It's the dark of the moon.
?
  1. [needs with:valencia] (Let Valencia go first.)  → val
  2. [PERSUADE 15 · adv voice:pragmatic] We're from the Sovereign. Inspection.  → pass in · fail sneak
  3. (Go round the back.)  → sneak
#val VALENCIA: You know my face.
BLOOD-HOUSE STEWARD: My lady. We thought, that is, the household thought you were dead.
VALENCIA: The household was wrong. Open the cellar.  → in
#in BLOOD-HOUSE STEWARD: This way.  ~ set blood-house-open  → end
#sneak NARRATOR: The back wall's low. The kitchen door isn't locked; nobody in Morvaine expects anyone to come in uninvited.  ~ set blood-house-sneak  → end
```

`blood-house-sneak` makes the house a stealth map (patrol cones; caught means the fight `sq-pell-house`). `blood-house-open` walks you straight down.

#### script sq-pell-cellar · map `blood-house` · trigger at the cellar door

```
! place ide at cellar-cot · anim ide sit-up · tint candle
IDE: Pell sent you? Pell can't even send a letter. He can't spell "sister".
IDE: They said I was a reserve. That means extra. For when the lords have guests.
?
  1. You're coming home.  → home
  2. Can you walk?  → walk
#home IDE: Home's here. Hollowmere. Unless it isn't any more.  → walk
#walk IDE: If I lean on you. I'll lean on you.  ~ set ide-rescued · give ide-token
```

Bringing Ide to Pell (`sq-pell-reunion`, at Hollowmere) is one line from each: `PELL: You're thin.` / `IDE: You're stupid. Same as ever.` and gives a Hollowmere reward. Ide and Pell move to Questeria in Act 3 (A3-C1-02).

### 5.2 SQ-Valencia's Trail

Opens at `valencia-fled` (A2-C2-02). She went to ground in a sea-cave on the strait.

#### script sq-valencia-trail · map `strait-cave` · trigger at the back of the cave · needs `valencia-fled`

```
! tint cave · place valencia at cave-back · anim valencia sit
VALENCIA: You let the Order have me. You were going to.
?
  1. I was wrong.  ~ affinity-valencia +5  → wrong
  2. I didn't know what you were.  → know
#wrong VALENCIA: Yes. You were. I don't hear that often from people with swords.  → come
#know VALENCIA: Nobody does. That's what forty years of good manners buys you.  → come
#come VALENCIA: Fine. I'll come. You owe me, and I intend to collect.  ~ set valencia-found · clear valencia-fled
```

`valencia-found` makes A2-C2-03 playable; its opening line becomes `VALENCIA: Valencia Morvaine. You've met me twice now. The second time was in a cave, which I'd rather not discuss.` (written as a conditional first node).

---

## 6. Thornbridge: the still lives

At A3-C3-06 the frozen district can be walked. Ten citizens can be examined. After A3-C5-05 they're unfrozen, and each has an after line. Ten pairs, each `still-<n>` (examine, frozen) and `still-<n>-after` (talk, unfrozen). Six are people from Act 1's Thornbridge; four are new.

| # | Who | Frozen | After |
| --- | --- | --- | --- |
| 1 | Hedda, from the gate | `NARRATOR: Hedda, from the east gate. She's mid-shout, holding a loaf over her head out of someone's reach.` | `HEDDA: How long was I standing there? My arms don't hurt. Why don't my arms hurt?` |
| 2 | Lise, her daughter | `NARRATOR: Lise, Hedda's girl. Behind her mother, eyes shut, both hands over her ears.` | `LISE: I'm forty-six. Mam says I'm forty-six. I was twelve on Tuesday.` (aged by the tear) |
| 3 | Sergeant Brisk | `NARRATOR: Sergeant Brisk, between two men, arms out. Trying to push them apart.` | `BRISK: I was keeping the peace. Nobody told me what to do. So I just did it.` |
| 4 | The baker (Kael's) | `NARRATOR: A baker with flour to the elbows, a ledger under his arm. His mouth's open on a number.` | `BAKER: I owe Duskmantle three silver. I know. I was standing there knowing it for six weeks.` |
| 5 | The boy with the fever (Mira's errand) | `NARRATOR: The boy from the end tent, grown a foot. He's throwing a stone. It hangs in the air two yards from his hand.` | `BOY: Lady Mira? Is Lady Mira here? Tell her I got better.` |
| 6 | A Watch recruit | `NARRATOR: A Protectorate recruit with his cudgel half-raised. He's about sixteen.` | `RECRUIT: I wasn't going to hit anyone. I want that on record. I wasn't going to.` |
| 7 | A grandmother | `NARRATOR: An old woman sitting on a step, shelling peas into her apron. The only person in the street not shouting.` | `GRANDMOTHER: Riot? I didn't see any riot. I had peas to do.` |
| 8 | A child (the baby) | `NARRATOR: A girl of about seven, running. One shoe has come off and is still in the air behind her.` | `NARRATOR: Where the girl was, a woman is holding a baby wrapped in a coat too big for it. She's crying and won't let anyone help.` |
| 9 | A fiddler | `NARRATOR: A fiddler on a barrel, bow on the string. Snow lies on the fiddle and doesn't melt.` | `FIDDLER: It's the same note. I've had the same note in my head for six weeks. I'm never playing it again.` |
| 10 | Pell (if `ide-rescued`) or a dock porter | `NARRATOR: Pell from Hollowmere, holding the door of a bread shop open for someone who isn't there.` / `NARRATOR: A porter with a sack of flour on one shoulder.` | `PELL: Ide was inside. Is Ide all right? Somebody tell me Ide's all right.` / `PORTER: I've dropped it. Six weeks I've held this and I drop it now.` |

---

## 7. Coverage check

| | Act 1 | Act 2 | Act 3 |
| --- | --- | --- | --- |
| Fights | gulls, boars, scouts, hounds | rockjaws, hounds, drowned, riders, wights, thralls, scorpions | checkpoint, deserters (choice), crown-hollowed, raiders |
| Events | wren, net-mender, boy, pedlar, pilgrims, black-iron | net-mender, pilgrims, tithe riders, child | checkpoint, statue, granary |
| Merchants | Mags, Wager crew | Mags, Wager crew | Mags |
| Companions | otters, Brannock's flask, {sb} whittling, Burr, Kael | otters, bees, {sb} whittling, apricot, Burr, Kael | otters, apricot, Burr, Kael |
| Rare | relief ghost | relief ghost, mirage | relief ghost, stopped hare, leviathan |

Every companion has at least one road moment in each act they're in. Every region has at least one fight and one non-fight in each act it's visited.
