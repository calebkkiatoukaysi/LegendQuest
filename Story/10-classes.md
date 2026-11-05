# Classes

How classes work after this change: a mix of Baldur's Gate 3 and AdventureQuest Worlds. Under the hood it's still 5e, as now: ability scores, d20 rolls against AC, saving throws, advantage, and proficiency. On top of that sits AQW's class system. You collect classes from trainers and from people you help, equip one at a time, and each class ranks up from 1 to 10 as you fight with it, unlocking its skills as it goes.

The class *names and kits here are LegendQuest's own*. The AQW wiki was the reference for how a class roster is shaped (base classes from a trainer, advanced ones from quests, people and merchants; ranks; a handful of skills and two passives). No AQW class, name or skill is copied.

Combat rules the skills rely on (rows, Focus, actions) are in `11-combat.md`. Notation for the trainer scenes is as in `04-script-prologue-act1.md`.

---

## 1. The rules

### 1.1 Level and class are separate

| | Belongs to | What it gives |
| --- | --- | --- |
| **Level** (1 to 12 by the end of Act 3) | The character | Hit points, proficiency bonus, ability score increases at 4, 8 and 12, and the skill dice tiers (§1.5). Earned by experience, as now |
| **Class** | The character, one equipped at a time | The auto attack, four skills, two passives, which armour and weapons they're trained in, and a hit point modifier. Each class has its own **rank**, 1 to 10 |

Your creation-screen class is your first class, at rank 1. {sb}'s is theirs. Each companion joins with their own class (Sera a Wildcaller, Mira a Lightbearer, Kael a Shadowstep, Brannock a Warden, Valencia a Hemomancer) already at the rank in §6.

### 1.2 Switching

Anyone can switch to any class they know, any time outside a fight, from the **Classes** tab on the character sheet (Tab or C). Nothing is lost: ranks stay with the class. Switching to a class whose armour you're wearing but aren't trained in gives the 5e penalties (disadvantage on Strength and Dexterity attacks and checks), and the tab warns you before you confirm.

### 1.3 Ranks

The equipped class earns **class points** (CP) from every fight it's equipped for, and from some quests. Classes not equipped earn nothing, as in AQW.

| Rank | CP total | Unlocks |
| --- | --- | --- |
| 1 | 0 | Auto attack and Skill 1 |
| 2 | 40 | Skill 2 |
| 3 | 120 | Skill 3 |
| 4 | 240 | Passive 1 |
| 5 | 400 | Ultimate |
| 6 | 600 | +1 to hit with the class's skills |
| 7 | 850 | +5 Focus at the start of each turn |
| 8 | 1,150 | +1 to the class's save DCs |
| 9 | 1,500 | +10% to the class's healing and damage |
| 10 | 2,000 | Passive 2 |

| Source | CP |
| --- | --- |
| A random fight | 15 |
| A story fight | 35 |
| A boss | 80 |
| A class trial (§4) | 60 |
| Some quests (the scripts say `cp +n`) | as written |

That puts a class equipped for most of an act at rank 7 to 10 by its end, and a class picked up late at rank 3 to 5 within a chapter. Ranks 1 to 5 come quickly on purpose: the skills are the fun, and a new class should feel complete within an evening.

### 1.4 Hit points

Hit points come from level and Constitution, not class: 10 + Con at level 1, then 7 + Con a level. The class adds a modifier on top:

| Modifier | Classes |
| --- | --- |
| +20% | Warden, Ironvein Bulwark, Watchblade |
| +10% | Blademaster, Gilded Knight, Ashwalker, Deckhand, Crownsworn |
| 0 | Everyone else |
| −10% | Arcanist, Guild Battlemage, Glasswright, Hemomancer |

### 1.5 Skill dice

A skill's dice go up with *level*, not rank: one more die at level 5, and another at 9. Skills written `2d6⁺` follow this (2d6, 3d6 at 5, 4d6 at 9). Skills without the mark don't.

### 1.6 What a skill line means

Each class table lists: the rank it unlocks at; the kind (**A** action, **B** bonus action, **R** reaction, **P** passive); Focus cost; cooldown in turns (**CD**); the target (`11-combat.md` §2.3 has the target shapes); and the effect. "1/fight" means once per fight.

---

## 2. The base classes

These are the eight the game already has, under the same story names (the SRD name is still the code id). Master Imra Vell teaches all of them at the Proving Hall in Questeria (§4.1), so anyone can learn any of them from Act 1 on.

### 2.1 Blademaster (`fighter`)

Front-line damage. Any armour, any weapon, shields.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Strike | A | — | — | 1 foe, front | Weapon dice + Str. +10 Focus |
| 1 | Cleave | A | 20 | 2 | foe + neighbours, front | Weapon dice to each |
| 2 | Second Wind | B | 20 | 4 | self | Heal 1d10 + level |
| 3 | Riposte | B | 20 | 3 | self | Until your next turn, a melee attack that misses you is answered with a Strike |
| 4 | Fighting Style | P | | | | +2 to hit with melee |
| 5 | Action Surge | B | 60 | 1/fight | self | Take another action this turn |
| 10 | Indomitable | P | | | | Once a fight, reroll a failed save |

### 2.2 Shadowstep (`rogue`)

