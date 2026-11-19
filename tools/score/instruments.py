"""
The orchestra: every instrument a piece can name, and where its recordings are.

Each name maps to an .sfz file in the sample cache (VSCO 2 CE or VCSL, fetched by fetch_samples.py)
or to one of synth.py's voices. A sampled instrument is levelled when it's first used: one note in
the middle of its range at velocity 90 is played and its loudest 50 ms is brought to -18 dBFS, so
every instrument starts the mix level with every other and the pieces set the balance themselves,
in their parts' gains, the way an orchestrator marks dynamics.

Unpitched percussion plays its articulations on keys, as the .sfz files map them; the constants
below name the ones the pieces use (BASS_DRUM_HIT and so on).
"""

import math
import os

import numpy as np

import sampler
import synth

VSCO = "VSCO-2-CE"
VCSL = "VCSL"
MEMB = VCSL + "/Membranophones/Struck Membranophones/"
IDIO = VCSL + "/Idiophones/Struck Idiophones/"
EDGE = VCSL + "/Aerophones/Edge-blown Aerophones/"
CHORD = VCSL + "/Chordophones/Composite Chordophones/"

# VSCO 2 CE's five timpani aren't at the pitches their regions claim. Each drum's principal tone,
# measured in every one of its hits (the strongest partial within a third of the claimed pitch, whose
# neighbours sit at the 1.5 and 2 times a timpano's modes do), is this many cents out, consistently
# across its velocity layers and round robins; the correction is the opposite. The rolls are the
# same five drums.
TIMPANI_TUNING = {"Timpani1_": +54.0, "Timpani2_": -76.0, "Timpani3_": -51.0, "Timpani4_": -47.0,
                  "Timpani5_": -68.0}

# name: (sfz file, options for sampler.Sampled, the key to level it on (None: the middle))
SPECS = {
    # strings (VSCO 2 CE's sections: about eight violins, six violas, six cellos; a solo bass)
    "vln":        (VSCO + "/ViolinEnsSusVib.sfz", dict(veltrack=80), None),
    "vln_soft":   (VSCO + "/ViolinEnsSusVib-Quiet.sfz", dict(veltrack=80), None),
    "vln_spic":   (VSCO + "/ViolinEnsSpic.sfz", dict(veltrack=80, max_release=0.35), None),
    "vln_trem":   (VSCO + "/ViolinEnsTrem.sfz", dict(veltrack=80), None),
    "vln_pizz":   (VSCO + "/ViolinEnsPizz.sfz", dict(veltrack=80, max_release=1.5), None),
    "vla":        (VSCO + "/ViolaEnsSusVib.sfz", dict(veltrack=80), None),
    "vla_spic":   (VSCO + "/ViolaEnsSpic.sfz", dict(veltrack=80, max_release=0.35), None),
    "vla_trem":   (VSCO + "/ViolaEnsTrem.sfz", dict(veltrack=80), None),
    "vla_pizz":   (VSCO + "/ViolaEnsPizz.sfz", dict(veltrack=80, max_release=1.5), None),
    "vc":         (VSCO + "/CelloEnsSusVib.sfz", dict(veltrack=80), None),
    "vc_spic":    (VSCO + "/CelloEnsSpic.sfz", dict(veltrack=80, max_release=0.4), None),
    "vc_trem":    (VSCO + "/CelloEnsTrem.sfz", dict(veltrack=80), None),
    "vc_pizz":    (VSCO + "/CelloEnsPizz.sfz", dict(veltrack=80, max_release=1.8), None),
    "cb":         (VSCO + "/ContrabassSusVB.sfz", dict(veltrack=80), None),
    "cb_spic":    (VSCO + "/ContrabassSpic.sfz", dict(veltrack=80, max_release=0.4), None),
    "cb_pizz":    (VSCO + "/ContrabassPizz.sfz", dict(veltrack=80, max_release=2.0), None),
    "cb_trem":    (VSCO + "/ContrabassTrem.sfz", dict(veltrack=80), None),
    "solo_vln":   (VSCO + "/SViolinVib.sfz", dict(veltrack=80), None),
    "harp":       (VSCO + "/Harp.sfz", dict(veltrack=88, max_release=4.0), None),
    "folk_harp":  (CHORD + "Folk Harp.sfz", dict(veltrack=80, max_release=3.5), None),
    "strum":      (CHORD + "Strumstick.sfz", dict(veltrack=80, max_release=2.5), None),
    # brass
    "horn":       (VSCO + "/FHornSus.sfz", dict(veltrack=80), None),
    "horn_stac":  (VSCO + "/FHornStac.sfz", dict(veltrack=80, max_release=0.6), None),
    "tpt":        (VSCO + "/TrumpetSus.sfz", dict(veltrack=80), None),
    "tpt_vib":    (VSCO + "/TrumpetSusVib.sfz", dict(veltrack=80), None),
    "tpt_stac":   (VSCO + "/TrumpetStac.sfz", dict(veltrack=80, max_release=0.6), None),
    "tbn":        (VSCO + "/TromboneSus.sfz", dict(veltrack=80), None),
    "tbn_stac":   (VSCO + "/TromboneStac.sfz", dict(veltrack=80, max_release=0.6), None),
    "tuba":       (VSCO + "/TubaSus.sfz", dict(veltrack=80), None),
    "tuba_stac":  (VSCO + "/TubaStac.sfz", dict(veltrack=80, max_release=0.7), None),
    # woodwinds
    "flute":      (VSCO + "/FluteSusVib.sfz", dict(veltrack=80), None),
    "flute_stac": (VSCO + "/FluteStac.sfz", dict(veltrack=80, max_release=0.5), None),
    "piccolo":    (VSCO + "/PiccoloSus.sfz", dict(veltrack=80), None),
    "oboe":       (VSCO + "/OboeSusVib.sfz", dict(veltrack=80), None),
    "clar":       (VSCO + "/ClarinetSus.sfz", dict(veltrack=80), None),
    "bsn":        (VSCO + "/BassoonSus.sfz", dict(veltrack=80), None),
    "bsn_stac":   (VSCO + "/BassoonStac.sfz", dict(veltrack=80, max_release=0.5), None),
    "recorder":   (EDGE + "Baroque Alto Recorder - SusVib.sfz", dict(veltrack=60), None),
    "recorder_hi": (EDGE + "Baroque Soprano Recorder - Sustain.sfz", dict(veltrack=60), None),
    "recorder_lo": (EDGE + "Baroque Tenor Recorder - SusVib.sfz", dict(veltrack=60), None),
    # keys and bells
    "organ":      (VSCO + "/OrganLoud.sfz", dict(veltrack=10), None),
    "organ_soft": (VSCO + "/OrganQuiet.sfz", dict(veltrack=10), None),
    "organ_pedal": (VSCO + "/OrganLoudPedal.sfz", dict(veltrack=10), None),
    "glock":      (VSCO + "/Glockenspiel.sfz", dict(veltrack=80, max_release=3.0), None),
    "bells":      (IDIO + "Tubular Bells 1.sfz", dict(veltrack=80, max_release=6.0), None),
    "chimes":     (IDIO + "Hand Chimes.sfz", dict(veltrack=80, max_release=4.0), None),
    # drums and metal (unpitched: the key picks the stroke)
    "timp":       (VSCO + "/Timpani.sfz", dict(veltrack=88, max_release=3.0, retune=TIMPANI_TUNING), 45),
    "timp_roll":  (VSCO + "/TimpaniRolls.sfz", dict(veltrack=80, max_release=2.0, retune=TIMPANI_TUNING), 45),
    "bass_drum":  (MEMB + "Bass Drum 2.sfz", dict(veltrack=80, max_release=3.0), 62),
    "snare":      (MEMB + "Snare Drum, Rope Tension.sfz", dict(veltrack=80, max_release=1.2), 62),
    "tom":        (MEMB + "Tom 1.sfz", dict(veltrack=80, max_release=1.5), 62),
    "frame_drum": (MEMB + "Frame Drum.sfz", dict(veltrack=88, max_release=1.5), 61),
    "gong":       (IDIO + "Gong 1.sfz", dict(veltrack=88, max_release=8.0), 60),
    "cymbal":     (IDIO + "Clash Cymbals 1.sfz", dict(veltrack=88, max_release=5.0), 60),
    "sus_cymbal": (IDIO + "Suspended Cymbal 1.sfz", dict(veltrack=88, max_release=5.0), 68),
    "tambourine": (IDIO + "Tambourine 1.sfz", dict(veltrack=88, max_release=1.0), 60),
    "anvil":      (IDIO + "Anvil.sfz", dict(veltrack=88, max_release=2.0), 60),
}

