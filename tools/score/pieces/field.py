"""
"Meadowlark", for the road and the fields: the theme as a journey, in A major with a Lydian lift
(B major for D, the raised fourth, which is what makes open country sound wide), 108 BPM, 40 bars.

The violas run eighths on an open fifth the whole way, the basses walk under them in pizzicato,
and a light snare keeps a march. Over it:

    bars  0-3   the running strings alone, A and B over a held A
    bars  4-11  two horns play the theme, the second bar lifted onto B major
    bars 12-19  the flute's own tune, over D, A, B and E, the oboe holding a line under it
    bars 20-27  the theme again an octave up, flute and violins, the horns holding under it
    bars 28-31  the head turned over in the cellos and horns through F sharp minor, D, B and E
    bars 32-35  everyone: the theme's second half, F and G (the flat six and seven) home to A
    bars 36-39  the running strings alone again, round to the top
"""

from common import bass, broken, pad
from score import Piece
from theory import n, theme
import instruments as I

INTRO = "A:4 | B/A:4 | D/A:4 | A:4"
A = "A:4 | B:4 | F#m:2 E:2 | Esus4:2 E:2 | A:4 | F:4 | G:4 | A:4"
TUNE = "D:4 | A/C#:4 | Bm:4 | E:4 | D:4 | A/C#:4 | B:4 | E:4"
DEV = "F#m:4 | D:4 | B:4 | E:4"
GRAND = "A:4 | F:4 | G:4 | A:4"

FLUTE_TUNE = ("E5:2 F#5:1 A5:1 | C#6:3 B5:1 | A5:2 F#5:2 | G#5:4 | F#5:2 A5:1 B5:1 | C#6:2 E6:2 "
              "| D#6:3 B5:1 | B5:2 G#5:2")
OBOE_LINE = "F#4:4 | E4:4 | D4:4 | E4:2 D4:2 | D4:4 | E4:4 | D#4:4 | E4:4"


