using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendQuest.Story;

/// <summary>
/// One of the story's paintings: its layers, back to front, and what moves over them.
/// </summary>
/// <remarks>
/// Everything's in art pixels on the paintings' canvas (416x277, drawn at 4x, the middle 400x225 on
/// screen). Something drawn "on" a layer is drawn just over it and sways with it, so whatever's on
/// the layers in front still covers it. Anything a painting doesn't have is left at its default.
/// </remarks>
public sealed class Painting
{
    /// <summary>The layers, back to front: the end of each file's name (Content/Paintings/(id)-(layer).png), and how much of the sway it takes.</summary>
    public required IReadOnlyList<(string Layer, float Depth)> Layers { get; init; }

    /// <summary>How many raindrops are falling at once, over everything. None if 0.</summary>
    public int Rain { get; init; }

    /// <summary>Where lightning comes down, on the first layer (the sky): its bolts' tops are somewhere along the top edge of this. Empty for none.</summary>
    public Rectangle Lightning { get; init; }

    /// <summary>The water that glints, and the layer it's on. Empty for none.</summary>
    public Rectangle Water { get; init; }

    /// <summary>The layer the water that glints is on.</summary>
    public string WaterLayer { get; init; }

    /// <summary>How many glints there are on the water at once.</summary>
    public int Glints { get; init; }

    /// <summary>The colour the glints are tinted, the light they're reflecting.</summary>
    public Color GlintColour { get; init; } = Color.White;

    /// <summary>Where the feet of the two of you go, standing with your backs to us: you, then the Soul-Bound. None if empty.</summary>
    public IReadOnlyList<Vector2> Figures { get; init; } = Array.Empty<Vector2>();

    /// <summary>The layer the two of you stand on.</summary>
    public string FiguresLayer { get; init; }

    /// <summary>Where the light behind the two of you is, which edges your silhouettes.</summary>
    public Vector2 Backlight { get; init; }

    /// <summary>The colour of the edge the light puts on your silhouettes.</summary>
    public Color Rim { get; init; }

    /// <summary>The colour of your silhouettes.</summary>
    public Color Shadow { get; init; }

    /// <summary>Where the Soul-Bond's mark glows, where your hands meet, or null for none. It's on the figures' layer.</summary>
    public Vector2? Sigil { get; init; }

    /// <summary>The hook a lantern swings from, or null for none.</summary>
    public Vector2? Lantern { get; init; }

    /// <summary>The layer the lantern hangs on.</summary>
    public string LanternLayer { get; init; }
}
