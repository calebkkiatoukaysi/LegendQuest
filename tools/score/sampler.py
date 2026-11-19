"""
The sampler: plays a note on one of the sample libraries' instruments and hands back the sound.

Each recording is read once, turned to float and resampled to the score's rate (32 kHz, the rate
every song in Content/Music is at) with a proper anti-alias filter (scipy's resample_poly), and the
result is cached in memory and on disk beside the libraries (<cache>/.score-32k), so later renders
skip the slow step. A note picks its regions from the .sfz (sfz.py): the one or two recordings
nearest the key at that velocity, the next one round, and the keyswitch's articulation. It's then
re-pitched by reading the recording faster or slower with four-point cubic interpolation, and shaped
by the SFZ envelope: attack, hold, decay to the sustain level, held until the note ends, then
released along an exponential that is 60 dB down at the release time. A note held longer than its
recording is lengthened by looping the recording's last steady stretch with equal-power crossfades.

Notes are stereo float64 arrays at RATE. Everything is deterministic: the same note with the same
round-robin count and random draw comes out the same, sample for sample.
"""

import hashlib
import math
import os
import sys

import numpy as np
from scipy.io import wavfile
from scipy.signal import resample_poly

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import fetch_samples  # noqa: E402  (after the path fix, so the module loads from anywhere)
import sfz  # noqa: E402

RATE = 32000


class MissingSamples(SystemExit):
    pass


def require_cache():
    """Stops with a plain message if the sample libraries haven't been fetched."""
    gone = fetch_samples.missing()
    if gone:
        raise MissingSamples(
            "The sample libraries aren't in %s (missing: %s).\n"
            "Fetch them first (about 4 GB on disk, once, outside the repo):\n\n"
            "    python tools/score/fetch_samples.py\n\n"
            "or point LQ_SAMPLES at a folder that already holds them."
            % (fetch_samples.cache_root(), ", ".join(gone)))
    return fetch_samples.cache_root()


# --------------------------------------------------------------------------- #
# Recordings
# --------------------------------------------------------------------------- #

_LOADED = {}


def _disk_cache(path):
    st = os.stat(path)
    key = hashlib.sha1(("%s|%d|%d" % (os.path.relpath(path, fetch_samples.cache_root()),
                                      st.st_size, RATE)).encode()).hexdigest()
    return os.path.join(fetch_samples.cache_root(), ".score-32k", key[:2], key + ".npy")


