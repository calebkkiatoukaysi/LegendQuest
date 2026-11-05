# Script: Act 2 · "The Long Eclipse"

All new. Notation as in `04-script-prologue-act1.md`.

**Fights.** Where a scene describes a fight's layout (tiles, circles on the field, walking into a zone), `11-combat.md` §5 is what to build: fights are side-view rows in the room now, and that section restates every special fight. Who helps in each fight is `11-combat.md` §4.1.

Lord Severin of Morvaine means to put out the sun with the Eye of Solvaris, and only his daughter Valencia can wake it. You find her, find the Eye, and turn its light on her father. Underneath that, {sb} learns that Fable's crown was built on a massacre, is handed the weapon that did it, and decides the trouble was never the weapon, only who was holding it.

**Mechanics this act adds** (build plan §3):

- `eclipse-stage` goes 1 to 5. Each stage darkens the world tint and gives night enemies their night strength in the day. It drops to 0 at A2-C6-05.
- Valencia joins as a Hemomancer at rank 4 (`10-classes.md` §3.2). Its passive, The Old Enemy, is her weakness in daylight and strength at night; from `eclipse-stage` 3 on, the failing sun stops weakening her.
- `affinity-valencia` starts at 10.

---

## Chapter 1 · "The Pale Morning"

### A2-C1-01 · The Festival of Dawn

#### script a2-c1-01 · map `questeria` · plays on arrival · needs `act1-done`

Six months on. Lanterns over the plaza, sun banners, music. You and {sb} are in your Champions' outfits (art spec §5).

```
CARD: Act 2 · The Long Eclipse  ~ act =2
CHAPTER: Chapter 1 · The Pale Morning
KAEL: Champions of Fable. Do they pay extra for the cape, or did you buy that?
SB: I bought it. Look at it. Of course I bought it.
! sfx crowd-toast · tint eclipse-1 · backdrop eclipse
NARRATOR: At noon the whole plaza lifts its cups to the sun, and the light goes thin, like a lamp turned down.
NARRATOR: A grey ring slides across the sun. The birds stop.
! backdrop off · place thessaly at plaza-steps
THESSALY: Nobody panic. That isn't an eclipse. Nothing in the sky has moved.
THESSALY: Something is pulling the light out of it.
! tint off
NARRATOR: A minute later the sun comes back. Nobody can agree how long it was gone.  ~ eclipse-stage =1 · set a2-c1-01
```

### A2-C1-02 · The royal crypt

#### script a2-c1-02 · map `sunspire-keep` · plays that night · needs `a2-c1-01`

```
! place royal-guard at hall-door · anim royal-guard stagger
ROYAL GUARD (afraid): The crypt. Under the chapel. They've taken the sun-scrolls.  ~ objective "Go down into the Aurelian Crypt"
```

#### script a2-c1-02-vesk · map `aurelian-crypt` · trigger at the lectern

```
! place vesk at lectern · hold vesk hand_front sun-scroll
VESK: Champions. My lord sends his compliments, and his thanks. You killed Maleficio for us. It saved a great deal of trouble.
VESK: If you would be so good as to stand aside.  ~ fight a2-c1-02-vesk
```

#### script a2-c1-02-vesk-after

```
NARRATOR: Vesk comes apart into mist. Half a scroll drops out of it.  ~ give sun-scroll
MIRA [reading]: A temple record. "The Eye of Solvaris sleeps beneath Sunfall, and wakes only for the blood of the Dawnborn."
MIRA: I don't know that word. I know nearly all the words.  ~ set a2-c1-02
```

### A2-C1-03 · The king's study

#### script a2-c1-03 · map `sunspire-keep` · with `leon` · marker quest

```
! place leon at study-desk · anim leon pour-wine
LEON: Sit down, all of you. You've earned the truth, and I've put it off long enough.
LEON: Three hundred years ago my family were priests of Solvaris. They made the Eye to turn sunlight into fire, and they used it on every vampire in Fable.
LEON: The peaceful ones. The children. Thousands of them, in the square under this window. They called it the Purging Dawn. My family were crowned the next spring.
LEON: The ones who lived went north and became Morvaine. One of them was a boy called Severin. He watched his family burn. I don't imagine he's forgotten.
! camera soulbound
SB: The Festival of Dawn. They light the lanterns in that same square, don't they?
LEON (tired): Every year.
?
  1. Thank you for telling us. It doesn't excuse Severin.  ~ set mc-side-leon  → excuse
  2. Your family started this.  ~ bond +2  → started
  3. (Look at {sb}.)  ~ ambition +3  → look
#excuse LEON: No. But it explains him, and I'd rather understand a man before I fight him.  → hall
#started LEON: Yes. I've known since I was eleven. I've never said it aloud in this room before.  → hall
#look SB [to Leon]: You still go out on the balcony for it, don't you? You still wave.
LEON: I do. I don't know what else to do.  → hall
#hall ! fade out 0.4 · place soulbound at hall-window · fade in 0.4
SB [to you]: A king who says thank you. Remember the boat? I thought that meant something.  ~ ambition +5 · set sb-knows-purge · set a2-c1-03
```

### A2-C1-04 · The Gilded Morning

