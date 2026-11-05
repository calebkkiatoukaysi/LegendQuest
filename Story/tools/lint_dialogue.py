#!/usr/bin/env python3
"""Lint LegendQuest script files (story/0*-script-*.md, 07, 08) for the writing guide's mechanical rules.

Usage:  python story/tools/lint_dialogue.py [files...]      (default: the script files, story/04 to 08)
Exit code 1 if any ERROR. WARNs are for a human to judge.

Checks (02-writing-guide.md section in brackets):
  ERROR  node with no speaker (a bare ~effects or →next line, or a label on a ?)    [notation]
  ERROR  a cue line (!) carrying ~effects or →next                                     [notation]
  ERROR  verb agreeing with {sb_they}: "{sb_they} is/was/has/'s/does"                  [4]
  ERROR  leading ellipsis; more than one ellipsis in a script                          [2]
  ERROR  "a beat", "after a pause", "for a long moment", "something shifts"            [2]
  ERROR  therapy words / stock phrases / modern pep talk                               [2]
  ERROR  em dash anywhere but the very end of a line (interruption)                    [2]
  ERROR  line over 170 characters of spoken text                                       [1]
  ERROR  a [CALLBACK x] phrase used more than four times in the game                   [3]
  ERROR  → label that doesn't exist in the same script                                 [notation]
  WARN   "It's not X, it's Y" / "not X. Y" shapes                                       [2]
  WARN   exclamation mark outside a bark table or a fight cue                          [2]
  WARN   three fragments of one to two words in a row                                   [2]
  WARN   more than four nodes in a row from one speaker; three narrator lines in a row [1, 6]
  WARN   narrator line with "seems", "feels", or an emotion adjective about someone     [6]
  WARN   player option over twelve words                                                [5]
"""
import re, sys, glob, os, collections

BANNED = [r"\ba beat\b", r"\bafter a pause\b", r"\bfor a long moment\b", r"\bsomething shifts\b",
          r"\bvalid\b", r"\bprocess (this|it|that)\b", r"\bboundaries\b", r"\bI hear you\b", r"\bjourney\b",
          r"\btrauma\b", r"\btestament to\b", r"\bweight of\b", r"\bthe air (was |is )?thick with\b",
          r"\bpalpable\b", r"\ba dance of\b", r"\bwhispers of\b", r"\btapestry\b", r"\bunspoken\b",
          r"\bin that moment\b", r"\bwe've got this\b", r"\byou're not alone\b", r"\blet that sink in\b",
          r"\bhere's the thing\b", r"\bpower corrupts\b", r"\bgreater good\b", r"\bwe're not so different\b",
          r"\bthe real enemy\b"]
PROUD_OK = "LEON"   # Leon's dying line is the one allowed "proud"
SB_THEY = re.compile(r"\{[Ss]b_they\}(\s+(is|was|has|does|isn't|wasn't|hasn't|doesn't)\b|'s\b)")
SPEAKER = re.compile(r"^(?P<who>[A-Z][A-Z0-9 \-]*(\((?:sb|you)\))?)(\s*\((?P<expr>[\w-]+)\))?(\s*\[[^\]]*\])?\s*:\s*(?P<text>.*)$")
NARR_EMO = re.compile(r"\b(seems|seemed|feels|felt|conflicted|sadly|angrily|nervously|hesitantly|clearly upset)\b", re.I)

def strip_tail(text):
    text = re.sub(r"\s+\[CALLBACK [\w-]+\]", "", text)
    text = re.split(r"\s{2}(?:~|→)", text)[0]
    return text.strip()

def scripts(path):
    """yield (script_id, [(lineno, raw)]) for each fenced block, attributing it to the last #### script header."""
    cur = None; inb = False; buf = []; skip = False
    for n, line in enumerate(open(path, encoding="utf-8"), 1):
        s = line.rstrip("\n")
        m = re.match(r"^#### (?:script |banter )?([a-z][\w-]+)", s)
        if m and not inb: cur = m.group(1)
        if s.startswith("```"):
            if inb:
                if not skip: yield cur, buf
                buf = []
            else:
                skip = s.strip() != "```"   # ```text blocks are examples, not scripts
            inb = not inb; continue
        if inb: buf.append((n, s))

