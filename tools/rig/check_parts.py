"""Audit atlas parts and creature frames; --json FILE saves measurements.

Exit 1 for findings outside --allow-list FILE. The allow-list maps exact
part/bone keys (or creature/row/column) to {check: reason, _rgba_sha256: hash}.
An exception applies only to the exact reviewed pixels; no wildcard waivers.
Written by Caleb Kiatoukaysi for Legend Quest; released under public domain.
"""
import argparse
import hashlib
from collections import Counter
import json
from pathlib import Path
import re

import numpy as np
from PIL import Image
from scipy import ndimage as nd

ROOT = Path(__file__).resolve().parents[2]
CHECKS = ("islands", "holes", "flat_edges", "opaque_card", "fragmented", "bleed", "frame_edges")


def fingerprint(rgba):
    return hashlib.sha256(str(rgba.shape).encode("ascii") + rgba.tobytes()).hexdigest()


def sizes(mask):
    labels, n = nd.label(mask)
    return labels, np.bincount(labels.ravel(), minlength=n + 1)[1:]


def border_share(mask, total):
    labels, counts = sizes(mask)
    ids = np.unique(np.concatenate((labels[0], labels[-1], labels[:, 0], labels[:, -1])))
    return float(sum(int(counts[k - 1]) for k in ids if k)) / max(1, total)


def longest(mask):
    padded = np.r_[False, mask, False].astype(int)
    starts = np.flatnonzero(np.diff(padded) == 1)
    ends = np.flatnonzero(np.diff(padded) == -1)
    return int(max(ends - starts, default=0))


def measure(rgba):
    opaque = rgba[:, :, 3] > 8
    labels, counts = sizes(opaque)
    total = int(opaque.sum())
    islands = []
    main = float(max(counts, default=0)) / max(1, total)
    if len(counts):
        biggest = int(np.argmax(counts)) + 1
        distance = nd.distance_transform_edt(labels != biggest)
        islands = [int(n) for k, n in enumerate(counts, 1)
                   if k != biggest and n >= 4 and distance[labels == k].min() >= 4]
    _, holes = sizes(nd.binary_fill_holes(opaque) & ~opaque)
    edges = (opaque[0], opaque[-1], opaque[:, 0], opaque[:, -1])
    flat = [name for name, edge in zip(("top", "bottom", "left", "right"), edges)
            if longest(edge) >= max(20, len(edge) * .5)]
    rgb = rgba[:, :, :3]
    white = opaque & (rgb.min(axis=2) > 235)
    black = opaque & (rgb.max(axis=2) < 50)
    shares = {"white": float(white.sum()) / max(1, total), "black": float(black.sum()) / max(1, total)}
    border = {"white": border_share(white, total), "black": border_share(black, total)}
    # Border connection alone includes outlines. Require a measurable card area.
    card = [name for name in shares if shares[name] > .30 or border[name] >= .03]
    return dict(islands=islands, holes=[int(n) for n in holes if n >= 150],
                flat_edges=flat, opaque_card=card, largest_share=main,
                opaque=total, colour_share=shares, border_share=border)