# the strokes on the unpitched instruments' keys
BASS_DRUM_HIT, BASS_DRUM_ROLL = 62, 63
SNARE_HIT, SNARE_RIM = 62, 60
TOM_HIT, TOM_ROLL = 62, 63
FRAME_LOW, FRAME_HIGH, FRAME_LOW_MUTED = 61, 64, 62
GONG_HIT = 60
CYMBAL_CRASH = 60
SUS_SWELL_2S, SUS_SWELL_4S, SUS_HIT, SUS_ROLL = 65, 66, 68, 71
TAMB_HIT, TAMB_SHAKE = 60, 62

SYNTHS = {
    "choir":    lambda: synth.Choir("ah", voices=6, gain=0.0, name="choir"),
    "choir_oo": lambda: synth.Choir("oo", voices=6, gain=0.0, name="choir_oo"),
    "choir_oh": lambda: synth.Choir("oh", voices=6, gain=0.0, name="choir_oh"),
}

LEVEL_DB = -18.0      # the loudest 50 ms of the levelling note
_BUILT = {}


def _loudest(x, win):
    p = np.mean(x ** 2, axis=1)
    c = np.concatenate([[0.0], np.cumsum(p)])
    if len(p) <= win:
        return math.sqrt(c[-1] / max(1, len(p)))
    r = (c[win:] - c[:-win]) / win
    return math.sqrt(float(np.max(r)))


def get(name):
    """The instrument called `name`, built and levelled on first use."""
    inst = _BUILT.get(name)
    if inst is not None:
        return inst
    if name in SYNTHS:
        inst = SYNTHS[name]()
        x = inst.render(57, 90, 1.5)
    else:
        root = sampler.require_cache()
        rel, opts, key = SPECS[name]
        inst = sampler.Sampled(os.path.join(root, rel), name=name, **opts)
        if key is None:
            key = (inst.lo + inst.hi) // 2
        x = inst.render(key, 90, 1.0)
    level = _loudest(x, int(0.05 * sampler.RATE))
    inst.gain = getattr(inst, "gain", 0.0) + (LEVEL_DB - 20.0 * math.log10(max(level, 1e-9)))
    _BUILT[name] = inst
    return inst
