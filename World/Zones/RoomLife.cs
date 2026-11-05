using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// Something small that moves in a room on its own, in a box: gulls, clouds, chimney smoke, a flag,
/// water shimmer, leaves, motes, ash or candle flicker (12-rooms §3.2).
/// </summary>
/// <param name="Kind">Which: gulls, clouds, smoke, flag, shimmer, leaves, motes, ash or candle.</param>
/// <param name="Area">Where it happens, in world pixels.</param>
public sealed record RoomLife(string Kind, BoundingRectangle Area);
