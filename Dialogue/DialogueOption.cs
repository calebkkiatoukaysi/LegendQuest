using System.Collections.Generic;

namespace LegendQuest.Dialogue;

/// <summary>
/// One thing you can say when a line has options: what it says, when it's there, what it does, and
/// where it goes.
/// </summary>
/// <remarks>
/// An option whose conditions aren't met is hidden, unless it's Locked, when it's shown greyed with
/// what it needs, so you can see there was something there. A check rolls on screen and goes to
/// OnSuccess or OnFailure; anything else goes to Next, or on to the line after the question if
/// Next is empty, which is how the script's "rejoin" works.
/// </remarks>
public sealed class DialogueOption
{
    /// <summary>What you say. May hold {name} and the other tokens.</summary>
    public required string Text { get; init; }

    /// <summary>What has to be true for it to be open.</summary>
    public IReadOnlyList<DialogueCondition> Conditions { get; init; } = new DialogueCondition[0];

    /// <summary>Whether it's shown greyed when its conditions aren't met, instead of hidden.</summary>
    public bool Locked { get; init; }

    /// <summary>Whether it's a flirt. It gets a heart beside it.</summary>
    public bool Flirt { get; init; }

    /// <summary>What it does when it's picked, before anything else.</summary>
    public IReadOnlyList<DialogueEffect> Effects { get; init; } = new DialogueEffect[0];

    /// <summary>The check it rolls, or null for none.</summary>
    public DialogueCheck Check { get; init; }

    /// <summary>Where it goes: a node's id, DialogueScript.End, or null for the line after the question.</summary>
    public string Next { get; init; }

    /// <summary>Where it goes when its check passes.</summary>
    public string OnSuccess { get; init; }

    /// <summary>Where it goes when its check fails.</summary>
    public string OnFailure { get; init; }
}
