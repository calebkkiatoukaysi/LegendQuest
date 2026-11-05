#!/usr/bin/env python3
"""Compile the story's scripts (Story/04 to 08) into the game's C#: one DialogueScript per script.

Usage:  python tools/story/compile_scripts.py              writes Dialogue/Scripts/Generated/*.g.cs
        python tools/story/compile_scripts.py --check      writes nothing; exit 1 on any error, if a
                                                           generated file on disk isn't what it would
                                                           write, or if a generated script's id is
                                                           also a hand-written one in Dialogue/Scripts
        python tools/story/compile_scripts.py --diff ID..  the hand-written script and the generated
                                                           one side by side: header, flags, conditions,
                                                           effects, jumps and node count
Exit code 1 if any ERROR. A WARN is an id a table in the repo doesn't have yet (a key item, a meter):
the C# is right, but the game's start-up check will stop on it until the table has it.

Every block in 04 to 08 is parsed and checked, so a notation or header error anywhere fails. Only the
scripts EMIT_SCOPE names are written, and the checks that only matter for C# (speakers, enum members,
effects and conditions the model doesn't have yet) run on those alone. Nothing is written while an
emitted script has an error that leaves its C# wrong. A speaker who isn't in Cast or Origins is an
error too, but its C# is right either way, so the files are still written.

The rules (build plan §4.1 and §5.3, the notation table in 04, and the M1 team contract):
  blocks      a ``` block under a "#### script <id>", "#### banter <id>" or "#### enc-<id>" header;
              ```text blocks are examples, skipped. Two blocks under one header are one script.
  headers     fields after " · ": with `x`, needs `a`, `!b` (a ! goes to Unless), unless `a`,
              marker quest|event|none, map `x`, as-built (the script is skipped: it's in the game).
              Anything else, and words after a with or map's id, is a trigger note for the
              registry's Triggers (Story/tools/build_registry.py reads them the same way). needs and unless hold
              backticked flags and commas, nothing else; prose in them is an error.
  lines       a node, unless it starts with ! (cues for the next node), ? (the node above asks),
              "1." (an option) or — (a reading aid, ignored)
  labels      #label first on a node line is its Id; on a cue line, the Id of the node the cues are for
  conditions  {if a, b} after the label, all of them. a | b is any of them. x is a flag, or a key item
              if Story/KeyItems.cs has it; !x, with:x, !with:x, voice:x, class:x, act:n, m>=n, m>n,
              m<n, m<=n, and a>b or a>=b between two meters (a<b is written b>a). knows:x and the
              other negated prefixes come later, so they're errors in an emitted script.
  speakers    upper-case words up to the first ( [ or :. YOU, SB, NARRATOR, CARD, CHAPTER, VOICE(x),
              ECHO-X, or a cast or origin id in capitals (RED GULL is red-gull)
  nodes       SPEAKER (expr) [direction]: text  ~ effects  → jump   [CALLBACK x]. expr is one of the
              twelve faces; the callback is stripped; → end ends the script
  effects     ~ a · b: set f, clear f, m +n, m -n, m =n, gold ±n, give x, take x, join x, fight x,
              objective "…", travel map spawn, rest, voice v, shop x, swap a b, leave x, rejoin x
              (rejoin valencia also adds affinity-valencia +5 if mc-claimed-valencia). train, cp, xp,
              support-off, fight-add and fight-guest parse, but come with M4 and M5.
  options     "  1. ♥ [needs c] [locked c] [PERSUADE 13 · adv a, b] text  ~ effects  → jump". A check
              is [LABEL dc · adv …] or [CHECK skill dc · adv …], DC 12 if left out, and jumps
              "→ pass x · fail y". A question has two to four options.
  timers      ? timer 3 → x puts the question on a three-second clock that goes to x
  cues        ! verb words · verb words, the verbs of build plan §5.3, for the next node. An {if …}
              first guards that line's cues only. Cues between a line and its ? come up with the
              question, so they're that line's. Cues after the last node are the script's Closing.
  jumps       every → x, pass x, fail y and timer target names a label in the same script, or end
  legacy      Painting from a backdrop that's in Story/Paintings.cs to the next backdrop; Board from
              board on to board off; Sound from a node's first sfx cue that's a member of Sfx
"""
import argparse
import collections
import glob
import os
import re
import sys
import textwrap
from dataclasses import dataclass, field

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
STORY = os.path.join(ROOT, "Story")
SCRIPTS_DIR = os.path.join(ROOT, "Dialogue", "Scripts")
GENERATED_DIR = os.path.join(SCRIPTS_DIR, "Generated")
NAMESPACE = "LegendQuest.Dialogue.Scripts.Generated"
INDEX_CLASS = "GeneratedScripts"

# The script files, in order, with the class each compiles to and what the class is.
SOURCES = (
    ("04-script-prologue-act1.md", "StoryPrologueAct1", "The Prologue and Act 1"),
    ("05-script-act2.md", "StoryAct2", "Act 2"),
    ("06-script-act3.md", "StoryAct3", "Act 3"),
    ("07-companions.md", "StoryCompanions", "The companions' talks, quests and romances"),
    ("08-random-encounters.md", "StoryEncounters", "The random encounters' scripts"),
)

# What gets emitted as C#, by file: the heading its emitted scripts stop before, or None for the
# whole file. A file that isn't here is parsed and checked, and nothing in it is emitted. Only
# "#### script" blocks are emitted; banters and enc- entries get their own outputs later (build plan
# §6 and §7). Each milestone extends this (build plan §11): M6 drops 04's stop, M7 adds 05, M8 06,
# M9 07 and M10 08.
EMIT_SCOPE = {
    "04-script-prologue-act1.md": "## Chapter 5",   # M1: the Prologue and Chapters 1 to 4
}

EXPRESSIONS = ("neutral", "smile", "grin", "laugh", "smirk", "sad", "worried", "afraid", "angry", "cold", "tired", "shock")
CUE_VERBS = ("place", "walk", "face", "travel-actor", "anim", "hold", "drop", "light", "sigil", "show", "hide", "swap",
             "echo", "reflection", "trail", "backdrop", "tint", "camera", "fade", "shake", "slow", "sfx", "music", "wait",
             "board")
CHECK_ABILITIES = {"PERSUADE": "Charisma", "INTIMIDATE": "Charisma", "INSIGHT": "Wisdom", "STEALTH": "Dexterity",
                   "ATHLETICS": "Strength", "ARCANA": "Intelligence"}
USUAL_DC = 12             # DialogueCheck.Usual
USUAL_ABILITY = "Charisma"
HIGHEST_DC = 30           # DialogueScript's limit
FEWEST_OPTIONS, MOST_OPTIONS = 2, 4
END = "end"               # DialogueScript.End
LATER_EFFECTS = {         # effects the notation has and the model doesn't yet (build plan §3.4): milestone, form
    "train": ("M5", "train <trainer>"),
    "cp": ("M5", "cp +n"),
    "xp": ("M4", "xp +n"),
    "support-off": ("M4", "support-off <id>"),
    "fight-add": ("M4", "fight-add <enemy> <n>"),
    "fight-guest": ("M4", "fight-guest <id>"),
}
REJOIN_BUMPS = {"valencia": ("affinity-valencia", 5, "mc-claimed-valencia")}   # build plan §3.2
ENC_FIELD = re.compile(r"^(?:fight|event|merchant|companion|rare|once|(?:regions|acts|party|time|approach) .+|(?:weight|cooldown) \d+)$")
LINE_LIMIT = 160          # longest generated line that's packed; longer strings get a line to themselves

ID = re.compile(r"^[a-z0-9][a-z0-9_-]*$")
HEADER = re.compile(r"^#### (?:(?P<kind>script|banter) (?P<id>[a-z][\w-]*)|(?P<enc>enc-[\w-]+))(?P<rest>.*)$")
SPEAKER = re.compile(r"^(?P<who>VOICE\([^)]*\)|[A-Z][A-Z0-9 '\-]*?)\s*(?:\((?P<expr>[^)]*)\))?\s*(?:\[(?P<dir>[^\]]*)\])?\s*:\s+(?P<text>\S.*)$")
CALLBACK = re.compile(r"\s*\[CALLBACK [\w-]+\]")
COMPARE = re.compile(r"^([a-z][\w-]*)\s*(>=|<=|>|<)\s*([a-z][\w-]*|\d+)$")
CHECK_TAG = re.compile(r"^(?:CHECK\s+(?P<skill>[a-z]+)|(?P<label>[A-Z]+))(?:\s+(?P<dc>\d+))?(?:\s*·\s*adv\s+(?P<adv>.+))?$")
FLAG_LIST = re.compile(r"^`[^`]+`(?:\s*,\s*`[^`]+`)*$")

