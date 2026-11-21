# Front-facing portrait series

The owner requested automatic progress through the full626-picture document, overriding its per-picture pauses. Pictures are generated one at a time, in order; expression edits reference their own neutral, and variants retain their stated parent. The current position and exact sources are in progress.json; jobs.json contains the resolved prompt/dependency list.

New versions live under tools/source/pixel/portraits/<character>/front-facing/ so the previous128-grid pilot is preserved. Original exports remain under art/pixel-portraits/originals. Owner-approved nearest-neighbour sizing normalizes square exports to1024x1024 without cropping or palette/grid conversion. These are source drafts for subsequent conversion and registration review, not runtime imports.

The source workflow is tools/pixel/portrait_sources.py: plan, ready, save, review and check. Contact sheets in art/pixel-portraits/review support visual checks as each set completes. No new user message is required between pictures.

Valencia design update: the owner requested black hair, red eyes and fangs while retaining the art. The 13 original silver-haired portraits remain in their original paths. Active replacements and future variants use front-facing/black-hair-red-eyes/. design-overrides.json records the user-directed changes; design-history/valencia-silver-hair/ preserves the prior provenance and prompts.