Single-target damage, reaches the back row. Light armour; finesse and light weapons, hand crossbows.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Strike | A | — | — | 1 foe, front | Weapon dice + Dex. **Sneak Attack**: +1d6⁺ if the target has been hit by an ally this round, or is Marked or Prone. +10 Focus |
| 1 | Backstab | A | 20 | 1 | 1 foe, any row | Weapon dice + Dex, with Sneak Attack |
| 2 | Smoke Step | B | 20 | 3 | self | Shift for free; attacks against you have disadvantage until your next turn |
| 3 | Poisoned Edge | B | 20 | 3 | self | Your next hit Poisons (1d4 a turn for 3 turns; disadvantage on attacks) |
| 4 | Cunning | P | | | | +3 initiative; Shift is a bonus action instead of using your one free Shift |
| 5 | Shadow Dance | A | 60 | 1/fight | 3 random foes | A Backstab on each, each with Sneak Attack |
| 10 | Evasion | P | | | | Area skills do half to you, none on a passed save |

### 2.3 Lightbearer (`cleric`)

Healer. Medium armour, shields, simple weapons.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Sacred Flame | A | — | — | 1 foe | Dex save or 1d8⁺ radiant. +10 Focus |
| 1 | Cure Wounds | A | 25 | 1 | 1 ally | Heal 1d8⁺ + Wis. Gets a Downed ally up |
| 2 | Bless | B | 25 | 4 | 3 allies | +1d4 to attacks and saves for 3 turns |
| 3 | Guiding Bolt | A | 30 | 2 | 1 foe | 4d6 radiant; the next attack on it has advantage |
| 4 | Disciple of Life | P | | | | Your heals heal 3 more |
| 5 | Dawnlight | A | 60 | 1/fight | all foes, all allies | Foes: 3d10 radiant, Con save for half (undead save with disadvantage). Allies: heal 2d6 |
| 10 | Blessed Healer | P | | | | When you heal someone else, you heal 1d6 |

### 2.4 Arcanist (`wizard`)

Area damage. No armour; staffs and daggers.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Fire Bolt | A | — | — | 1 foe | 1d10⁺ fire. +10 Focus |
| 1 | Magic Missile | A | 25 | 1 | up to 3 foes | Three darts of 1d4 + 1 force; they always hit |
| 2 | Burning Hands | A | 30 | 2 | 1 row | 3d6 fire, Dex save for half |
| 3 | Shield | R | 20 | 2 | self | +5 AC against one attack, and until your next turn |
| 4 | Arcane Recovery | P | | | | +10 Focus at the start of each turn |
| 5 | Fireball | A | 70 | 1/fight | all foes | 8d6 fire, Dex save for half |
| 10 | Empowered | P | | | | Once a turn, add Int to one damage roll |

### 2.5 Warden (`paladin`)

Tank and burst. Heavy armour, shields, martial weapons.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Strike | A | — | — | 1 foe, front | Weapon dice + Str. +10 Focus |
| 1 | Divine Smite | B | 25 | 1 | after a hit | +2d8 radiant (+1d8 more against undead and fiends) |
| 2 | Lay on Hands | A | 20 | — | 1 ally | Heal from a pool of 5 × level a fight. Can cure Poison |
| 3 | Raise Shield | B | 20 | 3 | self | +2 AC and every foe's next single-target attack must target you |
| 4 | Aura of Protection | P | | | | Allies in your row add your Cha to saves |
| 5 | Unbroken Oath | A | 60 | 1/fight | all allies | Temporary hit points 2d10 + level; immune to Frightened and Charmed for 3 turns |
| 10 | Radiant Strikes | P | | | | Melee hits do +1d8 radiant |

### 2.6 Wildcaller (`druid`)

Control and healing. Light and medium non-metal armour, shields of wood, simple weapons.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Thorn Whip | A | — | — | 1 foe, any row | 1d6⁺ piercing; a back-row foe is pulled to the front. +10 Focus |
| 1 | Entangle | A | 30 | 3 | 1 row | Str save or Restrained 2 turns |
| 2 | Healing Word | B | 20 | 1 | 1 ally | Heal 1d4⁺ + Wis. Gets a Downed ally up |
| 3 | Moonbeam | A | 30 | 3 | a column | 2d10 radiant now and at the start of each of its turns, 3 turns (concentration) |
| 4 | Natural Recovery | P | | | | After a fight, the party heals 10% |
| 5 | Wild Shape: Bear | A | 60 | 1/fight | self | A bear for 4 turns: temporary hit points 3 × level + 10, Claws 2d6⁺, Maul hits foe + neighbours |
| 10 | Nature's Ward | P | | | | Immune to Poisoned |

### 2.7 Hexblade (`warlock`)

Ranged damage and curses. Light armour, simple weapons.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Eldritch Blast | A | — | — | 1 foe | 1d10 force; a second beam at level 5, a third at 9. +10 Focus |
| 1 | Hex | B | 20 | — | 1 foe | Your hits on it do +1d6 necrotic (concentration) |
| 2 | Hellish Rebuke | R | 25 | 2 | who hit you | 2d10 fire, Dex save for half |
| 3 | Armour of Shadows | B | 20 | 4 | self | Temporary hit points 1d8 + level + Cha |
| 4 | Agonising Blast | P | | | | Add Cha to Eldritch Blast's damage |
| 5 | Void Maw | A | 60 | 1/fight | all foes | Blinded 2 turns and 2d6 cold at the start of each of their turns |
| 10 | Devil's Sight | P | | | | Ignore Blinded and darkness |

### 2.8 Skald (`bard`)

