using System;

namespace LegendQuest.Rules;

/// <summary>
/// One die, rolled.
/// </summary>
/// <remarks>
/// Every roll in the rules comes through here, and takes its Random from whoever's asking, so a
/// seeded one gives the same rolls every time and --simulate repeats itself. Dice with something
/// added are a DiceExpression, parsed once and rolled with this.
/// </remarks>
public static class Dice
{
    /// <summary>The die every attack, check and save rolls.</summary>
    public const int D20 = 20;

    /// <summary>Rolls one die.</summary>
    /// <param name="random">Where the roll comes from.</param>
    /// <param name="sides">How many sides it has.</param>
    /// <returns>From 1 to sides.</returns>
    public static int Roll(Random random, int sides)
    {
        if (sides < 1) throw new ArgumentOutOfRangeException(nameof(sides), sides, "A die needs at least one side.");
        return random.Next(1, sides + 1);
    }
}
