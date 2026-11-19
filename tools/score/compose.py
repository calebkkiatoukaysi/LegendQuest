"""
LegendQuest's score: renders every song and jingle in Content/Music from the pieces in
tools/score/pieces, played on the Versilian sample libraries through tools/score's own sampler,
with the choir and the scenery synthesised. Run it from the repo root:

    python tools/score/fetch_samples.py          # once: the sample libraries (about 4 GB on disk, outside the repo)
    python tools/score/compose.py                # everything (a few minutes)
    python tools/score/compose.py steel engage   # just these, by file name
    python tools/score/compose.py -v steel       # also print each part's level
    python tools/score/compose.py --out DIR      # write somewhere else instead of Content/Music
    python tools/score/check.py                  # loudness, peaks and loop seams of what's there

Every piece is built on one theme (theory.THEME_MAJOR), re-harmonised and re-scored for each place.
The songs loop: each is a whole number of bars, with every note tail and the hall's echo wrapped
round onto its start, so it plays round under MediaPlayer.IsRepeating without a seam. The jingles
and the stinger are one-shots that end in silence.

Deterministic: the same pieces and the same samples (pinned in fetch_samples.py) write the same
bytes. Needs CPython 3.12 with numpy and scipy.
"""

import importlib
import os
import sys
import time

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
sys.path.insert(0, os.path.join(HERE, "pieces"))

ROOT = os.path.normpath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "Content", "Music")

# file name: (module in pieces/, function, what it's for). The eight songs keep the file names the
# game already loads (Audio/AudioManager.SongName); the jingles and the stinger are new.
SONGS = {
    "embers":            ("title", "build", "Title: the title screen, options and save slots"),
    "hearthsmoke":       ("town", "build", "Exploring: towns, lit maps and houses"),
    "meadowlark":        ("field", "build", "Meadow: the road and the fields"),
    "cold-stone":        ("dungeon", "build", "Underground: barrows, quarries and vaults"),
    "salt-and-lanterns": ("sea", "build", "Sea: the Gull's Wager"),
    "sunspire":          ("keep", "build", "Court: Sunspire Keep"),
    "steel":             ("battle", "build", "Battle: fights"),
    "the-hunger":        ("boss", "build", "Boss: boss fights"),
}
ONE_SHOTS = {
    "victory":           ("jingles", "victory", "Victory: a fight won (once, never loops)"),
    "defeat":            ("jingles", "defeat", "Defeat: a fight lost (once, never loops)"),
    "engage":            ("jingles", "engage", "the stinger as a fight starts, in the battle key"),
}
ALL = dict(SONGS, **ONE_SHOTS)


def build(name):
    mod, fn, _ = ALL[name]
    return getattr(importlib.import_module(mod), fn)()


def main(argv):
    import mix
    import sampler
    verbose = "-v" in argv
    out = OUT
    args = [a for a in argv if a != "-v"]
    if "--out" in args:
        i = args.index("--out")
        out = args[i + 1]
        del args[i:i + 2]
    names = args or list(ALL)
    for nm in names:
        if nm not in ALL:
            raise SystemExit("no piece called %r (try: %s)" % (nm, ", ".join(ALL)))
    sampler.require_cache()
    os.makedirs(out, exist_ok=True)
    for nm in names:
        t0 = time.time()
        piece = build(nm)
        print("%s: %.2f BPM, %d/%s, %d bars%s" % (nm, piece.bpm, piece.meter, "4" if piece.meter != 6 else "8",
                                                 piece.bars, " (loop)" if piece.loop else ""), flush=True)
        audio = piece.render(verbose=verbose)
        path = os.path.join(out, nm + ".wav")
        mix.write_wav(path, audio, seed=piece.seed)
        r = piece.report
        print("  -> %s  %.2f s  %.1f LUFS  true peak %.1f dBFS  (%.0f s)" % (
            os.path.relpath(path, ROOT), len(audio) / sampler.RATE, r["lufs"], r["true_peak"], time.time() - t0),
            flush=True)


if __name__ == "__main__":
    main(sys.argv[1:])