Support. Light armour, simple weapons, rapiers.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Vicious Mockery | A | — | — | 1 foe | Wis save or 1d4⁺ psychic and disadvantage on its next attack. +10 Focus |
| 1 | Inspiration | B | 20 | 1 | 1 ally | +1d6 to their next attack, save or check |
| 2 | Healing Word | B | 20 | 1 | 1 ally | Heal 1d4⁺ + Cha. Gets a Downed ally up |
| 3 | Thunderwave | A | 30 | 2 | front row | 2d8 thunder, Con save for half; a failed save pushes them to the back row |
| 4 | Song of Rest | P | | | | After a fight, everyone heals your level; +1 to every check you're not trained in |
| 5 | Battle Hymn | A | 60 | 1/fight | all allies | +1d6 to hit and damage for 3 turns |
| 10 | Countercharm | P | | | | Allies have advantage on saves against Frightened and Charmed |

---

## 3. The advanced classes

Sixteen classes from people and trainers across the story, and one after it. Each is tied to who teaches it: their skills are what that person would know. They're rarely stronger than a base class at the same rank, but they do things the base classes can't.

### 3.1 Act 1

#### Watchblade (`watchblade`) · Captain Hale · the Watch House, Questeria

Defender and controller. Medium armour, shields, swords, cudgels.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Cudgel Blow | A | — | — | 1 foe, front | Weapon dice + Str; a hit on a foe already hit this round knocks it Prone. +10 Focus |
| 1 | Hold the Line | B | 20 | 3 | self | Taunt the front row for 1 turn; +2 AC |
| 2 | Arrest | A | 25 | 3 | 1 foe, front | Str save or Restrained 2 turns. In a brawl or nonlethal fight it takes the foe out at a quarter health |
| 3 | Whistle for the Watch | B | 30 | 1/fight | — | A Watchman joins your side for the fight (an ally, `11-combat.md` §4) |
| 4 | Twenty Years on the Beat | P | | | | +2 to saves; advantage on Insight checks in conversations |
| 5 | Shield Wall | A | 60 | 1/fight | your rows | Everyone on your side takes half damage until your next turn |
| 10 | Keeper of the Peace | P | | | | Foes you've Restrained or knocked Prone take +1d6 from your allies |

#### Netcaster (`netcaster`) · Old Tomas · Saltwind Docks

Support and control from the back row. Light armour, spears, nets, hooks.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Gaff | A | — | — | 1 foe, any row | 1d6⁺ piercing. +10 Focus |
| 1 | Cast the Net | A | 25 | 3 | 1 row | Dex save or Restrained 2 turns |
| 2 | Mend | B | 20 | 1 | 1 ally | Heal 1d6⁺ + Wis; ends Restrained |
| 3 | Haul In | A | 25 | 2 | 1 foe, back row | Pull it to the front row, Prone |
| 4 | Weather Eye | P | | | | +2 initiative for the whole party |
| 5 | High Tide | A | 60 | 1/fight | all foes | 3d8 cold, Str save or Prone; the party's next attacks have advantage |
| 10 | Knots Hold | P | | | | Your Restrained lasts a turn longer |

#### Deckhand (`deckhand`) · Captain Odelle · the *Gull's Wager*, Saltwind Docks

Skirmisher. Light armour, cutlasses, hooks, pistols (a one-shot handgun with 1d10 piercing).

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Cutlass | A | — | — | 1 foe, front | Weapon dice + Dex. +10 Focus |
| 1 | Belaying Pin | B | 20 | 3 | 1 foe, front | Con save or Stunned 1 turn |
| 2 | Rum Ration | B | 20 | 3 | 1 ally | Heal 2d4⁺ and +1d4 to their next attack |
| 3 | Swing Across | A | 30 | 2 | 1 foe, any row | Weapon dice + 1d6, then swap places with an ally |
| 4 | Sea Legs | P | | | | Immune to Prone; +1 AC |
| 5 | Broadside | A | 60 | 1/fight | all foes | The *Gull's Wager* fires from off-screen: 4d10 fire, Dex save for half. Outdoors only; indoors it's Grapeshot, 3d8 to the front row |
| 10 | Captain's Share | P | | | | +20% gold from fights |

#### Lodge Ranger (`lodge-ranger`) · Ranger Tamsin · the Rangers' Lodge, the Mirewood

Ranged damage with a hawk. Light and medium armour, bows, short swords.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Shot | A | — | — | 1 foe | Weapon dice + Dex. +10 Focus |
| 1 | Hunter's Mark | B | 20 | — | 1 foe | Marked: your hits do +1d6 (concentration) |
| 2 | Hawk Strike | B | 20 | 2 | 1 foe, any row | Your hawk: 1d6⁺, and the target can't take reactions |
| 3 | Volley | A | 30 | 3 | 1 row | Weapon dice to each, Dex save for half |
| 4 | Count the Ground | P | | | | Can't be ambushed: the party always acts first in an ambush |
| 5 | Rain of Arrows | A | 60 | 1/fight | all foes | 4d8 piercing, Dex save for half; Marked foes take it twice |
| 10 | Old Growth | P | | | | +2 to hit Marked foes |

#### Guild Battlemage (`battlemage`) · Archmage Thessaly · the Arcanum Spire

