"""Plan, save and review the ordered front-facing source portraits.

Image painting uses the built-in image tool; this script only records sources,
replays owner-approved export sizing, and makes diagnostic contact sheets.
Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""
import argparse
import hashlib
import json
import re
import shutil
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / "docs/pixel-art-portraits"
STATE = DOCS / "progress.json"
PLAN = DOCS / "jobs.json"


def read(path):
    return json.loads(path.read_text(encoding="utf-8"))


def write(path, value):
    temporary = path.with_suffix(path.suffix + ".pending")
    temporary.write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    temporary.replace(path)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def plan():
    text = (DOCS / "ALL-PORTRAITS-for-ChatGPT.md").read_text(encoding="utf-8")
    style = text.split("## The style (every picture)\n", 1)[1].split("## The size", 1)[0].strip()
    size = text.split("## The size and framing (every picture)\n", 1)[1].split("## The expressions", 1)[0].strip()
    expressions = dict(re.findall(r"^- \*\*([a-z]+)\*\*: (.+)$", text, re.M))
    groups = re.split(r"^### (.+?) \(`([a-z0-9-]+)`\), template: (male|female)\s*$", text, flags=re.M)
    jobs = []
    for i in range(1, len(groups), 4):
        title, character, template, content = groups[i:i+4]
        character_text = re.search(r"^CHARACTER\..+$", content, re.M).group()
        for number, filename, instructions in re.findall(r"^(\d+)\. `([^`]+\.png)`: (.+)$", content, re.M):
            number = int(number)
            parent_match = re.search(r"an edit of #(\d+)", instructions)
            parent = int(parent_match[1]) if parent_match else None
            expression = filename.rsplit(".", 2)[-2]
            if instructions.startswith("variant"):
                change = re.split(r"an edit of #\d+\.\s*", instructions, maxsplit=1)[1]
                prompt = ("Edit the attached neutral portrait for this character. Change ONLY the variant described below, "
                          "keeping the same person's identity, neutral expression, pixel grid, size, position and front-facing framing. "
                          "Paint new clothing and details on the same coarse96by96 grid of big square pixels, "
                          "with flat three-or-four-tone cel shading and dark brown-violet outlines. "
                          "No fine texture, fabric weave, grain, gradients, smoothing or smaller pixels. "
                          "Keep the flat #00FF00 background. Give it back at exactly1024by1024pixels.\n\n"
                          "VARIANT. " + change + "\n\nI'll save it as " + filename + ".")
                kind = "variant"
            elif parent is not None:
                prompt = ("Edit the attached neutral portrait: change ONLY the expression, to the one below. "
                          "Move only the eyes, eyebrows, nose, mouth and cheeks, and the head no more than the expression needs. "
                          "The hair, beard, clothes, colours, pixel grid, size and position of everything else stay exactly the same, "
                          "and the face keeps looking straight at the viewer. Keep it real pixel art on the same grid of big pixels, "
                          "on the same flat #00FF00 background. Give it back at exactly1024by1024pixels.\n"
                          "EXPRESSION. " + expressions[expression] + "\nI'll save it as " + filename + ".")
                kind = "expression"
            else:
                prompt = "\n\n".join((style, size, character_text,
                                        "EXPRESSION. " + expressions["neutral"], "I'll save it as " + filename + "."))
                kind = "neutral"
            destination = f"tools/source/pixel/portraits/{character}/front-facing/{filename}"
            jobs.append(dict(number=number, filename=filename, character=character, title=title,
                             kind=kind, expression=expression, parent=parent, template=template,
                             instructions=instructions, prompt=prompt, path=destination))
    overrides_path = DOCS / "design-overrides.json"
    if overrides_path.exists():
        overrides = read(overrides_path)
        for job in jobs:
            job.update(overrides.get(str(job["number"]), {}))
    # The first 626 are the original series (2026-11-20); later pictures are only ever appended after them.
    assert [j["number"] for j in jobs] == list(range(1, len(jobs) + 1)) and len(jobs) >= 626,         "Numbering changed or parser lost a row"
    assert len({j["path"] for j in jobs}) == len(jobs), "Duplicate destination"
    assert all(j["parent"] is None or j["parent"] < j["number"] for j in jobs), "Forward reference"
    write(PLAN, jobs)
    print(f"{len(jobs)} jobs; {len(set(j['character'] for j in jobs))} character groups")


def save(number, source):
    jobs = read(PLAN)
    job = jobs[number-1]
    state = read(STATE)
    if number in state["completed_numbers"]:
        raise ValueError(f"Already recorded #{number}; refusing replacement")
    if number != state["next_number"]:
        raise ValueError("Out of order: next is " + str(state["next_number"]))
    source = Path(source)
    original_dir = job.get("original_dir", f"art/pixel-portraits/originals/{job['character']}")
    original = ROOT / original_dir / f"{number:03}-{job['filename']}"
    dest = ROOT / job["path"]
    original.parent.mkdir(parents=True, exist_ok=True)
    dest.parent.mkdir(parents=True, exist_ok=True)
    if original.exists() or dest.exists():
        raise ValueError("Existing unrecorded file requires recovery review: " + str(dest))
    with Image.open(source) as im:
        if im.format != "PNG" or im.width != im.height:
            raise ValueError("Expected square PNG, without cropping or nonuniform scaling")
        native = list(im.size)
        shutil.copyfile(source, original)
        im.resize((1024, 1024), Image.Resampling.NEAREST).save(dest)
    prompt_path = DOCS / f"{number:03}-prompt.txt"
    prompt_path.write_text(job["prompt"] + "\n", encoding="utf-8")
    if job["parent"] is not None:
        parent = next(p for p in state["portraits"] if p["number"] == job["parent"])
        reference = parent["path"]
    else:
        reference = f"tools/source/pixel/templates/front-facing/portrait-template-{job['template']}.png"
    reference = job.get("reference_override", reference)
    record = dict(number=number, filename=dest.name, path=job["path"],
                  original=original.relative_to(ROOT).as_posix(), reference_number=job["parent"],
                  reference=reference, reference_sha256=digest(ROOT/reference), native_size=native,
                  export_size=[1024, 1024], source_sha256=digest(original), export_sha256=digest(dest),
                  prompt=prompt_path.relative_to(ROOT).as_posix(),
                  export_method="nearest-neighbour size normalization, explicitly approved by owner; untouched original retained",
                  status="source draft; exact grid, palette and registration require conversion review")
    state["portraits"].append(record)
    state["completed_numbers"].append(number)
    state["next_number"] = number+1 if number < len(jobs) else None
    state["automatic"] = True
    state["review_after_number"] = None
    state["review_note"] = "Owner requested automatic progress through the document; visual review remains part of the run"
    write(STATE, state)
    assets = ROOT / "ASSETS.md"
    credit = (f"\n{job['path']} - picture{number}, {job['title']} {job['expression']}, supplied by Caleb Kiatoukaysi, "
              "made in ChatGPT with built-in image generation from the ordered portrait brief. "
              "Original and reference/export fingerprints retained in art/pixel-portraits and docs/pixel-art-portraits; "
              "owner-approved nearest-neighbour export sizing only. Source draft for pixel conversion; "
              "released for Legend Quest by its owner.\n")
    with assets.open("a", encoding="utf-8") as stream:
        stream.write(credit)
    print(json.dumps(dict(number=number, path=job["path"], native_size=native, next=state["next_number"])))


def review(character):
    jobs = {j["number"]: j for j in read(PLAN)}
    records = [r for r in read(STATE)["portraits"] if jobs[r["number"]]["character"] == character]
    if not records:
        raise ValueError("No completed pictures for " + character)
    columns = min(6, len(records))
    rows = (len(records)+columns-1)//columns
    sheet = Image.new("RGB", (columns*220, rows*245), (28, 29, 39))
    draw = ImageDraw.Draw(sheet)
    for i, record in enumerate(records):
        x, y = i % columns*220, i//columns*245
        with Image.open(ROOT/record["path"]) as im:
            sheet.paste(im.convert("RGB").resize((220, 220), Image.Resampling.NEAREST), (x, y+25))
        draw.text((x+4, y+6), str(record["number"])+" "+jobs[record["number"]]["expression"], fill="white")
    path = ROOT/f"art/pixel-portraits/review/{character}.jpg"
    path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(path, quality=93)
    print(path)


def revise(number, source, prompt_file):
    state = read(STATE)
    row = next(r for r in state["portraits"] if r["number"] == number)
    if any(r.get("reference_number") == number for r in state["portraits"]):
        raise ValueError("Dependents already exist; revision needs coordinated regeneration")
    source = Path(source)
    original = ROOT / f"art/pixel-portraits/revisions/{number:03}-{digest(source)[:12]}.png"
    original.parent.mkdir(parents=True, exist_ok=True)
    with Image.open(source) as im:
        if im.format != "PNG" or im.width != im.height:
            raise ValueError("Expected square PNG")
        old = dict(row)
        old.pop("attempts", None)
        row.setdefault("attempts", []).append(old)
        shutil.copyfile(source, original)
        im.resize((1024, 1024), Image.Resampling.NEAREST).save(ROOT / row["path"])
        row["native_size"] = list(im.size)
    row.update(original=original.relative_to(ROOT).as_posix(), source_sha256=digest(original),
               export_sha256=digest(ROOT/row["path"]), prompt=Path(prompt_file).as_posix())
    write(STATE, state)
    print(f"Revised #{number}; prior export retained; next #{state['next_number']}")


def check():
    state = read(STATE)
    problems = []
    archived = []
    for design in state.get("design_history", []):
        archived.extend(read(ROOT / design["archive"]))
    for row in state["portraits"] + archived:
        path, original = ROOT/row["path"], ROOT/row["original"]
        if digest(path) != row["export_sha256"] or digest(original) != row["source_sha256"]:
            problems.append(f"#{row['number']} changed pixels")
        with Image.open(path) as im:
            if im.format != "PNG" or im.size != (1024, 1024):
                problems.append(f"#{row['number']} invalid export")
        if "reference_sha256" in row and digest(ROOT / row["reference"]) != row["reference_sha256"]:
            problems.append(f"#{row['number']} reference changed")
    write(DOCS/"source-check.json", dict(completed=len(state["portraits"]), total=626,
                                        next_number=state["next_number"], retained_alternates=len(archived), problems=problems,
                                        runtime_imported=False, pixel_conversion_accepted=False))
    print(f"{len(state['portraits'])}/626 exports; {len(problems)} file/dimension/fingerprint problems")
    for problem in problems:
        print(problem)
    return bool(problems)


def ready():
    """One unseen neutral/variant, or expressions whose neutral has been reviewed."""
    state, jobs = read(STATE), read(PLAN)
    number = state["next_number"]
    if number is None:
        print("[]")
        return
    completed = set(state["completed_numbers"])
    selected = []
    for job in jobs[number-1:]:
        if selected and (job["kind"] != "expression" or len(selected) >= 12):
            break
        if job["parent"] is not None and job["parent"] not in completed:
            break
        reference = jobs[job["parent"]-1]["path"] if job["parent"] is not None else (
            f"tools/source/pixel/templates/front-facing/portrait-template-{job['template']}.png")
        selected.append(dict(job, reference=job.get("reference_override", reference)))
        if job["kind"] != "expression":
            break
    if not selected:
        raise ValueError("Next picture has no recorded neutral reference")
    print(json.dumps(selected, ensure_ascii=False))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="mode", required=True)
    sub.add_parser("plan")
    save_parser = sub.add_parser("save")
    save_parser.add_argument("number", type=int)
    save_parser.add_argument("source")
    review_parser = sub.add_parser("review")
    review_parser.add_argument("character")
    sub.add_parser("check")
    sub.add_parser("ready")
    revise_parser = sub.add_parser("revise")
    revise_parser.add_argument("number", type=int)
    revise_parser.add_argument("source")
    revise_parser.add_argument("prompt_file")
    args = parser.parse_args()
    if args.mode == "plan":
        plan()
    elif args.mode == "save":
        save(args.number, args.source)
    elif args.mode == "review":
        review(args.character)
    elif args.mode == "ready":
        ready()
    elif args.mode == "revise":
        revise(args.number, args.source, args.prompt_file)
    else:
        return check()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
