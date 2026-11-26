"""Prepare the batch-2 repaint inventory without generating or changing images.

Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""
import hashlib
import json
import re
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
DOCS = ROOT / 'docs/pixel-art-batch-2-repaints'


def main():
    brief = ROOT / 'docs/pixel-art-batch-2-for-ChatGPT.md'
    part = brief.read_text(encoding='utf-8').split('## Part 2.', 1)[1].split('## Part 3.', 1)[0]
    groups = re.split(r'^### (.+)$', part, flags=re.M)
    jobs, problems = [], []
    for index in range(1, len(groups), 2):
        title, body = groups[index:index + 2]
        listing, prompt_block = body.split('```text', 1)
        prompt = prompt_block.split('```', 1)[0].strip()
        size = re.search(r'exactly (\d+) by (\d+) pixels', prompt)
        expected = [int(size[1]), int(size[2])]
        paths = re.findall(r'`([^`]+\.png)`', listing)
        count = int(re.search(r'\((\d+)\)', title)[1])
        assert len(paths) == count, (title, len(paths), count)
        for relative in paths:
            source = ROOT / 'tools/source/characters' / relative
            target = ROOT / 'tools/source/pixel/repaints' / relative
            job = {
                'inventory_number': len(jobs) + 1,
                'group': title.strip(),
                'source': source.relative_to(ROOT).as_posix(),
                'destination': target.relative_to(ROOT).as_posix(),
                'requested_size': expected,
                'prompt': prompt,
                'status': 'not generated',
            }
            if not source.is_file():
                problems.append({'source': job['source'], 'problem': 'missing source'})
            else:
                job['source_sha256'] = hashlib.sha256(source.read_bytes()).hexdigest()
                with Image.open(source) as image:
                    job['source_size'] = list(image.size)
                if job['source_size'] != expected:
                    problems.append({'source': job['source'], 'problem': 'source/brief dimension mismatch',
                                     'actual': job['source_size'], 'requested': expected})
            job['destination_already_exists'] = target.exists()
            jobs.append(job)
    assert len(jobs) == 204
    assert len({job['destination'] for job in jobs}) == 204
    DOCS.mkdir(parents=True, exist_ok=True)
    for name, value in [('jobs.json', jobs), ('source-audit.json', {
        'total': len(jobs), 'problems': problems,
        'generated': 0, 'runtime_imported': False,
        'brief_sha256': hashlib.sha256(brief.read_bytes()).hexdigest(),
        'order': 'After outdoor room 195: bodies first, then party and Prologue armours; remaining groups follow.',
        'inventory_numbers_are_execution_order': False,
    })]:
        (DOCS / name).write_text(json.dumps(value, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')
    print(json.dumps({'jobs': len(jobs), 'source_problems': problems,
                      'existing_destinations': sum(job['destination_already_exists'] for job in jobs)}))


if __name__ == '__main__':
    main()
