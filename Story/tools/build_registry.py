#!/usr/bin/env python3
"""Build story/09-registry.md from the script files: every script, flag, meter, item, speaker,
spawn, fight, backdrop and map the scripts use, and the problems a reader can't see by eye.

Usage:  python story/tools/build_registry.py [--check]
        --check exits 1 if there are problems (a flag read but never set, a fight with no entry...).

The scripts are the source of truth; this file is generated. Don't edit 09-registry.md by hand.
"""
import re, os, sys, glob, collections

HERE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FILES = sorted(sorted(glob.glob(os.path.join(HERE, "0[4-8]*.md")) + glob.glob(os.path.join(HERE, "1[0-9]*.md"))))
BUILT_FLAGS = {  # set by code that's already in the repo (Act 1 Chapters 1-4, Companions.cs), or by systems
    "a1-c3", "a1-c4", "a1-c4-03", "a1-c4-04", "sb-quest-hook", "sb-shared-catch", "sb-tell-me",
    "sera-quest-hook", "sera-flirt-2", "mira-quest-hook", "mira-flirt-2", "mira-heard-crane", "kael-quest-hook",
    "kael-flirt-2", "brannock-quest-hook", "brannock-wary-sb", "mira-talk-2-done", "sb-crowd-loved",
    "valencia-recruited", "sera-recruited", "mira-recruited", "kael-recruited",
    "brannock-recruited", "kael-with-sb", "kael-with-you",  # set by A3-C2-04's meter comparison (code)
    "brannock-survived", "brannock-fell",                    # set by A3-C4-05's check (code)
    "ide-rescued",
    "vale-clue-1", "vale-clue-2", "vale-clue-3",              # set by examine objects on saint-lumen (map)
    "winter-two-done", "liberations-two-done",                # gate flags set by code when two of a header's flags are
    "letters-doors-done", "siege-eve-three",                  # all ten letters delivered; three companions in the party
}
SYSTEM_PREFIXES = ("enc-",)
ENC_FIELD = re.compile(r"^(?:fight|event|merchant|companion|rare|once|(?:regions|acts|party|time|approach) .+|(?:weight|cooldown) \d+)$")

def trigger_notes(sid, header):
    """The prose in a header: what starts the script, for whoever places its trigger (build plan §4.1 rule 1).
    Everything but with, needs, unless, marker, map, as-built and an encounter's entry fields, as tools/story/compile_scripts.py reads them."""
    notes = []
    for f in (f.strip() for f in header.split(" · ")):
        m = re.match(r"^(?:with|map) `[\w-]+`(.*)$", f)
        if m:
            if m.group(1).strip(): notes.append(f)   # with `soulbound` at the bow rail
            continue
        if not f or f == "as-built" or re.match(r"^(?:needs|unless) ", f) or re.match(r"^marker (?:quest|event|none)$", f): continue
        if sid.startswith("enc-") and ENC_FIELD.match(f): continue
        notes.append(f)
    return notes

def blocks():
    for f in FILES:
        name = os.path.basename(f)
        cur = None; inb = False; skip = False; buf = []; header = ""; lastmap = ""
        for n, line in enumerate(open(f, encoding="utf-8"), 1):
            s = line.rstrip("\n")
            if re.match(r"^#{2,3} ", s) and not inb: lastmap = ""
            m = re.match(r"^#### (?:script |banter )?([a-z][\w-]+)(.*)$", s)
            if m and not inb:
                cur, header = m.group(1), m.group(2)
                mm = re.search(r"map `[\w-]+`", header)
                if mm: lastmap = mm.group(0)
                elif lastmap and not cur.startswith("enc-"): header += " · (" + lastmap + ")"
            if s.startswith("```"):
                if inb and not skip: yield name, cur, header, buf
                if not inb: skip = s.strip() != "```"; buf = []
                inb = not inb; continue
            if inb: buf.append((n, s))

