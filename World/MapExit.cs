using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A way off a map: walk into its rectangle and you go to another map.
/// </summary>
/// <param name="Id">What the map file calls it.</param>
/// <param name="Area">The rectangle you walk into, in world pixels.</param>
/// <param name="ToMap">The map it goes to.</param>
/// <param name="ToSpawn">The spawn on that map you arrive at.</param>
/// <param name="Presence">When it's there.</param>
/// <param name="Name">What it's called, for an entrance into a building, or null.</param>
public sealed record MapExit(string Id, BoundingRectangle Area, string ToMap, string ToSpawn, MapPresence Presence, string Name = null)
{
    /// <summary>Where an exit out of a building goes: back to wherever you came in, outside the door you came in by.</summary>
    public const string Back = "back";
}
