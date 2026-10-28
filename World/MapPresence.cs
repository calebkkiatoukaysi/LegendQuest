using System.Collections.Generic;

namespace LegendQuest.World;

/// <summary>
/// When something on a map is there: only once a flag's set, only until one is, or always.
/// </summary>
/// <remarks>
/// It's how one map shows the story going on: the Red Gulls on the docks till they're beaten, the
/// Watch only after. The same map file, instead of a copy of the map for every state of it.
/// </remarks>
/// <param name="When">A flag that has to be set for it to be there, or null.</param>
/// <param name="Unless">A flag that makes it go once it's set, or null.</param>
public readonly record struct MapPresence(string When, string Unless)
{
    /// <summary>Always there.</summary>
    public static MapPresence Always => new(null, null);

    /// <summary>Whether it's there with these flags set.</summary>
    /// <param name="flags">The game's flags.</param>
    public bool Holds(ISet<string> flags) =>
        (When is null || flags.Contains(When)) && (Unless is null || !flags.Contains(Unless));
}
