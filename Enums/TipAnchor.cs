namespace LegendQuest;

/// <summary>
/// Where on the screen a tutorial tip's card goes, so it sits by what it's about and clear of everything else.
/// </summary>
/// <remarks>Saved nowhere, so the order doesn't matter. TutorialPanel has the numbers for each.</remarks>
public enum TipAnchor
{
    /// <summary>The bottom right corner, clear of the party's cards. Most tips go here.</summary>
    BottomRight,

    /// <summary>Under what you're to do next, top left, so it points at that line.</summary>
    TopLeft,

    /// <summary>Under the keys along the top right, where M and Esc are written.</summary>
    TopRight,

    /// <summary>Just over the party's cards, bottom left.</summary>
    AboveStrip,

    /// <summary>The right edge, halfway down, clear of a fight's turn order along the top and its hotbar along the bottom.</summary>
    Right,

    /// <summary>The middle of the screen, for a tip that holds everything until it's read.</summary>
    Centre,
}
