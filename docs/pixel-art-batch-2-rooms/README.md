# Batch 2 room source paintings

The 265 prompts are parsed from the supplied batch document. Generation follows the 195 outdoor pictures in story order; characters follow those, then the remaining 70 interior pictures. The helper records dependencies, original exports, prompt text and SHA-256 fingerprints. Every picture uses a separate built-in image-generation call, grouped into uninterrupted working batches where possible.

The deck's 15 source pictures are saved at 1536 × 1024. Its batch-1 pilot exports are preserved in `art/pixel-batch-2-rooms/preserved-batch-1/`; the original batch-1 generation files and provenance also remain. New room sources replace only the requested source paths, not runtime content.

These are source drafts. The key-colour report found near-magenta rather than exact #FF00FF in generated cutouts. A targeted generator repair of the first sea layer is retained in repair-attempts for comparison. Exact keying, flat palette/grid conversion, pixel registration, panel overlap and composite reconstruction require review before import. The sky bands also retain tonal variation. No acceptance or runtime integration is implied by the file/fingerprint check. Foreground dimensions and door clearance need the final registration pass.

Use `tools/pixel/room_sources.py plan`, `ready`, `save`, `review <zone/room>` and `check`. `progress.json` records the next picture. Original game panels remain untouched in `art/rooms/generated/`.

Checkpoint: 30/265 room source pictures saved and reviewed through Saltwind Harbourside. Its distance layers were corrected to restore missing ships and harbour structures and retain panel-specific positions. The third sky's added sun was removed, and the fishing boat appears only in its distance layer. Earlier attempts remain archived. Next: Fish Market panel 1.

Checkpoint: 45/265 saved and reviewed through Fish Market. Corrected panel 1 water depth to fill extraction gaps; retained the panel-specific Sunspire positions in panels 2 and 3; moved panel 3 foreground lamp to the left to clear the staircase. File, dimension and fingerprint checks passed. Next: Saltwind Pier.

Checkpoint: 50/265 saved and reviewed through Saltwind Pier. Its foreground bollard was shifted to the central berth to clear the warehouse doors. Next: Questeria Gate Plaza.

Checkpoint: 65/265 saved and reviewed through Gate Plaza. Removed spire ornament from first sky, extended distance water behind the gate, and narrowed foreground planters to clear doors and the festival arch. The third distance layer retains an arch-shaped patch of the festival view: its edges and fill behind the arch still need registration/parallax review. Exact key colour and palette remain pending as above. Next: Festival Square.

Checkpoint: 80/265 saved and reviewed through Festival Square. Extended city/terrain behind removed near objects; connected castle bases, corrected panel 3 castle positions, and moved foreground topiaries to clear stairs and gates. All 80 current exports passed file/dimension/fingerprint checks. Pixel registration, exact keying and composite acceptance remain pending. Next: Lantern Market.

Checkpoint: 95/265 saved and reviewed through Lantern Market. Removed spire ornament from first sky; continued distant city below the skyline; restored forge smoke to panel 2 middle ground; moved/shrank planter and moved final lamp to clear doors. All 95 exports passed file/dimension/fingerprint checks. Exact magenta, palette, registration and runtime import remain pending. Next: Sunspire Steps.

Checkpoint: 100/265 saved and reviewed through Sunspire Steps. Extended far fill; restored near green foliage in middle ground; narrowed foreground pillar to clear chapel stairs, removed its added banner, and reduced the plain topiary below the quarter-height limit. File/dimension/fingerprint and asset-credit checks passed. Next: Questeria Thornbridge Gate.

Checkpoint: 105/265 saved and reviewed through Questeria Thornbridge Gate. Kept the camp/bridge/river through the arch in the far layer, restored continuous far fill to the street line, and kept foreground lamp clear of entrances. Next: Old Road Farmland.

Checkpoint: 120/265 saved and reviewed through Old Road Farmland. Kept sun only in panel 1 sky; separated windmill sails/vane, distant village/castle/aqueduct/waterfall and near bridge/stream into their intended layers. Extended distant fields behind removed near scenery in panels 1 and 2. File/dimension/fingerprint and asset-credit checks passed. Exact keying, grid/palette, registration and runtime import remain pending. Next: Old Road Fork.

Checkpoint: 135/265 saved and reviewed through Old Road Fork. Narrowed all foreground trunks/rocks and corrected panel 1 bases to the bottom edge; extended panel 2 far coverage and retained panel 3 watchtower, bridge and second village. Panel 1 far hill skyline still sits lower than the full redraw despite a targeted generator correction; resolve during registration/composite review. All file/dimension/fingerprint and asset-credit checks passed; exact keying, palette/grid and runtime integration remain pending. Next: Mirewood Edge.
