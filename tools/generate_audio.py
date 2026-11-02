"""
Audio generator for Legend Quest. Every WAV in Content/Music and Content/Sfx comes out of this
script. Run it from the repo root:

    python tools/generate_audio.py                     # everything
    python tools/generate_audio.py music               # the songs
    python tools/generate_audio.py sfx                 # every sound effect
    python tools/generate_audio.py embers menu-move    # just these, by file name
    python tools/generate_audio.py -v embers           # also print each layer's level

Only needs CPython (3.12 or newer, for math.sumprod). No samples are used: everything is built from
sines, wavetables, noise and a few filters, and every random number is seeded, so a rerun writes
the exact same files. The oscillators, filters, reverb and song mixer are The Black Box's, copied
over. The lute, the frame drum, the brass, the timpani, the chimes and the birds are new.

Music is 16-bit stereo at 32 kHz and loops cleanly with MediaPlayer.IsRepeating. Sound effects are
16-bit mono at 44.1 kHz and already balanced against each other, so the game can play them all at
the same volume.
"""

import cmath
import math
import os
import random
import sys
import time
import wave
import zlib
from array import array
from itertools import accumulate, repeat
from operator import add, mul, sub

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")

MUSIC_RATE = 32000
SFX_RATE = 44100

# Loudness targets. Music gets normalised to an RMS level and kept under the peak ceiling.
# Sound effects get normalised by peak, quieter for the ones that fire a lot.
MUSIC_PEAK_DB = -1.0
TITLE_RMS_DB = -21.0
EXPLORE_RMS_DB = -22.0  # under everything else that's going on, since it plays the longest
DARK_RMS_DB = -24.0     # quieter again, so the drips and the footsteps carry
LIMIT_KNEE = 0.5        # the limiter only touches samples above this (linear)

PEAK_MENU = -6.0        # confirm and back
PEAK_CLICK = -8.0       # UI clicks
PEAK_REPEAT = -10.0     # footsteps, which fire constantly

TAU = 2.0 * math.pi
VERBOSE = False


# --------------------------------------------------------------------------- #
# WAV writer and meters
# --------------------------------------------------------------------------- #

def db(x):
    return 20.0 * math.log10(x) if x > 1e-12 else -240.0


def undb(d):
    return 10.0 ** (d / 20.0)


def rms(*chans):
    count = sum(len(c) for c in chans)
    return math.sqrt(sum(math.sumprod(c, c) for c in chans) / count) if count else 0.0


def peak(*chans):
    return max(max(map(abs, c)) for c in chans if c)


def to_pcm(buf):
    """Floats in -1..1 to 16-bit, rounded half up and clamped."""
    return array("h", [-32767 if v < -1.0 else 32767 if v > 1.0 else int(v * 32767.0 + 32768.5) - 32768
                       for v in buf])


def write_wav(rel, chans, rate):
    path = os.path.join(ROOT, rel)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    pcm = [to_pcm(c) for c in chans]
    n = len(pcm[0])
    if len(pcm) == 1:
        data = pcm[0]
    else:
        data = array("h", bytes(4 * n))
        data[0::2] = pcm[0]
        data[1::2] = pcm[1]
    if sys.byteorder == "big":
        data.byteswap()
    with wave.open(path, "wb") as w:
        w.setnchannels(len(pcm))
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(data.tobytes())
    top = max(max(map(abs, p)) for p in pcm) / 32768.0
    line = "  %s  (%.*f s, peak %.1f dBFS, rms %.1f dBFS)" % (
        rel, 2 if n < rate else 1, n / rate, db(top), db(rms(*chans)))
    clipped = sum(p.count(32767) + p.count(-32767) for p in pcm)
    if clipped:
        line += "  [%d samples at full scale]" % clipped
    print(line)


def seam_report(chans, rate):
    """Prints how clean the loop point is. I compare the jump from the last sample back to the
    first against the average step in the 10 ms around it (a click would be a lot bigger), then
    print the level of the first and last 50 ms."""
    def mean_step(x):
        return sum(map(abs, map(sub, x[1:], x[:-1]))) / (len(x) - 1)
    k = int(0.01 * rate)
    jump = max(abs(c[0] - c[-1]) for c in chans)
    near = max(mean_step(c[-k:] + c[:k]) for c in chans)
    whole = max(mean_step(c) for c in chans)
    w = int(0.05 * rate)
    head = rms(*[c[:w] for c in chans])
    tail = rms(*[c[-w:] for c in chans])
    print("      seam: jump %.4f vs step %.4f nearby (x%.2f), %.4f overall; "
          "rms first 50 ms %.1f dBFS, last 50 ms %.1f dBFS"
          % (jump, near, jump / near if near else 0.0, whole, db(head), db(tail)))


# --------------------------------------------------------------------------- #
# Buffers, oscillators and envelopes
# --------------------------------------------------------------------------- #
# Buffers are plain lists of floats. The Black Box found map() and comprehensions over lists faster
# than array('d') or explicit loops. Finished song layers are stored as array('d') since a list
# takes four times the memory.

def silence(n):
    return [0.0] * max(0, n)


def scale(x, g):
    return list(map(mul, x, repeat(g)))


def shaped(x, env):
    return list(map(mul, x, env))


def norm(x, top=1.0):
    m = max(map(abs, x)) if x else 0.0
    return scale(x, top / m) if m > 0 else x


def mix(dst, src, at=0, gain=1.0):
    """Adds src into dst starting at sample `at`. Anything past either end gets dropped."""
    if at < 0:
        src = src[-at:]
        at = 0
    n = min(len(src), len(dst) - at)
    if n <= 0:
        return
    seg = src if n == len(src) else src[:n]
    if gain != 1.0:
        seg = map(mul, seg, repeat(gain))
    dst[at:at + n] = map(add, dst[at:at + n], seg)


def mix_loop(dst, src, at, gain=1.0):
    """Same as mix() but wraps around the end of dst, so tails that run past the loop point end
    up at the start. This is the main thing that keeps the songs looping cleanly."""
    size = len(dst)
    at %= size
    off = 0
    while off < len(src):
        n = min(size - at, len(src) - off)
        seg = src[off:off + n]
        if gain != 1.0:
            seg = map(mul, seg, repeat(gain))
        dst[at:at + n] = map(add, dst[at:at + n], seg)
        off += n
        at = 0


def pan_gains(pan):
    """Constant-power pan, -1 left to +1 right."""
    a = (pan + 1.0) * math.pi / 4.0
    return math.cos(a), math.sin(a)


def midi_hz(m, cents=0.0):
    return 440.0 * 2.0 ** ((m - 69 + cents / 100.0) / 12.0)


def noise(n, rng):
    r = rng.random
    return [r() * 2.0 - 1.0 for _ in range(n)]


def sine(freq, n, rate, amp=1.0, tau=0.0):
    """A sine starting at zero phase. If tau > 0 it decays with that time constant (seconds).

    It rotates a complex phasor with accumulate() instead of calling sin() every sample. Same
    result, about twice as fast."""
    if n <= 0:
        return []
    r = math.exp(-1.0 / (tau * rate)) if tau > 0 else 1.0
    step = cmath.rect(r, TAU * freq / rate)
    return [z.imag for z in accumulate(repeat(step, n - 1), mul, initial=complex(amp, 0.0))]


def glide(f_start, f_end, tau, n, rate, amp=1.0):
    """A sine that slides from f_start toward f_end with time constant tau. For drums and
    thumps. The phase is worked out directly, so there's no running state."""
    a = TAU * f_end / rate
    b = TAU * (f_start - f_end) * tau
    k = 1.0 / (tau * rate)
    s, e = math.sin, math.exp
    return [amp * s(a * i + b * (1.0 - e(-k * i))) for i in range(n)]


def decay(n, tau, rate):
    if n <= 0:
        return []
    return list(accumulate(repeat(math.exp(-1.0 / (tau * rate)), n - 1), mul, initial=1.0))


def envelope(n, rate, attack=0.002, tau=0.0, gate=None, release=0.05):
    """Raised-cosine attack, exponential decay (tau, 0 means hold), and an exponential release
    after `gate` if one is given. Always ends with a short fade down to zero."""
    e = decay(n, tau, rate) if tau > 0 else [1.0] * n
    a = min(n, int(attack * rate))
    for i in range(a):
        e[i] *= 0.5 - 0.5 * math.cos(math.pi * i / a)
    if gate is not None:
        g = int(gate * rate)
        if 0 <= g < n:
            e[g:] = map(mul, e[g:], decay(n - g, release, rate))
    return fade(e, rate, 0.0, 0.003)


def lines(points, n, rate):
    """Piecewise-linear envelope through (seconds, level) points, held after the last one."""
    out = silence(n)
    last = 0
    for (t0, v0), (t1, v1) in zip(points, points[1:]):
        i0, i1 = int(t0 * rate), int(t1 * rate)
        if i1 <= i0:
            continue
        d = (v1 - v0) / (i1 - i0)
        stop = min(n, i1)
        if stop > i0:
            out[i0:stop] = [v0 + d * k for k in range(stop - i0)]
        last = i1
    if last < n:
        out[last:] = [points[-1][1]] * (n - last)
    return out


