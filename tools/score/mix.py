"""
The mixing desk: filters, the hall, loudness, the limiter and the WAV writer, all in numpy.

A looping song is processed as a circle. Its buffer is exactly a whole number of bars long, and
every note that runs past the end has already been wrapped onto the start (score.py). Here the
filters and the reverb work in the frequency domain over the whole buffer, so they're circular too:
the hall's tail at the end of the loop rings on into its first bars exactly as it would on the
second time round, and an equaliser has no start-up. The limiter has no memory (each sample is
bent by the same curve), so nothing about the seam differs from any other sample, and the loop
plays round without a click, a gap or a jump in level.

A one-shot (a jingle or a stinger) gets silence padded after it long enough for the hall to die
away, and the same circular maths then never wraps anything audible.

Loudness is ITU-R BS.1770-4: K-weighted, 400 ms blocks overlapping by 75%, gated at -70 LUFS and
then 10 LU under the ungated level. True peak is measured four times oversampled.
"""

import math
import wave

import numpy as np
from scipy.signal import lfilter, resample_poly

from sampler import RATE


def db(x):
    return 20.0 * math.log10(max(x, 1e-12))


def undb(d):
    return 10.0 ** (d / 20.0)


# --------------------------------------------------------------------------- #
# Filters
# --------------------------------------------------------------------------- #

def biquad(kind, fc, q=0.7071, gain_db=0.0, rate=RATE):
    """RBJ cookbook biquad coefficients (b, a), normalised."""
    w0 = 2.0 * math.pi * fc / rate
    cw, sw = math.cos(w0), math.sin(w0)
    alpha = sw / (2.0 * q)
    A = 10.0 ** (gain_db / 40.0)
    if kind == "lowpass":
        b = [(1 - cw) / 2, 1 - cw, (1 - cw) / 2]
        a = [1 + alpha, -2 * cw, 1 - alpha]
    elif kind == "highpass":
        b = [(1 + cw) / 2, -(1 + cw), (1 + cw) / 2]
        a = [1 + alpha, -2 * cw, 1 - alpha]
    elif kind == "bandpass":
        b = [alpha, 0.0, -alpha]
        a = [1 + alpha, -2 * cw, 1 - alpha]
    elif kind == "peak":
        b = [1 + alpha * A, -2 * cw, 1 - alpha * A]
        a = [1 + alpha / A, -2 * cw, 1 - alpha / A]
    elif kind == "highshelf":
        sa = 2.0 * math.sqrt(A) * alpha
        b = [A * ((A + 1) + (A - 1) * cw + sa), -2 * A * ((A - 1) + (A + 1) * cw), A * ((A + 1) + (A - 1) * cw - sa)]
        a = [(A + 1) - (A - 1) * cw + sa, 2 * ((A - 1) - (A + 1) * cw), (A + 1) - (A - 1) * cw - sa]
    elif kind == "lowshelf":
        sa = 2.0 * math.sqrt(A) * alpha
        b = [A * ((A + 1) - (A - 1) * cw + sa), 2 * A * ((A - 1) - (A + 1) * cw), A * ((A + 1) - (A - 1) * cw - sa)]
        a = [(A + 1) + (A - 1) * cw + sa, -2 * ((A - 1) + (A + 1) * cw), (A + 1) + (A - 1) * cw - sa]
    else:
        raise ValueError(kind)
    return np.array(b) / a[0], np.array(a) / a[0]


def response(b, a, freqs, rate=RATE):
    """A biquad's complex response at these frequencies."""
    z = np.exp(-1j * 2.0 * np.pi * freqs / rate)
    return (b[0] + b[1] * z + b[2] * z * z) / (a[0] + a[1] * z + a[2] * z * z)


def eq(x, bands, rate=RATE):
    """x (frames, 2) through a chain of biquads, applied circularly in the frequency domain.

    bands: [(kind, fc, q, gain_db), ...]. The magnitude and phase are the biquads' own, so a pad
    rounded off by a lowpass sounds as it would through the analogue filter, minus its start-up."""
    if not bands:
        return x
    n = len(x)
    freqs = np.fft.rfftfreq(n, 1.0 / rate)
    h = np.ones(len(freqs), complex)
    for kind, fc, q, g in bands:
        b, a = biquad(kind, fc, q, g, rate)
        h *= response(b, a, freqs, rate)
    X = np.fft.rfft(x, axis=0)
    return np.fft.irfft(X * h[:, None], n=n, axis=0)


