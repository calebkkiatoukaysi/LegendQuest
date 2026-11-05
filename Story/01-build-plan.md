# Build plan

This is the file to work from. It says what to build, in what order, how the script files turn into C#, and how to know each step is done. The other files are the content; this one is the engineering.

Read it with the repo open. Where it names a class, method or file in the repo, that's what the repo had when this was written; if it's moved, follow the repo and note the change here.

---

## 0. Ground rules

1. **The script files are the source of truth for every line.** `04` to `08` are compiled into C# by a tool (§4). Nobody edits the generated C#. A wrong line is fixed in the `.md` and recompiled.
2. **Ids don't change.** Scripts, flags, items, fights, cast and maps keep the ids the files give them. Act 1 Chapters 1 to 4 and `Companions.cs` are already in the game: their ids, flags and the constants classes that name them (`Act1Chapter4.KaelRecruited` and the rest) stay exactly as they are.
3. **One milestone at a time, in order** (§11). Each ends with its checks passing and a commit. Don't start the next with the last one's checks red.
4. **Everything playable at every step.** The whole story must run with placeholder rooms and Legacy characters from M8 on; the rig comes after the content, behind a switch, so the content never waits for art.
5. **Every PNG from a script.** The repo's rule stands: generated or cut, seeded, listed in `ASSETS.md` and `Content/Content.mgcb`, and `check_assets.py` passes.
6. **Run the four checks before every commit:**

   ```text
   dotnet build
   python story/tools/lint_dialogue.py          # 0 errors (warnings are for a human)
   python story/tools/build_registry.py --check # 0 problems
   python tools/check_assets.py
   ```

   From M1 on, add `python tools/story/compile_scripts.py --check` (the generated C# matches the `.md`), and from M2 on, the game's own start-up validation (`DialogueCatalog`) with no errors.

## 1. What's in the repo (the parts this plan touches)

| Area | What's there | Where |
| --- | --- | --- |
| Conversations | `DialogueScript` (`Id`, `With`, `Needs`, `Unless`, `Marker`, `Nodes`), `DialogueNode` (`Id`, `Speaker`, `Line`, `Direction`, `Conditions`, `Effects`, `Options`, `Next`, `Seconds`, `Timeout`, `Painting`, `Board`, `Sound`), `DialogueOption` (`Text`, `Conditions`, `Locked`, `Flirt`, `Effects`, `Check`, `Next`, `OnSuccess`, `OnFailure`), `DialogueCheck` (`Label`, `Ability`, `Dc`, `Advantage`) | `Dialogue/` |
| Flow | A node with no options goes to `Next`, or the next node if `Next` is null. A node whose conditions fail is skipped the same way. An option with no `Next` goes to the node after the question. A check goes to `OnSuccess` or `OnFailure` | `DialogueNode`, `DialogueOption` remarks |
| Conditions | `Flag`, `NotFlag`, `MeterAtLeast`, `MeterBelow`, `Voice`, `InParty`, `Class` | `DialogueCondition` |
| Effects | `SetFlag`, `AddMeter`, `SetMeter`, `Gold`, `GiveItem`, `TakeItem`, `Recruit`, `Objective`, `Voice`, `Travel`, `Shop`, `Fight`, `Rest` | `DialogueEffect` |
| Speakers | Origin ids, `Cast` ids, map NPCs, and `Speakers.Narrator`, `.Card`, `.Chapter` | `Dialogue/Speakers.cs`, `Story/Cast.cs` |
| Tables checked at start-up | `Meters`, `KeyItems`, `Cast`, `Origins`, `ShopCatalog`, `EncounterCatalog`, `Paintings` | `Story/`, `Combat/` |
| Fights | `EncounterDefinition` (`Foes`, `Waves` of `EncounterWave(round, foes, bark)`, `Guests`, `SitsOut`, `YieldsAt`, `Nonlethal`, `Victory`, `Then`, `Lost`, `Boss`, `Music`, `LootTable`). Foes are `Npc(enemy, npcId)` or `Spawn(enemy, x, y)` | `Combat/` |
| Companion talks | Talking to a follower plays the first of their scripts that can start | `Dialogue/Scripts/Companions.cs`, `DialogueCatalog` |
| Banters | Barks by id, placed by maps | `Story/Banters.cs` |
| Maps | Text files: layers as characters, objects with settings. **Replaced by AQW-style rooms** (`12-rooms.md`) in M3 | `Content/Maps/`, README "Maps" |
| Art | 32x48 ten-column sheets, `Paperdoll`, `WalkerSprite`, `WalkerShape`, `CharacterPalettes`; generators in `tools/` | README "The walker", "Regenerating the art" |
| Proof run | `--proof` plays scripted beats and saves shots; `--simulate` tunes fights | README |

## 2. The files in `story/`

| File | What | Who reads it when |
| --- | --- | --- |
| `README.md` | What's here | First |
| `01-build-plan.md` | This | Always open |
| `02-writing-guide.md` | How every line is written | Before changing or adding any line |
| `03-art-overhaul.md` | The rig, items, faces, tiers, detail level | M11 to M14 |
| `03b-world-art.md` | Each place's brief, buildings, props, effects, UI, music | M3, M6 to M8, M13 |
| `04-script-prologue-act1.md` | Prologue and Act 1 | M1, M6 |
| `05-script-act2.md` | Act 2 | M7 |
| `06-script-act3.md` | Act 3 | M8 |
| `07-companions.md` | Camp talks, personal quests, romances, {sb}'s letters, Elara, banters | M6 to M9 |
| `08-random-encounters.md` | The encounter system and every encounter | M10 |
| `10-classes.md` | Classes (AQW-style swapping and ranks, on 5e), every class's skills, the trainers and their scenes | M5 |
| `11-combat.md` | Side-view row combat, allies in fights, difficulty modes and targets, the level curve | M4 |
| `12-rooms.md` | AQW-style rooms: the format, exits, door and shop badges, painting detail, every zone's rooms | M3, M13 |
| `09-registry.md` | Generated: every id the scripts use | Whenever you need to know who sets or reads something |
| `tools/lint_dialogue.py` | The writing guide's mechanical rules | Every commit |
| `tools/build_registry.py` | Writes `09` and checks every flag read is set | Every commit |

`COMPANIONS.md` at the repo root stays: it's who the companions are. Where it and `07` differ on a line, `07` wins; on a fact, `COMPANIONS.md` wins.

In the repo, `story/` is the `Story/` folder, beside the C# that was already there (`Cast.cs`, `Banters.cs`, `Paintings.cs` and the rest). Windows doesn't mind the case, so the commands in this plan run as written. Both Python tools set their output to UTF-8, because a Windows console's default code page has no `→` or `·`.

## 3. Model changes

All additions; nothing existing changes meaning.

### 3.1 Conditions

| New `ConditionKind` | Notation | Holds when |
| --- | --- | --- |
| `NotInParty` | `!with:kael` | They aren't in the party |
| `MeterCompare` | `affinity-kael-sb>affinity-kael`, `>=` | One meter against another. `Id` is "a>b" or "a>=b"; parse it once at load |
| `Act` | `act:2` | The current act is this. Act is a new meter, `act`, set by the act cards (`act =1` on `p-01`'s card, `act =2` on `a2-c1-01`'s, `act =3` on `a3-c1-01`'s) |
| `HasItem` | a bare key-item id: `sb-herring`, `!valencia-vial` | The bag has it. A bare id is a flag unless `KeyItems.Exists(id)` |
| `AnyOf` | `[needs a \| b \| c]` | Any of its children holds. Add `Children` to `DialogueCondition` (make it a class, or keep the struct and store `AnyOf` groups in a side table by an id; either is fine) |
| (existing) `MeterAtLeast`, `MeterBelow` | `bond>=70`, `bond<70`; `>n` is `AtLeast n+1`, `<=n` is `Below n+1` | |

### 3.2 Effects

| New `EffectKind` | Notation | Does |
| --- | --- | --- |
| `ClearFlag` | `clear f` | |
| `Leave` | `leave valencia` | Out of the party for a while: kept in the save as "away", not at camp, not selectable. Her talks don't start |
| `Rejoin` | `rejoin valencia` | Back from away, into the party even if it's full (party size may go to five for a story stretch; the combat setup takes all five) |
| `MeterBump` on `Rejoin` | | `rejoin valencia` also applies `affinity-valencia +5` if `mc-claimed-valencia` (A2-C5-01). Write it as a second effect in the compiled script, not as magic in `Rejoin` |

Everything else in the scripts is an existing effect: `set`, `+n`/`-n` on a meter (`AddMeter`), `=n` (`SetMeter`), `gold`, `give`, `take`, `join` (`Recruit`), `objective`, `travel map spawn`, `rest`, `voice`, `shop`, `fight`. `fame +2` is `AddMeter fame`.

### 3.3 Nodes

- `Expression` (string, nullable): one of the twelve in `03-art-overhaul.md` §5.2. Validated at start-up. Legacy art ignores it.
- `Cues` (`IReadOnlyList<StageCue>`): §5. Run before the node shows.
- `Painting` stays and keeps working; the compiler fills it when a `backdrop` cue names a painting that exists in `Paintings` and Legacy is on.

### 3.4 Fights, classes and rooms

The combat model changes are in `11-combat.md` §7 (rows replace the grid; `Supports`, `EndsAfterRounds`, `EndsAtHealthShare`, `Interlude`, `FreezeZones`, multi-room fights). The class model is in `10-classes.md` §7. The room model is in `12-rooms.md` §4 and §8. In short, the new effects and conditions those bring:

| New | Notation | Does |
| --- | --- | --- |
| Effect `Train` | `train <trainer>` | Opens the class trainer screen |
| Effect `ClassPoints` | `cp +n` | Class points to the equipped class |
| Effect `Experience` | `xp +n` | Experience to the party |
| Effect `SupportOff` | `support-off <id>` | Ends a support ally's turns (inside a fight's interlude or cue) |
| Effects `FightAdd`, `FightGuest` | `fight-add <enemy> <n>`, `fight-guest <id>` | Inside an interlude only |
| Condition `KnowsClass` | `knows:x` | Has learned class x |
| (existing) `Class` | `class:x` | Now means the *equipped* class |

### 3.5 Tables

Every new id in the scripts goes in its table. `09-registry.md` lists them all; the lists to add are:

- **Cast**: every speaker in the registry's Speakers table who isn't an origin or already in `Cast`. Name, then look. Until M11 the look is a `CharacterLook` like the existing ones; the art spec's §7.4 description is the brief for it.
- **KeyItems**: every item in the registry's Items table. Description: the art spec §6.4's Look column, rewritten as one plain sentence in the existing items' style.
- **Meters**: `act` (0 to 3). No other new meters: the scripts use only the existing ones.
- **TrainerCatalog** and the classes: `10-classes.md` §4 and §7.
- **ShopCatalog**: `mags` (Pedlar Mags: rope, salt, liniment, lanterns, travel food; prices up a fifth in Act 3) and `wager-crate` (southern goods: spices, oilskins, a fishing knife, at a third off).
- **EncounterCatalog**: every fight id in the registry's Fights table.
- **Paintings**: Legacy versions of the backdrops that already exist (`pier-memory`, `red-moon`, `questeria-dawn`). The rest are M13.

## 4. From notation to C#

### 4.1 The compiler

Write `tools/story/compile_scripts.py`. It reads `story/04` to `08`, and for every fenced block under a `#### script <id>`, `#### banter <id>` or `#### enc-<id>` header (and not a ` ```text ` block), writes a `DialogueScript` into `Dialogue/Scripts/Generated/<file>.g.cs`, one static class per file (`StoryPrologueAct1`, `StoryAct2`, `StoryAct3`, `StoryCompanions`, `StoryEncounters`), and a `GeneratedScripts.All` list that `DialogueCatalog` registers. Banters go to `Generated/Banters.g.cs` in `Banters.cs`'s format. Encounter headers go to `Generated/RandomEncounters.g.cs` (§7).

For a script id that already exists in a hand-written class (Act 1 Chapters 1 to 4, `Companions.cs`), the generated one replaces it: delete the hand-written `DialogueScript` field, keep the class and its constants. `compile_scripts.py --check` fails if a hand-written script and a generated one share an id.

It's a line-oriented parser. Its rules are the notation table in `04-script-prologue-act1.md`, plus these, which make it exact:

1. **Headers.** A header is `#### script <id>` then any number of ` · ` fields. The compiler reads these fields and ignores the rest as notes:
   - ``map `x` `` (where the scene happens; used by the spawn check, not by `DialogueScript`)
   - ``with `x` `` → `With`
   - ``needs `a`, `b` `` → `Needs` (a backticked `!x` goes to `Unless`)
   - ``unless `a` `` → `Unless`
   - `marker quest` / `marker event` → `Marker`

   Anything else in a header is prose for whoever places the trigger on the map ("plays on arrival", "trigger at the throne", "plays when you rest"). The compiler writes all of them to `story/09-registry.md`'s triggers list (add a section to `build_registry.py`) so none get lost. **If a header's `needs` holds prose the compiler can't read** ("two of `x`, `y`, `z`", "(or `valencia-found` after the trail)"), it fails with the script id. Fix it in the `.md`: either rewrite it as plain flags, or move the logic into a gate flag set by code, and say which in the header. There are about six of these; they're listed by the first run.
2. **Lines.** A line is a node unless it starts with `!` (cues for the next node), `?` (the node above has options), a number and a dot (an option), or `—` (a reading aid, ignored).
3. **Labels.** `#label` at the start of a node line is its `Id`. On a cue line, it labels the node the cues belong to.
4. **Conditions.** `{if a, b}` at the start of a node or cue line. On a cue line it guards that cue line only.
5. **Speaker.** Upper-case words up to the first `(`, `[` or `:`. `YOU` is the player's origin id (the one the existing scripts use for you), `SB` is `Origins.SoulBound`, `NARRATOR`, `CARD` and `CHAPTER` are the `Speakers` constants, `VOICE(x)` is speaker `voice:<x's id>` (new; the dialogue screen draws `x`'s portrait tinted, with no name), `ECHO-X` is cast id `echo-x`. Everything else is lower-cased with spaces as hyphens and must be a cast or origin id: `RED GULL` is `red-gull`. `SISTER VALE` is cast `sister-vale`, a second name for Valencia's look.
6. **Expression and direction.** `(expr)` → `Expression`. `[text]` → `Direction`. `CORRA [letter]` is Corra, direction "letter".
7. **Tail.** After the text, two spaces then `~` starts effects (split on `·`), two spaces then `→` starts the jump. `→ end` is `DialogueScript.End`. `[CALLBACK x]` is stripped (and counted by the lint).
8. **Options.** `  1. text  ~ effects  → label`. Before the text: `♥` sets `Flirt`; `[needs c]` sets `Conditions`; `[locked c]` sets `Conditions` and `Locked`; `[PERSUADE 13 · adv a, b]` or `[CHECK stealth 12 · adv a]` sets `Check` (label upper-cased; DC defaults to 12; `adv` conditions are `Advantage`) and the jump must be `→ pass x · fail y`. Ability by label: PERSUADE Charisma, INSIGHT Wisdom, STEALTH Dexterity, ATHLETICS Strength, ARCANA Intelligence, INTIMIDATE Charisma.
9. **Timed questions.** `? timer 3 → burned` sets `Seconds = 3`, `Timeout = "burned"` on the question's node.
10. **Jump targets.** Every `→ x` must name a label in the same script. If the target's conditions fail at run time, the game's existing skip rule carries on to the next node that passes. That's the "router" pattern (§4.3), and it's deliberate.
11. **Prose inside blocks.** Some blocks end a branch with a line of explanation outside the code; that's outside the fence and ignored.
12. **Tokens.** `{name}`, `{sb}`, `{sb_them}`, `{sb_their}`, and the capitalised forms, as the game already has. `{sb_them}self` needs nothing new.

### 4.2 Speakers to add

The registry's Speakers table, minus the origins (`YOU`, `SB`, the companions) and the `Speakers` constants, is the cast to add. Today that's about seventy. Give each a `Cast` row in M3 to M5, as their act comes up.

### 4.3 A worked example

A3-C5-04's router, in notation:

```text
#hand {if bond>=70} NARRATOR: Your hands catch.  → high
{if bond>=40} NARRATOR: Your fingers touch {sb}'s.  → mid
NARRATOR: Your fingers close on nothing.  → defiant
#high ! anim you catch-hand soulbound · sigil both gold · hold you hand_front sb-herring
NARRATOR: The herring is between your palms, cord and all.   [CALLBACK herring]
SB (sad) [smiling]: There you are.
```

compiles to:

```csharp
new() { Id = "hand", Speaker = Speakers.Narrator, Line = "Your hands catch.", Next = "high",
        Conditions = new[] { DialogueCondition.AtLeast(Meters.Bond, 70) } },
new() { Speaker = Speakers.Narrator, Line = "Your fingers touch {sb}'s.", Next = "mid",
        Conditions = new[] { DialogueCondition.AtLeast(Meters.Bond, 40) } },
new() { Speaker = Speakers.Narrator, Line = "Your fingers close on nothing.", Next = "defiant" },
new()
{
    Id = "high", Speaker = Speakers.Narrator, Line = "The herring is between your palms, cord and all.",
    Cues = new[]
    {
        StageCue.Anim("you", "catch-hand", partner: "soulbound"),
        StageCue.Sigil("both", "gold"),
        StageCue.Hold("you", "hand_front", "sb-herring"),
    },
},
new() { Speaker = Origins.SoulBound, Expression = "sad", Direction = "smiling", Line = "There you are." },
```

At bond 55, `hand` fails and is skipped, the next node passes and jumps to `mid`; at bond 20 both are skipped and the third plays. Every router in the scripts works this way.

An option with a check:

```text
  2. [PERSUADE 13 · adv voice:witty] Left them in my other coat.  → pass waved · fail fight
```

```csharp
new()
{
    Text = "Left them in my other coat.",
    Check = new DialogueCheck { Label = "PERSUADE", Dc = 13, Advantage = new[] { DialogueCondition.Speaks(Voice.Witty) } },
    OnSuccess = "waved", OnFailure = "fight",
},
```

### 4.4 The built chapters

Act 1 Chapters 1 to 4 are rewritten in `04` with the same ids and flags (the file says which lines are "as built"). Where `04` says "as built", the compiler must find the script already present and leave it; mark those headers `· as-built`, and the compiler skips them. Before deleting a hand-written script, diff its flags against the generated one's: `compile_scripts.py --diff a1-c3-02` prints both.

## 5. Stage cues

### 5.1 The model

```csharp
public sealed record StageCue(string Verb, IReadOnlyList<string> Args, IReadOnlyList<DialogueCondition> Conditions);
```

The compiler writes one per `·`-separated cue. `CutsceneDirector` (new, in `Screens/` beside the dialogue screen) runs a node's cues before its line shows.

### 5.2 Timing

- Each actor has its own queue: a cue waits for the actor's previous cue to finish. Cues on different actors run together. `anim soulbound strike · anim crane die` plays both at once; to put them in order, write `wait` between.
- `fade`, `wait`, `backdrop`, `camera`, `travel-actor` and `slow` are barriers: everything before them finishes first.
- The line shows when every queue is empty.
- Esc or a click skips: every queued cue jumps to its end state.

### 5.3 The verbs

| Verb | Forms | Blocking | Legacy behaviour |
| --- | --- | --- | --- |
| `place` | `place x at <spawn> [facing left\|right]`, `beside <actor>`, `on\|before\|over <prop or actor>`, `in-crowd` (a free spot in the room's `crowd` box), `x ×4` (four, suffixed -1 to -4, round the spot) | no | same |
| `walk` | `walk x to <spawn or actor>`; `walk party to <spawn>` | yes | same |
| `face` | `face x <actor or left/right>` | no | same; up and down are kept for Legacy sheets |
| `travel-actor` | `travel-actor x <map> [<spawn>]`: one actor (or `you`, which takes the party) to another map mid-scene | barrier | same |
| `anim` | `anim x <clip> [<partner or target>]` | yes for x | the five Legacy columns where there's a match (`strike`, `cast`, `hurt`, `downed`, `windup`); otherwise nothing |
| `hold` / `drop` | `hold x <socket> <item>` / `drop x <socket>` | no | nothing (the socket state is still kept, so switching to Rig mid-game is right) |
| `light` | `light <item or actor> [<socket>] <state>` | no | the actor glow only |
| `sigil` | `sigil you\|soulbound\|both <state> [1 frame]` | no | the HUD sigil |
| `show` / `hide` | `show <map object id> [at <spawn>]`, `hide <id>` | no | same. Map objects with these ids must exist, hidden by default (`when=` on the object) |
| `swap` | `swap x <look>` | no | the cast id's other `CharacterLook` |
| `echo` | `echo x [<clip>]`: places x as a memory | no | x at 50% alpha |
| `reflection` | `reflection x delay <s>` | no | nothing |
| `trail` | `trail x <colour>` | no | nothing |
| `backdrop` | `backdrop <id>` / `backdrop off` | barrier | `Painting` if one has the id, else nothing |
| `tint` | `tint <id>` / `tint off` | no (fades 0.6 s) | same |
| `camera` | `camera <actor or spawn>` (`camera north` in old drafts means the nearest spawn above; replace it with a spawn when compiling) | barrier | same |
| `fade` | `fade out <s>` / `fade in <s>` | barrier | same |
| `shake` | `shake <strength 1-3>` | no | same |
| `slow` | `slow <factor>` (1 restores) | barrier | same |
| `sfx` / `music` | `sfx <id>`, `music <track or none>` | no | same |
| `wait` | `wait <s>` | barrier | same |
| `board` | `board on/off` | no | sets `Board` |

`CutsceneDirector` reports a cue it can't run (unknown verb, actor not on the map, spawn missing) as a start-up validation error in debug builds and a log line in release.

### 5.4 Ids in cues

`you`, `soulbound`, `party` (everyone following), `both` (you and {sb}), a cast id, a map object id, or a numbered copy (`protectorate-guard-2`). `check_spawns.py` (§8) checks every `at <spawn>` against the map the script runs on.

## 6. Companions, camp and banter

- **Talk order.** As now. Add: each companion script gets `Unless = <id>-done` and an effect `set <id>-done` on its last node, added by the compiler for every script in `07` §1 to §6 whose header has `with`.
- **Camp state.** A room's `camp` box with a ring of spawns: when you rest there or walk into it with followers, each follower is placed at a ring spot and their talks are open. The existing `ExploreScreen.Camp.cs` waiting-at-camp code is the starting point.
- **"plays when you rest"** headers: the rest action checks for a script whose header says so (the compiler emits a `RestScripts` list), plays the first that can start, then rests.
- **Romance exclusivity.** A condition can't express "none of the other four", so the compiler expands `<id>-romanced` gates as the `07` scripts write them (each lists the other three explicitly). Nothing new needed.
- **Banter triggers.** A room line `banter <id> = x0 y0 x1 y1 · needs <conditions>` fires once when the party walks into the box, if the conditions hold, and sets `banter-<id>-done`.
- **Brannock's shield.** Its texture picks by the flags `shield-names-started`, `brannock-letters-done`, `brannock-fell`.

## 7. Random encounters

From `08-random-encounters.md`, which is room-based now (its §2):

- `RandomEncounterDefinition` record with the §2.8 fields; the compiler writes `RandomEncounterCatalog` from the `#### enc-` headers (fields in backticks and the words after `regions`, `acts`, `party`, `time`, `weight`, `cooldown`, `once`, `approach`).
- `EncounterDirector` (in `World/`): the danger meter counted in rooms entered, the threshold, the eligibility rules of `08` §1.2, the pick, placing the foes or people *in the room* on entry (`08` §2.4), the spotted and ambush approaches, and the save fields.
- Zone header: `region = <id>`, `danger = <n>`. Road rooms: `road = yes` on the room.
- The options screen: `Encounters: Normal / Fewer / Story only` (and Story difficulty sets Story only by default, `11-combat.md` §6.1).
- Debug: F9 forces a roll in the current room; `--encounter <id>` starts one on load.
- Random fights are generated `EncounterDefinition`s: `rnd-<region>-<name>`, built from `08` §3's foe table, in rows, `Lost = "rnd-lost"`.

## 8. Validators and tools

| Tool | Checks | Milestone |
| --- | --- | --- |
| `story/tools/lint_dialogue.py` | The writing guide's mechanical rules; every jump target exists; callback counts | Now |
| `story/tools/build_registry.py` | Every flag read is set somewhere; ids unique; writes `09` | Now |
| `tools/story/compile_scripts.py --check` | The generated C# is current; no id is both generated and hand-written | M1 |
| `DialogueCatalog` start-up validation (extend) | Every speaker, meter, item, fight, expression, cue verb and clip exists; every jump lands; every `travel` map and spawn exists | M1, M2 |
| `tools/check_spawns.py` | Every `place ... at <spawn>` and `walk ... to <spawn>` exists as a spawn in the script's zone (and in zones a `travel-actor` reaches in the same script) | M3 |
| `ZoneCatalog` start-up validation | Every room's exits and doors lead to rooms that exist, floors are closed polygons, every badge kind is known | M3 |
| `tools/rooms/make_room_template.py`, `cut_room.py`, `generate_room_placeholders.py` | `12-rooms.md` §3.5 | M3, M13 |
| `tools/rig/check_clips.py` | Every `anim` clip name has a clip or alias | M12 |
| `tools/rig/check_lineup.py` | Notable characters are distinct in silhouette | M14 |
| `tools/check_assets.py` (extend) | `Content/Rig/`, `Content/Rooms/` and `Content/Backdrops/` | M3, M11, M13 |

**The proof run.** Each milestone adds its scenes to the proof schedule (`ExploreScreen.ProofChapters.cs`) so `--proof` plays through them and saves a shot per scene. M8's proof plays Prologue to the Act 3 stinger with one fixed set of choices (high bond, every liberation, Valencia romanced); M10 adds a second run with low bond, two liberations, Kael with {sb} and Brannock falling.

## 9. New enemies

Stat targets are relative to enemies already tuned, so `--simulate` can check them: "like X" means within 10% of X's HP and damage, "between X and Y" means between them.

| Id | First fight | Target |
| --- | --- | --- |
| `hollowed-climber`, `ram-beast` | A1-C6-01 | climber like `hollowed-footman`, faster; ram beast a mini-boss, like `vault-construct` ×2 HP |
| `varkhul` | A1-C6-01 | Boss: between `thornmother` and 1.5× her |
| `maleficio-boss` | A1-C8-02 | Boss, three phases, each like `varkhul` |
| `hungering` | A1-C8-03 | Survive, not kill: ends at a fifth of its HP |
| `rot-boar`, `ash-hound`, `gloomfang` (existing) | Act 1 random | like `gloomfang` |
| `cube-cultist` | Act 1 random | like `hollowed-hexer` |
| `thrall`, `vesk`, `vreya`, `mordain` | A2-C1-02 | thrall like `hollowed-footman` +20% speed; thrall-knights mini-bosses like `vault-construct` |
| `gilded-knight`, `gilded-sergeant` | A2-C2-02, `07` §5.1 | like `red-gull-thug` ×1.5 (they're nonlethal) |
| `drowned`, `dire-bat`, `tithe-rider`, `blood-wight`, `rockjaw` | Act 2 random | between `hollowed-footman` and `vault-construct` |
| `priest-construct`, `sun-scorched`, `glass-scorpion` | A2-C4 | construct like `vault-construct`; the others like `gloomfang` |
| `severin-boss` | A2-C4-04, A2-C6-03 | Boss: like `maleficio-boss` +15% |
| `protectorate-watchman`, `protectorate-warden` | A3 | watchman like `gilded-knight`; warden like `vault-construct` |
| `crown-hollowed`, `crown-construct` | A3 | crown-hollowed like `hollowed-footman` +25%; construct like `priest-construct` +20% |
| `kael-shadow` | A3-C4-04 | Boss: Kael's own kit, like `severin-boss` ×0.7 |
| `sb-sovereign` | A3-C5-02 | Boss: three phases; {sb}'s class kit in phase 1; the final fight of the game, the hardest |

## 10. Saves

Add to `SaveData`: the `act` meter (it's a meter, so free); "away" party members (`Leave`); each party member's known classes, their ranks and class points, and the equipped one; each actor's sockets (§6.2 of the art spec); the encounter director's meter, threshold and cooldowns; where you are as zone, room and position. The art switches and the difficulty mode stay in settings, not the save. Old saves: each member knows their current class at rank 1; position goes to the old map's zone's first room. Bump the save version and migrate old saves: `act` = 1 if `act1-done` isn't set, else 2.

## 11. Milestones

Each milestone ends when its "done when" holds, the four checks pass (§0.6), and its proof shots look right.

| # | Milestone | Done when |
| --- | --- | --- |
| **M0** | **Set up.** Copy `story/` into the repo root. Run the lint and the registry. Read `02` | Both pass in the repo |
| **M1** | **The model and the compiler.** §3 and §4. Generate the Prologue and Act 1 Chapters 1 to 4 and replace the hand-written scripts with them | `compile_scripts.py --check` passes; the game starts with no validation errors; the existing proof run's shots for P-01 to A1-C4 differ only in lines `04` rewrote |
| **M2** | **Stage cues in Legacy.** §5: `CutsceneDirector`, every verb, Legacy behaviour | Every cue in `04` Chapters 1 to 4 runs or degrades as §5.3 says; no cue errors in the log |
| **M3** | **Rooms.** `12-rooms.md`: `ZoneCatalog`, `RoomScene`, `RoomWalker`, `IndicatorLayer` (badges, names, exit chevrons, glints, Alt), `ZoneMapScreen`, the room tools with placeholder rooms, and every built map (Act 1 Chapters 1 to 4, the docks, Questeria) converted to zones; `check_spawns.py` | The proof run plays P-01 to A1-C4 in rooms; every shop, trainer and door in those zones has its badge; edge exits are labelled; no tile map is loaded |
| **M4** | **Row combat and difficulty.** `11-combat.md` §1 to §4, §6 and §7: `Formation`, Shift, reach, Focus, the shared actions, allies (guests, supports), the new UI bar, difficulty modes, the simulator with the level table and targets. The built fights retuned | `--simulate all` on Balanced meets `11-combat.md` §6.2's targets for every built fight; the built fights play in rooms with their §4.1 allies |
| **M5** | **Classes and trainers.** `10-classes.md`: `SkillCatalog` for all classes, ranks and CP, the Classes tab, `TrainerCatalog`, the trainer screen, trials, the Proving Hall, and the Act 1 Chapter 1 to 4 trainers (Imra, Hale, Tomas, Odelle, Tamsin) | Each base class can be learned, equipped and ranked to 5 in a test save; every skill in the eight base classes and the five early advanced classes works in `--simulate` |
| **M6** | **Act 1, Chapters 5 to 8.** Zones (`12-rooms.md` §6), cast, items, fights (§9, in rows, with allies), the scripts in `04`, `07` §1 to §4, and the Act 1 trainers and teachers in `10` | `--proof` plays A1-C5-01 to the Act 1 card; the four winter quests can each be finished both ways; their fights meet the targets |
| **M7** | **Act 2.** As M6 for `05`, `07` §5 and the Act 2 classes; the eclipse stages (world tints); Valencia's leave and rejoin | `--proof` plays Act 2 to its card; Valencia's trail (`08` §5.2) and SQ-Pell can be finished |
| **M8** | **Act 3.** As M6 for `06`, `07` §6 and Crownsworn; the Protectorate states of every zone, the broken Soul-Link, Kael's side, the four endings | `--proof` plays Act 3 to the stinger in both proof runs (§8); every ending flag is reachable |
| **M9** | **Companions and banter.** §6: camp state, talk order, rest scripts, banter triggers, the shield's textures, the companion teachers | Every `07` script and every `10` §5.3 scene can start in a playthrough; every banter fires once |
| **M10** | **Random encounters.** §7 and `08` | A 30-minute `--proof` walk in each region triggers encounters at `08` §1's rates (logged); every entry can be forced with `--encounter` and finishes |
| **M11** | **The rig.** `03-art-overhaul.md` §3, §4.1, §4.2, §12, §13: `rig_spec.py`, `vector.py`, placeholder kits for every character at 200 px, `RigLibrary` to `RigRenderer`, `IActorView`, the switch | With `Characters = Rig`, the whole proof run plays with placeholder rigs in rooms: walking, fighting in rows, facing left and right |
| **M12** | **Faces, portraits, live items, cutscene clips.** Art spec §4.3, §5, §6 | `check_clips.py` passes; the §6.5 items are visible in the proof shots; portraits show expressions |
| **M13** | **Painted rooms and backdrops.** `12-rooms.md` §3: room templates, the author's paintings cut into layers, animated details, lights; art spec §10 backdrops | Each zone's painted rooms replace its placeholders as they arrive; the proof run's shots show them with badges and labels readable on top |
| **M14** | **Painted kits and the lineup.** As the author supplies kits (`03-art-overhaul.md` §12.1), cut them, replace placeholders, run the lineup | `check_lineup.py` passes for every notable character with a painted kit; the lineup images are in `shots/` |

Rooms (M3) and row combat (M4) come before the new content on purpose: building Act 1's later chapters as tile maps and grid fights and converting them afterwards would be done twice. Make Legacy characters the default until M14 is done for the party and the companions, then switch the default to Rig.

## 12. When something in the content is wrong

- **A line breaks a rule** in `02` and the lint didn't catch it: fix it in the `.md`, recompile.
- **A flag's logic doesn't work** (a gate can't be reached, a branch is dead): fix it in the `.md`, and add a line to the script's prose saying what changed.
- **A scene needs a line that isn't written** (a map needs a bark, a door needs a sign): write it in the `.md` file it belongs to, following `02`, then compile. Don't put new lines straight into C#.
- **The plot doesn't hold together**: stop and ask the author. Don't rewrite story.
