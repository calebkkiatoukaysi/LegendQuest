namespace LegendQuest.Rules;

/// <summary>
/// One class: what it's called and its hit die. More goes on it (proficiencies, the starting kit,
/// the abilities) as the systems that use them come in.
/// </summary>
public sealed class ClassDefinition
{
    /// <summary>Which class this is.</summary>
    public required ClassId Id { get; init; }

    /// <summary>What the story calls it on screen. The ClassId is the SRD's name for it.</summary>
    public required string Name { get; init; }

    /// <summary>What it's for, in a few words, for the creation screen.</summary>
    public required string Role { get; init; }

    /// <summary>The biggest number on its hit die. At level 1 a character has this many hit points, plus their Constitution modifier.</summary>
    public required int HitDie { get; init; }

    /// <summary>Where 5e's standard array (15, 14, 13, 12, 10, 8) goes for this class. The creation screen starts you on it.</summary>
    public required AbilityScores Recommended { get; init; }
}
