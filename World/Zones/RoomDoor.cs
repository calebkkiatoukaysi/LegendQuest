using Microsoft.Xna.Framework;
using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A door, gate, arch or stair in a room's painting: a box standing on the back of the floor, with a
/// badge over it saying what's through it.
/// </summary>
/// <remarks>Where it goes works as a RoomExit's does: another room or zone, Back, or nowhere while it's locked.</remarks>
/// <param name="Id">What the zone file calls it, unique in its zone.</param>
/// <param name="Area">Its box in world pixels, its bottom on the floor's back edge.</param>
/// <param name="Badge">The badge over it.</param>
/// <param name="Name">What its plaque says.</param>
/// <param name="ToZone">The zone it goes to (this one, for another room in it), RoomExit.Back, or null while it's locked.</param>
/// <param name="ToRoom">The room it goes to, or null for Back and for a locked one.</param>
/// <param name="ToSpawn">The spawn it puts you on there, or null for Back and for a locked one.</param>
/// <param name="LockedWhy">Why it won't open, or null if it will.</param>
/// <param name="Presence">When it's there.</param>
public sealed record RoomDoor(string Id, BoundingRectangle Area, BadgeKind Badge, string Name,
    string ToZone, string ToRoom, string ToSpawn, string LockedWhy, MapPresence Presence)
{
    /// <summary>The middle of its bottom edge: where you stand to go through it.</summary>
    public Vector2 Foot => new(Area.X + Area.Width / 2f, Area.Bottom);
}
