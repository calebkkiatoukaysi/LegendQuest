"""
"Embers", the title theme: LegendQuest's theme stated in full, in D major, 80 BPM, 32 bars.

    bars  0-3   the harp spreading D, B flat and C over a held D, a soft choir, and the flute
                calling the theme's head from far off
    bars  4-11  the horns play the theme over the strings, the harp running on in eighths
    bars 12-19  the violins take it an octave up, the horns holding a line under them, the choir
                in, the trumpet joining for the flat-six, flat-seven, one cadence
    bars 20-23  the bridge: the head called in B flat (trumpet), answered in G minor (horns),
                climbed in E flat (violins), then C and A, the dominant, with the timpani rolling
    bars 24-27  everyone: the theme's second half, the brass in octaves, a cymbal on the downbeat
    bars 28-31  quiet again over the D: the glockenspiel echoes the head, and it goes round
"""

from common import bass, broken, pad
from score import Piece
from theory import n, theme
import instruments as I

THEME_CHART = "D:4 | G:4 | Bm:2 A:2 | Asus4:2 A:2 | D:4 | Bb:4 | C:4 | D:4"
HALO_CHART = "D:4 | Bb/D:4 | C/D:4 | D:4"
BRIDGE_CHART = "Bb:4 | Gm:4 | Eb:4 | C:2 A:2"
GRAND_CHART = "D:4 | Bb:4 | C:4 | D:4"


