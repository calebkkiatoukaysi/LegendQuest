"""
A piece of music as the pieces write it: a tempo, a metre, a length in bars, and parts, each an
instrument with its notes, its place in the stereo field and how much of it goes to the hall.

    p = Piece("steel", bpm=150, meter=4, bars=64, lufs=-18.5)
    horns = p.part("horns", "horn", pan=-0.3, send=0.3)
    horns.line(16, "D4:1 A4:1.5 G4:.5 A4:1 | Bb4:3 A4:.5 G4:.5")
    audio = p.render()

Beats count from 0 at the top of the loop, in the metre's beats (a quarter note in 4/4 and 3/4; an
eighth in 6/8). A note's length is in beats too. The beat is snapped to a whole number of samples,
so a loop is exactly its bars long and the bar lines never drift; the tempo printed is the snapped
one.

A part humanises what it plays from its own seed: each note lands a few milliseconds early or late
(timing sd `human_ms`) and a little louder or softer (velocity sd `human_vel`), so the same score
renders the same audio every time, but no two notes of a run are mechanically alike. Round robin is
counted per key, as a player alternates bows.

Line notation, one token per note, bar lines '|' ignored:
    D4:1        D4 for a beat          r:2        a rest of two beats
    D3+A3:2     a chord                F#4:.5>    accented (+16 velocity); '<' is softer (-16)
    A4:1'       staccato (sounds for 45% of its length); '~' is held a little over (legato 1.1)
"""

import math
import re
import zlib

import numpy as np

import instruments
import mix
from sampler import RATE
from theory import n as note_number


class Part:
    def __init__(self, piece, name, inst, pan=0.0, gain=0.0, send=0.25, human_ms=7.0, human_vel=5.0,
                 width=1.0, eq=None, legato=0.98, delay_ms=0.0):
        self.piece, self.name, self.inst = piece, name, inst
        self.pan, self.gain, self.send, self.width = pan, gain, send, width
        self.human_ms, self.human_vel, self.legato = human_ms, human_vel, legato
        self.eq = eq or []
        self.delay_ms = delay_ms        # a fixed late start: some samples speak late (strings' bow)
        self.notes = []
        self.automation = None

    # -- writing ------------------------------------------------------------- #

    def note(self, beat, key, beats, vel=80, legato=None, attack=None, release=None, cents=0.0, swell=None):
        """One note. swell=(start dB, end dB) ramps its level over its held length."""
        if isinstance(key, str):
            key = note_number(key)
        self.notes.append(dict(beat=beat, key=key, beats=beats, vel=vel,
                               legato=self.legato if legato is None else legato,
                               attack=attack, release=release, cents=cents, swell=swell))
        return beat + beats

    def chord(self, beat, keys, beats, vel=80, roll=0.0, **kw):
        """Several notes at once; roll spreads them upward by that many beats each (a harp's spread)."""
        for i, k in enumerate(keys):
            self.note(beat + i * roll, k, beats - i * roll, vel, **kw)
        return beat + beats

    def notes_of(self, beat, pairs, vel=80, transpose=0, **kw):
        """[(midi, beats), ...] one after another (theory.theme's output). Returns the end beat."""
        for key, beats in pairs:
            if key is not None:
                self.note(beat, key + transpose, beats, vel, **kw)
            beat += beats
        return beat

    def line(self, beat, text, vel=80, transpose=0, **kw):
        """Notes in the line notation (see the module's docstring). Returns the end beat."""
        for tok in text.split():
            if tok == "|":
                continue
            m = re.fullmatch(r"([^:]+):([0-9.]+)([>'<~]*)", tok)
            if not m:
                raise ValueError("bad token %r in %s" % (tok, self.name))
            pitches, beats, marks = m.group(1), float(m.group(2)), m.group(3)
            v = vel + 16 * marks.count(">") - 16 * marks.count("<")
            leg = None
            if "'" in marks:
                leg = 0.45
            elif "~" in marks:
                leg = 1.1
            if pitches != "r":
                for p in pitches.split("+"):
                    args = dict(kw)
                    if leg is not None:
                        args["legato"] = leg
                    self.note(beat, note_number(p) + transpose, beats, v, **args)
            beat += beats
        return beat

    def fade(self, points):
        """Level automation over the piece: [(beat, dB), ...], straight lines between, held at the
        ends (and joined round the loop for a looping piece)."""
        self.automation = sorted(points)

    # -- playing ------------------------------------------------------------- #

    def render(self, length):
        p = self.piece
        rng = np.random.default_rng(zlib.crc32(("%s/%s" % (p.name, self.name)).encode()))
        inst = instruments.get(self.inst) if isinstance(self.inst, str) else self.inst
        buf = np.zeros((length, 2))
        rr = {}
        for nt in sorted(self.notes, key=lambda d: (d["beat"], d["key"])):
            jitter = float(np.clip(rng.normal(0.0, self.human_ms), -2.5 * self.human_ms, 2.5 * self.human_ms))
            dv = float(np.clip(rng.normal(0.0, self.human_vel), -2.5 * self.human_vel, 2.5 * self.human_vel))
            rand = float(rng.random())
            at = p.at(nt["beat"]) + int((jitter + self.delay_ms) * RATE / 1000.0)
            dur = nt["beats"] * p.beat_seconds * nt["legato"]
            k = nt["key"]
            x = inst.render(k, nt["vel"] + dv, dur, rr=rr.get(k, 0), rand=rand, attack=nt["attack"],
                            release=nt["release"], cents=nt["cents"])
            rr[k] = rr.get(k, 0) + 1
            if nt["swell"]:
                g0, g1 = nt["swell"]
                held = max(1, int(dur * RATE))
                ramp = np.full(len(x), g1, float)
                ramp[:held] = np.linspace(g0, g1, held)[:len(x)]
                x = x * (10.0 ** (ramp / 20.0))[:, None]
            p.place(buf, x, at)
        if self.automation:
            buf *= self._automation_curve(length)[:, None]
        if self.eq:
            buf = mix.eq(buf, self.eq)
        # width (mid/side), then a constant-power pan
        if self.width != 1.0:
            mid = 0.5 * (buf[:, 0] + buf[:, 1])
            side = 0.5 * (buf[:, 0] - buf[:, 1]) * self.width
            buf = np.stack([mid + side, mid - side], axis=1)
        a = (self.pan + 1.0) * math.pi / 4.0
        buf[:, 0] *= math.cos(a) * math.sqrt(2.0)
        buf[:, 1] *= math.sin(a) * math.sqrt(2.0)
        return buf * mix.undb(self.gain)

    def _automation_curve(self, length):
        p = self.piece
        pts = self.automation
        xs = [p.at(b) for b, _ in pts]
        ys = [d for _, d in pts]
        if p.loop:
            xs = [xs[-1] - length] + xs + [xs[0] + length]
            ys = [ys[-1]] + ys + [ys[0]]
        curve_db = np.interp(np.arange(length), xs, ys)
        return 10.0 ** (curve_db / 20.0)