# Where a note is checked: NOTATION everywhere; EMIT and LOOKUP only in emitted scripts. EMIT leaves
# the C# wrong, so nothing is written; LOOKUP (a speaker no table has) doesn't. WARN is for a human.
NOTATION, EMIT, LOOKUP, WARN = "notation", "emit", "lookup", "warn"


class Fatal(Exception):
    """The repo isn't what the compiler reads: a table it can't find, or a scope heading that's gone."""


# ---------------------------------------------------------------------------------------------
# The repo's tables, read by regex.

def _read(rel):
    path = os.path.join(ROOT, rel)
    if not os.path.exists(path):
        raise Fatal(f"{rel} isn't there; the compiler reads it")
    with open(path, encoding="utf-8-sig") as f:
        return f.read()


def _ids(rel, pattern, what):
    found = set(re.findall(pattern, _read(rel), re.M))
    if not found:
        raise Fatal(f"found no {what} in {rel}; has its format changed?")
    return found


def _enum(rel, name):
    m = re.search(r"\benum " + name + r"\s*\{(.*?)\}", _read(rel), re.S)
    if not m:
        raise Fatal(f"no enum {name} in {rel}")
    body = re.sub(r"//[^\n]*|/\*.*?\*/|\[[^\]]*\]", "", m.group(1), flags=re.S)
    members = {re.match(r"\s*(\w+)", part).group(1) for part in body.split(",") if part.strip()}
    if not members:
        raise Fatal(f"enum {name} in {rel} has no members")
    return members


def _consts(text):
    return dict(re.findall(r'const string (\w+) = "([^"]*)";', text))


class Tables:
    """The ids the scripts are checked against."""

    def __init__(self):
        self.cast = _ids("Story/Cast.cs", r'^\s*new\("([a-z0-9][\w-]*)",', "cast members")
        origins = _read("Characters/Origins.cs")
        consts = _consts(origins)
        self.origins = set(re.findall(r'\bId = "([a-z0-9][\w-]*)"', origins))
        self.origins |= {consts[c] for c in re.findall(r"\bId = (\w+)\s*,", origins) if c in consts}
        if not self.origins:
            raise Fatal("found no origins in Characters/Origins.cs")
        self.key_items = _ids("Story/KeyItems.cs", r'^\s*new\("([a-z0-9][\w-]*)",', "key items")
        self.paintings = _ids("Story/Paintings.cs", r'^\s*\["([a-z0-9][\w-]*)"\]\s*=', "paintings")
        meters = _read("Story/Meters.cs")
        consts = _consts(meters)
        rows = re.findall(r'^\s*new\((\w+|"[\w-]+"),', meters, re.M)
        self.meters = {r.strip('"') if r.startswith('"') else consts.get(r, r) for r in rows}
        if not self.meters:
            raise Fatal("found no meters in Story/Meters.cs")
        self.sfx = _enum("Enums/Sfx.cs", "Sfx")
        self.class_ids = _enum("Enums/ClassId.cs", "ClassId")
        self.voices = _enum("Enums/Voice.cs", "Voice") - {"None"}
        self.abilities = _enum("Enums/Ability.cs", "Ability")
        for ability in set(CHECK_ABILITIES.values()) | {USUAL_ABILITY}:
            if ability not in self.abilities:
                raise Fatal(f"Ability has no {ability}, which a check needs")


# ---------------------------------------------------------------------------------------------
# The model the parser builds.

@dataclass
class Note:
    line: int
    text: str
    stage: str


@dataclass
class Option:
    line: int
    text: str = ""
    flirt: bool = False
    conditions: list = field(default_factory=list)
    locked: bool = False
    effects: list = field(default_factory=list)
    check: dict = None
    on_success: str = None
    on_failure: str = None
    next: str = None


@dataclass
class Node:
    line: int
    id: str = None
    speaker: str = None
    expression: str = None
    direction: str = None
    text: str = ""
    conditions: list = field(default_factory=list)
    effects: list = field(default_factory=list)
    next: str = None
    question: bool = False
    options: list = field(default_factory=list)
    seconds: str = None
    timeout: str = None
    cues: list = field(default_factory=list)
    painting: str = None
    board: bool = False
    sound: str = None


@dataclass
class Script:
    source: str
    kind: str
    id: str
    line: int
    summary: str
    in_scope: bool
    as_built: bool = False
    with_: str = None
    needs: list = field(default_factory=list)
    unless: list = field(default_factory=list)
    marker: str = None
    map: str = None
    triggers: list = field(default_factory=list)
    blocks: int = 0
    lines: list = field(default_factory=list)
    nodes: list = field(default_factory=list)
    closing: list = field(default_factory=list)
    notes: list = field(default_factory=list)

    @property
    def emitted(self):
        return self.in_scope and self.kind == "script" and not self.as_built

    def note(self, line, text, stage=NOTATION):
        self.notes.append(Note(line, text, stage))


# ---------------------------------------------------------------------------------------------
# Small helpers.

def cs_str(text):
    """A C# regular string literal."""
    text = text.replace("\\", "\\\\").replace('"', '\\"').replace("\t", "\\t").replace("\r", "\\r").replace("\n", "\\n")
    return f'"{text}"'


def pascal(kebab):
    return "".join(part[:1].upper() + part[1:] for part in kebab.split("-"))


def field_name(script_id):
    """a1-c1-01-gulls-after is A1_C1_01_Gulls_After."""
    return "_".join(part[:1].upper() + part[1:] for part in script_id.split("-"))


def xml(text):
    return text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def split_dots(text):
    """Splits on · outside double quotes, so an objective's text can hold one."""
    parts, cur, quoted = [], [], False
    for ch in text:
        if ch == '"':
            quoted = not quoted
        if ch == "·" and not quoted:
            parts.append("".join(cur))
            cur = []
        else:
            cur.append(ch)
    parts.append("".join(cur))
    return parts


def jump_cs(label):
    return "DialogueScript.End" if label == END else cs_str(label)


# ---------------------------------------------------------------------------------------------
# Conditions, effects and cues.

def parse_conditions(text, s, n, tables):
    """{if a, b | c} as C#: one condition per comma, a | in a part making it an AnyOf."""
    out = []
    for part in text.split(","):
        part = part.strip()
        if "|" in part:
            kids = [parse_atom(piece.strip(), s, n, tables) for piece in part.split("|")]
            if all(kids):
                out.append(f"DialogueCondition.AnyOf({', '.join(kids)})")
        else:
            atom = parse_atom(part, s, n, tables)
            if atom:
                out.append(atom)
    return out


def parse_atom(atom, s, n, tables):
    if not atom:
        s.note(n, "an empty condition (a stray , or |)")
        return None
    negated = atom.startswith("!")
    body = atom[1:] if negated else atom
    m = re.match(r"^([a-z][\w-]*):(.*)$", body)
    if m:
        prefix, value = m.group(1), m.group(2).strip()
        if prefix not in ("with", "voice", "class", "act", "knows"):
            s.note(n, f"`{atom}`: {prefix}: isn't a condition prefix (with:, voice:, class:, act:, knows:)")
            return None
        if not value:
            s.note(n, f"`{atom}` names nothing after {prefix}:")
            return None
        if prefix == "with":
            if not ID.match(value):
                s.note(n, f"`{atom}`: with: takes an origin id")
                return None
            return f"DialogueCondition.{'Without' if negated else 'With'}({cs_str(value)})"
        if prefix == "act" and not value.isdigit():
            s.note(n, f"`{atom}`: act: takes a number")
            return None
        if negated:
            s.note(n, f"`{atom}`: {prefix}: has no negated form", EMIT)
            return None
        if prefix == "knows":
            s.note(n, f"`{atom}`: knows: comes with M5 (build plan §3.4)", EMIT)
            return None
        if prefix == "act":
            return f"DialogueCondition.InAct({int(value)})"
        member = pascal(value)
        if prefix == "voice":
            if member not in tables.voices:
                s.note(n, f"`{atom}`: Voice has no {member}", EMIT)
                return None
            return f"DialogueCondition.Speaks(Voice.{member})"
        if member not in tables.class_ids:
            s.note(n, f"`{atom}`: ClassId has no {member}", EMIT)
            return None
        return f"DialogueCondition.IsClass(ClassId.{member})"
    m = COMPARE.match(body)
    if m:
        meter, op, rhs = m.groups()
        if negated:
            s.note(n, f"`{atom}`: a comparison has no negated form; turn it round")
            return None
        if rhs.isdigit():
            amount = int(rhs)
            return {">=": f"DialogueCondition.AtLeast({cs_str(meter)}, {amount})",
                    ">": f"DialogueCondition.AtLeast({cs_str(meter)}, {amount + 1})",
                    "<": f"DialogueCondition.Below({cs_str(meter)}, {amount})",
                    "<=": f"DialogueCondition.Below({cs_str(meter)}, {amount + 1})"}[op]
        if op in ("<", "<="):   # Compare reads > and >=, so a<b is written b>a
            meter, op, rhs = rhs, op.replace("<", ">"), meter
        return f"DialogueCondition.Compare({cs_str(meter + op + rhs)})"
    if re.search(r"[<>=:]", body) or not ID.match(body):
        s.note(n, f"`{atom}` isn't a flag, a key item or a condition the compiler reads")
        return None
    if body in tables.key_items:
        return f"DialogueCondition.{'Lacks' if negated else 'Has'}({cs_str(body)})"
    return f"DialogueCondition.{'Not' if negated else 'Is'}({cs_str(body)})"