def build():
    p = Piece("embers", bpm=80, meter=4, bars=32, lufs=-19.0, title="Embers",
              hall=dict(rt60=2.8, predelay=0.03, hf_rt=0.4), wet=-4.0)
    harp = p.part("harp", "harp", pan=-0.35, send=0.45, human_ms=6)
    flute = p.part("flute", "flute", pan=0.25, send=0.55)
    horn = p.part("horn", "horn", pan=-0.2, send=0.4, gain=1.0)
    horn2 = p.part("horns 2-4", "horn", pan=-0.3, send=0.45, gain=-3.0)
    tpt = p.part("trumpet", "tpt", pan=0.15, send=0.4, gain=-1.0)
    tbn = p.part("trombones", "tbn", pan=0.3, send=0.4, gain=-3.0)
    vln = p.part("violins I", "vln", pan=-0.25, send=0.35, gain=0.0, delay_ms=25)
    vln2 = p.part("violins II", "vln", pan=-0.05, send=0.35, gain=-4.0, delay_ms=25)
    soft = p.part("violins soft", "vln_soft", pan=-0.15, send=0.45, gain=-6.0, delay_ms=25)
    vla = p.part("violas", "vla", pan=0.2, send=0.35, gain=-5.0, delay_ms=25)
    vc = p.part("cellos", "vc", pan=0.35, send=0.3, gain=-3.0, delay_ms=20)
    cb = p.part("basses", "cb", pan=0.45, send=0.25, gain=-6.0, delay_ms=20)
    choir = p.part("choir", "choir_oo", pan=0.0, send=0.6, gain=-8.0, width=1.4)
    choir_ah = p.part("choir ah", "choir", pan=0.0, send=0.6, gain=-9.0, width=1.4)
    glock = p.part("glockenspiel", "glock", pan=0.3, send=0.6, gain=-10.0)
    timp = p.part("timpani", "timp", pan=0.0, send=0.35, gain=-4.0, human_ms=4)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.35, gain=-6.0)
    cym = p.part("cymbal", "sus_cymbal", pan=0.2, send=0.5, gain=-9.0)

    D3, D2 = n("D3"), n("D2")

    # -- bars 0-3 and 28-31: the halo over the D ------------------------------------------------- #
    for start in (p.bar(0), p.bar(28)):
        broken(harp, start, HALO_CHART, n("D3"), [0, 1, 2, 3, 4, 5, 4, 2], 0.5, vel=50, accent=8)
        pad(soft, start, HALO_CHART, [n("A4"), n("D5"), n("F#5")], 64, 84, vel=44, attack=0.8, release=2.0)
        pad(choir, start, HALO_CHART, [n("F#3"), n("A3"), n("D4")], 50, 70, vel=42, attack=1.5, release=2.5)
        cb.note(start, D2, 16, 46, attack=1.0, release=2.0)
        vc.note(start, n("A2"), 16, 42, attack=1.0, release=2.0)
    flute.line(p.bar(1, 2), "D5:1 A5:1.5 G5:.5 | A5:1 B5:3", vel=58, attack=0.12)
    glock.line(p.bar(29), "D6:1 A6:1.5 G6:.5 A6:1 | r:4 | D6:1 A6:1.5 G6:.5 | B6:4", vel=70)
    flute.line(p.bar(30), "A5:2 F#5:2 | D5:4", vel=52, attack=0.15)

    # -- bars 4-11: the horns' theme ------------------------------------------------------------- #
    a = p.bar(4)
    horn.notes_of(a, theme(n("D4")), vel=88, attack=0.06)
    pad(vla, a, THEME_CHART, [n("F#3"), n("A3"), n("D4")], 50, 69, vel=66, attack=0.3)
    pad(soft, a, THEME_CHART, [n("A4"), n("D5")], 62, 78, vel=58, attack=0.5)
    bass(vc, a, THEME_CHART, D3, vel=72)
    bass(cb, a, THEME_CHART, D2, vel=68)
    broken(harp, a, THEME_CHART, n("D3"), [0, 2, 4, 5, 4, 2, 1, 3], 0.5, vel=60)
    timp.note(a, D2, 2, 78)
    timp.note(p.bar(11), D2, 2, 70)
    timp.note(p.bar(10, 2), n("A2"), 2, 64)

    # -- bars 12-19: the violins' theme, an octave up -------------------------------------------- #
    b = p.bar(12)
    vln.notes_of(b, theme(n("D5")), vel=92, attack=0.05)
    vln2.notes_of(b, theme(n("D4")), vel=82, attack=0.05)
    pad(horn2, b, THEME_CHART, [n("F#3"), n("A3"), n("D4")], 50, 67, vel=72, attack=0.15)
    pad(vla, b, THEME_CHART, [n("A3"), n("D4"), n("F#4")], 55, 72, vel=72, attack=0.2)
    pad(choir_ah, b, THEME_CHART, [n("A3"), n("D4"), n("F#4")], 55, 72, vel=70, attack=0.6)
    bass(vc, b, THEME_CHART, D3, vel=80, rhythm=[(0, 2, 0), (2, 2, -8)])
    bass(cb, b, THEME_CHART, D2, vel=76)
    broken(harp, b, THEME_CHART, n("D3"), [0, 2, 4, 6, 7, 6, 4, 2], 0.5, vel=64)
    # the trumpet joins for the cadence (theme bars 6-8)
    tpt.notes_of(p.bar(17), theme(n("D5"))[16:], vel=84, attack=0.05)
    tbn.line(p.bar(17), "Bb2+F3:4 | C3+G3:4 | D3+A3:4", vel=78, attack=0.08)
    roll.note(p.bar(18), n("A2"), 4, 60, swell=(-12.0, 0.0))
    timp.note(p.bar(19), D2, 2, 92)
    timp.note(p.bar(19, 3), n("A2"), 1, 74)

    # -- bars 20-23: the bridge ------------------------------------------------------------------ #
    c = p.bar(20)
    tpt.line(c, "Bb4:1> F5:1.5 Eb5:.5 F5:1", vel=90, attack=0.04)
    horn.line(c + 4, "G4:1> D5:1.5 C5:.5 D5:1", vel=92, attack=0.04)
    vln.line(c + 8, "Eb5:1 Bb5:1.5 Ab5:.5 Bb5:1 | C6:2 C#6:2", vel=92, attack=0.04)
    vln2.line(c + 8, "Bb4:1 G5:1.5 F5:.5 G5:1 | G5:2 A5:2", vel=84, attack=0.04)
    pad(vla, c, BRIDGE_CHART, [n("D4"), n("F4"), n("Bb4")], 55, 74, vel=74, attack=0.2)
    pad(horn2, c, BRIDGE_CHART, [n("D3"), n("F3"), n("Bb3")], 48, 67, vel=70, attack=0.12)
    pad(choir_ah, c, BRIDGE_CHART, [n("D4"), n("F4"), n("Bb4")], 55, 74, vel=74, attack=0.5)
    bass(vc, c, BRIDGE_CHART, n("Bb2"), vel=82, rhythm=[(0, 1, 6), (1, 1, -6), (2, 1, 0), (3, 1, -6)])
    bass(cb, c, BRIDGE_CHART, n("Bb1"), vel=78)
    broken(harp, c, BRIDGE_CHART, n("Bb2"), [0, 1, 2, 3, 4, 5, 6, 7], 0.5, vel=66)
    timp.line(c, "Bb2:2 r:2 | G2:2 r:2 | Bb2:2 r:2 | C3:2 A2:2", vel=78)
    roll.note(p.bar(23), n("A2"), 4, 72, swell=(-14.0, 2.0))
    cym.note(p.bar(23), I.SUS_SWELL_2S, 4, 82)

    # -- bars 24-27: everyone -------------------------------------------------------------------- #
    d = p.bar(24)
    second = theme(n("D4"))[12:]
    horn.notes_of(d, second, vel=110, attack=0.03)
    tpt.notes_of(d, [(k + 12, b) for k, b in second], vel=108, attack=0.03)
    vln.notes_of(d, [(k + 12, b) for k, b in second], vel=98, attack=0.03)
    vln2.notes_of(d, second, vel=90, attack=0.03)
    pad(horn2, d, GRAND_CHART, [n("F#3"), n("A3"), n("D4")], 50, 67, vel=86, attack=0.08)
    pad(tbn, d, GRAND_CHART, [n("D3"), n("A3")], 45, 62, vel=86, attack=0.05)
    pad(vla, d, GRAND_CHART, [n("A3"), n("D4"), n("F#4")], 55, 74, vel=82, attack=0.1)
    pad(choir_ah, d, GRAND_CHART, [n("A3"), n("D4"), n("F#4")], 55, 74, vel=84, attack=0.3)
    bass(vc, d, GRAND_CHART, D3, vel=90, rhythm=[(0, 1.5, 6), (1.5, 0.5, -10), (2, 2, 0)])
    bass(cb, d, GRAND_CHART, D2, vel=86, rhythm=[(0, 1.5, 6), (1.5, 0.5, -10), (2, 2, 0)])
    broken(harp, d, GRAND_CHART, n("D3"), [0, 2, 4, 6, 7, 6, 4, 2], 0.5, vel=70)
    cym.note(d, I.SUS_HIT, 6, 92)
    timp.line(d, "D2:1.5> D2:.5 A2:1 D2:1 | Bb2:1.5> Bb2:.5 F2:2 | C3:1.5> C3:.5 G2:2 | D2:2> A2:1 D2:1", vel=84)
    return p