#### script a2-c1-04 · map `sunspire-keep` · courtyard · plays the next morning

```
! show gilded-knights · place corwin at courtyard-gate
CORWIN: The Order of the Gilded Morning answers the crown's call. "Where the dark gathers, there we stand." Point us north.
THESSALY: Your Order was disbanded a hundred years ago.
CORWIN: Disbanded is a word on a page. We kept the swords sharp.
CORWIN [to the party]: Our records say the Dawnborn is a woman. Severin's daughter. She was seen crossing the Ashen Strait forty years ago. Find her, and we'll see to it the Eye never wakes.
YOU: See to it how?
CORWIN: The usual way.
MIRA [to you, after]: There's one house on the strait islands that takes in anyone who knocks. Saint Lumen's.
MIRA: If I'd wanted to hide for forty years, I'd have gone somewhere nobody asks questions.  ~ set a2-c1-04 · objective "Sail to Saint Lumen's Abbey"
```

---

## Chapter 2 · "Sister Vale"

### A2-C2-01 · Saint Lumen's Abbey

#### script a2-c2-01 · map `saint-lumen` · with `anselle` · plays on arrival

```
CHAPTER: Chapter 2 · Sister Vale
ANSELLE: Champions, here? We've nothing worth taking but honey and prayers, and the bees guard the honey.  ~ set a2-c2-01
```

The abbey is free to walk in daylight. Three things can be examined (`sign` objects with no sign drawn), each setting `vale-clue-<n>`:

| Where | Text |
| --- | --- |
| The shady side of the cloister | `Sister Vale has worked the shady side all morning. She hasn't stepped into the sun once.` |
| The well | `A cup someone set down on the well's edge. It's cold where her fingers were.` |
| The abbey roll, in the chapel | `"Sister Vale, professed in the year of the long frost." Forty years ago. She looks twenty-five.` |

#### script a2-c2-01-orla · with `orla`

```
! place orla at hives · hold orla hand_front smoke-pot
ORLA: Mind the hives. They don't know you, and they're not friendly to people they don't know. Same as most of us.
ORLA: Vale? She'll be in the shade. She always is.
```

#### script a2-c2-01-vale · with `valencia` · marker quest

```
! place valencia at cloister-shade · swap valencia sister-vale
SISTER VALE (smirk): You're staring. Most visitors stare at the glass.
?
  1. ♥ The glass isn't as interesting.  ~ affinity-valencia +2 · set valencia-flirt-1  → glass
  2. [needs vale-clue-3] Forty years here. You don't look it.  ~ set mc-suspects-vale  → years
  3. Sorry. Long road.  → road
#glass SISTER VALE: Careful. This is a holy place, and I'm a holy woman. Mostly.  → bell
#years SISTER VALE: Clean living. And no mirrors.  → bell
#road SISTER VALE: They're all long. Sit in the shade a while. Everyone does in the end.  → bell
#bell ! sfx bell-vespers
SISTER VALE: Vespers. You'll want to be on the boat before dark. The strait isn't kind at night.  ~ set a2-c2-01-vale
```

### A2-C2-02 · The night raid

#### script a2-c2-02 · plays at dusk, after `a2-c2-01-vale` · map `saint-lumen`

```
! tint night · sfx bells-alarm · place vreya at cliff-top
VREYA: Little princess! Come out, come out! Papa misses you!  ~ fight a2-c2-02-raid
```

The raid's second wave plays this mid-fight scene (a fight's `Interlude`, build plan §3):

#### script a2-c2-02-vale-reveals

```
! anim valencia hood-back · swap valencia valencia-unhooded · anim valencia drain
NARRATOR: Sister Vale pushes her hood back. Her eyes are red. She closes her hand, and three of the thralls fall over.
! show gilded-knights · place corwin at abbey-gate
CORWIN: The Dawnborn! There! Take her!
?
  1. Protect her.  ~ affinity-valencia +10 · set corwin-hostile  → end
  2. Hold the thralls. Corwin can deal with her.  ~ affinity-valencia -10 · set valencia-fled  → end
  3. (Let {sb} decide.)  → sb
#sb SB: She's the key to the Eye. Nobody touches her.  ~ affinity-valencia +5 · affinity-valencia-sb +10 · set corwin-hostile  → end
```

Picking 1 or 3 makes the fight's second wave Sir Corwin and two Gilded knights instead of more thralls, fought nonlethally (they yield at a quarter health; `11-combat.md` §5.2). Picking 2 makes Valencia leave the fight; she's found again in SQ-Valencia's-Trail (`08-random-encounters.md`, a one-off sea-cave event).

#### script a2-c2-02-raid-after

```
! anim vreya downed
VREYA (laugh): You think you're saving her? You're only the next ones who want her blood.  ~ set a2-c2-02
```

### A2-C2-03 · Valencia

#### script a2-c2-03 · map `abbey-chapel` · plays when you enter · needs `a2-c2-02`, `!valencia-fled` (or `valencia-found` after the trail)

