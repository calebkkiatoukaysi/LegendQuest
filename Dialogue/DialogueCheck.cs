using System.Collections.Generic;

namespace LegendQuest.Dialogue;

/// <summary>
/// A check an option rolls: what it's called, which ability adds to it, the DC, and what gives it
/// advantage.
/// </summary>
/// <remarks>The story's rule: DC 12 unless the odds are against you (15), with advantage when a class, a voice or a flag the script names is on your side.</remarks>
public sealed class DialogueCheck
{
    /// <summary>The DC when the script doesn't say otherwise.</summary>
    public const int Usual = 12;

    /// <summary>What the screen calls it: PERSUADE, INTIMIDATE.</summary>
    public required string Label { get; init; }

    /// <summary>The ability whose modifier adds to the roll.</summary>
    public Ability Ability { get; init; } = Ability.Charisma;

    /// <summary>The number to meet or beat.</summary>
    public int Dc { get; init; } = Usual;

    /// <summary>Any one of these being true gives advantage.</summary>
    public IReadOnlyList<DialogueCondition> Advantage { get; init; } = new DialogueCondition[0];
}