def audit(root, allow):
    folder = root / "Content/Characters/Parts"
    data = json.loads((folder / "parts.json").read_text(encoding="utf-8"))
    pages = {name: np.array(Image.open(folder / (name + ".png")).convert("RGBA")) for name in data["pages"]}
    rows = []
    for pid, part in data["parts"].items():
        entries = part.get("pieces", {"": part})
        for bone, entry in entries.items():
            x, y, w, h = entry["rect"]
            page = pages[entry["page"]]
            if x < 0 or y < 0 or w <= 0 or h <= 0 or x + w > page.shape[1] or y + h > page.shape[0]:
                raise ValueError("Invalid atlas rectangle: " + pid + "/" + bone)
            crop = page[y:y+h, x:x+w]
            row = measure(crop)
            row["rgba_sha256"] = fingerprint(crop)
            row.update(key=pid + ("/" + bone if bone else ""), part=pid, bone=bone, slot=part["slot"])
            ring = page[max(0, y-2):y+h+2, max(0, x-2):x+w+2, 3].copy()
            dx, dy = x - max(0, x-2), y - max(0, y-2)
            ring[dy:dy+h, dx:dx+w] = 0
            row["bleed"] = int((ring > 8).sum())
            row["fragmented"] = not bone and part["slot"] in ("weapon", "offhand", "helm", "hair") and row["largest_share"] < .7
            rows.append(row)
    text = (root / "Characters/Creatures.cs").read_text(encoding="utf-8")
    for pid, width, height in re.findall(r'new\("([\w-]+)",\s*"[^"]+",\s*Standing\((\d+),\s*(\d+)', text):
        path = root / "Content/Creatures" / (pid + ".png")
        sheet = np.array(Image.open(path).convert("RGBA"))
        w, h = int(width), int(height)
        if sheet.shape[:2] != (h * 4, w * 10):
            raise ValueError("Creature sheet dimensions disagree with Creatures.cs: " + pid)
        for r in range(4):
            for c in range(10):
                alpha = sheet[r*h:(r+1)*h, c*w:(c+1)*w, 3] > 8
                edges = [name for name, edge in zip(("top", "bottom", "left", "right"),
                         (alpha[0], alpha[-1], alpha[:, 0], alpha[:, -1])) if edge.any()]
                crop = sheet[r*h:(r+1)*h, c*w:(c+1)*w]
                rows.append(dict(key=f"{pid}/{r}/{c}", creature=pid, frame_edges=edges, rgba_sha256=fingerprint(crop)))
    summary, raw = Counter(), Counter()
    for row in rows:
        row["findings"] = [check for check in CHECKS if row.get(check)]
        reviewed = allow.get(row["key"], {})
        matches = reviewed.get("_rgba_sha256") == row["rgba_sha256"]
        row["unexpected"] = [check for check in row["findings"] if not (matches and reviewed.get(check))]
        raw.update(row["findings"])
        summary.update(row["unexpected"])
    return {"summary": dict(summary), "raw_summary": dict(raw), "rectangles": sum("part" in r for r in rows),
            "rig_islands": sum(bool(r.get("bone") and r.get("islands")) for r in rows),
            "held_fragmented": sum(bool(r.get("fragmented") and r.get("slot") in ("weapon", "offhand")) for r in rows),
            "rows": rows}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--json", type=Path)
    parser.add_argument("--allow-list", type=Path, default=ROOT / "tools/rig/parts-allow-list.json")
    args = parser.parse_args()
    allow = json.loads(args.allow_list.read_text(encoding="utf-8")) if args.allow_list.exists() else {}
    for key, checks in allow.items():
        if not re.fullmatch(r"[0-9a-f]{64}", checks.get("_rgba_sha256", "")):
            parser.error("Each allowance needs the reviewed RGBA fingerprint: " + key)
        reasons = {check: reason for check, reason in checks.items() if check != "_rgba_sha256"}
        if not reasons or any(check not in CHECKS or not isinstance(reason, str) or not reason.strip() for check, reason in reasons.items()):
            parser.error("Each allowance needs a known check and a written reason: " + key)
    report = audit(ROOT, allow)
    print(f"{report['rectangles']} rectangles; {report['rig_islands']} rig pieces with islands; {report['held_fragmented']} fragmented held items")
    print(json.dumps(report["summary"], sort_keys=True))
    print("Raw findings before reviewed exceptions:", json.dumps(report["raw_summary"], sort_keys=True))
    offenders = [r for r in report["rows"] if r["unexpected"]]
    for row in sorted(offenders, key=lambda r: (-len(r["unexpected"]), -sum(r.get("islands", [])), r["key"]))[:30]:
        print(row["key"], ", ".join(row["unexpected"]), "islands", row.get("islands", []), "holes", row.get("holes", []))
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    return bool(offenders)


if __name__ == "__main__":
    raise SystemExit(main())
