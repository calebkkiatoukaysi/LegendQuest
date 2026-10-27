namespace LegendQuest;

/// <summary>
/// Which cursor shows. A screen sets it every frame for whatever the mouse is over; nothing set means Default.
/// </summary>
/// <remarks>The order is the order of the frames in cursors.png.</remarks>
public enum CursorShape
{
    /// <summary>An arrow: menus, and anything there's nothing to do with.</summary>
    Default,

    /// <summary>A ring: walk here.</summary>
    Move,

    /// <summary>A hand: open, pull, read, travel.</summary>
    Use,

    /// <summary>A speech bubble: talk to them.</summary>
    Talk,

    /// <summary>A crossed-out circle: there's no way there.</summary>
    Invalid,
}