```
! tint moonlight · place valencia at altar-steps · anim valencia sit
VALENCIA: Valencia Morvaine. Daughter of the Blood Sovereign. Forty years of hiding, undone by one evening of good manners.
VALENCIA: My father needs me to wake the Eye. It has to be my blood, and I have to give it of my own will. He's spent forty years looking for a way round the second part.
MIRA: Why run from him?
VALENCIA: Because he loves me. And because if he puts out the sun, every village in the world becomes one of his.
VALENCIA: A cup a month, and a smile at the door when they come to collect it.
! place corwin at chapel-door · anim brannock bar-door
CORWIN: A pretty speech. Her blood wakes a weapon that ends the world. Spill it tonight and there's nothing left to wake.
?
  1. She's under our protection. Stand down, Corwin.  ~ affinity-valencia +5  → protect
  2. [needs sb-knows-purge] Kill her, and you're the Purging Dawn in better armour.  ~ affinity-valencia +5 · ambition +2  → purge
  3. [needs voice:pragmatic] She's more use to us alive. The Eye could work for us.  ~ affinity-valencia -5 · ambition +3  → use
#protect CORWIN: When she turns on you, remember I offered.  → come
#purge CORWIN: That was holy work.
SB: It was murder with a choir.  → come
#use VALENCIA (cold): Ah. There it is.
SB: {name}'s not wrong, though.  → come
#come ! anim corwin leave
VALENCIA: Fine. I'll come. Not because I trust any of you.
VALENCIA: The Eye can shine as well as burn, if the right blood asks it to. And my father should be stopped by someone who isn't trying to kill me.
VALENCIA: One thing before we go. Sister Orla, who keeps the bees. If the Order comes back, they'll take her first. Someone should be watching her.  ~ set valencia-quest-hook
! anim soulbound offer-hand
SB: Welcome aboard. We're all a bit haunted here. You'll fit right in.
! anim valencia shake-hands soulbound · anim valencia breathe-in
VALENCIA [looking at {sb}'s hand, then {sb}'s face]: Some of you more than others.  ~ set valencia-sensed-shard · join valencia · set valencia-recruited · set a2-c2-03
```

---

## Chapter 3 · "The Glasswood"

### A2-C3-01 · Into Morvaine

#### script a2-c3-01 · map `morvaine-pass` · plays on arrival

```
! tint eclipse-2
CHAPTER: Chapter 3 · The Glasswood
NARRATOR: Noon in Morvaine. The sun is a pale coin you can look straight at.  ~ eclipse-stage =2
VALENCIA: Pine resin and old iron. I'd hoped never to smell this again.
VALENCIA: My mother hid what we need in her glasshouse in the Glasswood. Only her blood opens the door, or mine.  ~ set a2-c3-01 · objective "Find Lady Isadora's glasshouse in the Glasswood"
```

### A2-C3-02 · Hollowmere

#### script a2-c3-02-maren · map `hollowmere` · with `maren-elder`

```
MAREN: The Tithe? A cup a month. For that, no wolves, no bandits, and nobody goes hungry in winter.
MAREN: The Blood Lords keep their word. That's more than your kings ever did for us.
```

#### script a2-c3-02-pell · with `pell` · marker quest

```
PELL [low]: You're from the south? Take me with you. Please.
PELL: My sister's cup was due last month. She went up to the blood-house and she hasn't come home.  ~ set sq-pell-hook
```

#### script a2-c3-02 · trigger at the village's far gate

```
SB: I expected monsters. They're just rulers. Ones who actually deliver.
?
  1. They farm people, {sb}. Politely. It's still farming.  ~ bond +2  → farm
  2. Maybe there's something to learn from it.  ~ ambition +3  → learn
  3. [needs sq-pell-hook] Pell's sister went up to the blood-house and didn't come back.  → pell
#farm SB: Isn't every kingdom a farm, if you stand far enough back?  ~ set a2-c3-02  → end
#learn SB: That's all I'm saying. Learn from it.  ~ set a2-c3-02  → end
#pell SB: Right. Of course. Sorry. I was thinking out loud.  ~ set sq-pell-started · set a2-c3-02  → end
```

SQ-Pell: the sister, Ide, is alive in the blood-house cellar, kept as a "reserve" past her month. Getting her out is a stealth or persuasion job with Valencia in the party (she knows the house rules). Its scenes are in `08-random-encounters.md` §5, with the other side quests.

### A2-C3-03 · The Moonlit Conservatory

#### script a2-c3-03-door · map `glasswood` · trigger at the conservatory door

```
! anim valencia cut-palm · sfx door-glass
NARRATOR: Valencia presses her cut palm to the black glass, and the door lets go of its frame.
VALENCIA: She grew night-flowers. Nothing in here has had a gardener for forty years, and they're still flowering.
```

The prisms are a puzzle. Turning each one lights a memory spot (`echo` actors, art spec §7).

#### script a2-c3-03-echo-1 · memory spot 1

```
! echo isadora · echo valencia-child
ECHO-ISADORA: Your father lost everyone in the Purging Dawn, my little dawn. He's still standing in that square.
ECHO-ISADORA: Don't you go and stand there with him.
```

#### script a2-c3-03-echo-2 · memory spot 2

