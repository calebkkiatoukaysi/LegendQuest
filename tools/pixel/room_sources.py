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
            path = f'tools/source/pixel/rooms/{room}/{filename}'
            reference = (f'art/rooms/generated/{room}/bg-{panel}.png' if kind == 'panel'
                         else f'tools/source/pixel/rooms/{room}/bg-{panel}.png')
            references = [reference]
            if panel > 1:
                previous = filename.replace(f'bg-{panel}', f'bg-{panel-1}')
                references.append(f'tools/source/pixel/rooms/{room}/{previous}')
                prompt += '\n\nImage 1 is the edit target / scene reference. Image 2 is the finished same-kind preceding panel. Where this panel shows the same stretch as the second attached picture, make it match that one exactly. Do not move anything in image 1 to imitate image 2.'
            jobs.append(dict(room=room, title=title, outdoor=outdoor, panel=panel,
                             kind=kind, filename=filename, path=path,
                             references=references, prompt=prompt))
    # Follow the requested outdoor story order, then finish interiors after characters.
    jobs = [j for j in jobs if j['outdoor']] + [j for j in jobs if not j['outdoor']]
    assert len(jobs) == 265, len(jobs)
    assert len({j['path'] for j in jobs}) == 265
    for n, job in enumerate(jobs, 1):
        job['number'] = n
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
    parser.add_argument('command', choices=['plan','ready','save','check','review'])
    parser.add_argument('args', nargs='*')
    args = parser.parse_args()
    if args.command == 'save':
        save(int(args.args[0]), args.args[1])
    elif args.command == 'review':
        review(args.args[0])
    else:
        globals()[args.command]()
