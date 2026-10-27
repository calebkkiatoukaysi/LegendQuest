using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A way off a map: walk into its rectangle and you go to another map.
/// </summary>
/// <param name="Id">What the map file calls it.</param>
/// <param name="Area">The rectangle you walk into, in world pixels.</param>
/// <param name="ToMap">The map it goes to.</param>
/// <param name="ToSpawn">The spawn on that map you arrive at.</param>
public sealed record MapExit(string Id, BoundingRectangle Area, string ToMap, string ToSpawn);