class Piece:
    """A song or a jingle.

    loop: True for a song (exactly `bars` bars, everything wrapped round); False for a one-shot,
        which gets `tail` seconds after its last bar for the hall to die away in.
    lufs: the loudness it's mastered to.
    hall: hall_ir's arguments (rt60, predelay, ...); wet: the hall's level against the dry mix, dB.
    """

    def __init__(self, name, bpm, meter, bars, loop=True, tail=0.0, lufs=-20.0, hall=None, wet=0.0,
                 title=None, seed=None):
        self.name, self.title = name, title or name
        self.meter, self.bars, self.loop, self.tail = meter, bars, loop, tail
        self.spb = int(round(RATE * 60.0 / bpm))
        self.bpm = RATE * 60.0 / self.spb
        self.beat_seconds = self.spb / RATE
        self.lufs = lufs
        self.hall = dict(rt60=2.2, predelay=0.025)
        self.hall.update(hall or {})
        self.wet = wet
        self.seed = zlib.crc32(name.encode()) if seed is None else seed
        self.parts = []
        self.beds = []          # whole-loop textures: (stereo array, gain dB, send, pan)
        self.report = {}

    @property
    def beats(self):
        return self.bars * self.meter

    @property
    def length(self):
        n = self.beats * self.spb
        return n if self.loop else n + int(self.tail * RATE)

    def at(self, beat):
        return int(round(beat * self.spb))

    def bar(self, b, beat=0.0):
        """The beat at which bar b (counting from 0) starts, plus `beat`."""
        return b * self.meter + beat

    def part(self, name, inst, **kw):
        prt = Part(self, name, inst, **kw)
        self.parts.append(prt)
        return prt

    def bed(self, audio, gain=0.0, send=0.0):
        """A whole-loop texture (synth.sea and the like), already exactly the loop's length."""
        assert len(audio) == self.length, "a bed must be exactly the piece's length"
        self.beds.append((audio, gain, send))

    def place(self, buf, x, at):
        """Adds x into buf at sample `at`: round the loop for a song, cut at the end for a jingle."""
        n = len(buf)
        if self.loop:
            at %= n
            while len(x):
                k = min(len(x), n - at)
                buf[at:at + k] += x[:k]
                x = x[k:]
                at = 0
        else:
            if at < 0:
                x, at = x[-at:], 0
            k = min(len(x), n - at)
            if k > 0:
                buf[at:at + k] += x[:k]

    def render(self, verbose=False):
        n = self.length
        dry = np.zeros((n, 2))
        send = np.zeros((n, 2))
        for prt in self.parts:
            buf = prt.render(n)
            if verbose:
                rms = math.sqrt(float(np.mean(buf ** 2))) if len(buf) else 0.0
                print("    %-14s %-14s %4d notes  rms %6.1f dB" % (prt.name, prt.inst, len(prt.notes), mix.db(rms)))
            dry += buf
            send += buf * prt.send
        for audio, gain, s in self.beds:
            g = mix.undb(gain)
            dry += audio * g
            send += audio * g * s
        ir = mix.hall_ir(self.seed, **self.hall)
        wet = mix.convolve(send, ir) * mix.undb(self.wet)
        out = dry + wet
        if not self.loop:
            # end in silence: the last stretch of the hall's tail fades out along a quarter cosine
            k = int(min(0.6, self.tail / 3.0) * RATE)
            out[-k:] *= np.cos(np.linspace(0.0, np.pi / 2, k))[:, None] ** 2
        return mix.master(out, self.lufs, loop=self.loop, report=self.report)
