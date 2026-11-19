# Combat: real time, in the room you're in

Fights are real time. You walk into a room, click what you want to fight (or a story scene starts the fight), and you trade blows where you stand: your auto attack swings on its own timer, and four class skills and a potion sit on the bar on keys 1 to 6, each with a Focus cost and a cooldown. Your companions fight beside you under their own AI with their own kits. The foes hold the room, pick on whoever hurts them most, telegraph their big moves, and bosses change as they're hurt.

AdventureQuest Worlds is the reference for how this *works and feels*: a steady auto-attack heartbeat, skills on short separate cooldowns, a small global cooldown, a 100-point resource with class-specific ways of filling it, stacking auras with icons and timers, plain floating numbers and a white flash on impact. Nothing of AQW's is used: no class, skill, monster, place, art, interface graphic, music or sound. Every class, skill and fight here is LegendQuest's own, from `10-classes.md` and the scripts.

Under the real-time model the character sheet is still 5e: ability scores, AC, hit points, saving throws, proficiency, advantage, conditions and the class kits. §3 says exactly how each piece maps.

This file replaces the turn-based rows design (initiative, a turn's action, bonus action, reaction and Shift, front and back rows). It supersedes how `04` to `08` describe a fight's *layout*; what each fight *is* (who, why, what ends it) still comes from the script. §6 restates every special fight in real-time terms.

The research behind these decisions, with its sources, is summarised in §11.

---

## 1. The picture

```text
 ┌─[YOU]──────────────┐        ┌─[TARGET: Hollowed Footman]──────────┐
 │ ▓ portrait  HP ████████░░  │        │ ████████████░░░░  22 / 30   AC 13      │
 │            FOCUS ██████░░  │        │ casting: OVERHEAD CHOP ▓▓▓▓░░░        │
 │ [aura][aura 4s][aura]      │        │ [Hexed 6s][Bleeding ×3 4s]            │
 └────────────────────────────┘        └───────────────────────────────────────┘
 ┌ Soul-Bound ███░  ┐
 ┌ Sera       ████  ┐   (party frames)
 ┌ Mira       ██░░  ┐
       ...the room: you, your companions and the foes, all on the floor band...
                         ┌───────── cast / swing ─────────┐
           [1 auto][2 skill][3 skill][4 skill][5 ultimate][6 potion]  [7 soul-link]
```

- **Top left**: your frame. Portrait, hit points, Focus, and your auras with time left.
- **Top centre**: the target frame. Its name, hit points, AC, its cast bar when it's winding up something, and its auras. A boss gets a wide frame across the top with its phase pips.
- **Under your frame**: the party frames, one small row each, with their hit points, Focus, a down marker and their auras.
- **Bottom centre**: the hotbar. Six slots on keys 1 to 6, each clickable, each with its icon, Focus cost, a cooldown sweep and seconds left; the global cooldown sweeps over every skill slot; a slot whose target is out of reach is tinted red; one you can't afford is greyed. Slot 7 is the Soul-Link sigil. Over the bar: your cast bar while you're casting, and the auto-attack swing indicator.
- **Over each fighter**: a nameplate with a thin health bar; foes show a telegraph icon and a filling bar while winding up a big move.
- **In the world**: floating numbers, a ring under your target, ground markers for area attacks, projectiles, and the effects in §8.

## 2. The loop

### 2.1 Starting a fight

A fight is an encounter (`EncounterCatalog`). It starts in one of three ways:

| Way | What happens |
| --- | --- |
| **A story scene** | As now: a conversation ends with `fight <id>`, and the fight starts where everyone stands. Nobody walks to slots |
| **Clicking a foe** | A hostile figure in the room (a foe of an encounter that's free to start, with a red nameplate) can be clicked: you target it and walk into reach, and the fight starts |
| **Getting too close** | An aggressive foe starts its fight when you come within its aggro radius (default 120 world px), or when an ally of its is attacked |

When it starts, every foe of the encounter in the room engages, the battle music comes in, the fight's name goes across the screen, and the room's exits close until it's over. Townsfolk step back to the far edge of the band, or leave if the fight's lethal, as now. Foes who arrive later (waves) walk in from the room's edges, or fade in where the encounter says.

### 2.2 Moving and reach

You move as you do outside fights: click the floor, or WASD and the arrows. Everyone has a position on the floor band; there are no rows or slots.

| Reach | World px (a figure is about 49 tall) | Used by |
| --- | --- | --- |
| **Melee** | 30 between the two fighters' feet | Swords, axes, fists, claws, bites |
| **Reach** | 42 | Spears, polearms, whips, Thorn Whip, Gaff |
| **Short** | 100 | Thrown things, short spells (Burning Hands, Thunderwave) |
| **Long** | 220 | Bows, crossbows, most spells |
| **Room** | anywhere in the room | Ultimates that say "all foes", party buffs |

Clicking a foe targets it; if your auto attack can't reach, you walk until it can. Pressing a skill whose target is out of reach walks you into reach and then uses it. Moving doesn't stop your auto attack, but the swing only lands if you're in reach when it comes round. Casting a spell with a cast time roots you; moving cancels it.

### 2.3 Targeting

| Input | Does |
| --- | --- |
| Left-click a foe | Target it (and walk into auto-attack reach) |
| Left-click an ally | Target them, for heals and buffs |
| Tab | Next foe, nearest first |
| T | Assist: target what your target is targeting (an ally's foe, a foe's victim) |
| Esc | Clear the target and stop auto-attacking |
| A skill with no target | Picks the nearest foe (or, for a heal, the most hurt ally; for a self skill, you) |
| Target dies | Auto-targets the nearest foe that's attacking you, else the nearest |

### 2.4 The bar

| Key | Slot | Unlocks at class rank |
| --- | --- | --- |
| 1 | Auto attack (toggle: on while you have a foe targeted) | 1 |
| 2 | Skill 1 | 1 |
| 3 | Skill 2 | 2 |
| 4 | Skill 3 | 3 |
| 5 | Ultimate | 5 |
| 6 | Potion (the bag's best healing potion; a 30 s cooldown of its own) | always |
| 7 | Soul-Link Strike, you and {sb} only (§5.3) | — |

Passives (ranks 4 and 10) act on their own and show as icons on your frame.

### 2.5 Timers

- **Auto attack**: swings every **2.0 s** (light and finesse weapons 1.8 s, heavy and two-handed 2.4 s, crossbows 2.6 s), shortened by haste. It costs nothing and builds Focus.
- **Global cooldown (GCD)**: using an **action** skill (kind A in `10-classes.md`) puts every action skill on a **1.5 s** global cooldown, shortened by haste down to 1.0 s. **Bonus** skills (kind B) are off the GCD: instant, and usable inside it, but they share a short 0.5 s lockout with each other. The auto attack ignores the GCD.
- **Cooldowns**: each skill's own, in seconds (§4.2). Haste shortens them.
- **Cast times**: most skills are instant: the clip plays and the blow lands on its impact frame (about 0.25 to 0.7 s in). A few spells have a cast time (0.8 to 1.5 s) shown on a cast bar; damage, moving or a Stun interrupts them. Ultimates have a short wind-up everyone can see.
- **A tick** of the rules is 20 ms. Everything in the engine runs in whole milliseconds on a fixed step, so the simulator and the game play the same fight from the same seed.

### 2.6 Ending a fight

As now: the fight's won when every foe is beaten (or yields, or the encounter's own ending fires: a time limit survived, a health share reached), and lost when the whole party is down. The panel comes up with experience, gold, loot and class points; the `Victory` flag is set; anyone down gets up on 1 hit point; the `Then` script plays. Lose a fight with a `Lost` script and it plays; otherwise it's the game over screen with its retries.

Out of a fight, hit points come back slowly while you walk (1% of the most you can have every 2 s) and quickly while you **rest** (R, out of combat only: kneel, 5% every second, Focus is irrelevant out of fights). Resting is cancelled by moving or by a foe engaging.

---

## 3. The 5e sheet underneath

### 3.1 What stays

Ability scores, proficiency, hit points (level and Constitution, the class modifier), AC (armour, Dexterity, shield), saving throws and their DCs, advantage and disadvantage, the conditions, resistances, the dice on every weapon and skill, and the level table. Nothing on the character sheet changes meaning.

### 3.2 Attacks

Every attack still rolls a d20 plus its to-hit against the target's AC, with advantage and disadvantage as 5e has them. What the roll *says* is new:

| Roll | Result | Shown as |
| --- | --- | --- |
| Natural 20, or a crit (§3.3) | Critical: double dice | gold number, bigger, CRITICAL |
| Meets AC | Hit | white number |
| Misses AC, but would have hit an AC without the target's Dexterity bonus | **Dodged**: the target's quickness made it miss | DODGE, and on-dodge effects fire |
| Misses AC, but would have hit without the target's shield | **Blocked** | BLOCK, the block clip, a shield sound |
| Otherwise | Missed | MISS |

So dodge and block are *read off* the 5e roll, not rolled again: a nimble character dodges more because their AC is made of Dexterity, and a shield-bearer blocks.

Saving throws are as 5e: d20 + save bonus against the DC, with half damage on a save where the skill says so.

### 3.3 Crit, haste and dodge from the stats

| Stat | From | Formula | Cap |
| --- | --- | --- | --- |
| **Crit chance** | the attack's ability modifier | 5% (a natural 20) + 2% per point of the modifier above 0 + skills and passives | 40% |
| **Crit damage** | | double the dice (5e), +50% more for skills that say "crits harder" | |
| **Haste** | Dexterity (weapon users) or the casting ability (casters) | 3% per point of the modifier above 0 + skills, auras and potions | 50% |
| **Dodge** | Dexterity | not a number: the share of AC that Dexterity provides (§3.2) | |

Haste shortens the auto-attack interval, every cooldown, the GCD (to 1.0 s at most) and cast times, by its percentage.

### 3.4 Dice that go up with level

`2d6⁺` in a skill means the dice go up at levels 5 and 9, as `10-classes.md` §1.5 says.

### 3.5 Durations

`10-classes.md`'s tables were written in turns. In real time, **a turn is 3 seconds**: "2 turns" is 6 s, "until your next turn" is 3 s, an effect "at the start of each of its turns" ticks every 3 s. That keeps every die in the tables as it was. `10-classes.md` now gives each skill's numbers in seconds directly.

### 3.6 Conditions

| Condition | Real-time effect |
| --- | --- |
| Stunned, Frozen | Can't act, move or auto-attack; interrupts a cast. Frozen also can't be hit except by area skills |
| Restrained | Rooted; attacks on it have advantage; its attacks have disadvantage; Str or Dex save every 3 s to end it where the skill says so |
| Prone (knocked down) | On the ground for 1.5 s, then gets up (the knockdown and get-up clips); attacks on it have advantage in melee |
| Blinded | Its attacks have disadvantage; attacks on it have advantage |
| Poisoned | Its attacks have disadvantage; a poison that deals damage ticks every 3 s |
| Frightened | Its attacks have disadvantage; can't move closer to the source |
| Charmed | Can't attack the charmer |
| Taunted | Must attack the taunter (overrides aggro) |
| Marked | The next hit on it has advantage; some skills do more |
| Slowed | Haste −50% (auto attacks and cooldowns slower), moves at half speed |
| Burning, Bleeding | Damage every 3 s as the skill says; can stack (§4.4) |
| Concentration | As 5e: one concentration effect at a time; damage forces a Con save (DC 10 or half the damage) or it ends |

---

## 4. Skills, Focus and auras

### 4.1 Focus and the class resource models

Every fighter has **Focus, 0 to 100**. Skills cost it. It's gone when the fight ends; a fight starts with the difficulty's starting Focus (Story 60, Balanced 40, Tactician 30). In a fight it barely refills on its own (**1 a second**); each class fills it its own way, and that's most of what makes a class feel like itself. Each class has one model:

| Model | Fills Focus | Classes |
| --- | --- | --- |
| **Momentum** | +8 for each of your hits, +14 on a crit | Blademaster, Ashwalker, Watchblade, Deckhand, Crownsworn |
| **Opportunist** | +5 a hit, +16 a crit, +12 when you dodge | Shadowstep, Lanerunner, Lodge Ranger |
| **Bulwark** | +6 when you're hit, +10 when you block, +4 for each of your hits | Warden, Ironvein Bulwark, Gilded Knight |
| **Wellspring** | +5 a second, steadily | Arcanist, Guild Battlemage, Glasswright, Hourwarden |
| **Devotion** | +3 a second, +4 for every heal you land | Lightbearer, Ember Mender, Lamplighter, Hivekeeper |
| **Wild** | +3 a second, +2 for every tick of your damage or healing over time | Wildcaller, Netcaster |
| **Pact** | +6 for each hit on a target carrying your curse (Hex), +2 otherwise; +1 a second | Hexblade |
| **Tempo** | starts every fight on 80; +6 for each of your hits | Skald, Wayfarer |
| **Blood** | +4 a second; some skills cost hit points as well | Hemomancer |

The auto attack's +10 Focus in `10-classes.md` is now the model's "+N a hit". Foes have Focus too, filled by Momentum, but a foe skill's cost is just its cooldown.

### 4.2 Converting the class tables

Every skill in `10-classes.md` keeps its target, effect and dice, and gets real-time numbers by these rules, then tuned by the simulator (§9); the tuned numbers are written back into `10-classes.md`:

| Old | Real time |
| --- | --- |
| **A** (action) | On the GCD |
| **B** (bonus action) | Off the GCD |
| **R** (reaction) | Pressed to **arm** it: it stays ready for 4 s (or until it fires) and fires when its trigger happens: Riposte answers the next melee miss on you, Shield raises AC against the next attack, Hellish Rebuke answers the next hit, Counterspell stops the next foe cast, Cover takes the next hit meant for the ally, Refract turns the next ranged attack on an ally |
| **P** (passive) | Always on, as written |
| No cooldown | GCD only |
| CD 1 | 6 s |
| CD 2 | 8 s |
| CD 3 | 12 s |
| CD 4 | 16 s |
| 1/fight (the ultimates and the summons) | 45 s, ready at the start of the fight |
| "Take another action" (Action Surge, Command) | Resets the GCD and the cooldown of the action skill used next |

Focus costs keep their numbers (a skill that cost 25 costs 25).

### 4.3 Shapes, in a room without rows

| Old shape | Real time |
| --- | --- |
| 1 foe, front | 1 foe within melee reach |
| 1 foe, any row / 1 foe | 1 foe within the skill's range |
| foe + neighbours | the target and every foe within 36 px of it |
| 1 row | every foe within 48 px of the target |
| front row of foes | every foe within 60 px in front of you |
| a column | a line from you through the target, 24 px wide, to the skill's range |
| all foes / all allies | everyone on that side in the fight |
| N random foes | N picks among engaged foes, can repeat |
| your row / your back row | allies within 60 px of you / allies further from the foes than you |
| pushed to the back row | knocked back 40 px |
| pulled to the front | pulled to the caster's melee reach |
| Shift / "move to any row" | a short dash (60 px) or a teleport the skill names |

### 4.4 Auras

A buff, debuff, damage or healing over time is an **aura**: an icon with its time left, a stack count, and who put it there. The rules:

- An aura has a duration in seconds and, if it ticks, a tick every 3 s (damage, healing or a save).
- Re-applying the same aura from the same source **refreshes** its duration; if the skill says it **stacks**, it adds a stack (up to its cap, default 5) and refreshes. The same aura from two sources is two auras.
- Damage and healing over time can't crit and can't be dodged or blocked; each tick counts as a hit for Focus models that count ticks.
- Cleansing (Cauterise, The Bell, Mend) removes the named conditions.
- Temporary hit points are an aura (a shield) with an amount that hits come off first.

### 4.5 Ranks

A class's rank, 1 to 10, unlocks its slots as `10-classes.md` §1.3 says. Class points come from fights: a random fight 15, a story fight 35, a boss 80. The rank is kept per person per class in the save.

---

## 5. Who fights how

### 5.1 Foes

Every foe has the stat block it had (`EnemyCatalog`), plus:

- **An auto attack** on its own timer: 2.5 s for most, 2.0 s for quick beasts (wolves, wraiths), 3.0 s for big ones. A Multiattack foe strikes its attacks in a short combo on the one swing.
- **Its skills**, each with a cooldown (a 5e "recharge 5–6" becomes 14 s, "recharge 6" 18 s) and a **telegraph**: a wind-up of 0.5 s or more for a normal skill and 1.0 to 1.5 s for a heavy one, shown by the wind-up clip held, a glint, the cast bar over its head, and, for area skills, a ground marker that fills and resolves the moment it's full. Stepping out of a ground marker before it's full avoids it.
- **Aggro.** Each foe keeps a threat table: damage done to it counts 1 a point, healing done to anyone it's fighting counts ½ a point (split across the foes engaged), and a taunt puts the taunter on top for its duration. Wardens, Bulwarks and Watchblades generate double threat. A foe attacks the top of its table that it can reach, and only switches when someone else passes 110% of its current target's threat (melee) or 130% (ranged). With nobody on its table yet, it goes for whoever's nearest.
- **Movement.** A melee foe walks into reach of its target; a ranged one keeps within its range and steps back from anyone in melee with it if it can.
- **Holding the room.** Foes stay in the fight area; one pulled more than 300 px from where it started walks back and heals (it "resets") if nobody's hit it for 6 s.

### 5.2 Bosses and phases

A boss (or mini-boss) has **phases**, triggered at shares of its health: each phase can change its stat block, add or swap skills, call a wave, bark, or change the room's tint. A phase change interrupts what it was doing, flashes, and puts its name across the screen. A boss's big skills have long telegraphs and ground markers, and its frame shows the phase pips. §6 lists each one.

### 5.3 The party

- **You** control one character: the leader. The rest of the party, {sb} included, fight under the party AI with their own class kits, and so do guests.
- **The party AI** (the old `EnemyMind`'s utility scoring, now run only when a fighter is free to act, about every 0.3 s): it scores every usable skill on every target by expected damage, healing and control, weighed by what the target means to the fight (its threat to the party, how hurt it is, whether it's casting something), minus the price (Focus, cooldown). Tanks taunt foes that are hitting someone else; healers heal before they hurt; everyone assists your target unless something more urgent is happening; ranged fighters keep their distance from melee foes. It has no randomness of its own.
- **Going down.** A party member at 0 hit points is **Downed**: on the ground, rolling a death save every 3 s (5e: three successes and they're stable on 0, three failures and they've fallen for the rest of the fight; a natural 20 gets them up on 1). Healing gets them up at once. **Help** (anyone can, for free) is a 2 s channel standing next to them that gets them up on 1 hit point; moving or a Stun breaks it. A foe hitting a Downed fighter counts a failed death save. Nobody in the party dies for good: everyone fallen gets up on 1 hit point when the fight's won.
- **If you go down**, the camera stays with you, the bar greys, and the fight goes on; you're up the moment someone heals or helps you. If the whole party is down, the fight's lost.
- **The Soul-Link.** When you and {sb} both land a hit on the same foe within 3 s of each other, it charges (at most once every 3 s); at two charges the sigil on slot 7 lights and either of you can spend it on the **Soul-Link Strike**: both of you strike that foe together, both weapons' damage and 1d8 radiant, a sure hit, with the duo's own clip. Its barks are the P-05 lines. While `soul-bond-broken` is set it's off and the sigil is grey.

### 5.4 Allies

| Kind | In the fight | Can be hit | Controlled by | Example |
| --- | --- | --- | --- | --- |
| **Guest** | Fights beside you | Yes. At 0 they fall back out of the fight, unhurt for the story | The party AI | Mira at Thornbridge; Kael against the sappers |
| **Support** | Stands behind the fight, off the floor band's front | No | A fixed ability on a timer (every 6 s from its start time unless it says otherwise) | Odelle's crew, Tomas's net, the Watch's crossbows |
| **Summoned** | A guest a class skill calls in | Yes | The party AI | Whistle for the Watch; Call the Order |

Supports keep their banner, now with its timer ring filling to the next time it acts. A script can stand one down (`support-off`).

| Fight | Allies |
| --- | --- |
| `p-05-wraiths` | Support: Odelle's crew (every 6 s: one wraith takes 1d6 from a boat-hook) |
| `a1-c1-01-gulls` | Support: Old Tomas (from 8 s, every 8 s: throws a net, one Gull Restrained 3 s) |
| `a1-c2-01-gloomfangs` | Support: Sera at the treeline (from 8 s, every 6 s: an arrow, 1d8) |
| `a1-c2-02-thornmother` | Support: the Lodge wardens on the quarry lip (every 6 s: 1d8 to the Thornmother), if Tamsin was told (`lodge-told`) |
| `a1-c2-02-guard-wave` | Sera sits out at the crystal (a support with no ability and a progress ring); the party holds for 45 s |
| `a1-c3-02-raid` | Guest: Mira. Support: Sergeant Brisk and the Watch (from 12 s, every 6 s: 1d6 to a raider) |
| `a1-c4-02-kael` | None. A duel |
| `a1-c4-02-sappers` | Guest: Kael |
| Later fights | As the old table in this file had them (§6 for the special ones), with "end of round" read as "every 6 s" and "from round N" as "from (N − 1) × 6 s" |

---

## 6. The fights

### 6.1 Built

| Fight | Kind | In real time |
| --- | --- | --- |
| `p-05-wraiths` | Tutorial | Three sea-wraiths on the deck. Teaches the loop a step at a time (§7.2): target, auto attack, a skill, the GCD and cooldowns, Focus, the potion, the Soul-Link when it first charges |
| `a1-c1-01-gulls` | Story, nonlethal | Bosun Grell and three thugs. Grell has a telegraphed **Hook Pull** (pulls you to him, 1.0 s wind-up) |
| `a1-c2-01-gloomfangs` | Story | Four wolves. Pack Tactics: advantage while another wolf is in melee with the same target. **Trip** telegraphed 0.6 s: Str save or knocked down |
| `a1-c2-02-thornmother` | **Boss** (the first) | Three phases. **Phase 1 (100–66%)**: Bite and Claws combo, Trip. **Phase 2 (66–33%)**: she roars, the floor around her cracks, and **Thorn Burst** comes in: a ground ring that fills over 1.8 s around her, 2d6 piercing, Dex save for half; she calls two gloomfang whelps from the brush. **Phase 3 (below 33%)**: enraged, attacks 25% faster, Thorn Burst every 10 s, and **Uprooting Charge**: she marks a line across the room for 1.5 s and charges along it, knocking down anyone in it |
| `a1-c2-02-guard-wave` | Story, survive | Hold for 45 s while Sera draws the rot out (her ring fills). Waves walk in from the room's edge at 0, 12 and 24 s, sooner if the field's clear |
| `a1-c3-02-raid` | Story | Footmen and a hexer; a second wave at 18 s ("Where's the Watch?!"). The hexer's **Ashen Bolt** has a 1.0 s cast that interrupts can stop |
| `a1-c4-02-kael` | Mini-boss, a duel, yields at 30% | Two phases. **Phase 1**: rapier and off-hand combo, **Feint** (telegraphed 0.5 s, his next hit crits). **Phase 2 (below 60%)**: **Smoke Bomb**: a cloud, he vanishes for 2 s and reappears behind his target with a guaranteed Sneak Attack |
| `a1-c4-02-sappers` | Story | Three sappers and their chief. **Blasting Charge**: the sapper lobs a charge that lands as a ground marker filling for 1.5 s, 2d6+2 fire to everyone in it, Dex save for half |

### 6.2 To come (as the scripts are built), restated for real time

| Fight | In real time |
| --- | --- |
| `a1-c6-01-walls` | On the wall-walk. Climbers arrive at the room's far edge every 8 s and need a few seconds to climb over: hit what's in reach, or stop what's coming. The ram beast is a mini-boss with a charge |
| `a1-c6-01-varkhul` | Boss. At half health his visor cracks (`swap varkhul varkhul-unmasked`): AC −3, gains **Cleave** (a 120° arc in front of him, telegraphed 1.2 s) |
| `a1-c8-02-maleficio` | Boss, three phases: the scholar casting from behind two Hollowed (interrupt his casts); the Box (it pulses a ring outward every 10 s); the Hungering's first stirrings (he's Restrained by it, his attacks hit random fighters on both sides) |
| `a1-c8-03-hungering` | Survive 60 s or bring it to a fifth of its health. Its hits drain Focus |
| `a2-c2-02-raid` | Wave 1 thralls; the interlude at 18 s; wave 2 as the choice says (Corwin's knights nonlethal, yielding at a quarter, or more thralls) |
| `a2-c4-04-survive` | Survive 24 s, or until anyone drops below a quarter. Severin can't be targeted in darkness; the Eye-bearer's **light** (a bar action) reveals him for 4 s |
| `a2-c6-01-gears` | Four gear assemblies, no attacks, 30 hit points each; the Orrery's arm sweeps a line every 8 s, marked 3 s ahead |
| `a2-c6-03-severin` | Boss. Four **Sun Mirrors** in the room: the Eye-bearer's bar gets **Light a Mirror** (stand by one and channel 2 s). Each lit mirror lowers his Blood Shield by a quarter; all four, and Dawn phase: Slowed, can't heal |
| `a3-c2-02-stairs` | Two nonlethal waves of guards, one per landing, yielding at a quarter |
| `a3-c2-05-chase` | A running fight through three rooms: win each by beating it or by getting everyone through the room's exit; Elara must reach the third room's exit |
| `a3-c4-02-gates` | The siege in three rooms. **Freeze zones**: a violet marker fills on the floor for 3 s every 10 s; whoever's in it is Frozen 6 s. Liberation flags change it as written before |
| `a3-c4-04-kael-shadow` | Kael and two Protectorate Shadows who swap places with him whenever he's hit hard |
| `a3-c5-02-sovereign` | Alone. Three phases: {sb} with their class kit; {sb} floating, freeze zones, Box pulses, Eye beams along a line, the echoes of Varkhul, Maleficio and Severin each striking once; then Soul-Clash replaces Soul-Link (whenever you and {sb} both hit within 3 s, the crown cracks a quarter). `valencia-vial` is on the bar from phase 2 |

---

## 7. The HUD, input and teaching

### 7.1 Controls

| Input | In a fight |
| --- | --- |
| Left-click foe / ally | Target (and approach a foe) |
| Left-click floor, WASD, arrows | Move |
| 1 to 6, or click the slot | Auto attack toggle, skills, ultimate, potion |
| 7, or click the sigil | Soul-Link Strike |
| Tab / T / Esc | Next foe / assist / clear target |
| L | The combat log |
| Alt | Every nameplate's numbers |
| Escape (no target) | Pause |

### 7.2 Teaching

The tutorial fight (`p-05-wraiths`) teaches one thing at a time, pausing the fight while each card is up: targeting, the auto attack, your first skill, the cooldown sweep and the GCD, Focus and your class's way of filling it, the potion, a telegraph (the first wraith wind-up), and the Soul-Link the first time it charges. Other fights give the same tips once, the first time their moment comes.

### 7.3 Options

Screen shake (full, reduced, off), hit flashes (on, off), damage numbers (on, off), and combat speed in the proof run only.

---

## 8. How it looks and sounds

Each attack, skill, hit and death is staged so it reads, and the damage lands on the frame the blow lands:

- **Timing.** Every move is a clip with an **impact** event (or a **release** for something thrown or shot). The engine schedules the damage at that event's time, so retiming a clip retimes the fight. A light swing is about 4 to 6 frames of anticipation, 2 frames of contact and 12 of recovery; a heavy skill 10 to 14, 3 and 18 to 22, with a held coil before the blow and an overshoot after it. Recovery can be cancelled by your next action once the impact has happened.
- **Impact.** On a hit: hit-stop (the attacker and the target alone freeze for 3 frames on a light hit, up to 10 on a big one, 12 on a kill, never the whole world), a 2-frame white flash on the target (4 on a crit), a squash and a knockback of a few pixels, sparks at the contact point in the damage's colour, the weapon's or the school's sound, and the number. Screen shake only on heavy skills, crits, kills, the player taking a big hit and boss slams; never on autos.
- **Motion.** Arcs come from rotation down the arm; the fastest frames stretch the weapon and forearm (a smear) and leave a fading ribbon trail; skirts and capes follow on springs.
- **Spells.** A charge glow on the hand during the wind-up, the projectile or beam in the school's colour, a burst at the target, the target tinted in the element for a moment.
- **Numbers.** White for your damage, gold and bigger with a bounce for crits, red for damage you take, green with a "+" for healing, grey words for MISS, DODGE and BLOCK, smaller and tinted for ticks over time; at most six alive over one fighter, the rest summed.
- **Foes** get the same care: a telegraph pose and glint, the attack, a flinch, and a death (flash, knockback, collapse, a held beat, then a fade in motes). Creatures drawn as sheets get it by squash, lean, tint and tip-over on their frames.
- **Music.** The zone's track plays until a fight starts; the battle track (or the boss's) comes in with a stinger and plays until the fight ends; the victory or defeat jingle plays once, then the zone track comes back from where it left off.
- **Sound.** Each weapon kind has its own swing and impact (light blade, heavy blade, axe, blunt, polearm, bow, crossbow, unarmed and claws and bites), each school its own cast and impact (fire, frost, lightning, radiant, necrotic, force, psychic, nature, thunder, poison), plus crits, blocks, dodges, heals, auras, telegraph warnings and the bar's clicks, each with a few variations played at slightly different pitches.

## 9. Difficulty and tuning

### 9.1 Modes

| | Story | Balanced (default) | Tactician |
| --- | --- | --- | --- |
| Foe hit points | −35% | as tuned | +25% |
| Foe to-hit and save DCs | −2 | as tuned | +1 |
| Starting Focus | 60 | 40 | 30 |
| Foe telegraphs | 1.5× as long | as tuned | as tuned |
| After a story fight | Full heal | Heal half of what's missing | Nothing |
| Losing | Retry, and "Win this fight" after two losses | Retry, or retry on Story for this fight | Retry |

### 9.2 Targets for the simulator

`--simulate` fights each encounter with the AI driving everyone, you included, in real time, on a fixed step, from a seed. The party AI is given a human handicap so it doesn't over-report: it reacts 0.4 s late and uses the best skill 85% of the time.

| Fight kind | Win rate (Balanced) | Length (Balanced) | Most downed may be |
| --- | --- | --- | --- |
| Tutorial | 99%+ | 20 to 50 s | 40% |
| Random fights | 97%+ | 15 to 45 s | 40% |
| Story fights | 92 to 97% | 25 to 75 s | 40% |
| Mini-bosses | 88 to 94% | 45 to 120 s | 70% |
| Bosses | 82 to 90% | 75 to 180 s | 70% |
| The Sovereign | 78 to 88% with the vial | 120 to 240 s | 70% |

On Story every fight is won 99% of the time or more; Tactician is about 15 points under Balanced.

**No class is useless.** `--simulate classes` puts each of the 24 classes in your place in every built fight, at the rank and level the story would have, and reports win rate, fight length, damage a second, healing a second and threat held. Every class must win its share of the fights within 5 points of the median class, and each must lead its role (damage, healing, holding threat or control) in at least one way. The numbers it settled on are in `10-classes.md`, and the last report is in the README.

How to tune a fight that's off, in this order: supports (their dice and start time); foe hit points (fight length); foe to-hit (how often people go down); telegraph lengths (how fair big hits are); damage; waves and time limits last.

### 9.3 Levels

As before: levels come from experience, at the levels the table below sets for each chapter, and `--simulate` uses it.

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

Experience to the next level: 300 × the current level. Story fights give about a third of a level at the expected level, random fights a tenth; −25% a level above the expected level, +25% a level below.

## 10. The code

| | |
| --- | --- |
| `Combat/` (rules and data, draws nothing) | Dice, attack maths, saves and stats as before; `EnemyCatalog`, `EncounterCatalog` (now with telegraphed skills, auto-attack timers, phases, waves and supports in seconds); `SkillCatalog` (every class's kit, by skill id `<class>-<skill>`), `ClassCatalog` with all 24 classes and the Hourwarden |
| `Combat/Realtime/` | `Battle`: the fight, stepped in fixed 20 ms ticks from a seed, taking commands (target, move, use a skill, potion, Soul-Link) and returning timed events; fighters, auras, threat, the party and foe brains, encounter scripting (waves, supports, phases, endings) |
| `Combat/CombatSimulator` | `--simulate <id\|all\|classes> [runs] [difficulty]` on the real-time engine |
| `Screens/ExploreScreen.Combat*` | The fight as a layer on the room: steps the engine, plays its events as cues on each fighter, takes your input, draws the HUD |
| `Components/` | Player, target, party and boss frames; the hotbar with sweeps; aura rows; cast and swing bars; nameplates; floating numbers; the result panel |
| `Characters/Parts/ClipLibrary`, `Characters/WalkerSprite` | Clips with events, per-key easing, blend times, hit-stop, smears and trails, springs |
| `tools/rig/clips.py` | Every combat clip: autos per weapon kind, a clip per class skill, casts, telegraphs, blocks, dodges, hit reactions, knockdowns, deaths, victories, the idle |
| `Audio/AudioManager`, `tools/generate_audio.py` | The score and the combat sounds (§8) |

## 11. Where this came from

The research notes (scratch, not in the repo) checked AQW's combat against its official handbook and stats guide, its wiki, its design notes and two community mechanics guides, each claim against two sources where possible. Settled: a 2 s auto attack (1.5 s on quick classes), separate skill cooldowns of about 2 to 16 s, a global cooldown of about 1.5 s on skills but not the auto attack, 100 mana for everyone with class-specific ways of filling it and almost no regeneration in combat, haste as a percentage cut to cooldowns capped at 50%, damage over time that can't crit or miss, stacking auras with icons, a taunt of 4 to 6 s, monsters respawning in seconds, and a white flash on impact. Not settled, and decided here on our own: how foes pick targets (the threat table in §5.1), what happens when you go down (§5.3), and whether music changes in a fight (§8: it does here). The feel numbers in §8 come from fighting-game frame data, Sakurai's notes on hit-stop, the trauma model of screen shake, and the animation principles; they're starting points the clips and the simulator tune.
