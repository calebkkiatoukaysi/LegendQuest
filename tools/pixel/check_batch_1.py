"""Validate the 29 source-pilot files; --normalize replays approved size-only exports.

This is source QA, not the pixel-art converter or a runtime importer.
Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""
import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / "docs/pixel-art-batch-1"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def local(name, prefix):
    path = (ROOT / name).resolve()
    if not path.is_relative_to((ROOT / prefix).resolve()):
        raise ValueError("Path outside the pilot: " + name)
    return path


def key_mask(rgb, green=False):
    a = rgb.astype(np.int16)
    if green:
        return (a[..., 1] > 210) & (a[..., 0] < 50) & (a[..., 2] < 50)
    return (a[..., 0] > 210) & (a[..., 2] > 210) & (a[..., 1] < 50)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--normalize", action="store_true",
                        help="Replay owner-approved nearest-neighbour export sizing from immutable originals")
    args = parser.parse_args()
    plan = json.loads((DOCS / "prompts.json").read_text(encoding="utf-8"))
    records = json.loads((DOCS / "generation.json").read_text(encoding="utf-8"))
    by_index = {r["index"]: r for r in records}
    errors, warnings, measurements = [], [], []
    if len(plan) != 29 or len(records) != 29 or len(by_index) != 29:
        errors.append("Expected exactly 29 prompt and generation records")
    for i, entry in enumerate(plan):
        row = by_index.get(i)
        if row is None:
            errors.append(f"Missing generation record {i}")
            continue
        path = local(entry["path"], "tools/source/pixel")
        original = local(row["original"], "art/pixel-batch-1")
        if not original.is_file():
            errors.append(f"Missing untouched original: {original}")
            continue
        if digest(original) != row["sha256"]:
            errors.append(f"Original changed: {original}")
            continue
        with Image.open(original) as image:
            native = image.size
            if args.normalize:
                width, height = entry["size"]
                if abs(native[0] / width - native[1] / height) > 1e-6:
                    errors.append(f"Refusing nonuniform scaling: {path}")
                    continue
                if path.exists() and digest(path) not in (row["sha256"], row.get("exported_sha256")):
                    errors.append(f"Refusing to overwrite changed export: {path}")
                    continue
                image.resize((width, height), Image.Resampling.NEAREST).save(path)
                row["exported_sha256"] = digest(path)
                row["export_size"] = [width, height]
                row["export_method"] = "nearest-neighbour size-only; no crop, palette, keying, registration or grid conversion"
                row["status"] = "source pilot; conversion and registration review pending"
        if not path.is_file():
            errors.append(f"Missing export: {path}")
            continue
        if row.get("exported_sha256") and digest(path) != row["exported_sha256"]:
            errors.append(f"Export changed since approved normalization: {path}")
        with Image.open(path) as image:
            if image.format != "PNG" or list(image.size) != entry["size"]:
                errors.append(f"Wrong PNG dimensions: {path}")
            rgb = np.asarray(image.convert("RGB"))
        m = dict(index=i, path=entry["path"], size=list(rgb.shape[1::-1]),
                 native_size=list(native), sha256=digest(path))
        packed = (rgb[..., 0].astype(np.uint32) << 16) | (rgb[..., 1].astype(np.uint32) << 8) | rgb[..., 2]
        m["unique_colours"] = int(np.unique(packed).size)
        kind = entry["kind"]
        keyed = kind in ("portrait", "expression", "hair", "collar", "emotes", "life", "room-layer")
        if keyed:
            green = kind in ("portrait", "expression")
            key = key_mask(rgb, green)
            exact = np.all(rgb == ([0, 255, 0] if green else [255, 0, 255]), axis=2)
            m["key_region_fraction"] = round(float(key.mean()), 6)
            m["off_key_pixels_in_key_region"] = int((key & ~exact).sum())
            yy, xx = np.nonzero(~key)
            m["art_bounds"] = [int(xx.min()), int(yy.min()), int(xx.max()) + 1, int(yy.max()) + 1] if len(xx) else None
            if m["off_key_pixels_in_key_region"]:
                warnings.append(f"{entry['path']}: near-key colour noise; converter must flatten/key it")
            if kind in ("emotes", "life"):
                cells = []
                for cell in range(12):
                    x0, x1 = (cell % 4) * 384, (cell % 4 + 1) * 384
                    y0, y1 = (cell // 4) * 1024 // 3, (cell // 4 + 1) * 1024 // 3
                    art = ~key[y0:y1, x0:x1]
                    yy, xx = np.nonzero(art)
                    bounds = [int(xx.min()), int(yy.min()), int(xx.max()) + 1, int(yy.max()) + 1] if len(xx) else None
                    safe = bool(bounds and bounds[0] >= 24 and bounds[1] >= 24 and bounds[2] <= 360 and bounds[3] <= y1-y0-24)
                    cells.append(dict(cell=cell+1, bounds=bounds, inside_margin=safe))
                    if not safe:
                        warnings.append(f"{entry['path']}: cell {cell+1} needs margin review")
                m["cells"] = cells
        if kind in ("portrait", "expression", "hair", "collar"):
            blocks = rgb.reshape(128, 8, 128, 8, 3)
            flat = np.all(blocks == blocks[:, :1, :, :1, :], axis=(1, 3, 4))
            m["constant_8x8_block_fraction"] = round(float(flat.mean()), 6)
            if not flat.all():
                warnings.append(f"{entry['path']}: not yet a strict 128x128 grid; converter must snap it")
        measurements.append(m)
    if args.normalize:
        (DOCS / "generation.json").write_text(json.dumps(records, indent=2) + "\n", encoding="utf-8")
    report = dict(files=len(measurements), errors=errors, conversion_warnings=warnings,
                  strict_pixel_art_accepted=False, runtime_imported=False, measurements=measurements)
    (DOCS / "validation.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"{len(measurements)}/29 source PNGs; {len(errors)} export errors; {len(warnings)} conversion warnings")
    print("Pixel-grid, key-colour and registration acceptance belongs to the documented conversion pass.")
    for error in errors:
        print(error)
    return bool(errors)


if __name__ == "__main__":
    raise SystemExit(main())
