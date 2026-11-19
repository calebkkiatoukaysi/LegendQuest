"""
Fetches the sample libraries the score is rendered from, into a cache outside the repo. Nothing it
downloads is ever committed or shipped: only the WAVs that tools/score/compose.py renders from them
are, under Content/Music.

    python tools/score/fetch_samples.py           # clone or update both libraries (about 4 GB on disk)
    python tools/score/fetch_samples.py --check   # only say whether the cache is complete

The libraries, both by Versilian Studios and both released under CC0 1.0
(https://creativecommons.org/publicdomain/zero/1.0/), checked on their repositories' pages and LICENSE
files (VCSL's LICENSE is on its master branch; the README of its sfz branch, used here, says the same):

    VSCO 2 Community Edition   https://github.com/sgossner/VSCO-2-CE   branch SFZ
    Versilian Community Sample Library (VCSL)   https://github.com/sgossner/VCSL   branch sfz

Each is cloned partially (no blobs until they're checked out) and sparsely (only the instrument
folders listed below), then pinned to the commit in COMMITS, so the score always renders from the
same recordings. The cache is %LOCALAPPDATA%/LegendQuest-samples, or wherever LQ_SAMPLES points.

Needs git on the PATH and CPython 3.12.
"""

import os
import subprocess
import sys

LIBRARIES = {
    "VSCO-2-CE": {
        "url": "https://github.com/sgossner/VSCO-2-CE.git",
        "branch": "SFZ",
        "commit": "6dd651d55dde97fd4028699be9d4481f26917891",
        "folders": [
            "Strings/Violin Section", "Strings/Viola Section", "Strings/Cello Section",
            "Strings/Solo Contrabass", "Strings/Solo Violin", "Strings/Harp",
            "Brass/F Horn", "Brass/Trumpet", "Brass/Tenor Trombone", "Brass/Tuba",
            "Woodwinds/Flute", "Woodwinds/Oboe", "Woodwinds/Clarinet", "Woodwinds/Bassoon",
            "Woodwinds/Piccolo", "Percussion", "Keys/Organ",
        ],
    },
    "VCSL": {
        "url": "https://github.com/sgossner/VCSL.git",
        "branch": "sfz",
        "commit": "dfcf4a4918771eee884b96ad4493de82ef84daf6",
        "folders": [
            "Chordophones/Composite Chordophones/Folk Harp",
            "Chordophones/Composite Chordophones/Strumstick",
            "Chordophones/Zithers/Harpsichord, Italian/Psaltery, Bowed and Plucked",
            "Aerophones/Edge-blown Aerophones/Baroque Alto Recorder",
            "Aerophones/Edge-blown Aerophones/Baroque Soprano Recorder",
            "Aerophones/Edge-blown Aerophones/Baroque Tenor Recorder",
            "Membranophones/Struck Membranophones/Frame Drum",
            "Membranophones/Struck Membranophones/Bass Drum 2",
            "Membranophones/Struck Membranophones/Snare Drum, Rope Tension",
            "Membranophones/Struck Membranophones/Tom 1",
            "Idiophones/Struck Idiophones/Gong 1",
            "Idiophones/Struck Idiophones/Suspended Cymbal 1",
            "Idiophones/Struck Idiophones/Clash Cymbals 1",
            "Idiophones/Struck Idiophones/Tubular Bells 1",
            "Idiophones/Struck Idiophones/Tambourine 1",
            "Idiophones/Struck Idiophones/Hand Chimes",
            "Idiophones/Struck Idiophones/Anvil",
        ],
    },
}

# A file in each library that has to be there once it's checked out, as a quick completeness test.
MARKERS = {
    "VSCO-2-CE": "Strings/Violin Section/susVib/VlnEns_susVib_A3_v1.wav",
    "VCSL": "Chordophones/Composite Chordophones/Folk Harp.sfz",
}


def cache_root():
    """Where the libraries live: LQ_SAMPLES, else %LOCALAPPDATA%/LegendQuest-samples."""
    root = os.environ.get("LQ_SAMPLES")
    if not root:
        base = os.environ.get("LOCALAPPDATA") or os.path.expanduser("~/.local/share")
        root = os.path.join(base, "LegendQuest-samples")
    return root


def missing():
    """The libraries that aren't in the cache yet (or aren't checked out at their pinned commit)."""
    out = []
    for name, lib in LIBRARIES.items():
        path = os.path.join(cache_root(), name)
        if not os.path.isfile(os.path.join(path, MARKERS[name])):
            out.append(name)
            continue
        head = subprocess.run(["git", "-C", path, "rev-parse", "HEAD"], capture_output=True, text=True)
        if head.stdout.strip() != lib["commit"]:
            out.append(name)
    return out


def git(*args, cwd=None):
    print("  git " + " ".join(args), flush=True)
    subprocess.run(["git", *args], cwd=cwd, check=True)


def fetch(name, lib):
    path = os.path.join(cache_root(), name)
    if not os.path.isdir(os.path.join(path, ".git")):
        os.makedirs(cache_root(), exist_ok=True)
        git("clone", "--filter=blob:none", "--sparse", "--no-checkout", "-b", lib["branch"], lib["url"], path)
    git("config", "core.longpaths", "true", cwd=path)
    git("sparse-checkout", "set", "--cone", *lib["folders"], cwd=path)
    git("checkout", "--detach", lib["commit"], cwd=path)


def main(argv):
    if "--check" in argv:
        gone = missing()
        print("sample cache %s: %s" % (cache_root(), "complete" if not gone else "missing " + ", ".join(gone)))
        return 1 if gone else 0
    for name, lib in LIBRARIES.items():
        print("%s -> %s" % (name, os.path.join(cache_root(), name)), flush=True)
        fetch(name, lib)
    gone = missing()
    if gone:
        print("still missing: " + ", ".join(gone))
        return 1
    print("sample cache complete: " + cache_root())
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
