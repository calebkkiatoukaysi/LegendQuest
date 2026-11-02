using Microsoft.Xna.Framework;

namespace LegendQuest.Combat;

/// <summary>
/// A fight's ground with nothing on it: every tile can be stood on and seen across. What the
/// simulator fights on.
/// </summary>
/// <param name="Width">How many tiles across.</param>
/// <param name="Height">How many tiles down.</param>
public sealed record OpenGround(int Width, int Height) : ICombatGrid
{
    /// <summary>Whether a tile is on the ground at all.</summary>
    /// <param name="cell">The tile.</param>
    public bool Walkable(Point cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;

    /// <summary>Never: there's nothing to hide behind.</summary>
    /// <param name="cell">The tile.</param>
    public bool BlocksSight(Point cell) => false;
}
