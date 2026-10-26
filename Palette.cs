using Microsoft.Xna.Framework;

namespace LegendQuest;

/// <summary>
/// Colours and shadows more than one screen uses. Anything only one class uses stays there.
/// </summary>
public static class Palette
{
    /// <summary>How far every text shadow sits down and right of its text.</summary>
    public static readonly Vector2 ShadowOffset = new(2f, 2f);

    /// <summary>The usual text shadow.</summary>
    public static readonly Color Shadow = Color.Black * 0.6f;

    /// <summary>A bit heavier, for the forms over the title screen.</summary>
    public static readonly Color FormShadow = Color.Black * 0.7f;

    /// <summary>Heavier still, under button labels, which sit on lit plates.</summary>
    public static readonly Color LabelShadow = Color.Black * 0.75f;

    /// <summary>The dark the forms veil the screen with. Tinted, not flat black, so the gate dims instead of going grey.</summary>
    public static readonly Color Veil = new(8, 6, 6);

    /// <summary>The heading on a form.</summary>
    public static readonly Color Heading = new(220, 208, 192);

    /// <summary>The line under a form's heading that says how it works.</summary>
    public static readonly Color Note = new(138, 126, 118);

    /// <summary>Torchlight, for the way forward: starting, carrying on, resuming.</summary>
    public static readonly Color Torch = new(232, 150, 58);

    /// <summary>A dying ember, for anything that ends something or can't be taken back: leaving, erasing.</summary>
    public static readonly Color Ember = new(214, 64, 42);

    /// <summary>Cold bone, for the choices that don't commit you to anything.</summary>
    public static readonly Color Bone = new(222, 212, 198);
}
