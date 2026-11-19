"""
"Salt and Lanterns", for the Gull's Wager at sea: the theme as a shanty, in E Dorian (minor with a
major fourth chord, A, that keeps it from sounding lost), in 6/8 that rocks like a hull, a dotted
quarter at 56, 40 bars.

The folk harp rocks through each chord in eighths, the basses pluck the two big beats, a frame
drum knocks like the hull on a swell, and the sea rolls in and out under it all, ten waves a loop.
Over it:

    bars  0-3   the rocking alone, E minor and A
    bars  4-11  the clarinet plays the theme, up a fifth and back, lifting onto the Dorian C sharp
    bars 12-19  the recorder takes it an octave up, a clarinet and bassoon holding the chords
                under it like a squeezebox
    bars 20-27  the cellos sing a second tune in G major, the violins holding over them, and turn
                onto B, home's dominant
    bars 28-35  the violins and the recorder together on the theme, the horn holding under them
    bars 36-39  the rocking alone, round to the top

Beats here are eighth notes (six to a bar).
"""

import synth
from common import bass, broken, pad
from score import Piece
from theory import n
import instruments as I

INTRO = "Em:6 | A:6 | Em:6 | A:6"
A = "Em:6 | A:6 | G:6 | D:6 | Em:6 | C:6 | D:6 | Em:6"
B = "G:6 | D/F#:6 | Em:6 | C:6 | G:6 | D:6 | Am:6 | B7:6"

TUNE = "E4:2 B4:3 A4:1 | B4:2 C#5:3 B4:1 | G4:2 E4:1 F#4:2 G4:1 | A4:6 | E4:2 B4:3 A4:1 | E5:3 D5:2 C5:1 | D5:2 B4:1 A4:2 F#4:1 | E4:6"
CELLO_TUNE = "B3:2 D4:3 G4:1 | F#4:3 E4:2 D4:1 | E4:2 G4:3 B3:1 | C4:6 | B3:2 D4:3 G4:1 | A4:3 F#4:2 D4:1 | E4:2 C4:2 A3:2 | B3:6"


def build():
    p = Piece("salt-and-lanterns", bpm=168, meter=6, bars=40, lufs=-20.5, title="Salt and Lanterns",
              hall=dict(rt60=2.4, predelay=0.02, hf_rt=0.45), wet=-4.0)
    harp = p.part("folk harp", "folk_harp", pan=-0.35, send=0.35, gain=-2.0, human_ms=6)
    pizz = p.part("basses pizz", "cb_pizz", pan=0.35, send=0.3, gain=-5.0)
    frame = p.part("frame drum", "frame_drum", pan=0.1, send=0.3, gain=-8.0, human_ms=8)
    vc = p.part("cellos", "vc", pan=0.3, send=0.35, gain=-4.0, delay_ms=25)
    clar = p.part("clarinet", "clar", pan=-0.1, send=0.35, gain=-1.0)
    rec = p.part("recorder", "recorder", pan=0.15, send=0.35, gain=-1.0, human_ms=8)
    reeds = p.part("clarinet chords", "clar", pan=-0.2, send=0.4, gain=-9.0)
    bsn = p.part("bassoon", "bsn", pan=0.2, send=0.4, gain=-8.0)
    vln = p.part("violins", "vln", pan=-0.25, send=0.4, gain=-3.0, delay_ms=25)
    vln_pad = p.part("violins pad", "vln_soft", pan=-0.1, send=0.45, gain=-9.0, delay_ms=25)
    horn = p.part("horn", "horn", pan=-0.3, send=0.45, gain=-8.0)
    bells = p.part("ship's bell", "chimes", pan=0.4, send=0.7, gain=-18.0)

    def rock(start, text, vel=64):
        broken(harp, start, text, n("E3"), [0, 2, 3, 4, 3, 2], 1, vel=vel, accent=8)
        bass(pizz, start, text, n("E2"), vel=vel + 8, rhythm=[(0, 3, 4), (3, 3, -8)])

    def hull(start, bars, vel=74):
        for b in range(bars):
            frame.note(start + 6 * b, I.FRAME_LOW, 3, vel)
            frame.note(start + 6 * b + 3, I.FRAME_LOW_MUTED, 3, vel - 18)

    # bars 0-3 and 36-39: rocking alone
    for s in (p.bar(0), p.bar(36)):
        rock(s, INTRO, 60)
        hull(s, 4, 66)
        vc.note(s, n("E2"), 24, 50, attack=1.5, release=2.0, swell=(-6.0, 0.0))
    bells.line(p.bar(2), "E5:3 r:9", vel=70)

    # bars 4-11: the clarinet's theme
    a = p.bar(4)
    clar.line(a, TUNE, vel=86, attack=0.05)
    rock(a, A, 64)
    hull(a, 8)
    pad(vc, a, A, [n("E3"), n("B3")], 40, 62, vel=58, attack=0.8, swell=(-4.0, 0.0))

    # bars 12-19: the recorder, an octave up, the squeezebox under it
    b = p.bar(12)
    rec.line(b, TUNE, vel=86, transpose=12)
    pad(reeds, b, A, [n("G4"), n("B4")], 55, 74, vel=64, attack=0.15)
    pad(bsn, b, A, [n("E3")], 40, 55, vel=66, attack=0.1)
    rock(b, A, 66)
    hull(b, 8)

    # bars 20-27: the cellos' tune in G
    c = p.bar(20)
    vc.line(c, CELLO_TUNE, vel=90, attack=0.1)
    pad(vln_pad, c, B, [n("B4"), n("D5"), n("G5")], 64, 81, vel=60, attack=0.6)
    rock(c, B, 62)
    hull(c, 8, 70)

    # bars 28-35: the violins and the recorder together
    d = p.bar(28)
    vln.line(d, TUNE, vel=88, transpose=12, attack=0.05)
    rec.line(d, TUNE, vel=82, transpose=12)
    pad(horn, d, A, [n("G3"), n("B3")], 50, 64, vel=66, attack=0.2)
    pad(vc, d, A, [n("E3"), n("B3")], 40, 62, vel=64, attack=0.4)
    rock(d, A, 68)
    hull(d, 8, 78)
    bells.line(d + 6 * 7, "E5:6", vel=64)

    # the sea, ten waves a loop
    p.bed(synth.sea(p.length, waves=10, seed=41), gain=-24.0, send=0.15)
    return p
