"""
Writing tools the pieces share: chord charts, pads that voice-lead, broken chords, ostinati and
drum patterns. Each writes notes into a score.Part and returns the beat it finished on.

A chord chart is a string of chord symbols each with its length in beats, bar lines ignored:
    "Dm:4 | Bb:2 C:2 | Dm:4"
"""

from theory import bass_note, chord, nearest, progression


def chart(text):
    """[(symbol, beats, offset from the chart's start)] from a chart string."""
    out, t = [], 0.0
    for tok in text.split():
        if tok == "|":
            continue
        sym, beats = tok.rsplit(":", 1)
        out.append((sym, float(beats), t))
        t += float(beats)
    return out


def length(text):
    return sum(b for _, b, _ in chart(text))


def pad(part, start, text, voicing, lo, hi, vel=70, attack=0.25, release=None, legato=1.02, swell=None):
    """Held chords, voiced from `voicing` (a list of MIDI notes, one per voice) and led on."""
    ch = chart(text)
    vo = progression([s for s, _, _ in ch], voicing, lo, hi)
    for (sym, beats, off), notes in zip(ch, vo):
        for k in notes:
            part.note(start + off, k, beats, vel, legato=legato, attack=attack, release=release, swell=swell)
    return start + sum(b for _, b, _ in ch), vo[-1]


def bass(part, start, text, near, vel=80, rhythm=None, octave=False, legato=0.98, **kw):
    """The chords' bass notes near `near`; rhythm=[(offset, beats, vel delta), ...] repeats it
    inside each chord (cut at the chord's end), and octave=True doubles it an octave down."""
    for sym, beats, off in chart(text):
        k = bass_note(sym, near)
        hits = rhythm or [(0.0, beats, 0)]
        reps = 1
        if rhythm:
            span = max(o + b for o, b, _ in rhythm)
            reps = max(1, int(round(beats / span)))
        else:
            span = beats
        for r in range(reps):
            for o, b, dv in hits:
                t = off + r * span + o
                if t >= off + beats - 1e-6:
                    continue
                b = min(b, off + beats - t)
                part.note(start + t, k, b, vel + dv, legato=legato, **kw)
                if octave:
                    part.note(start + t, k - 12, b, vel + dv - 4, legato=legato, **kw)
    return start + length(text)


def broken(part, start, text, near, pattern, step, vel=70, accent=10, legato=1.0, span=3, **kw):
    """Broken chords: `pattern` indexes the chord's notes stacked upward from near `near`
    (0 = the bass of the stack; span = how many octaves of chord tones are stacked), one per
    `step` beats, repeated through each chord. The first note of each chord is accented."""
    for sym, beats, off in chart(text):
        _, tones, _ = chord(sym)
        base = nearest(tones[0], near)
        stack = sorted({nearest(t, base + 6) + 12 * o for t in tones for o in range(span)})
        stack = [k for k in stack if k >= base]
        steps = int(round(beats / step))
        for i in range(steps):
            idx = pattern[i % len(pattern)]
            if idx is None:
                continue
            k = stack[min(idx, len(stack) - 1)]
            part.note(start + off + i * step, k, step, vel + (accent if i == 0 else 0), legato=legato, **kw)
    return start + length(text)


def drums(part, start, pattern, key, vel=90, step=0.25, repeats=1, loud=18, soft=-22):
    """A drum pattern, one character a step: 'X' loud, 'x' normal, 'o' soft (a ghost note),
    '.' rest, '|' ignored. Plays `repeats` times."""
    pattern = pattern.replace("|", "").replace(" ", "")
    t = start
    for _ in range(repeats):
        for c in pattern:
            if c in "Xxo":
                part.note(t, key, step * 4, vel + {"X": loud, "x": 0, "o": soft}[c], legato=1.0)
            t += step
    return t


def ostinato(part, start, notes, step, beats, vel=80, accents=None, legato=0.9, **kw):
    """A repeated figure: `notes` (MIDI, or None for a rest) one per `step` beats, round and round
    for `beats` beats; accents is a string as long as the figure, '>' marking the accented steps."""
    count = int(round(beats / step))
    for i in range(count):
        k = notes[i % len(notes)]
        if k is None:
            continue
        acc = accents is not None and accents[i % len(accents)] == ">"
        part.note(start + i * step, k, step, vel + (14 if acc else 0), legato=legato, **kw)
    return start + beats


def melody(part, start, pairs, vel=85, transpose=0, shape=None, **kw):
    """[(midi, beats), ...] with a phrase's dynamic shape: the velocity rises toward the phrase's
    highest note and eases off at its end, unless shape is given as a list of velocity deltas."""
    if not pairs:
        return start
    top = max(k for k, _ in pairs if k is not None)
    t = start
    for i, (k, beats) in enumerate(pairs):
        if k is not None:
            if shape is not None:
                dv = shape[i % len(shape)]
            else:
                dv = 6 if k == top else 0
                if i == len(pairs) - 1:
                    dv -= 4
            part.note(t, k + transpose, beats, vel + dv, **kw)
        t += beats
    return t
