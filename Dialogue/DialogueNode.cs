using System.Collections.Generic;

namespace LegendQuest.Dialogue;

/// <summary>
/// One line of a conversation: who says it, what they say, how, what it does, and the options
/// under it if it's a question.
/// </summary>
/// <remarks>
/// A line with no options goes to Next, or to the next line in the script if Next is empty, so a
/// run of lines reads in order without every one naming the next. A line whose conditions aren't
/// met is skipped the same way; that's how the lines only one voice says work. The narrator and
/// cards are speakers too (see Speakers): the narrator has no face, and a card is the line big in
/// the middle of the screen.
/// </remarks>
public sealed class DialogueNode
{
    /// <summary>What options and other lines call it, or null if nothing points at it.</summary>
    public string Id { get; init; }

    /// <summary>Who says it: an origin id, a cast id, someone on the map, or one of Speakers.</summary>
    public required string Speaker { get; init; }

    /// <summary>What they say. May hold {name}, {sb} and the other tokens.</summary>
    public required string Line { get; init; }

    /// <summary>How they say it, shown small beside their name: grinning, quietly. Null for nothing.</summary>
    public string Direction { get; init; }

    /// <summary>What has to be true for it to be said at all.</summary>
    public IReadOnlyList<DialogueCondition> Conditions { get; init; } = new DialogueCondition[0];

    /// <summary>What it does when it's said.</summary>
    public IReadOnlyList<DialogueEffect> Effects { get; init; } = new DialogueEffect[0];

    /// <summary>The options under it, two to four of them, or none.</summary>
    public IReadOnlyList<DialogueOption> Options { get; init; } = new DialogueOption[0];

    /// <summary>Where it goes when it has no options: a node's id, DialogueScript.End, or null for the next line.</summary>
    public string Next { get; init; }

    /// <summary>For a question on a clock: how many seconds you get. Zero for no clock.</summary>
    public float Seconds { get; init; }

    /// <summary>Where a question on a clock goes when the time runs out.</summary>
    public string Timeout { get; init; }

    /// <summary>A painting to show behind the line, by its id in Paintings, or null for the scene behind.</summary>
    public string Painting { get; init; }

    /// <summary>Whether the quest board's up behind the line.</summary>
    public bool Board { get; init; }

    /// <summary>A sound to play as the line comes up, or null: a bell, a splash.</summary>
    public Sfx? Sound { get; init; }
}