def parse_effects(text, s, n, tables):
    out = []
    if "→" in text:
        s.note(n, "a → among the effects: it needs two spaces before it")
        return out
    for effect in split_dots(text):
        effect = effect.strip()
        if not effect:
            s.note(n, "an empty effect (a stray ·)")
            continue
        out += parse_effect(effect, s, n, tables)
    return out


def parse_effect(e, s, n, tables):
    words = e.split()
    verb = words[0]

    def takes(count, what):
        if len(words) == count + 1 and all(ID.match(w) for w in words[1:]):
            return True
        s.note(n, f"`{e}`: {verb} takes {what}")
        return False

    if verb == "objective":
        m = re.match(r'^objective\s+"([^"]+)"$', e)
        if not m:
            s.note(n, f"`{e}`: objective takes its text in straight double quotes")
            return []
        return [f"DialogueEffect.Objective({cs_str(m.group(1))})"]
    one_id = {"set": "Set", "clear": "Clear", "give": "Give", "take": "Take", "join": "Recruit", "fight": "Fight",
              "shop": "Shop", "leave": "Leave", "rejoin": "Rejoin"}
    if verb in one_id:
        if not takes(1, "one id"):
            return []
        if verb in ("give", "take") and words[1] not in tables.key_items:
            s.note(n, f"`{e}`: {words[1]} isn't in Story/KeyItems.cs yet", WARN)
        out = [f"DialogueEffect.{one_id[verb]}({cs_str(words[1])})"]
        if verb == "rejoin" and words[1] in REJOIN_BUMPS:
            meter, amount, flag = REJOIN_BUMPS[words[1]]
            out.append(f"DialogueEffect.Add({cs_str(meter)}, {amount}).If(DialogueCondition.Is({cs_str(flag)}))")
        return out
    if verb == "travel":
        return [f"DialogueEffect.Travel({cs_str(words[1])}, {cs_str(words[2])})"] if takes(2, "a map and a spawn") else []
    if verb == "swap":
        return [f"DialogueEffect.Swap({cs_str(words[1])}, {cs_str(words[2])})"] if takes(2, "two ids") else []
    if verb == "rest":
        return ["DialogueEffect.Rest()"] if takes(0, "nothing after it") else []
    if verb == "voice":
        if not takes(1, "a voice"):
            return []
        if pascal(words[1]) not in tables.voices:
            s.note(n, f"`{e}`: Voice has no {pascal(words[1])}", EMIT)
            return []
        return [f"DialogueEffect.Speak(Voice.{pascal(words[1])})"]
    if verb in LATER_EFFECTS:
        milestone, form = LATER_EFFECTS[verb]
        pattern = re.escape(form).replace(r"<n>", r"\d+").replace(r"\+n", r"[+-]\d+")
        pattern = re.sub(r"<[a-z]+>", r"[a-z][\\w-]*", pattern)
        if not re.match("^" + pattern + "$", e):
            s.note(n, f"`{e}` isn't {form}")
        else:
            s.note(n, f"`{e}`: {verb} comes with {milestone} (build plan §3.4)", EMIT)
        return []
    m = re.match(r"^([a-z][\w-]*) ([+-]\d+|=\d+)$", e)
    if m:
        name, amount = m.groups()
        if name == "gold":
            if amount.startswith("="):
                s.note(n, f"`{e}`: gold takes +n or -n")
                return []
            return [f"DialogueEffect.Gold({int(amount)})"]
        if name not in tables.meters:
            s.note(n, f"`{e}`: {name} isn't in Story/Meters.cs yet", WARN)
        if amount.startswith("="):
            return [f"DialogueEffect.SetMeter({cs_str(name)}, {int(amount[1:])})"]
        return [f"DialogueEffect.Add({cs_str(name)}, {int(amount)})"]
    s.note(n, f"`{e}` isn't an effect the compiler knows")
    return []


def parse_cues(text, conditions, s, n):
    """One line's cues as (verb, args, C#), each guarded by the line's {if …}."""
    out = []
    when = f".When({', '.join(conditions)})" if conditions else ""
    for cue in split_dots(text):
        words = cue.split()
        if not words:
            s.note(n, "an empty cue (a stray ·)")
            continue
        verb, args = words[0], []
        for word in words[1:]:
            if "×" in word:
                if not re.match(r"^×\d+$", word):
                    s.note(n, f"`{cue.strip()}`: write a count as ×n after a space")
                word = word.replace("×", "x")
            args.append(word)
        if verb not in CUE_VERBS:
            s.note(n, f"`{cue.strip()}`: {verb} isn't a cue verb (build plan §5.3)")
            continue
        problem = cue_shape(verb, args)
        if problem:
            s.note(n, f"`{cue.strip()}`: {problem}")
            continue
        out.append((verb, args, f"StageCue.{pascal(verb)}({', '.join(cs_str(a) for a in args)}){when}"))
    return out


def cue_shape(verb, args):
    """What's wrong with a cue's words, for the verbs whose forms §5.3 fixes, or None."""
    number = re.compile(r"^\d+(\.\d+)?$")
    if not args:
        return "nothing after the verb"
    if verb == "board" and args not in (["on"], ["off"]):
        return "board takes on or off"
    if verb in ("backdrop", "tint", "sfx", "music") and len(args) != 1:
        return f"{verb} takes one id"
    if verb in ("wait", "shake", "slow") and (len(args) != 1 or not number.match(args[0])):
        return f"{verb} takes one number"
    if verb == "fade" and (len(args) != 2 or args[0] not in ("in", "out") or not number.match(args[1])):
        return "fade takes in or out, then seconds"
    return None


# ---------------------------------------------------------------------------------------------
# The parser.

def split_tail(text, s, n):
    """A line's text, its effects and its jump: "text  ~ effects  → jump", callbacks stripped."""
    text = CALLBACK.sub("", text)
    jump = effects = None
    m = re.search(r"\s{2,}→\s*(.*)$", text)
    if m:
        jump, text = m.group(1).strip(), text[:m.start()]
    m = re.search(r"\s{2,}~\s+(.*)$", text)
    if m:
        effects, text = m.group(1).strip(), text[:m.start()]
    text = text.strip()
    if "→" in text or re.search(r"(?:^|\s)~(?:\s|$)", text):
        s.note(n, "a ~ or → in the text: effects and jumps need two spaces before them")
    return text, effects, jump


def speaker_of(who, s, n, tables):
    fixed = {"YOU": "Origins.You", "SB": "Origins.SoulBound", "NARRATOR": "Speakers.Narrator",
             "CARD": "Speakers.Card", "CHAPTER": "Speakers.Chapter"}
    if who in fixed:
        return fixed[who]
    m = re.match(r"^VOICE\((.+)\)$", who)
    if m:
        whose = m.group(1).strip().lower()
        ident = {"sb": "soulbound", "you": "you"}.get(whose, whose.replace(" ", "-"))
        if ident not in tables.origins and ident not in tables.cast:
            s.note(n, f"VOICE({m.group(1)}) is voice:{ident}, and {ident} isn't in Story/Cast.cs or Characters/Origins.cs", LOOKUP)
        return cs_str("voice:" + ident)
    ident = who.lower().replace(" ", "-")
    if not ID.match(ident):
        s.note(n, f"{who} can't be a speaker id")
    elif ident not in tables.origins and ident not in tables.cast:
        s.note(n, f"{who} is {ident}, who isn't in Story/Cast.cs or Characters/Origins.cs", LOOKUP)
    return cs_str(ident)


