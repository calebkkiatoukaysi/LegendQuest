namespace LegendQuest;

/// <summary>
/// The kinds of thing on a map you can walk up to and use. Map files name them in [objects].
/// </summary>
public enum InteractableKind
{
    /// <summary>Opens once and gives up what's in it.</summary>
    Chest,

    /// <summary>Opens and stays open, unless it's locked or something else works it.</summary>
    Door,

    /// <summary>Pulled one way or the other, which sets or clears a flag other things read.</summary>
    Lever,

    /// <summary>Has something written on it.</summary>
    Sign,

    /// <summary>A stone you can travel between, once you've found it.</summary>
    Waypoint,
}
