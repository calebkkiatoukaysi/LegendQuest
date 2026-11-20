"""A soft target ellipse, sampled linearly at screen scale.

Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""
from pathlib import Path
import numpy as np
from PIL import Image


def main():
    width, height = 512, 256
    yy, xx = np.mgrid[:height, :width]
    radius = np.sqrt(((xx + .5 - width/2)/(width/2))**2 + ((yy + .5 - height/2)/(height/2))**2)
    alpha = np.clip((.045 - np.abs(radius - .9)) * (height/2) + .5, 0, 1)
    rgba = np.full((height, width, 4), 255, np.uint8)
    rgba[:, :, 3] = np.rint(alpha * 255).astype(np.uint8)
    output = Path(__file__).resolve().parent.parent / 'Content/target-ring.png'
    Image.fromarray(rgba).save(output)
    print(output)


if __name__ == '__main__':
    main()
