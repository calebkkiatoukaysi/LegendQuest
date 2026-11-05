namespace LegendQuest.World;

/// <summary>
/// A way out of a room that isn't a door: off one end of the floor, or up or down a stair.
/// </summary>
/// <remarks>
/// It goes to a spawn in another room of the zone, a spawn in another zone, back where you came in
/// (ToZone is Back), or nowhere while it's locked, which is when LockedWhy is set. A locked one is
/// usually the twin of an open one, there unless the flag the open one waits for is set.
/// </remarks>
/// <param name="Id">What the zone file calls it, unique in its zone.</param>
/// <param name="Side">Which end it's off, or which way the stair goes.</param>
/// <param name="X">Where it is along the room, in world pixels: 0 or the room's width for an end, the stair's foot for a stair.</param>
/// <param name="ToZone">The zone it goes to (this one, for another room in it), Back, or null while it's locked.</param>
/// <param name="ToRoom">The room it goes to, or null for Back and for a locked one.</param>
/// <param name="ToSpawn">The spawn it puts you on there, or null for Back and for a locked one.</param>
/// <param name="Label">What its label says, or null for the name of where it goes.</param>
/// <param name="LockedWhy">Why it won't let you through, or null if it will.</param>
/// <param name="Travel">Whether it opens the world map instead of going straight there.</param>
/// <param name="Presence">When it's there.</param>
public sealed record RoomExit(string Id, ExitSide Side, float X, string ToZone, string ToRoom, string ToSpawn,
    string Label, string LockedWhy, bool Travel, MapPresence Presence)
{
    /// <summary>ToZone for a way out that goes back where you came in, which is how a building's way out works.</summary>
    public const string Back = "back";

    /// <summary>How near its end of the room an edge exit goes off, in world pixels (48 canvas pixels).</summary>
    public const float EdgeReach = 12f;
}