```
! echo isadora · echo severin
ECHO-ISADORA: You want to put out the sun so nobody can burn us again. Severin, then we're the fire. Every village a Hollowmere, for ever.
ECHO-SEVERIN: Better we rule them than they burn us.
```

#### script a2-c3-03-echo-3 · memory spot 3 · plays when all prisms are lit

```
! echo isadora writing · anim isadora hurt
ECHO-ISADORA: The Eye sleeps under Sunfall, in the Sunscar. It answers the will of whoever's blood wakes it.
ECHO-ISADORA: Given in hate, it eats the light. Given in love, it shines.
ECHO-ISADORA: Valencia. Whatever you choose, choose it yourself.
! hide isadora · show isadora-journal at desk
NARRATOR: Valencia doesn't move. On the desk where the echo sat, there's a journal.  ~ give isadora-journal
?
  1. (Stand beside her. Say nothing.)  ~ affinity-valencia +5  → quiet
  2. She loved you.  ~ affinity-valencia +3  → loved
  3. ♥ [needs valencia-flirt-1] (Take her hand.)  ~ affinity-valencia +5 · set valencia-romance-2  → hand
#quiet VALENCIA: Thank you for not saying anything.  → ambush
#loved VALENCIA: She died of it.  → ambush
#hand ! anim you take-hand valencia
VALENCIA: Your hand's so warm. I'd forgotten hands were.  → ambush
#ambush ! place mordain at conservatory-door
MORDAIN [whispering]: Lord Severin knows where you're going. He has a friend in your court. Such a good friend.  ~ set crane-double · fight a2-c3-03-mordain
```

### A2-C3-04 · The camp in the Glasswood

#### script a2-c3-04 · camp · plays when you rest after `a2-c3-03-mordain` is won

You wake in the night. Control is yours; the scene plays when you walk within six tiles of the two figures at the camp's edge (a trigger that only exists in this scene). Walking away instead skips it.

```
! tint night · place valencia at camp-edge · place soulbound at camp-edge · light soulbound pouch on
VALENCIA [low]: You smell like an open grave. I noticed at the abbey. What are you carrying?
SB: A souvenir from Doomward. A splinter. It's harmless.
VALENCIA: Nothing that smells like that is harmless. Does {name} know?
SB (smile): You hid what you were for forty years. You lied to every sister in that abbey to keep them safe. You understand better than anyone.
SB: Some secrets protect people.
VALENCIA: Some do.  ~ set mc-heard-sb-valencia
```

#### script a2-c3-04-morning · with `soulbound` · needs `mc-heard-sb-valencia` · marker event

```
SB: Morning. You look like you slept on a rock.
?
  1. I heard you last night. What splinter, {sb}?  ~ bond -3  → splinter
  2. (Say nothing. Watch {sb}.)  ~ set mc-watching-sb  → end
#splinter SB: Mira's got the shard. I kept a chip of it, the size of a fingernail, wrapped in oilcloth in my left boot, so I'd know the feel of it if Severin's got anything like it.
SB: I should have told you. I'm sorry.  ~ set a2-c3-04  → end
```

{sb} lies by adding detail. The fingernail, the oilcloth and the boot are the tell (writing guide, voice sheet). Don't trim them.

---

## Chapter 4 · "Sunfall"

### A2-C4-01 · The Sunscar Waste

#### script a2-c4-01 · map `sunscar-waste` · plays on arrival

```
! tint ember-sun
CHAPTER: Chapter 4 · Sunfall
VALENCIA: I'm standing in the noon light. I have never been able to stand in the noon light.
VALENCIA: He's very nearly finished.  ~ eclipse-stage =3 · set a2-c4-01
```

### A2-C4-02 · Sunfall Temple

#### script a2-c4-02 · map `sunfall-temple` · trigger at the great mural

```
! backdrop purging-dawn
SB: Look at the priests' faces. Calm as men mending nets. They thought they were saving the world.
MIRA: So did my teacher.
SB: Yes. He did.  ~ set a2-c4-02
```

### A2-C4-03 · The waking

#### script a2-c4-03 · trigger at the sanctum altar

```
! show eye-pedestal · hold eye-pedestal top eye-of-solvaris
VALENCIA: Given in hate, it eats the light. Given in love, it shines.
VALENCIA: What if I can't tell which one I'm feeling?
?
  1. You don't have to do it now.  ~ affinity-valencia +3  → now
  2. Think about your mother. Not him.  ~ affinity-valencia +5 · set eye-woken-love  → mother
  3. ♥ [needs valencia-romance-2] Think about something you'd want to keep.  ~ affinity-valencia +5 · set eye-woken-love  → keep
#now VALENCIA: I do. He's darkening the sky while we stand here.  → wake
#mother VALENCIA: Mother. Yes.  → wake
#keep VALENCIA [looking at you]: That isn't difficult.  → wake
#wake ! anim valencia cut-palm · sfx eye-wake · light eye-of-solvaris gold · backdrop eye-wakes
NARRATOR: Blood runs down the channel, and the Eye opens. The room fills with a warm gold light. Somewhere, impossibly, there are birds.
! backdrop off · anim soulbound lift-item · hold soulbound hand_front eye-of-solvaris · light eye-of-solvaris gold-violet · light soulbound pouch on
NARRATOR: {sb} lifts it off the pedestal. In {sb}'s hands the gold has a violet edge.
VOICE(sb): The Box takes and this gives. You've touched both now.
MIRA: It's the same kind of thing as the Box. Old, and made by people who wanted to fix everything at once.
MIRA: Put it in the case, {sb}. Now, please.
SB: In a minute.  ~ set a2-c4-03
```

