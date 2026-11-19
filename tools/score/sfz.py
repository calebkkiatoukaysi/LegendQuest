"""
A reader for the SFZ instrument files the Versilian libraries come with, and the choice of which of
an instrument's recordings plays a given note.

Only the opcodes those files use are understood (fetch_samples.py's libraries, counted across every
file they ship): sample, default_path, lokey/hikey/key, lovel/hivel, pitch_keycenter, pitch_keytrack,
tune, transpose, volume, offset, seq_length/seq_position (round robin), lorand/hirand, trigger,
xfin_/xfout_ lovel/hivel (velocity crossfades), amp_veltrack, the ampeg_ envelope (attack, hold, decay,
sustain, release) and the keyswitch opcodes sw_lokey/sw_hikey/sw_last/sw_default. Anything else is
kept on the region but ignored. Headers nest as SFZ says: <control> sets the path, and <global>,
<master> and <group> opcodes are inherited by every <region> under them until the next header of
their own kind.

    python tools/score/sfz.py <file.sfz>    lists each region: keys, velocities, round robin, sample
"""

import os
import re
import sys

_OPCODE = re.compile(r"(?:^|\s)([A-Za-z0-9_]+)=")
_HEADER = re.compile(r"<(\w+)>")

_INT = {"lokey", "hikey", "key", "lovel", "hivel", "pitch_keycenter", "seq_length", "seq_position",
        "xfin_lovel", "xfin_hivel", "xfout_lovel", "xfout_hivel", "sw_lokey", "sw_hikey", "sw_last",
        "sw_default", "transpose", "offset"}
_FLOAT = {"tune", "volume", "pitch_keytrack", "amp_veltrack", "lorand", "hirand", "ampeg_attack",
          "ampeg_hold", "ampeg_decay", "ampeg_sustain", "ampeg_release", "ampeg_delay", "pan"}

_NOTE = {"c": 0, "d": 2, "e": 4, "f": 5, "g": 7, "a": 9, "b": 11}


def _key(text):
    """A key number from an SFZ value: a MIDI number, or a name such as c4 or f#3 (c4 = 60)."""
    text = text.strip().lower()
    try:
        return int(text)
    except ValueError:
        pass
    m = re.fullmatch(r"([a-g])([#b]?)(-?\d+)", text)
    if not m:
        raise ValueError("not a key: %r" % text)
    shift = {"#": 1, "b": -1, "": 0}[m.group(2)]
    return 12 * (int(m.group(3)) + 1) + _NOTE[m.group(1)] + shift


def _value(name, text):
    if name in _INT:
        return _key(text) if name in {"lokey", "hikey", "key", "pitch_keycenter", "sw_lokey", "sw_hikey",
                                      "sw_last", "sw_default"} else int(float(text))
    if name in _FLOAT:
        return float(text)
    return text


class Region:
    """One recording and the notes and velocities it answers, with every inherited opcode resolved."""

    def __init__(self, opcodes, base):
        o = dict(opcodes)
        if "key" in o:
            o.setdefault("lokey", o["key"])
            o.setdefault("hikey", o["key"])
            o.setdefault("pitch_keycenter", o["key"])
        self.op = o
        self.path = os.path.normpath(os.path.join(base, o.get("default_path", "").replace("\\", "/"),
                                                  o["sample"].replace("\\", "/")))
        self.lokey = o.get("lokey", 0)
        self.hikey = o.get("hikey", 127)
        self.center = o.get("pitch_keycenter", 60)
        self.lovel = o.get("lovel", 0)
        self.hivel = o.get("hivel", 127)
        self.tune = o.get("tune", 0.0) + 100.0 * o.get("transpose", 0)
        self.keytrack = o.get("pitch_keytrack", 100.0)
        self.volume = o.get("volume", 0.0)
        self.offset = o.get("offset", 0)
        self.seq_length = o.get("seq_length", 1)
        self.seq_position = o.get("seq_position", 1)
        self.lorand = o.get("lorand", 0.0)
        self.hirand = o.get("hirand", 1.0)
        self.trigger = o.get("trigger", "attack")
        self.veltrack = o.get("amp_veltrack", 100.0)
        self.attack = o.get("ampeg_attack", 0.0)
        self.hold = o.get("ampeg_hold", 0.0)
        self.decay = o.get("ampeg_decay", 0.0)
        self.sustain = o.get("ampeg_sustain", 100.0)
        self.release = o.get("ampeg_release", 0.001)
        self.sw_lokey = o.get("sw_lokey")
        self.sw_hikey = o.get("sw_hikey")
        self.sw_last = o.get("sw_last")
        self.xfin = (o.get("xfin_lovel"), o.get("xfin_hivel"))
        self.xfout = (o.get("xfout_lovel"), o.get("xfout_hivel"))

    def velocity_window(self):
        """The velocities this region sounds at, widened to cover its crossfades."""
        lo, hi = self.lovel, self.hivel
        if self.xfin[0] is not None:
            lo = min(lo, self.xfin[0])
        if self.xfout[1] is not None:
            hi = max(hi, self.xfout[1])
        return lo, hi

    def crossfade(self, vel):
        """The region's gain from its velocity crossfades at this velocity (equal power), 0..1."""
        g = 1.0
        lo, hi = self.xfin
        if lo is not None and hi is not None and hi > lo:
            if vel < lo:
                return 0.0
            if vel < hi:
                g *= ((vel - lo) / (hi - lo)) ** 0.5
        lo, hi = self.xfout
        if lo is not None and hi is not None and hi > lo:
            if vel > hi:
                return 0.0
            if vel > lo:
                g *= ((hi - vel) / (hi - lo)) ** 0.5
        return g


