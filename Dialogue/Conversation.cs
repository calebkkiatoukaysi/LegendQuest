using System.Collections.Generic;

namespace LegendQuest.Dialogue;

/// <summary>
/// One conversation: who it's with, when it can happen, what gets said, and what it changes once
/// it's over.
/// </summary>
/// <remarks>
/// The first shape of the BG3-style conversations to come. There are no choices yet, just lines one
/// after another, but it's data the same way they'll be: Conversations has every one, and talking
/// to someone picks the first one of theirs whose flags are right. The choices, the checks with the
/// dice and the approval go on this when dialogue gets its milestone.
/// </remarks>
public sealed class Conversation
{
    /// <summary>What it's called. Triggers start one by this.</summary>
    public required string Id { get; init; }

    /// <summary>The id of the person on the map it's with, or null if only a trigger or the arrival starts it.</summary>
    public string With { get; init; }

    /// <summary>Flags that have to be set for it to happen.</summary>
    public IReadOnlyList<string> Needs { get; init; } = new string[0];

    /// <summary>Flags that stop it happening if they're set. A conversation that sets one of these only happens once.</summary>
    public IReadOnlyList<string> Unless { get; init; } = new string[0];

    /// <summary>What gets said, in order.</summary>
    public required IReadOnlyList<Line> Lines { get; init; }

    /// <summary>Flags set once it's over.</summary>
    public IReadOnlyList<string> Sets { get; init; } = new string[0];

    /// <summary>What you're to do next once it's over, or null to leave it as it was.</summary>
    public string Objective { get; init; }

    /// <summary>Gold handed over once it's over.</summary>
    public int Gold { get; init; }

    /// <summary>Whether it can happen with these flags set.</summary>
    /// <param name="flags">The flags.</param>
    public bool CanHappen(ISet<string> flags)
    {
        foreach (string flag in Needs)
            if (!flags.Contains(flag)) return false;
        foreach (string flag in Unless)
            if (flags.Contains(flag)) return false;
        return true;
    }
}

/// <summary>One line of a conversation.</summary>
/// <param name="Speaker">Who says it: you, someone in the party by their id, or someone on the map by theirs.</param>
/// <param name="Text">What they say. {name} is your name.</param>
public sealed record Line(string Speaker, string Text);