def build():
    p = Piece("meadowlark", bpm=108, meter=4, bars=40, lufs=-20.0, title="Meadowlark",
              hall=dict(rt60=2.0, predelay=0.02, hf_rt=0.5), wet=-5.0)
    vla = p.part("violas", "vla_spic", pan=0.15, send=0.25, gain=-4.0, human_ms=4)
    pizz = p.part("basses pizz", "cb_pizz", pan=0.4, send=0.2, gain=-4.0)
    vcp = p.part("cellos pizz", "vc_pizz", pan=0.3, send=0.2, gain=-6.0)
    vc = p.part("cellos", "vc", pan=0.3, send=0.3, gain=-3.0, delay_ms=20)
    horn = p.part("horns", "horn", pan=-0.2, send=0.35, gain=0.0)
    horn2 = p.part("horns 3-4", "horn", pan=-0.3, send=0.35, gain=-5.0)
    tpt = p.part("trumpet", "tpt", pan=0.1, send=0.35, gain=-4.0)
    flute = p.part("flute", "flute", pan=0.2, send=0.35, gain=0.0)
    oboe = p.part("oboe", "oboe", pan=0.1, send=0.35, gain=-4.0)
    vln = p.part("violins", "vln", pan=-0.25, send=0.32, gain=-2.0, delay_ms=20)
    vln2 = p.part("violins II", "vln", pan=-0.05, send=0.32, gain=-6.0, delay_ms=20)
    harp = p.part("harp", "harp", pan=-0.35, send=0.4, gain=-7.0)
    snare = p.part("snare", "snare", pan=-0.1, send=0.25, gain=-16.0, human_ms=4)
    timp = p.part("timpani", "timp", pan=0.0, send=0.3, gain=-5.0)
    sus = p.part("suspended cymbal", "sus_cymbal", pan=0.2, send=0.4, gain=-12.0)
    glock = p.part("glockenspiel", "glock", pan=0.3, send=0.5, gain=-12.0)

    def running(start, text, vel=64):
        """The violas' eighths: low, fifth, octave, fifth on each chord."""
        broken(vla, start, text, n("A3"), [0, 2, 3, 2], 0.5, vel=vel, accent=10)

    def walk(start, text, vel=72):
        bass(pizz, start, text, n("A1"), vel=vel, rhythm=[(0, 1, 6), (2, 1, -4)])
        bass(vcp, start, text, n("A2"), vel=vel - 6, rhythm=[(1, 1, -6), (3, 1, -8)])

    def march(start, bars, vel=70):
        for b in range(bars):
            t = start + 4 * b
            snare.line(t, "r:1 C4:.5 C4:.25 C4:.25 r:1 C4:.5 C4:.5", vel=vel, transpose=I.SNARE_HIT - n("C4"))

    # bars 0-3 and 36-39: the running strings alone
    for s in (p.bar(0), p.bar(36)):
        running(s, INTRO, 66)
        pizz.note(s, n("A1"), 16, 62, legato=0.2)
        pad(vc, s, INTRO, [n("A2"), n("E3")], 40, 60, vel=54, attack=0.6)
        glock.line(s + 12, "A6:1 E7:1.5 D7:.5 E7:1", vel=58)

    # bars 4-11: the horns' theme
    a = p.bar(4)
    horn.notes_of(a, theme(n("A3")), vel=92, attack=0.04)
    running(a, A, 66)
    walk(a, A)
    pad(vln2, a, A, [n("C#5"), n("E5")], 64, 81, vel=56, attack=0.5)
    march(a, 8, 64)
    timp.note(a, n("A2"), 2, 84)
    timp.note(p.bar(8), n("A2"), 2, 76)
    timp.note(p.bar(11), n("A2"), 2, 80)

    # bars 12-19: the flute's tune
    b = p.bar(12)
    flute.line(b, FLUTE_TUNE, vel=86, attack=0.05)
    oboe.line(b, OBOE_LINE, vel=72, attack=0.1)
    running(b, TUNE, 64)
    walk(b, TUNE, 70)
    broken(harp, b, TUNE, n("D3"), [0, 1, 2, 3, 4, 3, 2, 1], 0.5, vel=60)
    pad(vln2, b, TUNE, [n("F#4"), n("A4"), n("D5")], 60, 79, vel=56, attack=0.5)
    march(b, 8, 60)

    # bars 20-27: the theme an octave up
    c = p.bar(20)
    sus.note(c - 4, I.SUS_SWELL_4S, 4, 70)
    flute.notes_of(c, theme(n("A5")), vel=88, attack=0.04)
    vln.notes_of(c, theme(n("A4")), vel=88, attack=0.06)
    pad(horn2, c, A, [n("C#3"), n("E3"), n("A3")], 48, 64, vel=72, attack=0.12)
    pad(vln2, c, A, [n("C#4"), n("E4")], 55, 72, vel=62, attack=0.3)
    running(c, A, 68)
    walk(c, A, 74)
    broken(harp, c, A, n("A2"), [0, 1, 2, 3, 4, 3, 2, 1], 0.5, vel=62)
    march(c, 8, 68)
    timp.line(c, "A2:2 r:2 | B2:2 r:2 | F#2:2 E2:2 | E2:4 | A2:2 r:2 | F2:2 r:2 | G2:2 r:2 | A2:2 E2:2", vel=78)

    # bars 28-31: the head turned over
    d = p.bar(28)
    vc.line(d, "F#3:1 C#4:1.5 B3:.5 C#4:1 | D4:4", vel=88, attack=0.05)
    horn.line(d + 8, "B3:1 F#4:1.5 E4:.5 F#4:1 | G#4:2 B4:2", vel=92, attack=0.04)
    pad(vln2, d, DEV, [n("A4"), n("C#5"), n("F#5")], 64, 84, vel=66, attack=0.25, swell=(-4.0, 2.0))
    pad(horn2, d, DEV, [n("C#3"), n("F#3"), n("A3")], 48, 64, vel=66, attack=0.15)
    running(d, DEV, 70)
    walk(d, DEV, 74)
    march(d, 4, 70)
    snare.line(p.bar(31, 2), " ".join(["C4:.25"] * 8), vel=64, transpose=I.SNARE_HIT - n("C4"))
    timp.line(d, "F#2:2 r:2 | D2:2 r:2 | B2:2 r:2 | E2:1 E2:1 E2:1 E2:1", vel=82)

    # bars 32-35: everyone, the theme's second half
    e = p.bar(32)
    sus.note(e, I.SUS_HIT, 6, 84)
    second = theme(n("A3"))[12:]
    horn.notes_of(e, second, vel=100, attack=0.03)
    tpt.notes_of(e, [(k + 12, b) for k, b in second], vel=94, attack=0.03)
    vln.notes_of(e, [(k + 12, b) for k, b in second], vel=94, attack=0.04)
    flute.notes_of(e, [(k + 24, b) for k, b in second], vel=84, attack=0.04)
    pad(horn2, e, GRAND, [n("C#3"), n("E3"), n("A3")], 48, 64, vel=84, attack=0.06)
    pad(vln2, e, GRAND, [n("E4"), n("A4"), n("C#5")], 60, 79, vel=76, attack=0.1)
    pad(vc, e, GRAND, [n("A2"), n("E3")], 40, 60, vel=80, attack=0.1)
    running(e, GRAND, 74)
    walk(e, GRAND, 80)
    broken(harp, e, GRAND, n("A2"), [0, 1, 2, 3, 4, 5, 4, 3], 0.5, vel=68)
    march(e, 4, 74)
    timp.line(e, "A2:1.5 A2:.5 E2:2 | F2:1.5 F2:.5 C3:2 | G2:1.5 G2:.5 D2:2 | A2:2 E2:1 A2:1", vel=86)
    return p
