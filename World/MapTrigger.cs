using LegendQuest.Collisions;

namespace LegendQuest.World;

/// <summary>
/// A rectangle on a map that does something when you walk into it.
/// </summary>
/// <remarks>
/// Saying something, setting a flag, starting a conversation, or starting a banter the party has
/// while you keep walking. Starting a fight hangs off the same thing later, when there are fights.
/// </remarks>
/// <param name="Id">What the map file calls it. A trigger that only goes off once is saved as used by this.</param>
/// <param name="Area">The rectangle, in world pixels.</param>
/// <param name="Once">True if it only ever goes off once; false if it goes off every time you walk in.</param>
/// <param name="Say">A line to show, or null.</param>
/// <param name="SetFlag">A flag to set, or null.</param>
/// <param name="Talk">A conversation to start, by its id in DialogueCatalog, or null.</param>
/// <param name="Bark">A banter to play over people's heads, by its id in Banters, or null.</param>
/// <param name="Mark">Whether a red "!" floats over it while it's there and hasn't gone off, for the important ones.</param>
/// <param name="Presence">When it's there.</param>
public sealed record MapTrigger(string Id, BoundingRectangle Area, bool Once, string Say, string SetFlag, string Talk, string Bark, bool Mark,
    MapPresence Presence)
{
    /// <summary>Whether all it does is start a banter. One of those waits, unused, until its banter can play.</summary>
    public bool OnlyBarks => Bark is not null && Say is null && SetFlag is null && Talk is null;
}
