"""
"Hearthsmoke", for towns, lit maps and houses: the theme as a country dance, in G major leaning
Mixolydian (the F natural, the flat seventh, is what makes it sound old and homely), in three at
116 BPM, 48 bars.

The strumstick (a little fretted folk lute) strums oom-pah-pah, the cellos and basses pluck the
downbeats and a frame drum keeps the step. Over it:

    bars  0-3   the dance alone
    bars  4-11  the recorder plays the theme, re-cut as a waltz: up a fifth and back, lifting to
                the sixth, and down from the octave through the F natural to G
    bars 12-19  again, with the folk harp running eighths and the violins holding a line under it
    bars 20-27  a second tune on the solo violin, in E minor, ending on D to turn back
    bars 28-35  the clarinet takes the second tune, the recorder answering over it
    bars 36-43  everyone: recorder and violins on the theme, the clarinet a third under, the
                tambourine shaking
    bars 44-47  the dance alone, C, G, F (the flat seventh again) and D, round to the top
"""

from common import bass, broken, chart
from score import Piece
from theory import chord, nearest, n
import instruments as I

INTRO = "G:3 | C:3 | G:3 | D:3"
A = "G:3 | C:3 | Am:3 | D:3 | G:3 | Em:3 | F:3 | G:3"
B = "Em:3 | C:3 | D:3 | G:3 | Em:3 | Am:3 | D7:3 | D:3"
OUTRO = "C:3 | G:3 | F:3 | D:3"

TUNE_A = "G4:1 D5:1.5 C5:.5 | D5:1 E5:2 | C5:1 A4:1 B4:.5 C5:.5 | D5:3 | G4:1 D5:1.5 C5:.5 | D5:1 G5:2 | F5:1 D5:1 C5:.5 A4:.5 | G4:3"
UNDER_A = "B4:3 | C5:3 | C5:3 | A4:3 | B4:3 | B4:3 | A4:3 | B4:3"
THIRDS_A = "E4:1 B4:1.5 A4:.5 | B4:1 C5:2 | A4:1 F#4:1 G4:.5 A4:.5 | F#4:3 | E4:1 B4:1.5 A4:.5 | B4:1 E5:2 | D5:1 A4:1 A4:.5 F4:.5 | D4:3"
TUNE_B = "B4:1 E5:1.5 D5:.5 | C5:1 E5:1 G5:1 | F#5:1.5 E5:.5 D5:1 | D5:3 | B4:1 E5:1.5 D5:.5 | C5:1 A4:1 C5:1 | D5:1 E5:.5 D5:.5 C5:1 | A4:3"
ANSWER_B = "r:3 | G5:1.5 E5:1.5 | r:3 | B5:1 A5:1 G5:1 | r:3 | E5:1.5 C5:1.5 | r:3 | F#5:1 E5:1 D5:1"


def oom_pah_pah(strum, pluck, start, text, vel=70):
    """Plucked bass on one, the chord strummed on two and three (a light roll, low to high)."""
    for sym, beats, off in chart(text):
        root, tones, bass_pc = chord(sym)
        pluck.note(start + off, nearest(bass_pc, n("G2")), 1, vel + 6)
        voicing = sorted(nearest(t, n("B3")) for t in tones[:3])
        for b in range(1, int(beats)):
            strum.chord(start + off + b, voicing, 1, vel - (4 if b == 2 else 0), roll=0.02, legato=0.7)


