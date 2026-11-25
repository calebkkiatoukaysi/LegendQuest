using Microsoft.Xna.Framework;

namespace LegendQuest;

/// <summary>
/// Colours and shadows more than one screen uses. Anything only one class uses stays there.
/// </summary>
/// <remarks>
/// The UI's look is the title painting's: cream marble and gold round a periwinkle enamel, sky
/// gems, and plum where the old look had black, for the outlines, the shadows and the veils.
/// tools/ui_theme.py has the same colours for the art the scripts draw (the panel, the buttons,
/// the slots and the hotbar).
/// </remarks>
public static class Palette
{
    /// <summary>How far every text shadow sits down and right of its text.</summary>
    public static readonly Vector2 ShadowOffset = new(2f, 2f);

    /// <summary>The plum the UI uses where black would go: outlines, shadows, the veils.</summary>
    public static readonly Color Plum = new(58, 34, 82);

    /// <summary>The usual text shadow: deep plum, so it softens rather than cuts.</summary>
    public static readonly Color Shadow = new Color(30, 14, 50) * 0.6f;

    /// <summary>A bit heavier, for the forms over the title screen.</summary>
    public static readonly Color FormShadow = new Color(30, 14, 50) * 0.7f;

    /// <summary>Heavier still, under button labels, which sit on lit plates.</summary>
    public static readonly Color LabelShadow = new Color(30, 14, 50) * 0.75f;

    /// <summary>What the forms veil the screen with: a twilight violet, so the room behind dims to dusk instead of going dark.</summary>
    public static readonly Color Veil = new(44, 30, 82);

    /// <summary>The enamel the panels are filled with (tools/ui_theme.py's ENAMEL), a deeper one, and its light lip.</summary>
    public static readonly Color Enamel = new(86, 80, 168);
    public static readonly Color EnamelDeep = new(48, 42, 108);
    public static readonly Color EnamelLight = new(128, 122, 210);

    /// <summary>The cream marble and the gold the frames are made of.</summary>
    public static readonly Color Marble = new(244, 232, 212);
    public static readonly Color Trim = new(246, 198, 96);

    /// <summary>The heading on a form.</summary>
    public static readonly Color Heading = new(255, 246, 226);

    /// <summary>The line under a form's heading that says how it works: a soft lavender.</summary>
    public static readonly Color Note = new(206, 198, 240);

    /// <summary>Sunlight, for the way forward: starting, carrying on, resuming.</summary>
    public static readonly Color Torch = new(255, 196, 92);

    /// <summary>A rose red, for anything that ends something or can't be taken back: leaving, erasing.</summary>
    public static readonly Color Ember = new(250, 112, 128);

    /// <summary>Cream, for the choices that don't commit you to anything.</summary>
    public static readonly Color Bone = new(252, 246, 236);

    /// <summary>The narrator's lines: a little cooler than anyone speaking.</summary>
    public static readonly Color Narration = new(216, 228, 255);

    /// <summary>The gold of the Soul-Bond, a pass, and anything royal.</summary>
    public static readonly Color Gold = new(255, 214, 108);

    /// <summary>Dark writing on something pale, like the number on a die.</summary>
    public static readonly Color Ink = new(34, 26, 48);
}
