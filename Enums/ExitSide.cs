namespace LegendQuest;

/// <summary>
/// Where a room's way out is: off one end of the floor, or up or down a stair standing somewhere along it.
/// </summary>
public enum ExitSide
{
    /// <summary>Off the left end of the room.</summary>
    West,

    /// <summary>Off the right end of the room.</summary>
    East,

    /// <summary>Up a stair, at an x along the floor.</summary>
    Up,

    /// <summary>Down a stair, at an x along the floor.</summary>
    Down,
}
