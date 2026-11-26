# Batch 2 character repaint queue

All 204 requested original files exist, match their prompt dimensions and have recorded SHA-256 fingerprints. No repaints have been generated or imported. `jobs.json` preserves each group's complete supplied prompt and the exact original and destination paths; `source-audit.json` records the audit. Rebuild with `python tools/pixel/plan_repaints.py`.

These are inventory numbers, not execution order. Finish outdoor rooms through room image 195 first. Then generate the two bodies, party and Prologue armours, and remaining repaint groups before the 70 interior room images. Inspect each original before its edit. Save new source paintings beneath `tools/source/pixel/repaints/` with the same subpath; preserve originals. Each output needs visual overlay/registration, solid-black background and flat-colour checks before conversion or runtime replacement. None of those output checks is implied by this input audit.

The owner requested automatic continuation in this chat. The brief's new-chat suggestions do not authorize creating chats or interrupting every eight images. Use one built-in image generation call per distinct requested asset and keep prompts and original generation exports with provenance.