class Fill:
    """The legacy fields worked out from the cues as the script goes: the painting and the board."""
    painting = None
    board = False


def take_cues(node, cues, fill, tables):
    """Gives a node its cues, and the Painting, Board and Sound they work out to."""
    for verb, args, call in cues:
        if verb == "backdrop":
            fill.painting = args[0] if args[0] != "off" and args[0] in tables.paintings else None
        elif verb == "board":
            fill.board = args[0] == "on"
        elif verb == "sfx" and node.sound is None and pascal(args[0]) in tables.sfx:
            node.sound = "Sfx." + pascal(args[0])
        node.cues.append(call)
    node.painting, node.board = fill.painting, fill.board


def parse_script(s, tables):
    nodes, pending, labels, jumps = [], [], {}, []
    pending_label = None      # (label, line) from a cue line, for the next node
    question = None           # the node whose options are being read
    expect = 1                # the number the next option should have
    fill = Fill()

    def label_node(node, label, n):
        if label == END:
            s.note(n, f"a label can't be called {END}")
        elif label in labels:
            s.note(n, f"#{label} labels two lines (the first at line {labels[label]})")
        else:
            labels[label] = n
            node.id = label

    for n, raw in s.lines:
        t = raw.strip()
        if not t or t.startswith("—"):
            continue
        if t[0] in "~→":
            s.note(n, "effects or a jump with no speaker: they go at the end of a line")
            continue
        label, rest = None, t
        if t.startswith("#"):
            m = re.match(r"^#([\w-]+)\s+(\S.*)$", t)
            if not m:
                s.note(n, "a #label with no line after it")
                continue
            label, rest = m.group(1), m.group(2)
        conditions_text = None
        m = re.match(r"^\{if\s+([^}]*)\}\s*(.*)$", rest)
        if m:
            conditions_text, rest = m.group(1), m.group(2)
        elif rest.startswith("{"):
            s.note(n, "a { at the start of a line that isn't {if …}")
            continue
        option = re.match(r"^(\d+)\.\s+(.*)$", rest)
        if not option:
            question = None

        if rest.startswith("!"):
            body = rest[1:].strip()
            if "→" in body or re.search(r"(?:^|\s)~(?:\s|$)", body):
                s.note(n, "a cue line can't carry effects or a jump: put them on the line it's for")
                continue
            conditions = parse_conditions(conditions_text, s, n, tables) if conditions_text else []
            pending += parse_cues(body, conditions, s, n)
            if label:
                if pending_label:
                    s.note(n, f"two labels for one line: #{pending_label[0]} and #{label}")
                else:
                    pending_label = (label, n)
            continue

        if rest.startswith("?"):
            if label:
                s.note(n, "a label on a ?: label the line above it")
            if conditions_text is not None:
                s.note(n, "an {if …} on a ?: put it on the line above it")
            m = re.match(r"^\?(?:\s+timer\s+(\d+(?:\.\d+)?)\s+→\s*([\w-]+))?$", rest)
            if not m:
                s.note(n, "a ? line that isn't ? or ? timer n → label")
                continue
            target = nodes[-1] if nodes else None
            if target is None:
                s.note(n, "a ? with no line above it")
                continue
            if target.question:
                s.note(n, "a second ? under one line")
                continue
            if target.next:
                s.note(n, "the line above has a → and options: the options say where it goes")
            if pending_label:
                s.note(pending_label[1], f"#{pending_label[0]} labels cues right before a ?: label the line above instead")
                pending_label = None
            take_cues(target, pending, fill, tables)   # cues between a line and its ? come up with the question
            pending = []
            target.question = True
            if m.group(1):
                target.seconds, target.timeout = m.group(1), m.group(2)
                jumps.append((n, m.group(2)))
            question, expect = target, 1
            continue

        if option:
            if label or conditions_text is not None:
                s.note(n, "a label or {if …} on an option: give it [needs …] instead")
            if question is None:
                s.note(n, "an option with no ? above it")
                continue
            number = int(option.group(1))
            if number != expect:
                s.note(n, f"option {number} where {expect} should be")
            expect = number + 1
            question.options.append(parse_option(option.group(2), s, n, tables, jumps))
            continue

        sm = SPEAKER.match(rest)
        if not sm:
            s.note(n, "a line the compiler can't read: SPEAKER (expr) [direction]: text")
            continue
        node = Node(line=n, speaker=speaker_of(sm.group("who").strip(), s, n, tables))
        expression = sm.group("expr")
        if expression is not None:
            if expression.strip() in EXPRESSIONS:
                node.expression = expression.strip()
            else:
                s.note(n, f"({expression}) isn't one of the twelve expressions: {', '.join(EXPRESSIONS)}")
        if sm.group("dir") is not None:
            node.direction = sm.group("dir").strip()
            if not node.direction:
                s.note(n, "an empty [direction]")
        node.text, effects, jump = split_tail(sm.group("text"), s, n)
        if not node.text:
            s.note(n, "a line that says nothing")
        if conditions_text is not None:
            node.conditions = parse_conditions(conditions_text, s, n, tables)
        if effects:
            node.effects = parse_effects(effects, s, n, tables)
        if jump:
            if re.match(r"^[\w-]+$", jump):
                node.next = jump
                jumps.append((n, jump))
            else:
                s.note(n, f"→ {jump}: a line jumps to one label (pass and fail are for checks)")
        if label and pending_label:
            s.note(n, f"two labels for one line: #{pending_label[0]} and #{label}")
        if label or pending_label:
            label_node(node, label or pending_label[0], n)
        pending_label = None
        take_cues(node, pending, fill, tables)
        pending = []
        nodes.append(node)

    if pending_label:
        s.note(pending_label[1], f"#{pending_label[0]} labels cues with no line after them")
    s.closing = [call for _, _, call in pending]
    s.nodes = nodes
    if not nodes:
        s.note(s.line, "the script has no lines")
    for node in nodes:
        if node.question and not FEWEST_OPTIONS <= len(node.options) <= MOST_OPTIONS:
            count = len(node.options)
            s.note(node.line, f"a question with {count} option{'' if count == 1 else 's'}; it takes {FEWEST_OPTIONS} to {MOST_OPTIONS}")
    for n, target in jumps:
        if target != END and target not in labels:
            s.note(n, f"→ {target}: there's no #{target} in {s.id}")


def parse_option(body, s, n, tables, jumps):
    option, rest = Option(line=n), body
    while True:
        if rest.startswith("♥"):
            option.flirt, rest = True, rest[1:].lstrip()
            continue
        m = re.match(r"^\[([^\]]*)\]\s*(.*)$", rest)
        if not m:
            break
        tag, rest = m.group(1).strip(), m.group(2)
        tm = re.match(r"^(needs|locked)\s+(.+)$", tag)
        cm = CHECK_TAG.match(tag)
        if tm:
            option.conditions += parse_conditions(tm.group(2), s, n, tables)
            option.locked = option.locked or tm.group(1) == "locked"
        elif cm:
            if option.check:
                s.note(n, "an option with two checks")
            option.check = parse_check(cm, s, n, tables)
        else:
            s.note(n, f"[{tag}] isn't [needs …], [locked …] or a check like [PERSUADE 13 · adv …]")
    option.text, effects, jump = split_tail(rest, s, n)
    if not option.text:
        s.note(n, "an option that says nothing")
    if effects:
        option.effects = parse_effects(effects, s, n, tables)
    if jump:
        pf = re.match(r"^pass\s+([\w-]+)\s*·\s*fail\s+([\w-]+)$", jump)
        if pf:
            if not option.check:
                s.note(n, "→ pass … · fail … on an option with no check")
            option.on_success, option.on_failure = pf.groups()
            jumps += [(n, pf.group(1)), (n, pf.group(2))]
        elif re.match(r"^[\w-]+$", jump):
            if option.check:
                s.note(n, "a check jumps → pass x · fail y")
            option.next = jump
            jumps.append((n, jump))
        else:
            s.note(n, f"→ {jump} isn't a label or pass x · fail y")
    elif option.check:
        s.note(n, "a check jumps → pass x · fail y")
    return option