### A2-C4-04 · The Blood Sovereign

#### script a2-c4-04 · plays straight after

```
! sfx applause · place severin at sanctum-door · place crane beside severin · hold severin head crown-of-thorns
SEVERIN: Thank you, Champions. Truly. Forty years I've looked for her, and you found her in a season and walked her up to the altar.
VALENCIA: Father.
SEVERIN: My dawn. You look like your mother.
SB (cold) [to Crane]: You.
CRANE: Sir {sb}, you were a generous buyer and I'll say so to anyone. Lord Severin was more generous. You of all people understand a market.
SEVERIN: Do you know what your Eye did, Champions? I was nine. My mother is in that square still, and everyone round her is singing hymns.
SEVERIN: I want shade, Champions. A world where my people never kneel to the sky again.
?
  1. And your Tithe villages kneel to you instead.  → kneel
  2. What happened to your people was wrong. This won't put it right.  → wrong
  3. (Let {sb} answer.)  → sb
#kneel SEVERIN: They kneel to a master who keeps his word. Ask them which they'd rather.  → dark
#wrong SEVERIN: Your king said sorry, did he? My mother is still in that square.  → dark
#sb SB: You're right about the Aurelians.
SEVERIN: Am I? Then you and I may have more to talk about than your friends would like.  ~ ambition +5  → dark
#dark ! anim severin raise-hand · tint black · sfx sun-out
NARRATOR: Severin lifts a hand, and the sun outside goes out. In the dark, he moves.  ~ fight a2-c4-04-survive
```

`a2-c4-04-survive` is a fight you can't win: it ends after 90 seconds of combat, or at 60 seconds if anyone drops below a quarter of their health, and plays the next script.

#### script a2-c4-04-survive-after

```
! anim soulbound lift-item · light eye-of-solvaris gold-violet · sfx sunbeam · shake 1
NARRATOR: {sb} lifts the Eye. Gold-and-violet light goes across the sanctum like a scythe.
NARRATOR: It hits Severin and throws him into a pillar. It keeps going. It goes across the party.
! anim valencia burn · anim brannock shield-cover mira
? timer 3 → burned
  1. {sb}! Stop!  ~ bond +5  → stopped
  2. (Grab the Eye.)  ~ bond -5 · set mc-took-eye  → grabbed
#stopped NARRATOR: {sb} drops the beam.  → crane
#grabbed ! anim you grab-item soulbound
SB (shock): You took it off me.  → crane
#burned NARRATOR: The beam runs until the Eye goes dark. Valencia stays down.  ~ affinity-valencia -10 · ambition +5  → crane
#crane ! anim crane snatch · hold crane hand_front eye-of-solvaris · anim severin cloak-around valencia · sfx bat-mist
NARRATOR: Crane darts in and snatches the Eye off the floor. Severin has his cloak round Valencia.
SEVERIN: You see? Even your heroes can't hold the sun without burning their friends. Come home, daughter.
! hide severin · hide crane · hide valencia · tint eclipse-4
NARRATOR: Then there's only bats, and the smell of blood, and a weaker light outside than before.  ~ eclipse-stage =4 · leave valencia · set a2-c4-04
```

`leave valencia` takes her out of the party until A2-C6-01 (new effect, build plan §3).

### A2-C4-05 · Aftermath

#### script a2-c4-05 · plays straight after

```
BRANNOCK (angry) [to {sb}]: You nearly cooked her. And us.
SB: I stopped him. He'd have killed every one of us. Is nobody going to say that?
SERA: I saw your light, {sb}. It wasn't gold.
! camera soulbound · face soulbound you
SB [quietly, to you]: I had him. For one second I had the means to end it. And then Crane. Crane, of all people.
SB: I'm going to fix this. All of it.  ~ ambition +5 · set sb-used-eye · set a2-c4-05
```

---

## Chapter 5 · "Noxhallow"

### A2-C5-01 · The war council

#### script a2-c5-01 · map `sunspire-keep` · war room · plays on arrival

```
! tint eclipse-4
CHAPTER: Chapter 5 · Noxhallow
THESSALY: With the Eye and the girl, Severin can finish in three days. After that the sun doesn't come back.
CORWIN: The Order will storm his castle and burn it to the rock. With his daughter in it, if that's what it takes.
?
  1. Valencia's one of us. Anyone who hurts her answers to me.  ~ set mc-claimed-valencia  → ours
  2. Only her blood makes it shine. Kill her and you've lost.  → blood
  3. (Let {sb} answer.)  → sb
#ours CORWIN (cold): Understood, Champion.  → plan
#blood CORWIN: Then she lives. For now.  → plan
#sb SB: The Order takes my orders in Noxhallow, Sir Corwin, or it stays home.
LEON [after a moment]: You heard Sir {sb}.  ~ ambition +3  → plan
#plan LEON: The Guild will open a gate at the foot of the falls. The Order hits the main gate to draw them off. The Champions go in up the ice.
KAEL: Up the ice. Into a vampire castle. In a night that doesn't end. I love it here.
{if ashfall-loyal-sb} BRENNA: You came for Ashfall, {sb}. Ashfall comes for you.  ~ set a2-c5-01  → end
{if !ashfall-loyal-sb} THESSALY: The gate opens at the bell. Don't be late.  ~ set a2-c5-01  → end
```

