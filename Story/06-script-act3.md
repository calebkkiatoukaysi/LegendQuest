# Script: Act 3 · "Soul-Bound"

All new. Notation as in `04-script-prologue-act1.md`.

**Fights.** Where a scene describes a fight's layout (tiles, circles on the field, walking into a zone), `11-combat.md` §5 is what to build: fights are side-view rows in the room now, and that section restates every special fight. Who helps in each fight is `11-combat.md` §4.1.

One year after the Long Eclipse. {sb} keeps the outer seal of the Deepvault and sees, every night, the end of the world. {sb} fuses the Box and the Eye into one crown, kills the king without meaning to, cuts the Soul-Bond to keep you out of what comes next, and rules Fable well enough that people cheer. You lead the resistance. The last fight cracks the crown and tears time open, and the thing that has been speaking in {sb}'s voice since Greywatch walks through.

**The four rules for writing {sb} in this act** (from `COMPANIONS.md`, restated because this is where they're tested):

1. {sb} is right about the problem every time and wrong about the answer.
2. Every escalation protects somebody small. {sb} never sneers at anyone weak.
3. {sb} apologises and means it, then does it again.
4. {sb} always offers you a place beside {sb_them}. Even at the end.

{sb}'s pronoun drift reaches its last stage: "you", meaning other people's understanding ("You'll see"), and "them", meaning everyone, said tenderly. {sb} still never says "obey" or "peasant". The word is "safe".

**Mechanics this act adds** (build plan §3):

- **Bond tiers.** High is `bond>=70`, mid is `bond>=40, bond<70`, low is `bond<40`. They decide the coup's last line, the summit's opening, and the ending.
- **The broken Soul-Bond.** `soul-bond-broken` turns off the Soul-Link combo and greys your palm sigil (`sigil you grey`). Nothing replaces it until the summit's third phase. The player should miss it.
- **The Protectorate map.** A region overlay on the world map: violet regions are {sb}'s, gold are liberated. Four liberation quests in Chapter 3; two are needed to go on, and each adds a unit to the siege.
- **Kael's side** is fixed at A3-C2-04 by comparing `affinity-kael-sb` with `affinity-kael`: higher for {sb} sets `kael-with-sb`, otherwise `kael-with-you`.
- **Live items this act** (art spec §6): the Crown of Dusk and Dawn on {sb}'s head socket; the outer key; {sb}'s herring, left on a table; King Leon's sword, which goes to Elara and stays on her; Valencia's vial; Brannock's shield.

---

## Chapter 1 · "The Lord Protector"

### A3-C1-01 · The vision

#### script a3-c1-01 · map `questeria-still` · plays first, as {sb} · needs `act2-done`

The dream map is a copy of `questeria` with every citizen placed mid-step, `anim <x> frozen` on all of them, snow particles stopped in the air, and a black sky with no stars. Control is {sb}'s. Walking up the main street to the Sunspire steps starts the lines below one at a time (trigger tiles).

```
CARD: Act 3 · Soul-Bound  ~ act =3
! tint starless · music none · sfx wind-stopped
VOICE(sb): You always said somebody ought to be ready.   [first trigger]
VOICE(sb): Look how ready they are.   [second trigger, beside a frozen baker]
! place vorlaith-shape at sunspire-top · camera sunspire-top
NARRATOR: At the top of the Sunspire stands a tall shape of cracked glass. It wears a crown of broken clock-hands.
SB: Who are you?
VOICE(sb): They'll vote on it. They'll still be voting when it walks up these steps.
! place frozen-child at street-corner · walk soulbound to frozen-child · anim soulbound reach
NARRATOR: {sb} touches a child's shoulder. The child goes to sand and runs out through {sb_their} fingers.
! fade out 0.2 · place soulbound at quarters-bed · tint night · fade in 0.2 · anim soulbound wake-gasp · hold soulbound hand_front shard · light soulbound shard on
NARRATOR: {sb} wakes with the shard in one fist, gripped hard enough to cut.  ~ set a3-c1-01
```

### A3-C1-02 · Questeria, one year on

#### script a3-c1-02 · map `questeria` (Protectorate state) · plays on arrival, as you

The city map's Protectorate state: violet-and-gold tabards on patrol, curfew bells at dusk, a new granary on the Lantern Market's east side, a free clinic on the Thornbridge camp site, no beggars on the docks. Free to walk. The lines here are people you meet on the way to the keep (each a single-node script on its NPC, marker none).

```
CARD: Chapter 1 · The Lord Protector
```

#### script a3-c1-02-tomas · with `tomas`

```
TOMAS: Red Gulls are gone. Every one. Lord {sb} hanged the last three off the end of the harbour wall.
TOMAS: I didn't go and watch. Can't say I miss them, though.
```

#### script a3-c1-02-guard · with `protectorate-guard`

```
PROTECTORATE GUARD: Papers. Oh, it's you, Sir {name}. Sorry. Everyone gets asked now. It's the rule.
```

#### script a3-c1-02-pell · with `pell`

```
{if ide-rescued} PELL: Lord {sb} found Ide and me a room with a window that faces south. She sits in it all day.
{if !ide-rescued} PELL: Lord {sb} found me a room in the Low Ring. I keep a candle in the window for Ide. Lord {sb} says that's allowed.
PELL: Anything {sb} asks, we'd do it.
```

#### script a3-c1-02-sera · with `sera` · plays at the city gate

```
SERA: The city's never been this safe. You can hear the gulls from the market. I don't like it.
```

### A3-C1-03 · The Protectorate Act

#### script a3-c1-03 · map `sunspire-keep` · trigger at the throne · marker quest

```
! place leon on throne · anim leon cough · place elara beside throne · place soulbound before throne · hold soulbound hand_front protectorate-writ
LEON: Sir {sb}. You asked for the court. You have it.
SB: Majesty. My scouts have Hollowed moving in Doomward again. Morvaine's lords are fighting over Severin's chair. And there are lights in the northern sky nobody can name.
SB: I'm asking for the Protectorate Act. The Watch, the Guild and the treasury under one hand until the danger's past.
ELARA: And who decides when it's past?
SB (smile): Your father taught me he'd rather be told the truth rudely than lies politely. So, rudely: I do.   [CALLBACK rudely]
SB: I'm the one who keeps being right.
LEON [after coughing]: Sir {name}. You know {sb} better than anyone alive. What do you advise?
?
  1. Grant it. {sb} has earned it.  ~ set protectorate-act  → grant
  2. Grant it for thirty days. The Guild keeps the vault.  ~ bond -3 · set protectorate-limited  → limits
  3. Deny it. Nobody should hold all that. Not even {sb}.  ~ bond -8 · affinity-elara +10 · set mc-denied-act  → deny
#grant LEON: So be it.
ELARA (cold) [to you]: I hope you're right.  → after
#limits LEON: Thirty days. That's sensible.
SB (smile): Thirty days. Very generous.  → after
#deny NARRATOR: Nobody in the hall moves.
SB [quietly]: Not even me.
LEON: Then it's denied.  → after
#after ! anim soulbound bow · walk soulbound to hall-door
NARRATOR: {sb} bows to the king and walks out. The writ stays in {sb_their} hand.  ~ set a3-c1-03
```

All three branches lead to the same coup; what changes is who helps you after it. `protectorate-act` starts Chapter 3 with one fewer liberation unlocked; `mc-denied-act` gives Elara's speech at A3-C4-01 its alternative line.

### A3-C1-04 · The roof

#### script a3-c1-04 · map `sunspire-roof` · with `soulbound` · marker quest · needs `a3-c1-03`

```
! tint night · place soulbound at parapet · hold soulbound neck herring-charm
SB: On the boat I said I wanted somewhere nobody kneels in the mud.   [CALLBACK mud]
SB: I've been seeing something, {name}. Every night for a month. The whole city stopped mid-step, and a glass man on top of the Sunspire.
SB: And everybody's going to stand in that hall arguing about writs while it happens.
! anim soulbound show-palm · hold soulbound hand_front shard · light soulbound shard on
SB: Here. I need you to see it too.
?
  1. (Touch the shard.)  ~ set mc-saw-vision  → touch
  2. You kept it. All this time.  ~ bond -3  → kept
  3. Give it to Mira. Tonight. Please.  → mira
#touch ! backdrop starless-flash · wait 0.3 · backdrop off
NARRATOR: For a moment you see a black sky, a street of people standing still, and a glass figure at the top of the tower. It turns its head toward you.
YOU: What was that?
SB: That's why I don't sleep.  → promise
#kept SB: I kept it so I'd be ready. And I have been. Every time.  → promise
#mira ! anim soulbound close-fist · light soulbound shard off
SB: I can't. Not now. Not when I finally know what's coming.  → promise
#promise SB: When it comes, I'll need you next to me. Whatever I have to do. Say you won't leave.
?
  1. I'm here, aren't I?  ~ bond +5  → oath
  2. That depends what you're planning.  → depends
  3. [needs voice:witty] Leave? You still owe me for the boat.  ~ bond +3  → boat
#oath SB (sad) [quietly]: Never leave the other behind.   [CALLBACK oath]  ~ set a3-c1-04  → end
#depends SB: That's fair.
NARRATOR: Neither of you says anything else for a while.  ~ set a3-c1-04  → end
#boat SB (laugh): Gods, I've missed you. When did we stop doing this?  ~ set a3-c1-04  → end
```

### A3-C1-05 · Warnings

Five short talks, one per companion, each `<id>-a3-warn` with that companion, needs `a3-c1-04`, marker event. They can be had in any order before the alarm (A3-C2-01, which plays at the next rest). Each sets `<id>-a3-warn-done`.

#### script valencia-a3-warn · with `valencia`

```
VALENCIA: The thing on {sb}'s belt is singing. I can hear it from the chapel. It's the same note the Eye made before it woke.
VALENCIA: I promised {sb} in the Glasswood that I'd keep quiet about it. I've decided I'd rather break a promise than keep that one.  ~ set valencia-told
```

#### script mira-a3-warn · with `mira`

```
{if mira-index-shared} MIRA: I've had the Guild's assays next to the sun-priests' records for a month. The Box and the Eye are the same metal, cast in the same year.
{if mira-index-shared} MIRA: They were one thing once, {name}. And somebody wants them back together.  ~ set mira-suspects-crown  → end
MIRA: I don't have the Index any more, so I can't prove anything. But I watched my teacher get very calm, very quickly, the year before Doomward.
MIRA: {sb} has got very calm.
```

#### script brannock-a3-warn · with `brannock`

```
BRANNOCK: Served three kings and one warlord. You know how you tell them apart?
BRANNOCK: The warlord's always protecting you from something.
```

#### script sera-a3-warn · with `sera`

```
SERA: If it comes to it, I'll be beside you. Not {sb}. I'm sorry.
{if sera-charter} SERA: {sb} offered me the Mirewood by decree last week. No axe in it, ever. I said I already had a charter.  → end
{if sera-spiked} SERA: {sb} offered me the Mirewood by decree last week. No axe in it, ever. I said no. I nearly said yes.
```

#### script kael-a3-warn · with `kael`

```
{if affinity-kael-sb>affinity-kael} KAEL: You worry too much, boss. {sb}'s the only one in this city who finishes anything.  → end
KAEL: {sb} pays on time, boss. Nobody in this city's ever paid me on time.
```

---

## Chapter 2 · "The Deepvault"

### A3-C2-01 · The alarm

#### script a3-c2-01 · map `sunspire-keep` · plays on your next rest after `a3-c1-04` · your quarters

```
! tint night · sfx bells-alarm · place guild-apprentice at quarters-door · anim guild-apprentice stagger
CARD: Chapter 2 · The Deepvault
GUILD APPRENTICE: The inner seals. Somebody's in the Deepvault. The Archmage went down on her own.
GUILD APPRENTICE: She told me to fetch you and nobody else.  ~ set a3-c2-01 · objective "Get down to the Deepvault"
```

### A3-C2-02 · The Deepvault

The way down through `arcanum-spire` to `deepvault`: Guild wards flickering, Protectorate guards on the stairwells. They're the Watch. Each landing can be passed by a nonlethal fight (`a3-c2-02-stairs`, enemies yield at a quarter health) or a persuasion check.

#### script a3-c2-02-landing · with `protectorate-guard` · trigger on each landing

```
PROTECTORATE GUARD: Lord Protector's orders, Sir {name}. Nobody goes down.
?
  1. [PERSUADE 13 · adv voice:honorable] The Archmage is down there alone.  → pass let · fail stand
  2. Then I'm sorry about this.  → stand
#let PROTECTORATE GUARD: I didn't see you. Go.  → end
#stand PROTECTORATE GUARD: So am I, Sir.  ~ fight a3-c2-02-stairs
```

#### script a3-c2-02 · trigger at the vault floor

```
! place thessaly at seal-ring · place soulbound at relics · hold soulbound belt_front outer-key · show seals-broken-5
NARRATOR: Five of the twelve seals are broken.
! anim thessaly cast · sfx spell-fire · anim soulbound catch · light soulbound shard on · sfx spell-die
NARRATOR: Thessaly throws a spear of blue fire. {sb} catches it in the shard's light, and it goes out.
! anim soulbound raise-hand · sfx crystal-grow · swap thessaly thessaly-crystal
SB: I'm sorry, Archmage. You're safe in there. I'll need you when this is over.
! walk you to vault-floor · camera soulbound
SB [not turning round]: I knew it'd be you. It's always you.
?
  1. Step away from the relics, {sb}.  → step
  2. Whatever you saw, we face it together. Not like this.  ~ bond +2  → together
  3. (Draw your weapon.)  ~ bond -5  → draw
#step SB: I can't. I've seen what happens if I do.  → forge
#together SB: Together with what? A writ? Leon coughing on a throne?  → forge
#draw ! anim you draw
SB (sad) [turning]: You'd really do it.  → forge
#forge ! anim soulbound lift-both · hold soulbound hand_front black-box · hold soulbound hand_back eye-of-solvaris · light eye-of-solvaris gold-violet · hold soulbound float shard
NARRATOR: {sb} lifts the Box in one hand and the Eye in the other. The shard rises out of {sb_their} belt and hangs between them.
! sfx heartbeat-reverse · backdrop crown-forged · wait 1.5 · backdrop off · drop soulbound hand_front · drop soulbound hand_back · hold soulbound hand_front crown-dusk-dawn · light crown-dusk-dawn on
NARRATOR: The three of them run together like hot wax. What's left is a circlet of black iron and gold glass, with the shard set in the front of it.
VOICE(sb): Yes.  ~ set crown-forged · set a3-c2-02
```

### A3-C2-03 · The king

#### script a3-c2-03 · plays straight after

```
! place leon at vault-stair · place royal-guard beside leon · anim leon walk-slow · hold leon hand_front leon-sword
LEON: {sb}. Put it down.
SB: Majesty, you shouldn't be out of bed—
LEON: My family held half of that thing and called themselves saints while the square burned. Don't make our mistake. Put it down.
! walk leon to soulbound · anim leon grab-wrist soulbound · light crown-dusk-dawn violet-flare · sfx drain
NARRATOR: Leon takes hold of {sb}'s wrist. The black half of the crown flares by itself, and the light goes out of Leon in a stream.
! anim leon fall · drop leon hand_front · anim soulbound catch leon
SB (shock): No. No, I didn't, {name}, I didn't—
LEON [faintly, to {sb}]: I know you didn't.
LEON [to you]: Tell Elara I was proud of her. I never said it enough.   [CALLBACK proud]
! anim leon die · sfx none · wait 2
NARRATOR: {sb} stays on the floor with him.
VOICE(sb): He would have voted while it walked up the steps. Now nobody's left to wait for.
! anim soulbound stand · anim soulbound crown-self · hold soulbound head crown-dusk-dawn · light crown-dusk-dawn on
SB (cold) [quietly]: He was right. I won't make his mistake.
! walk soulbound to vault-stair · hide soulbound
NARRATOR: The king's sword is on the floor where he dropped it.
?
  1. (Take the sword.)  → took
  2. (Stay with the king.)  → stayed
#took ! anim you pick-up
NARRATOR: The grip's wrapped in old blue leather, worn pale where his hand went.  ~ give leon-sword  → guards
#stayed ! anim you kneel leon
NARRATOR: When the guards come for you, the royal guard puts the king's sword in your hands himself.  ~ give leon-sword  → guards
#guards ! hold you hand_back leon-sword · place protectorate-guard ×4 at vault-stair
PROTECTORATE GUARD: Sir {name}. The Lord Protector would like a word. At dawn.  ~ ambition =100 · set leon-dead · set a3-c2-03
```

Leon's sword is a live item from here to the end of the act: on your back socket until A3-C2-05, then on Elara's. It's the only royal sword in the game, and its art is unique (art spec §5).

### A3-C2-04 · The coup

#### script a3-c2-04 · map `sunspire-keep` · balcony, then the small council room · plays at dawn

```
! backdrop coup · music coup-anthem
NARRATOR: Dawn. The square under the balcony is full.
{if kael-with-sb} NARRATOR: Kael stands at {sb}'s right shoulder. He doesn't look for you in the crowd.
{if ashfall-loyal-sb} NARRATOR: Brenna's fighters hold the front rank, in black with violet sashes.
SB: Last night King Leon died. The princess has left the city. There are lights over Doomward the Guild can't name, and none of you can see them from here.
SB: I can. And I promise you this. Nobody in Fable kneels in the mud again.   [CALLBACK mud]
! sfx crowd-roar · anim crowd kneel · backdrop off
NARRATOR: The crowd shouts {sb}'s name. Then the Watch tells the square to kneel, and it does.
! place brannock in-crowd · anim brannock stand · place mira in-crowd · camera brannock
NARRATOR: One Stoneborn in the eleventh row stays standing. Two rows behind him, Mira hasn't moved at all.
```

```
! place you at council-table · place protectorate-guard ×2 at council-door · place soulbound at council-window · hold soulbound head crown-dusk-dawn · hold soulbound neck herring-charm
SB: Leave us.
! hide protectorate-guard
SB: Stand with me. You and me, the way it was always supposed to go. I'll make you anything. Marshal. Co-ruler. Pick one.
?
  1. As your friend: take the crown off.  ~ bond +3  → friend
  2. You killed the king.  → king
  3. No.  → no
  4. [needs bond>=60] (Kneel. Lie to {sb_them}.)  ~ bond +2  → kneel
#friend SB: I can't.  → cut
#king SB (shock): The Box did that. It was an accident. And it was necessary. Both of those.  → cut
#no NARRATOR: {sb} looks at you for a long time.
SB: All right.  → cut
#kneel ! anim you kneel
SB (sad) [smiling]: Get up. You've never once managed to lie to me.  → cut
#cut ! anim soulbound show-palm · sigil soulbound glow · anim soulbound touch-crown · sigil soulbound violet · sfx bond-break · sigil soulbound off · sigil you grey · anim you stagger
NARRATOR: {sb} presses the crown's light into the mark on {sb_their} palm. It burns violet and goes out. Yours turns grey, and you nearly fall.
SB: I said I'd never be small again. I can't carry you and keep them safe.
SB: Leave Questeria. Go a long way. When it's over, you'll understand. You'll see I was right.
{if bond>=70} SB (sad): Please. Just go. I can't stand here and watch you look at me like that.  → herring
{if bond>=40, bond<70} SB: Go. Before I think better of letting you.  → herring
SB (cold): Get out of my city.  → herring
#herring ! anim soulbound untie-cord · drop soulbound neck · show herring-on-table · walk soulbound to council-door · hide soulbound
NARRATOR: {sb} unties the herring from {sb_their} neck and leaves it on the table. Then {sb} goes.   [CALLBACK herring]
?
  1. (Take the herring.)  → taken
  2. (Leave it.)  → left
#taken ! anim you pick-up · hold you pouch sb-herring · hide herring-on-table
NARRATOR: The cord's still knotted the way {sb} always knots it, twice, like a net.  ~ give sb-herring · set soul-bond-broken · set a3-c2-04  → end
#left NARRATOR: You leave it where it is.  ~ set soul-bond-broken · set a3-c2-04  → end
```

At the start of this script the game sets `kael-with-sb` or `kael-with-you` by the meter comparison, before any line plays, so the balcony line can read it. `kael-with-sb` takes Kael out of the party.

`sb-herring` is optional. If you took it, it pays off at A3-C5-04. If you left it, it's found at the summit on the arm of the throne (A3-C5-01), and pays off the same way.

### A3-C2-05 · The Low Ring

A chase through `low-ring` and its sewers, Protectorate patrols behind you. Elara is a guest in the party (`join elara`, cannot be controlled, no turn in fights). It ends on a hillside outside the walls at sunrise.

#### script a3-c2-05-grate · map `low-ring` · trigger at the sewer grate

```
{if kael-with-you} ! place kael at sewer-grate · hold kael hand_front lamp · light kael lamp on
{if kael-with-you} KAEL: Took your time. In. The lamp's mine, so keep up.  → sera
NARRATOR: The grate's already been levered open.  → sera
#sera ! place sera at sewer-mouth · place elara beside sera
SERA: She was in the chapel. They were coming up both stairs.
ELARA: I can speak for myself, thank you. Later. Run now.  ~ join elara · fight a3-c2-05-chase
```

#### script a3-c2-05-chase-after · map `questeria-hills` · plays when the chase ends

```
! tint dawn · place elara at hilltop · anim elara look-back
ELARA: My father's dead, and my city's cheering for the one who killed him.
?
  1. It was the crown. {sb} didn't mean it.  → meant
  2. Then we take it back. All of it.  ~ affinity-elara +10  → back
  3. (Look at your palm.)  → palm
#meant ELARA: Does that make it better or worse? I honestly can't tell.  → sword
#back ELARA: Yes. We do.  → sword
#palm ! anim you look-palm · sigil you grey
ELARA: You lost someone last night too.  → sword
#sword YOU: He asked me to tell you he was proud of you.   [CALLBACK proud]
ELARA: He'd never have said that to my face. Hells. He'd have cleared his throat at me.
! anim you give-item elara · drop you hand_back · hold elara hand_back leon-sword
NARRATOR: You give her the sword. She puts it on her back without looking at it.  ~ take leon-sword
! place valencia at tree-shade · place brannock beside valencia · place mira at hilltop
VALENCIA: I heard it happen. I was in the bell-tower's shade, out of habit. I heard the shard become a crown. I knew what it was.
VALENCIA: I want you to hear that from me before anyone asks.
BRANNOCK: Left the clasps off my beard on the king's bier. I'm done owing crowns. I owe people.
MIRA: My teacher stood on a balcony once too. I'd like to talk about anything else.  ~ set a3-c2-05 · travel mirewood resistance-camp
```

---

## Chapter 3 · "Embers of Fable"

### A3-C3-01 · The resistance camp

#### script a3-c3-01 · map `mirewood` (camp state) · plays on arrival

The camp near the quarry: tents, fires, a stump for a council table, a few dozen loyalists. If `sera-charter`, it's under the silverbark.

```
CARD: Chapter 3 · Embers of Fable
! place elara at stump-table · hold elara hand_back leon-sword
ELARA: {sb} has the city, the Watch, the crowd, and a crown that can stop time. We have a forest and some very bad tempers.
BRANNOCK: I've had worse odds. Not often.
MIRA: Soldiers won't be enough. We need to know what that crown actually is.
ELARA: Then here's the war. Four places {sb} holds that might not want to be held.
! board on
ELARA: Take two of them and we can try for the city. Take all four and we might win.  ~ set a3-c3-01 · objective "Turn at least two of {sb}'s regions"
```

The board lists the four liberations. `protectorate-act` hides SQ3-Hale until one other is done (with the Act passed, the Watch is harder to reach).

| Quest | Region | Gains for the siege | Turns on |
| --- | --- | --- | --- |
| SQ3-Sunward | Morvaine | Valencia's night-riders; the truth about the Voice | `severin-alive` / `severin-dead`; required if `mira-index-burned` |
| SQ3-Ashfall | Doomward | Brenna's fighters free the Hollowed | `ashfall-loyal-sb` |
| SQ3-Gilded | The highlands | The Order of the Gilded Morning | `corwin-saved` / `corwin-dead` |
| SQ3-Hale | Questeria, at night | Hale and half the Watch; the east gate | persuasion |

The birthday parcel (`sb-birthday-parcel`, in `07-companions.md`) arrives at the camp the first time you return to it after any liberation.

### A3-C3-02 · SQ3-Sunward

#### script a3-c3-02 · map `sunward-cell` · with `severin` · needs `severin-alive` · marker quest

```
! place severin at cell-window · tint daylight · place valencia at cell-door
SEVERIN: The Champion. And my dawn. Have you come to gloat, or to ask me something?
VALENCIA: To ask. Father, when you held the Eye, did anything speak to you?
SEVERIN: Every night. In my own voice. It said the sun had to go out, and it called that justice.
{if mira-index-shared} MIRA: My teacher's journals say the same. A voice like his own, telling him to feed the world.  → hunger
{if !mira-index-shared} MIRA: My teacher heard a voice too. He told me once that it sounded like him on a good day.  → hunger
#hunger SEVERIN: Different words. The same appetite behind them.
SEVERIN: Whatever made the Box and the Eye isn't dead, Champion. It doesn't want them used. It wants them put back together.
MIRA (shock): Then the voice wrote the plan and {sb} signed it. The crown was its idea from the start.  → vial
#vial VALENCIA: Then I'll give you something for it.
! anim valencia cut-palm · anim valencia fill-vial · hold valencia hand_front valencia-vial · light valencia-vial gold
VALENCIA: The Eye still answers my blood. If any of it's left in that crown, this will reach it. Once.
! anim valencia give-item you · hold you belt_back valencia-vial
SEVERIN [to Valencia]: You gave that freely.
VALENCIA: I give everything freely now, Father. It's very inconvenient.  ~ give valencia-vial · set knows-voice-truth · set sq3-sunward
```

If `severin-dead`, the same truth comes from Isadora's last journal, in the conservatory:

#### script a3-c3-02-journal · map `glasswood` · trigger at the conservatory desk · needs `severin-dead` · marker quest

```
! echo isadora writing
ECHO-ISADORA: It speaks to him in his own voice, my dawn. That's how you know it isn't his.
ECHO-ISADORA: It doesn't want the Eye used. It wants the Eye and the Box made one again.
MIRA (shock): Then the voice wrote the plan and {sb} signed it. The crown was its idea from the start.  → vial
#vial VALENCIA: Then I'll give you something for it.
! anim valencia cut-palm · anim valencia fill-vial · hold valencia hand_front valencia-vial · light valencia-vial gold
VALENCIA: The Eye still answers my blood. If any of it's left in that crown, this will reach it. Once.
! anim valencia give-item you · hold you belt_back valencia-vial
VALENCIA: Mother would have liked you. She liked anyone who argued with her.  ~ give valencia-vial · set knows-voice-truth · set sq3-sunward
```

The journal script's `#vial` is the same scene without Severin. Valencia's last line differs, so both are written in full.

The quest also sets Valencia's night-riders for the siege (`ally-sunward`).

### A3-C3-03 · SQ3-Ashfall

#### script a3-c3-03 · map `ashfall` (garrison state) · with `brenna` · marker quest

```
{if !ashfall-loyal-sb} BRENNA: You're late. We've been throwing rocks at {sb}'s garrison for a month. We'll take some help with the bigger rocks.  ~ set ally-ashfall · set sq3-ashfall  → end
! place brenna at hollowed-line · show hollowed ×8 · anim hollowed stand-still
BRENNA: {sb} brought them back with the crown. They do what they're told. They don't hurt. It's better than ash.
?
  1. Look at their faces. Your father was one of them.  → father
  2. [PERSUADE 14 · adv knows-voice-truth] {sb} is doing what Maleficio did.  → pass turned · fail fight
  3. We're taking them back, Brenna.  → fight
#father ! camera hollowed-knight
BRENNA [looking at one of them]: He had that face. The last year. Nobody in it.
BRENNA: Fine. Sleep, all of you. You've served enough.  ~ set ally-ashfall · set sq3-ashfall  → end
#turned BRENNA: {sb} came for us when nobody else did. I'll hate you for this a while.  ~ set ally-ashfall · set sq3-ashfall  → end
#fight BRENNA: Then come and take them.  ~ fight a3-c3-03-brenna
```

#### script a3-c3-03-brenna-after

```
BRENNA [yielding]: Enough. You hit like Doomward. I'll come.  ~ set ally-ashfall · set sq3-ashfall
```

### A3-C3-04 · SQ3-Gilded

#### script a3-c3-04 · map `gilded-chapterhouse` · marker quest

```
{if corwin-saved} ! place corwin at chapter-altar · show gilded-knights
{if corwin-saved} CORWIN: Champion. I said I owed you a life. I'd like to pay it standing up.
{if corwin-saved} CORWIN: The Order's been wrong for three hundred years. Give us one morning to be right.  ~ set sq3-gilded  → end
! place davin at chapter-altar · show gilded-knights · hold davin hand_front corwin-sword
DAVIN: I was Sir Corwin's squire. He told you something at the end. To tell her he was sorry.
DAVIN: Did anyone tell her?
?
  1. [needs with:valencia] Ask her yourself. She's standing there.  → ask
  2. I told her.  → told
#ask ! camera valencia
VALENCIA: He did. I'm still deciding what I think of it. Bring your swords.  ~ set sq3-gilded  → end
#told DAVIN: Then the Order rides. He'd want to have been sorry somewhere useful.  ~ set sq3-gilded
```

### A3-C3-05 · SQ3-Hale

An infiltration of `questeria` at night: a stealth route over the Low Ring roofs (Kael's route if `kael-with-you`, and the patrol cones are shorter), to the Watch house. Getting caught starts a fight with a patrol that yields; the quest goes on either way.

#### script a3-c3-05 · map `watch-house` · with `hale` · marker quest

```
! place hale at desk · hold hale hand_front curfew-roster · tint night
HALE: Sir {name}. You know there's a price on you. I've got the paper here somewhere.
?
  1. [needs a3-c3-06] Thornbridge. You've seen it.  → thornbridge
  2. [PERSUADE 15 · adv voice:honorable, mc-denied-act] Leon trusted you. So did I.  → pass trusted · fail cells
  3. How many men would follow you out?  → count
#thornbridge HALE (tired): I walk my men past it twice a night. Twenty years I kept this city safe. Nobody told me it meant keeping it still.  → gate
#trusted HALE: Leon thanked my lads for every shift. Every one. {sb} sends a list.  → gate
#count HALE: Half. Maybe more. The other half have families inside the walls, and I don't blame them.  → gate
#cells HALE: I'm going to call for the guard now. Slowly. Take the window.  ~ fight a3-c3-05-escape  → end
#gate HALE: The east gate's mine on the night shift. When you come, it'll be open.  ~ set sq3-hale
```

`#cells` doesn't set `sq3-hale`. The quest can be retried once after any other liberation; Hale's first line changes to `HALE: Back again. I nearly hoped you would be.`

### A3-C3-06 · Thornbridge

#### script a3-c3-06 · plays at the camp when two liberations are done

```
! place elara at stump-table · place courier at camp-edge
ELARA: There were bread riots in Thornbridge yesterday. Rationing. {sb} went down there.
! travel-actor you thornbridge · tint violet-haze · show frozen-crowd · anim frozen-crowd frozen
NARRATOR: Thornbridge, the next morning. Three hundred people standing in the street mid-shout, in a violet haze. The snow over them isn't falling.
MIRA (shock): {name}? You've gone white. Have you seen this before?
SERA: I count forty-two in this street, and a dog by the bakery stopped mid-bark. Rot and rain.  ~ set a3-c3-06
{if with:kael} KAEL: I know that baker. He owes me money. He's going to owe me money for ever.
```

`travel-actor you thornbridge` takes the party to the frozen `thornbridge` map, walkable. Every frozen citizen can be examined; each has one line of description (writers: one per named NPC from Act 1's Thornbridge, ten in all, written in `08-random-encounters.md` §6 as "still lives"). Leaving the map returns to camp.

### A3-C3-07 · The dream

#### script a3-c3-07 · map `gulls-wager` (dream) · plays on your next rest after `a3-c3-06`

```
! tint night · place soulbound at bow facing right · anim soulbound lean-rail · hide crown-dusk-dawn
NARRATOR: The deck of the Gull's Wager, at night. {sb} is at the rail in the old coat, with no crown.
SB: Can't sleep either?   [CALLBACK sleep]
?
  1. You froze a whole district.  → froze
  2. Why are you here?  → why
  3. [needs knows-voice-truth] The voice isn't warning you. It's the thing coming.  ~ set sb-doubt-seed  → voice
#froze SB: They were killing each other over bread. Now nobody gets hurt. When it's passed, I'll let them go. I will.  → granaries
#why SB: Because I miss you. Is that allowed?  → granaries
#voice SB (laugh): That's what I'd say if I wanted somebody to drop their guard the night before it got here.
SB: I'd say it in exactly your voice, too.  → granaries
#granaries SB: I opened the granaries. Nobody in Fable's hungry this winter. Nobody's being bled by a lord, or a vampire, or a sorcerer.
{if sb-heron-petition} SB: The petition worked, by the way. I know it did. It took nine months. I counted every week of the winter it cost.
SB: Name one person you're protecting by fighting me.
?
  1. You.  ~ bond +5  → you
  2. The people standing in Thornbridge.  → people
  3. [needs sb-shared-catch] The cove fed itself that morning. Nobody ruled anybody.  ~ bond +3 · set sb-doubt-seed  → cove
  4. Whoever comes after you.  → after
#you NARRATOR: {sb} doesn't answer. The deck goes out from under you like a pulled rug.  ~ set a3-c3-07  → end
#people SB: I'll let them go when it's over. I told you.  ~ set a3-c3-07  → end
#cove SB (sad): Mam paid for that morning with her ring.
NARRATOR: The dream ends there.  ~ set a3-c3-07  → end
#after SB (cold): Whoever comes after me inherits full granaries. Ask them who filled them.  ~ set a3-c3-07  → end
```

The dream is a real projection through the crown. `sb-doubt-seed` gives advantage on the summit's choice (A3-C5-01). Either of the two options that set it will do.

---

## Chapter 4 · "The Siege of Questeria"

### A3-C4-01 · The muster

#### script a3-c4-01 · map `questeria-plain` · plays on arrival · needs `a3-c3-07`, `liberations-two-done` · a gate set by code: `liberations-two-done`, once two of `sq3-sunward`, `sq3-ashfall`, `sq3-gilded` and `sq3-hale` are set

The units on the field come from the liberation flags (night-riders, Ashfall fighters, the Order, nothing for Hale, who's inside).

```
CARD: Chapter 4 · The Siege of Questeria
! tint dawn · place elara at muster-cart · hold elara hand_back leon-sword · anim elara climb-cart
ELARA: My father said he'd rather be told the truth rudely than lies politely.   [CALLBACK rudely]
ELARA: So here it is. There are more of them. The one we're fighting was our friend. And we're going anyway.
{if mc-denied-act} ELARA: Sir {name} told my father no when it would have been easy to say yes. I'm asking you all to do the same thing for one day.
! sfx army-cheer · anim elara draw leon-sword
ELARA: Sound the advance.  ~ set a3-c4-01 · fight a3-c4-02-gates
```

### A3-C4-02 · The gates

A large battle: Protectorate soldiers, re-raised Hollowed, and frozen-time zones (violet circles on the field; anyone who walks in stops for three turns). Each liberation changes it, and each change has one bark:

| Flag | Change | Bark |
| --- | --- | --- |
| `sq3-hale` | The east gate opens; the breach section is skipped | `HALE: East gate! Mind the bar, it sticks!` |
| `ally-ashfall` | The Hollowed on the field fall still and lie down | `BRENNA: Sleep now. You've served enough.` |
| `sq3-gilded` | The Order holds the main square; no reinforcements from there | `CORWIN: The Gilded Morning stands!` or `DAVIN: For Sir Corwin!` |
| `sq3-sunward` | Night-riders on the rooftops thin the archers | `VALENCIA: Rooftops are ours. Keep your heads down.` |

### A3-C4-03 · The Deepvault again

#### script a3-c4-03 · map `deepvault` · trigger at Thessaly's crystal · with `mira`

The crystal is a Guild-glyph puzzle: turn six glyph rings so the lines join. Mira reads the glyphs aloud as hints, one per wrong turn.

```
! anim thessaly crystal-break · swap thessaly thessaly · anim thessaly fall-kneel
THESSALY: How long?
MIRA: Six weeks.
THESSALY: Then I've six weeks of complaints. Later. Listen to me.
THESSALY: That crown is a broken thing trying to be whole again. If it meets a force as strong as it is, it won't shatter. It'll tear.
THESSALY: And whatever's on the other side of the tear comes through it.
YOU: What's as strong as it is?
! camera you · sigil you grey
THESSALY [looking at your palm]: Something else that was made as two halves of one thing.  ~ set thessaly-freed · set a3-c4-03
```

### A3-C4-04 · Kael

#### script a3-c4-04 · map `questeria` · Lantern Market, burning · needs `kael-with-sb` · plays at the market's north end

```
! tint fire · place kael at market-north · hold kael hand_front kael-knives
KAEL: Hey. Long time. You look terrible.
{if kael-lane-given} KAEL: {sb} gave me a title, a house, and my name on a door. Nobody ever put my name on anything but a wanted poster.  → choose
KAEL: {sb} gave me a title and a house. I already had a door. Turns out I wanted the title more than I'd have guessed.  → choose
#choose KAEL: So. Are we doing this?
?
  1. [PERSUADE 13 · adv affinity-kael>=40] {sb} froze Thornbridge. There are children standing in it.  → pass step · fail duel
  2. ♥ [needs kael-romanced] I don't want to fight you.  → romance
  3. Then get out of the way.  → boss
#step KAEL: I know. I walk past them every day. Fine. Fine. I'm in. Don't make a thing of it.  ~ join kael · set kael-turned  → end
#romance ! anim kael lower-knives
KAEL: You never fight fair, gorgeous.  ~ join kael · set kael-turned  → end
#duel KAEL: Nice try. Nearly worked.  ~ fight a3-c4-04-kael-duel  → end
#boss KAEL: Yeah. Thought you'd say that.  ~ fight a3-c4-04-kael-shadow
```

#### script a3-c4-04-kael-duel-after

```
KAEL [yielding]: All right. All right. You win, and I'm coming, and my arm's going to be stupid about it for a week.  ~ join kael · set kael-turned
```

#### script a3-c4-04-kael-shadow-after

```
! anim kael downed
KAEL: Tell {sb} I'm sorry. I couldn't stop {sb_them} either.  ~ set kael-wounded
```

`a3-c4-04-kael-duel` ends at a fifth of Kael's health. `a3-c4-04-kael-shadow` is the full boss fight, Kael as the Protectorate Shadow; he survives it, wounded, and leaves the party for the rest of the act.

### A3-C4-05 · The stairs

#### script a3-c4-05 · map `sunspire-keep` · plays after the fight `a3-c4-05-stairs`, which starts at the foot of the grand stair (`11-combat.md` §4.1)

```
! show wardens ×12 · show crown-constructs ×3 · camera stair
BRANNOCK: Too many. Go. All of you. I'll hold the stairs.
! hold brannock arm_back brannock-shield · anim brannock set-shield
?
  1. Not without you.  → without
  2. You stopped counting. Don't make me count you.  → count
  3. (Grip his arm.)  ~ affinity-brannock +2  → arm
#without BRANNOCK: You're not losing me, little one. I'm staying behind. There's a difference.  → hold
#count BRANNOCK: Then don't count. Climb.  → hold
#arm BRANNOCK: Go on. Before I say something soft and we're both embarrassed.  → hold
#hold ! walk party to stair-top · anim brannock hold
NARRATOR: Behind you, the sound of the shield taking the first blow.  ~ set a3-c4-05
```

Brannock's fate is decided by `affinity-brannock` now, and shown at A3-C5-06: 60 or more sets `brannock-survived`, otherwise `brannock-fell`. `brannock-letters-delivered` adds 10 before the check (it's the difference between a man who wants to stop counting and one who wants to go home). Nothing tells the player the number.

On the climb, every floor flickers your palm sigil faintly gold for a moment (`sigil you flicker-gold`), with no line.

---

## Chapter 5 · "Soul-Bound"

### A3-C5-01 · The summit

#### script a3-c5-01 · map `sunspire-summit` · plays on arrival

The throne room with the roof torn off. A banner stopped half-folded in the air, a pigeon fixed mid-flight, rain hanging in beads. A wall of stopped time seals the door behind you; companions hit it and make no sound.

```
CARD: Chapter 5 · Soul-Bound
! place soulbound on throne · hold soulbound head crown-dusk-dawn · light crown-dusk-dawn on · show time-wall · anim party pound-wall
{if !sb-herring} ! show herring-on-throne-arm
SB: Of course you could walk through that. Half of my soul still has your name on it.
{if sb-herring} SB: You kept it. I wondered.  → stand
{if !sb-herring} SB [touching the herring on the throne's arm]: I brought this up with me. I don't know why.  → stand
#stand ! anim soulbound stand
SB: Go on, then. Tell me what I am. I'd like to hear it from you before Mira writes it down.
?
  1. A friend who got lost.  → lost
  2. A tyrant.  → tyrant
  3. [needs knows-voice-truth] The voice is the thing coming. You're opening the door.  → voice
#lost SB: I know exactly where I am. Same as the boat: I'm up front, so I'm the one who sees the rocks.  → tier
#tyrant SB: Say it the way Mira would. The way Leon's daughter wants you to.  → tier
#voice SB (shock): No.
VOICE(sb): They need you to doubt. Leon doubted, and Aldric died waiting.
{if sb-doubt-seed} SB: You said that on the boat. You've been saying it. Why do you keep saying it?  ~ set sb-doubt  → tier
SB: You're wrong. You've got to be wrong.  ~ set sb-doubt  → tier
#tier {if bond>=70} SB (sad): There's room up here. There was always room. Stand next to me and I'll take it off the minute it's safe.  ~ fight a3-c5-02-sovereign
{if bond>=40, bond<70} SB: Two kids off the landing. Mam always said one of us would drown the other.  ~ fight a3-c5-02-sovereign
SB (cold): You were always going to keep me small. Let's finish it.  ~ fight a3-c5-02-sovereign
```

### A3-C5-02 · {sb}, the Sovereign Protector

Three phases. Only you are in the fight; the companions are behind the time wall, and their portraits sit greyed on the party bar.

- **Phase 1, the Champion.** {sb} with {sb_their} own class kit from the Prologue, upgraded.
- **Phase 2, the Crown.** {sb} floats. The arena splits into time zones: freeze zones slow you, Box pulses drain health, Eye beams sweep the floor. Violet echoes of Varkhul, Maleficio and Severin appear for one attack each.
- **Phase 3, Soul-Bound.** Your grey sigil flares gold, and so does {sb}'s. The Soul-Link returns as Soul-Clash: whenever you both attack on the same beat, the light hits the crown and cracks it. The sky starts to split.

**Valencia's vial.** At the start of phase 2, if you have `valencia-vial`, a prompt appears over your belt. Using it plays:

#### script a3-c5-02-vial

```
! anim you uncork · drop you belt_back · hold you hand_front valencia-vial · light valencia-vial gold · anim you throw · sfx glass-break
NARRATOR: The vial breaks on the crown. The gold half of it flares, and the beams stop sweeping.
SB (shock): That's her. That's Valencia.  ~ take valencia-vial · set vial-used
```

`vial-used` turns off Eye beams for the rest of phase 2.

**Barks.**

| When | Line |
| --- | --- |
| Phase 1 starts | `SB: You still lead with your shoulder. I told you about that in Saltwind.` |
| You below half health, phase 1 | `SB: Get up. Please get up.` |
| Phase 2 starts | `SB: Every one of them failed because they waited. I won't.` |
| Varkhul's echo | `SB: He held a gate. Look what holding got him.` |
| Phase 3, your first Soul-Clash | `SB: Same as the light—` (cut off; {sb} stops mid-word) |
| Phase 3, {sb} at half | `SB: Why is it still lit? I cut it. I cut it.` then `YOU: You can't cut an oath. That was the point of it.` |

#### script a3-c5-02-doubt · plays when {sb} reaches a quarter of health in phase 3 · needs `sb-doubt`

```
! slow 0.3
SB: If you're right. If it's been lying the whole time, then Leon, and Thornbridge, and—
?
  1. Take the crown off. We'll fix it together.  → off
  2. (Lower your weapon.)  → lower
#off ! anim soulbound reach-crown · light crown-dusk-dawn violet-flare
VOICE(sb): No.
! anim soulbound scream · shake 2 · slow 1
NARRATOR: The crown tightens on {sb}'s head by itself.  ~ set crown-fights-both  → end
#lower ! anim you lower-weapon · anim soulbound lower-weapon · wait 1 · slow 1
NARRATOR: Neither of you moves. Then the crown lashes out by itself.  ~ set crown-fights-both  → end
```

`crown-fights-both` adds crown attacks that hit {sb} too, for the last quarter.

### A3-C5-03 · The tear

#### script a3-c5-02-sovereign-after

```
! anim you strike · anim soulbound strike · sigil both gold · sfx clocks-stop · wait 1
NARRATOR: You both strike at the same moment. Gold meets gold, with the crown between.
! anim crown-dusk-dawn split · sfx sky-tear · backdrop time-tear
NARRATOR: The crown splits down the middle. Above the Sunspire, the sky tears like paper.
! backdrop era-purging-dawn · wait 1.2 · backdrop era-ruins · wait 1.2 · backdrop era-gulls-wager · wait 1.2 · backdrop era-crowd-aging · wait 1.2 · backdrop era-leviathan · wait 1.5 · backdrop time-tear
VOICE(sb): Thank you. You held the door so well.
NARRATOR: The voice is still {sb}'s. Halfway through the sentence, it stops being.
! show vorlaith at tear · light vorlaith on · camera vorlaith
VORLAITH: Maleficio brought me the Box. Severin brought me the Eye. You two brought the rest.
VORLAITH: You were right, {sb}. Something was coming. You never thought it would come through you.
! drop soulbound head · hold soulbound hand_front crown-box-half · hold soulbound hand_back crown-eye-half
SB [quietly]: It was me. In the vision. It was me opening it.  ~ set a3-c5-03
```

The montage plates, in order (art spec §8): the Purging Dawn with a small boy watching (Severin); the Sunspire in ruins under a dead sun; the deck of the *Gull's Wager*, two young figures at the rail, one laughing; the crowd in the square ageing forty years and back; a winged leviathan crossing the tear.

### A3-C5-04 · Never leave the other behind

#### script a3-c5-04 · plays straight after

```
! anim tear pull · anim soulbound slide-to tear · anim you slide-to tear · sfx wind-up
NARRATOR: The tear pulls. Stones lift off the floor. The pigeon tumbles past you into the sky.
NARRATOR: You and {sb} are both sliding. {sb} is closer.
?
  1. (Reach for {sb}'s hand.)  → hand
  2. (Reach for the broken crown.)  → crown
  3. (Hold on to the throne.)  → throne
#hand {if bond>=70} NARRATOR: Your hands catch.  → high
{if bond>=40} NARRATOR: Your fingers touch {sb}'s.  → mid
NARRATOR: Your fingers close on nothing.  → defiant
#high ! anim you catch-hand soulbound · sigil both gold · hold you hand_front sb-herring
NARRATOR: The herring is between your palms, cord and all.   [CALLBACK herring]
SB (sad) [smiling]: There you are.
! camera tear · show vorlaith-hand at tear
SB: Never leave the other behind. So I'm going on ahead.   [CALLBACK oath]
! anim soulbound let-go · anim soulbound dive tear · hide soulbound · sigil you gold
NARRATOR: {sb} lets go, takes both halves of the crown, and jumps into the tear.  ~ set sb-sacrifice  → seal
#mid ! anim you touch-fingers soulbound
SB (sad): {name}, I'm sorry. I'm so—
! anim soulbound slip · hide soulbound · sigil you flicker-gold
NARRATOR: {sb}'s fingers slide out of yours. The tear takes {sb_them}, and the crown.  ~ set sb-lost  → seal
#throne ! anim you hold-throne
NARRATOR: You get an arm round the throne's leg and hold on.  → defiant
#defiant SB (angry): It came in through me. It can go out the same way.
! anim soulbound leap tear · anim soulbound strike vorlaith · hide soulbound · hide vorlaith · sigil you grey
NARRATOR: {sb} takes both halves of the crown and throws {sb_them}self at the thing in the tear. It closes over both of them.  ~ set sb-defiant  → pulse
#pulse ! wait 1 · sigil you flicker-gold 1 frame
NARRATOR: Your sigil is grey. Once, very faintly, it pulses gold.  → seal
#crown ! anim you grab-item soulbound · hold you hand_front crown-eye-half · light crown-eye-half gold · sigil you flicker-gold
NARRATOR: You get one half: the gold one. {sb} goes into the tear with the other.  ~ give crown-eye-half · set mc-has-eye-half  → seal
#seal ! sfx laughter-far · anim tear shrink · show time-scar
NARRATOR: Somewhere behind the sky, something laughs. The tear shrinks to a jagged violet scar. It doesn't close.  ~ set time-scar · set a3-c5-04
```

`#hand` is a router: three nodes in a row, each with a `Next`, and the first whose conditions pass plays (build plan §4, the jump rule). The build plan's worked example (§4.3) is this scene.

`{sb_them}self` needs nothing new: "them", "him" and "her" plus "self" are all correct words.

### A3-C5-05 · A broken clock

#### script a3-c5-05 · plays straight after

```
! hide time-wall · anim party run-in · tint flicker-day-night
NARRATOR: The time wall falls. Below, Questeria is in pieces, and the pieces are in different years.
NARRATOR: The east wall looks built yesterday. The west wall's a thousand-year-old ruin. The sun goes up and down twice in an hour.
THESSALY: Time isn't running any more. It's bleeding. That scar's a wound, and something on the other side is leaning on it.
VALENCIA: I can hear it. A heartbeat, running backwards.
! camera you · anim you look-palm
MIRA [to you]: {sb}?
?
  1. {sb}'s still out there. I can feel it.  ~ set mc-vow-find-sb  → feel
  2. {sb} chose this.  → chose
  3. (Close your hand.)  → close
#feel MIRA: Then we find {sb_them}.  ~ set a3-c5-05  → end
#chose SERA: Something chose it for {sb}. A long time before Greywatch.  ~ set a3-c5-05  → end
#close NARRATOR: Nobody asks again.  ~ set a3-c5-05  → end
```

The Thornbridge district is unfrozen as part of this: walking it afterwards, some people are unchanged, some forty years older, and one child is a baby again (the "still lives" from A3-C3-06, each with an after line in `08-random-encounters.md` §6).

### A3-C5-06 · Coronation in the ruins

#### script a3-c5-06 · map `sunspire-summit` (ruined state) · plays a week later

```
! fade in 1 · show time-scar · place elara at throne-ruin · hold elara hand_front leon-sword · place you before throne-ruin
NARRATOR: A week later, in the broken throne room, Elara is crowned under the scar.
{if brannock-survived} ! place brannock in-crowd · anim brannock arm-sling · hold brannock arm_back brannock-shield
{if brannock-survived} BRANNOCK [when you pass him]: Told you. Staying behind.
{if brannock-fell} ! show brannock-shield-on-wall · camera brannock-shield-on-wall
{if brannock-fell} NARRATOR: Brannock's shield hangs behind the throne. Forty-one names are scratched into it, and a forty-second.
{if kael-turned} NARRATOR: Kael is at the back, by the door, which is where he always stands.
{if kael-wounded} ! show note-on-pillar
{if kael-wounded} NARRATOR: A note is pinned to a pillar at the back, in Kael's hand: "Still breathing. Don't come looking."
ELARA: My father ruled in peace. I'm told I'll rule in something nobody has a name for yet.
ELARA: Sir {name}, Champion of Fable. Whatever comes through that sky, I want you standing in front of it.
?
  1. I'll be there.  → there
  2. I'm going after {sb} first.  [needs mc-vow-find-sb]  → after
  3. (Kneel.)  → kneel
#there ELARA: Thank you. I mean it. Get up off the floor before you start.  ~ set a3-c5-06  → end
#after ELARA: I'd expect nothing else. Bring {sb_them} back to answer for it, and I'll decide what that means when I see {sb_them}.  ~ set a3-c5-06  → end
#kneel ! anim you kneel
ELARA: Get up. My father hated people kneeling, and I've decided I do too.  ~ set a3-c5-06  → end
```

`kael-turned` covers both `kael-with-you` from A3-C2-04 and a Kael won back at A3-C4-04; set it alongside `kael-with-you` at A3-C2-04, so this reads one flag.

### A3-C5-07 · The Unmade King

#### script a3-c5-07 · map `stopped-clocks` · plays straight after

```
! fade out 1 · backdrop vorlaith-hall · fade in 1 · music vorlaith
NARRATOR: Somewhere outside time, a hall of stopped clocks, each stopped at a different hour.
NARRATOR: On a throne of cracked glass sits a figure in a crown of clock-hands. Around him stand soldiers in the armour of every age, none with a face.
VORLAITH: The door's open a hand's width.
! backdrop pier-memory · camera pier
NARRATOR: In the air in front of him, a pier in a storm. A boat turned over. A child in the water. On the pier, another child about to jump.
! anim vorlaith reach · anim pier-child hesitate
VORLAITH: Somebody ought to be ready.
NARRATOR: On the pier, the second child stops.
! fade out 2
CARD: End of Act 3 · Soul-Bound  ~ set act4-vorlaith · set act3-done
```

Vorlaith has spoken four times in the act: twice at the tear, twice here. Don't add a fifth.

---

## Act 3 flags

| Flag | Set | Read |
| --- | --- | --- |
| `protectorate-act` / `protectorate-limited` / `mc-denied-act` | A3-C1-03 | A3-C3-01, A3-C3-05, A3-C4-01 |
| `mc-saw-vision` | A3-C1-04 | Act 4 |
| `valencia-told`, `mira-suspects-crown` | A3-C1-05 | Banters |
| `crown-forged`, `leon-dead` | A3-C2-02/03 | Everything after |
| `kael-with-sb` / `kael-with-you` / `kael-turned` | A3-C2-04, A3-C4-04 | A3-C2-04, -05, A3-C4-04, A3-C5-06 |
| `soul-bond-broken` | A3-C2-04 | Combat (Soul-Link off) |
| `sb-herring` (item) | A3-C2-04 | A3-C5-01, A3-C5-04 |
| `ally-sunward`, `sq3-sunward`, `knows-voice-truth` | A3-C3-02 | A3-C3-03 (advantage), A3-C3-07, A3-C5-01 |
| `ally-ashfall`, `sq3-ashfall` | A3-C3-03 | A3-C4-02 |
| `sq3-gilded` | A3-C3-04 | A3-C4-02 |
| `sq3-hale` | A3-C3-05 | A3-C4-02 |
| `sb-doubt-seed`, `sb-doubt`, `crown-fights-both` | A3-C3-07, A3-C5-01, A3-C5-02 | A3-C5-01, the boss fight |
| `thessaly-freed` | A3-C4-03 | A3-C5-05 |
| `kael-wounded` | A3-C4-04 | A3-C5-06 |
| `brannock-survived` / `brannock-fell` | A3-C4-05 | A3-C5-06, Act 4 |
| `vial-used` | A3-C5-02 | The boss fight |
| `sb-sacrifice` / `sb-lost` / `sb-defiant` / `mc-has-eye-half` | A3-C5-04 | Act 4 |
| `time-scar`, `mc-vow-find-sb`, `act4-vorlaith`, `act3-done` | A3-C5-04 to -07 | Act 4 |

Read from other files: `ide-rescued` (`08`, SQ-Pell), `sera-charter` / `sera-spiked`, `mira-index-shared` / `mira-index-burned`, `kael-lane-kept` / `kael-lane-given`, `kael-romanced`, `brannock-letters-delivered`, `sb-heron-petition`, `sb-shared-catch` (all `07` or built), `severin-alive` / `severin-dead`, `corwin-saved` / `corwin-dead`, `ashfall-loyal-sb` (Acts 1 and 2).