def parse_check(m, s, n, tables):
    label = (m.group("skill") or m.group("label")).upper()
    ability = CHECK_ABILITIES.get(label)
    if ability is None:
        s.note(n, f"no ability for a {label} check (the known ones: {', '.join(CHECK_ABILITIES)})")
    dc = int(m.group("dc")) if m.group("dc") else USUAL_DC
    if not 1 <= dc <= HIGHEST_DC:
        s.note(n, f"a DC of {dc}; it's 1 to {HIGHEST_DC}")
    advantage = parse_conditions(m.group("adv"), s, n, tables) if m.group("adv") else []
    return {"label": label, "ability": ability or USUAL_ABILITY, "dc": dc, "advantage": advantage}


# ---------------------------------------------------------------------------------------------
# Headers and files.

def read_header(s, rest, n, tables):
    rest = rest.rstrip()
    if not rest:
        return
    if not rest.startswith(" · "):
        s.note(n, "header fields go after ' · '")
        return
    for f in rest[3:].split(" · "):
        f = f.strip()
        word = f.split()[0] if f else ""
        if not f:
            s.note(n, "an empty header field")
        elif f == "as-built":
            s.as_built = True
        elif word in ("with", "map"):
            m = re.match(r"^\w+ `([a-z0-9][\w-]*)`(.*)$", f)
            if not m:
                s.note(n, f"“{f}”: {word} takes a backticked id")
                continue
            if getattr(s, "with_" if word == "with" else "map"):
                s.note(n, f"two {word} fields")
            setattr(s, "with_" if word == "with" else "map", m.group(1))
            if m.group(2).strip():   # with `soulbound` at the bow rail: the place is a trigger note
                s.triggers.append(f)
        elif word in ("needs", "unless"):
            read_flags(s, word, f[len(word):].strip(), n, tables)
        elif word == "marker":
            m = re.match(r"^marker (quest|event|none)$", f)
            if not m:
                s.note(n, f"“{f}”: marker is quest, event or none")
            elif s.marker:
                s.note(n, "two marker fields")
            else:
                s.marker = m.group(1)
        elif s.kind == "enc" and ENC_FIELD.match(f):
            continue
        else:
            s.triggers.append(f)


def read_flags(s, word, value, n, tables):
    if not FLAG_LIST.match(value):
        s.note(n, f"{word} holds prose the compiler can't read: “{value}”. Write it as backticked flags and "
                  "commas, or gate it on a flag that code sets and say so in the header")
        return
    for token in re.findall(r"`([^`]+)`", value):
        if s.kind == "enc":   # an entry's needs are conditions (08 §2.8); checked here, emitted in M10
            parse_conditions(token, s, n, tables)
            continue
        negated = token.startswith("!")
        name = token[1:] if negated else token
        if not ID.match(name):
            s.note(n, f"{word} takes flags, and `{token}` isn't one")
        elif word == "unless" and negated:
            s.note(n, f"`{token}` in unless is a double negative: put {name} in needs")
        elif negated or word == "unless":
            s.unless.append(name)
        else:
            s.needs.append(name)


def parse_file(name, tables, file_notes):
    path = os.path.join(STORY, name)
    stop = EMIT_SCOPE.get(name)
    stop_seen = False
    scripts, cur, target, section, scene = [], None, None, None, None
    in_block = skip = False
    with open(path, encoding="utf-8") as f:
        lines = f.read().splitlines()
    for n, s in enumerate(lines, 1):
        if in_block:
            if s.startswith("```"):
                in_block = False
            elif not skip and target:
                target.lines.append((n, s))
            continue
        if s.startswith("```"):
            in_block, skip = True, s.strip() != "```"
            target = None if skip else cur
            if not skip:
                if cur is None:
                    file_notes.append((name, n, None, "a ``` block with no #### script, banter or enc- header above it"))
                else:
                    cur.blocks += 1
            continue
        if stop and (s == stop or s.startswith(stop + " ")):
            stop_seen = True
        if re.match(r"^#{1,3} ", s):
            cur = None
            if s.startswith("## "):
                section, scene = s[3:].strip(), None
            elif s.startswith("### "):
                scene = s[4:].strip()
            continue
        if s.startswith("#### "):
            m = HEADER.match(s)
            if not m:
                file_notes.append((name, n, None, "a #### header that isn't #### script, #### banter or #### enc-"))
                cur = None
                continue
            kind = "enc" if m.group("enc") else m.group("kind")
            sid = m.group("enc") or m.group("id")
            cur = Script(source=name, kind=kind, id=sid, line=n, summary=scene or section or sid,
                         in_scope=name in EMIT_SCOPE and not stop_seen)
            read_header(cur, m.group("rest"), n, tables)
            scripts.append(cur)
    if in_block:
        file_notes.append((name, len(lines), None, "a ``` block that's never closed"))
    if stop and not stop_seen:
        raise Fatal(f"EMIT_SCOPE stops {name} at “{stop}”, which isn't a heading in it")
    for sc in scripts:
        if sc.as_built:
            continue
        if sc.blocks == 0:
            sc.note(sc.line, "a header with no ``` block under it (mark it as-built if it's in the game already)")
        else:
            parse_script(sc, tables)
    return scripts


def compile_all(tables):
    """Every script in 04 to 08, in file order, and the notes that belong to no script."""
    scripts, file_notes, seen = [], [], {}
    for name, _, _ in SOURCES:
        if not os.path.exists(os.path.join(STORY, name)):
            raise Fatal(f"Story/{name} isn't there")
        for s in parse_file(name, tables, file_notes):
            if s.id in seen:
                first = seen[s.id]
                s.note(s.line, f"the id {s.id} is used twice (first at {first.source}:{first.line})")
            else:
                seen[s.id] = s
            if field_name(s.id) == "All":
                s.note(s.line, "the id all would be the field All, which each class has already")
            scripts.append(s)
    return scripts, file_notes


# ---------------------------------------------------------------------------------------------
# Writing the C#.

class Arr:
    """An array: inline as head { a, b } when it fits, else one element a line."""

    def __init__(self, head, items, block=False):
        self.head, self.items, self.block = head, items, block


class Obj:
    """An object initializer. props are (name, value, group); a new group starts a new line."""

    def __init__(self, head, props, block=False):
        self.head, self.props, self.block = head, props, block


def inline(value):
    if isinstance(value, str):
        return value
    if value.block:
        return None
    if isinstance(value, Arr):
        parts = [inline(item) for item in value.items]
        return None if None in parts else f"{value.head} {{ {', '.join(parts)} }}"
    parts = [(name, inline(v)) for name, v, _ in value.props]
    if any(p is None for _, p in parts):
        return None
    return f"{value.head} {{ {', '.join(f'{name} = {p}' for name, p in parts)} }}"


def emit_value(prefix, value, indent, out, suffix=","):
    text = inline(value)
    if text is not None and (isinstance(value, str) or len(indent + prefix + text + suffix) <= LINE_LIMIT):
        out.append(indent + prefix + text + suffix)
        return
    out.append(indent + prefix + value.head)
    out.append(indent + "{")
    if isinstance(value, Arr):
        for item in value.items:
            emit_value("", item, indent + "    ", out)
    else:
        emit_props(value.props, indent + "    ", out)
    out.append(indent + "}" + suffix)


def emit_props(props, indent, out):
    cur, cur_group = None, None
    for name, value, group in props:
        text = inline(value)
        piece = None if text is None else f"{name} = {text}"
        if piece and cur is not None and group == cur_group and len(indent + cur + ", " + piece + ",") <= LINE_LIMIT:
            cur += ", " + piece
            continue
        if cur is not None:
            out.append(indent + cur + ",")
            cur = None
        if piece and (isinstance(value, str) or len(indent + piece + ",") <= LINE_LIMIT):
            cur, cur_group = piece, group
        else:
            emit_value(f"{name} = ", value, indent, out)
    if cur is not None:
        out.append(indent + cur + ",")


