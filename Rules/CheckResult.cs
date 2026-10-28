using System.Collections.Generic;

namespace LegendQuest.Rules;

/// <summary>
/// How a check went: the d20s rolled, the one kept, every modifier and where it came from, the
/// total, the DC, and whether it passed.
/// </summary>
/// <remarks>Everything the dice on screen show, so the overlay draws this and never rolls anything itself.</remarks>
public sealed class CheckResult
{
    /// <summary>What the check was for, as the screen shows it: PERSUADE, INTIMIDATE.</summary>
    public required string Label { get; init; }

    /// <summary>The d20s rolled: one, or two with advantage or disadvantage.</summary>
    public required IReadOnlyList<int> Rolls { get; init; }

    /// <summary>The d20 that counts.</summary>
    public required int Kept { get; init; }

    /// <summary>Each modifier added to it, by where it came from.</summary>
    public required IReadOnlyList<(string Source, int Amount)> Modifiers { get; init; }

    /// <summary>The number to meet or beat.</summary>
    public required int Dc { get; init; }

    /// <summary>Whether it was rolled with advantage.</summary>
    public required bool Advantage { get; init; }

    /// <summary>The kept roll plus every modifier.</summary>
    public int Total
    {
        get
        {
            int total = Kept;
            foreach ((_, int amount) in Modifiers) total += amount;
            return total;
        }
    }

    /// <summary>Whether it passed: the total meets the DC. A natural 20 always passes and a natural 1 always fails, the same as 5e's attacks.</summary>
    public bool Success => Kept == AbilityCheck.Sides || Kept != 1 && Total >= Dc;
}
