"""
Checks the score's WAVs the way an ear would, in numbers. Run it from the repo root:

    python tools/score/check.py                 # every file compose.py writes, as a table
    python tools/score/check.py steel victory   # just these
    python tools/score/check.py --bars steel    # also each bar's loudness and its strongest notes
    python tools/score/check.py --png DIR steel # also a spectrogram and a loudness curve, as PNGs

For each file: its length (a song 60-150 s and a whole number of bars, a jingle 4-8 s, the stinger
1.5-3 s), its integrated loudness (BS.1770-4, measured round the loop for a song), its true peak
(four times oversampled; it must stay at or under -1 dBFS), and for a song the loop seam, three ways:

    jump    the step from the last sample to the first, against the mean step in the 10 ms either
            side, and where it ranks among every sample-to-sample step in the song (a click would
            be many times the local step and among the very largest; it passes under 3x the local
            step, or if it's no bigger than 99.9% of the song's own steps)
    rms     the level of the 20 ms before the seam against the 20 ms after it, against the same
            change across every other bar line (it passes under 6 dB, or no more than 1 dB over the
            most any other bar line changes: the seam must be no worse than the song itself)
    hf      the energy above 6 kHz in a 256-sample window centred on the seam, against the median
            of the same windows in the half second either side (a click is a burst of highs). The
            seam is a bar line, and a downbeat's attack has highs of its own, so the same is
            measured at every other bar line: the seam passes if it's under 6 dB or no more than
            1 dB over the most any other bar line shows.

It exits non-zero if anything fails.
"""

import math
import os
import sys

import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import compose  # noqa: E402
import mix  # noqa: E402

LIMITS = {"song": (60.0, 150.0), "jingle": (4.0, 8.0), "stinger": (1.5, 3.0)}


def kind(name):
    if name in compose.SONGS:
        return "song"
    return "stinger" if name == "engage" else "jingle"


