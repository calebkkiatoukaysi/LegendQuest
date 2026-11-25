"""Record ordered batch-2 room paintings without changing runtime assets.

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
DOCS = ROOT / 'docs/pixel-art-batch-2-rooms'


def write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def plan():
    text = (ROOT / 'docs/pixel-art-batch-2-for-ChatGPT.md').read_text(encoding='utf-8')
    rooms = re.split(r'^### `([^`]+)` · (.+)$', text.split('## Part 3.', 1)[1], flags=re.M)
    jobs = []
    for i in range(1, len(rooms), 3):
        room, title, content = rooms[i:i+3]
        outdoor = 'open air;' in content.split('```', 1)[0]
        for prompt in re.findall(r'```text\n(.*?)\n```', content, re.S):
            filename = re.search(r"I'll save it as (bg-\d+(?:\.(?:sky|far|mid|fg))?\.png)\.", prompt)
            if not filename:
                continue
            filename = filename[1]
            match = re.fullmatch(r'bg-(\d+)(?:\.(sky|far|mid|fg))?\.png', filename)
            panel, kind = int(match[1]), match[2] or 'panel'
            if kind == 'sky':
                prompt += '\n\nSKY BOUNDARY. Building ornaments, gold sun emblems mounted on spires, crosses, finials, flags, chimney smoke and their halos are not celestial objects: remove them with their buildings. Retain an actual visible sun/moon only if it exists in this panel; never copy one from a preceding panel.'
            if kind == 'far':
                prompt += '\n\nEXTRACTION COMPLETENESS. Retain every visible distant detail in the scene, including distant boats, ships, lighthouses, harbour walls and far banks if present. Carry water or distant terrain behind all removed near objects down to the walking floor line, so it cannot leave gaps when combined with the middle ground. Do not invent objects or shift anything.'
            path = f'tools/source/pixel/rooms/{room}/{filename}'
            reference = (f'art/rooms/generated/{room}/bg-{panel}.png' if kind == 'panel'
                         else f'tools/source/pixel/rooms/{room}/bg-{panel}.png')
            references = [reference]
            if panel > 1:
                previous = filename.replace(f'bg-{panel}', f'bg-{panel-1}')
                references.append(f'tools/source/pixel/rooms/{room}/{previous}')
                prompt += '\n\nImage 1 is the edit target / scene reference. Image 2 is the finished same-kind preceding panel. Where this panel shows the same stretch as the second attached picture, make it match that one exactly. Do not move anything in image 1 to imitate image 2.'
            if room == 'docks/harbourside' and panel == 3 and kind == 'far':
                prompt += '\n\nPANEL 3 POSITION LOCK. Follow IMAGE 1 geometry: retain the fishing boat with red furled sail on the RIGHT at x1235-1478 and y400-636, the white hilltop lighthouse at x1223-1253 and y300-363, and the stone breakwater lighthouse on the far RIGHT at x1448-1509 and y389-516. Do not copy image 2\'s ship or lighthouse from the left side into this panel. Carry sea behind the removed near houses down to y715. Remove all sky, sun and clouds to magenta above the hills. Keep water, boat and far coast; no near buildings, tree, fish stall, bollard or floor.'
            if room == 'docks/fish-market' and panel == 2:
                if kind == 'sky':
                    prompt += '\n\nThis panel has NO visible sun. The gold sun emblem at x1357,y56 belongs to the distant castle spire and must be removed with that building, not retained in the sky. Do not copy the sun from image 2. Keep only image 1 sky and clouds.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 POSITION LOCK. Keep its large distant castle on the RIGHT at x980-1535, with tallest gold sun-tipped spire at x1357 and y32-260. Do not replace it with image 2\'s small central island castle. Keep only image 1 far coast, hills, castle, water; carry water behind near objects down to y731. Remove near sailboats and masts (they belong with middle ground), near buildings, pink tree, stairs, lamps, bushes and fish rack. All sky above castle/hills and area below y731 must be magenta. Do not add a sun, clouds or new castle.'
            if room == 'docks/fish-market' and panel == 3:
                if kind == 'sky':
                    prompt += '\n\nNo visible sun in this panel: remove the gold emblem and halo at x1232,y56 with the castle spire. Keep only sky and clouds from IMAGE 1. No emblem, sun, castle or scenery may remain.'
                elif kind == 'far':
                    prompt += '\n\nPANEL 3 POSITION LOCK: the main gold sun-tipped castle spire is centered at x1232 (not x1357 as in image 2), from y28 to y260. Keep IMAGE 1 castle and hill geometry unchanged. The castle begins behind the tree at x870. Carry sea down to y731. Remove all near stairs, trees, bushes, buildings, lamps, fish rack and sailboats. No sky, clouds or new sun; keep the castle emblem attached to its spire. Magenta above hill/castle and below y731.'
            if room == 'questeria/gate-plaza' and panel == 2:
                if kind == 'sky':
                    prompt += '\n\nThis panel has no visible sun. Remove Sunspire and its gold ornament at x1384,y84 completely. Keep only image 1 sky and clouds, not the round sun from the previous panel.'
                elif kind == 'far':
                    prompt += '\n\nThe far Sunspire remains at x1384 on the RIGHT with its gold ornament at y84. Retain image 1 distant city, hills and water in their exact positions. Continue the far scenery down to y670 behind removed near buildings. Remove all sky and clouds to magenta; do not copy the preceding panel\'s sea/lighthouse into the left side. No near houses, shop, well, bench, lamps, barrels or floor.'
                elif kind == 'fg':
                    prompt += '\n\nDOOR CLEARANCE. All foreground objects must fit in a clear space such as x820-1020, clear of shop door x300-380, green door x685-780 and red door x1070-1160. Make the overlay small enough to fit, no wider than195 pixels and no taller than140 pixels from the bottom edge.'
            if room == 'questeria/gate-plaza' and panel == 3:
                if kind == 'sky':
                    prompt += '\n\nNo actual visible sun: remove the castle ornament at x1337,y84. Retain only image 1 sky/clouds, no tents, buildings or flags.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 GEOMETRY: tallest Sunspire is at x1337,y84, not x1384 in image 2. Keep its distant hillside/city and the festival tents and distant bunting visible through the RIGHT arch at x1320-1458,y490-614. Retain the view through that arch and continue its distant plaza paving down to y670 behind the arch. No near arch masonry, houses, doors, well, barrels, lamps or walking floor. Keep the upper sky magenta and fill below the far shore as needed behind removed near objects, down to y670, then magenta below. Never copy preceding-panel scenery over image 1 positions.'
                elif kind == 'mid':
                    prompt += '\n\nThe distant festival tents and plaza view through the arch belong in the distance layer: remove that view inside the arch to magenta. Keep the near arch masonry, flowers, purple banners and the near triangular bunting fastened across the arch opening at y452-481. Keep every near house, door, lamp, well and street.'
                elif kind == 'fg':
                    prompt += '\n\nKeep foreground clear of the right arch x1320-1460 and all doors x180-260, x535-625, x890-965. Place a small overlay in the clear span x680-810, with its entire silhouette within that span. Do not cover an entrance.'
            if room == 'questeria/festival-square' and panel == 2:
                if kind == 'sky':
                    prompt += '\n\nThis panel has NO actual sun: the gold star at x494,y44 is mounted on a castle and must be removed, along with all bunting, buildings and trees.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 POSITION LOCK. Keep tall Sunspire at x494,y44 on the LEFT and the smaller pale castle at x1130,y225 on the RIGHT. No central dome from image 2. Keep the countryside visible through the open gate at x735-855,y535-675 and continue its green fields, far trees and distant hills naturally behind all removed near objects down to y695. Do not retain the gate shape or door leaves: fill the hidden terrain without a cutout border. All sky is magenta, including above the far terrain inside the gate. Remove near masonry, towers, banners, bunting, trees, crystal, houses and floor. Keep the gold ornament on Sunspire only, not in the sky.'
                elif kind == 'mid':
                    prompt += '\n\nRemove the entire distant landscape through the central gate opening to magenta. Keep both open wooden gate leaves, masonry, near flowers, banners, bunting, near trees, crystal pedestal, houses and floor exactly unchanged. Remove distant castles too.'
                elif kind == 'fg':
                    prompt += '\n\nEntrance clearance: place small foreground objects only in x320-520, clear of the open gate x680-915, red door x1215-1280 and purple door x1430-1535. Keep the entire overlay silhouette outside those entrances.'
            if room == 'questeria/festival-square' and panel == 3:
                if kind == 'sky':
                    prompt += '\n\nNo actual sun in this panel. Remove the castle ornament at x468,y44 and all bunting. Keep image 1 clouds and sky only.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 GEOMETRY ONLY. Sunspire is at x468,y44 on the LEFT; smaller distant castle at x1000,y225; another far tower at x1445,y225. Keep all three exactly in place, not at the previous panel\'s positions. Join their existing foliage bases downward with distant cool blue-green wooded hills into the countryside band; no floating cutouts or magenta gaps below castles. Keep landscape through gate x648-744,y535-675 unchanged and extend it behind removed near objects down to y695. All sky and clouds are magenta. No near buildings, gate leaves/masonry, bunting, trees, crystal or floor. Keep castle ornaments attached to castles.'
                elif kind == 'mid':
                    prompt += '\n\nRemove the distant countryside through the open gate to magenta, retaining both open wooden leaves and near gate masonry. Remove all distant castles and hills. Keep nearby trees, crystal, houses, flowers, bunting, banners and the entire walking floor.'
                elif kind == 'fg':
                    prompt += '\n\nForeground entrance clearance: keep every overlay object in clear span x300-450, away from open gate x595-795, red door x1055-1135 and purple door x1230-1340. No part may cross these entrance spans. Keep each object slim, and retain bottom-edge anchoring.'
            if room == 'questeria/lantern-market' and panel == 2:
                if kind == 'sky':
                    prompt += '\n\nNo actual sun here. Remove the gold star and halo at x218,y38 with its castle. Remove all chimney smoke at upper right with the chimney. Keep only sunset sky bands and ordinary clouds from image 1.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 GEOMETRY: the only tall Sunspire is on the LEFT at x218,y38, not on the right as in image 2. Keep the distant city visible on the left, remove all near lantern strings, buildings, trees, chimney and smoke. Naturally continue the same distant city roofs, pale walls and cool distant foliage across the width and DOWN TO y680 with no gaps or floating cutout band. Sky above the city is magenta, bottom below y680 magenta. Do not copy image 2\'s right-hand castle or move any existing image 1 landmark.'
                elif kind == 'fg':
                    prompt += '\n\nDoor clearance: small foreground props may occupy the clear span x805-905 in front of the pink tree only, or the far-left stall area x130-280. Keep clear of doors x415-485, x675-745, x940-1055, x1220-1295 and x1450-1535. Place the whole silhouette within clear spans, not just its center. Keep ground props small and bottom-edge anchored.'
            if room == 'questeria/lantern-market' and panel == 3:
                if kind == 'sky':
                    prompt += '\n\nRemove the castle gold star/halo at x218,y38 and all chimney smoke. There is no actual sun in this panel. Keep image 1 ordinary sunset clouds and bands only.'
                elif kind == 'far':
                    prompt += '\n\nIMAGE 1 LANDMARKS: Sunspire on left at x218,y38, clifftop buildings at x620-860,y110-200 and far aqueduct at x800-900,y180-240 and x1390-1535,y180-240. Keep all visible far landmarks in exactly their own image 1 positions. Continue distant city roofs and cool blue-violet foliage down to y680 behind all removed near objects, with no floating cutout strip. Do not substitute image 2\'s skyline. No near trees, roofs, lanterns, smoke, chimney, shop, forge, bench, well or floor. Sky above skyline and area below y680 magenta.'
                elif kind == 'mid':
                    prompt += '\n\nKeep the forge chimney smoke from image 1 as a near element: it rises from x920,y160 and curves upward to the right out of the top edge around x1150. Remove ordinary sky clouds and distant city/aqueduct/castle only. Keep smoke, chimneys, lantern strings, near trees, buildings, doors, bench, well and floor unchanged.'
                elif kind == 'fg':
                    prompt += '\n\nDoor clearance: ground props may be entirely within clear span x600-700 in front of the pink tree or x900-990 in front of a window. Keep clear of doors x240-310, x500-565, x750-865, x1010-1090 and x1240-1345. Hanging foreground objects should stay within top200pixels and leave all entrances clear.'
            jobs.append(dict(room=room, title=title, outdoor=outdoor, panel=panel,
                             kind=kind, filename=filename, path=path,
                             references=references, prompt=prompt))
    # Follow the requested outdoor story order, then finish interiors after characters.
    jobs = [j for j in jobs if j['outdoor']] + [j for j in jobs if not j['outdoor']]
    assert len(jobs) == 265, len(jobs)
    assert len({j['path'] for j in jobs}) == 265
    for n, job in enumerate(jobs, 1):
        job['number'] = n
    overrides = read(DOCS/'prompt-additions.json') if (DOCS/'prompt-additions.json').exists() else {}
    for job in jobs:
        addition = overrides.get(job['room']+'/'+job['filename'])
        if addition:
            job['prompt'] += '\n\n'+addition
    write(DOCS / 'jobs.json', jobs)
    if not (DOCS / 'progress.json').exists():
        write(DOCS / 'progress.json', dict(images=[], next_number=1, runtime_imported=False))
    print(f'{len(jobs)} images; {sum(j["outdoor"] for j in jobs)} outdoor images')


def ready():
    state = read(DOCS / 'progress.json')
    n = state['next_number']
    if n is None:
        print('null')
        return
    job = read(DOCS / 'jobs.json')[n-1]
    for ref in job['references']:
        assert (ROOT / ref).exists(), ref
    print(json.dumps(job, ensure_ascii=False))


def save(number, source):
    state = read(DOCS / 'progress.json')
    assert number == state['next_number'], 'Out of order'
    jobs = read(DOCS / 'jobs.json')
    job = jobs[number-1]
    original = ROOT / 'art/pixel-batch-2-rooms/originals' / job['room'] / job['filename']
    dest = ROOT / job['path']
    assert not original.exists(), 'Already saved'
    original.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, original)
    with Image.open(source) as im:
        native = list(im.size)
        assert im.format == 'PNG'
        assert im.width * 2 == im.height * 3, f'Unexpected aspect ratio {im.size}; original retained for review'
        if dest.exists():
            # The brief explicitly preserves the batch-1 deck pilot.
            assert job['room'] == 'gulls-wager/deck' and job['panel'] == 1
            pilot = ROOT / 'art/pixel-batch-2-rooms/preserved-batch-1' / job['filename']
            assert not pilot.exists()
            pilot.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(dest, pilot)
        dest.parent.mkdir(parents=True, exist_ok=True)
        if native == [1536, 1024]:
            shutil.copyfile(source, dest)
        else:
            im.resize((1536, 1024), Image.Resampling.NEAREST).save(dest)
    prompt = DOCS / 'prompts' / job['room'] / (job['filename'] + '.txt')
    prompt.parent.mkdir(parents=True, exist_ok=True)
    prompt.write_text(job['prompt'] + '\n', encoding='utf-8')
    record = dict(number=number, path=job['path'], original=original.relative_to(ROOT).as_posix(),
                  native_size=native, export_size=[1536,1024],
                  source_sha256=digest(original), export_sha256=digest(dest),
                  references=[dict(path=r, sha256=digest(ROOT/r)) for r in job['references']],
                  prompt=prompt.relative_to(ROOT).as_posix(),
                  status='source draft; alignment, pixel conversion and runtime registration pending')
    state['images'].append(record)
    state['next_number'] = number+1 if number < len(jobs) else None
    write(DOCS / 'progress.json', state)
    with (ROOT / 'ASSETS.md').open('a', encoding='utf-8') as credits:
        credits.write(f'\n{job["path"]} - batch 2 room source, supplied by Caleb Kiatoukaysi, made in ChatGPT with built-in image generation. Prompt, originals and fingerprints retained in docs/pixel-art-batch-2-rooms and art/pixel-batch-2-rooms. Source draft awaiting alignment and pixel conversion; released for Legend Quest by its owner.\n')
    print(f'Saved {number}: {job["path"]}; next {state["next_number"]}')


def check():
    state = read(DOCS / 'progress.json')
    problems = []
    for entry in state['images']:
        for field, sha in [('path','export_sha256'), ('original','source_sha256')]:
            path = ROOT / entry[field]
            if not path.exists() or digest(path) != entry[sha]:
                problems.append(entry[field])
        for ref in entry['references']:
            if digest(ROOT/ref['path']) != ref['sha256']:
                problems.append(ref['path'])
        with Image.open(ROOT / entry['path']) as im:
            if im.size != (1536,1024):
                problems.append(entry['path'] + ' size')
    report = dict(saved=len(state['images']), total=265, problems=problems,
                  runtime_imported=False, alignment_accepted=False, pixel_conversion_accepted=False)
    write(DOCS/'source-check.json', report)
    print(json.dumps(report))
    assert not problems


def revise(number, source, prompt_file):
    state = read(DOCS/'progress.json')
    entry = next(r for r in state['images'] if r['number'] == number)
    assert not any(ref['path'] == entry['path'] for r in state['images'] for ref in r['references']), 'Revise before dependent generations'
    previous = dict(entry)
    revision = len(entry.get('earlier_attempts', []))+1
    dest = ROOT/entry['path']
    original = ROOT/'art/pixel-batch-2-rooms/revisions'/f'{number:03}-revision-{revision}.png'
    original.parent.mkdir(parents=True, exist_ok=True)
    assert not original.exists()
    shutil.copyfile(source, original)
    with Image.open(source) as im:
        assert im.size == (1536,1024), im.size
    shutil.copyfile(source, dest)
    prompt = DOCS/'revisions'/f'{number:03}-revision-{revision}.txt'
    prompt.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(prompt_file, prompt)
    entry.update(original=original.relative_to(ROOT).as_posix(), source_sha256=digest(original),
                 export_sha256=digest(dest), prompt=prompt.relative_to(ROOT).as_posix(),
                 earlier_attempts=entry.get('earlier_attempts', [])+[previous])
    write(DOCS/'progress.json', state)
    print(f'Revised {number}; prior attempt preserved')


def review(room):
    jobs = [j for j in read(DOCS/'jobs.json') if j['room'] == room]
    saved = {r['path'] for r in read(DOCS/'progress.json')['images']}
    jobs = [j for j in jobs if j['path'] in saved]
    sheet = Image.new('RGB', (320*5, 234*((len(jobs)+4)//5)), '#202026')
    draw = ImageDraw.Draw(sheet)
    stats = []
    for i, job in enumerate(jobs):
        with Image.open(ROOT/job['path']) as im:
            rgb = im.convert('RGB')
            x, y = i%5*320, i//5*234
            sheet.paste(rgb.resize((320,213)), (x,y+21))
            draw.text((x+4,y+4), job['filename'], fill='white')
            if job['kind'] in ('far','mid','fg'):
                colors = rgb.getcolors(rgb.width*rgb.height)
                count, color = max(colors)
                stats.append(dict(path=job['path'], dominant_color=list(color),
                                  dominant_fraction=count/(rgb.width*rgb.height),
                                  exact_magenta_pixels=sum(n for n,c in colors if c==(255,0,255))))
    target = ROOT/'art/pixel-batch-2-rooms/review'/room
    target.mkdir(parents=True, exist_ok=True)
    sheet.save(target/'contact.jpg', quality=94)
    write(target/'key-colour-check.json', stats)
    print(target/'contact.jpg')
    print(json.dumps(stats))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('command', choices=['plan','ready','save','check','review','revise'])
    parser.add_argument('args', nargs='*')
    args = parser.parse_args()
    if args.command == 'save':
        save(int(args.args[0]), args.args[1])
    elif args.command == 'review':
        review(args.args[0])
    elif args.command == 'revise':
        revise(int(args.args[0]), args.args[1], args.args[2])
    else:
        globals()[args.command]()
