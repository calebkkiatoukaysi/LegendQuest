# Batch 2 room source paintings

The 265 prompts are parsed from the supplied batch document. Generation follows the 195 outdoor pictures in story order; characters follow those, then the remaining 70 interior pictures. The helper records dependencies, original exports, prompt text and SHA-256 fingerprints. Every picture uses a separate built-in image-generation call, grouped into uninterrupted working batches where possible.

The deck's 15 source pictures are saved at 1536 × 1024. Its batch-1 pilot exports are preserved in `art/pixel-batch-2-rooms/preserved-batch-1/`; the original batch-1 generation files and provenance also remain. New room sources replace only the requested source paths, not runtime content.

These are source drafts. The key-colour report found near-magenta rather than exact #FF00FF in generated cutouts. A targeted generator repair of the first sea layer is retained in repair-attempts for comparison. Exact keying, flat palette/grid conversion, pixel registration, panel overlap and composite reconstruction require review before import. The sky bands also retain tonal variation. No acceptance or runtime integration is implied by the file/fingerprint check. Foreground dimensions and door clearance need the final registration pass.

Use `tools/pixel/room_sources.py plan`, `ready`, `save`, `review <zone/room>` and `check`. `progress.json` records the next picture. Original game panels remain untouched in `art/rooms/generated/`.

Checkpoint: 30/265 room source pictures saved and reviewed through Saltwind Harbourside. Its distance layers were corrected to restore missing ships and harbour structures and retain panel-specific positions. The third sky's added sun was removed, and the fishing boat appears only in its distance layer. Earlier attempts remain archived. Next: Fish Market panel 1.

Checkpoint: 45/265 saved and reviewed through Fish Market. Corrected panel 1 water depth to fill extraction gaps; retained the panel-specific Sunspire positions in panels 2 and 3; moved panel 3 foreground lamp to the left to clear the staircase. File, dimension and fingerprint checks passed. Next: Saltwind Pier.

Checkpoint: 50/265 saved and reviewed through Saltwind Pier. Its foreground bollard was shifted to the central berth to clear the warehouse doors. Next: Questeria Gate Plaza.
