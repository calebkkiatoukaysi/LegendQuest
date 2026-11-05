using Microsoft.Xna.Framework;
using LegendQuest.Collisions;
using LegendQuest.Combat;

namespace LegendQuest.World;

/// <summary>
/// The ground a fight in a room is fought on, as the explore screen needs it: the engine's grid, and
/// where each of its cells is in the room.
/// </summary>
/// <remarks>
/// The engine only ever sees cells, through ICombatGrid. The screen has to go between cells and the
/// world as well: which cell someone's feet or the mouse are in, where feet go to stand on a cell,
/// and the patch of floor a cell covers, to light the ones you can reach. On the tile maps a cell was
/// a tile and the map answered all three; a room's floor has to be asked. RoomCombatGrid, M3's
/// interim ground, is the one there is; M4's Formation replaces them both.
/// </remarks>
public interface IFightGround : ICombatGrid
{
    /// <summary>Which cell a point is in. A point off the ground gives a cell off it, and the engine starts anyone given one on the nearest free cell.</summary>
    /// <param name="world">The point, in world pixels.</param>
    Point CellOf(Vector2 world);

    /// <summary>Where someone's feet go to stand on a cell: the middle of it, on the floor.</summary>
    /// <param name="cell">The cell.</param>
    Vector2 FeetOn(Point cell);

    /// <summary>The patch of floor a cell covers, in world pixels.</summary>
    /// <param name="cell">The cell.</param>
    BoundingRectangle Area(Point cell);
}
