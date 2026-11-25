"""
The UI's colours, shared by the scripts that draw it (generate_ui.py's buttons and panel,
generate_items.py's slot wells, generate_combat_art.py's hotbar and portrait frames), so they all
come out of one palette. Palette.cs has the same colours for what the game draws itself.

The look is the title painting's: cream marble with gold trim, sky-blue gems like the banners'
diamonds, a periwinkle-violet enamel to fill things in (light enough to be bright, dark enough
that cream writing reads on it), and a plum outline instead of black, so the frames sit soft
against the painted rooms instead of cutting holes in them.

Not run on its own. Written by Caleb Kiatoukaysi for Legend Quest; public domain.
"""

#: The outline round everything: a deep plum, never black.
OUTLINE = (58, 34, 82)

#: Cream marble, from its lit edge to its shade.
MARBLE_HI = (255, 251, 240)
MARBLE = (244, 232, 212)
MARBLE_SH = (216, 196, 182)
MARBLE_DK = (166, 136, 152)

#: The gold trim.
GOLD_HI = (255, 238, 168)
GOLD = (246, 198, 96)
GOLD_DK = (196, 132, 58)

#: The enamel panels and wells are filled with, from its light lip to its deepest shadow.
ENAMEL_HI = (128, 122, 210)
ENAMEL = (86, 80, 168)
ENAMEL_DK = (66, 60, 140)
ENAMEL_DEEP = (48, 42, 108)

#: The banner blue the buttons are enamelled in, lit and shaded.
BANNER_HI = (138, 170, 244)
BANNER = (84, 110, 210)
BANNER_DK = (56, 74, 160)

#: The gems in the corners: shadow, colour, glint.
GEM_SKY = ((44, 104, 186), (104, 190, 246), (232, 250, 255))
GEM_ROSE = ((176, 58, 108), (244, 126, 168), (255, 226, 236))
