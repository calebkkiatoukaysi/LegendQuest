"""
The one-shots: the stinger as a fight starts, and the jingles when it ends. None of them loops;
each ends in silence once the hall has died away.

    engage    D minor, the battle's key and tempo (150 BPM): the strings rip up an octave and a
              half over a timpani roll and a tom ruff, and on the next downbeat the brass hit
              D minor with the timpani and a cymbal. About 2.6 s; the hit lands at 1.6 s.
    victory   D major at 112 BPM, the battle's D minor turned to major: the brass throw the theme's
              head out, then climb the flat-six, flat-seven, one cadence (B flat, C, D) to a held D
              major chord with the timpani rolling, a cymbal and a harp sweep. About 6 s.
    defeat    D minor, slow (54 BPM): a solo cello sings the theme's head falling instead of rising
              over low horns and a choir sighing, the harmony sinking D minor, B flat, G minor to a
              bare D. About 7 s.
"""

from score import Piece
from theory import n
import instruments as I


def engage():
    p = Piece("engage", bpm=150, meter=4, bars=1, loop=False, tail=1.05, lufs=-17.0, title="Engage",
              hall=dict(rt60=1.6, predelay=0.015), wet=-6.0)
    vln = p.part("violins", "vln_spic", pan=-0.25, send=0.25, gain=-2.0, human_ms=2)
    vla = p.part("violas", "vla_spic", pan=0.15, send=0.25, gain=-4.0, human_ms=2)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.3, gain=-2.0)
    timp = p.part("timpani", "timp", pan=0.0, send=0.3, gain=0.0)
    tom = p.part("toms", "tom", pan=0.15, send=0.2, gain=-4.0, human_ms=2)
    kick = p.part("bass drum", "bass_drum", pan=0.0, send=0.2, gain=0.0,
                  eq=[("highpass", 42.0, 0.7, 0.0), ("peak", 58.0, 1.0, -4.0)])
    horn = p.part("horns", "horn_stac", pan=-0.2, send=0.3, gain=0.0, human_ms=2)
    tpt = p.part("trumpets", "tpt_stac", pan=0.15, send=0.3, gain=-1.0, human_ms=2)
    tbn = p.part("trombones", "tbn_stac", pan=0.3, send=0.3, gain=0.0, human_ms=2)
    low = p.part("low strings", "cb_spic", pan=0.35, send=0.2, gain=0.0)
    cym = p.part("cymbal", "cymbal", pan=0.25, send=0.4, gain=-4.0)
    # the rip: sixteenths up D minor's scale through beats 2-4, crescendo
    run = "D4 E4 F4 G4 A4 Bb4 C5 D5 E5 F5 G5 A5".split()
    for i, k in enumerate(run):
        vln.note(1.0 + i * 0.25, n(k), 0.25, 62 + 4 * i, legato=0.9)
        vla.note(1.0 + i * 0.25, n(k) - 12, 0.25, 58 + 4 * i, legato=0.9)
    roll.note(0.0, n("D2"), 4.0, 64, swell=(-18.0, 2.0))
    for i, t in enumerate([2.5, 2.75, 3.0, 3.25, 3.5, 3.75]):
        tom.note(t, I.TOM_HIT, 0.5, 70 + 7 * i)
    # the hit, on the next downbeat
    hit = 4.0
    timp.note(hit, n("D2"), 2.0, 120)
    kick.note(hit, I.BASS_DRUM_HIT, 2.0, 120)
    low.note(hit, n("D2"), 1.0, 118)
    horn.chord(hit, [n("D3"), n("A3"), n("D4"), n("F4")], 1.0, 120)
    tpt.chord(hit, [n("A4"), n("D5"), n("F5")], 1.0, 118)
    tbn.chord(hit, [n("D2"), n("D3"), n("A3")], 1.0, 120)
    cym.note(hit, I.CYMBAL_CRASH, 3.0, 118)
    return p


