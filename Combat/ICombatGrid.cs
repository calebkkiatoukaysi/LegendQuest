using Microsoft.Xna.Framework;

namespace LegendQuest.Combat;

/// <summary>
/// The ground a fight is on, one tile at a time: where can be stood on, and what can't be seen
/// through.
/// </summary>
/// <remarks>
/// An interface so the engine never touches a room: the explore screen hands it one laid over the
/// room's floor (RoomCombatGrid), and the simulator one that's open ground. Who's standing where is
/// the engine's business, not the grid's.
/// </remarks>
public interface ICombatGrid
{
    /// <summary>How many tiles across.</summary>
    int Width { get; }

    /// <summary>How many tiles down.</summary>
    int Height { get; }

    /// <summary>Whether a tile can be stood on. Off the grid can't.</summary>
    /// <param name="cell">The tile.</param>
    bool Walkable(Point cell);

    /// <summary>Whether a tile stops a line of sight through it: a wall, not a crate.</summary>
    /// <param name="cell">The tile.</param>
    bool BlocksSight(Point cell);
}