# --------------------------------------------------------------------------- #
# The hall
# --------------------------------------------------------------------------- #

def hall_ir(seed, rt60=2.2, predelay=0.02, hf_rt=0.45, lf_rt=1.25, early=0.5, length=None, rate=RATE):
    """A synthetic stereo impulse response: a spray of early reflections, then a diffuse tail of
    decorrelated noise whose decay time varies with frequency (rt60 in the middle, hf_rt times it
    at the top, lf_rt times it at the bottom), as a stone hall's does. No recording, no licence."""
    rng = np.random.default_rng(seed)
    length = length or rt60 * 1.15 + predelay
    n = int(length * rate)
    t = np.arange(n) / rate
    out = np.zeros((n, 2))
    pre = int(predelay * rate)
    # the tail, band by band: four bands crossing at 250 Hz, 1.2 kHz and 4.5 kHz
    edges = [0.0, 250.0, 1200.0, 4500.0, rate / 2]
    factors = [lf_rt, 1.0, 0.8, hf_rt]
    freqs = np.fft.rfftfreq(n, 1.0 / rate)
    for ch in range(2):
        noise = rng.standard_normal(n)
        N = np.fft.rfft(noise)
        tail = np.zeros(n)
        for i in range(4):
            lo, hi = edges[i], edges[i + 1]
            band = np.fft.irfft(N * _band_weight(freqs, lo, hi), n=n)
            rt = rt60 * factors[i]
            band *= np.exp(-6.9078 * np.maximum(t - predelay, 0.0) / rt)
            tail += band
        onset = np.clip((t - predelay) / 0.03, 0.0, 1.0)  # the tail swells in over 30 ms
        tail *= onset
        # early reflections: a dozen taps between 7 and 70 ms, thinning out
        er = np.zeros(n)
        for k in range(12):
            d = pre + int(rng.uniform(0.007, 0.07) * rate)
            if d < n:
                er[d] += rng.choice([-1.0, 1.0]) * rng.uniform(0.3, 1.0) * (1.0 - k / 14.0)
        er_energy = math.sqrt(np.sum(er ** 2)) or 1.0
        tail_energy = math.sqrt(np.sum(tail ** 2)) or 1.0
        out[:, ch] = tail / tail_energy + early * er / er_energy
    # a fade to nothing at the very end
    k = int(0.05 * rate)
    out[-k:] *= np.linspace(1.0, 0.0, k)[:, None]
    return out / math.sqrt(np.mean(np.sum(out ** 2, axis=0)))


def _band_weight(freqs, lo, hi):
    """A band of a complementary set: smooth raised-cosine crossovers an octave wide on a log axis."""
    f = np.maximum(freqs, 1e-3)
    def step(edge):  # 0 below edge/sqrt2, 1 above edge*sqrt2
        if edge <= 0:
            return np.ones_like(f)
        if edge >= RATE / 2:
            return np.zeros_like(f)
        u = np.clip((np.log2(f / edge) + 0.5), 0.0, 1.0)
        return np.sin(0.5 * np.pi * u) ** 2
    return step(lo) - step(hi)


def convolve(x, ir):
    """x (frames, 2) convolved circularly with a stereo IR: left with the IR's left, right with its
    right. x must be at least as long as the IR."""
    n = len(x)
    if len(ir) > n:
        ir = ir[:n]
    out = np.empty_like(x)
    for ch in range(2):
        X = np.fft.rfft(x[:, ch])
        H = np.fft.rfft(ir[:, ch], n=n)
        out[:, ch] = np.fft.irfft(X * H, n=n)
    return out


# --------------------------------------------------------------------------- #
# Loudness, peaks and the limiter
# --------------------------------------------------------------------------- #

