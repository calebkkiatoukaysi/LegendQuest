# Combat: rows, allies and difficulty

Fights move from the top-down tile grid to side-view rows inside the room you're in, the way an AQW map is the place you fight. The rules underneath stay 5e and stay in `Combat/`: initiative, d20 against AC, saves, advantage, conditions, Downed and death saves, the Soul-Link. What changes is *where* everyone stands and how they reach each other, and the class kits from `10-classes.md` replace the per-class abilities.

This file supersedes how `04` to `08` describe a fight's *layout* (tiles, sight cones in a fight, "walk into the circle"); what each fight *is* (who, why, what ends it) still comes from the script. §5 restates every special fight in row terms.

---

## 1. The picture

```text
  ┌───────────────────────────── the room's painted background ─────────────────────────────┐
  │  (support allies stand in the mid-ground here: archers on a wall, battlemages on a step) │
  │                                                                                          │
  │      back:   [ 4 ] [ 5 ] [ 6 ]                            [ 5 ] [ 6 ] [ 7 ] [ 8 ]        │
  │     front:      [ 1 ] [ 2 ] [ 3 ]                      [ 1 ] [ 2 ] [ 3 ] [ 4 ]           │
  │ ───────────── floor ─────────────────────────────────────────────────────────────────── │
  └──────────────────────────────────────────────────────────────────────────────────────────┘
   your side: up to 6 (5 party + 1 guest)                   their side: up to 8
  ┌──────────────────────────────────────────────────────────────────────────────────────────┐
  │ [portrait] HP ██████░░  Focus ████░░   [Strike][Skill 1][Skill 2][Skill 3][Ult][Potion]  │
  │ turn order: You ▸ hound ▸ Sera ▸ brute ▸ {sb} ▸ ...                       [ END TURN ]  │
  └──────────────────────────────────────────────────────────────────────────────────────────┘
```

When a fight starts, the party walks to their slots on the left third of the room, foes to theirs on the right, and the camera settles. Front rows face each other across the middle. Back rows stand a step behind and a little higher on the screen (deeper in the room), so everyone is visible and nobody overlaps a face. A big foe (the Thornmother, Varkhul, Severin, {sb} in phase 2) takes two slots.

## 2. Rows and reach

### 2.1 Slots and lanes