def build():
    p = Piece("hearthsmoke", bpm=116, meter=3, bars=48, lufs=-20.5, title="Hearthsmoke",
              hall=dict(rt60=1.4, predelay=0.012, hf_rt=0.55), wet=-7.0)
    strum = p.part("strumstick", "strum", pan=-0.3, send=0.2, gain=-2.0, human_ms=6)
    pizz = p.part("cellos pizz", "vc_pizz", pan=0.3, send=0.2, gain=-3.0)
    cbp = p.part("basses pizz", "cb_pizz", pan=0.4, send=0.2, gain=-8.0)
    rec = p.part("recorder", "recorder", pan=0.1, send=0.3, gain=-1.0, human_ms=8)
    clar = p.part("clarinet", "clar", pan=-0.15, send=0.3, gain=-3.0)
    fiddle = p.part("solo violin", "solo_vln", pan=0.2, send=0.3, gain=-1.0, delay_ms=15)
    vln = p.part("violins", "vln_soft", pan=-0.2, send=0.35, gain=-8.0, delay_ms=20)
    harp = p.part("folk harp", "folk_harp", pan=-0.4, send=0.3, gain=-6.0)
    frame = p.part("frame drum", "frame_drum", pan=0.15, send=0.2, gain=-6.0, human_ms=5)
    tamb = p.part("tambourine", "tambourine", pan=0.35, send=0.25, gain=-14.0, human_ms=5)
    bsn = p.part("bassoon", "bsn", pan=0.25, send=0.25, gain=-7.0)

    def step(start, bars, vel=80):
        for b in range(bars):
            frame.note(start + 3 * b, I.FRAME_LOW, 1, vel)
            frame.note(start + 3 * b + 1, I.FRAME_HIGH, 1, vel - 24)
            frame.note(start + 3 * b + 2, I.FRAME_HIGH, 1, vel - 20)

    # bars 0-3 and 44-47: the dance alone
    oom_pah_pah(strum, pizz, p.bar(0), INTRO, 70)
    oom_pah_pah(strum, pizz, p.bar(44), OUTRO, 70)
    bass(cbp, p.bar(0), INTRO, n("G1"), vel=70)
    bass(cbp, p.bar(44), OUTRO, n("G1"), vel=70)
    step(p.bar(0), 4, 74)
    step(p.bar(44), 4, 74)
    rec.line(p.bar(46), "r:2 C5:.5 D5:.5 | C5:1 A4:1 F#4:1", vel=66)

    # bars 4-11: the recorder's theme
    a = p.bar(4)
    rec.line(a, TUNE_A, vel=84)
    oom_pah_pah(strum, pizz, a, A, 72)
    bass(cbp, a, A, n("G1"), vel=72)
    step(a, 8, 80)

    # bars 12-19: again, the harp and the violins under it
    b = p.bar(12)
    rec.line(b, TUNE_A, vel=86)
    vln.line(b, UNDER_A, vel=70, attack=0.2)
    broken(harp, b, A, n("G3"), [0, 1, 2, 3, 2, 1], 0.5, vel=66, accent=8)
    oom_pah_pah(strum, pizz, b, A, 72)
    bass(cbp, b, A, n("G1"), vel=72)
    step(b, 8, 80)

    # bars 20-27: the second tune, the solo violin
    c = p.bar(20)
    fiddle.line(c, TUNE_B, vel=88, attack=0.06)
    oom_pah_pah(strum, pizz, c, B, 70)
    bass(cbp, c, B, n("G1"), vel=70)
    bass(bsn, c, B, n("G2"), vel=72, rhythm=[(0, 3, 0)])
    step(c, 8, 76)

    # bars 28-35: the clarinet takes it, the recorder answering
    d = p.bar(28)
    clar.line(d, TUNE_B, vel=86)
    rec.line(d, ANSWER_B, vel=76)
    broken(harp, d, B, n("E3"), [0, 1, 2, 3, 2, 1], 0.5, vel=62, accent=8)
    oom_pah_pah(strum, pizz, d, B, 72)
    bass(cbp, d, B, n("G1"), vel=72)
    step(d, 8, 80)

    # bars 36-43: everyone
    e = p.bar(36)
    rec.line(e, TUNE_A, vel=90)
    fiddle.line(e, TUNE_A, vel=84, transpose=0, attack=0.05)
    clar.line(e, THIRDS_A, vel=80)
    vln.line(e, UNDER_A, vel=74, attack=0.2)
    broken(harp, e, A, n("G3"), [0, 1, 2, 3, 2, 1], 0.5, vel=70, accent=8)
    oom_pah_pah(strum, pizz, e, A, 76)
    bass(cbp, e, A, n("G1"), vel=76)
    bass(bsn, e, A, n("G2"), vel=74, rhythm=[(0, 3, 0)])
    step(e, 8, 84)
    for bar in range(8):
        tamb.note(e + 3 * bar + 1, I.TAMB_SHAKE, 1, 70)
        tamb.note(e + 3 * bar + 2, I.TAMB_HIT, 1, 76)
    return p