def fade(x, rate, fin=0.0005, fout=0.005):
    """Raised-cosine fade in and out, in place, so the first and last samples are exactly zero."""
    n = len(x)
    a = min(n // 2, int(fin * rate))
    b = min(n // 2, int(fout * rate))
    cos, pi = math.cos, math.pi
    if a > 0:
        for i in range(a):
            x[i] *= 0.5 - 0.5 * cos(pi * i / a)
    elif n:
        x[0] = 0.0
    if b > 0:
        for i in range(b):
            x[n - 1 - i] *= 0.5 - 0.5 * cos(pi * i / b)
    elif n:
        x[-1] = 0.0
    return x


def modes(partials, n, rate, f0=1.0, amp=1.0):
    """Sum of decaying sines, partials given as (ratio, gain, tau). Anything plucked or struck:
    lute strings, wood, iron."""
    out = silence(n)
    top = 0.45 * rate
    for ratio, g, tau in partials:
        f = f0 * ratio
        if f >= top:
            continue
        m = min(n, int(tau * rate * 8) + 1)   # 8 tau is about -70 dB, so it stops rendering there
        mix(out, sine(f, m, rate, g * amp, tau))
    return out


# Band-limited wavetables. A naive saw at 32 kHz aliases badly, so each table only holds the
# harmonics that fit under Nyquist for the note that reads it.
TABLE_SIZE = 4096
TABLE_MASK = TABLE_SIZE - 1
_TABLES = {}


def _table(harmonics):
    if harmonics not in _TABLES:
        acc = silence(TABLE_SIZE)
        for k in range(1, harmonics + 1):
            mix(acc, sine(k, TABLE_SIZE, TABLE_SIZE, 1.0 / k))
        _TABLES[harmonics] = norm(acc)
    return _TABLES[harmonics]


def saw_table(freq, rate):
    h = max(1, int(0.45 * rate / max(freq, 1.0)))
    h = min(256, 1 << (h.bit_length() - 1))   # powers of two keep the cache small
    return _table(h)


def vibrato_time(n, rate, cents, vib_hz, delay_s):
    """Sample times with a vibrato in them that fades in after delay_s. Reading an oscillator at
    these times instead of 0, 1, 2... gives it vibrato, and several voices can share the list."""
    if not cents or n <= 0:
        return list(range(n))
    w = TAU * vib_hz / rate
    depth = (2.0 ** (cents / 1200.0) - 1.0) / w
    swing = lines(((0.0, 0.0), (delay_s, 0.0), (delay_s + 0.25, depth)), n, rate)
    wobble = [1.0 - z.real for z in accumulate(repeat(cmath.rect(1.0, w), n - 1), mul, initial=1 + 0j)]
    return list(map(add, range(n), map(mul, swing, wobble)))


def saw_at(freq, times, rate, amp=1.0, phase=0.0):
    """Band-limited saw read at the given sample times (see vibrato_time)."""
    tab = saw_table(freq * 1.03, rate)
    if amp != 1.0:
        tab = [v * amp for v in tab]
    inc = freq * TABLE_SIZE / rate
    p = phase * TABLE_SIZE
    m = TABLE_MASK
    return [tab[int(p + inc * t) & m] for t in times]


# --------------------------------------------------------------------------- #
# Filters and effects
# --------------------------------------------------------------------------- #

def lowpass(x, fc, rate):
    """One-pole low-pass, 6 dB/octave."""
    a = 1.0 - math.exp(-TAU * fc / rate)
    y = 0.0
    return [y := y + a * (v - y) for v in x]


def highpass(x, fc, rate):
    """One-pole high-pass. At 20 Hz it's the DC blocker."""
    a = 1.0 / (1.0 + TAU * fc / rate)
    y = prev = 0.0
    out = silence(len(x))
    for i, v in enumerate(x):
        y = a * (y + v - prev)
        prev = v
        out[i] = y
    return out


def biquad(x, kind, fc, q, rate):
    """RBJ cookbook biquad: 'lp', 'hp' or 'bp' (band-pass with 0 dB at the centre)."""
    w = TAU * min(fc, 0.45 * rate) / rate
    cw, sw = math.cos(w), math.sin(w)
    al = sw / (2.0 * q)
    if kind == "lp":
        b0 = b2 = (1.0 - cw) / 2.0
        b1 = 1.0 - cw
    elif kind == "hp":
        b0 = b2 = (1.0 + cw) / 2.0
        b1 = -(1.0 + cw)
    else:
        b0, b1, b2 = al, 0.0, -al
    a0 = 1.0 + al
    a1, a2 = -2.0 * cw / a0, (1.0 - al) / a0
    b0, b1, b2 = b0 / a0, b1 / a0, b2 / a0
    s1 = s2 = 0.0
    out = silence(len(x))
    for i, v in enumerate(x):
        y = b0 * v + s1
        s1 = b1 * v - a1 * y + s2
        s2 = b2 * v - a2 * y
        out[i] = y
    return out


def looped(fn, x, rate, pre_s=1.0):
    """Runs a filter with short memory over a loop. It's fed the last second first so its state at
    sample 0 matches coming round from the end. Same result as a second full pass, just cheaper."""
    pre = min(len(x), 16 * int(pre_s * rate / 16))
    return fn(x[-pre:] + x)[pre:]


REVERB_LINES_MS = (43.7, 53.9, 63.1, 75.3)
REVERB_DIFFUSERS_MS = (5.3, 7.7, 12.3)
REVERB_DIFFUSION = 0.62


def reverb(send, rate, rt60, damp_hz, size=1.0, loop=False):
    """Feedback-delay-network reverb: three allpass diffusers into four damped delay lines mixed
    through a Hadamard matrix. Mono in, (left, right) wet out.

    With loop=True the send runs through twice and the second pass is kept, so the tail from the
    end of the song is already there at sample 0 and the loop doesn't jump."""
    lens = [int(rate * ms * size / 1000.0) | 1 for ms in REVERB_LINES_MS]
    aps = [int(rate * ms / 1000.0) | 1 for ms in REVERB_DIFFUSERS_MS]
    g0, g1, g2, g3 = [0.5 * 10.0 ** (-3.0 * n / (rt60 * rate)) for n in lens]
    c = 1.0 - math.exp(-TAU * damp_hz / rate)
    l0, l1, l2, l3 = lens
    m0, m1, m2 = aps
    d0, d1, d2, d3 = [0.0] * l0, [0.0] * l1, [0.0] * l2, [0.0] * l3
    a0, a1, a2 = [0.0] * m0, [0.0] * m1, [0.0] * m2
    i0 = i1 = i2 = i3 = j0 = j1 = j2 = 0
    f0 = f1 = f2 = f3 = 0.0
    G = REVERB_DIFFUSION
    n = len(send)
    outl, outr = silence(n), silence(n)
    for _ in range(2 if loop else 1):
        for i in range(n):
            x = send[i]
            b = a0[j0]
            w = x + G * b
            a0[j0] = w
            x = b - G * w
            j0 += 1
            if j0 == m0:
                j0 = 0
            b = a1[j1]
            w = x + G * b
            a1[j1] = w
            x = b - G * w
            j1 += 1
            if j1 == m1:
                j1 = 0
            b = a2[j2]
            w = x + G * b
            a2[j2] = w
            x = b - G * w
            j2 += 1
            if j2 == m2:
                j2 = 0
            y0, y1, y2, y3 = d0[i0], d1[i1], d2[i2], d3[i3]
            f0 += c * (y0 - f0)
            f1 += c * (y1 - f1)
            f2 += c * (y2 - f2)
            f3 += c * (y3 - f3)
            s, t, u, v = f0 + f1, f0 - f1, f2 + f3, f2 - f3
            d0[i0] = x + g0 * (s + u)
            d1[i1] = g1 * (t + v) - x
            d2[i2] = x + g2 * (s - u)
            d3[i3] = g3 * (t - v) - x
            outl[i] = y0 + y2
            outr[i] = y1 + y3
            i0 += 1
            if i0 == l0:
                i0 = 0
            i1 += 1
            if i1 == l1:
                i1 = 0
            i2 += 1
            if i2 == l2:
                i2 = 0
            i3 += 1
            if i3 == l3:
                i3 = 0
    return outl, outr


def limit(x, gain, knee, ceiling):
    """Soft limiter with no memory: untouched below the knee, a tanh curve above it, never past
    the ceiling. Since it has no state it can't mess up the loop point."""
    span = ceiling - knee
    th = math.tanh
    return [v if -knee <= v <= knee else
            (knee + span * th((v - knee) / span) if v > 0 else -knee - span * th((-v - knee) / span))
            for v in map(mul, x, repeat(gain))]


# --------------------------------------------------------------------------- #
# Song mixer
# --------------------------------------------------------------------------- #

class Song:
    """One looping song: the beat grid, and the layers that get summed into the left and right
    channels and the reverb send."""

    def __init__(self, bpm, bars, beats=4, rate=MUSIC_RATE):
        self.rate = rate
        # The beat is snapped to a whole number of samples (a multiple of 12, so swung eighths and
        # triplets land on samples too). The loop is then exactly `bars` bars long, `beats` to a bar.
        self.beat = 12 * round(rate * 60.0 / bpm / 12)
        self.beats = beats
        self.bar = beats * self.beat
        self.length = bars * self.bar
        self.layers = []    # (samples, left gain, right gain, reverb send gain)

    def at(self, bar, beat=0.0):
        """Sample index of a 1-based bar and a 0-based beat inside it."""
        return (bar - 1) * self.bar + int(round(beat * self.beat))

    def secs(self, beats):
        return beats * self.beat / self.rate

    def track(self):
        return silence(self.length)

    def half_at(self, bar, beat=0.0):
        return self.at(bar, beat) // 2

    def half_track(self):
        return silence(self.length // 2)

    def add_half(self, track, gain_db=0.0, pan=0.0, send=0.0, label=""):
        """Adds a layer rendered at half rate, doubled back up and smoothed."""
        up = looped(lambda y: lowpass(y, 7000.0, self.rate), upsample2(track), self.rate, 0.01)
        self.add(up, gain_db, pan, send, label)

    def add(self, track, gain_db=0.0, pan=0.0, send=0.0, label=""):
        g = undb(gain_db)
        gl, gr = pan_gains(pan)
        self.layers.append((array("d", track), g * gl, g * gr, g * send))
        if VERBOSE:
            print("      %-12s rms %6.1f  peak %6.1f" % (label, db(rms(track) * g), db(peak(track) * g)))

    def add_stereo(self, left, right, gain_db=0.0, send=0.0, label=""):
        g = undb(gain_db)
        self.layers.append((array("d", left), g, 0.0, g * send * 0.5))
        self.layers.append((array("d", right), 0.0, g, g * send * 0.5))
        if VERBOSE:
            print("      %-12s rms %6.1f  peak %6.1f" % (
                label, db(rms(left, right) * g), db(peak(left, right) * g)))

    def bus(self, which):
        """Sums the layers into one bus (1 left, 2 right, 3 reverb send) in one pass with zip and sumprod."""
        picked = [(layer[0], layer[which]) for layer in self.layers if layer[which]]
        if not picked:
            return self.track()
        tracks = [t for t, _ in picked]
        gains = [g for _, g in picked]
        sp = math.sumprod
        return [sp(c, gains) for c in zip(*tracks)]

    def add_reverb(self, rt60, damp_hz, size, gain_db):
        # The reverb runs at half rate. The tail is dark anyway, so it can't be heard, and it's half the work.
        rate = self.rate
        send = looped(lambda x: lowpass(biquad(x, "lp", 6000.0, 0.7, rate), 7000.0, rate),
                      self.bus(3), rate, 0.05)
        wl, wr = reverb(send[0::2], rate // 2, rt60, damp_hz, size, loop=True)
        self.add_stereo(upsample2(wl), upsample2(wr), gain_db, label="reverb")

    def finish(self, rel, rms_db):
        rate = self.rate
        left = looped(lambda x: highpass(x, 18.0, rate), self.bus(1), rate)
        right = looped(lambda x: highpass(x, 18.0, rate), self.bus(2), rate)
        self.layers = []
        target = undb(rms_db)
        ceiling = undb(MUSIC_PEAK_DB)
        gain = target / rms(left, right)
        # The limiter takes a little RMS off the loud bits, so the gain gets re-aimed a few times.
        for _ in range(4):
            outl = limit(left, gain, LIMIT_KNEE, ceiling)
            outr = limit(right, gain, LIMIT_KNEE, ceiling)
            got = rms(outl, outr)
            if abs(db(got) - rms_db) < 0.05:
                break
            gain *= target / got
        if VERBOSE:
            print("      raw peak %.1f dB over rms, limiter gain %.1f dB" % (
                db(peak(left, right) / rms(left, right)), db(gain)))
        write_wav(rel, [outl, outr], rate)
        seam_report([outl, outr], rate)


def upsample2(x):
    """Doubles the rate of a loop by linear interpolation, wrapping at the end."""
    out = silence(2 * len(x))
    out[0::2] = x
    out[1::2] = [0.5 * (a + b) for a, b in zip(x, x[1:] + x[:1])]
    return out


def note_cache(render):
    """Memoises a note renderer. Most notes in a song repeat exactly, so each is only built once."""
    cache = {}

    def get(*key):
        if key not in cache:
            cache[key] = render(*key)
        return cache[key]
    return get


# --------------------------------------------------------------------------- #
# Instruments
# --------------------------------------------------------------------------- #
# Strings and choir render at half rate (16 kHz). Nothing they make goes much past 6 kHz, and it
# halves the slowest part of the run. Song.add_half doubles them back up.

VOWELS = {                  # (formant Hz, gain) per vowel, roughly a low choir
    "oo": ((320.0, 1.0), (800.0, 0.45), (2300.0, 0.15)),
    "oh": ((480.0, 1.0), (850.0, 0.55), (2500.0, 0.18)),
}
_BOW_HISS = {}

#: Where along the string a lute is plucked, as a fraction of its length. Near the bridge is brighter.
PLUCK_AT = 0.16

#: How far apart a lute's paired strings are tuned, in cents. Enough to shimmer, not enough to sound wrong.
LUTE_PAIR_CENTS = 3.0


def bow_hiss(n, rate, rng):
    """Bow noise for one note. It's a slice of one long strip of band-passed noise, so it only
    has to be filtered once."""
    if rate not in _BOW_HISS:
        _BOW_HISS[rate] = norm(biquad(noise(40 * rate, random.Random(rate)), "bp", 2000.0, 0.7, rate))
    strip = _BOW_HISS[rate]
    out = []
    while len(out) < n:
        start = rng.randrange(len(strip) // 2)
        out += strip[start:start + n - len(out)]
    return out


def bowed(freqs, dur, rate, rng, attack=1.0, release=1.5, vib_cents=10.0, bow=0.12, bright=1.0):
    """Bowed strings: two saws a few cents apart per note, one vibrato shared by all of them,
    low-passed for the body, plus bow hiss and slightly uneven bow pressure. dur is how long the
    bow stays on (s)."""
    n = int((dur + 3.0 * release) * rate)
    times = vibrato_time(n, rate, vib_cents, rng.uniform(4.3, 5.2), min(attack, 0.8))
    x = silence(n)
    for f in freqs:
        for c in (-4.0, 4.0):
            mix(x, saw_at(f * 2 ** (c / 1200), times, rate, 0.5, rng.random()))
    body = min(0.4 * rate, 300.0 + 5.0 * min(freqs) * bright)
    x = lowpass(lowpass(x, body, rate), body, rate)
    mix(x, bow_hiss(n, rate, rng), 0, bow * len(freqs))
    pressure = lines([(0.3 * k, 1.0 + 0.1 * rng.uniform(-1.0, 1.0)) for k in range(int(n / rate / 0.3) + 2)], n, rate)
    return shaped(shaped(x, envelope(n, rate, attack=attack, gate=dur, release=release)), pressure)


def choir(freqs, dur, rate, rng, vowel="oo", attack=3.0, release=2.0, breath=0.35):
    """Choir pad: two saws per voice plus some breath noise, all through three vowel formants."""
    n = int((dur + 3.0 * release) * rate)
    times = vibrato_time(n, rate, 8.0, rng.uniform(4.4, 5.0), 0.8)
    src = silence(n)
    for f in freqs:
        for c in (-7.0, 7.0):
            mix(src, saw_at(f * 2 ** (c / 1200), times, rate, 1.0, rng.random()))
    src = norm(src)
    mix(src, noise(n, rng), 0, breath)
    out = silence(n)
    for fc, g in VOWELS[vowel]:
        mix(out, biquad(src, "bp", fc, 5.0, rate), 0, g)
    return shaped(out, envelope(n, rate, attack=attack, gate=dur, release=release))


def lute(freq, rate, rng, vel=0.6, length=3.4):
    """A plucked lute string: harmonics that die away faster the higher they are, shaped by where
    the string was plucked, a second string a few cents off (lutes have their strings in pairs),
    the click of the nail, and the body ringing under it all. Softer notes lose their top first."""
    n = int(length * rate)
    slow = 1.5 * (196.0 / freq) ** 0.5
    parts, pair = [], []
    for k in range(1, 12):
        g = abs(math.sin(math.pi * k * PLUCK_AT)) / k ** 1.1 * vel ** (0.35 * (k - 1))
        tau = slow / (1.0 + 0.5 * (k - 1))
        parts.append((k, g, tau))
        if k <= 4:
            pair.append((k, 0.6 * g, tau))
    x = modes(parts, n, rate, freq)
    mix(x, modes(pair, n, rate, freq * 2 ** (LUTE_PAIR_CENTS / 1200)))
    m = int(0.004 * rate)
    nail = shaped(highpass(noise(m, rng), 2000.0, rate), envelope(m, rate, 0.0002, 0.0008))
    mix(x, norm(nail), 0, 0.04 + 0.04 * vel)
    mix(x, biquad(x, "bp", 230.0, 1.4, rate), 0, 0.5)
    return fade(lowpass(x, 1800.0 + 3200.0 * vel, rate), rate, 0.0005, 0.25)


def frame_drum(rate, rng):
    """A frame drum hit with the palm: a low thud that drops in pitch, and a little skin noise."""
    n = int(0.9 * rate)
    body = shaped(glide(110.0, 62.0, 0.04, n, rate), envelope(n, rate, attack=0.002, tau=0.16))
    skin = shaped(norm(lowpass(noise(n, rng), 600.0, rate)), envelope(n, rate, attack=0.001, tau=0.03))
    mix(body, skin, 0, 0.35)
    return body


def flute(freq, beats, rate, rng, vel=0.7):
    """A wooden flute: mostly the fundamental with a little of the two above it, breath under it,
    and a vibrato that only comes in once the note has settled. beats is how long it's held, in seconds."""
    release = 0.25
    n = int((beats + 3.0 * release) * rate)
    times = vibrato_time(n, rate, 12.0, rng.uniform(4.8, 5.4), 0.35)
    w = TAU * freq / rate
    s = math.sin
    tone = [s(w * t) + 0.18 * s(2.0 * w * t) + 0.08 * s(3.0 * w * t) for t in times]
    breath = norm(biquad(noise(n, rng), "bp", min(2.2 * freq, 0.4 * rate), 1.5, rate))
    mix(tone, breath, 0, 0.12 + 0.1 * (1.0 - vel))
    return shaped(scale(tone, vel), envelope(n, rate, attack=0.06, gate=beats, release=release))


def sweep(f0, f1, n, rate, amp=1.0):
    """A sine gliding exponentially from f0 to f1 over n samples."""
    if abs(f1 - f0) < 1e-6 or n < 2:
        return sine(f0, n, rate, amp)
    k = math.log(f1 / f0) / n
    c = TAU * f0 / (rate * k)
    s, e = math.sin, math.exp
    return [amp * s(c * (e(k * i) - 1.0)) for i in range(n)]


def drip(rate, rng):
    """A drop of water landing in a puddle: a quick falling blip, and a smaller one as it bounces."""
    n = int(0.25 * rate)
    f0 = rng.uniform(1400.0, 2000.0)
    x = shaped(sweep(f0, f0 * 0.6, n, rate), envelope(n, rate, attack=0.0005, tau=0.025))
    m = int(0.12 * rate)
    mix(x, shaped(sweep(f0 * 1.4, f0 * 0.9, m, rate, 0.3), envelope(m, rate, 0.0005, 0.012)), int(0.07 * rate))
    return x


#: How far flat a brass note starts, in cents, before the lips pull it up to pitch.
BRASS_SCOOP_CENTS = 30.0

#: How quickly a brass note's scoop settles, in seconds (a time constant).
BRASS_SCOOP_S = 0.03

#: How long the extra brightness at the front of a brass note lasts, in seconds (a time constant).
BRASS_BLARE_S = 0.12

#: How quickly a brass note sags once the breath goes, if it's asked to, in seconds (a time constant).
BRASS_FALL_S = 0.25

#: The furthest a brass note's low-pass ever opens, in Hz. Any brighter and the saws start to buzz.
BRASS_BRIGHTEST_HZ = 5000.0

#: A small bright bar struck with a hard mallet, nearly in tune with itself, like a celesta. (ratio, gain, tau)
CHIME = ((1.0, 1.0, 0.9), (2.0, 0.2, 0.4), (3.0, 0.07, 0.25), (4.1, 0.05, 0.12))

#: A timpani head. The kettle under it pulls its modes nearly into tune with each other. (ratio, gain, tau)
TIMPANI = ((1.0, 1.0, 1.0), (1.5, 0.5, 0.7), (1.98, 0.3, 0.5), (2.44, 0.18, 0.35), (2.94, 0.1, 0.25))

#: A tower bell cast with a major third in it instead of the usual minor one, so it rings in the
#: key instead of against it. (ratio, gain, tau)
TOWER_BELL = ((0.5, 0.45, 3.2), (1.0, 1.0, 2.6), (1.25, 0.4, 1.8), (1.5, 0.4, 1.5), (2.0, 0.35, 1.2),
              (2.52, 0.2, 0.8), (3.0, 0.18, 0.6))

#: A tambourine's zils, the little cymbals round its rim. (ratio, gain, tau)
ZILS = ((1.0, 1.0, 0.08), (1.47, 0.6, 0.06), (2.09, 0.35, 0.04))


def swept_lowpass(x, cutoffs, rate):
    """One-pole low-pass whose cutoff (Hz) can be different for every sample."""
    k = -TAU / rate
    e = math.exp
    y = 0.0
    return [y := y + (1.0 - e(k * fc)) * (v - y) for v, fc in zip(x, cutoffs)]


def brass(freqs, dur, rate, rng, vel=0.8, attack=0.05, release=0.3, vib_cents=5.0, fall_cents=0.0):
    """Brass: two saws a few cents apart per note, through a low-pass that opens as the note
    swells, so it gets brighter the harder it's blown, with a little blare at the front. Every note
    starts a shade flat and lips up to pitch, there's breath in it, and a vibrato comes in once it's
    held. dur is how long it's blown (s). With fall_cents it sags that far once the breath goes."""
    n = int((dur + 3.0 * release) * rate)
    times = vibrato_time(n, rate, vib_cents, rng.uniform(4.8, 5.6), min(0.5, 0.6 * dur))
    # Reading the saw at warped times bends its pitch: a time that runs slow is a note that's flat.
    lift = BRASS_SCOOP_S * rate * (1.0 - 2.0 ** (-BRASS_SCOOP_CENTS / 1200.0))
    times = [t - lift * (1.0 - d) for t, d in zip(times, decay(n, BRASS_SCOOP_S, rate))]
    g = int(dur * rate)
    if fall_cents and g < n:
        drop = 1.0 - 2.0 ** (-fall_cents / 1200.0)
        tau = BRASS_FALL_S * rate
        sag = decay(n - g, BRASS_FALL_S, rate)
        times[g:] = [t - drop * (k - tau * (1.0 - d)) for k, (t, d) in enumerate(zip(times[g:], sag))]
    x = silence(n)
    for f in freqs:
        for c in (-3.0, 3.0):
            mix(x, saw_at(f * 2 ** (c / 1200), times, rate, 0.5, rng.random()))
    env = envelope(n, rate, attack=attack, gate=dur, release=release)
    top = min(0.45 * rate, BRASS_BRIGHTEST_HZ)
    low = min(freqs)
    reach = 2.0 + 8.0 * vel
    cut = [min(top, low * (1.2 + reach * e * (1.0 + 0.4 * b))) for e, b in zip(env, decay(n, BRASS_BLARE_S, rate))]
    x = swept_lowpass(swept_lowpass(x, cut, rate), cut, rate)
    breath = shaped(norm(biquad(noise(n, rng), "bp", min(3.0 * low, 0.4 * rate), 1.0, rate)), decay(n, 0.08, rate))
    mix(x, breath, 0, 0.05 * len(freqs))
    return shaped(x, env)


def chime(freq, rate, rng, vel=0.7, length=2.2):
    """A chime: the bar's modes, and the tick of the mallet."""
    n = int(length * rate)
    x = modes(CHIME, n, rate, freq, vel)
    m = int(0.003 * rate)
    tick = shaped(highpass(noise(m, rng), 3000.0, rate), envelope(m, rate, 0.0002, 0.0006))
    mix(x, norm(tick), 0, 0.06 * vel)
    return fade(x, rate, 0.0005, 0.3)


def timpani(freq, rate, rng):
    """A timpani stroke: the head ringing at its pitch, the thud of the felt mallet dropping onto
    it, and a little skin noise."""
    n = int(2.4 * rate)
    x = norm(modes(TIMPANI, n, rate, freq))
    mix(x, shaped(glide(2.0 * freq, freq, 0.02, n, rate), envelope(n, rate, attack=0.002, tau=0.07)), 0, 0.7)
    m = int(0.06 * rate)
    mix(x, shaped(norm(lowpass(noise(m, rng), 1200.0, rate)), envelope(m, rate, attack=0.001, tau=0.015)), 0, 0.3)
    return fade(lowpass(x, 2500.0, rate), rate, 0.0005, 0.3)


def side_drum(rate, rng):
    """A field drum tapped softly, the snares buzzing under the head."""
    n = int(0.5 * rate)
    x = shaped(glide(260.0, 190.0, 0.02, n, rate), envelope(n, rate, attack=0.001, tau=0.05))
    snares = shaped(norm(biquad(noise(n, rng), "bp", 3200.0, 0.6, rate)), envelope(n, rate, attack=0.001, tau=0.09))
    mix(x, snares, 0, 0.6)
    return lowpass(x, 5000.0, rate)


def jingle(rate, rng):
    """A tambourine tapped once: a rattle of noise, and the zils ringing over it, each a little late."""
    n = int(0.35 * rate)
    x = shaped(norm(highpass(noise(n, rng), 6000.0, rate)), envelope(n, rate, attack=0.001, tau=0.05))
    for _ in range(4):
        mix(x, modes(ZILS, n, rate, rng.uniform(5000.0, 7000.0)), int(rng.uniform(0.0, 0.012) * rate), 0.3)
    return x


def tower_bell(freq, rate, rng):
    """A bell in a tower a long way off: struck once, ringing on, the top of it lost on the way."""
    n = int(5.0 * rate)
    x = norm(modes(TOWER_BELL, n, rate, freq))
    m = int(0.005 * rate)
    mix(x, norm(shaped(highpass(noise(m, rng), 1500.0, rate), envelope(m, rate, 0.0003, 0.001))), 0, 0.2)
    return fade(lowpass(lowpass(x, 2400.0, rate), 2400.0, rate), rate, 0.0005, 0.5)


def birdsong(length, rate, rng, songs):
    """A small bird somewhere out in the grass: little runs of whistled chirps sliding up or down,
    and now and then a trill instead. Placed round the loop so the end runs into the start."""
    x = silence(length)
    s = math.sin
    for _ in range(songs):
        at = rng.randrange(length)
        base = rng.uniform(2800.0, 4400.0)
        if rng.random() < 0.3:
            # A trill: one whistle warbling fast, swelling up and stopping.
            n = int(rng.uniform(0.35, 0.6) * rate)
            fm = rng.uniform(22.0, 32.0)
            w, u, v = TAU * base / rate, TAU * fm / rate, rng.uniform(300.0, 600.0) / fm
            call = [s(w * i + v * s(u * i)) for i in range(n)]
            mix_loop(x, shaped(call, envelope(n, rate, attack=0.08, gate=n / rate - 0.06, release=0.02)), at,
                     rng.uniform(0.4, 0.8))
            continue
        up = rng.random() < 0.6
        t = 0
        for _ in range(rng.randint(2, 6)):
            n = int(rng.uniform(0.035, 0.08) * rate)
            f0 = base * rng.uniform(0.94, 1.06)
            f1 = f0 * (rng.uniform(1.25, 1.6) if up else rng.uniform(0.6, 0.8))
            chirp = shaped(sweep(f0, f1, n, rate), envelope(n, rate, attack=0.004, tau=0.4 * n / rate))
            mix_loop(x, chirp, at + t, rng.uniform(0.5, 1.0))
            t += n + int(rng.uniform(0.03, 0.09) * rate)
    return x


# --------------------------------------------------------------------------- #
# embers.wav: title theme
# --------------------------------------------------------------------------- #
# A minor, 56 BPM, 12 bars (about 51 s). A lute picks slow broken chords over a bowed drone
# (Am, F, Dm, Em, then F, G and an E major that pulls back round to the A), a melody comes in on
# the lute an octave up for the second half, with a quiet choir under it. A frame drum on the
# downbeats, wind, and a fire crackling close by.

EMBERS_BPM, EMBERS_BARS = 56, 12

EMBERS_BARS_CHORDS = (     # one per bar: (drone MIDI, lute voicing low to high)
    ((45, 52), (57, 64, 69, 72)),   # Am
    ((45, 52), (57, 64, 69, 72)),   # Am
    ((41, 48), (53, 60, 65, 69)),   # F
    ((41, 48), (53, 60, 65, 69)),   # F
    ((38, 45), (50, 57, 62, 65)),   # Dm
    ((38, 45), (50, 57, 62, 65)),   # Dm
    ((40, 47), (52, 59, 64, 67)),   # Em
    ((40, 47), (52, 59, 64, 67)),   # Em
    ((41, 48), (53, 60, 65, 69)),   # F
    ((43, 50), (55, 62, 67, 71)),   # G
    ((40, 47), (52, 59, 64, 68)),   # E, with the G sharp that pulls back to A
    ((40, 47), (52, 59, 64, 68)),   # E
)

#: The broken chord the lute picks every bar: (beat, which note of the voicing, velocity).
EMBERS_PICKING = ((0.0, 0, 0.70), (1.0, 1, 0.50), (1.5, 2, 0.45), (2.0, 3, 0.55), (3.0, 2, 0.45), (3.5, 1, 0.40))

EMBERS_MELODY_FROM = 5     # the melody comes in here, and the picking steps back a little under it

EMBERS_MELODY = (          # (bar, beat, MIDI, velocity)
    (5, 0.0, 74, 0.75), (5, 1.0, 77, 0.60), (5, 2.0, 76, 0.60), (5, 3.0, 74, 0.55),
    (6, 0.0, 72, 0.70), (6, 2.0, 69, 0.55),
    (7, 0.0, 71, 0.70), (7, 1.0, 74, 0.60), (7, 2.0, 76, 0.65), (7, 3.0, 79, 0.60),
    (8, 0.0, 76, 0.70),
    (9, 0.0, 81, 0.80), (9, 1.0, 79, 0.60), (9, 2.0, 77, 0.60), (9, 2.5, 76, 0.50),
    (10, 0.0, 74, 0.70), (10, 1.5, 71, 0.55), (10, 2.0, 74, 0.60), (10, 3.0, 79, 0.60),
    (11, 0.0, 80, 0.80), (11, 2.0, 76, 0.60),
    (12, 0.0, 71, 0.60), (12, 2.0, 74, 0.55), (12, 3.0, 68, 0.60),    # G sharp, last, so the loop resolves it
)

EMBERS_CHOIR = (           # (first bar, bars, MIDI, vowel)
    (5, 2, (50, 53, 57), "oo"),
    (7, 2, (52, 55, 59), "oo"),
    (9, 1, (53, 57, 60), "oh"),
    (10, 1, (55, 59, 62), "oh"),
    (11, 2, (52, 56, 59), "oo"),
)

EMBERS_DRUM = ((0.0, 1.0), (2.0, 0.45))       # (beat, velocity), every bar
EMBERS_DRUM_PUSH = ((3.5, 0.3),)              # and this as well from bar 9, to lean into the turnaround

#: How many times a second the fire pops, on average.
CRACKLE_RATE = 3.5

#: How many slow gusts of wind fit in one loop. Whole numbers, so the loop has no seam.
WIND_GUSTS = 3


def crackle(length, rate, rng):
    """A fire close by: tiny pops at random, now and then a few at once, and the odd soft knock of
    a log settling. Placed round the loop so the end runs into the start."""
    x = silence(length)
    pops = int(length / rate * CRACKLE_RATE)
    for _ in range(pops):
        at = rng.randrange(length)
        for _ in range(1 if rng.random() < 0.8 else rng.randint(2, 5)):
            m = int(rng.uniform(0.001, 0.004) * rate)
            pop = shaped(biquad(noise(m, rng), "bp", rng.uniform(1500.0, 5000.0), 1.2, rate),
                         envelope(m, rate, 0.0002, m / rate * 0.3))
            mix_loop(x, pop, at, rng.uniform(0.2, 1.0))
            at += int(rng.uniform(0.01, 0.05) * rate)
    for _ in range(3):
        m = int(0.3 * rate)
        knock = shaped(glide(90.0, 55.0, 0.05, m, rate), envelope(m, rate, 0.003, 0.06))
        mix_loop(x, knock, rng.randrange(length), 0.25)
    return x


def wind(length, rate, rng):
    """Wind going past the wall: band-passed noise, swelling in slow gusts."""
    x = looped(lambda y: biquad(y, "bp", 520.0, 0.6, rate), noise(length, rng), rate)
    gusts = [0.55 + 0.45 * math.sin(TAU * WIND_GUSTS * i / length + 1.3) ** 2 for i in range(length)]
    return shaped(norm(x), gusts)


def build_embers():
    rng = random.Random(5601)
    s = Song(EMBERS_BPM, EMBERS_BARS)
    R, L, H = s.rate, s.length, s.rate // 2

    # The drone: one bow per chord, starting a second early and ringing over so the changes overlap.
    strings = s.half_track()
    for first, bars, notes in drone_spans(EMBERS_BARS_CHORDS):
        note = bowed([midi_hz(m) for m in notes], s.secs(4 * bars) + 1.0, H, rng, attack=1.8, release=1.0,
                     vib_cents=5.0, bow=0.06, bright=0.8)
        mix_loop(strings, note, s.half_at(first) - H)
    s.add_half(strings, -15.0, send=0.3, label="drone")

    # The lute: broken chords every bar, and the melody an octave up from bar 5. Each note is
    # nudged off the grid a little so it doesn't sound like a machine plucking it.
    pluck = note_cache(lambda m, vel: lute(midi_hz(m), R, rng, vel))
    picking = s.track()
    for bar, (_, voicing) in enumerate(EMBERS_BARS_CHORDS, start=1):
        under = 0.75 if bar >= EMBERS_MELODY_FROM else 1.0
        for beat, which, vel in EMBERS_PICKING:
            jitter = int(rng.gauss(0.0, 0.010) * R)
            mix_loop(picking, pluck(voicing[which], vel), s.at(bar, beat) + jitter, under * rng.uniform(0.92, 1.0))
    s.add(picking, -10.0, pan=-0.12, send=0.35, label="picking")

    melody = s.track()
    for bar, beat, m, vel in EMBERS_MELODY:
        jitter = int(rng.gauss(0.0, 0.008) * R)
        mix_loop(melody, pluck(m, vel), s.at(bar, beat) + jitter)
    s.add(melody, -6.0, pan=0.1, send=0.45, label="melody")

    # The choir, kept very quiet.
    ch = s.half_track()
    for first, bars, notes, vowel in EMBERS_CHOIR:
        c = choir([midi_hz(m) for m in notes], s.secs(4 * bars), H, rng, vowel, attack=2.5, release=1.5)
        mix_loop(ch, c, s.half_at(first) - H)
    s.add_half(ch, -16.0, pan=0.15, send=0.6, label="choir")

    # The frame drum.
    hit = frame_drum(R, rng)
    drum = s.track()
    for bar in range(1, EMBERS_BARS + 1):
        for beat, vel in EMBERS_DRUM + (EMBERS_DRUM_PUSH if bar >= 9 else ()):
            mix_loop(drum, hit, s.at(bar, beat), vel)
    s.add(drum, -11.0, send=0.3, label="drum")

    # Wind either side, and the fire in the middle.
    s.add_stereo(wind(L, R, rng), wind(L, R, rng), -25.0, label="wind")
    s.add(crackle(L, R, rng), -15.0, pan=0.05, send=0.1, label="fire")

    s.add_reverb(rt60=3.5, damp_hz=2500.0, size=1.2, gain_db=-9.0)
    s.finish("Content/Music/embers.wav", TITLE_RMS_DB)


# --------------------------------------------------------------------------- #
# hearthsmoke.wav: walking about on a lit map
# --------------------------------------------------------------------------- #
# D Dorian, 66 BPM, 16 bars (about 58 s). Folk rather than dread: the lute rolls through the
# chords in eighths, a wooden flute plays a tune over the second half, the frame drum keeps a slow
# walk, and the drone holds it down. The major IV (G) is what makes it Dorian, and keeps it from
# sounding as sad as the title. Ends on Am into Dm, so the loop resolves.

HEARTH_BPM, HEARTH_BARS = 66, 16

HEARTH_CHORDS = (          # one per bar: (drone MIDI, lute voicing low to high)
    ((38, 45), (50, 57, 62, 65)), ((38, 45), (50, 57, 62, 65)),     # Dm
    ((36, 43), (48, 55, 60, 64)), ((36, 43), (48, 55, 60, 64)),     # C
    ((43, 50), (55, 62, 67, 71)), ((43, 50), (55, 62, 67, 71)),     # G
    ((38, 45), (50, 57, 62, 65)), ((38, 45), (50, 57, 62, 65)),     # Dm
    ((41, 48), (53, 60, 65, 69)), ((41, 48), (53, 60, 65, 69)),     # F
    ((36, 43), (48, 55, 60, 64)), ((36, 43), (48, 55, 60, 64)),     # C
    ((43, 50), (55, 62, 67, 71)), ((43, 50), (55, 62, 67, 71)),     # G
    ((45, 52), (57, 64, 69, 72)),                                   # Am
    ((38, 45), (50, 57, 62, 65)),                                   # Dm
)

#: Rolling eighths through the voicing, every bar: (beat, which note, velocity).
HEARTH_PICKING = ((0.0, 0, 0.6), (0.5, 1, 0.4), (1.0, 2, 0.45), (1.5, 3, 0.4),
                  (2.0, 1, 0.5), (2.5, 2, 0.4), (3.0, 3, 0.45), (3.5, 2, 0.35))

HEARTH_TUNE_FROM = 5       # the flute comes in here, and the lute steps back a little under it

HEARTH_TUNE = (            # (bar, beat, MIDI, beats held)
    (5, 0.0, 74, 1.5), (5, 1.5, 76, 0.5), (5, 2.0, 74, 1.0), (5, 3.0, 71, 1.0),
    (6, 0.0, 69, 2.0), (6, 2.0, 71, 1.0), (6, 3.0, 74, 1.0),
    (7, 0.0, 76, 1.5), (7, 1.5, 77, 0.5), (7, 2.0, 76, 1.0), (7, 3.0, 74, 1.0),
    (8, 0.0, 74, 3.0),
    (9, 0.0, 77, 1.0), (9, 1.0, 76, 1.0), (9, 2.0, 74, 1.0), (9, 3.0, 72, 1.0),
    (10, 0.0, 69, 2.0), (10, 2.0, 72, 2.0),
    (11, 0.0, 76, 1.5), (11, 1.5, 74, 0.5), (11, 2.0, 72, 1.0), (11, 3.0, 67, 1.0),
    (12, 0.0, 72, 3.0),
    (13, 0.0, 71, 1.0), (13, 1.0, 74, 1.0), (13, 2.0, 79, 1.5), (13, 3.5, 77, 0.5),
    (14, 0.0, 76, 2.0), (14, 2.0, 74, 2.0),
    (15, 0.0, 72, 1.0), (15, 1.0, 71, 1.0), (15, 2.0, 69, 2.0),
    (16, 0.0, 74, 3.0),
)

HEARTH_DRUM = ((0.0, 0.8), (2.0, 0.5))
HEARTH_DRUM_LATE = ((1.5, 0.25), (3.5, 0.3))     # and these as well once the tune is in


def drone_spans(chords):
    """A song's drone as (first bar, bars, MIDI): one bow per run of bars on the same chord."""
    spans = []
    for bar, (drone, _) in enumerate(chords, start=1):
        if spans and spans[-1][2] == drone:
            first, bars, _ = spans[-1]
            spans[-1] = (first, bars + 1, drone)
        else:
            spans.append((bar, 1, drone))
    return spans


def build_hearthsmoke():
    rng = random.Random(6601)
    s = Song(HEARTH_BPM, HEARTH_BARS)
    R, L, H = s.rate, s.length, s.rate // 2

    strings = s.half_track()
    for first, bars, notes in drone_spans(HEARTH_CHORDS):
        note = bowed([midi_hz(m) for m in notes], s.secs(4 * bars) + 1.0, H, rng, attack=1.4, release=1.0,
                     vib_cents=6.0, bow=0.06, bright=0.9)
        mix_loop(strings, note, s.half_at(first) - H)
    s.add_half(strings, -17.0, send=0.25, label="drone")

    pluck = note_cache(lambda m, vel: lute(midi_hz(m), R, rng, vel, 2.6))
    picking = s.track()
    for bar, (_, voicing) in enumerate(HEARTH_CHORDS, start=1):
        under = 0.7 if bar >= HEARTH_TUNE_FROM else 1.0
        for beat, which, vel in HEARTH_PICKING:
            jitter = int(rng.gauss(0.0, 0.008) * R)
            mix_loop(picking, pluck(voicing[which], vel), s.at(bar, beat) + jitter, under * rng.uniform(0.9, 1.0))
    s.add(picking, -11.0, pan=-0.15, send=0.3, label="picking")

    tune = s.track()
    for bar, beat, m, held in HEARTH_TUNE:
        jitter = int(rng.gauss(0.0, 0.006) * R)
        mix_loop(tune, flute(midi_hz(m), s.secs(held) * 0.92, R, rng, 0.75), s.at(bar, beat) + jitter)
    s.add(tune, -10.0, pan=0.15, send=0.45, label="flute")

    hit = frame_drum(R, rng)
    drum = s.track()
    for bar in range(1, HEARTH_BARS + 1):
        for beat, vel in HEARTH_DRUM + (HEARTH_DRUM_LATE if bar >= HEARTH_TUNE_FROM else ()):
            mix_loop(drum, hit, s.at(bar, beat), vel)
    s.add(drum, -12.0, send=0.25, label="drum")

    s.add_stereo(wind(L, R, rng), wind(L, R, rng), -30.0, label="wind")

    s.add_reverb(rt60=2.6, damp_hz=3000.0, size=1.0, gain_db=-11.0)
    s.finish("Content/Music/hearthsmoke.wav", EXPLORE_RMS_DB)


# --------------------------------------------------------------------------- #
# salt-and-lanterns.wav: the ship at night
# --------------------------------------------------------------------------- #
# E minor, 60 BPM with every beat in three, so it rocks like a hull (12/8 in all but name), 16 bars
# (64 s). The lute picks low-high-middle in each beat, the flute comes in at bar 5 with a tune that
# keeps lifting to the G and coming back, a frame drum knocks like the hull on a swell, and under
# it all the sea rolls in and out, four long waves to a loop. Ends on Em, so it goes round.

SEA_BPM, SEA_BARS = 60, 16

SEA_CHORDS = (             # one per bar: (drone MIDI, lute voicing low to high)
    ((40, 47), (52, 59, 64, 67)), ((36, 43), (48, 55, 60, 64)),     # Em, C
    ((43, 50), (55, 59, 62, 67)), ((38, 45), (50, 57, 62, 66)),     # G, D
    ((40, 47), (52, 59, 64, 67)), ((36, 43), (48, 55, 60, 64)),     # Em, C
    ((38, 45), (50, 57, 62, 66)), ((38, 45), (50, 57, 62, 66)),     # D, D
    ((40, 47), (52, 59, 64, 67)), ((36, 43), (48, 55, 60, 64)),     # Em, C
    ((43, 50), (55, 59, 62, 67)), ((38, 45), (50, 57, 62, 66)),     # G, D
    ((36, 43), (48, 55, 60, 64)), ((38, 45), (50, 57, 62, 66)),     # C, D
    ((40, 47), (52, 59, 64, 67)), ((40, 47), (52, 59, 64, 67)),     # Em, Em
)

#: Each beat in three: the low note on the beat, then the top, then the middle. (beat part, which note, velocity)
SEA_PICKING = ((0.0, 0, 0.55), (1 / 3, 3, 0.35), (2 / 3, 2, 0.3))

SEA_TUNE_FROM = 5

SEA_TUNE = (               # (bar, beat, MIDI, beats held)
    (5, 0.0, 71, 4 / 3), (5, 4 / 3, 74, 2 / 3), (5, 2.0, 76, 4 / 3), (5, 10 / 3, 74, 2 / 3),
    (6, 0.0, 72, 2.0), (6, 2.0, 71, 1.0), (6, 3.0, 67, 1.0),
    (7, 0.0, 66, 4 / 3), (7, 4 / 3, 69, 2 / 3), (7, 2.0, 74, 2.0),
    (8, 0.0, 69, 3.0), (8, 3.0, 66, 1.0),
    (9, 0.0, 71, 4 / 3), (9, 4 / 3, 74, 2 / 3), (9, 2.0, 79, 4 / 3), (9, 10 / 3, 78, 2 / 3),
    (10, 0.0, 76, 2.0), (10, 2.0, 72, 1.0), (10, 3.0, 71, 1.0),
    (11, 0.0, 74, 4 / 3), (11, 4 / 3, 71, 2 / 3), (11, 2.0, 67, 1.0), (11, 3.0, 71, 1.0),
    (12, 0.0, 69, 4.0),
    (13, 0.0, 72, 4 / 3), (13, 4 / 3, 71, 2 / 3), (13, 2.0, 69, 1.0), (13, 3.0, 67, 1.0),
    (14, 0.0, 66, 4 / 3), (14, 4 / 3, 69, 2 / 3), (14, 2.0, 74, 2.0),
    (15, 0.0, 71, 4 / 3), (15, 4 / 3, 69, 2 / 3), (15, 2.0, 67, 1.0), (15, 3.0, 66, 1.0),
    (16, 0.0, 64, 3.5),
)

#: The hull knocking on the swell: (beat, velocity).
SEA_DRUM = ((0.0, 0.55), (2.0, 0.35))

#: How many waves roll in and out in one loop. It has to be whole, so the loop has no seam.
SEA_WAVES = 4


def sea_swell(length, rate, rng):
    """The sea against the hull: low-passed noise, rising and falling in long waves."""
    x = looped(lambda y: lowpass(lowpass(y, 420.0, rate), 420.0, rate), noise(length, rng), rate)
    waves = [0.2 + 0.8 * math.sin(math.pi * SEA_WAVES * i / length) ** 2 for i in range(length)]
    return shaped(norm(x), waves)


def build_salt_and_lanterns():
    rng = random.Random(6002)
    s = Song(SEA_BPM, SEA_BARS)
    R, L, H = s.rate, s.length, s.rate // 2

    strings = s.half_track()
    for first, bars, notes in drone_spans(SEA_CHORDS):
        note = bowed([midi_hz(m) for m in notes], s.secs(4 * bars) + 1.0, H, rng, attack=1.6, release=1.2,
                     vib_cents=5.0, bow=0.05, bright=0.7)
        mix_loop(strings, note, s.half_at(first) - H)
    s.add_half(strings, -18.0, send=0.35, label="drone")

    pluck = note_cache(lambda m, vel: lute(midi_hz(m), R, rng, vel, 2.4))
    picking = s.track()
    for bar, (_, voicing) in enumerate(SEA_CHORDS, start=1):
        under = 0.7 if bar >= SEA_TUNE_FROM else 1.0
        for beat in range(4):
            for part, which, vel in SEA_PICKING:
                jitter = int(rng.gauss(0.0, 0.008) * R)
                mix_loop(picking, pluck(voicing[which], vel), s.at(bar, beat + part) + jitter, under * rng.uniform(0.88, 1.0))
    s.add(picking, -12.0, pan=-0.2, send=0.35, label="picking")

    tune = s.track()
    for bar, beat, m, held in SEA_TUNE:
        jitter = int(rng.gauss(0.0, 0.006) * R)
        mix_loop(tune, flute(midi_hz(m), s.secs(held) * 0.92, R, rng, 0.7), s.at(bar, beat) + jitter)
    s.add(tune, -11.0, pan=0.2, send=0.55, label="flute")

    hit = frame_drum(R, rng)
    drum = s.track()
    for bar in range(SEA_TUNE_FROM, SEA_BARS + 1):
        for beat, vel in SEA_DRUM:
            mix_loop(drum, hit, s.at(bar, beat), vel)
    s.add(drum, -16.0, send=0.3, label="drum")

    s.add_stereo(sea_swell(L, R, rng), sea_swell(L, R, rng), -19.0, label="sea")
    s.add_stereo(wind(L, R, rng), wind(L, R, rng), -32.0, label="wind")

    s.add_reverb(rt60=3.0, damp_hz=2800.0, size=1.0, gain_db=-10.0)
    s.finish("Content/Music/salt-and-lanterns.wav", EXPLORE_RMS_DB)


# --------------------------------------------------------------------------- #
# cold-stone.wav: walking about underground
# --------------------------------------------------------------------------- #
# C Phrygian, 48 BPM, 10 bars (50 s). Hardly a tune at all: a low drone on C and G, a choir that
# swells in and leans on the flat second, a few low notes from the lute far apart, one deep boom,
# cold air moving, and water dripping somewhere in a long reverb.

COLD_BPM, COLD_BARS = 48, 10

COLD_CHOIR = (             # (first bar, bars, MIDI, vowel)
    (1, 4, (48, 55), "oo"),
    (5, 3, (49, 55, 60), "oh"),        # D flat against the C drone
    (8, 3, (48, 51, 55), "oo"),
)

COLD_NOTES = (             # (bar, beat, MIDI, velocity) on the lute, far apart
    (1, 0.0, 48, 0.5), (2, 2.0, 51, 0.45), (3, 1.0, 49, 0.5), (4, 0.0, 48, 0.4),
    (6, 2.0, 55, 0.45), (7, 0.0, 56, 0.5), (8, 3.0, 53, 0.4), (9, 0.0, 49, 0.5),
)

COLD_BOOM = (6, 0.0)

#: How many drips land in one loop.
COLD_DRIPS = 9


def build_cold_stone():
    rng = random.Random(4801)
    s = Song(COLD_BPM, COLD_BARS)
    R, L, H = s.rate, s.length, s.rate // 2

    # The drone is the whole loop long and wraps round on itself, so it never changes bow.
    strings = s.half_track()
    mix_loop(strings, bowed([midi_hz(36), midi_hz(43)], s.secs(4 * COLD_BARS) + 1.0, H, rng, attack=2.0,
                            release=1.5, vib_cents=4.0, bow=0.05, bright=0.6), -H)
    s.add_half(strings, -15.0, send=0.35, label="drone")

    ch = s.half_track()
    for first, bars, notes, vowel in COLD_CHOIR:
        c = choir([midi_hz(m) for m in notes], s.secs(4 * bars), H, rng, vowel, attack=4.0, release=2.5)
        mix_loop(ch, c, s.half_at(first) - H)
    s.add_half(ch, -12.0, pan=-0.1, send=0.7, label="choir")

    low = s.track()
    for bar, beat, m, vel in COLD_NOTES:
        mix_loop(low, lute(midi_hz(m) * 2 ** (-8 / 1200), R, rng, vel, 4.0), s.at(bar, beat))
    s.add(low, -12.0, pan=0.1, send=0.6, label="lute")

    boom = s.track()
    n = int(3.5 * R)
    hit = shaped(glide(58.0, 30.0, 0.4, n, R), envelope(n, R, attack=0.004, tau=0.8))
    mix(hit, shaped(norm(lowpass(lowpass(noise(n, rng), 120.0, R), 120.0, R)), envelope(n, R, attack=0.01, tau=0.5)), 0, 0.3)
    mix_loop(boom, hit, s.at(*COLD_BOOM))
    s.add(boom, -13.0, send=0.5, label="boom")

    drips = s.track()
    for _ in range(COLD_DRIPS):
        mix_loop(drips, drip(R, rng), rng.randrange(L), rng.uniform(0.4, 1.0))
    s.add(drips, -20.0, pan=rng.uniform(-0.5, 0.5), send=1.0, label="drips")

    air = looped(lambda x: lowpass(lowpass(x, 300.0, R), 300.0, R), noise(L, rng), R)
    s.add(shaped(norm(air), [0.6 + 0.4 * math.sin(TAU * 2 * i / L) ** 2 for i in range(L)]), -26.0, label="air")

    s.add_reverb(rt60=6.0, damp_hz=1800.0, size=1.5, gain_db=-7.0)
    s.finish("Content/Music/cold-stone.wav", DARK_RMS_DB)


# --------------------------------------------------------------------------- #
# meadowlark.wav: out in the green fields round Questeria
# --------------------------------------------------------------------------- #
# G major, 136 BPM in three, 48 bars (about 64 s). The only waltz: the lute goes low-strum-strum
# under a flute tune that leaps up a sixth and skips back down, like a lark going up, and says it
# twice, once out to D and once home to G. The middle climbs in sequences to the high E over held
# strings while the lute rolls in eighths, then the tune comes round a last time with chimes an
# octave over it and a tambourine. A frame drum on the downbeats, small birds out in the grass
# either side, a breeze. The chimes call the lark's leap once more, and the D7 brings it home.

MEADOW_BPM, MEADOW_BARS, MEADOW_BEATS = 136, 48, 3

MEADOW_VOICINGS = {        # chord: (strings MIDI, lute voicing low to high)
    "G": ((43, 50), (55, 59, 62, 67)),
    "C": ((48, 55), (48, 55, 60, 64)),
    "D": ((38, 45), (50, 57, 62, 66)),
    "D7": ((38, 45), (50, 57, 60, 66)),
    "Em": ((40, 47), (52, 59, 64, 67)),
    "Am": ((45, 52), (57, 60, 64, 69)),
    "Bm": ((47, 54), (47, 54, 59, 62)),
}

MEADOW_BARS_CHORDS = (     # one chord a bar
    "G", "C", "G", "D7",                                # 1-4: the lute on its own
    "G", "G", "C", "Am", "D", "Em", "Am", "D7",         # 5-12: the tune, out to D
    "G", "G", "C", "Am", "D", "Am", "D7", "G",          # 13-20: and home to G
    "C", "D", "Bm", "Em", "C", "D7", "Em", "D7",        # 21-28: the middle, climbing
    "C", "G", "Am", "Em", "Am", "D", "C", "D7",         # 29-36: and down again
    "G", "G", "C", "Am", "D", "Am", "D7", "G",          # 37-44: the tune with the chimes on it
    "G", "C", "Am", "D7",                               # 45-48: the lark's call, and round again
)

#: The lute's waltz, every bar: the low note on the beat, then two strings strummed together on
#: each of the other two. (beat, which notes, velocity)
MEADOW_WALTZ = ((0.0, (0,), 0.62), (1.0, (2, 3), 0.42), (2.0, (1, 3), 0.36))

#: Rolling eighths up and back down the voicing, for the middle. (beat, which notes, velocity)
MEADOW_ROLL = ((0.0, (0,), 0.6), (0.5, (1,), 0.38), (1.0, (2,), 0.42), (1.5, (3,), 0.4), (2.0, (2,), 0.4),
               (2.5, (1,), 0.34))

#: How far apart the strings of a strum sound, in seconds.
MEADOW_STRUM_S = 0.014

MEADOW_TUNE_BARS = range(5, 45)      # the flute's in, and the lute steps back a little under it
MEADOW_ROLLING = range(21, 37)       # the lute rolls in eighths instead of waltzing
MEADOW_HELD = range(21, 45)          # the strings hold the chord, and the drum fills in the waltz
MEADOW_BRIGHT = range(37, 45)        # chimes on the tune, and the tambourine

#: How loud the held chord is against the strings' low notes.
MEADOW_HELD_LEVEL = 0.6

MEADOW_HEAD = (             # the first five bars of the tune: (bar from its start, beat, MIDI, beats held)
    (0, 0.0, 74, 1.0), (0, 1.0, 83, 1.5), (0, 2.5, 81, 0.5),
    (1, 0.0, 79, 2.0), (1, 2.0, 74, 1.0),
    (2, 0.0, 76, 1.0), (2, 1.0, 84, 1.5), (2, 2.5, 83, 0.5),
    (3, 0.0, 81, 2.0), (3, 2.0, 76, 1.0),
    (4, 0.0, 78, 1.0), (4, 1.0, 81, 1.0), (4, 2.0, 86, 1.0),
)

MEADOW_OUT = (              # its first ending, stopping on the D7
    (5, 0.0, 83, 1.5), (5, 1.5, 81, 0.5), (5, 2.0, 79, 1.0),
    (6, 0.0, 76, 1.5), (6, 1.5, 78, 0.5), (6, 2.0, 79, 1.0),
    (7, 0.0, 81, 1.0), (7, 1.0, 78, 1.5),
)

MEADOW_HOME = (             # its second ending, down the D7 and home to G
    (5, 0.0, 84, 1.5), (5, 1.5, 83, 0.5), (5, 2.0, 81, 1.0),
    (6, 0.0, 86, 1.0), (6, 1.0, 84, 1.0), (6, 2.0, 81, 0.5), (6, 2.5, 78, 0.5),
    (7, 0.0, 79, 2.5),
)

#: Where the tune starts each time round, and which ending it takes.
MEADOW_TUNE_AT = ((5, MEADOW_OUT), (13, MEADOW_HOME), (37, MEADOW_HOME))

MEADOW_MIDDLE = (           # (bar, beat, MIDI, beats held)
    (21, 0.0, 76, 2.0), (21, 2.0, 79, 1.0),
    (22, 0.0, 78, 2.0), (22, 2.0, 81, 1.0),
    (23, 0.0, 83, 1.5), (23, 1.5, 81, 0.5), (23, 2.0, 78, 1.0),
    (24, 0.0, 79, 2.0), (24, 2.0, 76, 1.0),
    (25, 0.0, 79, 1.0), (25, 1.0, 84, 1.5), (25, 2.5, 83, 0.5),
    (26, 0.0, 81, 1.0), (26, 1.0, 86, 1.5), (26, 2.5, 84, 0.5),
    (27, 0.0, 83, 1.0), (27, 1.0, 88, 2.0),                       # the high E, the top of the song
    (28, 0.0, 86, 1.0), (28, 1.0, 84, 1.0), (28, 2.0, 81, 1.0),
    (29, 0.0, 76, 1.0), (29, 1.0, 79, 1.0), (29, 2.0, 84, 1.0),
    (30, 0.0, 83, 2.0), (30, 2.0, 79, 1.0),
    (31, 0.0, 81, 1.0), (31, 1.0, 84, 1.0), (31, 2.0, 88, 1.0),
    (32, 0.0, 86, 1.5), (32, 1.5, 83, 0.5), (32, 2.0, 79, 1.0),
    (33, 0.0, 81, 1.5), (33, 1.5, 79, 0.5), (33, 2.0, 76, 1.0),
    (34, 0.0, 78, 2.0), (34, 2.0, 74, 1.0),
    (35, 0.0, 76, 1.0), (35, 1.0, 79, 1.0), (35, 2.0, 84, 1.0),
    (36, 0.0, 81, 1.5), (36, 1.5, 79, 0.5), (36, 2.0, 78, 0.5), (36, 2.5, 76, 0.5),   # running down into the tune
)

MEADOW_LARK = (             # the chimes calling the tune's leap at the end: (bar, beat, MIDI, velocity)
    (45, 0.0, 86, 0.7), (45, 1.0, 95, 0.6), (45, 2.5, 93, 0.5), (46, 0.0, 91, 0.6),
)

MEADOW_DRUM = ((0.0, 0.6),)                       # (beat, velocity), every bar once the tune's in
MEADOW_DRUM_LILT = ((1.0, 0.2), (2.0, 0.2))       # and these as well while the strings hold
MEADOW_JINGLE = ((1.0, 0.7), (2.0, 0.55))         # the tambourine, the last time through the tune

#: How loud the tambourine is against the drum.
MEADOW_JINGLE_LEVEL = 0.25

#: How many little songs each bird sings in one loop.
MEADOW_BIRD_SONGS = 9

#: How loud the breeze is against the birds.
MEADOW_BREEZE = 0.5


def meadow_tune():
    """The flute's whole part as (bar, beat, MIDI, beats held): the head of the tune with each of
    its endings where they come, and the middle as written."""
    notes = []
    for first, ending in MEADOW_TUNE_AT:
        notes += [(first + bar, beat, m, held) for bar, beat, m, held in MEADOW_HEAD + ending]
    return notes + list(MEADOW_MIDDLE)


def build_meadowlark():
    rng = random.Random(13601)
    s = Song(MEADOW_BPM, MEADOW_BARS, MEADOW_BEATS)
    R, L, H = s.rate, s.length, s.rate // 2
    chords = [MEADOW_VOICINGS[name] for name in MEADOW_BARS_CHORDS]

    # The strings: the chord's two low notes bowed lightly under everything, one bow per run of
    # bars on the same chord, and in the middle the rest of the chord held over them, a bow a bar.
    strings = s.half_track()
    for first, bars, notes in drone_spans(chords):
        note = bowed([midi_hz(m) for m in notes], s.secs(s.beats * bars) + 0.25, H, rng, attack=0.5, release=0.6,
                     vib_cents=5.0, bow=0.05, bright=1.0)
        mix_loop(strings, note, s.half_at(first) - H // 4)
    for bar in MEADOW_HELD:
        _, voicing = chords[bar - 1]
        note = bowed([midi_hz(m) for m in voicing[1:]], s.secs(s.beats) + 0.15, H, rng, attack=0.35, release=0.5,
                     vib_cents=8.0, bow=0.08, bright=1.2)
        mix_loop(strings, note, s.half_at(bar) - int(0.15 * H), MEADOW_HELD_LEVEL)
    s.add_half(strings, -19.0, send=0.3, label="strings")
    # Every finished layer is dropped as soon as it's added, here and below. The song keeps its own
    # copy, and holding on to them all doubles what the run needs.
    del strings, note

    # The lute: the waltz, rolling eighths in the middle, every strum a little spread.
    pluck = note_cache(lambda m, vel: lute(midi_hz(m), R, rng, vel, 2.2))
    strum = int(MEADOW_STRUM_S * R)
    picking = s.track()
    for bar, (_, voicing) in enumerate(chords, start=1):
        under = 0.7 if bar in MEADOW_TUNE_BARS else 1.0
        for beat, which, vel in MEADOW_ROLL if bar in MEADOW_ROLLING else MEADOW_WALTZ:
            at = s.at(bar, beat) + int(rng.gauss(0.0, 0.007) * R)
            g = under * rng.uniform(0.9, 1.0)
            for k, w in enumerate(which):
                mix_loop(picking, pluck(voicing[w], vel), at + k * strum, g)
    s.add(picking, -12.5, pan=-0.15, send=0.3, label="picking")
    del picking, pluck

    tune = s.track()
    for bar, beat, m, held in meadow_tune():
        jitter = int(rng.gauss(0.0, 0.006) * R)
        vel = 0.8 if beat == 0.0 else 0.72
        mix_loop(tune, flute(midi_hz(m), s.secs(held) * 0.9, R, rng, vel), s.at(bar, beat) + jitter)
    s.add(tune, -9.0, pan=0.12, send=0.4, label="flute")
    del tune

    # The chimes: the tune an octave up the last time round, then the lark's call on its own.
    ring = note_cache(lambda m, vel: chime(midi_hz(m), R, rng, vel))
    bells = s.track()
    for bar, beat, m, _ in meadow_tune():
        if bar in MEADOW_BRIGHT:
            mix_loop(bells, ring(m + 12, 0.6), s.at(bar, beat))
    for bar, beat, m, vel in MEADOW_LARK:
        mix_loop(bells, ring(m, vel), s.at(bar, beat))
    s.add(bells, -17.0, pan=-0.25, send=0.5, label="chimes")
    del bells, ring

    # The frame drum, and the tambourine with it at the end.
    hit = frame_drum(R, rng)
    shakes = [jingle(R, rng) for _ in range(3)]
    drums = s.track()
    for bar in range(MEADOW_TUNE_BARS.start, MEADOW_BARS + 1):
        for beat, vel in MEADOW_DRUM + (MEADOW_DRUM_LILT if bar in MEADOW_HELD else ()):
            mix_loop(drums, hit, s.at(bar, beat), vel)
        if bar in MEADOW_BRIGHT:
            for beat, vel in MEADOW_JINGLE:
                mix_loop(drums, rng.choice(shakes), s.at(bar, beat), vel * MEADOW_JINGLE_LEVEL)
    s.add(drums, -14.0, send=0.25, label="drums")
    del drums

    # A bird either side, and the breeze under them.
    left = birdsong(L, R, rng, MEADOW_BIRD_SONGS)
    right = birdsong(L, R, rng, MEADOW_BIRD_SONGS)
    mix(left, wind(L, R, rng), 0, MEADOW_BREEZE)
    mix(right, wind(L, R, rng), 0, MEADOW_BREEZE)
    s.add_stereo(left, right, -24.0, send=0.4, label="birds")
    del left, right

    s.add_reverb(rt60=2.2, damp_hz=3500.0, size=0.9, gain_db=-12.0)
    s.finish("Content/Music/meadowlark.wav", EXPLORE_RMS_DB)


# --------------------------------------------------------------------------- #
# sunspire.wav: the royal keep
# --------------------------------------------------------------------------- #
# D major leaning Mixolydian (the C major chord is the old colour, like a chant), 72 BPM, 18 bars
# (60 s). Bells toll a long way off and two horns call over the strings while the timpani roll in.
# Then a trumpet plays a stately tune in dotted steps over held horn chords, a harp runs up at every
# change and the timpani mark one and three. The second time the violins take the tune up to the
# high D, the trumpet goes under them in long notes, a choir comes in, chimes ring on the downbeats
# and a field drum walks it like a procession. It ends going C to A, so the loop comes round to D.

COURT_BPM, COURT_BARS = 72, 18

COURT_VOICINGS = {         # chord: (bass MIDI, horns low to high, strings low to high)
    "D": (38, (62, 66, 69), (50, 57, 62, 66)),
    "Asus": (45, (62, 64, 69), (52, 57, 62, 64)),
    "A": (45, (61, 64, 69), (52, 57, 61, 64)),
    "G": (43, (62, 67, 71), (50, 55, 59, 62)),
    "Bm": (47, (62, 66, 71), (54, 59, 62, 66)),
    "C": (48, (60, 64, 67), (52, 55, 60, 64)),
    "Em": (40, (59, 64, 67), (52, 59, 64, 67)),
}

COURT_BARS_CHORDS = (      # one chord a bar, or two for a bar that changes halfway
    ("D",), ("Asus", "A"),                                                    # 1-2: the horns' call
    ("D",), ("G",), ("D",), ("A",), ("Bm",), ("G",), ("C",), ("Asus", "A"),   # 3-10: the trumpet's tune
    ("D",), ("G",), ("Bm",), ("G",), ("C",), ("Em",), ("C",), ("Asus", "A"),  # 11-18: the violins take it up
)

COURT_THEME_FROM = 3       # the tune starts here, and the horns hold the chords from here
COURT_STRINGS_FROM = 11    # the violins have it from here, with the choir and the field drum

COURT_CALL = (             # the two horns calling: (bar, beat, MIDI, beats held)
    (1, 0.0, (62, 69), 1.5), (1, 1.5, (62, 69), 0.5), (1, 2.0, (66, 74), 2.0),
    (2, 0.0, (69, 76), 1.5), (2, 1.5, (69, 76), 0.5), (2, 2.0, (69, 73), 2.0),
)

COURT_THEME = (            # the trumpet: (bar, beat, MIDI, beats held)
    (3, 0.0, 74, 1.5), (3, 1.5, 69, 0.5), (3, 2.0, 74, 1.0), (3, 3.0, 78, 1.0),
    (4, 0.0, 79, 2.0), (4, 2.0, 78, 1.0), (4, 3.0, 76, 1.0),
    (5, 0.0, 78, 1.5), (5, 1.5, 76, 0.5), (5, 2.0, 74, 1.0), (5, 3.0, 69, 1.0),
    (6, 0.0, 71, 1.0), (6, 1.0, 73, 1.0), (6, 2.0, 76, 2.0),
    (7, 0.0, 74, 1.5), (7, 1.5, 73, 0.5), (7, 2.0, 71, 1.0), (7, 3.0, 78, 1.0),
    (8, 0.0, 79, 1.5), (8, 1.5, 78, 0.5), (8, 2.0, 76, 1.0), (8, 3.0, 74, 1.0),
    (9, 0.0, 76, 1.5), (9, 1.5, 74, 0.5), (9, 2.0, 72, 1.0), (9, 3.0, 76, 1.0),     # the C natural
    (10, 0.0, 74, 2.0), (10, 2.0, 73, 2.0),
)

COURT_VIOLINS = (          # the tune again, climbing: (bar, beat, MIDI, beats held)
    (11, 0.0, 74, 1.5), (11, 1.5, 69, 0.5), (11, 2.0, 74, 1.0), (11, 3.0, 78, 1.0),
    (12, 0.0, 79, 2.0), (12, 2.0, 83, 1.0), (12, 3.0, 81, 1.0),
    (13, 0.0, 78, 1.5), (13, 1.5, 79, 0.5), (13, 2.0, 81, 1.0), (13, 3.0, 83, 1.0),
    (14, 0.0, 86, 2.0), (14, 2.0, 83, 1.0), (14, 3.0, 79, 1.0),                      # the high D
    (15, 0.0, 84, 1.5), (15, 1.5, 83, 0.5), (15, 2.0, 81, 1.0), (15, 3.0, 79, 1.0),
    (16, 0.0, 83, 1.5), (16, 1.5, 81, 0.5), (16, 2.0, 79, 1.0), (16, 3.0, 74, 1.0),
    (17, 0.0, 76, 1.0), (17, 1.0, 79, 1.0), (17, 2.0, 84, 2.0),
    (18, 0.0, 86, 2.0), (18, 2.0, 85, 2.0),
)

COURT_COUNTER = (          # the trumpet under the violins, in long notes: (bar, beat, MIDI, beats held)
    (11, 0.0, 66, 2.0), (11, 2.0, 69, 2.0), (12, 0.0, 71, 4.0),
    (13, 0.0, 71, 2.0), (13, 2.0, 74, 2.0), (14, 0.0, 74, 4.0),
    (15, 0.0, 76, 2.0), (15, 2.0, 72, 2.0), (16, 0.0, 71, 2.0), (16, 2.0, 67, 2.0),
    (17, 0.0, 67, 2.0), (17, 2.0, 72, 2.0), (18, 0.0, 74, 2.0), (18, 2.0, 73, 2.0),
)

COURT_BELLS = ((1, 0.0, 62, 1.0), (1, 2.0, 57, 0.8), (2, 0.0, 62, 0.65))   # (bar, beat, MIDI, velocity)

COURT_TIMPANI = ((0.0, 0.9), (2.0, 0.5))          # (beat, velocity), every bar once the tune's in
COURT_TIMPANI_LATE = ((3.5, 0.35),)               # and this as well with the violins, leading on

#: The timpani roll into the tune: how many strokes across bar 2, and how loud the first and last are.
COURT_ROLL_STROKES = 24
COURT_ROLL_FROM, COURT_ROLL_TO = 0.1, 0.55

COURT_SIDE_DRUM = ((0.0, 0.5), (1.5, 0.22), (1.75, 0.22), (2.0, 0.45), (3.0, 0.3))

#: How fast the harp runs up a chord, in beats a note.
COURT_HARP_STEP = 1 / 6

#: How loud the strings' chord is against their bass, the held horns against the call, the
#: trumpet's long notes against its tune, the chimes against the harp and the field drum against
#: the timpani.
COURT_UPPER_STRINGS = 0.7
COURT_HELD_HORNS = 0.55
COURT_COUNTER_LEVEL = 0.5
COURT_CHIME_LEVEL = 0.5
COURT_SIDE_DRUM_LEVEL = 0.35


def court_spans():
    """Every chord in the song as (bar, beat, beats, chord): one per bar, or two for a bar that
    changes halfway."""
    spans = []
    for bar, names in enumerate(COURT_BARS_CHORDS, start=1):
        beats = 4.0 / len(names)
        spans += [(bar, k * beats, beats, name) for k, name in enumerate(names)]
    return spans


def build_sunspire():
    rng = random.Random(7201)
    s = Song(COURT_BPM, COURT_BARS)
    R, L, H = s.rate, s.length, s.rate // 2

    # The strings: the bass in octaves and the chord over it, bowed again at every change.
    strings = s.half_track()
    for bar, beat, beats, name in court_spans():
        bass, _, upper = COURT_VOICINGS[name]
        at = s.half_at(bar, beat) - int(0.2 * H)
        low = bowed([midi_hz(bass), midi_hz(bass + 12)], s.secs(beats) + 0.2, H, rng, attack=0.4, release=0.7,
                    vib_cents=4.0, bow=0.05, bright=0.8)
        high = bowed([midi_hz(m) for m in upper], s.secs(beats) + 0.2, H, rng, attack=0.5, release=0.8,
                     vib_cents=7.0, bow=0.07, bright=1.1)
        mix_loop(strings, low, at)
        mix_loop(strings, high, at, COURT_UPPER_STRINGS)
    s.add_half(strings, -17.5, send=0.35, label="strings")
    # Every finished layer is dropped as soon as it's added, here and below. The song keeps its own
    # copy, and holding on to them all doubles what the run needs.
    del strings, low, high

    # The violins, from bar 11.
    violins = s.half_track()
    for bar, beat, m, held in COURT_VIOLINS:
        jitter = int(rng.gauss(0.0, 0.005) * H)
        note = bowed([midi_hz(m)], s.secs(held) * 0.95, H, rng, attack=0.08, release=0.35, vib_cents=14.0, bow=0.1,
                     bright=0.9)
        mix_loop(violins, note, s.half_at(bar, beat) + jitter)
    s.add_half(violins, -8.5, pan=0.2, send=0.5, label="violins")
    del violins, note

    # The horns: the call at the top, then soft held chords under the tune.
    horns = s.half_track()
    for bar, beat, notes, held in COURT_CALL:
        mix_loop(horns, brass([midi_hz(m) for m in notes], s.secs(held) * 0.9, H, rng, 0.75, attack=0.04, release=0.25),
                 s.half_at(bar, beat))
    for bar, beat, beats, name in court_spans():
        if bar >= COURT_THEME_FROM:
            _, chord, _ = COURT_VOICINGS[name]
            note = brass([midi_hz(m) for m in chord], s.secs(beats) * 0.95, H, rng, 0.45, attack=0.25, release=0.4)
            mix_loop(horns, note, s.half_at(bar, beat), COURT_HELD_HORNS)
    s.add_half(horns, -15.0, pan=-0.2, send=0.45, label="horns")
    del horns, note

    # The trumpet: the tune, then the long notes under the violins.
    blow = note_cache(lambda m, held, vel: brass([midi_hz(m)], held, H, rng, vel, attack=0.035, release=0.22,
                                                 vib_cents=6.0))
    trumpet = s.half_track()
    for bar, beat, m, held in COURT_THEME:
        jitter = int(rng.gauss(0.0, 0.005) * H)
        mix_loop(trumpet, blow(m, s.secs(held) * 0.88, 0.9 if beat == 0.0 else 0.8), s.half_at(bar, beat) + jitter)
    for bar, beat, m, held in COURT_COUNTER:
        mix_loop(trumpet, blow(m, s.secs(held) * 0.95, 0.6), s.half_at(bar, beat), COURT_COUNTER_LEVEL)
    s.add_half(trumpet, -8.0, pan=0.1, send=0.4, label="trumpet")
    del trumpet, blow

    # The harp running up every chord from the tune on, and the chimes on the violins' downbeats.
    pluck = note_cache(lambda m, vel: lute(midi_hz(m), R, rng, vel, 2.6))
    ring = note_cache(lambda m, vel: chime(midi_hz(m), R, rng, vel, 3.0))
    harp = s.track()
    for bar, beat, beats, name in court_spans():
        if bar >= COURT_THEME_FROM:
            _, chord, _ = COURT_VOICINGS[name]
            run = chord + tuple(note + 12 for note in chord)
            for k, m in enumerate(run):
                mix_loop(harp, pluck(m, 0.5 + 0.05 * k), s.at(bar, beat + k * COURT_HARP_STEP))
    for bar, beat, m, _ in COURT_VIOLINS:
        if beat == 0.0:
            mix_loop(harp, ring(m + 12, 0.6), s.at(bar), COURT_CHIME_LEVEL)
    s.add(harp, -19.0, pan=-0.3, send=0.55, label="harp")
    del harp, pluck, ring

    ch = s.half_track()
    for bar, beat, beats, name in court_spans():
        if bar >= COURT_STRINGS_FROM:
            _, chord, _ = COURT_VOICINGS[name]
            c = choir([midi_hz(m) for m in chord], s.secs(beats), H, rng, "oh", attack=0.8, release=0.8)
            mix_loop(ch, c, s.half_at(bar, beat) - int(0.3 * H))
    s.add_half(ch, -11.0, pan=0.15, send=0.6, label="choir")
    del ch, c

    # The timpani, tuned to each chord's bass: the roll into the tune, then one and three. The
    # field drum walks under the violins.
    kettle = note_cache(lambda m: timpani(midi_hz(m), R, rng))
    drums = s.track()
    for i in range(COURT_ROLL_STROKES):
        vel = COURT_ROLL_FROM + (COURT_ROLL_TO - COURT_ROLL_FROM) * i / (COURT_ROLL_STROKES - 1)
        jitter = int(rng.gauss(0.0, 0.004) * R)
        mix_loop(drums, kettle(45), s.at(2, 4.0 * i / COURT_ROLL_STROKES) + jitter, vel * rng.uniform(0.85, 1.0))
    for bar, beat, beats, name in court_spans():
        if bar >= COURT_THEME_FROM:
            bass = COURT_VOICINGS[name][0]
            for b, vel in COURT_TIMPANI + (COURT_TIMPANI_LATE if bar >= COURT_STRINGS_FROM else ()):
                if beat <= b < beat + beats:
                    mix_loop(drums, kettle(bass), s.at(bar, b), vel)
    snare = side_drum(R, rng)
    for bar in range(COURT_STRINGS_FROM, COURT_BARS + 1):
        for beat, vel in COURT_SIDE_DRUM:
            jitter = int(rng.gauss(0.0, 0.004) * R)
            mix_loop(drums, snare, s.at(bar, beat) + jitter, vel * COURT_SIDE_DRUM_LEVEL)
    s.add(drums, -13.0, send=0.35, label="drums")
    del drums, kettle

    toll = s.track()
    for bar, beat, m, vel in COURT_BELLS:
        mix_loop(toll, tower_bell(midi_hz(m), R, rng), s.at(bar, beat), vel)
    s.add(toll, -15.0, pan=-0.35, send=0.9, label="bells")
    del toll

    s.add_reverb(rt60=3.6, damp_hz=2800.0, size=1.3, gain_db=-8.0)
    s.finish("Content/Music/sunspire.wav", EXPLORE_RMS_DB)


# --------------------------------------------------------------------------- #
# Sound-effect helpers
# --------------------------------------------------------------------------- #

R = SFX_RATE

#: An iron latch, struck. (ratio, gain, tau)
IRON_LATCH = ((1.0, 1.0, 0.08), (2.3, 0.6, 0.05), (3.9, 0.35, 0.03), (6.1, 0.2, 0.015))


def sec(t):
    return int(round(t * R))


def place(dst, src, t, gain=1.0):
    mix(dst, src, sec(t), gain)


def burst(dur, tau, rng, kind=None, fc=1000.0, q=0.7, attack=0.0004):
    """A noise burst: optional biquad colour, attack, exponential decay."""
    n = sec(dur)
    x = noise(n, rng)
    if kind:
        x = biquad(x, kind, fc, q, R)
    return shaped(norm(x), envelope(n, R, attack=attack, tau=tau))


def strike(partials, f0, dur, rng, decay_scale=1.0, click=0.3, click_hp=2000.0):
    """A struck object: decaying partials plus a click of noise for the moment of contact."""
    out = norm(modes([(r, g, t * decay_scale) for r, g, t in partials], sec(dur), R, f0))
    if click:
        mix(out, burst(0.005, 0.0008, rng, "hp", click_hp), 0, click)
    return out


def thump(f0, f1, dur, tau, attack=0.001, sweep_tau=0.03):
    n = sec(dur)
    return shaped(glide(f0, f1, sweep_tau, n, R), envelope(n, R, attack=attack, tau=tau))


def room(x, rt60, damp_hz, wet, size=0.7):
    """Adds reverb to a dry effect. The wet signal is scaled to the dry peak, so `wet` is how loud
    the tail is compared to the hit."""
    x = x + silence(sec(rt60))
    wl, wr = reverb(x, R, rt60, damp_hz, size)
    w = norm(list(map(add, wl, wr)), max(map(abs, x)))
    return list(map(add, x, scale(w, wet)))


def finish_sfx(rel, x, peak_db):
    x = highpass(x, 20.0, R)                 # DC block
    floor = max(map(abs, x)) * 5e-4          # 66 dB under the peak counts as silence
    end = len(x)
    while end > 1 and abs(x[end - 1]) < floor:
        end -= 1
    x = x[:min(len(x), end + sec(0.005))]
    fade(x, R, 0.0005, min(0.03, 0.1 * len(x) / R))
    write_wav(rel, [norm(x, undb(peak_db))], R)


# --------------------------------------------------------------------------- #
# UI sounds
# --------------------------------------------------------------------------- #

def sfx_menu_move(rng):
    """A soft tick, like a fingernail on the body of a lute."""
    x = silence(sec(0.08))
    body = modes(((1.0, 1.0, 0.010), (2.4, 0.45, 0.006), (4.1, 0.2, 0.003)), sec(0.08), R, 820.0)
    place(x, norm(body), 0, 0.7)
    place(x, burst(0.005, 0.0012, rng, "bp", 2600.0, 0.9), 0, 0.5)
    return x


def sfx_menu_confirm(rng):
    """A low string plucked, its fifth just after it, over a soft knock on wood."""
    x = silence(sec(1.2))
    place(x, norm(lute(midi_hz(45), R, rng, vel=0.8, length=1.2)), 0, 0.9)
    place(x, norm(lute(midi_hz(52), R, rng, vel=0.6, length=1.1)), 0.035, 0.45)
    place(x, thump(140.0, 70.0, 0.25, 0.05), 0, 0.5)
    return x


def sfx_menu_back(rng):
    """A knock on wood played backwards, so it swells up and stops, then a very soft tap."""
    knock = list(map(add, thump(150.0, 70.0, 0.2, 0.06, 0.001, 0.03), scale(burst(0.2, 0.035, rng, "lp", 900.0), 0.4)))
    x = knock[::-1]
    fade(x, R, 0.002, 0.012)
    x += silence(sec(0.1))
    place(x, thump(120.0, 85.0, 0.08, 0.02), 0.2, 0.25)
    return x


def sfx_option_change(rng):
    """Two small wooden ticks, the second a fifth up."""
    x = silence(sec(0.12))
    for t, f in ((0.0, 980.0), (0.05, 1470.0)):
        place(x, norm(modes(((1.0, 1.0, 0.010), (2.6, 0.3, 0.005)), sec(0.06), R, f)), t, 0.8)
        place(x, burst(0.003, 0.0006, rng, "hp", 2200.0), t, 0.3)
    return x


def sfx_pause(rng):
    """An iron latch dropping into its catch, and the door it's on settling against the frame."""
    x = silence(sec(0.45))
    place(x, strike(IRON_LATCH, 640.0, 0.3, rng, click=0.6), 0, 0.8)
    place(x, strike(IRON_LATCH, 910.0, 0.2, rng, decay_scale=0.6, click=0.4), 0.05, 0.4)
    place(x, thump(120.0, 70.0, 0.3, 0.07), 0.0, 0.7)
    place(x, burst(0.06, 0.02, rng, "lp", 1100.0), 0.0, 0.3)
    return room(x, 0.5, 3000.0, 0.2)


# --------------------------------------------------------------------------- #
# Walking
# --------------------------------------------------------------------------- #

def sfx_footstep_stone(rng):
    """One step on stone: heel, a low thump, a little grit, then the toe. The Black Box's concrete step."""
    x = silence(sec(0.13))
    heel = lowpass(burst(0.05, 0.012, rng, "lp", 1400.0), 1400.0, R)
    place(x, norm(heel), 0, 0.8)
    place(x, thump(110.0, 60.0, 0.11, 0.03, 0.001, 0.02), 0, 0.7)
    place(x, burst(0.03, 0.008, rng, "bp", 3000.0, 0.8), 0.002, 0.15)
    place(x, norm(lowpass(burst(0.04, 0.01, rng, "lp", 1100.0), 1100.0, R)), 0.04, 0.35)
    place(x, burst(0.02, 0.005, rng, "bp", 3500.0, 0.8), 0.041, 0.08)
    return x


def sfx_footstep_grass(rng):
    """One step on grass: a soft thud, and the blades crushing under it."""
    x = silence(sec(0.16))
    place(x, thump(90.0, 55.0, 0.12, 0.035, 0.002, 0.02), 0, 0.6)
    place(x, burst(0.09, 0.03, rng, "bp", 3200.0, 0.6, attack=0.004), 0.0, 0.5)
    place(x, burst(0.06, 0.02, rng, "bp", 5200.0, 0.8, attack=0.003), 0.03, 0.25)
    return x


# --------------------------------------------------------------------------- #
# Using things
# --------------------------------------------------------------------------- #

#: A wooden plank, knocked. (ratio, gain, tau)
WOOD_KNOCK = ((1.0, 1.0, 0.012), (2.7, 0.5, 0.006), (5.1, 0.25, 0.003))

#: A small coin, flicked. (ratio, gain, tau)
COIN = ((1.0, 1.0, 0.09), (2.76, 0.6, 0.05), (5.4, 0.35, 0.03), (8.9, 0.2, 0.015))


def creak(dur, f_start, f_end, rng, body=420.0):
    """Wood rubbing on wood: a run of tiny slips, getting quicker or slower as it goes, each one
    knocking the plank. It's how a hinge creaks (stick and slip), so it's built that way."""
    x = silence(sec(dur + 0.05))
    t = 0.0
    while t < dur:
        f = f_start + (f_end - f_start) * (t / dur)
        g = (0.4 + 0.6 * math.sin(math.pi * t / dur)) * rng.uniform(0.6, 1.0)
        place(x, modes(WOOD_KNOCK, sec(0.03), R, body * rng.uniform(0.95, 1.05)), t, g)
        t += 1.0 / (f * rng.uniform(0.85, 1.15))
    return x


def sfx_door(rng):
    """A heavy door dragging open: the hinges creaking, stone grinding under it, and the thud as it stops."""
    x = silence(sec(1.6))
    place(x, creak(0.9, 55.0, 30.0, rng, 320.0), 0.0, 0.7)
    n = sec(1.0)
    rough = lines([(0.05 * k, rng.uniform(0.45, 1.0)) for k in range(21)], n, R)
    grind = shaped(norm(lowpass(lowpass(noise(n, rng), 500.0, R), 500.0, R)), envelope(n, R, attack=0.15, gate=0.8, release=0.08))
    place(x, shaped(grind, rough), 0.02, 0.5)
    place(x, thump(85.0, 50.0, 0.4, 0.09), 0.92, 0.9)
    place(x, burst(0.12, 0.04, rng, "lp", 700.0), 0.92, 0.3)
    return room(x, 1.2, 2200.0, 0.3)


def sfx_door_locked(rng):
    """Trying a locked door: the latch rattling against the lock, and the door not moving."""
    x = silence(sec(0.6))
    for t, f, g in ((0.0, 700.0, 0.7), (0.09, 760.0, 0.9), (0.2, 720.0, 0.6)):
        place(x, strike(IRON_LATCH, f, 0.2, rng, decay_scale=0.7, click=0.5), t, g)
        place(x, thump(110.0, 75.0, 0.12, 0.03), t, 0.4)
    return room(x, 0.6, 2500.0, 0.2)


def sfx_chest(rng):
    """A chest lid: a short creak up on its hinges, and the knock as it falls back against them."""
    x = silence(sec(0.8))
    place(x, creak(0.28, 70.0, 110.0, rng, 650.0), 0.0, 0.6)
    place(x, norm(modes(WOOD_KNOCK, sec(0.15), R, 240.0)), 0.3, 0.8)
    place(x, thump(130.0, 80.0, 0.2, 0.05), 0.3, 0.6)
    place(x, strike(IRON_LATCH, 980.0, 0.15, rng, decay_scale=0.5, click=0.2), 0.31, 0.25)
    return room(x, 0.5, 3000.0, 0.15)


def sfx_lever(rng):
    """A heavy iron lever: the catch clicking over a few teeth, the clunk at the bottom, and a chain
    somewhere taking up the slack."""
    x = silence(sec(1.0))
    for i in range(4):
        place(x, strike(IRON_LATCH, 1300.0 + 60.0 * i, 0.06, rng, decay_scale=0.3, click=0.6), 0.04 * i, 0.4)
    place(x, strike(IRON_LATCH, 380.0, 0.4, rng, click=0.7), 0.18, 1.0)
    place(x, thump(95.0, 55.0, 0.3, 0.07), 0.18, 0.8)
    for i in range(6):
        place(x, strike(IRON_LATCH, rng.uniform(1800.0, 2600.0), 0.05, rng, decay_scale=0.25, click=0.4),
              0.3 + 0.045 * i + rng.uniform(0.0, 0.02), 0.15)
    return room(x, 0.9, 2400.0, 0.3)


def sfx_pickup(rng):
    """A handful of coins: a few small rings close together, landing on each other."""
    x = silence(sec(0.55))
    t = 0.0
    for _ in range(5):
        place(x, strike(COIN, rng.uniform(2300.0, 3400.0), 0.3, rng, decay_scale=rng.uniform(0.6, 1.0), click=0.3, click_hp=4000.0),
              t, rng.uniform(0.4, 0.8))
        t += rng.uniform(0.03, 0.08)
    return x


def sfx_waypoint(rng):
    """A waypoint stone waking: a low hum coming up out of the stone, and something like glass ringing over it."""
    x = silence(sec(2.6))
    hum = choir([midi_hz(45), midi_hz(52), midi_hz(57)], 0.6, R, rng, "oo", attack=0.35, release=0.6)
    place(x, norm(hum), 0.0, 0.7)
    for i, m in enumerate((81, 88, 93)):
        place(x, norm(modes(((1.0, 1.0, 0.8), (2.01, 0.3, 0.4)), sec(2.0), R, midi_hz(m))), 0.1 + 0.12 * i, 0.25)
    return room(x, 2.0, 3500.0, 0.4)


# --------------------------------------------------------------------------- #
# Conversations: the dice, and the companions' approval
# --------------------------------------------------------------------------- #

#: A bone die: hard, small, a bright click and a short ring.
BONE = ((1.0, 1.0, 0.03), (2.4, 0.6, 0.015), (4.7, 0.3, 0.008))

#: A ship's bell: a long ring with the bell's own out-of-tune partials.
BELL = ((0.5, 0.5, 2.6), (1.0, 1.0, 2.2), (1.19, 0.5, 1.6), (1.5, 0.45, 1.3), (2.0, 0.35, 1.0), (2.66, 0.25, 0.7), (3.0, 0.2, 0.5))


def sfx_dice_roll(rng):
    """The d20 thrown down and tumbling: a few hard clicks bunched up and spreading out, quieter as it slows."""
    x = silence(sec(0.9))
    t, gap, loud = 0.0, 0.035, 1.0
    for _ in range(9):
        place(x, strike(BONE, rng.uniform(1700.0, 2600.0), 0.12, rng, click=0.6, click_hp=3000.0), t, loud)
        t += gap
        gap *= rng.uniform(1.18, 1.32)
        loud *= 0.84
    return room(x, 0.3, 4000.0, 0.15)


def sfx_check_pass(rng):
    """A check passing: two bright notes going up, a fifth apart, like a small bell."""
    x = silence(sec(1.4))
    for i, m in enumerate((76, 83)):
        place(x, norm(modes(((1.0, 1.0, 0.9), (2.0, 0.35, 0.5), (3.01, 0.15, 0.3)), sec(1.2), R, midi_hz(m))), 0.09 * i, 0.6)
    return room(x, 1.2, 4500.0, 0.3)


def sfx_check_fail(rng):
    """A check failing: a low, dull knock and a note sliding down."""
    x = silence(sec(1.0))
    place(x, thump(140.0, 70.0, 0.4, 0.12), 0.0, 0.8)
    place(x, norm(shaped(glide(midi_hz(64), midi_hz(58), 0.15, sec(0.7), R), envelope(sec(0.7), R, attack=0.01, tau=0.25))), 0.04, 0.4)
    return room(x, 0.8, 2500.0, 0.25)


def sfx_approve(rng):
    """A companion approving: a soft, warm note that rises a little."""
    x = silence(sec(1.0))
    hum = choir([midi_hz(64), midi_hz(71)], 0.3, R, rng, "oo", attack=0.05, release=0.25)
    place(x, norm(hum), 0.0, 0.5)
    place(x, norm(modes(((1.0, 1.0, 0.5), (2.0, 0.3, 0.3)), sec(0.8), R, midi_hz(83))), 0.05, 0.3)
    return room(x, 0.9, 4000.0, 0.3)


def sfx_disapprove(rng):
    """A companion disapproving: the same note, lower and turning down."""
    x = silence(sec(1.0))
    hum = choir([midi_hz(57), midi_hz(60)], 0.3, R, rng, "oh", attack=0.05, release=0.25)
    place(x, norm(hum), 0.0, 0.5)
    place(x, norm(shaped(glide(midi_hz(69), midi_hz(65), 0.2, sec(0.6), R), envelope(sec(0.6), R, attack=0.01, tau=0.2))), 0.05, 0.25)
    return room(x, 0.8, 3000.0, 0.25)


# --------------------------------------------------------------------------- #
# The story
# --------------------------------------------------------------------------- #

def sfx_heartbeat(rng):
    """The Prologue's card: one heartbeat on its own, then two together, in step."""
    x = silence(sec(3.2))

    def beat():
        b = silence(sec(0.5))
        place(b, thump(70.0, 45.0, 0.3, 0.07), 0.0, 1.0)
        place(b, thump(62.0, 40.0, 0.3, 0.06), 0.16, 0.7)
        return lowpass(b, 400.0, R)

    place(x, beat(), 0.0, 0.9)
    place(x, beat(), 1.3, 0.9)
    place(x, beat(), 1.36, 0.75)
    place(x, beat(), 2.2, 0.9)
    place(x, beat(), 2.26, 0.75)
    return room(x, 1.2, 1500.0, 0.25)


def sfx_ship_bell(rng):
    """A ship's bell rung for all hands: three quick strokes, ringing on."""
    x = silence(sec(3.2))
    for i in range(3):
        place(x, strike(BELL, 520.0, 2.8, rng, click=0.4, click_hp=2500.0), 0.32 * i, 0.7)
    return room(x, 2.0, 3500.0, 0.35)


# --------------------------------------------------------------------------- #
# The road east: the Mirewood, Thornbridge and the keep
# --------------------------------------------------------------------------- #

#: The black crystal's hum: two notes a semitone apart an octave down, and the same an octave up
#: with a tritone between, so it beats and never settles. (Hz, gain)
CRYSTAL_HUM = ((55.0, 1.0), (58.27, 0.8), (77.78, 0.5), (110.0, 0.45), (116.54, 0.35))

#: The glass in the crystal: partials that belong to no note. (ratio, gain, tau)
CRYSTAL_GLASS = ((1.0, 1.0, 1.2), (1.41, 0.6, 0.9), (2.13, 0.4, 0.6), (2.97, 0.25, 0.4))

#: When the crystal throbs, in seconds from the start.
CRYSTAL_THROBS = (0.15, 1.0, 1.85)

#: How far the hum sags at the top of a throb, as a fraction of its pitch (about half a semitone).
CRYSTAL_SAG = 0.03


def howl(contour, rng):
    """One wolf's howl: a voice sliding through the contour's (seconds, Hz) points with a slow
    waver that grows as it's held, mostly its own pitch with a little of the two above, breath under
    it, swelling in and dying away at the end."""
    dur = contour[-1][0]
    n = sec(dur)
    f = lines(contour, n, R)
    wob = sine(rng.uniform(4.5, 6.0), n, R)
    depth = lines(((0.0, 0.0), (0.5, 0.0), (1.0, 0.012), (dur, 0.02)), n, R)
    w = TAU / R
    phase = list(accumulate(w * fi * (1.0 + d * v) for fi, d, v in zip(f, depth, wob)))
    s = math.sin
    tone = [s(p) + 0.3 * s(2.0 * p) + 0.1 * s(3.0 * p) for p in phase]
    mix(tone, norm(biquad(noise(n, rng), "bp", 1200.0, 0.8, R)), 0, 0.08)
    env = lines(((0.0, 0.0), (0.25, 1.0), (0.7 * dur, 0.85), (dur, 0.0)), n, R)
    return shaped(lowpass(tone, 3000.0, R), env)


def sfx_wolf_howl(rng):
    """A gloomfang howling in the trees: a long voice lifting, holding and falling away, and a
    second wolf answering higher before the first has finished."""
    x = silence(sec(4.2))
    place(x, howl(((0.0, 360.0), (0.4, 600.0), (1.7, 640.0), (2.4, 470.0), (2.7, 380.0)), rng), 0.0, 1.0)
    place(x, howl(((0.0, 420.0), (0.35, 720.0), (1.4, 760.0), (1.9, 600.0), (2.2, 500.0)), rng), 1.1, 0.6)
    return room(x, 2.4, 2200.0, 0.45, size=1.2)


def sfx_crystal_pulse(rng):
    """The black crystal: a deep hum that's wrong, its notes rubbing against each other, throbbing
    three times like something with a pulse and sagging flat at every throb, a thump under each
    one, and cold glassy partials ringing out of tune over the top."""
    n = sec(3.0)
    throbs = silence(n)
    for t in CRYSTAL_THROBS:
        place(throbs, envelope(sec(1.2), R, attack=0.06, tau=0.3), t)
    # Warped time again: it runs slow while a throb is up, so the whole hum sags flat with it.
    clock = list(accumulate(1.0 - CRYSTAL_SAG * p for p in throbs))
    s = math.sin
    hum = silence(n)
    for f, g in CRYSTAL_HUM:
        w = TAU * f / R
        mix(hum, [s(w * t) for t in clock], 0, g)
    hum = norm(hum)
    hum = [math.tanh(1.8 * v) for v in hum]                  # driven a little, for the grit
    swell = lines(((0.0, 0.0), (0.3, 0.4), (2.3, 0.4), (3.0, 0.0)), n, R)
    x = shaped(lowpass(hum, 900.0, R), list(map(add, swell, scale(throbs, 0.6))))
    for t in CRYSTAL_THROBS:
        place(x, thump(70.0, 38.0, 0.6, 0.15), t, 0.5)
    glass = norm(modes(CRYSTAL_GLASS, n, R, 1244.5))
    mix(glass, sine(1251.0, n, R, 0.5, 1.0))
    place(x, shaped(glass, list(map(add, repeat(0.3), throbs))), 0.0, 0.08)
    return room(x, 1.6, 1800.0, 0.3)


def horn_blast(freq, dur, rng, fall_cents=0.0):
    """One blast on a war horn: blown hard and rough, the roughness a slow flutter in the lips."""
    x = brass([freq], dur, R, rng, vel=1.0, attack=0.08, release=0.25, vib_cents=0.0, fall_cents=fall_cents)
    flutter = norm(lowpass(lowpass(noise(len(x), rng), 30.0, R), 30.0, R))
    return shaped(x, [1.0 + 0.25 * v for v in flutter])


def sfx_war_horn(rng):
    """War horns a long way off: a short blast, then a long one with a second horn coming in a
    semitone above it, so they grind, both sagging as the breath runs out. Muffled by the distance
    and spread out by the hills."""
    x = silence(sec(3.5))
    place(x, horn_blast(110.0, 0.55, rng), 0.0, 0.8)
    place(x, horn_blast(110.0, 2.0, rng, 70.0), 0.8, 1.0)
    place(x, horn_blast(116.54, 1.6, rng, 90.0), 1.15, 0.7)
    x = lowpass(lowpass(x, 1400.0, R), 1400.0, R)
    return room(x, 2.8, 1600.0, 0.55, size=1.4)


def sfx_bowshot(rng):
    """A bow loosed: the string snapping forward and thrumming, the limbs knocking, the string
    slapping the bracer, and the arrow hissing away from you with its fletching fluttering."""
    x = silence(sec(0.9))
    n = sec(0.35)
    twang = shaped(glide(190.0, 150.0, 0.03, n, R), envelope(n, R, attack=0.0005, tau=0.07))
    mix(twang, shaped(glide(380.0, 300.0, 0.03, n, R), envelope(n, R, attack=0.0005, tau=0.04)), 0, 0.4)
    place(x, twang, 0.0, 0.8)
    place(x, norm(modes(WOOD_KNOCK, sec(0.08), R, 520.0)), 0.0, 0.5)
    place(x, burst(0.01, 0.002, rng, "hp", 2500.0), 0.0, 0.5)
    n = sec(0.6)
    going = lines(((0.0, 5000.0), (0.6, 1200.0)), n, R)
    air = swept_lowpass(swept_lowpass(highpass(noise(n, rng), 600.0, R), going, R), going, R)
    flutter = [1.0 + 0.35 * v for v in sine(rng.uniform(55.0, 70.0), n, R)]
    place(x, shaped(shaped(norm(air), envelope(n, R, attack=0.02, tau=0.18)), flutter), 0.01, 0.45)
    place(x, shaped(sweep(2200.0, 1500.0, n, R), envelope(n, R, attack=0.03, tau=0.12)), 0.01, 0.06)
    return room(x, 0.7, 3500.0, 0.15)


def sfx_coin(rng):
    """A coin flipped: the flick of a thumb, the coin ringing and wobbling as it spins, a soft slap
    as a palm catches it, then it set down on another coin. Pay counted into a hand sounds the same."""
    x = silence(sec(1.0))
    catch = 0.42
    place(x, burst(0.004, 0.0008, rng, "hp", 3000.0), 0.0, 0.5)
    ring = strike(COIN, 3100.0, catch + 0.03, rng, decay_scale=3.0, click=0.4, click_hp=4000.0)
    n = len(ring)
    spin = [0.6 + 0.4 * v for v in sweep(22.0, 14.0, n, R)]
    held = lines(((0.0, 1.0), (catch, 1.0), (catch + 0.02, 0.0)), n, R)
    place(x, shaped(shaped(ring, spin), held), 0.0, 0.6)
    place(x, thump(160.0, 110.0, 0.12, 0.025), catch, 0.5)
    place(x, norm(lowpass(burst(0.05, 0.012, rng), 1500.0, R)), catch, 0.35)
    place(x, strike(COIN, 3600.0, 0.3, rng, decay_scale=0.7, click=0.3, click_hp=4000.0), catch + 0.2, 0.5)
    place(x, strike(COIN, 2900.0, 0.3, rng, decay_scale=0.6, click=0.3, click_hp=4000.0), catch + 0.245, 0.3)
    return x



# --------------------------------------------------------------------------- #
# Build
# --------------------------------------------------------------------------- #

SONGS = {
    "embers": build_embers,
    "hearthsmoke": build_hearthsmoke,
    "cold-stone": build_cold_stone,
    "salt-and-lanterns": build_salt_and_lanterns,
    "meadowlark": build_meadowlark,
    "sunspire": build_sunspire,
}

SFX = {   # name: (folder, peak dBFS, builder)
    "menu-move": ("Content/Sfx", PEAK_CLICK, sfx_menu_move),
    "menu-confirm": ("Content/Sfx", PEAK_MENU, sfx_menu_confirm),
    "menu-back": ("Content/Sfx", PEAK_MENU, sfx_menu_back),
    "option-change": ("Content/Sfx", PEAK_CLICK, sfx_option_change),
    "pause": ("Content/Sfx", PEAK_CLICK, sfx_pause),
    "footstep-stone": ("Content/Sfx", PEAK_REPEAT, sfx_footstep_stone),
    "footstep-grass": ("Content/Sfx", PEAK_REPEAT, sfx_footstep_grass),
    "door": ("Content/Sfx", PEAK_MENU, sfx_door),
    "door-locked": ("Content/Sfx", PEAK_CLICK, sfx_door_locked),
    "chest": ("Content/Sfx", PEAK_MENU, sfx_chest),
    "lever": ("Content/Sfx", PEAK_MENU, sfx_lever),
    "pickup": ("Content/Sfx", PEAK_CLICK, sfx_pickup),
    "waypoint": ("Content/Sfx", PEAK_MENU, sfx_waypoint),
    "dice-roll": ("Content/Sfx", PEAK_CLICK, sfx_dice_roll),
    "check-pass": ("Content/Sfx", PEAK_MENU, sfx_check_pass),
    "check-fail": ("Content/Sfx", PEAK_MENU, sfx_check_fail),
    "approve": ("Content/Sfx", PEAK_CLICK, sfx_approve),
    "disapprove": ("Content/Sfx", PEAK_CLICK, sfx_disapprove),
    "heartbeat": ("Content/Sfx", PEAK_MENU, sfx_heartbeat),
    "ship-bell": ("Content/Sfx", PEAK_MENU, sfx_ship_bell),
    "wolf-howl": ("Content/Sfx", PEAK_MENU, sfx_wolf_howl),
    "crystal-pulse": ("Content/Sfx", PEAK_MENU, sfx_crystal_pulse),
    "war-horn": ("Content/Sfx", PEAK_MENU, sfx_war_horn),
    "bowshot": ("Content/Sfx", PEAK_MENU, sfx_bowshot),
    "coin": ("Content/Sfx", PEAK_CLICK, sfx_coin),
}


def build_sfx(name):
    folder, peak_db, builder = SFX[name]
    # crc32 instead of hash(), because str hashes change every run and these seeds can't.
    rng = random.Random(zlib.crc32(name.encode()))
    finish_sfx(folder + "/" + name + ".wav", builder(rng), peak_db)


if __name__ == "__main__":
    args = [a for a in sys.argv[1:] if a != "-v"]
    VERBOSE = len(args) != len(sys.argv) - 1
    wanted = []
    for a in args or ["music", "sfx"]:
        a = os.path.splitext(os.path.basename(a))[0]
        if a == "music":
            wanted += list(SONGS)
        elif a == "sfx":
            wanted += list(SFX)
        elif a in SONGS or a in SFX:
            wanted.append(a)
        else:
            sys.exit("No song or sound called '%s'." % a)
    started = time.perf_counter()
    print("Generating audio for Legend Quest...")
    for name in dict.fromkeys(wanted):
        if name in SONGS:
            SONGS[name]()
        else:
            build_sfx(name)
    print("Done in %.1f s." % (time.perf_counter() - started))
