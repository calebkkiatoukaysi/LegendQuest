"""
The bits of music theory the pieces are written with: note names, chord symbols, voicing chords so
each voice moves as little as it can, and LegendQuest's theme, which every piece is built on.

Note names are scientific pitch: C4 is middle C, MIDI 60; sharps are '#', flats 'b' (Bb3, F#4).
A chord symbol is a root, a quality and an optional bass: D, Dm, D7, Dmaj7, Dm7, Dsus4, Dsus2, Ddim,
Daug, D5 (no third), Dadd9, D/F# or Dm/A.
"""

import itertools
import re

_PC = {"C": 0, "D": 2, "E": 4, "F": 5, "G": 7, "A": 9, "B": 11}

QUALITIES = {
    "": (0, 4, 7), "m": (0, 3, 7), "7": (0, 4, 7, 10), "maj7": (0, 4, 7, 11), "m7": (0, 3, 7, 10),
    "sus4": (0, 5, 7), "sus2": (0, 2, 7), "dim": (0, 3, 6), "aug": (0, 4, 8), "5": (0, 7),
    "add9": (0, 2, 4, 7), "madd9": (0, 2, 3, 7), "m6": (0, 3, 7, 9), "6": (0, 4, 7, 9),
    "7sus4": (0, 5, 7, 10), "m(b6)": (0, 3, 7, 8),
}


def n(name):
    """A note name as a MIDI number: n("C4") == 60, n("F#3") == 54, n("Bb2") == 46."""
    m = re.fullmatch(r"([A-G])([#b]*)(-?\d+)", name)
    if not m:
        raise ValueError("not a note: %r" % name)
    pc = _PC[m.group(1)] + m.group(2).count("#") - m.group(2).count("b")
    return 12 * (int(m.group(3)) + 1) + pc


def pc(name):
    """A pitch class (0-11) from a name with no octave: pc("F#") == 6."""
    return (_PC[name[0]] + name[1:].count("#") - name[1:].count("b")) % 12


def chord(symbol):
    """(root pitch class, the chord's pitch classes in order from the root, bass pitch class)."""
    m = re.fullmatch(r"([A-G][#b]?)([^/]*)(?:/([A-G][#b]?))?", symbol)
    if not m or m.group(2) not in QUALITIES:
        raise ValueError("not a chord: %r" % symbol)
    root = pc(m.group(1))
    tones = [(root + i) % 12 for i in QUALITIES[m.group(2)]]
    bass = pc(m.group(3)) if m.group(3) else root
    return root, tones, bass


def nearest(pitch_class, near):
    """The MIDI note of this pitch class closest to `near`."""
    base = near - ((near - pitch_class) % 12)
    return base if near - base <= 6 else base + 12


def bass_note(symbol, near):
    """The chord's bass note closest to `near`."""
    return nearest(chord(symbol)[2], near)


def voice(symbol, prev, lo, hi):
    """Voices the chord for as many voices as `prev` has, each moving as little as it can from its
    note in `prev`, every chord tone covered where there are voices enough, kept within lo..hi."""
    _, tones, _ = chord(symbol)
    k = len(prev)
    best, best_cost = None, None
    # try each assignment of chord tones to voices that covers as many tones as possible
    need = tones if len(tones) <= k else tones[:k]
    for combo in itertools.product(tones, repeat=k):
        if not set(need) <= set(combo):
            continue
        notes = []
        for p, t in zip(prev, combo):
            c = nearest(t, p)
            while c < lo:
                c += 12
            while c > hi:
                c -= 12
            notes.append(c)
        cost = sum(abs(a - b) for a, b in zip(notes, prev)) + 6 * (len(notes) - len(set(notes)))
        # keep voices from crossing
        cost += 4 * sum(1 for a, b in zip(notes, notes[1:]) if b < a)
        if best_cost is None or cost < best_cost:
            best, best_cost = notes, cost
    return best


def progression(symbols, start, lo, hi):
    """Voices a list of chords one after another from the voicing `start`."""
    out, prev = [], list(start)
    for s in symbols:
        prev = voice(s, prev, lo, hi)
        out.append(prev)
    return out


# --------------------------------------------------------------------------- #
# The theme
# --------------------------------------------------------------------------- #

# LegendQuest's theme, eight bars of 4/4, as (semitones above the tonic, beats). Its head (bar 1:
# up a fifth, a step down and back) is the call every piece answers; bar 2 lifts to the sixth;
# bars 6-8 come down from the octave through the flat seventh and the flat sixth, over the
# flat-six, flat-seven, one cadence that gives it its sound.
THEME_MAJOR = [
    (0, 1), (7, 1.5), (5, 0.5), (7, 1),          # 1  I
    (9, 3), (7, 0.5), (5, 0.5),                  # 2  IV
    (4, 1), (0, 1), (2, 1.5), (4, 0.5),          # 3  vi, V/V... (Bm, A)
    (7, 4),                                      # 4  V
    (0, 1), (7, 1.5), (5, 0.5), (7, 1),          # 5  I
    (12, 3), (10, 0.5), (8, 0.5),                # 6  bVI
    (10, 1.5), (7, 0.5), (5, 1), (2, 1),         # 7  bVII
    (0, 4),                                      # 8  I
]
THEME_MAJOR_CHORDS = ["I", "IV", "vi|V", "V", "I", "bVI", "bVII", "I"]

# The same tune in the minor (battle, defeat): the sixth and third flattened.
THEME_MINOR = [
    (0, 1), (7, 1.5), (5, 0.5), (7, 1),
    (8, 3), (7, 0.5), (5, 0.5),
    (3, 1), (0, 1), (2, 1.5), (3, 0.5),
    (7, 4),
    (0, 1), (7, 1.5), (5, 0.5), (7, 1),
    (12, 3), (10, 0.5), (8, 0.5),
    (10, 1.5), (7, 0.5), (5, 1), (2, 1),
    (0, 4),
]

# The head alone, the call: (semitones, beats).
HEAD = [(0, 1), (7, 1.5), (5, 0.5), (7, 1)]


def theme(tonic, minor=False, bars=None):
    """The theme on this tonic (a MIDI note) as [(midi, beats)], all eight bars or the first `bars`."""
    src = THEME_MINOR if minor else THEME_MAJOR
    out, total = [], 0.0
    for semis, beats in src:
        if bars is not None and total >= 4 * bars:
            break
        out.append((tonic + semis, beats))
        total += beats
    return out


def tokens(notes):
    """[(midi, beats)] back to Part.line text, for printing a line in a readable form."""
    names = ["C", "C#", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B"]
    return " ".join("%s%d:%g" % (names[m % 12], m // 12 - 1, b) for m, b in notes)