def load(path):
    """A recording as stereo float32 at RATE (cached)."""
    got = _LOADED.get(path)
    if got is not None:
        return got
    cached = _disk_cache(path)
    if os.path.isfile(cached):
        x = np.load(cached)
    else:
        if not os.path.isfile(path):
            raise FileNotFoundError("sample missing from the cache: " + path)
        import warnings
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            rate, data = wavfile.read(path)
        if data.dtype == np.int16:
            x = data.astype(np.float64) / 32768.0
        elif data.dtype == np.int32:
            x = data.astype(np.float64) / 2147483648.0
        elif data.dtype == np.uint8:
            x = (data.astype(np.float64) - 128.0) / 128.0
        else:
            x = data.astype(np.float64)
        if x.ndim == 1:
            x = np.stack([x, x], axis=1)
        x = x[:, :2]
        if rate != RATE:
            g = math.gcd(RATE, rate)
            x = resample_poly(x, RATE // g, rate // g, axis=0)
        x = x.astype(np.float32)
        os.makedirs(os.path.dirname(cached), exist_ok=True)
        np.save(cached, x)
    _LOADED[path] = x
    return x


_LEVELS = {}


def level(path):
    """A recording's loudness: the RMS of its loudest 50 ms (cached)."""
    got = _LEVELS.get(path)
    if got is None:
        x = load(path).astype(np.float64)
        p = np.mean(x ** 2, axis=1)
        win = int(0.05 * RATE)
        c = np.concatenate([[0.0], np.cumsum(p)])
        r = (c[win:] - c[:-win]) / win if len(p) > win else np.array([c[-1] / max(1, len(p))])
        got = max(1e-6, math.sqrt(float(np.max(r))))
        _LEVELS[path] = got
    return got


def _extended(x, need, start_min):
    """x lengthened to at least `need` frames by looping its last steady stretch (crossfaded)."""
    n = len(x)
    if n >= need:
        return x
    xf = int(0.25 * RATE)
    tail_cut = int(0.20 * RATE)                    # leave the recording's own fade alone
    le = max(start_min + 2 * xf + 1, n - tail_cut)
    ls = max(start_min, le - int(1.6 * RATE))
    if le - ls < 2 * xf + 8 or le > n:
        out = np.zeros((need, 2), np.float32)
        out[:n] = x
        return out
    seg = x[ls:le].astype(np.float64)
    fade = np.linspace(0.0, 1.0, xf)[:, None]
    up, down = np.sqrt(fade), np.sqrt(1.0 - fade)
    out = [x[:le - xf].astype(np.float64)]
    tail = x[le - xf:le].astype(np.float64)        # the bit fading out into the next pass
    total = le - xf
    while total < need:
        body = seg.copy()
        body[:xf] = body[:xf] * up + tail * down
        out.append(body[:-xf])
        tail = body[-xf:]
        total += len(body) - xf
    out.append(tail)
    return np.concatenate(out)[:need].astype(np.float32)


def _cubic(x, pos):
    """x (frames, 2) read at fractional positions, four-point Catmull-Rom; zero past either end."""
    n = len(x)
    i = np.floor(pos).astype(np.int64)
    t = (pos - i)[:, None]
    def at(k):
        k = i + k
        ok = (k >= 0) & (k < n)
        v = x[np.clip(k, 0, n - 1)].astype(np.float64)
        v[~ok] = 0.0
        return v
    p0, p1, p2, p3 = at(-1), at(0), at(1), at(2)
    return p1 + 0.5 * t * (p2 - p0 + t * (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3 + t * (3.0 * (p1 - p2) + p3 - p0)))


def envelope(n, gate, attack, hold, decay, sustain, release):
    """The ADSR: rises over attack, holds, decays to sustain (0..1), held to `gate` frames, then
    falls exponentially, 60 dB down after `release` seconds, and reaches zero by the end of n."""
    t = np.arange(n) / RATE
    env = np.ones(n)
    a = max(attack, 0.002)
    env = np.where(t < a, t / a, env)
    d0 = a + hold
    if decay > 0 and sustain < 1.0:
        k = np.clip((t - d0) / decay, 0.0, None)
        env = np.where(t >= d0, sustain + (1.0 - sustain) * np.exp(-5.0 * k), env)
    g = gate / RATE
    level = float(np.interp(g, t, env)) if n else 0.0
    rel = max(release, 0.005)
    after = t >= g
    env[after] = level * np.exp(-6.9078 * (t[after] - g) / rel)
    # land on zero: a short fade over the last few milliseconds
    k = min(n, int(0.004 * RATE))
    if k > 0:
        env[-k:] *= np.linspace(1.0, 0.0, k)
    return env


# --------------------------------------------------------------------------- #
# Instruments
# --------------------------------------------------------------------------- #

class Sampled:
    """An instrument played from an .sfz file.

    gain: a trim in dB, so instruments sit level with each other before the mix.
    veltrack: how far velocity moves loudness, 0..100 (the SFZ amp_veltrack; libraries with several
        velocity layers already get louder with their layers, so they want less than 100).
    release: a release time in seconds that overrides the file's (None keeps it).
    max_release: the longest a release tail may run, in seconds.
    switch: the keyswitch key to hold, for the files that have them.
    retune: {part of a sample's file name: cents} to correct recordings that aren't at the pitch
        their region says (VSCO's timpani are up to 90 cents out; measured by their principal
        tone, see instruments.TIMPANI_TUNING).
    even: level every recording to the same loudness (its loudest 50 ms) instead of trusting the
        file's volume opcodes. The libraries' recordings vary by up to 10 dB from one note to the
        next, which makes a tune lurch; evened out, loudness follows velocity alone, and the
        velocity layers still change the tone.
    """

    def __init__(self, sfz_path, gain=0.0, veltrack=60.0, release=None, max_release=3.0, attack=None,
                 switch=None, shift=0, name=None, even=True, retune=None):
        self.name = name or os.path.basename(sfz_path)
        self.inst = sfz.Instrument(sfz_path)
        self.gain = gain
        self.veltrack = veltrack
        self.release = release
        self.max_release = max_release
        self.attack = attack
        self.switch = switch
        self.shift = shift
        self.even = even
        self.retune = retune or {}
        self.lo, self.hi = self.inst.keys()

    def _regions(self, key, vel, rr, rand):
        got = self.inst.pick(key, vel, rr, rand, self.switch)
        if got:
            return got, 0
        # outside the recorded range (or a gap): borrow the nearest key that has a region, re-pitched
        for d in range(1, 13):
            for k in (key - d, key + d):
                got = self.inst.pick(k, vel, rr, rand, self.switch)
                if got:
                    return got, 0
        # the velocity has no layer here: try any velocity
        for v in (64, 100, 30, 127, 1):
            got = self.inst.pick(key, v, rr, rand, self.switch)
            if got:
                return got, 0
        raise ValueError("%s has nothing for key %d" % (self.name, key))

    def render(self, key, vel, dur, rr=0, rand=0.0, attack=None, release=None, cents=0.0):
        """One note: `dur` seconds held, plus its release. Stereo float64."""
        key = int(key) + self.shift
        vel = int(max(1, min(127, round(vel))))
        regions, _ = self._regions(key, vel, rr, rand)
        out = None
        for r, xg in regions:
            src = load(r.path)
            fix = sum(c for part, c in self.retune.items() if part in os.path.basename(r.path))
            ratio = 2.0 ** (((key - r.center) * r.keytrack / 100.0 + (r.tune + cents + fix) / 100.0) / 12.0)
            rel = release if release is not None else (self.release if self.release is not None else r.release)
            rel = min(rel, self.max_release)
            att = attack if attack is not None else (self.attack if self.attack is not None else r.attack)
            gate = int(dur * RATE)
            n = gate + int(rel * RATE) + 1
            off = int(r.offset * RATE / 44100.0)
            need = int(off + n * ratio) + 4
            src = _extended(src, need, off + int(0.5 * RATE))
            pos = off + np.arange(n) * ratio
            y = _cubic(src, pos)
            env = envelope(n, gate, att, r.op.get("ampeg_hold", 0.0), r.decay, r.sustain / 100.0, rel)
            if self.even:
                amp = 0.1 / level(r.path) * xg
            else:
                amp = 10.0 ** (r.volume / 20.0) * xg
            vt = self.veltrack / 100.0
            amp *= (1.0 - vt) + vt * (vel / 127.0) ** 2
            y *= (env * amp)[:, None]
            if out is None:
                out = y
            else:
                m = max(len(out), len(y))
                o = np.zeros((m, 2))
                o[:len(out)] += out
                o[:len(y)] += y
                out = o
        return out * 10.0 ** (self.gain / 20.0)