`mc-claimed-valencia` gives +5 to `affinity-valencia` when she rejoins at A2-C6-01.

### A2-C5-02 · The frozen falls

The climb up `noxhallow-falls` and through `noxhallow`, as the old draft had it: ice ledges, thrall patrols to sneak past (Kael leads if he's with you, and the patrols' sight cones are shorter), the hanging library, the blood-fountain halls.

#### script a2-c5-02-corwin · map `noxhallow` · courtyard trigger · needs `!corwin-hostile`

```
! place corwin at courtyard · show gilded-knights · show thralls
CORWIN: Champions! We're pinned! If you've a mind to help, now's the moment!
?
  1. Hold on. We're coming.  → help
  2. We can't stop. The Eye first.  → leave
#help CORWIN: Shields up! The Champions are with us!  ~ fight a2-c5-02-courtyard  → end
#leave NARRATOR: You go on. Behind you, the Order's line holds a while, then doesn't.  ~ set corwin-dead  → end
```

#### script a2-c5-02-courtyard-after

```
CORWIN (tired): I owe you a life. I don't enjoy owing things.  ~ set corwin-saved
```

The fight costs time: the party reaches the Orrery with its rest-based resources not refreshed (no short rest at the library landing).

#### script a2-c5-02-corwin-death · plays in A2-C6-05 if `corwin-dead`

```
! place corwin at gate-ruin · anim corwin downed
CORWIN [dying, to whoever finds him]: Three hundred years. We told ourselves it was holy.
CORWIN: Tell the girl I'm sorry. Tell her I said it myself.
```

### A2-C5-03 · Crane

#### script a2-c5-03 · map `noxhallow` · treasury trigger

```
! place crane at treasury-chest · anim crane stuff-satchel
CRANE: Sir {sb}. Thank heaven. I was coming to find you. I have everything you'll need. Severin's guard rotations, the way into the ritual chamber—
SB: You sold me.
CRANE: I sell everyone, Sir {sb}. You knew that when you bought me, and it was the cleverest thing you did all year. Sir {sb}, if you'd only let me—
! anim soulbound draw
?
  1. {sb}, he's a prisoner. Leon decides.  → prisoner
  2. (Grab {sb}'s arm.)  → arm
  3. (Say nothing.)  → nothing
#prisoner SB: Fine.
! anim soulbound pommel-strike crane · anim crane downed
NARRATOR: {sb} hits him once with the pommel. He goes down and stays down.  ~ ambition +3 · set crane-died-cell  → end
#arm ! anim you grab-arm soulbound
SB [not looking at you]: Let go.
{if bond>=80} NARRATOR: {sb} lets go of the hilt.  ~ bond +5 · set crane-trial  → end
{if bond<80} NARRATOR: {sb} pulls free, and it's done in one stroke.  ~ bond -5 · ambition +5 · set sb-executed-crane  → after
#nothing ! anim soulbound strike crane · anim crane die
NARRATOR: One stroke. {sb} wipes the blade on Crane's sleeve.
SB: He'd have sold Fable to the next bidder. Somebody had to.  ~ ambition +5 · set sb-executed-crane  → after
#after {if with:brannock} BRANNOCK: That's the first time I've been afraid of you, youngling.
{if with:kael, affinity-kael-sb>=affinity-kael} KAEL: Huh. Efficient.
{if with:kael, affinity-kael-sb<affinity-kael} KAEL [quietly]: Huh. Efficient.
{if with:sera} NARRATOR: Sera says nothing. She goes and stands beside you.
```

`affinity-kael-sb>=affinity-kael` compares two meters (a new condition, build plan §3).

### A2-C5-04 · The Blood Orrery

#### script a2-c5-04 · map `blood-orrery` · plays on arrival

```
! place valencia at orrery-heart · anim valencia chained · place severin before-orrery · light eye-of-solvaris red-black · camera orrery
NARRATOR: The top of the falls. A clockwork orrery of iron and glass turns in the dark, sun and moons on long brass arms. Valencia is chained at the heart of it.
SEVERIN: The Eye wakes for the will of the blood. Your love made it shine. So all I need is for you to hate, my dawn.
SEVERIN: The sun. Them. Anything.
! show memory-sphere · backdrop sunfall-beam
SEVERIN: Look what your new friends did to you. Hold on to that.
VALENCIA (sad): It hurt.
SEVERIN: Yes. It did.
! backdrop off · tint black · sfx eclipse-close
NARRATOR: The last thread of light round the sun closes.  ~ eclipse-stage =5
! place party at balcony-drop · anim party drop-in
SEVERIN [without turning]: There you are. Right on time.  ~ set a2-c5-04
```

