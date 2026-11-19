"""
"Steel", the battle music: the theme in D minor at 150 BPM, 64 bars, driven from underneath.

The cellos and basses never stop: spiccato eighths on each chord's root, leaping the octave in a
3+3+2 that the bass drum and the timpani lean on. Over them:

    bars  0-3   the drive alone, drums in, D minor to B flat and C (flat six, flat seven)
    bars  4-11  the horns play the theme in the minor, the violas chopping eighths above
    bars 12-19  the trumpets take it an octave up, the horns a third under, the violins in
                sixteenths, the snare on the backbeat
    bars 20-27  away to G minor: the violins sing the theme there, long, over trombone stabs,
                and turn onto A, the dominant
    bars 28-35  the build: the head called a step higher every two bars (D minor, E flat, F, A)
                over tremolo strings, the snare rolling up to
    bars 36-43  everyone: the theme in trumpets and horns, violins in sixteenths, cymbals
    bars 44-51  the low brass throw the head down and the violins answer in runs
    bars 52-59  the horns' theme once more under a high violin line, the trumpets in for the cadence
    bars 60-63  the drive alone again, round to the top
"""

from common import broken, chart, drums, pad
from score import Piece
from theory import bass_note, n, theme
import instruments as I

INTRO = "Dm:4 | Dm:4 | Bb:4 | C:4"
A = "Dm:4 | Gm:4 | Dm:2 C:2 | A:4 | Dm:4 | Bb:4 | C:4 | Dm:4"
B = "Gm:4 | Cm:4 | Gm:2 F:2 | D:4 | Gm:4 | Eb:4 | F:4 | A:4"
BUILD = "Dm:8 | Eb:8 | F:8 | A:8"
CALL = "Dm:8 | Bb:8 | Gm:8 | A:8"

KICK = "X.....x.....x..."          # 3+3+2 in sixteenths, one bar
KICK_FILL = "X.....x.....x.xx"
SNARE = "....X.......X..."
SNARE_GHOST = "....X..o....X.oo"


def low_drive(part, start, text, near, vel, figure=(0, 0, 12, 0, 0, 12, 0, 12), accents=">..>..>."):
    """The cellos' and basses' eighths: each chord's root, leaping the octave in a 3+3+2."""
    for sym, beats, off in chart(text):
        root = bass_note(sym, near)
        for i in range(int(beats * 2)):
            acc = accents[i % 8] == ">"
            part.note(start + off + i * 0.5, root + figure[i % 8], 0.5, vel + (14 if acc else 0), legato=0.8)
    return start + sum(b for _, b, _ in chart(text))


