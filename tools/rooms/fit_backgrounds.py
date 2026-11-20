"""Fit reviewed room backgrounds to their floor and interactive-door centres.

Used by cut_room.py --fit. The JSON records every source's exact pixels, its
painted floor line and the measured centre of each usable door. The original
paintings stay untouched. Written by Caleb Kiatoukaysi; public domain.
"""
import hashlib
import numpy as np


def fit_panel(zone, room, index, picture, entries):
    import sourcekit as sk
    key = f"{zone.id}/{room.id}/bg-{index}"
    entry = entries[key]
    if hashlib.sha256(picture.tobytes()).hexdigest() != entry["rgba_sha256"]:
        raise sk.SourceError(key + ": the painting changed; review its fitting measurements again")
    floor = entry["floor_y"]
    target_floor = sk.to_panel(0, room.back_edge_at(room.width / 2), 0)[1]
    left = sk.panel_lefts(room.width)[index - 1]
    anchors = [(0., 0.), (1535., 1535.)]
    seen = set()
    for door in room.doors:
        target = (door.x - left) * sk.SCALE
        if not 1 < target < 1534 or door.x in seen:
            continue
        seen.add(door.x)
        measured = entry.get("doors", {}).get(door.id)
        if measured is None:
            raise sk.SourceError(key + ": no reviewed centre for door " + door.id)
        anchors.append((target, float(measured)))
    anchors.sort()
    dest, source = np.array(anchors).T
    if (np.diff(source) <= 0).any() or not 500 <= floor <= 850:
        raise sk.SourceError(key + ": invalid floor or crossing door-centre measurements")
    stretch = np.diff(dest) / np.diff(source)
    if (stretch < .25).any() or (stretch > 4).any():
        raise sk.SourceError(key + ": fitting would stretch an interval excessively; remeasure this panel")
    # Separable, monotone resampling keeps the architecture upright. It moves
    # door centres and the floor, without moving any gameplay coordinates.
    xs = np.interp(np.arange(1536), dest, source)
    ys = np.interp(np.arange(1024), (0, target_floor, 1023), (0, floor, 1023))
    x0 = np.floor(xs).astype(int); x1 = np.minimum(x0 + 1, 1535)
    y0 = np.floor(ys).astype(int); y1 = np.minimum(y0 + 1, 1023)
    fx = (xs - x0)[None, :, None]; fy = (ys - y0)[:, None, None]
    a = picture[y0[:, None], x0[None, :]].astype(float)
    b = picture[y0[:, None], x1[None, :]].astype(float)
    c = picture[y1[:, None], x0[None, :]].astype(float)
    d = picture[y1[:, None], x1[None, :]].astype(float)
    return np.rint((a * (1-fx) + b * fx) * (1-fy) + (c * (1-fx) + d * fx) * fy).astype(np.uint8)


def seam_weight(previous, incoming, start, width):
    """A minimum-error seam with a narrow soft join, avoiding doubled buildings."""
    x0 = max(0, int(start)); x1 = min(incoming.shape[1], int(start + width))
    cost = np.abs(previous[:, x0:x1, :3] - incoming[:, x0:x1, :3]).mean(axis=2)
    h, w = cost.shape
    history = np.zeros((h, w), np.int8)
    total = cost[0].copy()
    for y in range(1, h):
        options = np.stack((np.r_[np.inf, total[:-1]], total, np.r_[total[1:], np.inf]))
        step = np.argmin(options, axis=0)
        history[y] = step - 1
        total = cost[y] + options[step, np.arange(w)]
    path = np.empty(h, int); path[-1] = int(total.argmin())
    for y in range(h-1, 0, -1):
        path[y-1] = path[y] + history[y, path[y]]
    distance = np.arange(incoming.shape[1])[None, :] - (path[:, None] + x0)
    t = np.clip((distance + 6.) / 12., 0., 1.)
    return (t * t * (3. - 2. * t))[:, :, None]