---

## Chapter 6 · "Eternal Dawn"

### A2-C6-01 · Valencia's choice

#### script a2-c6-01 · plays straight after

```
CHAPTER: Chapter 6 · Eternal Dawn
VALENCIA (angry): He is right. It hurt. {sb} burned me, and you all brought me here. Why should I not hate you?
?
  1. You don't. You hid forty years for strangers.  → forty
  2. ♥ [needs valencia-romance-2] I love you. And you love the morning, even when it burns.  ~ set valencia-romanced  → love
  3. Your mother told you. Choose it yourself.  → mother
  4. [needs affinity-valencia-sb>=20] (Let {sb} answer.)  → sb
#forty {if affinity-valencia>=40} NARRATOR: Valencia closes her eyes.  → shines
{if valencia-orla-saved, affinity-valencia>=25} NARRATOR: Valencia closes her eyes.  → shines
NARRATOR: Valencia turns her face away.  → dark
#love NARRATOR: Valencia closes her eyes.  → shines
#mother {if affinity-valencia>=30} NARRATOR: Valencia closes her eyes.  → shines
{if valencia-orla-saved, affinity-valencia>=15} NARRATOR: Valencia closes her eyes.  → shines
NARRATOR: Valencia turns her face away.  → dark
#sb SB: I'm sorry, Valencia. I lost control of it. It won't happen again.
VALENCIA: I believe you mean that. That's what frightens me.
NARRATOR: She closes her eyes.  → shines
#shines ! light eye-of-solvaris gold · anim valencia break-chains · sfx chains-melt
NARRATOR: The red goes out of the Eye. It's gold again, and the chains run like wax.
VALENCIA: I choose, Father. I choose the morning.  ~ set eye-shines · rejoin valencia  → end
#dark VALENCIA (afraid): I can't. I'm sorry. Help me.  ~ rejoin valencia · fight a2-c6-01-gears
```