def build():
    p = Piece("steel", bpm=150, meter=4, bars=64, lufs=-18.5, title="Steel",
              hall=dict(rt60=1.7, predelay=0.018, hf_rt=0.5), wet=-6.0)
    vc = p.part("cellos", "vc_spic", pan=0.3, send=0.18, gain=0.0, human_ms=4)
    cb = p.part("basses", "cb_spic", pan=0.4, send=0.15, gain=-2.0, human_ms=4)
    vla = p.part("violas", "vla_spic", pan=0.15, send=0.2, gain=-5.0, human_ms=4)
    vlnsp = p.part("violins spic", "vln_spic", pan=-0.25, send=0.22, gain=-6.0, human_ms=3)
    vln = p.part("violins", "vln", pan=-0.3, send=0.3, gain=-1.0, delay_ms=20)
    vln2 = p.part("violins II", "vln", pan=-0.1, send=0.3, gain=-5.0, delay_ms=20)
    trem = p.part("tremolo", "vln_trem", pan=-0.2, send=0.3, gain=-6.0)
    vlatrem = p.part("violas trem", "vla_trem", pan=0.2, send=0.3, gain=-7.0)
    horn = p.part("horns", "horn", pan=-0.15, send=0.3, gain=0.0)
    horn2 = p.part("horns 3-4", "horn", pan=-0.3, send=0.32, gain=-4.0)
    tpt = p.part("trumpets", "tpt", pan=0.1, send=0.28, gain=-1.0)
    stab = p.part("trumpet stabs", "tpt_stac", pan=0.15, send=0.3, gain=-4.0)
    tbn = p.part("trombones", "tbn", pan=0.3, send=0.28, gain=-3.0)
    tbnst = p.part("trombone stabs", "tbn_stac", pan=0.3, send=0.28, gain=-2.0)
    tuba = p.part("tuba", "tuba", pan=0.35, send=0.22, gain=-5.0)
    timp = p.part("timpani", "timp", pan=0.0, send=0.25, gain=-1.0, human_ms=3)
    roll = p.part("timpani roll", "timp_roll", pan=0.0, send=0.3, gain=-5.0)
    kick = p.part("bass drum", "bass_drum", pan=0.0, send=0.2, gain=-2.0, human_ms=3,
                  eq=[("highpass", 42.0, 0.7, 0.0), ("peak", 58.0, 1.0, -4.0)])
    snare = p.part("snare", "snare", pan=-0.1, send=0.2, gain=-9.0, human_ms=3)
    tom = p.part("toms", "tom", pan=0.2, send=0.2, gain=-6.0, human_ms=3)
    cym = p.part("cymbals", "cymbal", pan=0.25, send=0.35, gain=-9.0)
    sus = p.part("suspended cymbal", "sus_cymbal", pan=-0.2, send=0.4, gain=-10.0)

    D2, D3 = n("D2"), n("D3")

    def drive(start, text, vel=78):
        low_drive(vc, start, text, D3, vel)
        low_drive(cb, start, text, D2, vel - 4)

    def kit(start, bars, fill=True, snare_on=True, vel=86):
        for b in range(bars):
            last = fill and b == bars - 1
            drums(kick, start + 4 * b, KICK_FILL if last else KICK, I.BASS_DRUM_HIT, vel)
            if snare_on:
                drums(snare, start + 4 * b, SNARE_GHOST if b % 2 else SNARE, I.SNARE_HIT, vel - 6)
        if fill:
            drums(tom, start + 4 * bars - 2, "x.x.xxxx", I.TOM_HIT, vel - 4, step=0.25)

    def timps(start, text, vel=84):
        """The timpani on each chord's root (or its fifth below when that's out of range): beat 1 and
        the 3+3+2's second push."""
        for sym, beats, off in chart(text):
            root = bass_note(sym, n("F2"))
            while root < n("C2") + 2:
                root += 12
            for k in range(int(beats // 4) or 1):
                t = start + off + 4 * k
                timp.note(t, root, 1.5, vel + 10)
                timp.note(t + 1.5, root, 1.0, vel - 8)
                timp.note(t + 3, root + 7 if root + 7 <= n("C3") + 2 else root - 5, 1.0, vel - 4)

    # -- bars 0-3, 60-63: the drive alone -------------------------------------------------------- #
    for start in (p.bar(0), p.bar(60)):
        drive(start, INTRO, 80)
        broken(vla, start, INTRO, n("D4"), [1, 2, 3, 2], 0.5, vel=68, accent=12)
        kit(start, 4, snare_on=False)
        timps(start, INTRO)
        horn2.line(start + 8, "Bb2+F3:4' | C3+G3:2 C3+G3:1 C3+E3+G3:1>", vel=86, attack=0.03)
        tbnst.line(start + 8, "Bb2:1> r:3 | C3:1> r:1 C3:.5 C3:.5 C3:1>", vel=92)
    sus.note(p.bar(63), I.SUS_SWELL_2S, 4, 78)

    # -- bars 4-11: the horns' theme ------------------------------------------------------------- #
    a = p.bar(4)
    cym.note(a, I.CYMBAL_CRASH, 4, 96)
    horn.notes_of(a, theme(n("D4"), minor=True), vel=96, attack=0.03)
    drive(a, A, 82)
    broken(vla, a, A, n("D4"), [1, 2, 3, 2], 0.5, vel=70, accent=12)
    pad(tbn, a, A, [n("D3"), n("A3")], 43, 62, vel=70, attack=0.05)
    kit(a, 8, snare_on=False)
    timps(a, A)
    stab.line(p.bar(9), "D5+F5:.5> r:1 D5+F5:.5 r:2 | E5+G5:.5> r:1 E5+G5:.5 r:1 E5+G5:.5 E5+G5:.5", vel=90)

    # -- bars 12-19: the trumpets' theme --------------------------------------------------------- #
    b = p.bar(12)
    cym.note(b, I.CYMBAL_CRASH, 4, 100)
    tpt.notes_of(b, theme(n("D5"), minor=True), vel=96, attack=0.03)
    # the horns a third (or a sixth) under: the theme's lower line in the same chords
    horn.line(b, "A3:1 F4:1.5 E4:.5 F4:1 | G4:3 F4:.5 D4:.5 | D4:1 A3:1 C4:1.5 C4:.5 | E4:4"
                 " | A3:1 F4:1.5 E4:.5 F4:1 | F4:3 F4:.5 D4:.5 | G4:1.5 E4:.5 C4:1 G3:1 | A3:4", vel=88, attack=0.03)
    drive(b, A, 86)
    broken(vlnsp, b, A, n("D4"), [2, 3, 4, 3], 0.25, vel=68, accent=12)
    broken(vla, b, A, n("D4"), [1, 2, 3, 2], 0.5, vel=72, accent=12)
    pad(horn2, b, A, [n("D3"), n("F3"), n("A3")], 45, 64, vel=76, attack=0.04)
    pad(tbn, b, A, [n("D3"), n("A3")], 43, 62, vel=78, attack=0.04)
    kit(b, 8)
    timps(b, A, 88)

    # -- bars 20-27: G minor, the violins sing --------------------------------------------------- #
    c = p.bar(20)
    sus.note(c - 4, I.SUS_SWELL_4S, 4, 70)
    g_theme = theme(n("G4"), minor=True)[:-1] + [(n("A4"), 4)]
    vln.notes_of(c, [(k + 12, d) for k, d in g_theme], vel=94, attack=0.08)
    vln2.notes_of(c, g_theme, vel=86, attack=0.08)
    drive(c, B, 76)
    for i, (sym, beats, off) in enumerate(chart(B)):
        root = bass_note(sym, n("G2"))
        tbnst.note(c + off, root, 1, 96)
        tbnst.note(c + off + 1.5, root, 0.5, 80)
        tbnst.note(c + off + 3, root + 12, 1, 90)
    pad(horn2, c, B, [n("D3"), n("G3"), n("Bb3")], 45, 66, vel=74, attack=0.1)
    pad(tuba, c, B, [n("G1")], 31, 45, vel=80, attack=0.05)
    kit(c, 8, snare_on=False, vel=80)
    timps(c, B, 80)

    # -- bars 28-35: the build ------------------------------------------------------------------- #
    d = p.bar(28)
    horn.line(d, "D4:1> A4:1.5 G4:.5 A4:1 | Bb4:4", vel=92, attack=0.03)
    horn.line(d + 8, "Eb4:1> Bb4:1.5 Ab4:.5 Bb4:1 | C5:4", vel=96, attack=0.03)
    horn.line(d + 16, "F4:1> C5:1.5 Bb4:.5 C5:1 | D5:4", vel=100, attack=0.03)
    tpt.line(d + 24, "A4:1> E5:1.5 D5:.5 E5:1 | C#5:2 E5:2", vel=104, attack=0.03)
    horn2.line(d + 24, "A3:1> C#4:1.5 D4:.5 C#4:1 | A3:2 C#4:2", vel=98, attack=0.03)
    pad(trem, d, BUILD, [n("A4"), n("D5"), n("F5")], 64, 84, vel=72, attack=0.2, swell=(-8.0, 2.0))
    pad(vlatrem, d, BUILD, [n("D4"), n("F4")], 55, 72, vel=70, attack=0.2, swell=(-8.0, 2.0))
    drive(d, BUILD, 74)
    for k in range(4):
        root = bass_note(chart(BUILD)[k][0], n("F2"))
        timp.note(d + 8 * k, root, 2, 94)
        timp.note(d + 8 * k + 4, root, 2, 80)
        drums(kick, d + 8 * k, "X...............X.......x.......", I.BASS_DRUM_HIT, 84)
    # the snare rolls up over the last two bars
    for i in range(32):
        snare.note(d + 24 + i * 0.25, I.SNARE_HIT, 0.25, 40 + i * 2, legato=1.0)
    roll.note(d + 24, n("A2"), 8, 70, swell=(-16.0, 3.0))
    sus.note(d + 28, I.SUS_SWELL_4S, 4, 86)

    # -- bars 36-43: everyone -------------------------------------------------------------------- #
    e = p.bar(36)
    cym.note(e, I.CYMBAL_CRASH, 4, 108)
    tpt.notes_of(e, theme(n("D5"), minor=True), vel=106, attack=0.02)
    horn.notes_of(e, theme(n("D4"), minor=True), vel=104, attack=0.02)
    drive(e, A, 90)
    broken(vlnsp, e, A, n("D4"), [2, 3, 4, 3], 0.25, vel=76, accent=12)
    broken(vla, e, A, n("D4"), [1, 2, 3, 2], 0.5, vel=78, accent=12)
    pad(horn2, e, A, [n("D3"), n("F3"), n("A3")], 45, 64, vel=86, attack=0.03)
    pad(tbn, e, A, [n("D3"), n("A3")], 43, 62, vel=88, attack=0.03)
    pad(tuba, e, A, [n("D2")], 31, 45, vel=84, attack=0.04)
    kit(e, 8, vel=92)
    timps(e, A, 94)
    cym.note(p.bar(40), I.CYMBAL_CRASH, 4, 96)

    # -- bars 44-51: the low brass call, the violins answer -------------------------------------- #
    f = p.bar(44)
    tbn.line(f, "D3:1> A3:1.5 G3:.5 A3:1 | r:4 | Bb2:1> F3:1.5 Eb3:.5 F3:1 | r:4 | G2:1> D3:1.5 C3:.5 D3:1",
             vel=104, attack=0.02)
    tuba.line(f, "D2:1> A2:1.5 G2:.5 A2:1 | r:4 | Bb1:1> F2:1.5 Eb2:.5 F2:1 | r:4 | G1:1> D2:1.5 C2:.5 D2:1",
              vel=98, attack=0.03)
    horn2.line(f, "D3:1> A3:1.5 G3:.5 A3:1 | r:4 | Bb2:1> F3:1.5 Eb3:.5 F3:1 | r:4 | G2:1> D3:1.5 C3:.5 D3:1",
               vel=96, attack=0.02)
    runs = ["D5 E5 F5 G5 A5 Bb5 C6 D6 C6 Bb5 A5 G5 A5 F5 E5 D5",
            "D5 Eb5 F5 G5 A5 Bb5 C6 D6 C6 Bb5 A5 G5 F5 Eb5 D5 C5",
            "Bb4 C5 D5 Eb5 F5 G5 A5 Bb5 A5 G5 F5 Eb5 D5 C5 Bb4 A4"]
    for i, run in enumerate(runs):
        vlnsp.line(f + 4 + 8 * i, " ".join(x + ":.25" for x in run.split()), vel=80)
    drive(f, CALL, 84)
    stab.line(p.bar(50), "C#5+E5+A5:.5> r:.5 C#5+E5+A5:.5 r:.5 C#5+E5+A5:.5> r:1.5 | C#5+E5+A5:.5> C#5+E5+A5:.5 C#5+E5+A5:.5 C#5+E5+A5:.5 E5+A5+C#6:2>", vel=92)
    kit(f, 8, vel=88)
    timps(f, CALL, 88)

    # -- bars 52-59: the horns' theme, a high line over it --------------------------------------- #
    g = p.bar(52)
    cym.note(g, I.CYMBAL_CRASH, 4, 100)
    horn.notes_of(g, theme(n("D4"), minor=True), vel=100, attack=0.03)
    vln.line(g, "A5:4 | Bb5:4 | A5:2 G5:2 | E5:4 | F5:4 | F5:4 | E5:2 G5:2 | F5:4", vel=92, attack=0.1)
    vln2.line(g, "F5:4 | D5:4 | F5:2 E5:2 | C#5:4 | D5:4 | D5:4 | C5:2 E5:2 | D5:4", vel=84, attack=0.1)
    tpt.notes_of(p.bar(57), theme(n("D5"), minor=True)[16:], vel=100, attack=0.03)
    drive(g, A, 86)
    broken(vla, g, A, n("D4"), [1, 2, 3, 2], 0.5, vel=74, accent=12)
    pad(tbn, g, A, [n("D3"), n("A3")], 43, 62, vel=82, attack=0.04)
    pad(tuba, p.bar(56), "Dm:4 | Bb:4 | C:4 | Dm:4", [n("D2")], 31, 45, vel=82, attack=0.04)
    kit(g, 8, vel=90)
    timps(g, A, 90)
    return p
