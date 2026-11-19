"""
The synthesised voices the sample libraries don't have, ported to numpy from tools/generate_audio.py
and widened: a choir (neither Versilian library records singers), and the textures of a place (the
sea against a hull, wind, water dripping in a cave) that are scenery rather than instruments.

Each voice plays a note the way sampler.Sampled does (render(key, vel, dur, rr, rand, ...) returns
stereo float64 at RATE), so a score can't tell a synthesised part from a sampled one. The textures
are whole loops: they take the loop's length and come back exactly that long, rising and falling a
whole number of times so they join up at the seam.

Seeded throughout: the same note or texture with the same seed is the same, sample for sample.
"""

import math
import zlib

import numpy as np
from scipy.signal import lfilter

from mix import biquad, eq
from sampler import RATE, envelope

#: The vowels the choir sings: (formant Hz, bandwidth Hz, gain) for the first four formants, roughly
#: a mixed choir's, after the published averages for sung vowels.
VOWELS = {
    "ah": ((700.0, 110.0, 1.0), (1150.0, 120.0, 0.55), (2600.0, 160.0, 0.22), (3300.0, 250.0, 0.10)),
    "oh": ((450.0, 80.0, 1.0), (800.0, 90.0, 0.50), (2830.0, 140.0, 0.12), (3500.0, 200.0, 0.06)),
    "oo": ((330.0, 70.0, 1.0), (850.0, 90.0, 0.35), (2400.0, 120.0, 0.08), (3300.0, 200.0, 0.04)),
    "eh": ((540.0, 80.0, 1.0), (1700.0, 100.0, 0.45), (2500.0, 140.0, 0.25), (3400.0, 220.0, 0.10)),
}

_TABLES = {}


def _saw_table(max_harm):
    """One cycle of a band-limited sawtooth with max_harm harmonics (4096 points)."""
    t = _TABLES.get(max_harm)
    if t is None:
        ph = np.arange(4096) / 4096.0 * 2.0 * np.pi
        k = np.arange(1, max_harm + 1)[:, None]
        t = np.sum(np.sin(k * ph) / k, axis=0) * (2.0 / np.pi)
        t = np.append(t, t[0])
        _TABLES[max_harm] = t
    return t


def _osc(freq_curve, phase0):
    """A band-limited saw following a frequency curve (Hz per sample)."""
    fmax = float(np.max(freq_curve))
    harm = max(1, min(64, int(0.45 * RATE / fmax)))
    table = _saw_table(harm)
    ph = (phase0 + np.cumsum(freq_curve) / RATE) % 1.0
    idx = ph * 4096.0
    i = idx.astype(np.int64)
    f = idx - i
    return table[i] * (1.0 - f) + table[i + 1] * f


def midi_hz(m, cents=0.0):
    return 440.0 * 2.0 ** ((m - 69 + cents / 100.0) / 12.0)


class Choir:
    """A wordless choir section: `voices` singers per note, each a band-limited saw a few cents
    from the others with a vibrato of its own that comes in after the note settles, plus breath,
    all through the vowel's four formants and a gentle low-pass. Low notes are darker."""

    def __init__(self, vowel="ah", voices=6, detune=9.0, breath=0.25, gain=0.0, attack=0.6, release=1.2,
                 seed=1, name="choir"):
        self.vowel, self.voices, self.detune, self.breath = vowel, voices, detune, breath
        self.gain, self.attack, self.release, self.seed, self.name = gain, attack, release, seed, name
        self._cache = {}
        self._filters = [(biquad("bandpass", fc, fc / bw), g) for fc, bw, g in VOWELS[vowel]]

    def render(self, key, vel, dur, rr=0, rand=0.0, attack=None, release=None, cents=0.0):
        attack = self.attack if attack is None else attack
        release = self.release if release is None else release
        ck = (int(key), round(dur, 3), round(attack, 3), round(release, 3), rr % 3, round(cents, 1))
        base = self._cache.get(ck)
        if base is None:
            base = self._sing(key, dur, attack, release, rr, cents)
            self._cache[ck] = base
        amp = (0.35 + 0.65 * (vel / 127.0) ** 1.5) * 10.0 ** (self.gain / 20.0)
        return base * amp

    def _sing(self, key, dur, attack, release, rr, cents):
        rng = np.random.default_rng(zlib.crc32(("%s|%d|%d|%.1f" % (self.name, key, rr % 3, cents)).encode()) ^ self.seed)
        n = int((dur + release) * RATE) + 1
        t = np.arange(n) / RATE
        f0 = midi_hz(key, cents)
        out = np.zeros((n, 2))
        for v in range(self.voices):
            det = rng.uniform(-self.detune, self.detune)
            rate = rng.uniform(4.6, 5.6)
            depth = rng.uniform(14.0, 24.0)                  # vibrato depth, cents
            onset = np.clip((t - rng.uniform(0.25, 0.6)) / 0.5, 0.0, 1.0)
            drift = 4.0 * np.sin(2 * np.pi * rng.uniform(0.15, 0.35) * t + rng.uniform(0, 6.28))
            c = det + drift + depth * onset * np.sin(2 * np.pi * rate * t + rng.uniform(0, 6.28))
            f = f0 * 2.0 ** (c / 1200.0)
            s = _osc(f, rng.random())
            pan = (v / max(1, self.voices - 1)) * 1.2 - 0.6
            out[:, 0] += s * math.cos((pan + 1) * math.pi / 4)
            out[:, 1] += s * math.sin((pan + 1) * math.pi / 4)
        out /= math.sqrt(self.voices)
        out += self.breath * rng.standard_normal((n, 2)) * 0.6
        y = np.zeros_like(out)
        for (b, a), g in self._filters:
            y += g * lfilter(b, a, out, axis=0)
        # darker low: the lowpass follows the pitch
        b, a = biquad("lowpass", min(0.42 * RATE, 1800.0 + 9.0 * f0), 0.6)
        y = lfilter(b, a, y, axis=0)
        env = envelope(n, int(dur * RATE), attack, 0.0, 0.0, 1.0, release)
        y *= env[:, None]
        return y * 0.9