`rejoin valencia` puts her back in the party, displacing nobody (she's the fifth while this act lasts; build plan §3). With `mc-claimed-valencia`, it also applies `affinity-valencia +5`.

`a2-c6-01-gears` is an extra fight phase against the Orrery itself: four gear assemblies to break before the boss. Its `Then` is `a2-c6-02`.

### A2-C6-02 · Who holds the Eye

#### script a2-c6-02 · plays straight after

```
! anim eye-of-solvaris float-free · camera eye-of-solvaris
SB: Give it to me. I know how it works now. I can hold it this time.
?
  1. Valencia carries it.  ~ affinity-valencia +5  → val
  2. I'll carry it.  ~ bond -3  → you
  3. {sb} carries it.  ~ ambition +5 · set sb-wielded-eye-final  → sb
#val ! hold valencia hand_front eye-of-solvaris
SB (smile): Of course.  ~ set eye-bearer-valencia · fight a2-c6-03-severin
#you ! hold you hand_front eye-of-solvaris
SB: You don't trust me with it.  ~ set eye-bearer-you · fight a2-c6-03-severin
#sb ! hold soulbound hand_front eye-of-solvaris · light eye-of-solvaris gold-violet
VALENCIA: Careful with it.  ~ set eye-bearer-sb · fight a2-c6-03-severin
```

Whoever holds the Eye gets the Sunbeam ability for the fight. If it's {sb}, the beam does more damage and every use costs Valencia a little health. The Eye is a live item: it's in that person's hand in the fight, in the scenes after it, and in the Deepvault until it's put in its case.

### A2-C6-03 · The Blood Sovereign

The fight, as the old draft had it:

- **The Sovereign:** eternal night. Blood whips, mist-steps, thralls. Damage only gets through after a Sunbeam strips his Blood Shield.
- **The Orrery:** the arena turns and the moons sweep the floor. The Eye-bearer lights four Sun Mirrors; each one cracks the eclipse a little and brightens the tint.
- **Dawn:** all four lit. Real sunlight. Severin is slowed, can't heal, and his blood magic sputters. He fights on.

| When | Bark |
| --- | --- |
| Dawn phase begins | `SEVERIN: Mother. It burns the way it did in the square.` |
| At a tenth of his health | `SEVERIN: Valencia. My dawn. I only wanted you to stand in the open without being afraid.` |

### A2-C6-04 · The Blood Sovereign falls

#### script a2-c6-03-severin-after

```
! tint daylight · anim severin kneel-smoking · place valencia over severin
VALENCIA: Look, Father. Sunrise, and I'm standing in it.
?
  1. Spare him. Let him live in the light.  ~ affinity-valencia +5 · set severin-alive  → spare
  2. It's your choice, Valencia.  ~ affinity-valencia +10 · set severin-alive  → choice
  3. End it. He'll never stop.  ~ affinity-valencia -5 · set severin-dead  → end-it
  4. [needs eye-bearer-sb] (Let {sb} act.)  ~ ambition +3 · affinity-valencia-sb -10  → sb
#spare VALENCIA: A sunny room, Father. You'll hate it. Maybe in a hundred years you won't.  → corwin
#choice VALENCIA: Then I'll be the daughter my mother wanted.
VALENCIA: A sunny room, Father. You'll hate it. Maybe in a hundred years you won't.  → corwin
#end-it NARRATOR: Valencia doesn't answer. She lifts the Eye.
! light eye-of-solvaris gold · anim severin ash
VALENCIA: Goodbye, Father.  → corwin
#sb SB: He'll only try again.
! anim soulbound raise-item · anim valencia stop soulbound
VALENCIA (angry): Do not.
?
  1. Spare him.  ~ set severin-alive  → spare
  2. End it.  ~ set severin-dead  → end-it
#corwin {if corwin-saved} ! place corwin at orrery-door · anim corwin kneel · drop corwin hand_front
{if corwin-saved} CORWIN: The Gilded Morning owes you three hundred years of apologies. Start with mine.
NARRATOR: The sun is coming up over the falls.  ~ set a2-c6-04
```

An unspared Severin is ash; a spared one goes to the Sunward Cell (`sunward-cell` map). A redeemed Order re-forms as a guard for Morvaine's villages and keeps its name.

### A2-C6-05 · Morning

#### script a2-c6-05 · plays straight after

```
! fade out 1 · tint off · backdrop morning-montage · fade in 1
NARRATOR: The sun comes up over Morvaine, and over Fable, and over the black fields of Doomward.
NARRATOR: In Hollowmere, people stand in their doorways with their eyes shut.  ~ eclipse-stage =0
{if crane-died-cell} NARRATOR: In Questeria, Lord Chancellor Crane is found dead in his cell. Nobody saw who went in.
NARRATOR: The Long Eclipse ends at dawn, the way nights do.  ~ set a2-c6-05
```

If `corwin-dead`, `a2-c5-02-corwin-death` plays when you walk back down through the courtyard.

### A2-C6-06 · The Deepvault

#### script a2-c6-06 · map `deepvault` · plays on arrival

```
! show stillglass-reliquary · show eye-case · anim thessaly place-item eye-case
THESSALY: Two things that should never have been made. Side by side, under twelve seals. I'll sleep better.
! place leon at vault-stair · anim leon walk-slow
LEON: Twice now Fable's been saved by the same two names. The people want a guardian. Someone they trust.
LEON: Sir {sb}, I name you Lord Protector of Fable, keeper of the Deepvault's outer seal. Sir {name}, you stand beside {sb_them}, as you always have.
THESSALY: Majesty. With respect, the Deepvault is Guild business.
LEON: The Guild keeps the inner seals. Sir {sb} keeps the outer. Two keys, Archmage, so nobody can open it alone. Not even me.
! hold leon hand_front outer-key
?
  1. We accept. Together.  ~ bond +3  → together
  2. [needs mc-heard-sb-valencia | mc-noticed-shard | mc-watching-sb] Majesty, the key should stay with the Guild.  ~ bond -10 · set mc-objected-key  → object
  3. (Let {sb} answer.)  ~ ambition +3  → sb
#together SB: Together.  → key
#object NARRATOR: Nobody says anything for a while.
SB (sad): After everything?
LEON: My decision stands.
NARRATOR: Thessaly looks at you, then at the key in Leon's hand.  → key
#sb ! anim soulbound kneel
SB: I'll guard it with my life, Majesty. I'll sleep on the vault stair if I have to, third step down, out of the draught.  → key
#key ! anim leon give-item soulbound · hold soulbound belt_front outer-key
LEON: The outer key, Sir {sb}. Mind it.  ~ set a2-c6-06
```

`[needs a | b | c]` is any one of them (a new `AnyOf` condition, build plan §3).

### A2-C6-07 · Two halves

#### script a2-c6-07 · map `deepvault` · plays that night as {sb}

```
! tint night · place soulbound at vault-stair · hold soulbound belt_front outer-key
NARRATOR: The outer seal opens at the touch of the key. The twelve inner seals glow, unbroken. {sb} can't pass them. Not yet.
! walk soulbound to relics · hold soulbound hand_front shard · light soulbound shard on · anim stillglass-reliquary lean-toward soulbound · anim eye-case lean-toward soulbound
NARRATOR: {sb} opens {sb_their} hand. In the cases, the Box and the Eye turn toward the shard, the way plants turn to a window.
VOICE(sb): You wanted to know the feel of it. Now you know the feel of both.
SB: I watched what it did to them.
VOICE(sb): You watched weak men hold great things. {name} needs you to stay small.
VOICE(sb): Good men make lovely kings in peacetime. It isn't peacetime any more.   [CALLBACK lovely-kings]
SB: Leon's family held the Eye for three hundred years and all they ever did with it was burn a square. I'd do better with it in a week.
! anim soulbound close-fist · reflection soulbound delay 1.0
NARRATOR: {sb} closes {sb_their} hand. In the Stillglass, {sb}'s reflection doesn't close its hand for a full second.
! fade out 0.5 · place you at quarters-bed · fade in 0.5 · sigil you cold
NARRATOR: You wake up. The mark on your palm is cold. It's never been cold before.
CARD: End of Act 2 · The Long Eclipse  ~ ambition +10 · set sb-has-outer-key · set act2-done
```