def k_weight(x, rate=RATE):
    """The BS.1770 K-weighting: a high shelf (+4 dB above about 1.7 kHz) and a high-pass at 38 Hz,
    designed for this rate from the standard's analogue prototypes."""
    b1, a1 = biquad("highshelf", 1681.974450955533, 0.7071752369554196, 3.999843853973347, rate)
    b2, a2 = biquad("highpass", 38.13547087602444, 0.5003270373238773, 0.0, rate)
    y = lfilter(b1, a1, x, axis=0)
    return lfilter(b2, a2, y, axis=0)


def lufs(x, rate=RATE, loop=False):
    """Integrated loudness (LUFS). A loop is measured as it plays round: a second pass is prepended
    so the filters and the blocks see the wrap."""
    if loop:
        y = k_weight(np.concatenate([x, x]), rate)[len(x):]
    else:
        y = k_weight(x, rate)
    block, hop = int(0.4 * rate), int(0.1 * rate)
    if len(y) < block:
        return -70.0
    if loop:
        y = np.concatenate([y, y[:block]])
        starts = range(0, len(x), hop)
    else:
        starts = range(0, len(y) - block + 1, hop)
    p = np.mean(y ** 2, axis=1) * 2.0          # sum over the two channels, each weighted 1
    c = np.concatenate([[0.0], np.cumsum(p)])
    z = np.array([(c[s + block] - c[s]) / block for s in starts])
    l = -0.691 + 10.0 * np.log10(np.maximum(z, 1e-12))
    z = z[l > -70.0]
    if not len(z):
        return -70.0
    rel = -0.691 + 10.0 * np.log10(np.mean(z)) - 10.0
    l = -0.691 + 10.0 * np.log10(np.maximum(z, 1e-12))
    z = z[l > rel]
    return -0.691 + 10.0 * math.log10(np.mean(z))


def true_peak(x, loop=False):
    """The highest inter-sample peak, four times oversampled (dBFS)."""
    if loop:
        pad = 64
        y = np.concatenate([x[-pad:], x, x[:pad]])
        up = resample_poly(y, 4, 1, axis=0)[4 * pad:-4 * pad]
    else:
        up = resample_poly(x, 4, 1, axis=0)
    return db(float(np.max(np.abs(up))))


def soft_limit(x, knee, ceiling):
    """A memoryless limiter: straight up to `knee`, then bending smoothly (tanh) to `ceiling`."""
    a = np.abs(x)
    over = a > knee
    room = ceiling - knee
    y = x.copy()
    y[over] = np.sign(x[over]) * (knee + room * np.tanh((a[over] - knee) / room))
    return y


def master(x, target_lufs, loop=True, ceiling_db=-1.0, highpass=32.0, report=None):
    """The final stage: a gentle high-pass, then gain to the target loudness, the limiter, and a
    true-peak check that pulls the ceiling down until no inter-sample peak passes ceiling_db."""
    x = eq(x, [("highpass", highpass, 0.7071, 0.0)])
    ceiling = undb(ceiling_db - 0.6)
    gain = undb(target_lufs - lufs(x, loop=loop))
    for _ in range(8):
        y = soft_limit(x * gain, 0.55 * ceiling, ceiling)
        got = lufs(y, loop=loop)
        tp = true_peak(y, loop=loop)
        if tp > ceiling_db:
            ceiling *= undb(ceiling_db - tp - 0.05)
            continue
        if abs(got - target_lufs) < 0.05:
            break
        gain *= undb(target_lufs - got)
    if report is not None:
        report.update(lufs=got, true_peak=tp, gain_db=db(gain))
    return y


# --------------------------------------------------------------------------- #
# Files
# --------------------------------------------------------------------------- #

def write_wav(path, x, seed=0, rate=RATE):
    """16-bit stereo, with a seeded triangular dither of one step (so a rerun is byte for byte the same)."""
    rng = np.random.default_rng(seed)
    d = (rng.random(x.shape) - rng.random(x.shape))
    pcm = np.clip(np.round(x * 32767.0 + d), -32768, 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(pcm.tobytes())


def read_wav(path):
    with wave.open(path, "rb") as w:
        rate, ch, n = w.getframerate(), w.getnchannels(), w.getnframes()
        data = np.frombuffer(w.readframes(n), "<i2").astype(np.float64) / 32768.0
    return rate, data.reshape(-1, ch)
