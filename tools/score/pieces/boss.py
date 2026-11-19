"""
"The Hunger", for boss fights: the theme bent out of shape, in D Phrygian (the flat second, E flat,
grinding on D) with the tritone, A flat, where the theme's fifth should be. 132 BPM, 56 bars;
heavier and darker than Steel and in the same D, so the stinger serves both.

The cellos and basses grind eighths on D that catch on the E flat above; the bass drum and the
toms pound a half-time beat under them, and the timpani answer D with A flat.

    bars  0-3   the grind alone, a low choir rising out of it
    bars  4-11  the low brass in octaves (horns, trombones, tuba): the theme bent, D up to A flat,
                over D minor, B flat minor and A flat
    bars 12-19  the trumpets on top, a gong, the violins shivering high
    bars 20-27  the organ and the choir: chords a third apart that shouldn't follow each other
                (D minor, B flat minor, F sharp minor, E flat minor, B minor), the violins in
                sixteenths, the horns stabbing
    bars 28-35  half time: the bass falls a semitone at a time from D to A under the brass while
                the bells toll D and A flat
    bars 36-43  everyone: the bent theme with the trumpets high, cymbals
    bars 44-51  a trumpet tries the theme as it should be, in D major, and the low brass answer it
                bent; the violins trill a semitone apart
    bars 52-55  the grind alone, round to the top
"""

from common import broken, chart, drums, pad
from score import Piece
from theory import bass_note, n
import instruments as I

# the theme bent: (semitones above D, beats); the fifth is the tritone, the third and sixth flat
BENT = [(0, 1), (6, 1.5), (5, 0.5), (6, 1), (8, 3), (6, 0.5), (3, 0.5), (3, 1), (0, 1), (1, 1.5), (3, 0.5),
        (6, 4), (0, 1), (6, 1.5), (5, 0.5), (6, 1), (12, 3), (10, 0.5), (8, 0.5), (10, 1.5), (8, 0.5),
        (6, 1), (1, 1), (0, 4)]
A = "Dm:4 | Bbm:4 | Dm:2 Eb:2 | Ab:4 | Dm:4 | Bb:4 | Cm:2 Ab:2 | Dm:4"
MEDIANTS = "Dm:4 | Bbm:4 | F#m:4 | Dm:4 | Ebm:4 | Bm:4 | Gm:4 | A:4"
FALL = "Dm:4 | A/C#:4 | Dm/C:4 | G/B:4 | Gm/Bb:4 | A:4 | A:4 | A:4"
INTRO = "Dm:16"

KICK = "X.......x.....x.|X.......x...x.x."
TOMS = "....x.......x...|....x.....x.xxxx"


def bent(tonic):
    return [(tonic + s, b) for s, b in BENT]


def grind(part, start, text, near, vel):
    """Eighths on each chord's bass, catching on the semitone above every third note."""
    fig = [0, 0, 1, 0, 0, 1, 0, 12]
    acc = ">..>..>."
    for sym, beats, off in chart(text):
        root = bass_note(sym, near)
        for i in range(int(beats * 2)):
            part.note(start + off + i * 0.5, root + fig[i % 8], 0.5, vel + (14 if acc[i % 8] == ">" else 0),
                      legato=0.8)


