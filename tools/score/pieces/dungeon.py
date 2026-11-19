"""
"Cold Stone", for barrows, quarries and vaults: hardly a tune, in C Phrygian (the flat second, D flat,
leaning on C), 60 BPM, 24 bars.

The basses and cellos hold C under everything; the chords above them move two bars at a time
between C minor and the flat-second's D flat, B flat minor and A flat, a choir of low voices
breathing them. Cold air moves through the stone, and water drips somewhere far off. Over it,
the theme's head turns up in pieces:

    bars  0-3   the C alone, the air and the water
    bars  4-7   a horn, far off: the head, its sixth flattened, falling to the flat second
    bars  8-13  a low harp, the violas answering in a phrase that sinks
    bars 14-17  the bells toll the head, slowly, over tremolo cellos
    bars 18-21  a flute, low, comes down the scale to C
    bars 22-23  the C alone again
"""

import synth
from common import pad
from score import Piece
from theory import n
import instruments as I

CHART = ("Cm:8 | Db/C:8 | Cm:8 | Bbm/C:8 | Ab/C:8 | Db/C:8 | Cm:8 | Fm/C:8 | Db/C:8 | Cm:8 "
         "| Bbm/C:8 | Cm:8")


def build():
    p = Piece("cold-stone", bpm=60, meter=4, bars=24, lufs=-23.0, title="Cold Stone",
              hall=dict(rt60=4.2, predelay=0.045, hf_rt=0.35, lf_rt=1.3), wet=-1.0)
    cb = p.part("basses", "cb", pan=0.25, send=0.5, gain=-2.0, delay_ms=20)
    vc = p.part("cellos", "vc", pan=0.35, send=0.5, gain=-6.0, delay_ms=20)
    vctrem = p.part("cellos trem", "vc_trem", pan=0.3, send=0.6, gain=-12.0)
    choir = p.part("choir", "choir_oo", pan=0.0, send=0.7, gain=-4.0, width=1.5)
    horn = p.part("horn", "horn", pan=-0.35, send=0.8, gain=-6.0, eq=[("lowpass", 2400.0, 0.7, 0.0)])
    vla = p.part("violas", "vla", pan=0.2, send=0.6, gain=-6.0, delay_ms=25)
    harp = p.part("harp", "harp", pan=-0.3, send=0.6, gain=-6.0)
    bells = p.part("bells", "bells", pan=0.3, send=0.75, gain=-10.0)
    flute = p.part("flute", "flute", pan=0.15, send=0.7, gain=-5.0)
    timp = p.part("timpani roll", "timp_roll", pan=0.0, send=0.6, gain=-12.0)
    gong = p.part("gong", "gong", pan=-0.1, send=0.7, gain=-16.0)
    kick = p.part("bass drum", "bass_drum", pan=0.0, send=0.7, gain=-12.0)

    # the C under everything, re-bowed every four bars, each bow swelling and easing
    for b in range(0, 24, 4):
        cb.note(p.bar(b), n("C2"), 16, 62, attack=1.5, release=2.5, swell=(-6.0, 0.0))
        vc.note(p.bar(b), n("G2"), 16, 52, attack=2.0, release=2.5, swell=(-8.0, -2.0))
    pad(choir, 0, CHART, [n("Eb3"), n("G3"), n("C4")], 48, 66, vel=58, attack=2.5, release=3.0, legato=1.05)

    # the horn, far off
    horn.line(p.bar(4), "C4:1 G4:1.5 F4:.5 G4:1 | Ab4:4 | G4:2 F4:1 Eb4:1 | Db4:4", vel=70, attack=0.12, release=1.5)
    # the low harp, the violas answering
    harp.line(p.bar(8), "C2:2 G2:2 | C3:4 | Db2:2 Ab2:2 | F3:4 | Bb1:2 F2:2 | Db3:4", vel=64)
    vla.line(p.bar(10), "C4:2 Db4:2 | C4:4 | Bb3:2 Ab3:2 | G3:4", vel=68, attack=0.4, release=2.0)
    # the bells toll the head over tremolo cellos
    bells.line(p.bar(14), "C5:2 G5:2 | F5:2 G5:2 | Ab5:4 | r:4", vel=72)
    pad(vctrem, p.bar(14), "Ab/C:8 | Db/C:8", [n("C3"), n("Eb3")], 45, 60, vel=56, attack=1.0, release=2.0,
        swell=(-10.0, 0.0))
    # the flute, low, down to C
    flute.line(p.bar(18), "G4:1 Ab4:1 G4:2 | F4:2 Eb4:2 | Db4:4 | C4:4", vel=62, attack=0.2, release=1.5)

    timp.note(p.bar(12), n("C2"), 8, 56, swell=(-14.0, 0.0))
    gong.note(p.bar(14), I.GONG_HIT, 8, 54)
    for b in (2, 9, 17, 22):
        kick.note(p.bar(b, 2), I.BASS_DRUM_HIT, 4, 58)

    # the place: cold air and water
    p.bed(synth.wind(p.length, gusts=3, seed=31, centre=380.0), gain=-30.0, send=0.2)
    p.bed(synth.drips(p.length, count=22, seed=37), gain=-27.0, send=0.9)
    return p
