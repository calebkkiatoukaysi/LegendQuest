# Pixel art pilot, batch 1

All **29 source paintings** from the supplied batch brief are in
`tools/source/pixel/`, with the requested names. Generated with the built-in
ChatGPT image tool; the originals and selected repair attempts are preserved in
`art/pixel-batch-1/`. This is the painting handoff for the converter described in
the brief. It has not replaced the current game art.

| Source group | Files | Export dimensions |
|---|---:|---|
| Brannock: neutral, angry, disgust, smile, laugh, sad | 6 | 1024 × 1024 |
| Male and female face presets, the same six expressions each | 12 | 1024 × 1024 |
| Short curls and Brannock collar | 2 | 1024 × 1024 |
| Brannock armour repaint | 1 | 1024 × 1536 |
| Gloomfang look | 1 | 1536 × 1024 |
| Emote sheet | 1 | 1536 × 1024 |
| Gull's Wager whole panel, sky, far, mid, foreground, life sheet | 6 | 1536 × 1024 |

The foreground is intentionally empty, as the batch card permits. Every room
layer edit references the selected whole panel. Each expression references its
neutral identity to limit cumulative drift. No third-party game art was used.

## Review and provenance

- `brief.md`: unchanged copy of the owner's batch document.
- `../pixel-art-brief.md`: supplied broader design and conversion plan.
- `prompts.json`: all 29 resolved cards, exact destinations, reference paths and
  required dimensions. The female neutral card includes the female FACE line.
- `revisions.json`: targeted corrections and the owner's explicit export-sizing
  approval. Repairs addressed an ear-outline remnant, collar placement, emote
  margins, prop scale and the deck's guide fit.
- `generation.json`: original and export fingerprints, native dimensions and
  retained earlier attempts. The generator repeatedly returned 1254-square
  portraits, including after the document's size-fix retry. The owner approved
  size-only normalization: nearest-neighbour resampling to 1024-square, with no
  crop, palette conversion or registration warp. The wide/tall exports already
  had the specified dimensions.
- `validation.json`: reproducible export checks and colour/grid diagnostics.
- `layout-review.json`: guide-fit notes and a measured layer-stack comparison.
- `../../art/pixel-batch-1/review/expressions.jpg`: all eighteen expressions,
  cropped only in this review sheet so their faces can be compared.
- `../../art/pixel-batch-1/review/male-layer-diagnostic.png`: diagnostic combination
  of the male face, hair and collar; no adjustments to their positions.
- `../../art/pixel-batch-1/review/room-layer-stack.jpg`: whole panel beside the
  diagnostic keyed layer stack.

## Checks and remaining acceptance work

`python tools/pixel/check_batch_1.py` checks all 29 PNGs, their dimensions and
preserved-original fingerprints. **Zero export errors.** All 24 emote/life cells
are inside the guides' safe margins after the emote correction. The layer stack
has **zero uncovered pixels** using the diagnostic near-magenta key.
`python tools/check_assets.py` checks the source credits against the repository.

These are generated **source candidates**, not accepted true pixel art. The
checks retain 46 conversion warnings: the green/magenta backgrounds are near the
requested keys rather than single exact colours, and the twenty square pictures
do not yet have uniformly flat 8 × 8 blocks. Native exports also contain many more
than 32 colours. The converter must flatten the keys, remove coloured fringes,
reduce palettes and snap the portrait grid. Size normalization alone does none
of that, and the checker does not claim otherwise.

Registration also needs review before cutting:

- The selected deck floor begins around y688 rather than guide y656 despite
  several generator corrections; its horizon is around y508 rather than y512.
  Fit the whole panel and all layers together. The walking band is unobstructed.
- The male neck ends at y581 rather than the y600 guide. The collar has a raised
  upper edge; register their actual masks together. The diagnostic composite
  shows the relationship and residual key-colour fringes.
- The three seagull cells have the requested wing positions, but their body/head
  anchors and apparent sizes need registration before forming an animation loop.
  The revised sheet has substantially smaller props; the converter still needs
  to set their final room scale.
- Generated layer extraction is visually consistent rather than pixel-identical:
  the keyed stack's mean absolute RGB difference from the whole is 7.44/255;
  4.49% of pixels differ by more than 32 in at least one channel. Inspect rigging
  fringes and hidden-area fills before accepting it for parallax.
- Test-cut the armour on the existing bones and inspect joint overlap; this pilot
  does not claim that a generated sheet has passed the runtime armour importer.

## Next conversion pass

Follow the supplied broader brief: convert these sources, test Brannock's armour
against the old version, build the portrait blink/talk viewer, register the gull
loop and layered room, then use Gloomfang's look to make its parts template for
batch 2. Keep the painted runtime available until the owner accepts the pixel
look. This handoff includes no renderer switch, runtime import or balance change.

To replay the owner-approved export sizing from immutable originals:

```text
python tools/pixel/check_batch_1.py --normalize
```

The command refuses nonuniform scaling, changed originals or edited exports. It
does not crop, key, warp, quantize or promote an asset into the game.