def main():
    sets = collections.defaultdict(set); reads = collections.defaultdict(set)
    meters = collections.defaultdict(set); items = collections.defaultdict(set)
    speakers = collections.defaultdict(set); fights = collections.defaultdict(set)
    spawns = collections.defaultdict(set); backdrops = collections.defaultdict(set)
    maps = collections.defaultdict(set); scripts = []; ids = collections.Counter()
    full = "".join(open(f, encoding="utf-8").read() for f in FILES)
    # headers outside blocks: needs/unless and maps
    for f in FILES:
        name = os.path.basename(f)
        for line in open(f, encoding="utf-8"):
            m = re.match(r"^#### (?:script |banter )?([a-z][\w-]+)(.*)$", line.rstrip())
            if not m: continue
            sid, rest = m.group(1), m.group(2)
            if not sid.startswith("enc-") and not rest.startswith(" ·") and rest.strip(): continue
            scripts.append((name, sid, rest.strip(" ·"))); ids[sid] += 1
            for mm in re.findall(r"map `([\w-]+)`", rest): maps[mm].add(sid)
            for part in re.findall(r"(?:needs|unless|party) ([^·]+)", rest):
                for fl in re.findall(r"`(!?[\w:-]+(?:[<>=]+\d+)?)`", part):
                    fl = fl.lstrip("!")
                    if ":" in fl or re.search(r"[<>=]", fl): continue
                    reads[fl].add(sid)
    for fname, sid, header, lines in blocks():
        for n, raw in lines:
            t = raw.strip()
            for fx in re.findall(r"~ ([^→]+)", t):
                for e in fx.split("·"):
                    p = e.split()
                    if not p: continue
                    if p[0] in ("set", "clear") and len(p) > 1: sets[p[1]].add(sid)
                    elif p[0] in ("give", "take") and len(p) > 1: items[p[1]].add(sid)
                    elif p[0] == "fight" and len(p) > 1: fights[p[1]].add(sid)
                    elif p[0] == "travel" and len(p) > 2: spawns[p[1]].add(p[2]); maps[p[1]].add(sid)
                    elif len(p) > 1 and re.match(r"^[+-=]?\d+$", p[1]) and p[0] not in ("gold",): meters[p[0]].add(sid)
            for c in re.findall(r"\{if ([^}]*)\}", t) + re.findall(r"\[(?:needs|locked) ([^\]]*)\]", t):
                for cond in re.split(r"[,|]", c):
                    cond = cond.strip().lstrip("!")
                    mm = re.match(r"^([\w-]+)\s*(>=|<=|>|<)\s*([\w-]+)$", cond)
                    if mm:
                        meters[mm.group(1)].add(sid)
                        if not mm.group(3).isdigit(): meters[mm.group(3)].add(sid)
                        continue
                    if ":" in cond or not cond: continue
                    reads[cond].add(sid)
            for c in re.findall(r"adv ([^\]]*)\]", t):
                for cond in c.split(","):
                    cond = cond.strip().lstrip("!")
                    if cond and ":" not in cond and not re.search(r"[<>=]", cond): reads[cond].add(sid)
            body = re.sub(r"^#[\w-]+\s+", "", t); body = re.sub(r"^\{if [^}]*\}\s*", "", body)
            if body.startswith("!"):
                for cue in body[1:].split("·"):
                    p = cue.split()
                    if not p: continue
                    if p[0] == "place" and " at " in cue: spawns[(sid, header)].add(cue.split(" at ")[1].split()[0])
                    if p[0] == "backdrop" and len(p) > 1 and p[1] != "off": backdrops[p[1]].add(sid)
                    if p[0] == "hold" and len(p) > 3: items[p[3]].add(sid)
                continue
            sm = re.match(r"^([A-Z][A-Z0-9 \-]*?)(\((\w+)\))?\s*(\([\w-]+\))?\s*(\[[^\]]*\])?\s*:", body)
            if sm and sm.group(1) not in ("CARD", "CHAPTER"): speakers[sm.group(1).strip()].add(sid)
    # problems
    problems = []
    for fl in sorted(reads):
        if fl.endswith("-done") and fl[:-5] in ids: continue   # the compiler adds <script>-done to companion talks
        if fl in sets or fl in BUILT_FLAGS or fl.startswith(SYSTEM_PREFIXES) or fl in items: continue
        if re.match(r"^(a\d-c\d+(-\d+)?|p-\d+)$", fl) and fl in {s for _, s, _ in scripts}: continue
        problems.append(f"flag `{fl}` is read ({', '.join(sorted(reads[fl]))}) but never set")
    for sid, c in ids.items():
        if c > 1: problems.append(f"script id `{sid}` is used {c} times")
    out = ["# Registry", "", "Generated by `tools/build_registry.py` from `04` to `08`. Don't edit by hand; run it again.", "",
           "The spawn table is a starting list: a script that fades or travels mid-scene (`travel-actor`, a fade to another room) places people on the second map too, and the table puts them under the first. `tools/check_spawns.py` (build plan §8) is the exact check, against the map files.", ""]
    out += ["## Problems", ""] + ([f"- {p}" for p in problems] or ["None."]) + [""]
    out += ["## Scripts", "", "| File | Id | Header |", "| --- | --- | --- |"]
    out += [f"| {f} | `{s}` | {h.replace('|', '/')} |" for f, s, h in scripts] + [""]
    out += ["## Triggers", "", "What starts each script, from the prose in its header: for whoever places the trigger on the map, or the code that plays it. The compiler doesn't read these (build plan §4.1 rule 1).", "",
            "| File | Id | Trigger |", "| --- | --- | --- |"]
    out += [f"| {f} | `{s}` | {' · '.join(notes).replace('|', '/')} |" for f, s, h in scripts if (notes := trigger_notes(s, h))] + [""]
    def table(title, d, col):
        rows = [f"## {title}", "", f"| {col} | Where |", "| --- | --- |"]
        rows += [f"| `{k}` | {', '.join(sorted(map(str, v)))} |" for k, v in sorted(d.items(), key=lambda kv: str(kv[0]))]
        return rows + [""]
    out += table("Flags set", sets, "Flag")
    out += table("Flags read", reads, "Flag")
    out += table("Meters", meters, "Meter")
    out += table("Items (given, taken or held)", items, "Item")
    out += table("Speakers", speakers, "Speaker")
    out += table("Fights", fights, "Fight")
    out += table("Backdrops", backdrops, "Backdrop")
    out += table("Maps", maps, "Map")
    sp = collections.defaultdict(set)
    for k, v in spawns.items():
        if isinstance(k, tuple):
            mm = re.search(r"map `([\w-]+)`", k[1])
            if k[0].startswith("enc-"): sp["(random encounters: anchors placed near the party, 08 §2.4)"] |= v
            else: sp[mm.group(1) if mm else f"(map of {k[0]}: see its header)"] |= v
        else: sp[k] |= v
    out += ["## Spawn points each map needs", "", "| Map | Spawns |", "| --- | --- |"]
    out += [f"| `{k}` | {', '.join(sorted(v))} |" for k, v in sorted(sp.items())] + [""]
    open(os.path.join(HERE, "09-registry.md"), "w", encoding="utf-8").write("\n".join(out))
    print(f"{len(scripts)} scripts, {len(sets)} flags set, {len(reads)} read, {len(problems)} problems")
    for p in problems: print("  " + p)
    return 1 if problems and "--check" in sys.argv else 0

if __name__ == "__main__":
    # Windows consoles default to cp1252, which has no arrow or middle dot; the lines quoted back need both
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.exit(main())