class Instrument:
    """Every region of one .sfz file."""

    def __init__(self, path):
        self.path = path
        self.regions = parse(path)
        if not self.regions:
            raise ValueError("no regions in " + path)
        self.default_switch = next((r.op["sw_default"] for r in self.regions if "sw_default" in r.op), None)

    def keys(self):
        lo = min(r.lokey for r in self.regions)
        hi = max(r.hikey for r in self.regions)
        return lo, hi

    def pick(self, key, vel, rr=0, rand=0.0, switch=None):
        """The regions that sound for this note, each with its crossfade gain.

        rr counts the notes played on this key so far (round robin: a region sounds when
        rr % seq_length + 1 == seq_position); rand in [0, 1) picks among lorand/hirand regions;
        switch is the keyswitch key held (or the file's sw_default)."""
        switch = self.default_switch if switch is None else switch
        out = []
        for r in self.regions:
            if r.trigger != "attack" or not r.lokey <= key <= r.hikey:
                continue
            lo, hi = r.velocity_window()
            if not lo <= vel <= hi:
                continue
            if r.sw_last is not None and switch is not None and r.sw_last != switch:
                continue
            if r.seq_length > 1 and rr % r.seq_length + 1 != r.seq_position:
                continue
            if not r.lorand <= rand < r.hirand:
                continue
            g = r.crossfade(vel)
            if g > 0.0:
                out.append((r, g))
        return out


def _strip(line):
    i = line.find("//")
    return line if i < 0 else line[:i]


def parse(path):
    """The regions of an SFZ file, in file order."""
    with open(path, encoding="utf-8", errors="replace") as f:
        text = f.read()
    base = os.path.dirname(path)
    scopes = {"control": {}, "global": {}, "master": {}, "group": {}}
    order = ["control", "global", "master", "group"]
    regions = []
    current = None   # the opcodes of the header being read
    kind = None

    def close():
        if kind == "region" and current is not None and "sample" in current:
            merged = {}
            for s in order:
                merged.update(scopes[s])
            merged.update(current)
            regions.append(Region(merged, base))

    for raw in text.splitlines():
        line = _strip(raw)
        pos = 0
        while pos < len(line):
            m = _HEADER.search(line, pos)
            chunk_end = m.start() if m else len(line)
            chunk = line[pos:chunk_end]
            if chunk.strip() and current is not None:
                keys = list(_OPCODE.finditer(chunk))
                for i, k in enumerate(keys):
                    end = keys[i + 1].start() if i + 1 < len(keys) else len(chunk)
                    name = k.group(1)
                    val = chunk[k.end():end].strip()
                    if name not in ("sample", "default_path", "sw_label", "group_label"):
                        val = val.split()[0] if val else val
                    try:
                        current[name] = _value(name, val)
                    except ValueError:
                        current[name] = val
            if not m:
                break
            close()
            kind = m.group(1)
            if kind in scopes:
                # a new header of this kind resets it and every scope nested inside it
                for s in order[order.index(kind):]:
                    scopes[s] = {}
                current = scopes[kind]
            elif kind == "region":
                current = {}
            else:
                current = {}   # <curve>, <effect> and the like: read and dropped
            pos = m.end()
    close()
    return regions


if __name__ == "__main__":
    inst = Instrument(sys.argv[1])
    print("%d regions, keys %d-%d" % ((len(inst.regions),) + inst.keys()))
    for r in inst.regions:
        print("key %3d-%3d centre %3d vel %3d-%3d rr %d/%d sw %s vol %+5.1f  %s" % (
            r.lokey, r.hikey, r.center, r.lovel, r.hivel, r.seq_position, r.seq_length, r.sw_last,
            r.volume, os.path.basename(r.path)))
