using System;
using System.Collections.Generic;

namespace LegendQuest.Story;

/// <summary>
/// A few lines said in bubbles while you keep walking: the party talking about where they've got
/// to, or two people on the map talking to each other. When it can play, by the flags.
/// </summary>
/// <remarks>
/// A map's trigger names one with bark=, and walking into it plays the lines one after another
/// without stopping you. Nothing in it changes the game; the flags only say whether it's the
/// right time for it, so the same spot can have one thing said before the Red Gulls and another
/// after.
/// </remarks>
public sealed class Banter
{
    /// <summary>What it's called. A map's trigger names it by this.</summary>
    public required string Id { get; init; }

    /// <summary>Every line, in the order they're said.</summary>
    public required IReadOnlyList<BanterLine> Lines { get; init; }

    /// <summary>Flags that all have to be set for it to play.</summary>
    public IReadOnlyList<string> Needs { get; init; } = Array.Empty<string>();

    /// <summary>Flags that stop it playing if any of them are set.</summary>
    public IReadOnlyList<string> Unless { get; init; } = Array.Empty<string>();

    /// <summary>Whether it can play with these flags set: every one of Needs, and none of Unless.</summary>
    /// <param name="flags">The game's flags.</param>
    public bool CanPlay(ISet<string> flags)
    {
        foreach (string flag in Needs)
            if (!flags.Contains(flag)) return false;
        foreach (string flag in Unless)
            if (flags.Contains(flag)) return false;
        return true;
    }
}
