using System;

namespace LegendQuest.Rules;

/// <summary>
/// A character's six ability scores, and the modifier each one gives.
/// </summary>
/// <remarks>
/// The modifier is 5e's: half of (score minus 10), rounded down, so 10 and 11 give +0, 8 gives -1
/// and 15 gives +2. Not from a tutorial. Plain properties so it goes in a save as it is.
/// </remarks>
public class AbilityScores
{
    /// <summary>The score every ability starts at, before anything's been picked. 10 is +0.</summary>
    private const int Average = 10;

    /// <summary>Strength.</summary>
    public int Strength { get; init; } = Average;

    /// <summary>Dexterity.</summary>
    public int Dexterity { get; init; } = Average;

    /// <summary>Constitution.</summary>
    public int Constitution { get; init; } = Average;

    /// <summary>Intelligence.</summary>
    public int Intelligence { get; init; } = Average;

    /// <summary>Wisdom.</summary>
    public int Wisdom { get; init; } = Average;

    /// <summary>Charisma.</summary>
    public int Charisma { get; init; } = Average;

    /// <summary>One score, by which ability.</summary>
    /// <param name="ability">The ability.</param>
    public int this[Ability ability] => ability switch
    {
        Ability.Strength => Strength,
        Ability.Dexterity => Dexterity,
        Ability.Constitution => Constitution,
        Ability.Intelligence => Intelligence,
        Ability.Wisdom => Wisdom,
        Ability.Charisma => Charisma,
        _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, "There's no ability called that."),
    };

    /// <summary>What a score adds to a roll: half of (score minus 10), rounded down.</summary>
    /// <param name="ability">The ability.</param>
    public int Modifier(Ability ability) => (int)MathF.Floor((this[ability] - Average) / 2f);
}
