# Batch 2 checkpoint — 25 November 2026

Saved source deliverables from this brief: **436 of 732**. This comprises all **263 remaining portraits** (the complete portrait collection now has 686) and **173 of 265 room pictures**. The 204 character repaints have not been generated. There are 296 deliverables left: 22 outdoor room pictures, 204 repaints, then 70 interior room pictures.

The built-in generator returned HTTP 429 while requesting room image 174, `thornbridge/camp-gate/bg-1.mid.png`. It reported `resets_in_seconds: 58730`, approximately 16 hours 19 minutes, around **11:10 a.m. America/Chicago on 26 November**. This is an estimate from the tool response, not a guaranteed availability time. No fallback API was authorized or used. No image 174 was saved. No automatic retry or background continuation is scheduled.

Resume instructions:

1. Review `docs/pixel-art-batch-2-rooms/progress.json` and run `python tools/pixel/room_sources.py ready`; it must identify image 174.
2. First correct image 173's incomplete lower far fill with the built-in generator using `docs/pixel-art-batch-2-rooms/camp-gate-far-fill-pending.txt`, viewing the current export before editing. Save it with `room_sources.py revise 173 <generated-file> <prompt-file>`, preserving the earlier attempt.
3. Continue image 174, then 175 and the remaining outdoor panels through image 195. The full Camp Gate redraw and sky/far pictures are already saved; do not generate duplicates.
4. Proceed to the character queue in `docs/pixel-art-batch-2-repaints/`, bodies first and then party/Prologue armours. All 204 inputs have been audited, with no missing files or size mismatches. Select armour priorities from current story/equipment data before generation; inventory order is not execution order.
5. Finish the remaining 70 interior room images. Do not replace current runtime art merely because a source painting exists.

Artwork uses the built-in image generator. Room exports are in `tools/source/pixel/rooms/`; original generations and revisions are in `art/pixel-batch-2-rooms/`; saved prompts and progress are in `docs/pixel-art-batch-2-rooms/`. Portrait source, prompt and provenance locations are recorded in `docs/pixel-art-portraits/README.md`.

These are source drafts, not accepted pixel conversions or runtime imports. Exact key colours, flat palettes, uniform pixel grids, layer registration, edge coverage and panel joins still need conversion/review. Room README checkpoints record specific known issues, including the Fork skyline, Mirewood extraction shifts/margins and Deep Mirewood near mist/top edges. Keep those limitations visible when handing the sources over for conversion.