def lint(files):
    errs = warns = 0
    callbacks = collections.Counter()
    def out(level, f, n, msg, line=""):
        nonlocal errs, warns
        if level == "ERROR": errs += 1
        else: warns += 1
        print(f"{level} {os.path.basename(f)}:{n}: {msg}" + (f"\n      {line.strip()[:140]}" if line else ""))
    for f in files:
        for cb in re.findall(r"\[CALLBACK ([\w-]+)\]", open(f, encoding="utf-8").read()):
            callbacks[cb] += 1
        for sid, lines in scripts(f):
            labels = set(); targets = []; ellipses = 0; run_who = None; run = 0; frag = 0; pending = False
            prev_open = None   # (lineno, raw) of the last node if it ends without a jump and isn't a question
            for n, raw in lines:
                t = raw.strip()
                if not t: continue
                if re.match(r"^#[\w-]+\s", t) and prev_open and not re.match(r"^#[\w-]+\s+(\{if[^}]*\}\s*)?!", t):
                    out("WARN", f, prev_open[0], "falls through into " + t.split()[0] + " (add → or check it's meant)", prev_open[1])
                if not t.startswith("!") and not re.match(r"^#[\w-]+\s+(\{if[^}]*\}\s*)?!", t):
                    prev_open = None if ("→" in t or t.startswith("?") or re.match(r"^\d+\.", t)) else (n, raw)
                m = re.match(r"^#([\w-]+)\s+(.*)$", t)
                if m: labels.add(m.group(1)); body = m.group(2)
                else: body = t
                body = re.sub(r"^\{if [^}]*\}\s*", "", body)
                for tg in re.findall(r"→\s*([\w-]+)", t):
                    if tg not in ("end", "pass"): targets.append((n, tg, raw))
                branch = bool(m) or t.startswith("{if") or pending
                pending = False
                pf = re.search(r"→\s*pass\s+([\w-]+)\s*·\s*fail\s+([\w-]+)", t)
                if pf: targets += [(n, pf.group(1), raw), (n, pf.group(2), raw)]
                if body.startswith("!"):
                    pending = pending or branch
                    if "→" in body or re.search(r"\s~\s", body): out("ERROR", f, n, "cue line carries effects or a jump", raw)
                    continue
                if body.startswith("~") or body.startswith("→"):
                    out("ERROR", f, n, "node with no speaker", raw); continue
                if body.startswith("?"):
                    if m: out("ERROR", f, n, "label on a question marker", raw)
                    continue
                opt = re.match(r"^\d+\.\s+(.*)$", body)
                if opt:
                    o = strip_tail(re.sub(r"^(♥\s*)?(\[[^\]]*\]\s*)*", "", opt.group(1)))
                    if not o.startswith("(") and len(o.split()) > 12: out("WARN", f, n, "player option over twelve words", raw)
                    text, who = o, "YOU"
                else:
                    sm = SPEAKER.match(body)
                    if not sm:
                        if not body.startswith("CARD") and not body.startswith("CHAPTER"):
                            out("WARN", f, n, "unrecognised line", raw)
                        continue
                    who, text = sm.group("who").strip(), strip_tail(sm.group("text"))
                    if who in ("CARD", "CHAPTER"): continue
                    if len(text) > 170: out("ERROR", f, n, f"spoken text {len(text)} chars (max 170)", raw)
                    if who == run_who and not branch: run += 1
                    else: run_who, run = who, 1
                    if run == 5 and who != "NARRATOR": out("WARN", f, n, f"{who} has five nodes in a row", raw)
                    if run == 3 and who == "NARRATOR": out("WARN", f, n, "three narrator lines in a row (max two)", raw)
                    if who == "NARRATOR" and NARR_EMO.search(text): out("WARN", f, n, "narrator interprets", raw)
                low = text.lower()
                if SB_THEY.search(text): out("ERROR", f, n, "verb agrees with {sb_they}", raw)
                if text.startswith("...") or text.startswith("…"): out("ERROR", f, n, "leading ellipsis", raw)
                ellipses += text.count("...") + text.count("…")
                for b in BANNED:
                    if re.search(b, text, re.I): out("ERROR", f, n, f"banned phrase /{b}/", raw)
                if re.search(r"\bproud\b", low) and who != PROUD_OK and "[CALLBACK proud]" not in raw: out("ERROR", f, n, "'proud' is Leon's alone", raw)
                if "—" in text.rstrip("—") : out("ERROR", f, n, "em dash mid-line", raw)
                if re.search(r"\b(it'?s|that'?s|this is) not [^.,;]+, (it'?s|that'?s)\b", low) or re.search(r"\bnot (just )?an? [a-z]+\. an? [a-z]+\.", low):
                    out("WARN", f, n, "'not X, it's Y' shape", raw)
                if "!" in text and not opt: out("WARN", f, n, "exclamation mark (fine in a fight or a real shout)", raw)
                words = re.findall(r"[A-Za-z']+", text)
                if not opt and 0 < len(words) <= 2 and text.endswith("."): frag += 1
                else: frag = 0
                if frag == 3: out("WARN", f, n, "three fragments in a row", raw)
            if ellipses > 1: out("ERROR", f, lines[0][0], f"script {sid} has {ellipses} ellipses (max 1)")
            for n, tg, raw in targets:
                if tg not in labels: out("ERROR", f, n, f"jump to unknown label '{tg}' in script {sid}", raw)
    for cb, c in callbacks.items():
        if c > 4: print(f"ERROR callback '{cb}' marked {c} times (max 4)"); errs += 1
    print(f"\n{errs} errors, {warns} warnings")
    return errs

if __name__ == "__main__":
    # Windows consoles default to cp1252, which has no arrow or middle dot; the lines quoted back need both
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    here = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    files = sys.argv[1:] or sorted(glob.glob(os.path.join(here, "0[4-8]*.md")) + glob.glob(os.path.join(here, "1[0-9]*.md")))
    sys.exit(1 if lint(files) else 0)
