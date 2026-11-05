# LegendQuest: story and art plan

Everything needed to finish LegendQuest's main story, from the Prologue to the end of Act 3, and to rebuild the game as a mix of Baldur's Gate 3 and AdventureQuest Worlds: AQW-style painted rooms with marked shops and doors, side-view rigged characters, side-view turn-based fights in rows with NPC allies, and classes collected from trainers and people and ranked up AQW-style on top of 5e rules. It's written to be implemented by Claude working in the repo, one milestone at a time.

## Reading order

1. **`01-build-plan.md`** — start here. What to build, in what order, how the scripts become C#, and how to know each milestone is done.
2. **`02-writing-guide.md`** — the rules every line follows, and every character's voice. Read before touching a line.
3. **`04`–`08`** — the content:
   - `04-script-prologue-act1.md`: Prologue and Act 1 (Chapters 1–4 rewritten with the same ids; 5–8 new)
   - `05-script-act2.md`: Act 2, "The Long Eclipse"
   - `06-script-act3.md`: Act 3, "Soul-Bound"
   - `07-companions.md`: camp talks, the five personal quests, romances, {sb}'s letters home, Elara, banters
   - `08-random-encounters.md`: the encounter system and 36 written encounters
4. **`10-classes.md`** — classes: AQW-style swapping and ranks 1 to 10 on 5e rules, 8 base and 17 advanced classes with every skill, who teaches each one, and their scenes.
5. **`11-combat.md`** — side-view row fights, which NPCs fight beside you in every story fight, difficulty modes and win-rate targets, and the level curve.
6. **`12-rooms.md`** — AQW-style rooms: the format, edge exits, badges over shops, trainers, inns and doors, how detailed the painting is, and every zone's rooms.
7. **`03-art-overhaul.md`** and **`03b-world-art.md`** — the art: the rig, live items, faces, detail level, who looks like what; then each place's brief, buildings, props, effects, UI and music.
8. **`09-registry.md`** — generated. Every script, flag, meter, item, speaker, fight, backdrop and spawn the scripts use, and who sets and reads each.

`COMPANIONS.md`, at the repo root, stays the bible for who the companions are.

## The story in four lines

You and your Soul-Bound, sworn since childhood to never leave the other behind, sail to Fable. Act 1: you stop Maleficio and his Black Box, and {sb} quietly keeps a shard of it. Act 2: you stop Lord Severin putting out the sun, and {sb} learns the crown was built on a massacre and is handed the key to both relics. Act 3: {sb} fuses them into a crown to protect Fable from a vision of its ending, kills the king by accident, breaks your bond, and rules; your final fight tears time open, and the thing that sent the vision walks through.

## Tools

```text
python story/tools/lint_dialogue.py           # the writing guide's mechanical rules; exit 1 on errors
python story/tools/build_registry.py          # rewrites 09-registry.md
python story/tools/build_registry.py --check  # and exits 1 if a flag is read but never set
```

Both need only CPython 3.10+. The build plan adds the script compiler (`tools/story/compile_scripts.py`) in M1, the room tools in M3 and the rig tools in M11.

## Status when handed over

- Lint: 0 errors. The warnings left are shouts in fights and a pedlar hawking, which are allowed.
- Registry: 245 scripts, 0 problems (every flag read is set somewhere, or by code noted in the tool).
- Nothing in the repo is changed yet. M0 copies this folder into the repo root.