Battlefield caster. Light armour, staffs.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Force Bolt | A | — | — | 1 foe | 1d8⁺ force, pushes a front-row foe to the back. +10 Focus |
| 1 | Ward | B | 20 | 2 | 1 ally | Temporary hit points 1d10 + Int + level |
| 2 | Counterspell | R | 30 | 3 | a foe's spell | It fails |
| 3 | Chain Lightning | A | 35 | 3 | foe + 2 more | 3d8 lightning to the first, half to the others |
| 4 | Spire Training | P | | | | Concentration can't be broken by damage |
| 5 | Spire Lance | A | 70 | 1/fight | a column | 10d6 force; ignores resistance |
| 10 | Archmage's Patience | P | | | | Your first skill each fight costs no Focus |

#### Ashwalker (`ashwalker`) · Brenna · Ashfall, Doomward

Bruiser who fights in ash. Medium armour, greatswords and axes.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Ember Edge | A | — | — | 1 foe, front | Weapon dice + Str + 1d4 fire. +10 Focus |
| 1 | Ash Cloud | B | 25 | 3 | front row of foes | Blinded 1 turn, Con save ends it |
| 2 | Endure | B | 20 | 3 | self | Resistance to all damage until your next turn |
| 3 | Burn the Fields | A | 30 | 3 | 1 row | 2d6 fire now and 1d6 at the start of each of their turns, 3 turns |
| 4 | Doomward Born | P | | | | Resistance to fire; advantage on saves against Hollowing and necrotic |
| 5 | Last Harvest | A | 60 | 1/fight | foe + neighbours | Weapon dice × 3 + 3d6 fire; heal half the damage you do |
| 10 | Nothing Left to Lose | P | | | | Below half health, +2 to hit and +1d6 damage |

#### Ironvein Bulwark (`bulwark`) · Brannock · taught at camp

The best defender in the game. Heavy armour, shields, hammers and axes.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Shield Bash | A | — | — | 1 foe, front | 1d6⁺ + Str and push to the back row. +10 Focus |
| 1 | Cover | R | 15 | 1 | 1 ally | Take a hit meant for them |
| 2 | Hold | B | 25 | 3 | your back row | Foes can't target your back row until your next turn |
| 3 | Stone Skin | B | 25 | 4 | self | Resistance to slashing, piercing and bludgeoning, 2 turns |
| 4 | Forty-One Names | P | | | | +1 AC for each ally who has gone Downed this fight, up to +3 |
| 5 | Nobody Left Behind | A | 60 | 1/fight | all allies | Every Downed ally gets up on a quarter health; you take Cover for all of them until your next turn |
| 10 | Staying Behind | P | | | | Once a fight, drop to 1 hit point instead of 0 |

#### Ember Mender (`ember-mender`) · Mira · taught at camp

Healer who cauterises. Light armour, staffs, censers.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Cinder | A | — | — | 1 foe | 1d8⁺ fire. +10 Focus |
| 1 | Cauterise | A | 25 | 1 | 1 ally | Heal 2d6⁺ + Int; ends Bleeding and Poisoned. Gets a Downed ally up |
| 2 | Read the Wound | B | 15 | 2 | 1 foe | See its resistances and next action; your party has +2 to hit it this round |
| 3 | Warm Hands | B | 25 | 3 | your row | Heal 1d8 + Int each |
| 4 | Index of Remedies | P | | | | Potions you use heal 50% more |
| 5 | Rekindle | A | 60 | 1/fight | all allies | Heal 4d8 each and +2 to hit for 2 turns |
| 10 | Notes in the Margin | P | | | | Your heals on someone below a quarter health are doubled |

#### Lanerunner (`lanerunner`) · Kael · taught at camp