def victory():
    p = Piece("victory", bpm=112, meter=4, bars=2, loop=False, tail=2.2, lufs=-17.5, title="Victory",
              hall=dict(rt60=2.2, predelay=0.02), wet=-4.0)
    horn = p.part("horns", "horn", pan=-0.2, send=0.35, gain=0.0)
    horn2 = p.part("horns 3-4", "horn", pan=-0.3, send=0.35, gain=-3.0)
    tpt = p.part("trumpets", "tpt", pan=0.15, send=0.35, gain=0.0)
    tbn = p.part("trombones", "tbn", pan=0.3, send=0.35, gain=-2.0)
    tuba = p.part("tuba", "tuba", pan=0.35, send=0.3, gain=-5.0)
    vln = p.part("violins", "vln", pan=-0.25, send=0.35, gain=-3.0, delay_ms=20)
    vc = p.part("cellos", "vc", pan=0.3, send=0.3, gain=-4.0, delay_ms=20)
    harp = p.part("harp", "harp", pan=-0.35, send=0.5, gain=-4.0)
    timp = p.part("timpani", "timp", pan=0.0, send=0.3, gain=-2.0)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.3, gain=-3.0)
    cym = p.part("cymbal", "sus_cymbal", pan=0.2, send=0.45, gain=-6.0)
    glock = p.part("glockenspiel", "glock", pan=0.3, send=0.5, gain=-8.0)
    # bar 1: the head, D major, called out
    tpt.line(0, "D5:.5> D5:.5 A5:1.5 G5:.5 A5:1", vel=104, attack=0.02)
    horn.line(0, "D4:.5> D4:.5 A4:1.5 G4:.5 A4:1", vel=100, attack=0.02)
    horn2.line(0, "A3:1 F#4:1.5 E4:.5 F#4:1", vel=90, attack=0.02)
    tbn.line(0, "D3+A3:3' D3+A3:1", vel=92, attack=0.03)
    timp.line(0, "D2:1> r:1 A2:.5 A2:.5 D2:1", vel=96)
    # bar 2: flat six, flat seven, one
    tpt.line(4, "Bb5:1> C6:1 D6:4", vel=108, attack=0.02, release=0.8)
    horn.line(4, "F4:1> G4:1 A4+D5:4", vel=104, attack=0.02, release=0.8)
    horn2.line(4, "D4:1 E4:1 F#4:4", vel=96, attack=0.02, release=0.8)
    tbn.line(4, "Bb2+F3:1> C3+G3:1 D3+A3:4", vel=100, attack=0.02, release=0.8)
    tuba.line(4, "Bb1:1 C2:1 D2:4", vel=96, attack=0.03, release=0.8)
    vln.line(4, "D5+F5:1 E5+G5:1 F#5+A5+D6:4", vel=96, attack=0.05, release=0.8)
    vc.line(4, "Bb2:1 C3:1 D3:4", vel=94, attack=0.05, release=0.8)
    roll.note(6, n("D2"), 4, 92, swell=(-10.0, 0.0))
    cym.note(6, I.SUS_HIT, 6, 100)
    timp.note(6, n("D2"), 1, 110)
    harp.chord(6, [n("D3"), n("A3"), n("D4"), n("F#4"), n("A4"), n("D5"), n("F#5"), n("A5"), n("D6")], 6, 92, roll=0.08)
    glock.line(6, "D6:.5 F#6:.5 A6:.5 D7:4", vel=84)
    return p


def defeat():
    p = Piece("defeat", bpm=72, meter=4, bars=2, loop=False, tail=1.0, lufs=-21.0, title="Defeat",
              hall=dict(rt60=3.0, predelay=0.03, hf_rt=0.35), wet=-3.0)
    cello = p.part("solo cello", "vc", pan=0.1, send=0.45, gain=0.0, delay_ms=25)
    horn = p.part("horns", "horn", pan=-0.25, send=0.5, gain=-6.0)
    cb = p.part("basses", "cb", pan=0.3, send=0.4, gain=-6.0, delay_ms=20)
    choir = p.part("choir", "choir_oo", pan=0.0, send=0.6, gain=-6.0, width=1.3)
    harp = p.part("harp", "harp", pan=-0.3, send=0.6, gain=-8.0)
    timp = p.part("timpani", "timp", pan=0.0, send=0.4, gain=-8.0)
    # the head, falling: D, the A a fourth below, B flat, A ... onto the low D
    cello.line(0, "D4:.75 A3:1.25 Bb3:.5 A3:.75 G3:.25 | F3:.5 E3:.5 D3:2", vel=88, attack=0.15, release=1.6)
    horn.line(0, "F3+A3:1 F3+Bb3:1 D3+G3:1 A2+E3:1 | A2+D3:2", vel=70, attack=0.3, release=1.8)
    cb.line(0, "D2:1 Bb1:1 G1:1 A1:1 | D2:2", vel=72, attack=0.2, release=1.8)
    choir.line(0, "A3+D4:1 Bb3+D4:1 Bb3+D4:1 A3+C#4:1 | A3+D4:2", vel=70, attack=0.4, release=2.0)
    timp.note(0, n("D2"), 2, 70)
    timp.note(4, n("D2"), 2, 60)
    harp.chord(4, [n("D2"), n("A2"), n("D3"), n("F3")], 2, 66, roll=0.12)
    return p
