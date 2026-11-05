# Companion scenes and personal quests

Everything the main script hands off to the companions: camp talks, the five personal quests, the romances, {sb}'s quieter moments, Elara's arc, and where the party banters fire. Notation as in `04-script-prologue-act1.md`. Who these people are is in `COMPANIONS.md`; this file only writes their lines. Where the two disagree, `COMPANIONS.md` wins on facts and this file wins on wording.

## How companion scripts start

The existing rule stays: talking to a follower plays the first of their scripts that can start, so a companion's talks play in order, each carrying the gold "!" until done. Every script here that's a talk follows it. Each sets `<script id>-done` when it ends (add it in C#; it's left out below to save space), and has `unless <script id>-done`.

Three new ways in (build plan §6):

- **Camp.** A map's camp state places every follower round the fire with their talks available. Resting at a camp plays nothing by itself unless a script says `plays when you rest`.
- **Quest markers on places.** A `trigger at` line works as it does in the main script.
- **Banter triggers.** A trigger tile with a bark id and a party condition (§8).

Romance steps are gated by approval and flags. The romanceable companions are Sera, Mira, Kael and Valencia. Each has an `<id>-romanced` flag, set at the step where you both say so. Only one can be set; setting one closes the others' later steps (`unless` any other `-romanced`).

---

## 1. Kestrel Ford: the camp (A1-C5-02)

{sb}'s talk is in the main script and is the last one. These four come first. Each needs `a1-c5-01-done`.

#### script sera-camp-c5 · with `sera`

```
! place sera at camp-edge · anim sera sit · hold sera hand_front whetstone
SERA: Burr's been gone since noon. He does that when there's a big bird about. Or a big army.
SERA: Two days to Greywatch. I counted the carts. Nine. Eleven if the drivers are honest about what's under the sacking.
?
  1. You count everything.  → count
  2. ♥ [needs sera-flirt-2, affinity-sera>=40] Room on that log?  → log
  3. What do you make of {sb} lately?  → sb
#count SERA: Somebody should. The crown doesn't.  ~ affinity-sera +2  → end
#log SERA: Sit closer. It's warmer. That's the only reason.  ~ affinity-sera +3 · set sera-romance-3  → end
#sb SERA: {sb} walks like a wolf at the end of winter. Not mean. Just looking at everything as if it might be dinner.  → end
```

#### script mira-camp-c5 · with `mira`

```
! place mira at fire · anim mira read · hold mira hand_front index-half
MIRA: Half of Greywatch's walls are older than the kingdom. Old Emberic masons. There's a chance I can read the foundation stones.
MIRA: I'm aware that's not what anyone else is excited about.
?
  1. What does the Index say about Greywatch?  ~ affinity-mira +2  → index
  2. ♥ [needs mira-flirt-2, affinity-mira>=40] Tell me about Doomward. Before.  → before
  3. You should sleep.  → sleep
#index MIRA: That the Hollowed went there first, last time. Forty-one years ago. My teacher wrote "a test" in the margin.  → end
#before MIRA: When this is over, I'd like to show you what it looked like. In spring. Apricots from the river all the way up the hill.  ~ affinity-mira +3 · set mira-romance-3  → end
#sleep MIRA: I do. Between chapters.  → end
```

#### script kael-camp-c5 · with `kael`

```
! place kael at cart · anim kael lean · hold kael hand_front apple
KAEL: Nine carts. Four drivers who'd sell the lot for a good price. I've been very restrained.
?
  1. Why are you still here, Kael?  → why
  2. ♥ [needs kael-flirt-2, affinity-kael>=40] Restrained for my sake?  → you
  3. {sb} vouched for you. Don't make that wrong.  ~ affinity-kael-sb +3  → vouched
#why KAEL: The pay's terrible and the company's armed. I'm working on an answer.  ~ affinity-kael +2  → end
#you KAEL: Don't tell anyone. You're the reason I haven't robbed this caravan yet.  ~ affinity-kael +3 · set kael-romance-3  → end
#vouched KAEL: The chief vouched for me. Huh. Nobody's ever done that without wanting a cut.  → end
```

#### script brannock-camp-c5 · with `brannock`

```
! place brannock at fire · anim brannock sit · hold brannock arm_back brannock-shield
BRANNOCK: Last time I sat at this ford, it was the relief column's fire. Three days. I could see it from the pass.
?
  1. Hester's cottage is in the village. Pim's sister.  [needs brannock-quest-hook]  → hester
  2. Forty-one years is a long time to carry it.  → long
  3. (Sit with him.)  ~ affinity-brannock +3  → sit
#hester BRANNOCK: I know where it is. I walked past it twice today.  ~ set hester-known  → end
#long BRANNOCK: It's not heavy. That's the trouble. You get used to it.  → end
#sit NARRATOR: He doesn't say anything else. He passes you the flask once.  → end
```

`hester-known` puts a marker on Hester's door in the Kestrel Ford village (`brannock-hester`, §3.4).

---

## 2. Greywatch: the first name (A1-C6-03)

#### script brannock-greywatch · with `brannock` · needs `a1-c6-03` · map `greywatch` · trigger at Aldric's bier

```
! place brannock at aldric-bier · anim brannock kneel · hold brannock arm_back brannock-shield
BRANNOCK: He called the king "lad". To his face. Leon let him.
?
  1. (Kneel beside him.)  ~ affinity-brannock +3  → kneel
  2. He held. Because you came.  → came
#kneel NARRATOR: You kneel. Brannock takes the shield off his arm.  → scratch
#came BRANNOCK: He held because I said I'd come. Same as last time. The only difference is I turned up.  → scratch
#scratch ! anim brannock scratch-shield · hold brannock hand_front whittling-knife · swap brannock-shield brannock-shield-1
NARRATOR: He scratches a name into the rim with the point of a knife. ALDRIC.
BRANNOCK: Forty to go.  ~ set shield-names-started
```

From here the shield is a live item with a name count. Its texture swaps at 1, at 41 (after Forty Letters, either outcome), and at 42 (A3-C5-06, if he falls). Art spec §6 lists the three textures.

---

## 3. The winter quests (A1-C7)

### 3.1 The Heron's Due ({sb})

#### script sb-heron-1 · with `soulbound` · needs `a1-c7-01`, `sb-quest-hook` · marker quest

```
! place soulbound at board · hold soulbound hand_front harrow-petition
SB: Read that. Out loud. I want to hear somebody else say it.
YOU [reading]: "For the war, a third of every catch in the southern isles, to Heronsgate."
SB: A third. For a war three hundred miles from the isles. The cove's down to six boats as it is.
SB: He looked straight at me in the gallery, {name}. He hadn't the first idea who I was.
?
  1. Then we do it properly. Elara, the fishers, letters home.  ~ bond +3 · set heron-slow  → slow
  2. [needs sb-controls-crane] Crane knows where Harrow owes money.  ~ ambition +5 · set heron-fast  → fast
  3. What do you want to do?  → want
#slow SB: Properly takes all winter.
SB: Fine. All winter. Write to Mam, will you? She'll answer you quicker than me.  ~ objective "Gather the isles' case: Old Tomas, the fishers' marks, Kettle Cove's letters"  → end
#fast SB (smile): A week. Less. Nobody even needs to know it was us.  ~ objective "Go with {sb} to Crane"  → end
#want SB: I want him on his knees on the landing. I'll settle for him losing.
SB: You pick. You're better at the kind of losing that sticks.
?
  1. The slow way. Elara, the fishers, letters home.  ~ bond +3 · set heron-slow  → slow
  2. [needs sb-controls-crane] The fast way. Crane.  ~ ambition +5 · set heron-fast  → fast
```

#### script sb-heron-tomas · map `docks` · with `tomas` · needs `heron-slow` · marker quest

```
TOMAS: The isles' tithe? I've been short-weighted by Heronsgate's men since before you were born.
TOMAS: I'll put my mark on it. And I'll get you every boat on Saltwind that's ever sold fish to them. That's forty marks by the new moon.
SB: Forty. Thank you.
TOMAS: Don't. I'm only doing it because it's you two. Nobody ever asked us before.  ~ set heron-marks
```

#### script sb-heron-letter · map `sunspire-keep` · plays at your next rest after `heron-marks` · with `soulbound`

The first of Corra's three letters. A live item: the letter is a folded sheet in {sb}'s hand, and later in {sb}'s pouch.

```
! place soulbound at window · hold soulbound hand_front corra-letter-1
SB: Mam's written. Listen. "Your friend's letter came. Tomas from the harbour wrote as well, a man I've never met. Every house on the landing signed."
SB: "Eat something green. Doggett asked after you at the new year. I told him you were busy."
SB (laugh): Busy. She's never in her life told Doggett anything.
?
  1. She sounds well.  → well
  2. Does she mention the ring?  [needs sb-shared-catch]  → ring
#well SB: She sounds like she's been practising that line about Doggett since autumn.  ~ bond +2 · set heron-letters  → end
#ring SB: She never mentions the ring. That's how I know she still thinks about it.  ~ bond +3 · set heron-letters  → end
```

#### script sb-heron-elara · map `sunspire-keep` · with `elara` · needs `heron-letters` · marker quest

```
! place elara at study-desk · hold elara hand_front harrow-petition
ELARA: Forty marks from Saltwind, every house on Kettle Cove, and a letter from someone's mother telling me to eat greens.
ELARA: Thank you. That last one was for {sb}, but I've taken it to heart.
ELARA: I can stop the petition. I can't hurry the review of the old tithe. That's a winter, at least.
SB: A winter. The cove ate kelp the last time a winter went past.
?
  1. Then we wait a winter. And send them food.  ~ bond +3  → wait
  2. {sb}'s right. Can it be faster?  → faster
#wait ELARA: I'll send grain south with the Admiralty boats. Quietly. Nobody needs to know it was a princess.  ~ set heron-heard  → end
#faster ELARA: Faster means I tell my father to overrule his own court. I'll do it once in my life. I'd rather it wasn't for fish.
SB: It isn't fish. It's—
ELARA: I know what it is. A winter.  ~ set heron-heard  → end
```

#### script sb-heron-hall · map `sunspire-keep` · trigger at the throne · needs `heron-heard` · plays at the winter's end

```
! place harrow at hall-centre · place soulbound beside you · hold elara hand_front royal-writ
ELARA: The Heronsgate petition is refused. The old tithe goes to review, and until it's settled, the isles' nets are theirs by writ.
! anim harrow turn · face harrow soulbound
HARROW: You. You were in the gallery. Who are you?
SB: Nobody. I was on the landing.
NARRATOR: Harrow looks at {sb} for a while. Then he goes back to talking about the weather.  ~ bond +5 · ambition -3 · set sb-heron-petition · set sb-heron-done
```

#### script sb-heron-crane · map `sunspire-keep` · Crane's study · needs `heron-fast`

```
! place crane at desk · place soulbound at study-door
CRANE: Lord Harrow. Of course. Three mortgages on Heronsgate, two of them with men he shouldn't owe. I'll write to both tonight.
CRANE: May I ask what Lord Harrow did to you, Sir {sb}?
SB: You may not.
CRANE: Then I'll simply remember that you asked me. That's how this works, you understand.
! fade out 0.5 · wait 1 · fade in 0.5 · place soulbound at hall-window · hold soulbound hand_front harrow-notice
NARRATOR: Six days later. Harrow's estate is in the hands of his creditors. The new tithe goes with him.
SB (smile): Six days.
?
  1. You enjoyed that.  → enjoyed
  2. It worked.  ~ ambition +2  → worked
#enjoyed SB: I did. That's the bit I don't like.  ~ ambition +5 · set sb-heron-ruined · set sb-heron-done  → end
#worked SB: It did. Don't tell Mam how.  ~ ambition +5 · set sb-heron-ruined · set sb-heron-done  → end
```

### 3.2 The Last Silverbark (Sera)

#### script sera-writ · with `sera` · needs `a1-c7-01`, `sera-quest-hook` · marker quest

```
! place sera at board · hold sera hand_front timber-writ
SERA: The Admiralty's posted a writ. Old growth from the Mirewood for the war fleet. Their reeve's marked the trees already.
SERA: Same as last time. They even used the same paint.
?
  1. We take it to the Lodge, the cutters and Elara.  ~ affinity-sera +3 · set silverbark-table  → table
  2. What do you want to do?  → want
#table SERA: A table. Forty-one years ago there was a table. My mother wasn't invited to it.
SERA: All right. One table. If it goes wrong, I spike the stand.  ~ objective "Bring Tamsin, the woodcutters and Princess Elara to one table"  → end
#want SERA: Iron spikes in the trunks. A saw hits one, it jumps. They stop cutting.
?
  1. Somebody could lose a hand.  → hand
  2. Do it.  ~ set silverbark-spike  → spike
#hand SERA: Somebody lost a forest.
SERA: Fine. Your table. If it goes wrong, I spike it.  ~ set silverbark-table · objective "Bring Tamsin, the woodcutters and Princess Elara to one table"  → end
#spike SERA: Tonight, then. Don't come.  → end
```

#### script sera-table · map `rangers-lodge` · needs `silverbark-table` · marker quest

Bringing the three parties is a short errand loop (talk to Tamsin at the Lodge, the cutters' foreman Hobb at the mill, Elara at the keep). Then:

```
! place tamsin at table · place hobb at table · place elara at table · place sera at table-end
HOBB: The Admiralty pays by the trunk. Old growth's thick trunk. I've six men with families.
SERA: The storm last autumn put down two hundred trunks on the north slope. They're seasoning where they lie. Nobody's touched them.
HOBB: Two hundred. Thick?
SERA: Thicker than you'd want to carry.
TAMSIN: The Lodge can mark managed stands for the rest. We've wanted to for years.
ELARA: And the old growth comes out of the writ. Write it down, Tamsin, before anyone thinks better of it.
! anim elara sign · hold elara hand_front wardens-charter
NARRATOR: The writ comes back amended by noon. Sera reads it twice.  ~ affinity-sera +10 · set sera-charter · set sera-silverbark-done
```

#### script sera-spike · map `mirewood` · needs `silverbark-spike` · plays at your next rest

```
! tint night · sfx hammer-far
NARRATOR: You hear the hammer from the camp. It goes on until nearly dawn.
! fade out 1 · fade in 1 · tint day · place sera at camp-edge
SERA: A sawyer on Hobb's crew. Joss Feld. The saw jumped. He's lost three fingers and most of the hand.
SERA: The Admiralty's withdrawn the writ. The Lodge has struck my name off.
?
  1. You did what you said you'd do.  → said
  2. A man lost his hand, Sera.  → hand
  3. (Say nothing.)  → nothing
#said SERA: Yes. I'm aware.  → end-spike
#hand SERA: I know his name. I know his wife's name. I'll know them a long time.  → end-spike
#nothing NARRATOR: Sera goes back to oiling her bow. She does it for an hour.  → end-spike
#end-spike SERA: Rot and rain. Joss Feld, three fingers. That's a count I'll keep.  ~ affinity-sera -10 · set sera-spiked · set sera-silverbark-done
```

`sera-spiked` closes Sera's romance (every later step needs `!sera-spiked`).

#### script sera-silverbark · with `sera` · needs `sera-charter` · map `mirewood` · trigger at the silverbark

```
! place sera at silverbark · anim sera hand-on-bark
SERA: Forty-one years old. Too young for the axe, the last time. Now it's the oldest thing in the reach.
SERA: Elara wants seeds for the palace garden. I said yes before I'd thought about it.
{if sera-romance-3} ! anim sera take-hand you · anim you hand-on-bark
{if sera-romance-3} NARRATOR: She puts your hand on the bark, and leaves hers over it.  ~ set sera-romance-4  → end
SERA: You can touch it. It won't bite. That's more than I'd say for most things in here.
```

### 3.3 A Name on a Door (Kael)

#### script kael-gannet · map `low-ring` · plays inside Gannet's counting house, as part of A1-C7-03, before {sb} leaves · needs `kael-quest-hook`

```
! place gannet at counting-desk · hold gannet hand_front ledger
GANNET: Duskmantle. You've grown. Still owe me for a window, as I recall.
KAEL: Twelve families on Tallow Lane. You've doubled them. And you've been washing Doomward marks through this room for a year.
GANNET: Prove it.
! anim kael lift-ledger · hold kael hand_front ledger
KAEL: I can read upside down. Did you know that? Ma taught me. Laundry tickets.
SB [to you]: Crane'd make the deeds disappear into Kael's name by morning. Clean. Nobody'd ever argue.
?
  1. We take the ledger to Captain Hale. Properly.  ~ affinity-kael +10 · affinity-kael-sb -5 · set kael-lane-kept  → hale
  2. Let {sb} handle it.  ~ affinity-kael-sb +10 · bond +2 · set kael-lane-given  → given
  3. Your lane, Kael. You decide.  ~ affinity-kael +5  → kael
#kael KAEL: Me? Nobody asks me things.
KAEL: Hale. The lane should be theirs. Not mine. Theirs.  ~ affinity-kael +5 · affinity-kael-sb -5 · set kael-lane-kept  → hale
#hale GANNET (cold): Hale won't thank you. The Watch never thanks anyone.
KAEL: I didn't ask it to.  ~ set kael-gannet-done  → end
#given SB (smile): Leave it with me. You'll have the keys by the new moon.
KAEL [quietly]: Keys. Mine. Huh.  ~ set kael-gannet-done  → end
```

#### script kael-kitchen · map `low-ring` · with `kael` · needs `kael-lane-kept` · trigger at the old laundry on Tallow Lane

```
! show sign-duskmantles · place kael at laundry-door · show low-ring-kids · place wren at stove
NARRATOR: Somebody has painted DUSKMANTLE'S over the old laundry door. The letters lean. The S is backwards.
KAEL: The tenants did it. Wren did the S. Don't tell her.
KAEL: It's a kitchen now. The kids eat here. I didn't really decide that, it just sort of kept happening.
{if kael-romance-3, !sera-romanced, !mira-romanced, !valencia-romanced} ! face kael you
{if kael-romance-3, !sera-romanced, !mira-romanced, !valencia-romanced} KAEL: Stay. For supper. That's all. Stay.
?
  1. I'll stay.  ~ affinity-kael +5 · set kael-romanced  → yes
  2. Another time.  → end
WREN: Are you eating or are you standing in the door?  → end
#yes NARRATOR: Kael doesn't make a joke about it. He gets you a bowl.  → end
```

### 3.4 Forty Letters (Brannock)

#### script brannock-hester · map `kestrel-ford` · trigger at Hester's door · needs `hester-known`

```
! place hester at cottage-door · hold hester hand_front shawl
HESTER: Captain Ironvein. I'd know that beard anywhere. You've got it braided different.
BRANNOCK: Mistress Hollis.
HESTER: Pim wrote about you. Every letter. I've got them inside if you want them.
BRANNOCK: I've one for you. I've had it forty-one years.
HESTER: Then you'd best come in and sit before you drop it.  ~ set hester-met · objective "Help Brannock deliver the letters this winter"
```

#### script brannock-letters · map `rusty-anvil` · with `brannock` · needs `hester-met`, `a1-c7-01` · marker quest

```
! place brannock at anvil-table · hold brannock hand_front letter-bundle
BRANNOCK: Thirty-nine more. Fourteen in the city. The rest along the Kestrel road and up into Deepcleft.
BRANNOCK: Hester read me Pim's last one home. "The Captain says hold, so we'll hold. I'd follow him into the sea."
?
  1. Then we walk them round. All of them.  ~ affinity-brannock +5 · set letters-walk  → walk
  2. You don't have to do this.  → dont
#walk BRANNOCK: We'll see. Get your boots.  ~ objective "Deliver the forty letters"  → end
#dont BRANNOCK: No.
! anim brannock throw-fire · drop brannock hand_front · sfx paper-burn
NARRATOR: He drops the bundle in the fire. It's dry. It goes quickly.  ~ affinity-brannock -5 · set brannock-letters-burned · set brannock-letters-done · swap brannock-shield brannock-shield-41  → end
```

The walk is ten doors on the city and road maps; the other twenty-nine letters go in the same houses or the same villages, and aren't played one by one. Each door is a one-line scene, written below, and Hester's plays the end. Who answers and what they say:

| Door | Line |
| --- | --- |
| A tanner in the Low Ring | `TANNER: My grandad. I never met him. Thank you for bringing it.` |
| A widow on Saltwind | `WIDOW: Forty-one years. Did you think I'd be angry? I was angry for ten.` |
| A smith in the Lantern Market | `SMITH: He mentions you. "The Captain's teaching me cards. He cheats."` |
| Three sisters at the Kestrel inn | `ELDEST SISTER: We drew lots for who'd hate you. Nobody wanted it.` |
| A miller's family | `MILLER: Stay for bread. He'd have wanted you fed.` |
| Two doors with nobody left | `NARRATOR: Nobody lives here now. Brannock leaves the letter under a stone by the step.` |
| A Stoneborn shrine in Deepcleft | `KEEPER: We'll put it with his name. You'll stay for the reading.` |
| A farmer who closes the door | `NARRATOR: The door shuts. After a minute it opens again, and a hand takes the letter.` |
| Hester, last | (the script below) |

#### script brannock-letters-end · map `kestrel-ford` · trigger at Hester's door · needs `letters-walk` and the nine others

```
! place hester at cottage-door · anim brannock stand
HESTER: That's all of them, then.
BRANNOCK: All forty.
HESTER: Nobody blamed you, Captain. Not one house. You know that now.
BRANNOCK: I know they said so.
HESTER: That'll do to be going on with.
! swap brannock-shield brannock-shield-41
NARRATOR: On the walk back he scratches the last names into his shield. He's quicker at it than he was.  ~ affinity-brannock +10 · set brannock-letters-delivered · set brannock-letters-done
```

### 3.5 The Ember Index (Mira)

#### script mira-index · map `ashfall` · with `mira` · needs `a1-c7-05`, `mira-quest-hook` · marker quest

```
! place brenna at muster-board · hold brenna hand_front roll-of-living
MIRA: Brenna. You keep a list of the living. Could I see it? Only the A's.
BRENNA: You're looking for someone.
MIRA: Ashgrove. Tobin. A cider-presser from Orrin's Reach. He has a burn on his left hand from a still.
! anim brenna read-roll
BRENNA: Ashgrove, T. Thornwall levy, second company. They walk the path with us tomorrow.
NARRATOR: Mira sits down on the step, quite suddenly.  ~ set tobin-alive
```

#### script mira-tobin · map `thornwall` · trigger at the levy's camp · needs `tobin-alive`, `thornwall-guided`

```
! place tobin at levy-fire · anim tobin stand · place mira facing tobin
TOBIN: Mira? Mira, you've cut your hair.
MIRA: Papa.
TOBIN: They came for the village and I said no. I'd heard what the cube did. They sent me to dig ditches instead. I've been digging ditches for two years.
MIRA: I didn't know if you were dead or one of them. I couldn't even grieve properly. I'm very cross with you.
TOBIN: Yes. I'd imagine.
! anim mira hug tobin
NARRATOR: Neither of them lets go for a while. The levy captain pretends to check the path.  ~ affinity-mira +5 · set mira-reunion
```

#### script mira-index-end · map `ember-archive` · trigger at the lowest stacks · needs `a1-c8-01`, `mira-quest-hook`

```
! place mira at stack-shelf · hold mira hand_front index-half · show index-other-half
MIRA: Here. The other half. He shelved it under "agriculture". That's very him.
! anim mira lift-item · hold mira hand_back hollowing-ledger
MIRA: And this. His ledger. Every name the cube took, in his hand.
{if mira-reunion} MIRA: Ashgrove, T. "Refused. Sent to the levies." He wrote "refused" very neatly.  → choice
MIRA: Ashgrove, T. "Footman. Greywatch." He was there. At the walls. We fought them and he was there.
MIRA: I can stop wondering. That's something. It's a very small something.  → choice
#choice MIRA: The Index is the only complete account of what the Box does. Anyone who reads it can do it again.
?
  1. Give it to Thessaly. Under the Guild's seal.  ~ affinity-mira +10 · set mira-index-shared  → share
  2. Your call, Mira.  → hers
  3. Burn it.  ~ affinity-mira -5 · set mira-index-burned  → burn
#hers MIRA: My teacher kept things nobody should have kept. I don't want to be like him in that.
MIRA: But I don't want to be the only one who knows, either. Thessaly, then. Under seal.  ~ affinity-mira +10 · set mira-index-shared  → share
#share ! anim mira bind-book · hold mira hand_front sealed-index
MIRA: Somebody else should be able to check my work. That's what an archive's for.  → after
#burn ! anim mira burn-book · sfx paper-burn · drop mira hand_front
MIRA: Then nobody reads it again. Including me.  → after
#after {if mira-romance-4, !mira-index-burned, !sera-romanced, !kael-romanced, !valencia-romanced} MIRA: The last person I loved with my whole heart became something else.
{if mira-romance-4, !mira-index-burned, !sera-romanced, !kael-romanced, !valencia-romanced} MIRA: I've decided that isn't a reason. If I'm wrong again, I'd like to be wrong about you.  ~ set mira-romanced · set mira-index-done  → end
MIRA: Let's go. He's waiting for us.  ~ set mira-index-done
```

`mira-index-burned` stalls her romance: the commitment needs the Index shared.

---

## 4. The winter balcony (A1-C7)

One per romance, on the Sunspire's west balcony at night, during the winter. Each needs `<id>-romance-3` (or Sera's `-4`), `a1-c7-01`, and none of the other `-romanced` flags. Only the first one you go to plays; it sets `<id>-romanced` if you say yes. A different companion's balcony scene won't start after that.

#### script sera-romance-winter · with `sera` · needs `sera-romance-4`, `!sera-spiked`

```
! tint snow-night · place sera at balcony-rail
SERA: Burr won't fly in this. He's sulking on the stable roof.
SERA: I've been counting you. Since the quarry. Where you are in a fight. Where you sleep. I didn't mean to start.
?
  1. ♥ Don't stop.  ~ affinity-sera +5 · set sera-romanced  → yes
  2. Sera, I don't feel that way.  → no
#yes NARRATOR: She doesn't answer. She moves along the rail until your shoulders touch, and stays there.  → end
#no SERA: All right. I'll stop. It'll take a while. I'm thorough.  → end
```

#### script mira-romance-winter · with `mira` · needs `mira-romance-3`

```
! tint snow-night · place mira at balcony-rail · hold mira hand_front ink-pen
MIRA: Thessaly's let me into the Spire's late stacks. I've got ink on everything. I've got ink on you, look.
! anim mira touch-hand you
MIRA: Sorry. That's not why I did that.
?
  1. ♥ (Leave your hand where it is.)  ~ affinity-mira +5 · set mira-romance-4  → yes
  2. It'll wash off.  → no
#yes MIRA: All right. Good. I'm going to keep talking about the stacks so I don't have to think about it.  → end
#no MIRA: It will. Everything does, eventually. Goodnight, {name}.  → end
```

Mira's commitment is at the Archive (`mira-index-end`), not here. This step only opens it.

#### script kael-romance-winter · with `kael` · needs `kael-romance-3`, `!kael-romanced`

```
! tint snow-night · place kael at balcony-rail · hold kael hand_front wine-bottle
KAEL: I didn't steal this. Before you ask. Someone left it near me for a long time.
KAEL: So this is what the top of the city looks like. I always thought it'd be warmer up here.
?
  1. ♥ Come here, then.  ~ affinity-kael +5 · set kael-romanced  → yes
  2. Go to bed, Kael.  → no
#yes KAEL: Gorgeous, you can't just say things like that to people.
NARRATOR: He comes anyway.  → end
#no KAEL: Story of my life. Night, boss.  → end
```

Kael can be romanced either here or in the kitchen. Both set `kael-romanced`.

---

## 5. Act 2

### 5.1 The Willing (Valencia)

#### script valencia-willing-news · plays at the camp in `morvaine-pass` · needs `a2-c3-01`, `valencia-quest-hook`

```
! place valencia at camp-edge · hold valencia hand_front abbey-letter
VALENCIA: A letter from Mother Anselle. The Order came back to the abbey. They've taken Orla.
VALENCIA: "A vampire's cattle." That's what they wrote on the warrant. She's at their chapterhouse on the highland road.
VALENCIA: They'll hang her at the new moon. Six days.  ~ set willing-started · objective "Get Sister Orla out of the Gilded Chapterhouse before the new moon"
```

The quest runs on a clock: if A2-C4-01 plays before it's done, it fails (`valencia-orla-died`, below).

#### script valencia-willing · map `gilded-chapterhouse` · trigger at the cells · needs `willing-started`

```
! place orla at cell · place gilded-sergeant at cell-door · hold orla hand_front smoke-pot
GILDED SERGEANT: The prisoner is a confessed blood-donor to an abomination. It's the Order's business.
?
  1. [PERSUADE 14 · adv voice:honorable, !corwin-hostile] Ask her why she gave it.  → pass orla · fail fight
  2. Open the cell.  → fight
#orla GILDED SERGEANT: Fine. Woman, why?
ORLA: I gave because she asked. Every time. That's the whole of it.
ORLA: Thirty years. She never once took more than the cup. I've had worse from bees.
GILDED SERGEANT: That isn't what the warrant says.
ORLA: Then the warrant's wrong, and you can write that down too.  → out
#fight GILDED SERGEANT: To arms! Holy ground!  ~ fight a2-willing-cells
#out ! anim gilded-sergeant unlock · walk orla to valencia
VALENCIA: Orla.
ORLA: You look thin. Have you been eating?
VALENCIA: Orla, don't.
ORLA: I'm only asking.  ~ affinity-valencia +10 · set valencia-orla-saved
```

#### script a2-willing-cells-after

```
! anim gilded-sergeant yield · walk orla to valencia
ORLA: You look thin. Have you been eating?
VALENCIA: Orla, don't.
ORLA: I'm only asking.  ~ affinity-valencia +10 · set valencia-orla-saved
```

#### script valencia-orla-died · map `sunscar-waste` · plays on A2-C4-01's arrival · needs `willing-started`, `!valencia-orla-saved`

```
! place valencia at sunscar-edge · hold valencia hand_front abbey-letter
VALENCIA: Anselle's written again. Orla died in their cells. The night before the new moon. They didn't even wait.
VALENCIA (cold): I'm not going to talk about it. Please don't make me.  ~ affinity-valencia -10 · set valencia-orla-dead
```

### 5.2 Valencia's talks

#### script valencia-talk-1 · with `valencia` · needs `valencia-recruited`

```
VALENCIA: You're going to ask what I eat. Everybody does, eventually. They're usually less polite about it.
?
  1. I wasn't going to ask.  ~ affinity-valencia +3  → ask
  2. What do you eat?  → eat
#ask VALENCIA: Then I'll tell you. A cup at the new moon, given. Thirty years from one woman who keeps bees.
VALENCIA: I've never once taken it. If I ever need a gravestone, that's what I'd like on it.  → end
#eat VALENCIA: Honey cakes, mostly. And at the new moon, a cup of someone who offered.  → end
```

#### script valencia-talk-2 · with `valencia` · needs `a2-c4-05`

```
! place valencia at shade · anim valencia touch-burn
VALENCIA: It's healing. Slowly. The old enemy leaves a mark, and {sb}'s light left a worse one.
?
  1. {sb} didn't mean to.  ~ affinity-valencia-sb +5  → mean
  2. I should have stopped it sooner.  ~ affinity-valencia +5  → sooner
#mean VALENCIA: No. A person who means it can at least decide not to.  → end
#sooner VALENCIA: You stopped it at all. That's more than most people manage with my father.  → end
```

#### script valencia-dawn · map `hollowmere` · plays after `a2-c6-05` · needs `valencia-romanced`

```
! tint dawn · place valencia at doorway · anim valencia face-sun
VALENCIA: Look at me. Standing in a doorway at sunrise like a farmer's wife.
VALENCIA: The old enemy's quite warm, it turns out. Nobody ever tells you that.
! anim you stand-beside valencia
VALENCIA: Stay a minute. I don't need you to. I'd like you to. That's a new thing for me.
```

### 5.3 The others in Act 2

One talk each, after the given beat. Each is the companion's Act 2 voice from `COMPANIONS.md`.

#### script sera-a2 · with `sera` · needs `a2-c3-02`

```
SERA: Morvaine pines. They grow slow up here. Half the light, half the rain. Everything's thin and old.
SERA: Like the people. A cup a month and no wolves. I can see why they took it. I'd still have said no.
```

#### script mira-a2 · with `mira` · needs `a2-c4-03`

```
MIRA: The Eye's case. The Box's case. The Guild made both. The same hand engraved the seals, two hundred years apart, which isn't possible.
MIRA: I'm writing it down. I don't like things that aren't possible. They're always the important ones.
```

#### script kael-a2 · with `kael` · needs `a2-c3-02`

```
KAEL: Hollowmere's tithe is a cup a month. Questeria's is a third of your wages to a landlord. I'm not saying I prefer it.
KAEL: I'm saying I did the sums and I'm having a very confusing afternoon.
```

#### script brannock-a2 · with `brannock` · needs `a2-c4-05`

```
BRANNOCK: I put my shield in front of Mira. Didn't think about it. Old habit.
BRANNOCK: Forty years I've been waiting to find out what it'd be like if I got there in time. It's like that. You don't feel anything until after.
```

#### script sb-a2-purge · with `soulbound` · needs `a2-c1-03`

```
! place soulbound at festival-square · anim soulbound look-up
SB: They're taking the lanterns down. Same square. I keep looking at the cobbles.
SB: Leon thanked us on the boat. Well, Odelle said he would. And he did. And he's the grandson of the man who stood on that balcony.
?
  1. He told us himself. That counts.  ~ bond +2  → counts
  2. Every crown's got something under it.  ~ ambition +3  → under
#counts SB: It does count. I'm trying to work out what for.  → end
#under SB: I know. I just thought we'd found the one that didn't.  → end
```

#### script sb-a2-crane · with `soulbound` · needs `sb-executed-crane` · plays at the next camp

```
! place soulbound at fire · anim soulbound clean-blade · hold soulbound hand_front blade
SB: You're going to look at me like that all night, aren't you.
?
  1. You killed a prisoner, {sb}.  ~ bond -2  → prisoner
  2. Are you all right?  ~ bond +3  → right
  3. (Sit down next to {sb_them}.)  ~ bond +3  → sit
#prisoner SB: I killed a man who sold Thornbridge, and Greywatch, and us. I'm not sorry. I'm sorry you had to watch.  → end
#right SB: Ask me in a week.  → end
#sit NARRATOR: {sb} keeps cleaning the blade. After a while, {sb} leans against your shoulder.  → end
```

#### script corra-letter-2 · plays on arrival at `sunspire-keep` after `a2-c6-06` · with `soulbound`

```
! place soulbound at window · hold soulbound hand_front corra-letter-2
SB: Mam again. "They've put your name on a board on the landing. Lord Protector. I'd rather they'd put a rail on the landing."
SB: "Somebody will fall off it one of these winters."
SB (smile): She's never once been impressed by anything I've done.
?
  1. She's pleased. She just won't say it.  → said
  2. She's right about the rail.  ~ bond +2  → rail
#said NARRATOR: {sb} folds the letter very small and puts it in the pouch with the key.  ~ bond +2  → end
#rail SB (laugh): She is. I'll pay for one. First thing.  → end
```

### 5.4 Elara in Act 2

#### script elara-a2 · with `elara` · needs `a2-c6-06` · map `sunspire-keep` · marker event

```
! place elara at gallery-rail
ELARA: Two keys so nobody can open it alone. That was my idea. I'm telling you so you'll know who to blame.
ELARA: My father thinks the world of {sb}. So does the city. I keep waiting for somebody to say something I can argue with.
?
  1. You've got doubts.  → doubts
  2. {sb} has earned it.  ~ affinity-elara -3  → earned
  3. I've got doubts too.  ~ affinity-elara +5 · set elara-confided  → too
#doubts ELARA: I've got a gallery full of people who all agree. That's what I've got.  → end
#earned ELARA: Yes. That's exactly the problem. Earned things are very hard to take back.  → end
#too ELARA: Thank you. Truly. Now neither of us can say we didn't know.  → end
```

---

## 6. Act 3

### 6.1 The birthday parcel

#### script sb-birthday-parcel · map `mirewood` (camp state) · plays the first time you come back to camp after any liberation

```
! place courier at camp-edge · hold courier hand_front parcel · anim courier give-item you
COURIER: For Sir {name}. No name on it. Paid in silver, and the man said not to wait for an answer.
! anim you open-parcel · hold you hand_front herring-new
NARRATOR: A herring, whittled from driftwood. The cuts are new. There's no note.   [CALLBACK herring]
NARRATOR: It's your birthday. You'd lost track of the date.
?
  1. (Put it on with the other one.)  ~ bond +3  → wear
  2. (Put it in your pack.)  → pack
  3. (Throw it in the fire.)  ~ bond -5  → fire
#wear ! hold you neck herring-pair
NARRATOR: You hang it beside your own. The two of them knock together when you move.  → end
#pack NARRATOR: You put it at the bottom of your pack, under everything.  → end
#fire ! anim you throw · sfx fire-crackle
NARRATOR: Driftwood burns slowly. You watch it the whole way.  → end
```

This is the `herring` callback's second use. `herring-pair` is a neck item texture with two herrings (art spec §6). If you also took {sb}'s at the coup, the herring in your hand at A3-C5-04 is {sb}'s, from your pouch.

### 6.2 Corra's last letter

#### script corra-letter-3 · plays at the camp after `a3-c3-06`

```
! place courier at camp-edge · anim courier give-item you · hold you hand_front corra-letter-3
NARRATOR: A letter, addressed to you, in a hand you haven't seen since you were sixteen.
CORRA [letter]: Everyone on the landing says what a good thing it is, what {sb}'s doing. They put up a statue on the harbour wall.
CORRA [letter]: Nobody writes me the truth any more. I'd like it from you. You were always the one who told me when {sb} had been in the water.  ~ give corra-letter-3
?
  1. (Write back. Tell her everything.)  → truth
  2. (Write back. Tell her {sb}'s well.)  → lie
  3. (Don't write back.)  → none
#truth NARRATOR: It takes you most of the night. You leave out Leon's last words. You can't make them fit on the page.  ~ set corra-told  → end
#lie NARRATOR: It takes you four lines.  → end
#none NARRATOR: You put it with the herring.  → end
```

`corra-told` is for Act 4 (Corra is at Kettle Cove when Vorlaith reaches the pier).

### 6.3 The declarations

One per committed romance, at the resistance camp, after A3-C3-06. Each needs `<id>-romanced`.

#### script sera-a3-romance · with `sera` · trigger under the silverbark (it's at camp if `sera-charter`; otherwise at the camp's oak)

```
! place sera at silverbark · anim sera sit
SERA: I've stopped counting you. I noticed yesterday.
SERA: I just know where you are now. I don't have to check.  ~ affinity-sera +5
```

#### script mira-a3-romance · with `mira`

```
! place mira at stump-table · hold mira hand_front notes
MIRA: When this is done, I'm going to need you to tell me I was right to love {sb}, once. Not now. When it's done.
MIRA: And I'm going to need you to stay while I'm wrong about other things. Is that a reasonable thing to ask?
?
  1. Yes.  ~ affinity-mira +5  → yes
  2. (Take her hand.)  ~ affinity-mira +5  → yes
#yes MIRA: Good. I'll write it down.  → end
```

#### script kael-a3-romance · with `kael` · needs `kael-turned`

```
! place kael at camp-edge · anim kael whittle
KAEL: I keep thinking I should be somewhere else. Somewhere with a door.
KAEL: Then I look round and you're here, and I think, no, this'll do. Don't laugh.  ~ affinity-kael +5
```

#### script valencia-a3-romance · with `valencia`

```
! tint night · place valencia at camp-edge
VALENCIA: Forty years I didn't let anyone close enough to smell what I was.
VALENCIA: You smell of woodsmoke and that terrible soap. I'd know you in the dark. I mean that as a compliment. It's a very good one, where I come from.  ~ affinity-valencia +5
```

### 6.4 Elara in Act 3

#### script elara-a3-camp · with `elara` · needs `a3-c3-01` · map `mirewood` (camp state)

```
! place elara at stump-table · hold elara hand_back leon-sword
ELARA: When I was ten, my mother caught a summer fever. The court spent two days arguing over which physician had the right to treat a queen.
ELARA: She died on the second day. I've hated procedure ever since. I've learned all of it, so nobody can ever use it on me.
ELARA: And now I'm in a forest, and every decision's mine, and there's no procedure at all. And I miss it. Isn't that stupid.
?
  1. It's not stupid.  → not
  2. {sb} said the same thing once, about councils.  → sb
  3. (Say nothing.)  ~ affinity-elara +3  → nothing
#not ELARA: It is a bit. Thank you anyway.  → end
#sb ELARA: I know. {sb} and I want the same thing. I just want somebody allowed to tell me no.  ~ affinity-elara +3  → end
#nothing ELARA: You'd have lasted a week at court. Nobody there can keep quiet.  → end
```

---

## 7. Who romances whom, in one table

| Companion | Steps (flag, where) | Commitment (`<id>-romanced`) | Closed by |
| --- | --- | --- | --- |
| Sera | `sera-flirt-1` A1-C2-01, `sera-flirt-2` sera-talk-2, `sera-romance-3` sera-camp-c5, `sera-romance-4` sera-silverbark | sera-romance-winter | `sera-spiked` |
| Mira | `mira-flirt-1` A1-C3-02, `mira-flirt-2` mira-talk-3, `mira-romance-3` mira-camp-c5, `mira-romance-4` mira-romance-winter | mira-index-end | `mira-index-burned` |
| Kael | `kael-flirt-1` A1-C4-02, `kael-flirt-2` kael-talk-2, `kael-romance-3` kael-camp-c5 | kael-romance-winter or kael-kitchen | none |
| Valencia | `valencia-flirt-1` A2-C2-01, `valencia-romance-2` A2-C3-03 | A2-C6-01, option 2 | `valencia-orla-dead` doesn't close it |

---

## 8. Banters

Banters are barks: two to four lines, no choices, played over the map as speech bubbles while you walk (`Story/Banters.cs`). A banter trigger is a tile with `banter <id>` and a party condition; it fires once. Every line stands on its own, so a banter that cuts off when you walk away still reads.

### Placing the three that exist

| Banter | Trigger | Needs |
| --- | --- | --- |
| `party-sera-mira` | `kestrel-ford`, the road out of the village | `with:sera, with:mira` |
| `party-kael-sb` | `questeria`, Lantern Market north steps, after `a1-c4-03` | `with:kael` |
| `party-brannock-kael` | `greywatch-road`, the switchbacks | `with:brannock, with:kael` |

### New

#### banter party-valencia-kael · `saint-lumen` to `morvaine-pass` road · `with:valencia, with:kael`

```
KAEL: So. Sixty-eight. What's the secret?
VALENCIA: Never going outside.
KAEL: That's my secret too, as it happens. Mostly from the Watch.
```

#### banter party-valencia-mira · `glasswood` entrance · `with:valencia, with:mira`

```
MIRA: Your mother's glasshouse. Do you know who built it?
VALENCIA: My mother, with a trowel and a great deal of swearing.
MIRA: I'd have liked her.
VALENCIA: She'd have liked you. She liked anyone who read the labels.
```

#### banter party-brannock-valencia · `sunscar-waste` · `with:brannock, with:valencia` · needs `a2-c4-05`

```
VALENCIA: You put yourself between Corwin and me at the chapel. I never thanked you.
BRANNOCK: Didn't do it for thanks.
VALENCIA: No. I gathered. That's why I'm thanking you.
```

#### banter party-sera-valencia · `noxhallow-falls` base · `with:sera, with:valencia`

```
SERA: There's an owl in that pine. She's been watching you since the river.
VALENCIA: Owls always do. I think they suspect me.
SERA: They're right to. You'd suspect you.
```

#### banter party-mira-brannock-a3 · `mirewood` camp · `with:mira, with:brannock` · needs `a3-c3-01`

```
BRANNOCK: You've stopped reading at meals.
MIRA: I've run out of things that help.
BRANNOCK: Read me one anyway. I don't need it to help.
```

#### banter party-kael-sera-a3 · `questeria` roofs (SQ3-Hale) · `with:kael, with:sera`

```
KAEL: You're very quiet on a roof for a tall woman.
SERA: You're very loud for a thief.
KAEL: I'm talking so the guards think we're guards. It's a technique.
SERA: It's a noise.
```

#### banter party-all-siege · `questeria-plain`, the night before A3-C4-01 · needs any three companions

```
BRANNOCK: Sleep, the lot of you. Tomorrow you'll want it.
KAEL: Nobody sleeps the night before a siege.
SERA: I will.
MIRA: She will, too. I've watched her. It's unnerving.
```

---

## 9. Flags this file sets

| Flag | Set in | Read by |
| --- | --- | --- |
| `<id>-romance-3`, `-4`, `<id>-romanced` | §1, §3, §4, A2-C6-01 | §4, §6.3, A3-C4-04 (`kael-romanced`) |
| `hester-known`, `hester-met`, `letters-walk` | §1, §3.4 | §3.4 |
| `shield-names-started` | §2 | art (shield texture) |
| `heron-slow` / `heron-fast`, `heron-marks`, `heron-letters`, `heron-heard` | §3.1 | §3.1 |
| `sb-heron-petition` / `sb-heron-ruined`, `sb-heron-done` | §3.1 | A1-C7-04, A3-C3-07 |
| `silverbark-table` / `silverbark-spike` | §3.2 | §3.2 |
| `sera-charter` / `sera-spiked`, `sera-silverbark-done` | §3.2 | A1-C7-04, A3-C1-05, A3-C3-01 |
| `kael-lane-kept` / `kael-lane-given`, `kael-gannet-done` | §3.3 | A3-C4-04 |
| `brannock-letters-delivered` / `-burned`, `brannock-letters-done` | §3.4 | A1-C7-04, A3-C4-05 |
| `tobin-alive`, `mira-reunion` | §3.5 | §3.5 |
| `mira-index-shared` / `mira-index-burned`, `mira-index-done` | §3.5 | A3-C1-05, A3-C3-01, A3-C3-02 |
| `willing-started`, `valencia-orla-saved` / `valencia-orla-dead` | §5.1 | A2-C6-01 |
| `elara-confided` | §5.4 | Act 4 |
| `corra-told` | §6.2 | Act 4 |