def build():
    p = Piece("the-hunger", bpm=132, meter=4, bars=56, lufs=-18.5, title="The Hunger",
              hall=dict(rt60=2.6, predelay=0.025, hf_rt=0.4, lf_rt=1.3), wet=-4.5)
    vc = p.part("cellos", "vc_spic", pan=0.3, send=0.2, gain=0.0, human_ms=4)
    cb = p.part("basses", "cb_spic", pan=0.4, send=0.18, gain=-1.0, human_ms=4)
    vlnsp = p.part("violins spic", "vln_spic", pan=-0.25, send=0.25, gain=-6.0, human_ms=3)
    trem = p.part("violins trem", "vln_trem", pan=-0.25, send=0.35, gain=-8.0)
    vln = p.part("violins", "vln", pan=-0.3, send=0.35, gain=-4.0, delay_ms=20)
    horn = p.part("horns", "horn", pan=-0.2, send=0.35, gain=0.0)
    tbn = p.part("trombones", "tbn", pan=0.25, send=0.32, gain=0.0)
    tuba = p.part("tuba", "tuba", pan=0.35, send=0.28, gain=-2.0)
    tpt = p.part("trumpets", "tpt", pan=0.1, send=0.35, gain=-1.0)
    stabs = p.part("horn stabs", "horn_stac", pan=-0.25, send=0.35, gain=-3.0)
    tbnst = p.part("trombone stabs", "tbn_stac", pan=0.3, send=0.3, gain=-2.0)
    organ = p.part("organ", "organ", pan=0.0, send=0.5, gain=-11.0, width=1.3)
    choir = p.part("choir", "choir", pan=0.0, send=0.55, gain=-6.0, width=1.5)
    low_choir = p.part("low choir", "choir_oh", pan=0.0, send=0.55, gain=-6.0, width=1.3)
    kick = p.part("bass drum", "bass_drum", pan=0.0, send=0.25, gain=0.0, human_ms=3,
                  eq=[("highpass", 42.0, 0.7, 0.0), ("peak", 58.0, 1.0, -4.0)])
    tom = p.part("toms", "tom", pan=0.2, send=0.25, gain=-3.0, human_ms=3)
    timp = p.part("timpani", "timp", pan=0.0, send=0.3, gain=-1.0, human_ms=3)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.35, gain=-4.0)
    gong = p.part("gong", "gong", pan=-0.15, send=0.5, gain=-8.0)
    cym = p.part("cymbals", "cymbal", pan=0.25, send=0.4, gain=-9.0)
    bells = p.part("bells", "bells", pan=0.35, send=0.6, gain=-10.0)

    D2, D3 = n("D2"), n("D3")

    def low(start, text, vel=80):
        grind(vc, start, text, D3, vel)
        grind(cb, start, text, D2, vel - 4)

    def pound(start, bars, vel=92):
        for k in range(0, bars, 2):
            drums(kick, start + 4 * k, KICK, I.BASS_DRUM_HIT, vel)
            drums(tom, start + 4 * k, TOMS, I.TOM_HIT, vel - 6)
            timp.note(start + 4 * k, D2, 2, vel + 6)
            timp.note(start + 4 * k + 3, n("Ab2"), 1, vel - 6)
            timp.note(start + 4 * k + 4, D2, 2, vel)
            timp.note(start + 4 * k + 6.5, n("Ab2"), 0.5, vel - 10)
            timp.note(start + 4 * k + 7, n("Ab2"), 1, vel - 2)

    # bars 0-3 and 52-55: the grind alone
    for s in (p.bar(0), p.bar(52)):
        low(s, INTRO, 78)
        pound(s, 4, 86)
        low_choir.chord(s, [n("D3"), n("A3")], 8, 60, attack=2.0)
        low_choir.chord(s + 8, [n("Eb3"), n("Bb3")], 8, 66, attack=1.0)
    roll.note(p.bar(55), D2, 4, 74, swell=(-14.0, 2.0))

    # bars 4-11: the low brass, the theme bent
    a = p.bar(4)
    gong.note(a, I.GONG_HIT, 8, 100)
    horn.notes_of(a, bent(n("D4")), vel=98, attack=0.03)
    tbn.notes_of(a, bent(n("D3")), vel=98, attack=0.03)
    tuba.notes_of(a, bent(n("D2")), vel=92, attack=0.04)
    low(a, A, 82)
    pad(choir, a, A, [n("D4"), n("F4"), n("A4")], 58, 74, vel=64, attack=0.5)
    pound(a, 8, 90)

    # bars 12-19: the trumpets on top
    b = p.bar(12)
    gong.note(b, I.GONG_HIT, 8, 106)
    cym.note(b, I.CYMBAL_CRASH, 4, 100)
    tpt.notes_of(b, bent(n("D5")), vel=100, attack=0.03)
    horn.notes_of(b, bent(n("D4")), vel=100, attack=0.03)
    tbn.notes_of(b, bent(n("D3")), vel=100, attack=0.03)
    tuba.notes_of(b, bent(n("D2")), vel=94, attack=0.04)
    trem.chord(b, [n("D6"), n("Eb6")], 32, 58, attack=0.5, swell=(-6.0, 0.0))
    low(b, A, 86)
    pad(choir, b, A, [n("D4"), n("F4"), n("A4")], 58, 74, vel=74, attack=0.4)
    pound(b, 8, 94)

    # bars 20-27: the mediants
    c = p.bar(20)
    pad(organ, c, MEDIANTS, [n("D4"), n("F4"), n("A4")], 58, 76, vel=80, attack=0.1)
    pad(choir, c, MEDIANTS, [n("D4"), n("F4"), n("A4")], 58, 76, vel=82, attack=0.3)
    pad(tbn, c, MEDIANTS, [n("D3"), n("A3")], 43, 62, vel=86, attack=0.05)
    pad(tuba, c, MEDIANTS, [n("D2")], 31, 45, vel=84, attack=0.05)
    broken(vlnsp, c, MEDIANTS, n("D5"), [0, 1, 2, 1], 0.25, vel=74, accent=12)
    for i, (sym, beats, off) in enumerate(chart(MEDIANTS)):
        stabs.chord(c + off + 1.5, [bass_note(sym, n("D4")), bass_note(sym, n("D4")) + 7], 0.5, 100)
        stabs.chord(c + off + 3, [bass_note(sym, n("D4")), bass_note(sym, n("D4")) + 7], 0.5, 92)
    low(c, MEDIANTS, 84)
    pound(c, 8, 90)

    # bars 28-35: half time, the falling bass
    d = p.bar(28)
    gong.note(d, I.GONG_HIT, 8, 96)
    tbn.line(d, "D3+A3:4 | C#3+A3:4 | C3+A3:4 | B2+G3:4 | Bb2+G3:4 | A2+E3:4 | A2+E3:4 | A2+C#3+E3:4",
             vel=96, attack=0.04)
    tuba.line(d, "D2:4 | C#2:4 | C2:4 | B1:4 | Bb1:4 | A1:4 | A1:4 | A1:4", vel=94, attack=0.05)
    horn.line(d, "F4:4 | E4:4 | F4:4 | D4:4 | D4:4 | C#4:4 | E4:4 | E4:4", vel=90, attack=0.05)
    pad(choir, d, FALL, [n("F4"), n("A4"), n("D5")], 60, 78, vel=80, attack=0.3)
    vln.line(d, "D5:8 | E5:4 F5:4 | D5:4 C#5:4 | E5:8", vel=84, attack=0.15)
    for k in range(8):
        bells.note(d + 4 * k, n("D4") if k % 2 == 0 else n("Ab4"), 4, 82)
        kick.note(d + 4 * k, I.BASS_DRUM_HIT, 2, 104)
        timp.note(d + 4 * k, D2 if k < 5 else n("A2"), 2, 98)
    roll.note(p.bar(34), n("A2"), 8, 80, swell=(-12.0, 3.0))
    grind(cb, d, FALL, D2, 74)

    # bars 36-43: everyone
    e = p.bar(36)
    gong.note(e, I.GONG_HIT, 8, 110)
    cym.note(e, I.CYMBAL_CRASH, 4, 108)
    tpt.notes_of(e, bent(n("D5")), vel=108, attack=0.02)
    horn.notes_of(e, bent(n("D4")), vel=106, attack=0.02)
    tbn.notes_of(e, bent(n("D3")), vel=104, attack=0.02)
    tuba.notes_of(e, bent(n("D2")), vel=98, attack=0.03)
    broken(vlnsp, e, A, n("D5"), [0, 1, 2, 1], 0.25, vel=78, accent=12)
    pad(choir, e, A, [n("D4"), n("F4"), n("A4")], 58, 74, vel=86, attack=0.3)
    pad(organ, e, A, [n("D4"), n("F4"), n("A4")], 58, 76, vel=84, attack=0.1)
    low(e, A, 90)
    pound(e, 8, 98)
    cym.note(p.bar(40), I.CYMBAL_CRASH, 4, 100)

    # bars 44-51: the theme as it should be, answered bent
    f = p.bar(44)
    tpt.line(f, "D5:1 A5:1.5 G5:.5 A5:1 | B5:3 r:1", vel=96, attack=0.03)
    tbnst.line(f + 8, "D3:1> Ab3:1.5 G3:.5 Ab3:1 | Bb3:3> r:1", vel=110)
    tuba.line(f + 8, "D2:1 Ab2:1.5 G2:.5 Ab2:1 | Bb2:3 r:1", vel=100, attack=0.03)
    tpt.line(f + 16, "D5:1 A5:1.5 G5:.5 A5:1 | B5:1.5 Bb5:1.5 r:1", vel=100, attack=0.03)
    tbnst.line(f + 24, "D3:1> Ab3:1.5 G3:.5 Ab3:1 | D3:3> r:1", vel=112)
    tuba.line(f + 24, "D2:1 Ab2:1.5 G2:.5 Ab2:1 | D2:3 r:1", vel=104, attack=0.03)
    for k in range(32):
        vlnsp.note(f + k * 1.0, n("D6"), 0.25, 70, legato=0.9)
        vlnsp.note(f + k * 1.0 + 0.25, n("Eb6"), 0.25, 64, legato=0.9)
        vlnsp.note(f + k * 1.0 + 0.5, n("D6"), 0.25, 66, legato=0.9)
        vlnsp.note(f + k * 1.0 + 0.75, n("Eb6"), 0.25, 62, legato=0.9)
    pad(choir, f, "D:8 | Dm:8 | D:8 | Dm:8", [n("D4"), n("F#4"), n("A4")], 58, 74, vel=72, attack=0.5)
    low(f, "Dm:32", 82)
    pound(f, 8, 92)
    return p