def option_obj(o):
    props = [("Text", cs_str(o.text), 0)]
    if o.flirt:
        props.append(("Flirt", "true", 0))
    if o.conditions:
        props.append(("Conditions", Arr("new[]", o.conditions), 1))
    if o.locked:
        props.append(("Locked", "true", 1))
    if o.effects:
        props.append(("Effects", Arr("new[]", o.effects), 1))
    if o.check:
        check = [("Label", cs_str(o.check["label"]), 0)]
        if o.check["ability"] != USUAL_ABILITY:
            check.append(("Ability", "Ability." + o.check["ability"], 0))
        if o.check["dc"] != USUAL_DC:
            check.append(("Dc", str(o.check["dc"]), 0))
        if o.check["advantage"]:
            check.append(("Advantage", Arr("new[]", o.check["advantage"]), 1))
        props.append(("Check", Obj("new DialogueCheck", check), 2))
    if o.on_success:
        props += [("OnSuccess", jump_cs(o.on_success), 2), ("OnFailure", jump_cs(o.on_failure), 2)]
    if o.next:
        props.append(("Next", jump_cs(o.next), 1))
    return Obj("new()", props)


def node_obj(nd):
    props = []
    if nd.id:
        props.append(("Id", cs_str(nd.id), 0))
    props.append(("Speaker", nd.speaker, 0))
    if nd.expression:
        props.append(("Expression", cs_str(nd.expression), 0))
    if nd.direction:
        props.append(("Direction", cs_str(nd.direction), 0))
    props.append(("Line", cs_str(nd.text), 0))
    if nd.conditions:
        props.append(("Conditions", Arr("new[]", nd.conditions), 1))
    if nd.effects:
        props.append(("Effects", Arr("new[]", nd.effects), 1))
    if nd.next:
        props.append(("Next", jump_cs(nd.next), 1))
    if nd.seconds:
        props += [("Seconds", nd.seconds + "f", 1), ("Timeout", jump_cs(nd.timeout), 1)]
    if nd.cues:
        props.append(("Cues", Arr("new[]", nd.cues), 2))
    if nd.painting:
        props.append(("Painting", cs_str(nd.painting), 3))
    if nd.board:
        props.append(("Board", "true", 3))
    if nd.sound:
        props.append(("Sound", nd.sound, 3))
    if nd.options:
        props.append(("Options", Arr("new DialogueOption[]", [option_obj(o) for o in nd.options], block=True), 4))
    return Obj("new()", props, block=bool(nd.options))


def render_script(s):
    """The script's field, from its declaration to its };, at class indent."""
    props = [("Id", cs_str(s.id), 0)]
    if s.with_:
        props.append(("With", cs_str(s.with_), 0))
    if s.needs:
        props.append(("Needs", Arr("new[]", [cs_str(x) for x in s.needs]), 0))
    if s.unless:
        props.append(("Unless", Arr("new[]", [cs_str(x) for x in s.unless]), 0))
    if s.marker in ("quest", "event"):
        props.append(("Marker", "MarkerKind." + s.marker.title(), 0))
    props.append(("Nodes", Arr("new DialogueNode[]", [node_obj(nd) for nd in s.nodes], block=True), 1))
    if s.closing:
        props.append(("Closing", Arr("new[]", s.closing), 2))
    out = [f"    public static readonly DialogueScript {field_name(s.id)} = new()", "    {"]
    emit_props(props, "        ", out)
    out.append("    };")
    return out


def banner(source):
    return ["// <auto-generated>",
            f"// Generated by tools/story/compile_scripts.py from {source}. Don't edit it: fix the .md and run the compiler again.",
            "// </auto-generated>",
            ""]


def render_class(name, cls, what, scripts):
    stop = EMIT_SCOPE.get(name)
    out = banner(f"Story/{name}") + [
        "using System.Collections.Generic;",
        "using LegendQuest.Characters;",
        "using LegendQuest.Story;",
        "",
        f"namespace {NAMESPACE};",
        "",
        "/// <summary>",
        f"/// {what}, compiled from Story/{name}: a field for each script, in the file's order.",
        "/// </summary>",
    ]
    if stop:
        out.append(f"/// <remarks>So far only the scripts before its \"{xml(stop.lstrip('# '))}\" heading.</remarks>")
    out += [f"public static class {cls}", "{"]
    for s in scripts:
        out.append(f"    /// <summary>{xml(s.summary)}</summary>")
        out += render_script(s)
        out.append("")
    out.append("    /// <summary>Every script here, in the file's order.</summary>")
    out.append("    public static readonly IReadOnlyList<DialogueScript> All = new DialogueScript[]")
    out.append("    {")
    out += [f"        {field_name(s.id)}," for s in scripts]
    out += ["    };", "}"]
    return out


def render_index(classes):
    first, last = SOURCES[0][0][:2], SOURCES[-1][0][:2]
    return banner(f"Story/{first} to {last}") + [
        "using System.Collections.Generic;",
        "",
        f"namespace {NAMESPACE};",
        "",
        "/// <summary>",
        f"/// Every generated script: each class's All, one after the other, in the order of the files they come from, {first} to {last}.",
        "/// </summary>",
        f"public static class {INDEX_CLASS}",
        "{",
        "    /// <summary>Every generated script, in file order.</summary>",
        f"    public static readonly IReadOnlyList<DialogueScript> All = Join({', '.join(c + '.All' for c in classes)});",
        "",
        "    /// <summary>The lists one after the other.</summary>",
        "    private static DialogueScript[] Join(params IReadOnlyList<DialogueScript>[] lists)",
        "    {",
        "        var all = new List<DialogueScript>();",
        "        foreach (IReadOnlyList<DialogueScript> list in lists) all.AddRange(list);",
        "        return all.ToArray();",
        "    }",
        "}",
    ]


def outputs(scripts):
    """Every file the compiler writes, by path, as bytes: CRLF, UTF-8 without a BOM like the repo's."""
    files, classes = {}, []
    for name, cls, what in SOURCES:
        emitted = [s for s in scripts if s.source == name and s.emitted]
        if emitted:
            files[os.path.join(GENERATED_DIR, cls + ".g.cs")] = render_class(name, cls, what, emitted)
            classes.append(cls)
    files[os.path.join(GENERATED_DIR, INDEX_CLASS + ".g.cs")] = render_index(classes)
    return {path: ("\r\n".join(lines) + "\r\n").encode("utf-8") for path, lines in files.items()}


def stale_files(expected):
    return sorted(p for p in glob.glob(os.path.join(GENERATED_DIR, "*.g.cs")) if os.path.normcase(p) not in
                  {os.path.normcase(e) for e in expected})


# ---------------------------------------------------------------------------------------------
# Reading the C#: the hand-written scripts, and the generated ones, for --check and --diff.

def code_segments(text):
    """The text as (kind, piece) runs, kind being code, string (a string or char literal) or comment."""
    out, start, i = [], 0, 0
    while i < len(text):
        if text.startswith("//", i) or text.startswith("/*", i):
            close = "\n" if text[i + 1] == "/" else "*/"
            end = text.find(close, i + 2)
            end = len(text) if end < 0 else end + (0 if close == "\n" else 2)
            kind = "comment"
        elif text[i] == '"':
            verbatim = "@" in text[max(0, i - 2):i]
            end = i + 1
            while end < len(text):
                if text[end] == "\\" and not verbatim:
                    end += 2
                elif text[end] == '"' and verbatim and text.startswith('""', end):
                    end += 2
                elif text[end] == '"':
                    break
                else:
                    end += 1
            end, kind = end + 1, "string"
        elif text[i] == "'":
            end, kind = text.find("'", i + (3 if text[i + 1] == "\\" else 2)) + 1, "string"
        else:
            i += 1
            continue
        out += [("code", text[start:i]), (kind, text[i:end])]
        start = i = end
    out.append(("code", text[start:]))
    return out


def blank_literals(text):
    """The same text, the same length, with comments and the insides of literals turned to spaces."""
    def blank(kind, piece):
        if kind == "code":
            return piece
        if kind == "comment":
            return re.sub(r"[^\n]", " ", piece)
        return piece[0] + " " * (len(piece) - 2) + piece[-1]
    return "".join(blank(kind, piece) for kind, piece in code_segments(text))


def match_close(code, i):
    """Where the bracket at code[i] closes, in text from blank_literals."""
    opener = code[i]
    closer = {"{": "}", "(": ")", "[": "]"}[opener]
    depth = 0
    for j in range(i, len(code)):
        if code[j] == opener:
            depth += 1
        elif code[j] == closer:
            depth -= 1
            if depth == 0:
                return j
    raise ValueError(f"an unclosed {opener}")


def split_args(text):
    """A call's arguments, split on the commas outside brackets and literals."""
    code = blank_literals(text)
    parts, depth, start = [], 0, 0
    for j, c in enumerate(code):
        if c in "({[":
            depth += 1
        elif c in ")}]":
            depth -= 1
        elif c == "," and depth == 0:
            parts.append(text[start:j].strip())
            start = j + 1
    if text[start:].strip():
        parts.append(text[start:].strip())
    return parts


