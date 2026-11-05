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

    /// <summary>Who says it: an origin id, a cast id, someone on the map, one of Speakers, or a voice (Speakers.VoicePrefix and an origin or cast id) for someone heard and not seen.</summary>
    public required string Speaker { get; init; }

    /// <summary>What they say. May hold {name}, {sb} and the other tokens.</summary>
    public required string Line { get; init; }

    /// <summary>How they say it, shown small beside their name: grinning, quietly. Null for nothing.</summary>
    public string Direction { get; init; }

    /// <summary>The face they say it with, one of Expressions, or null for neutral. Legacy portraits have the one face, so they don't show it.</summary>
    public string Expression { get; init; }

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
    /// <remarks>For the hand-written lines. A script compiled from the story plays its sounds with sfx cues instead, when they come in the cues.</remarks>
    public Sfx? Sound { get; init; }

    /// <summary>What happens on the stage before the line shows, in order: the cue lines over it in the script.</summary>
    /// <remarks>CutsceneDirector plays them as the line comes up, and the line shows once they're done. The script compiler works Painting and Board out from them too, the way Legacy shows a backdrop and the quest board.</remarks>
    public IReadOnlyList<StageCue> Cues { get; init; } = new StageCue[0];
}
