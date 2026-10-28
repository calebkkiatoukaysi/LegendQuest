using System;
using System.Collections.Generic;

namespace LegendQuest.Rules;

/// <summary>
/// Ability checks: a d20, plus an ability's modifier, against a DC.
/// </summary>
/// <remarks>
/// 5e's from the SRD 5.1, the way dialogue uses them: no proficiency yet, since there are no skills
/// to be proficient in until the character sheet. Advantage rolls two and keeps the higher. It takes
/// its Random, so a seeded one gives the same rolls every time. Not from a tutorial.
/// </remarks>
public static class AbilityCheck
{
    /// <summary>A d20.</summary>
    public const int Sides = 20;

    /// <summary>Rolls a check.</summary>
    /// <param name="random">Where the dice come from.</param>
    /// <param name="label">What it's for, as the screen shows it.</param>
    /// <param name="ability">The ability that adds its modifier.</param>
    /// <param name="scores">Whoever's rolling's scores.</param>
    /// <param name="dc">The number to meet or beat.</param>
    /// <param name="advantage">Whether to roll two and keep the higher.</param>
    /// <returns>Everything that happened.</returns>
    public static CheckResult Roll(Random random, string label, Ability ability, AbilityScores scores, int dc, bool advantage)
    {
        var rolls = new List<int> { random.Next(1, Sides + 1) };
        if (advantage) rolls.Add(random.Next(1, Sides + 1));

        return new CheckResult
        {
            Label = label,
            Rolls = rolls,
            Kept = Math.Max(rolls[0], rolls[^1]),
            Modifiers = new[] { (ability.ToString().ToUpperInvariant(), scores.Modifier(ability)) },
            Dc = dc,
            Advantage = advantage,
        };
    }
}