# --------------------------------------------------------------------------- #
# Textures, whole loops
# --------------------------------------------------------------------------- #

def _noise(n, seed):
    return np.random.default_rng(seed).standard_normal((n, 2))


def sea(n, waves, seed=7):
    """The sea against the hull: low, wide noise rolling in and out `waves` times a loop, the
    hiss of the crest a little later than the swell, as a wave breaks."""
    x = eq(_noise(n, seed), [("lowpass", 380.0, 0.6, 0.0), ("lowpass", 520.0, 0.7, 0.0), ("highpass", 40.0, 0.7, 0.0)])
    hiss = eq(_noise(n, seed + 1), [("bandpass", 2600.0, 0.5, 0.0), ("lowpass", 6000.0, 0.7, 0.0)])
    ph = np.arange(n) / n * waves * 2.0 * np.pi
    swell = 0.18 + 0.82 * np.sin(ph / 2.0) ** 4
    crest = np.clip(np.sin(ph / 2.0 - 0.6), 0.0, 1.0) ** 6
    y = x / np.std(x) * swell[:, None] + 0.35 * hiss / np.std(hiss) * crest[:, None]
    return y / np.max(np.abs(y))


def wind(n, gusts, seed=11, centre=520.0):
    """Wind past stone: band-passed noise swelling in `gusts` slow gusts a loop."""
    x = eq(_noise(n, seed), [("bandpass", centre, 0.7, 0.0), ("lowpass", 2500.0, 0.7, 0.0)])
    ph = np.arange(n) / n * gusts * 2.0 * np.pi
    g = 0.45 + 0.55 * np.sin(ph + 1.3) ** 2
    y = x / np.std(x) * g[:, None]
    return y / np.max(np.abs(y))


def drips(n, count, seed=13):
    """Water dripping somewhere in a cave: short falling pitched blips, scattered round the loop."""
    rng = np.random.default_rng(seed)
    y = np.zeros((n, 2))
    m = int(0.09 * RATE)
    t = np.arange(m) / RATE
    for _ in range(count):
        f0 = rng.uniform(900.0, 2200.0)
        f = f0 * (1.0 + 1.6 * np.exp(-t / 0.006))
        ph = np.cumsum(f) / RATE * 2 * np.pi
        blip = np.sin(ph) * np.exp(-t / 0.018) * (1 - np.exp(-t / 0.0007))
        at = int(rng.integers(0, n))
        pan = rng.uniform(-0.8, 0.8)
        g = rng.uniform(0.3, 1.0)
        idx = (at + np.arange(m)) % n
        y[idx, 0] += g * blip * math.cos((pan + 1) * math.pi / 4)
        y[idx, 1] += g * blip * math.sin((pan + 1) * math.pi / 4)
    return y / max(1e-9, np.max(np.abs(y)))


def crackle(n, seed=17, rate_hz=9.0):
    """A fire crackling close by: sparse clicks of filtered noise, some louder pops."""
    rng = np.random.default_rng(seed)
    y = np.zeros((n, 2))
    count = int(n / RATE * rate_hz)
    for _ in range(count):
        m = int(rng.uniform(0.002, 0.012) * RATE)
        burst = rng.standard_normal(m) * np.exp(-np.arange(m) / (m / 4))
        at = int(rng.integers(0, n))
        g = rng.uniform(0.1, 1.0) ** 2
        pan = rng.uniform(-0.5, 0.5)
        idx = (at + np.arange(m)) % n
        y[idx, 0] += g * burst * (0.5 - pan / 2)
        y[idx, 1] += g * burst * (0.5 + pan / 2)
    y = eq(y, [("highpass", 900.0, 0.7, 0.0), ("lowpass", 7000.0, 0.7, 0.0)])
    return y / max(1e-9, np.max(np.abs(y)))
