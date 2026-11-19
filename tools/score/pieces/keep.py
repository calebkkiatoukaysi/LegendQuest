"""
"Sunspire", for Sunspire Keep: the theme as a royal chorale, in B flat major with the old modal
chords (A flat, the flat seventh; G flat, the flat sixth), 76 BPM, 28 bars.

    bars  0-3   the fanfare: trumpets call the theme's head, the horns answer it on the dominant,
                the trombones lay down E flat and F under a timpani roll
    bars  4-11  a trumpet plays the theme over the brass in chorale, the organ softly under them
                and the harp running up each chord
    bars 12-19  the violins take it, the harmony darkened (C minor, G minor), a choir and the
                brass soft beneath
    bars 20-27  everyone: trumpets and violins together, the organ full, the timpani and a cymbal,
                home to B flat and round to the fanfare
"""

from common import bass, broken, pad
from score import Piece
from theory import n, theme
import instruments as I

FANFARE = "Bb:4 | F:4 | Eb:4 | Fsus4:2 F:2"
A = "Bb:4 | Eb:4 | Gm:2 F:2 | Fsus4:2 F:2 | Bb:4 | Gb:4 | Ab:4 | Bb:4"
B = "Bb:4 | Cm:4 | Gm:2 F:2 | F:4 | Gm:4 | Gb:4 | Ab:4 | Bb:4"