class CsConstants:
    """Every string and int const in the C# the scripts name, by class and name."""

    def __init__(self):
        self.values = {}
        paths = glob.glob(os.path.join(ROOT, "Dialogue", "**", "*.cs"), recursive=True)
        paths += glob.glob(os.path.join(ROOT, "Story", "*.cs")) + glob.glob(os.path.join(ROOT, "Characters", "*.cs"))
        for path in sorted(paths):
            with open(path, encoding="utf-8-sig") as f:
                text = f.read()
            m = re.search(r"\b(?:class|record|struct) (\w+)", blank_literals(text))
            if m:
                for name, value in re.findall(r'\bconst (?:string|int) (\w+) = ("(?:[^"\\]|\\.)*"|-?\d+)\s*;', text):
                    self.values[(m.group(1), name)] = value

    def resolve(self, token, cls):
        """A literal's value, or a const's (Meters.Bond, or a bare name in cls), or the token as written."""
        token = token.strip()
        if token.startswith('"'):
            return re.sub(r"\\(.)", r"\1", token[1:-1])
        if re.match(r"^-?\d+$", token):
            return token
        owner, _, name = token.rpartition(".")
        value = self.values.get((owner.rpartition(".")[2] if owner else cls, name))
        return token if value is None else self.resolve(value, cls)


def cs_scripts(path):
    """(class, field, body) for each public static readonly DialogueScript in a file, with the file's
    private node helpers and condition arrays written out where they're used, so a diff sees them."""
    with open(path, encoding="utf-8-sig") as f:
        text = f.read()
    code = blank_literals(text)
    m = re.search(r"\bclass (\w+)", code)
    cls = m.group(1) if m else os.path.splitext(os.path.basename(path))[0]
    helpers = {}
    for hm in re.finditer(r"private static DialogueNode(?:\[\])? (\w+)\(([^)]*)\)\s*(=>|\{)", code):
        params = [p.split()[-1] for p in hm.group(2).split(",") if p.strip()]
        if hm.group(3) == "{":   # a block body puts its arguments together (Join); it builds no lines of its own
            helpers[hm.group(1)] = (params, None)
            continue
        start = end = hm.end()
        depth = 0
        while code[end] != ";" or depth:
            if code[end] in "({[":
                depth += 1
            elif code[end] in ")}]":
                depth -= 1
            end += 1
        helpers[hm.group(1)] = (params, text[start:end])
    arrays = dict(re.findall(r"private static readonly DialogueCondition\[\] (\w+) = (\{[^;]*\});", text))
    out = []
    for dm in re.finditer(r"public static readonly DialogueScript (\w+) = new\(\)\s*\{", code):
        start = dm.end() - 1
        body = text[start:match_close(code, start) + 1]
        out.append((cls, dm.group(1), inline_helpers(body, helpers, arrays)))
    return out


def inline_helpers(body, helpers, arrays):
    """The body with each call to a helper replaced by the helper's own text, its parameters filled in."""
    for _ in range(10):   # helpers call helpers (Join(Maleficio(), …)); ten deep is far more than any file has
        code = blank_literals(body)
        found = None
        for name in helpers:
            found = re.search(r"(?<![\w.])" + name + r"\(", code)
            if found:
                break
        if not found:
            break
        params, helper = helpers[name]
        close = match_close(code, found.end() - 1)
        args = split_args(body[found.end():close])
        if helper is None:
            body = body[:found.start()] + ", ".join(args) + body[close + 1:]
            continue
        pieces = []
        for kind, piece in code_segments(helper):
            if kind == "code":
                for param, arg in zip(params, args):
                    piece = re.sub(r"\b" + param + r"\b", lambda _: arg, piece)
            pieces.append(piece)
        body = body[:found.start()] + "".join(pieces) + body[close + 1:]
    pieces = []
    for kind, piece in code_segments(body):
        if kind == "code":
            for name, value in arrays.items():
                piece = re.sub(r"(?<=[=,(] )" + name + r"\b(?!\s*\()", lambda _: "new[] " + value, piece)
        pieces.append(piece)
    return "".join(pieces)


def hand_written():
    """id → (path, class, field, body) for every hand-written script in Dialogue/Scripts, and the consts."""
    consts = CsConstants()
    found = {}
    for path in sorted(glob.glob(os.path.join(SCRIPTS_DIR, "*.cs"))):
        for cls, name, body in cs_scripts(path):
            m = re.search(r"(?<![\w.])Id = ", blank_literals(body))
            if m:
                value = re.match(r'"(?:[^"\\]|\\.)*"|[\w.]+', body[m.end():]).group(0)
                found[consts.resolve(value, cls)] = (path, cls, name, body)
    return found, consts


ROWS = ("With", "Needs", "Unless", "Marker", "Flags set", "Flags cleared", "Flags read", "Conditions read", "Meters",
        "Gold", "Items", "Fights", "Travel", "Objectives", "Recruits", "Rest", "Other effects", "Jump targets", "Nodes")

CONDITION_WORDS = {   # factory → (row, how it reads), the arguments resolved
    "Is": ("Flags read", lambda a: a[0]), "Not": ("Flags read", lambda a: "!" + a[0]),
    "Has": ("Conditions read", lambda a: "has " + a[0]), "Lacks": ("Conditions read", lambda a: "lacks " + a[0]),
    "AtLeast": ("Conditions read", lambda a: f"{a[0]}>={a[1]}"), "Below": ("Conditions read", lambda a: f"{a[0]}<{a[1]}"),
    "With": ("Conditions read", lambda a: "with:" + a[0]), "Without": ("Conditions read", lambda a: "!with:" + a[0]),
    "Speaks": ("Conditions read", lambda a: "voice:" + a[0].rpartition(".")[2].lower()),
    "IsClass": ("Conditions read", lambda a: "class:" + a[0].rpartition(".")[2].lower()),
    "InAct": ("Conditions read", lambda a: "act:" + a[0]), "Compare": ("Conditions read", lambda a: a[0]),
}
EFFECT_WORDS = {
    "Set": ("Flags set", lambda a: a[0]), "Clear": ("Flags cleared", lambda a: a[0]),
    "Add": ("Meters", lambda a: f"{a[0]} {signed(a[1])}"), "SetMeter": ("Meters", lambda a: f"{a[0]} ={a[1]}"),
    "Gold": ("Gold", lambda a: signed(a[0])), "Give": ("Items", lambda a: "give " + a[0]),
    "Take": ("Items", lambda a: "take " + a[0]), "Fight": ("Fights", lambda a: a[0]),
    "Travel": ("Travel", lambda a: " ".join(a)), "Objective": ("Objectives", lambda a: f"“{a[0]}”"),
    "Recruit": ("Recruits", lambda a: "join " + a[0]), "Leave": ("Recruits", lambda a: "leave " + a[0]),
    "Rejoin": ("Recruits", lambda a: "rejoin " + a[0]), "Rest": ("Rest", lambda a: "rest"),
    "Speak": ("Other effects", lambda a: "voice " + a[0].rpartition(".")[2].lower()),
    "Shop": ("Other effects", lambda a: "shop " + a[0]), "Swap": ("Other effects", lambda a: "swap " + " ".join(a)),
}


def signed(number):
    return number if number.startswith("-") else "+" + number


