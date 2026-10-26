"""
Checks that ASSETS.md, Content.mgcb and the files on disk all agree. Run it from the repo root
before every commit:

    python tools/check_assets.py

It fails with a list of problems if:
  - a file under Content/ or tools/source/, or a script in tools/, isn't named in ASSETS.md
  - ASSETS.md names a file that doesn't exist
  - a /build: or /copy: line in Content.mgcb points at a file that doesn't exist
  - a file under Content/ isn't in Content.mgcb

One exception to the last one: a font file counts as in Content.mgcb when a .spritefont that is in
it names the font. The pipeline reads the TTF through the .spritefont and bakes the letters out of
it, so building or copying the TTF as well would just ship it twice.

Only needs CPython.
"""

import os
import re
import sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
CONTENT = os.path.join(ROOT, "Content")
MGCB = os.path.join(CONTENT, "Content.mgcb")
ASSETS = os.path.join(ROOT, "ASSETS.md")

#: Folders under Content/ the pipeline writes into. Nothing in them is mine.
BUILD_FOLDERS = ("bin", "obj")

#: Root-level files from the MonoGame template that ASSETS.md credits.
TEMPLATE_FILES = ("Icon.ico", "Icon.bmp", "app.manifest")

#: Anything in ASSETS.md that looks like a path to a file under Content/ or tools/.
PATH_PATTERN = re.compile(r"(?:Content|tools)/[A-Za-z0-9_./-]+\.[A-Za-z0-9]+")

FONT_PATTERN = re.compile(r"<FontName>\s*([^<]+?)\s*</FontName>")


def rel(path):
    return os.path.relpath(path, ROOT).replace(os.sep, "/")


def files_under(folder, skip=()):
    """Every file under a folder, as repo-relative paths with forward slashes."""
    found = []
    if not os.path.isdir(folder):
        return found
    for here, dirs, files in os.walk(folder):
        if here == folder:
            dirs[:] = [d for d in dirs if d not in skip]
        dirs[:] = [d for d in dirs if d != "__pycache__"]
        for name in files:
            found.append(rel(os.path.join(here, name)))
    return sorted(found)


def mgcb_entries():
    """The files Content.mgcb builds or copies, as repo-relative paths."""
    entries = []
    with open(MGCB, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            for key in ("/build:", "/copy:"):
                if line.startswith(key):
                    # A /build: line can say where the output goes after a semicolon. Only the source matters here.
                    source = line[len(key):].split(";")[0].strip()
                    entries.append("Content/" + source.replace("\\", "/"))
    return entries


def fonts_named_by(spritefonts):
    """The font files the given .spritefont files name, as repo-relative paths."""
    fonts = set()
    for sprite in spritefonts:
        with open(os.path.join(ROOT, sprite), encoding="utf-8") as f:
            for name in FONT_PATTERN.findall(f.read()):
                fonts.add(rel(os.path.join(os.path.dirname(os.path.join(ROOT, sprite)), name)))
    return fonts


def main():
    problems = []

    with open(ASSETS, encoding="utf-8") as f:
        assets_text = f.read()
    named = {m.rstrip(".,;:") for m in PATH_PATTERN.findall(assets_text)}
    named |= {name for name in TEMPLATE_FILES if re.search(r"(^|\s)" + re.escape(name) + r"\b", assets_text, re.M)}

    content_files = files_under(CONTENT, BUILD_FOLDERS)
    source_files = files_under(os.path.join(ROOT, "tools", "source"))
    scripts = sorted(rel(os.path.join(ROOT, "tools", n)) for n in os.listdir(os.path.join(ROOT, "tools")) if n.endswith(".py"))

    for path in content_files + source_files + scripts:
        if path not in named:
            problems.append("ASSETS.md doesn't name " + path)

    for path in sorted(named):
        if not os.path.isfile(os.path.join(ROOT, path)):
            problems.append("ASSETS.md names " + path + ", which doesn't exist")

    entries = mgcb_entries()
    for path in entries:
        if not os.path.isfile(os.path.join(ROOT, path)):
            problems.append("Content.mgcb builds " + path + ", which doesn't exist")

    covered = set(entries) | fonts_named_by([p for p in entries if p.endswith(".spritefont")])
    for path in content_files:
        if path == rel(MGCB):
            continue
        if path not in covered:
            problems.append(path + " isn't in Content.mgcb")

    if problems:
        print("check_assets: %d problem%s" % (len(problems), "" if len(problems) == 1 else "s"))
        for p in problems:
            print("  " + p)
        sys.exit(1)

    print("check_assets: ASSETS.md, Content.mgcb and the files agree (%d content files, %d sources, %d scripts)."
          % (len(content_files), len(source_files), len(scripts)))


if __name__ == "__main__":
    main()