Thief. Light armour, daggers, twin blades.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Twin Cuts | A | — | — | 1 foe, front | Two hits of weapon dice; Sneak Attack (as the Shadowstep's) on one. +10 Focus |
| 1 | Pick Pocket | B | 15 | 1/foe | 1 foe, front | Take gold (a fifth of the foe's loot) or a potion it carries |
| 2 | Rooftops | B | 20 | 2 | self | Move to any row; the next attack against you misses |
| 3 | Low Blow | A | 25 | 2 | 1 foe, any row | Weapon dice + 2d6; Con save or it loses its bonus action |
| 4 | Lane Rules | P | | | | Advantage on attacks against foes who attacked an ally last turn |
| 5 | Everyone's Pocket | A | 60 | 1/fight | all foes | Pick Pocket every foe, then a Twin Cuts on the richest |
| 10 | Name on the Door | P | | | | +1 to every roll for each ally still standing |

#### Wayfarer (`wayfarer`) · Pedlar Mags · on the road (random encounter)

Jack of all trades. Light armour, any simple weapon.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Walking Stick | A | — | — | 1 foe, front | 1d6⁺ + Str or Dex. +10 Focus |
| 1 | Throw Anything | A | 15 | 1 | 1 foe | Throw an item from the bag for its effect, at double strength |
| 2 | Duchess Kicks | B | 25 | 3 | 1 foe, front | 2d6 bludgeoning, push to the back row. (She's off-screen, but she's very much involved) |
| 3 | Rummage | B | 20 | 3 | self | Draw a random potion |
| 4 | Haggle | P | | | | Shops are a fifth cheaper |
| 5 | Lucky Stone | A | 60 | 1/fight | all allies | Each ally's next roll is a natural 20 |
| 10 | Seen It All | P | | | | +2 to every save |

### 3.2 Act 2

#### Gilded Knight (`gilded-knight`) · Sir Corwin (or Davin in Act 3) · the Gilded Chapterhouse

Sun paladin. Heavy armour, shields, longswords.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Morning Strike | A | — | — | 1 foe, front | Weapon dice + Str + 1d4 radiant. +10 Focus |
| 1 | Sunburst | B | 25 | 2 | 1 foe | 2d8⁺ radiant; vampires and undead are Blinded 1 turn |
| 2 | Vigil | B | 20 | 3 | 1 ally | They gain your AC bonus from your shield until your next turn |
| 3 | Call the Order | B | 35 | 1/fight | — | A Gilded knight joins your side for the fight |
| 4 | Where the Dark Gathers | P | | | | +2 to hit and +1d6 against undead and vampires |
| 5 | Gilded Dawn | A | 60 | 1/fight | all foes | 6d8 radiant, Con save for half; vampires take it with disadvantage and can't heal for 2 turns |
| 10 | Sorry Somewhere Useful | P | | | | Your heals and shields on others are 25% stronger |

#### Hemomancer (`hemomancer`) · Valencia · taught at camp

Blood mage. Light armour, daggers. Some skills cost hit points as well as Focus.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Blood Lash | A | — | — | 1 foe, any row | 1d10⁺ necrotic; heal 2. +10 Focus |
| 1 | Drain | A | 25 | 2 | 1 foe | 2d8⁺ necrotic; heal what you deal |
| 2 | Blood Shield | B | 20 + 5 HP | 3 | 1 ally | Temporary hit points 2d8 + level |
| 3 | Given Freely | B | 10 HP | 3 | 1 ally | Heal them 2d10 + Cha. (You can't drop yourself below 1) |
| 4 | The Old Enemy | P | | | | In daylight (outdoors, before `eclipse-stage` 3), −2 to hit; at night or underground, +2 |
| 5 | Crimson Tide | A | 60 | 1/fight | all foes | 4d10 necrotic, Con save for half; every ally heals a quarter of the total |
| 10 | Forty Years of Manners | P | | | | Charmed and Frightened have no effect on you |

Valencia joins as a Hemomancer, rank 4. Its daylight passive is her one in `05-script-act2.md`, now a passive any Hemomancer has.

#### Hivekeeper (`hivekeeper`) · Sister Orla · Saint Lumen's Abbey

Healer with a swarm. Light armour, staffs.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Swarm | A | — | — | 1 foe | 1d6⁺ piercing, and 1 more at the start of its next turn. +10 Focus |
| 1 | Honey Salve | A | 20 | 1 | 1 ally | Heal 1d8⁺ + Wis, and 2 more at the start of each of their turns for 3 turns |
| 2 | Smoke Pot | B | 20 | 3 | 1 row | Foes there have disadvantage on attacks for 1 turn |
| 3 | Angry Hive | A | 30 | 3 | 1 row | 2d6⁺ piercing to each, and Poisoned on a failed Con save |
| 4 | Thirty Summers | P | | | | Your healing over time can't be cleansed and ticks twice on Downed allies |
| 5 | The Whole Hive | A | 60 | 1/fight | all foes, all allies | Foes: 3d8 piercing. Allies: Honey Salve |
| 10 | They Know Me | P | | | | Beasts and swarms won't target you unless you attacked them |

#### Glasswright (`glasswright`) · Lady Isadora's journal · found in the Moonlit Conservatory

Illusionist of light. No armour, wands. The only class learned from a book: whoever reads the journal at camp learns it.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Prism Ray | A | — | — | 1 foe | 1d8⁺ radiant; a second ray at half to a random foe. +10 Focus |
| 1 | Mirror Image | B | 25 | 4 | self | Three images: each attack against you has a one-in-two chance to hit an image instead, until all three are gone |
| 2 | Refract | R | 20 | 2 | a ranged attack or spell aimed at an ally | It hits a random foe instead |
| 3 | Glasshouse | A | 30 | 3 | 1 row | Dex save or Blinded 1 turn; 2d6 radiant either way |
| 4 | Night-Flowers | P | | | | Your skills do +2 in dark places |
| 5 | Given in Love | A | 60 | 1/fight | all allies | Every ally's next hit does +4d6 radiant and heals them half of it |
| 10 | Choose It Yourself | P | | | | Immune to Charmed; once a fight, cancel one foe's control effect on an ally |

#### Lamplighter (`lamplighter`) · Mother Anselle · Saint Lumen's Abbey

Protector-healer. Medium armour, maces, lanterns.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Lantern Swing | A | — | — | 1 foe, front | 1d6⁺ + Wis radiant. +10 Focus |
| 1 | Sanctuary | B | 20 | 3 | 1 ally | Foes must pass a Wis save to target them, 2 turns |
| 2 | Vespers | A | 25 | 2 | your back row | Heal 1d8⁺ + Wis each |
| 3 | Reveal | B | 15 | 2 | all foes | Ends invisibility and Mirror Images; the next attack on each has advantage |
| 4 | Everyone Who Knocks | P | | | | The first time an ally would be Downed each fight, they stay on 1 instead |
| 5 | The Bell | A | 60 | 1/fight | all allies | Heal 3d8, end every condition |
| 10 | Bees Guard the Honey | P | | | | Allies under Sanctuary heal 1d6 when attacked |

### 3.3 Act 3

#### Crownsworn (`crownsworn`) · Elara · the resistance camp

Commander and duellist. Medium armour, longswords, rapiers.

| Rank | Skill | Kind | Focus | CD | Target | Effect |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | Riposte Strike | A | — | — | 1 foe, front | Weapon dice + Dex; +1 AC until your next turn. +10 Focus |
| 1 | Command | B | 25 | 2 | 1 ally | They take an action now, out of turn |
| 2 | Told Rudely | B | 15 | 3 | 1 foe | It's Marked and Taunted (must target you) for 1 turn |
| 3 | Rally | A | 30 | 3 | all allies | Heal 1d8⁺ each; Downed allies get up on 1 |
| 4 | No Procedure | P | | | | +2 initiative for the party; you always act in the first round |
| 5 | My Father's Sword | A | 60 | 1/fight | 1 foe | Weapon dice × 4 + 4d8 radiant; every ally gets a free Strike on it |
| 10 | Somebody Allowed to Say No | P | | | | Once a fight, cancel a foe's ultimate |

### 3.4 After Act 3

#### Hourwarden (`hourwarden`) · Archmage Thessaly · the Sunspire ruins

Time mage. No armour, staffs. Unlocked after the coronation (`a3-c5-06`), for Act 4: it's listed here so its trainer scene can be written with Act 3's ending, and so the systems are built for it. Its skills: Rewind (an ally's hit points go back to what they were at the start of the last round), Haste (an extra action next turn), Stop (a foe skips its next turn), Echo Strike (your last skill again, at half), and Ultimate, Bleed the Clock (every foe loses its next turn). Ranks and numbers are set with Act 4.

---

## 4. Where classes come from

### 4.1 The trainers table

| Class | Who teaches | Where | From | Needs | Cost |
| --- | --- | --- | --- | --- | --- |
| The eight base classes | Master Imra Vell | The Proving Hall, Questeria | A1-C1 | — | 50 gold each |
| Watchblade | Captain Hale | The Watch House | after A1-C3 | `a1-c3` | 150 gold and a trial |
| Netcaster | Old Tomas | Saltwind Docks | after A1-C1 | `a1-c1-01` | 100 gold |
| Deckhand | Captain Odelle | The *Gull's Wager* (docked until Act 2) | after A1-C1 | `a1-c1-01` | 120 gold |
| Lodge Ranger | Ranger Tamsin | The Rangers' Lodge | after A1-C2 | `a1-c3` | 150 gold |
| Guild Battlemage | Archmage Thessaly | The Arcanum Spire | after A1-C7-02 | `a1-c7-02` | 300 gold |
| Ashwalker | Brenna | Ashfall | after A1-C7-05 | `a1-c7-05`, `!a3-c2-04` or `ally-ashfall` | a trial |
| Ironvein Bulwark | Brannock | camp | after Forty Letters | `brannock-letters-done`, `affinity-brannock>=40` | free |
| Ember Mender | Mira | camp | after the Ember Index | `mira-index-done`, `affinity-mira>=40` | free |
| Lanerunner | Kael | camp | after A Name on a Door | `kael-gannet-done`, `affinity-kael>=40` | free |
| Wayfarer | Pedlar Mags | `enc-mags` | Act 1 on | — | 400 gold |
| Gilded Knight | Sir Corwin / Davin | The Gilded Chapterhouse | after A2-C6, or SQ3-Gilded | `corwin-saved` or `sq3-gilded` | a vigil (trial) |
| Hemomancer | Valencia | camp | after `valencia-talk-2` | `affinity-valencia>=40` | free |
| Hivekeeper | Sister Orla | Saint Lumen's | after The Willing | `valencia-orla-saved` | free |
| Glasswright | Isadora's journal | read at camp | after A2-C3-03 | `isadora-journal` | free |
| Lamplighter | Mother Anselle | Saint Lumen's | after A2-C2-01 | `a2-c2-01` | 200 gold |
| Crownsworn | Elara | the resistance camp | after A3-C3-01 | `affinity-elara>=30` | free |
| Hourwarden | Thessaly | the Sunspire ruins | after A3-C5-06 | `a3-c5-06` | — |

Each class is learned **per person**: a trainer teaches whoever you choose in the party, and the price is per person. Companions can learn any class you can, and switch the same way.

Missing a class is never a dead end except for the four that depend on a choice going well (Hivekeeper needs Orla alive; Ironvein, Ember Mender and Lanerunner need the companion close enough). That's on purpose: helping people is how you get their best.

### 4.2 The trainer screen

`train <trainer-id>` is a new effect: it opens the **Class Trainer** screen for that trainer, the way `shop` opens a shop. It shows the trainer's classes; for each one, its role, its seven skills (the locked ones greyed with the rank that opens them), its cost, and for each party member whether they know it. A class whose `Needs` aren't met shows the need in plain words ("Deliver Brannock's letters"), from a `NeedText` field in the trainer's catalog entry, never a flag id. Learning one puts it in that person's Classes tab at rank 1.

### 4.3 Trials

A trial is a short fight that teaches the class's idea. It's fought with the class equipped at rank 1 by whoever's learning it, alone or with the party as the trial says. Losing a trial costs nothing; you can try again straight away. Each gives 60 CP when won.

| Trial | Class | Fight | Shape |
| --- | --- | --- | --- |
| `trial-watch` | Watchblade | Two Watch sergeants in the Watch House yard, nonlethal | You alone. Teaches Hold the Line and Arrest |
| `trial-ash` | Ashwalker | Three Hollowed in the Ashfall ash-pits | You and Brenna (an ally). Teaches Ash Cloud |
| `trial-vigil` | Gilded Knight | The chapterhouse at night: two wights out of the crypt | You alone. Teaches Sunburst |

---

## 5. The trainer scenes

Every trainer and teacher has a scene, marked `trainer` in its header. The first time, the whole scene plays and its last node opens the trainer screen. After that, talking to them plays only the last node (the compiler records it as the script's `RepeatFrom`). Written to `02-writing-guide.md`.

### 5.1 The Proving Hall

#### script train-imra · map `proving-hall` · with `imra-vell` · marker event · trainer

```
! place imra-vell at hall-floor · hold imra-vell hand_front practice-sword
IMRA VELL: The Proving Hall. Fifty gold a discipline, and I teach the same eight to everyone. Lords, dockhands, the occasional duck.
IMRA VELL: Pick one. If it doesn't suit you, come back and pick another. Nobody ever learned a sword by thinking about it.  ~ train imra-vell
```

### 5.2 Act 1 teachers

#### script train-hale · map `watch-house` · with `hale` · needs `a1-c3` · marker event · trainer

```
! place hale at desk · hold hale hand_front curfew-roster
HALE: You want to learn the Watch's way. Most people learn it from the wrong end.
HALE: Hundred and fifty for the training, and you'll go two rounds in the yard with my sergeants. They won't hold back. They don't know how.
?
  1. When do we start?  ~ train hale  → end
  2. Another time.  → end
```

The trainer screen starts `trial-watch` when the class is bought. Its after script:

#### script trial-watch-after

```
HALE: Not bad. You put Brisk on his back, and he's going to tell everyone it was the cobbles.  ~ cp +60
```

#### script train-tomas · map `docks` · with `tomas` · needs `a1-c1-01` · marker event · trainer

```
! place tomas at net-rack · anim tomas mend
TOMAS: Nets. My father's knots, and his father's. Most of it's knowing where a fish wants to go and getting there first.
TOMAS: Works on people too. Mostly. A hundred and I'll show you how to throw one without wearing it.  ~ train tomas
```

#### script train-odelle · map `docks` · with `odelle` · needs `a1-c1-01` · unless `act2-done` · marker event · trainer

```
! place odelle at gangway · anim odelle lean
ODELLE: My crew fight the way the sea taught them. Dirty, fast, and nobody falls overboard.
ODELLE: Hundred and twenty. And if you call for the *Wager*'s guns in a fight, you'd better be standing on dry land and well back.  ~ train odelle
```

Odelle sails at the start of Act 2. After that, Deckhand is taught by her bosun, Ruck, at the same berth, with the line `RUCK: Captain left me the teaching. Same price. Less swearing, I'm afraid.`

#### script train-tamsin · map `rangers-lodge` · with `tamsin` · needs `a1-c3` · marker event · trainer

```
! place tamsin at map-table · place lodge-hawk on tamsin shoulder
TAMSIN: The Lodge trains wardens. Bow, hawk, and a count of everything you can see. Sera says you're not hopeless.
{if with:sera} SERA: I said not entirely.
TAMSIN: The hawk's your own. She'll pick you, not the other way round. A hundred and fifty to the Lodge.  ~ train tamsin
```

#### script train-thessaly · map `arcanum-spire` · with `thessaly` · needs `a1-c7-02` · marker event · trainer

```
! place thessaly at lectern
THESSALY: The Guild doesn't teach outsiders. The Guild also doesn't usually let outsiders into the late stacks, and here you are.
THESSALY: Three hundred. You'll learn to stand still while everything's on fire, which is most of it.  ~ train thessaly
```

#### script train-brenna · map `ashfall` · with `brenna` · needs `a1-c7-05` · marker event · trainer

```
! place brenna at ash-pit-edge · show hollowed ×3 at ash-pit
BRENNA: Ashwalkers fight the way Doomward lived. In the smoke. Glad of it.
BRENNA: No gold. Go down in the pit with me and three of them, and come up again.
?
  1. Let's go.  ~ train brenna  → end
  2. Not today.  → end
```

#### script trial-ash-after

```
BRENNA: You came up. Most do, the second time.  ~ cp +60
```

### 5.3 The companions

Each sets its class as learnable for the whole party, at no cost, by opening the trainer screen with `train <companion>`.

#### script brannock-teach · with `brannock` · needs `brannock-letters-done`, `affinity-brannock>=40` · marker event · trainer

```
! place brannock at camp-edge · hold brannock arm_back brannock-shield
BRANNOCK: You've been watching me. With the shield. You think I don't notice.
BRANNOCK: Fine. Stand here. Feet under your shoulders. Now don't move for an hour.  ~ train brannock
```

#### script mira-teach · with `mira` · needs `mira-index-done`, `affinity-mira>=40` · marker event · trainer

```
! place mira at fire · hold mira hand_front notes
MIRA: The Index was a book about taking people apart. Half of it, it turns out, is how to put them back together.
MIRA: I've copied that half out. Don't read the margins. The margins are me arguing with myself.  ~ train mira
```

#### script kael-teach · with `kael` · needs `kael-gannet-done`, `affinity-kael>=40` · marker event · trainer

```
! place kael at cart · anim kael flip-coin
KAEL: Lesson one. That was your coin. Lesson two, you get it back by asking nicely, and I'll explain the rest.
KAEL: Free. Don't make that face. I'm only teaching you so I've got someone to blame.  ~ train kael
```

#### script valencia-teach · with `valencia` · needs `valencia-talk-2-done`, `affinity-valencia>=40` · marker event · trainer

```
! tint night · place valencia at camp-edge
VALENCIA: Blood magic isn't taking. Everyone thinks it is. It's spending, and the first thing you spend is your own.
VALENCIA: I'll teach you on two conditions. You never use it on someone who didn't offer. And you tell me when you're tired.  ~ train valencia
```

#### script elara-teach · with `elara` · needs `a3-c3-01`, `affinity-elara>=30` · marker event · trainer

```
! place elara at stump-table · hold elara hand_back leon-sword
ELARA: My father's fencing master taught me the court style. I've since found out it's mostly about looking good.
ELARA: The rest of it I learned in this forest. I'll teach you both. Then you can choose which one you're ashamed of.  ~ train elara
```

### 5.4 Act 2 teachers

#### script train-anselle · map `saint-lumen` · with `anselle` · needs `a2-c2-01` · marker event · trainer

```
! place anselle at chapel-door · hold anselle hand_front lantern · light anselle lantern on
ANSELLE: We take in anyone who knocks. Some of them knock with swords, so the sisters learned to answer the door properly.
ANSELLE: Two hundred to the abbey roof fund. It leaks over the hives, and the bees take it personally.  ~ train anselle
```

#### script train-orla · map `saint-lumen` · with `orla` · needs `valencia-orla-saved` · marker event · trainer

```
! place orla at hives · hold orla hand_front smoke-pot · light orla smoke-pot on
ORLA: You got me out of a cell. I've nothing to give you but bees, so you're getting bees.
ORLA: Don't flap. They hate flapping. Nobody likes being flapped at.  ~ train orla
```

#### script train-isadora · camp · needs `isadora-journal` · marker event · plays when you rest with the journal unread

```
! anim you read · hold you hand_front isadora-journal
NARRATOR: Most of Isadora's journal is about night-flowers. The rest is about light: how to bend it, how to hold it, how to hide in it.
NARRATOR: Someone could learn a great deal from it in a few evenings by the fire.  ~ train isadora
```

#### script train-corwin · map `gilded-chapterhouse` · needs `a2-c6-04` · marker event · trainer

```
{if corwin-saved} ! place corwin at chapter-altar
{if corwin-saved} CORWIN: The Order teaches anyone who'll keep a vigil. One night in the crypt, no lamp. If you're still here at dawn, you're one of us.  ~ train corwin  → end
{if sq3-gilded} ! place davin at chapter-altar · hold davin hand_front corwin-sword
{if sq3-gilded} DAVIN: Sir Corwin would have done this properly. I'm going to try. One night in the crypt, and no lamp.  ~ train corwin  → end
NARRATOR: The chapterhouse doors are barred. Nobody answers.
```

With neither `corwin-saved` nor `sq3-gilded`, the scene only says the doors are barred, and the marker doesn't show (the marker's condition is the same `AnyOf`).

#### script trial-vigil-after

```
{if corwin-saved} CORWIN: Dawn. You're one of us. My condolences.  ~ cp +60  → end
DAVIN: Dawn. You stayed. That's all he ever asked anyone.  ~ cp +60
```

### 5.5 On the road

The Wayfarer is taught by Pedlar Mags, as the second option in her road scene (`08-random-encounters.md`, `enc-mags`).

---

## 6. Companions and classes

| Companion | Joins as | Rank on joining | Their own teaching |
| --- | --- | --- | --- |
| {sb} | Their creation class | 1 | None. {sb} learns from trainers like you |
| Sera | Wildcaller | 3 | (none; the Lodge Ranger is Tamsin's) |
| Mira | Lightbearer | 3 | Ember Mender |
| Kael | Shadowstep | 3 | Lanerunner |
| Brannock | Warden | 4 | Ironvein Bulwark |
| Valencia | Hemomancer | 4 | Hemomancer |
| Elara (Act 3 guest, then party) | Crownsworn | 5 | Crownsworn |

A companion fights best in their own class, and some story fights expect it: in a scripted fight where a companion's class matters (Sera's Entangle at the crystal, Brannock's Cover on the stairs), the fight uses their joining class whatever they've equipped, and says so on the fight's start card.

## 7. Scripts and conditions

- `class:x` in a script means the equipped class. `knows:x` (new condition `KnowsClass`) means they have it at any rank. The existing `class:bard` checks stay as they are.
- `cp +n` (new effect `ClassPoints`) adds to the equipped class.
- `train <trainer>` (new effect `Train`) opens the trainer screen.
- New tables: `ClassCatalog` gets the sixteen advanced classes and Hourwarden; `TrainerCatalog` (trainer id, classes, price, needs, need text, trial fight); `SkillCatalog` replaces `AbilityCatalog`'s per-class kits with the tables above. `ResourceCatalog`'s spell slots and per-rest resources go; Focus and cooldowns replace them (`11-combat.md` §3). Second Wind, Lay on Hands and the rest stay as skills.
- New cast: `imra-vell` (Master Imra Vell, a Stoneborn woman in her sixties with a broken nose and a teacher's patience; generic pool body, unique head and a sleeveless practice jerkin), `ruck` (Odelle's bosun), `lodge-hawk` (a creature, `03-art-overhaul.md` §9 flier rig).
- New map: `proving-hall`, an AQW-style room in the Lantern Market (`12-rooms.md` §6).