def _hf_burst(ring, centre, rate, win=256):
    """Energy above 6 kHz in a window centred at `centre` of `ring`, against the median of the
    same windows in the half second either side (dB)."""
    def hf(seg):
        spec = np.abs(np.fft.rfft(seg.mean(axis=1) * np.hanning(len(seg)))) ** 2
        f = np.fft.rfftfreq(len(seg), 1.0 / rate)
        return float(np.sum(spec[f > 6000.0])) + 1e-15
    half = int(0.5 * rate)
    at = hf(ring[centre - win // 2: centre + win // 2])
    others = [hf(ring[c - win // 2: c + win // 2]) for c in range(centre - half + win, centre + half - win, win)
              if abs(c - centre) > win]
    return 10.0 * math.log10(at / float(np.median(others)))


def seam(x, rate, bar_len):
    n = len(x)
    w = int(0.010 * rate)
    ring = np.concatenate([x[-w:], x[:w + 1]])
    steps = np.abs(np.diff(ring, axis=0))
    jump = float(np.max(np.abs(x[0] - x[-1])))
    every = np.max(np.abs(np.diff(x, axis=0)), axis=1)
    rank = 100.0 * float(np.mean(every < jump))       # where the seam's step falls among all of them
    local = float(np.mean(np.delete(steps, w - 1, axis=0)))
    # rms just either side, against the biggest 20 ms-to-20 ms change anywhere in the song
    k = int(0.020 * rate)
    def r(a):
        return mix.db(math.sqrt(float(np.mean(a ** 2))) + 1e-9)
    before, after = r(x[-k:]), r(x[:k])
    # the same change across every other bar line (a downbeat often lands louder than the bar
    # before it ended, and the seam is a bar line too)
    typical = max(abs(r(x[b:b + k]) - r(x[b - k:b])) for b in range(bar_len, n - k, bar_len))
    # a burst of highs at the seam, against the same measure at every other bar line (the seam is
    # a bar line too, and a downbeat's attack has highs of its own; a click would stand out)
    half = int(0.6 * rate)
    ring = np.concatenate([x[-half:], x, x[:half]])
    hf_db = _hf_burst(ring, half, rate)
    bar_hf = [_hf_burst(ring, half + b, rate) for b in range(bar_len, n - bar_len // 2, bar_len)]
    return dict(jump=jump, local=local, ratio=jump / max(local, 1e-9), rank=rank, before=before, after=after,
                rms_delta=abs(after - before), rms_typ=typical, hf_db=hf_db,
                hf_bars=float(np.max(bar_hf)) if bar_hf else 0.0)


def check(name, path, bars=False, png=None):
    rate, x = mix.read_wav(path)
    k = kind(name)
    dur = len(x) / rate
    loop = k == "song"
    row = dict(name=name, kind=k, dur=dur, rate=rate, lufs=mix.lufs(x, rate, loop=loop),
               tp=mix.true_peak(x, loop=loop), peak=mix.db(float(np.max(np.abs(x)))))
    lo, hi = LIMITS[k]
    fails = []
    if not lo <= dur <= hi:
        fails.append("length %.2f s outside %g-%g" % (dur, lo, hi))
    if row["tp"] > -1.0:
        fails.append("true peak %.2f dBFS over -1" % row["tp"])
    if loop:
        piece = compose.build(name)
        if len(x) != piece.length:
            fails.append("length %d samples is not %d bars (%d)" % (len(x), piece.bars, piece.length))
        s = seam(x, rate, piece.spb * piece.meter)
        row.update(s)
        if s["ratio"] > 3.0 and s["jump"] > 0.005 and s["rank"] > 99.9:
            fails.append("seam jump %.4f is %.1fx the local step" % (s["jump"], s["ratio"]))
        if s["rms_delta"] > max(6.0, s["rms_typ"] + 1.0):
            fails.append("seam level changes %.1f dB" % s["rms_delta"])
        if s["hf_db"] > max(6.0, s["hf_bars"] + 1.0):
            fails.append("seam has %.1f dB more highs than around it (other bar lines: at most %.1f)"
                         % (s["hf_db"], s["hf_bars"]))
    else:
        tail = x[-int(0.01 * rate):]
        if float(np.max(np.abs(tail))) > 0.001:
            fails.append("does not end in silence")
    row["fails"] = fails
    if bars and loop:
        print_bars(name, x, rate)
    if png:
        plot(name, x, rate, png)
    return row


NAMES = ["C", "C#", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B"]


def chroma(seg, rate):
    """The strength of each pitch class in a stretch of audio (60 Hz to 2 kHz)."""
    m = seg.mean(axis=1)
    spec = np.abs(np.fft.rfft(m * np.hanning(len(m))))
    f = np.fft.rfftfreq(len(m), 1.0 / rate)
    ok = (f > 60.0) & (f < 2000.0)
    pcs = np.round(12.0 * np.log2(f[ok] / 440.0)).astype(int) % 12
    pcs = (pcs + 9) % 12
    out = np.zeros(12)
    np.add.at(out, pcs, spec[ok] ** 2)
    return out / (out.max() + 1e-12)


def print_bars(name, x, rate):
    piece = compose.build(name)
    spb = piece.spb * piece.meter
    print("  bar  LUFS-ish  strongest notes")
    for b in range(piece.bars):
        seg = x[b * spb:(b + 1) * spb]
        l = mix.db(math.sqrt(float(np.mean(seg ** 2))) + 1e-9)
        c = chroma(seg, rate)
        top = [NAMES[i] for i in np.argsort(c)[::-1][:4] if c[i] > 0.25]
        print("  %3d  %6.1f   %s" % (b, l, " ".join(top)))


def plot(name, x, rate, out):
    from PIL import Image, ImageDraw
    os.makedirs(out, exist_ok=True)
    m = x.mean(axis=1)
    hop, win = 512, 2048
    frames = (len(m) - win) // hop
    w = np.hanning(win)
    spec = np.array([np.abs(np.fft.rfft(m[i * hop:i * hop + win] * w)) for i in range(frames)])
    f = np.fft.rfftfreq(win, 1.0 / rate)
    # log-frequency rows 40 Hz-16 kHz
    rows = 300
    edges = np.geomspace(40.0, 16000.0, rows + 1)
    img = np.zeros((rows, frames))
    for r in range(rows):
        sel = (f >= edges[r]) & (f < edges[r + 1])
        if not sel.any():
            sel = np.argmin(np.abs(f - edges[r]))
            img[r] = spec[:, sel]
        else:
            img[r] = spec[:, sel].mean(axis=1)
    img = 20.0 * np.log10(img + 1e-9)
    img = np.clip((img - (img.max() - 80.0)) / 80.0, 0.0, 1.0)[::-1]
    width = 1200
    pic = Image.fromarray((img * 255).astype(np.uint8)).resize((width, rows))
    canvas = Image.new("RGB", (width, rows + 140), "white")
    canvas.paste(pic.convert("RGB"), (0, 0))
    d = ImageDraw.Draw(canvas)
    # loudness curve (400 ms blocks), -40..0 dB
    blk = int(0.4 * rate)
    lv = [mix.db(math.sqrt(float(np.mean(x[i:i + blk] ** 2))) + 1e-9) for i in range(0, len(x) - blk, blk // 2)]
    pts = [(int(i * width / len(lv)), rows + 130 - int((min(0, max(-40, v)) + 40) * 3)) for i, v in enumerate(lv)]
    d.line(pts, fill="red", width=2)
    for dbv in (-10, -20, -30):
        y = rows + 130 - (dbv + 40) * 3
        d.line([(0, y), (width, y)], fill=(200, 200, 200))
        d.text((2, y - 10), "%d dB" % dbv, fill="black")
    try:
        piece = compose.build(name)
        for b in range(0, piece.bars + 1, 4 if piece.bars > 16 else 1):
            xpos = int(b * piece.spb * piece.meter / len(x) * width)
            d.line([(xpos, 0), (xpos, rows + 140)], fill=(0, 120, 255))
            d.text((xpos + 2, 2), str(b), fill="yellow")
    except Exception:
        pass
    d.text((5, rows + 5), "%s  (%.1f s)" % (name, len(x) / rate), fill="black")
    canvas.save(os.path.join(out, name + ".png"))


def main(argv):
    bars = "--bars" in argv
    png = None
    args = [a for a in argv if a != "--bars"]
    if "--png" in args:
        i = args.index("--png")
        png = args[i + 1]
        del args[i:i + 2]
    folder = compose.OUT
    if "--dir" in args:
        i = args.index("--dir")
        folder = args[i + 1]
        del args[i:i + 2]
    names = args or list(compose.ALL)
    rows = []
    for nm in names:
        path = os.path.join(folder, nm + ".wav")
        if not os.path.isfile(path):
            rows.append(dict(name=nm, fails=["missing: " + path]))
            continue
        rows.append(check(nm, path, bars, png))
    print("| file | kind | length s | LUFS | true peak dBFS | seam jump (x local step, rank) | seam rms before/after dB | seam highs dB | result |")
    print("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")
    bad = 0
    for r in rows:
        if "dur" not in r:
            print("| %s | | | | | | | | FAIL: %s |" % (r["name"], "; ".join(r["fails"])))
            bad += 1
            continue
        seam_cols = ("%.4f (%.1fx; %.0f%% of steps smaller)" % (r["jump"], r["ratio"], r["rank"]), "%.1f / %.1f" % (r["before"], r["after"]),
                     "%+.1f (bars max %+.1f)" % (r["hf_db"], r["hf_bars"])) if "jump" in r else ("-", "-", "-")
        res = "ok" if not r["fails"] else "FAIL: " + "; ".join(r["fails"])
        bad += bool(r["fails"])
        print("| %s.wav | %s | %.2f | %.1f | %.1f | %s | %s | %s | %s |" % ((r["name"], r["kind"], r["dur"], r["lufs"], r["tp"]) + seam_cols + (res,)))
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
