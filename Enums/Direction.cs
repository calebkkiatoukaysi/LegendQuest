namespace LegendQuest;

/// <summary>
/// Which way someone is facing. The value is the row on their sheet.
/// </summary>
/// <remarks>Same order as The Black Box's, so the sheets lay out the same way: down, left, right, up.</remarks>
public enum Direction
{
    /// <summary>Facing the screen.</summary>
    Down,

    /// <summary>Facing left.</summary>
    Left,

    /// <summary>Facing right.</summary>
    Right,

    /// <summary>Facing away from the screen.</summary>
    Up,
}
