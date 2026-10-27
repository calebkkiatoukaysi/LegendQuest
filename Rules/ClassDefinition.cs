namespace LegendQuest.Rules;

/// <summary>
/// One class: what it's called and its hit die. More goes on it (proficiencies, the starting kit,
/// the abilities) as the systems that use them come in.
/// </summary>
public sealed class ClassDefinition
{
    /// <summary>Which class this is.</summary>
    public required ClassId Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>The biggest number on its hit die. At level 1 a character has this many hit points, plus their Constitution modifier.</summary>
    public required int HitDie { get; init; }
}
