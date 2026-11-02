using System;
using System.Collections.Generic;

namespace LegendQuest.Rules;

/// <summary>
/// How a roll went: every die, every modifier and where it came from, and the total.
/// </summary>
/// <remarks>
/// Everything the combat log and the dice on screen show, the same idea as CheckResult: the screen
/// draws this and never rolls anything itself. A critical's doubled dice are all in Dice. Empty
/// rather than null when there's nothing in a list, so nothing that reads one has to check.
/// </remarks>
public sealed class RollResult
{
    /// <summary>Each die rolled, in the order they were rolled.</summary>
    public IReadOnlyList<int> Dice { get; init; } = Array.Empty<int>();

    /// <summary>Each number added to the dice, by where it came from: STR, the weapon's +1, the potion's bonus.</summary>
    public IReadOnlyList<(string Source, int Amount)> Modifiers { get; init; } = Array.Empty<(string, int)>();

    /// <summary>The dice and the modifiers, added up.</summary>
    public int Total { get; init; }
}
