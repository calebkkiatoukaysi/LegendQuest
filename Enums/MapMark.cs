namespace LegendQuest;

/// <summary>
/// The marks on map-marks.png, in the order they're drawn on it.
/// </summary>
/// <remarks>The map screen uses all of them; the map you're walking on borrows the dot and the ring for the path you're about to take.</remarks>
public enum MapMark
{
    /// <summary>A place you've been, on the world map.</summary>
    Place,

    /// <summary>A place you know of but haven't been to.</summary>
    Unknown,

    /// <summary>The ring round where you are, or round where a path ends.</summary>
    Here,

    /// <summary>A waypoint stone.</summary>
    Waypoint,

    /// <summary>The brackets round the place picked to travel to.</summary>
    Selected,

    /// <summary>You, on the local map.</summary>
    You,

    /// <summary>One dot of a road, or of a path.</summary>
    Dot,
}