def summarise(body, cls, consts):
    """What a script does, as a Counter of plain words per row of ROWS, read from its C#."""
    rows = {row: collections.Counter() for row in ROWS}
    code = blank_literals(body)

    def value_at(i):
        return consts.resolve(re.match(r'"(?:[^"\\]|\\.)*"|[\w.]+', body[i:]).group(0), cls)

    def calls(owner):
        """(factory, where the call starts, where its arguments start, its closing bracket) for each owner.X(…)."""
        for m in re.finditer(r"(?<![\w.])" + owner + r"\.(\w+)\(", code):
            yield m.group(1), m.start(), m.end(), match_close(code, m.end() - 1)

    def condition(call):
        """(row, words) for one DialogueCondition call's text; an AnyOf reads as its children."""
        m = re.match(r"\s*DialogueCondition\.(\w+)\(", call)
        if not m:
            return "Conditions read", call.strip()
        inner = call[m.end():match_close(blank_literals(call), m.end() - 1)]
        if m.group(1) == "AnyOf":
            return "Conditions read", "any of: " + " | ".join(condition(kid)[1] for kid in split_args(inner))
        row, word = CONDITION_WORDS.get(m.group(1), ("Conditions read", lambda a: f"{m.group(1)}({', '.join(a)})"))
        return row, word([consts.resolve(a, cls) for a in split_args(inner)])

    m = re.search(r"(?<![\w.])With = ", code)
    if m:
        rows["With"][value_at(m.end())] += 1
    for row in ("Needs", "Unless"):
        m = re.search(r"(?<![\w.])" + row + r" = new\s*(?:string\s*)?\[\]\s*\{", code)
        if m:
            for token in split_args(body[m.end():match_close(code, m.end() - 1)]):
                rows[row][consts.resolve(token, cls)] += 1
    m = re.search(r"(?<![\w.])Marker = MarkerKind\.(\w+)", code)
    if m and m.group(1) != "None":
        rows["Marker"][m.group(1).lower()] += 1
    rows["Nodes"][str(len(re.findall(r"(?<![\w.])Speaker = ", code)))] += 1
    for m in re.finditer(r"(?<![\w.])(?:Next|OnSuccess|OnFailure|Timeout) = ", code):
        rows["Jump targets"][value_at(m.end())] += 1
    for name, _, start, close in calls("DialogueEffect"):
        args = [consts.resolve(a, cls) for a in split_args(body[start:close])]
        row, word = EFFECT_WORDS.get(name, ("Other effects", lambda a: f"{name}({', '.join(a)})"))
        rows[row][word(args)] += 1
    any_of = []
    for name, begin, _, close in calls("DialogueCondition"):
        if any(s < begin and close < c for s, c in any_of):
            continue   # one of an AnyOf's, read as part of it
        if name == "AnyOf":
            any_of.append((begin, close))
        row, word = condition(body[begin:close + 1])
        rows[row][word] += 1
    return rows


def show_diff(script_id, scripts, rendered, hand, consts):
    """Prints the hand-written script and the generated one side by side, and what changes."""
    gen = next((s for s in scripts if s.id == script_id), None)
    print(script_id)
    sides = []
    if script_id in hand:
        path, cls, name, body = hand[script_id]
        print(f"  hand-written  {cls}.{name}  ({os.path.relpath(path, ROOT)})")
        sides.append(summarise(body, cls, consts))
    else:
        print("  hand-written  none in Dialogue/Scripts")
        sides.append(None)
    if gen is None:
        print("  generated     no script by that id in Story/04 to 08")
        sides.append(None)
    elif gen.as_built:
        print(f"  generated     none: {gen.source}:{gen.line} is marked as-built")
        sides.append(None)
    else:
        cls = next(c for name, c, _ in SOURCES if name == gen.source)
        where = "" if gen.emitted else "; not emitted, it's outside EMIT_SCOPE"
        print(f"  generated     {cls}.{field_name(gen.id)}  (from {gen.source}:{gen.line}{where})")
        sides.append(summarise("\n".join(rendered.get(gen.id) or render_script(gen)), cls, consts))
    width = 46

    def cell(counter):
        if counter is None:
            return ["—"]
        words = [w if c == 1 else f"{w} ×{c}" for w, c in sorted(counter.items())]
        return textwrap.wrap(", ".join(words), width, break_long_words=False, break_on_hyphens=False) or ["·"]

    print(f"\n    {'':<16}{'HAND-WRITTEN':<{width + 2}}GENERATED")
    changes = []
    for row in ROWS:
        a, b = (None if side is None else side[row] for side in sides)
        differs = a is not None and b is not None and a != b
        left, right = cell(a), cell(b)
        for i in range(max(len(left), len(right))):
            mark = "*" if differs and i == 0 else " "
            text = f"  {mark} {row if i == 0 else '':<16}{left[i] if i < len(left) else '':<{width + 2}}"
            print((text + (right[i] if i < len(right) else "")).rstrip())
        if differs:
            changes.append((row, a, b))
    if changes:
        print("\n  * What the generated one changes:")
        for row, a, b in changes:
            if row == "Nodes":
                print(f"    Nodes: {next(iter(a))} → {next(iter(b))}")
                continue
            parts = [f"- {w}" for w in sorted((a - b).elements())] + [f"+ {w}" for w in sorted((b - a).elements())]
            print(textwrap.fill(f"{row}: " + "; ".join(parts), 110, initial_indent="    ", subsequent_indent="      ",
                                break_long_words=False, break_on_hyphens=False))
    elif None not in sides:
        print("\n  Nothing listed differs; the lines themselves may.")
    print()


# ---------------------------------------------------------------------------------------------

def report(scripts, file_notes):
    """Prints every note in file order; returns (errors, warnings, emit_blocked)."""
    errors = warnings = 0
    blocked = False
    order = {name: i for i, (name, _, _) in enumerate(SOURCES)}
    lines = []
    for name, n, sid, text in file_notes:
        lines.append((order[name], n, f"ERROR {name}:{n}: {text}"))
        errors += 1
    for s in scripts:
        for note in s.notes:
            if note.stage == NOTATION or (s.emitted and note.stage in (EMIT, LOOKUP)):
                lines.append((order[s.source], note.line, f"ERROR {s.source}:{note.line} {s.id}: {note.text}"))
                errors += 1
                blocked = blocked or (s.emitted and note.stage != LOOKUP)
            elif s.emitted and note.stage == WARN:
                lines.append((order[s.source], note.line, f"WARN  {s.source}:{note.line} {s.id}: {note.text}"))
                warnings += 1
    for _, _, text in sorted(lines, key=lambda x: (x[0], x[1])):
        print(text)
    return errors, warnings, blocked


def main(argv=None):
    parser = argparse.ArgumentParser(description="Compile Story/04 to 08 into Dialogue/Scripts/Generated.")
    mode = parser.add_mutually_exclusive_group()
    mode.add_argument("--check", action="store_true", help="write nothing; fail if anything's wrong or out of date")
    mode.add_argument("--diff", nargs="+", metavar="ID", help="compare a hand-written script with the generated one")
    args = parser.parse_args(argv)
    try:
        tables = Tables()
        scripts, file_notes = compile_all(tables)
    except Fatal as e:
        print(f"ERROR {e}")
        return 2
    rendered = {s.id: render_script(s) for s in scripts if s.emitted}
    if args.diff:
        hand, consts = hand_written()
        for script_id in args.diff:
            show_diff(script_id, scripts, rendered, hand, consts)
        return 0

    errors, warnings, blocked = report(scripts, file_notes)
    files = outputs(scripts)
    emitted = [s for s in scripts if s.emitted]
    rel = lambda p: os.path.relpath(p, ROOT).replace(os.sep, "/")
    if args.check:
        for path, data in files.items():
            if not os.path.exists(path):
                print(f"ERROR {rel(path)} is missing: run the compiler")
                errors += 1
            else:
                with open(path, "rb") as f:
                    if f.read() != data:
                        print(f"ERROR {rel(path)} isn't what the .md compiles to: run the compiler")
                        errors += 1
        for path in stale_files(files):
            print(f"ERROR {rel(path)} is left over from an earlier run: run the compiler")
            errors += 1
        hand, _ = hand_written()
        for s in emitted:
            if s.id in hand:
                path, cls, name, _ = hand[s.id]
                print(f"ERROR {s.source}:{s.line} {s.id}: also hand-written, {cls}.{name} in {rel(path)}; "
                      "delete that one, after --diff")
                errors += 1
        verdict = "up to date" if errors == 0 else "not passing"
        print(f"\n{errors} errors, {warnings} warnings; {len(scripts)} scripts read, {len(emitted)} emitted; {verdict}")
        return 1 if errors else 0

    if blocked:
        print(f"\n{errors} errors, {warnings} warnings; nothing written: an emitted script has an error")
        return 1
    os.makedirs(GENERATED_DIR, exist_ok=True)
    written = []
    for path, data in files.items():
        current = None
        if os.path.exists(path):
            with open(path, "rb") as f:
                current = f.read()
        if current != data:
            with open(path, "wb") as f:
                f.write(data)
        written.append(rel(path))
    for path in stale_files(files):
        os.remove(path)
        print(f"removed {rel(path)}")
    print(f"\n{errors} errors, {warnings} warnings; {len(scripts)} scripts read, {len(emitted)} emitted to "
          + ", ".join(written))
    return 1 if errors else 0


if __name__ == "__main__":
    # Windows consoles default to cp1252, which has no arrow or middle dot; the lines quoted back need both
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.exit(main())
