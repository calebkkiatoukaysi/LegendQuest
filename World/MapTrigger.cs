using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A rectangle on a map that does something when you walk into it.
/// </summary>
/// <remarks>
/// For now that's saying something and setting a flag. Starting a conversation or a fight hangs off
/// the same thing later, when there are conversations and fights.
/// </remarks>
/// <param name="Id">What the map file calls it. A trigger that only goes off once is saved as used by this.</param>
/// <param name="Area">The rectangle, in world pixels.</param>
/// <param name="Once">True if it only ever goes off once; false if it goes off every time you walk in.</param>
/// <param name="Say">A line to show, or null.</param>
/// <param name="SetFlag">A flag to set, or null.</param>
public sealed record MapTrigger(string Id, BoundingRectangle Area, bool Once, string Say, string SetFlag);