Each side has a **front row** and a **back row**. Slots line up in **lanes** (your slot 1 is in front of slot 4; a foe's 1 is in front of its 5), which is what "column" targets mean.

### 2.2 Who can hit whom

| Attack | Can target |
| --- | --- |
| Melee | The foes' front row. If their front row is empty, their back row is their front |
| Reach (spears, polearms, whips, Thorn Whip, Gaff) | Either row |
| Ranged and spells | Anyone |
| "Any row" skills (Backstab, Hawk Strike, Low Blow) | Anyone; the skill says so |

Your front row protects your back row from melee in exactly the same way. That's the whole of positioning: who's in front, and whether they'll hold.

### 2.3 Target shapes

| Shape | Means |
| --- | --- |
| 1 foe, front | One foe in their front row |
| 1 foe, any row / 1 foe | One foe anywhere |
| foe + neighbours | One foe and those beside it in its row |
| 1 row | Everyone in one of their rows |
| a column | The front and back slot of one lane |
| all foes / all allies | Everyone on that side |
| N random foes | Picked at random, can repeat |
| your row / your back row | Allies in your row, or your side's back row |
| 1 ally, self | As written |

### 2.4 Shift

Once a turn, free, you may **Shift**: swap places with an ally next to you or behind you, or step into an empty slot in the other row. Being Restrained or Prone stops you shifting. A Shift that takes you out of the front row while a foe in their front row has melee and an unused reaction lets that foe make one attack against you first (the opportunity attack, as now). Pull and push effects move foes between their rows; if the row they'd go to is full, they swap with whoever's there.

## 3. Turns

### 3.1 What a turn has

A **Shift**, an **action**, a **bonus action**, and a **reaction** to use before your next turn, as in BG3. Initiative is d20 + Dex, shown along the bottom, and doesn't change during the fight except where a skill says so.

Everyone has these, whatever their class:

| | Kind | Does |
| --- | --- | --- |
| The class's auto attack | A | As its table says; +10 Focus |
| Dodge | A | Attacks against you have disadvantage until your next turn |
| Help | A | Get a Downed ally up on 1 hit point, or give an ally advantage on their next attack |
| Drink Potion | B | As the potion says. You can give a potion to an ally next to you instead |
| Throw (Alchemist's Fire and the like) | A | As the item says |
| Soul-Link Strike | A | You and {sb} only, at two charges (as now) |

### 3.2 Focus

Focus replaces spell slots and per-rest resources. Everyone starts a fight on 40, gains 15 at the start of each of their turns, and 10 more for each auto attack, up to 100. Skills cost Focus and some have cooldowns, counted in your own turns. Each ultimate works once a fight. Focus is gone at the end of the fight, so there's nothing to hoard.

### 3.3 Conditions

As now (Prone, Restrained, Poisoned, Frightened, Charmed, Blinded, Stunned), plus:

| Condition | Effect |
| --- | --- |
| Marked | The next attack on it has advantage; some skills do more |
| Taunted | Its single-target attacks must target whoever taunted it |
| Burning, Bleeding | Damage at the start of its turns, as the skill says |
| Frozen (stopped time) | Skips its turns; can't be targeted by anything but area skills; ends after the stated turns |
| Concentration | As now: damage forces a Con save, DC 10 or half the damage |

### 3.4 Going down

Unchanged. A party member at 0 is Downed and rolls death saves; healing or Help gets them up. Nobody in the party dies for good: whoever's fallen gets up on 1 hit point when the fight's won. Foes at 0 die, or yield in a nonlethal fight.

### 3.5 The Soul-Link

Unchanged: when you and {sb} both hit the same foe between your turns it charges, and at two charges either of you can spend it. In rows, both of you need to be able to reach the target with what you used. Its lines are the P-05 barks. While `soul-bond-broken` is set it's off, and the sigil on the bar is grey.

## 4. Allies

"NPCs help where it makes sense" is three things.

| Kind | On the field | Can be hit | Controlled by | Example |
| --- | --- | --- | --- | --- |
| **Guest** | In a slot on your side | Yes. At 0 they fall back, out of the fight, unhurt for the story | The AI (`EnemyMind`, playing your side) | Mira at Thornbridge; Corwin's knights in the courtyard |
| **Support** | In the room's mid-ground, behind the rows | No | A fixed ability, used at the end of every round | Watch archers on the wall; Guild battlemages warding the gate |
| **Summoned** | A guest that a class skill calls in | Yes | The AI | Whistle for the Watch; Call the Order |

The party's side has six slots. Guests fill free ones. If the party's five are all there, a guest takes the sixth; if a story fight brings two guests, the party has four (the start card says who sits out, and the game picks the companion with the lowest affinity or the one the fight's `SitsOut` names).

Supports are drawn in the room's mid-ground layer with a small banner over them showing what they'll do, and a line in the turn order ("Watch archers: end of round"). Their ability can be stopped by a script (`support-off <id>`) when the story says so: the wall falls, the battlemages are spent.

### 4.1 Who helps in which fight

Every story fight, and what joins it. Guests and supports are `EncounterDefinition.Guests` and the new `EncounterDefinition.Supports`. Where an ally depends on a choice, the flag is given.

| Fight | Allies | Why |
| --- | --- | --- |
| `p-05-wraiths` | Support: Captain Odelle's crew (end of round: one wraith takes 1d6 from a boat-hook) | It's her ship |
| `a1-c1-01-gulls` | Support: Old Tomas (end of round: throws a net, one Gull Restrained 1 turn), from round 2 | He's on the dock and he's had enough of them |
| `a1-c2-01-gloomfangs` | Guest: Sera (before she joins) | She's tracking the same pack |
| `a1-c2-02-thornmother` | Support: Lodge wardens on the quarry lip (end of round: 1d8 to the Thornmother), if Tamsin was told (`a1-c2-01`) | The Lodge came |
| `a1-c2-02-hold` | Sera sits out at the crystal, shown as a support channeling (no ability); the party holds | As written |
| `a1-c3-02-raid` | Guest: Mira (as built). Support: Sergeant Brisk and two Watchmen (end of round: 1d6 to a raider) from round 3 | The Watch turns up late, as the Watch does |
| `a1-c4-02-kael` | None | A duel |
| `a1-c4-02-sappers` | Guest: Kael (as built) | As built |
| `a1-c5-01-ambush` | Support: Garrick's teamsters behind the wagons (end of round: one crossbow bolt, 1d10) | They're armed and frightened |
| `a1-c6-01-walls` | Guest: Aldric. Support: Greywatch's archers (end of round: Volley on a foe row, 1d8 each), the battlemages (start of fight: Ward on your front row) | It's his wall |
| `a1-c6-01-gate` | Guest: Aldric. Support: the archers until the gate breaks (`support-off greywatch-archers`) | |
| `a1-c6-01-varkhul` | Guest: Aldric, if alive at this point. Support: none | Varkhul is the party's to beat |
| `a1-c8-02-maleficio` | Support: Thessaly and the battlemages (end of round: Counterspell on Maleficio's next spell, phases 1 and 2 only). Guest: Brenna, if `ashfall-loyal-sb` | The Guild is there to contain the Box |
| `a1-c8-03-hungering` | Support: the battlemages (end of round: Ward 2d6 on everyone) | They're holding the circle |
| `a2-c1-02-vesk` | Support: the royal guard (end of round: 1d6 to a thrall), rounds 1 to 3 | They follow you down |
| `a2-c2-02-raid` | Guest: Sister Vale (Valencia, before she joins) from the interlude on. Second wave: thralls, or Corwin's knights (§5) | She fights for the abbey |
| `a2-c3-03-mordain` | None | |
| `a2-c4-04-survive` | Guest: Valencia (who's then burned in the cutscene) | |
| `a2-c5-02-courtyard` | Guests: Sir Corwin and two Gilded knights | You chose to help them |
| `a2-c6-01-gears` and `a2-c6-03-severin` | Guest: Valencia, if `eye-shines` (she's free). If `corwin-saved`: support, Corwin's knights at the door (end of round: Sunburst on one thrall) | |
| `a2-willing-cells` | Guest: Sister Orla, once freed (round 2 on), with Swarm only | She's had enough too |
| `a3-c2-02-stairs` | None (they're the Watch, and you're alone) | |
| `a3-c2-05-chase` | Guest: Kael if `kael-with-you`; Elara as a protected guest (if she falls, the chase fails and restarts) | |
| `a3-c3-03-brenna` | Guest: one Ashfall fighter who's already turned, if Brenna's line was persuaded once and failed | |
| `a3-c3-05-escape` | Guest: Hale, if `sq3-hale` was already set on a retry | |
| `a3-c4-02-gates` | From the liberation flags (§5.3): the night-riders, the Ashfall fighters, the Order, Hale's gate, and the Watch deserters (`deserters-sent`) | The whole point of Chapter 3 |
| `a3-c4-04-kael-duel` / `-shadow` | None | |
| `a3-c4-05-stairs` (new: one wave of wardens at the foot of the stair, played before the script `a3-c4-05`, which is its `Then`) | Guest: Brannock | It shows the player what he's about to hold back alone |
| `a3-c5-02-sovereign` | None: the time wall seals them out. Their portraits are greyed on the bar | The point of the fight |
| Random fights | A summoned ally from a class skill; in Act 3, a deserter if `deserters-sent` and you're in the Mirewood | |

## 5. Story fights in rows

How the special fights look now. Where `04` to `06` describe these fights on a grid, this table is what to build.

### 5.1 Act 1

| Fight | In rows |
| --- | --- |
| `a1-c2-02-hold` | Three waves in the quarry room. Sera stands at the crystal in the mid-ground, out of the rows (`SitsOut`), with a progress ring over her; at full, the fight ends |
| `a1-c6-01-walls` | On the wall-walk room. Climbers arrive in the foes' **back** row each round and step to the front the next, so the party chooses between hitting what's in reach and stopping what's coming. The ram beast takes two front slots |
| `a1-c6-01-varkhul` | Two slots. At half health, his visor cracks (`swap varkhul varkhul-unmasked`), he loses a quarter of his AC and gains Cleave |
| `a1-c8-02-maleficio` | Three phases, each a wave: the scholar (back row, casting, guarded by two Hollowed in front), the Box (he floats to two slots; the Box pulses a row each round), then the Hungering's first stirrings (he's Restrained by it, his attacks hit random targets on both sides) |
| `a1-c8-03-hungering` | A survival fight. Ends after 5 rounds or at a fifth of its hit points. Its hits drain Focus as well as health |

### 5.2 Act 2

| Fight | In rows |
| --- | --- |
| `a2-c2-02-raid` | Wave 1: thralls. The interlude `a2-c2-02-vale-reveals` plays at the start of round 3. If you protect Vale (options 1 or 3), wave 2 is Corwin and two Gilded knights instead of more thralls, nonlethal, yielding at a quarter; if you side with Corwin (option 2), Vale leaves the fight and wave 2 is thralls. This replaces the "third side" in `05`: there's no third side in rows |
| `a2-c4-04-survive` | Ends after 4 rounds, or at the end of the round in which anyone in the party drops below a quarter. Severin is untargetable in darkness (only the Eye-bearer's light, a new action on the bar, can reveal him for one round) |
| `a2-c6-01-gears` | The four gear assemblies are foes in the four front slots, no attacks, 30 hit points each; the Orrery's arm sweeps one row each round (shown a round ahead) |
| `a2-c6-03-severin` | Severin takes two front slots. The **Sun Mirrors** are four objects in the room's mid-ground; the Eye-bearer's bar gets **Light a Mirror** (an action). Each lit mirror lowers his Blood Shield by a quarter and brightens the tint. With all four lit, Dawn phase: he's Slowed, can't heal, and his bark plays |

### 5.3 Act 3

| Fight | In rows |
| --- | --- |
| `a3-c2-02-stairs` | Two nonlethal waves of Protectorate guards, one per landing (two rooms), yielding at a quarter |
| `a3-c2-05-chase` | A running fight: three small waves in three rooms in a row. You win a wave by beating it or by moving everyone into the right-hand exit slot (a new "Flee" action that costs the actor their turn and moves them one slot right). Elara must reach the third room's exit |
| `a3-c4-02-gates` | The siege, in three rooms: the field, the gate, the square. Each liberation changes it: `sq3-hale` skips the gate room; `ally-ashfall` makes the crown-hollowed lie down (they leave the foe rows at the start of round 2); `sq3-gilded` adds the Order as a guest in the square; `sq3-sunward` adds the night-riders as a support (end of round: Volley on the back row). **Freeze zones** are slots: each round, one slot on each side is marked violet a round ahead; whoever stands there when it fires is Frozen 2 turns. Shift out of it |
| `a3-c4-04-kael-shadow` | Kael in the front row, and two Protectorate Shadows who swap places with him whenever he's hit (he's in the back row half the time) |
| `a3-c5-02-sovereign` | Only you. Phase 1, {sb} in one front slot with their class kit. Phase 2, {sb} floats over two back slots; your front slots take turns being freeze zones; Box pulses hit your whole row; Eye beams sweep a column; the echoes of Varkhul, Maleficio and Severin each take a front slot for one attack. Phase 3, your grey sigil lights: Soul-Clash replaces Soul-Link (any round in which you and {sb} both attack, the crown cracks a quarter). `valencia-vial` is on your bar at the start of phase 2 |

## 6. Difficulty

The game should be hard to lose without being easy to sleepwalk. The current tuning (story fights won 80 to 92% by the simulator's AI) is too hard for that. With allies, Focus and class skills, it moves up.

### 6.1 Modes

Set in the options, changeable any time outside a fight.

| | Story | Balanced (default) | Tactician |
| --- | --- | --- | --- |
| Foe hit points | −35% | as tuned | +25% |
| Foe to-hit and save DCs | −2 | as tuned | +1 |
| Your Focus at the start | 60 | 40 | 30 |
| After a story fight | Full heal | Heal half of what's missing | Nothing |
| Losing | Retry at once, with a "Win this fight" button after two losses | Retry, or retry on Story for this fight only | Retry |
| Random fights | Optional (`08`'s "Story only" is on by default) | On | On, and ambushes are twice as likely |

### 6.2 Targets for the simulator

On Balanced, with the expected level (§6.3), the party the story would have (classes at the ranks they'd have reached) and the fight's allies:

| Fight kind | Target win rate |
| --- | --- |
| The tutorial (`p-05-wraiths`) | 99%+ |
| Random fights | 97%+ |
| Story fights | 92 to 97% |
| Mini-bosses (Thornmother, Vesk, Mordain, Kael's duel) | 88 to 94% |
| Bosses (Varkhul, Maleficio, Severin, Kael as the Shadow) | 82 to 90% |
| The Sovereign (alone) | 78 to 88%, with the vial; without, 70 to 80% |
| Nobody in the party goes down in more than | 40% of story fights; 70% of boss fights |

On Story, everything is 99%+. On Tactician, subtract about 15 points.

How to tune one that's off stays as the README says: hit points, then to-hit, then damage, and waves last. Two new levers come first now: **allies** (a support's damage is the gentlest way to make a fight easier without changing it) and **Focus** (a boss that drains Focus is harder in a way the player can see).

### 6.3 Levels

Levels come from experience, which the story hands out so the party is at these levels for these chapters. `--simulate` uses this table.

| Chapter | Level | | Chapter | Level | | Chapter | Level |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Prologue | 1 | | A2-C1 | 6 | | A3-C1 | 9 |
| A1-C1 | 1 | | A2-C2 | 7 | | A3-C2 | 10 |
| A1-C2 | 2 | | A2-C3 | 7 | | A3-C3 | 10 to 11 |
| A1-C3 | 2 | | A2-C4 | 8 | | A3-C4 | 11 |
| A1-C4 | 3 | | A2-C5 | 8 to 9 | | A3-C5 | 12 |
| A1-C5 | 4 | | A2-C6 | 9 | | | |
| A1-C6 | 5 | | | | | | |
| A1-C7 | 5 to 6 | | | | | | |
| A1-C8 | 6 | | | | | | |

Experience to the next level: 300 × the current level. Story fights give roughly a third of a level at the expected level; random fights a tenth; quests as their scripts say (`xp +n`, a new effect). A party above the expected level gets less (−25% a level over), so wandering about doesn't break the story's fights, and a party below gets more (+25% a level under), so skipping random fights doesn't either.

Random fights are tuned for one level below the party, so they're never the hard part.

### 6.4 Things that make it kinder without making it easier

- The bar shows the hit chance and expected damage on every skill when you hover a target (as now), and the row it can reach is highlighted when you pick a skill.
- A foe's next action is shown over it as an icon once you've seen it do it once (BG3's intent-reading, but earned).
- Retrying a fight starts it fresh with the same allies and full Focus, and costs nothing.
- The first fight that uses each new idea (rows, Shift, Focus, allies, a freeze zone) has a one-line tip, from `ExploreScreen.Tutorial.cs`.

## 7. What changes in the code

| | Change |
| --- | --- |
| `CombatEngine` | `ICombatGrid` goes. A `Formation` (two sides, two rows, slots in lanes, big foes taking two) replaces it. `Move` becomes `Shift`. Reach is §2.2. Opportunity attacks only on leaving the front row |
| `AbilityCatalog` | Becomes `SkillCatalog`, from `10-classes.md`. The shared actions (§3.1) stay |
| `ResourceCatalog` | Spell slots and per-rest resources go; `Focus` and cooldowns are on `Combatant` |
| `EncounterDefinition` | Foes get a slot instead of `x, y` (`Slot(enemy, side, row, lane)`; `Spawn` and `Npc` take a slot). New: `Supports`, `EndsAfterRounds`, `EndsAtHealthShare`, `Interlude` on waves, `FreezeZones`, `Rooms` (a fight across several rooms, §5.3) |
| `EnemyMind` | No flood fill. For each actor: score every (skill, target, shift-first or not) and take the best, with the same weights. Guests use it for your side |
| `CombatSimulator` | Rows, allies, Focus, the level table, and the new targets |
| `MapCombatGrid` | Goes. The room the fight is in provides the slot positions (`12-rooms.md` §5) |
| The combat UI | The AQW-style bar (§1): portrait, health, Focus, six buttons (auto, three skills, ultimate, potion), turn order, END TURN. Existing components (`CombatHotbar`, `InitiativeBar`, `UnitFrame`, `SoulLinkSigil`, `CombatLog`, `DamagePopups`, `CombatResult`) are kept and re-laid |