def build():
    p = Piece("sunspire", bpm=76, meter=4, bars=28, lufs=-20.0, title="Sunspire",
              hall=dict(rt60=3.2, predelay=0.035, hf_rt=0.4), wet=-3.0)
    tpt = p.part("trumpet", "tpt", pan=0.1, send=0.4, gain=0.0)
    tpt2 = p.part("trumpets 2-3", "tpt", pan=0.2, send=0.42, gain=-4.0)
    horn = p.part("horns", "horn", pan=-0.25, send=0.45, gain=-2.0)
    tbn = p.part("trombones", "tbn", pan=0.3, send=0.45, gain=-3.0)
    tuba = p.part("tuba", "tuba", pan=0.35, send=0.4, gain=-6.0)
    organ = p.part("organ", "organ_soft", pan=0.0, send=0.5, gain=-12.0, width=1.3)
    organ_full = p.part("organ full", "organ", pan=0.0, send=0.5, gain=-11.0, width=1.3)
    pedal = p.part("organ pedal", "organ_pedal", pan=0.0, send=0.45, gain=-12.0)
    harp = p.part("harp", "harp", pan=-0.35, send=0.5, gain=-5.0)
    vln = p.part("violins", "vln", pan=-0.25, send=0.4, gain=-1.0, delay_ms=25)
    vln2 = p.part("violins II", "vln", pan=-0.05, send=0.4, gain=-6.0, delay_ms=25)
    vla = p.part("violas", "vla", pan=0.15, send=0.4, gain=-7.0, delay_ms=25)
    vc = p.part("cellos", "vc", pan=0.3, send=0.35, gain=-5.0, delay_ms=20)
    choir = p.part("choir", "choir", pan=0.0, send=0.6, gain=-9.0, width=1.4)
    timp = p.part("timpani", "timp", pan=0.0, send=0.35, gain=-4.0)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.4, gain=-6.0)
    sus = p.part("suspended cymbal", "sus_cymbal", pan=0.2, send=0.5, gain=-10.0)
    bells = p.part("bells", "bells", pan=-0.4, send=0.7, gain=-16.0)

    # bars 0-3: the fanfare
    f = p.bar(0)
    tpt.line(f, "Bb4:1> F5:1.5 Eb5:.5 F5:1", vel=96, attack=0.03)
    tpt2.line(f, "D4:1> D5:1.5 Bb4:.5 D5:1", vel=86, attack=0.03)
    horn.line(f + 4, "F3+C4:1> C4+A4:1.5 Bb3+G4:.5 C4+A4:1", vel=92, attack=0.04)
    pad(tbn, f + 8, "Eb:4 | Fsus4:2 F:2", [n("Eb3"), n("G3"), n("Bb3")], 46, 64, vel=86, attack=0.06)
    tuba.line(f + 8, "Eb2:4 | F2:4", vel=84)
    roll.note(f + 8, n("F2"), 8, 70, swell=(-12.0, 0.0))
    timp.line(f, "Bb2:1> r:3 | F2:1> r:3", vel=88)
    bells.line(f, "Bb4:4 | F4:4", vel=70)

    # bars 4-11: the trumpet's theme over the chorale
    a = p.bar(4)
    tpt.notes_of(a, theme(n("Bb4")), vel=92, attack=0.04)
    pad(horn, a, A, [n("D3"), n("F3"), n("Bb3")], 48, 67, vel=74, attack=0.12)
    pad(tbn, a, A, [n("Bb2"), n("F3")], 42, 62, vel=72, attack=0.1)
    bass(tuba, a, A, n("Bb1"), vel=74)
    pad(organ, a, A, [n("D4"), n("F4"), n("Bb4")], 60, 77, vel=70, attack=0.3)
    broken(harp, a, A, n("Bb2"), [0, 1, 2, 3, 4, 5, 6, 7], 0.5, vel=60, accent=8)
    timp.line(a, "Bb2:2 r:2 | Eb3:2 r:2 | G2:2 F2:2 | F2:4 | Bb2:2 r:2 | r:4 | r:4 | Bb2:2 F2:2", vel=74)

    # bars 12-19: the violins' theme, darker
    b = p.bar(12)
    vln.notes_of(b, theme(n("Bb4")), vel=90, attack=0.08)
    vln2.notes_of(b, theme(n("Bb3")), vel=82, attack=0.08)
    pad(vla, b, B, [n("D4"), n("F4")], 55, 72, vel=68, attack=0.25)
    bass(vc, b, B, n("Bb2"), vel=76)
    pad(choir, b, B, [n("F3"), n("Bb3"), n("D4")], 50, 68, vel=66, attack=0.6)
    pad(horn, b, B, [n("D3"), n("F3"), n("Bb3")], 48, 67, vel=60, attack=0.2)
    bass(tuba, b, B, n("Bb1"), vel=62)
    broken(harp, b, B, n("Bb2"), [0, 2, 4, 6, 7, 6, 4, 2], 0.5, vel=58, accent=8)
    sus.note(p.bar(19), I.SUS_SWELL_4S, 4, 76)
    roll.note(p.bar(19), n("F2"), 4, 66, swell=(-14.0, 0.0))

    # bars 20-27: everyone
    c = p.bar(20)
    sus.note(c, I.SUS_HIT, 6, 86)
    tpt.notes_of(c, theme(n("Bb4")), vel=100, attack=0.03)
    vln.notes_of(c, theme(n("Bb4")), vel=96, attack=0.05)
    vln2.notes_of(c, theme(n("Bb3")), vel=88, attack=0.05)
    pad(tpt2, c, A, [n("D4"), n("F4")], 55, 72, vel=80, attack=0.05)
    pad(horn, c, A, [n("D3"), n("F3"), n("Bb3")], 48, 67, vel=86, attack=0.06)
    pad(tbn, c, A, [n("Bb2"), n("F3")], 42, 62, vel=84, attack=0.05)
    bass(tuba, c, A, n("Bb1"), vel=86)
    bass(vc, c, A, n("Bb2"), vel=86)
    pad(organ_full, c, A, [n("D4"), n("F4"), n("Bb4")], 60, 77, vel=80, attack=0.15)
    bass(pedal, c, A, n("Bb1"), vel=80)
    pad(choir, c, A, [n("F3"), n("Bb3"), n("D4")], 50, 68, vel=80, attack=0.4)
    broken(harp, c, A, n("Bb2"), [0, 1, 2, 3, 4, 5, 6, 7], 0.5, vel=66, accent=8)
    timp.line(c, "Bb2:1.5> Bb2:.5 F2:2 | Eb3:1.5> Eb3:.5 Bb2:2 | G2:2 F2:2 | F2:1 F2:1 F2:1 F2:1"
                 " | Bb2:2> F2:2 | Gb2:2> Db3:2 | Ab2:2> Eb3:2 | Bb2:2> F2:1 Bb2:1", vel=84)
    bells.line(p.bar(27), "Bb4:4", vel=66)
    return p
