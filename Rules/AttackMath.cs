using System;

namespace LegendQuest.Rules;

/// <summary>
/// The arithmetic of an attack: the d20 with advantage or disadvantage, whether it hits, and the
/// exact chance that it will.
/// </summary>
/// <remarks>
/// 5e's, from the SRD 5.1. A natural 20 always hits (and is a critical), a natural 1 always misses,
/// and anything else hits if it and the attack bonus meet the target's AC. HitChance counts the
/// faces of the d20 that hit rather than estimating, so the tooltip's number is the true one. It
/// takes its Random, so a seeded one gives the same rolls every time.
/// </remarks>
public static class AttackMath
{
    /// <summary>The roll that always hits, and hits as a critical.</summary>
    public const int CriticalRoll = Dice.D20;

    /// <summary>The roll that always misses.</summary>
    public const int FumbleRoll = 1;

    /// <summary>Rolls the d20 for an attack.</summary>
    /// <param name="random">Where the roll comes from.</param>
    /// <param name="advantage">Whether to roll two and keep the higher or the lower.</param>
    /// <param name="first">The first d20.</param>
    /// <param name="second">The second, or 0 if only one was rolled.</param>
    /// <returns>The one that counts.</returns>
    public static int D20(Random random, Advantage advantage, out int first, out int second)
    {
        first = Dice.Roll(random, Dice.D20);
        second = advantage == Advantage.Normal ? 0 : Dice.Roll(random, Dice.D20);

        return advantage switch
        {
            Advantage.Advantage => Math.Max(first, second),
            Advantage.Disadvantage => Math.Min(first, second),
            _ => first,
        };
    }

    /// <summary>Whether a roll hits.</summary>
    /// <param name="kept">The d20 that counts.</param>
    /// <param name="attackBonus">Everything added to it.</param>
    /// <param name="armourClass">The target's AC.</param>
    public static bool Hits(int kept, int attackBonus, int armourClass) =>
        kept == CriticalRoll || kept != FumbleRoll && kept + attackBonus >= armourClass;

    /// <summary>The chance an attack hits, from 0 to 1.</summary>
    /// <remarks>With advantage it misses only if both dice miss; with disadvantage it hits only if both hit.</remarks>
    /// <param name="attackBonus">Everything added to the d20.</param>
    /// <param name="armourClass">The target's AC.</param>
    /// <param name="advantage">Whether it's rolled with advantage or disadvantage.</param>
    public static double HitChance(int attackBonus, int armourClass, Advantage advantage)
    {
        int faces = 0;
        for (int roll = 1; roll <= Dice.D20; roll++)
            if (Hits(roll, attackBonus, armourClass)) faces++;

        double once = faces / (double)Dice.D20;
        return advantage switch
        {
            Advantage.Advantage => 1.0 - (1.0 - once) * (1.0 - once),
            Advantage.Disadvantage => once * once,
            _ => once,
        };
    }

    /// <summary>What several reasons for advantage and disadvantage come to.</summary>
    /// <remarks>5e's rule: any of each cancels out to a straight roll, however many more of one there are.</remarks>
    /// <param name="advantages">How many things give advantage.</param>
    /// <param name="disadvantages">How many things give disadvantage.</param>
    public static Advantage Combine(int advantages, int disadvantages) => (advantages > 0, disadvantages > 0) switch
    {
        (true, false) => Advantage.Advantage,
        (false, true) => Advantage.Disadvantage,
        _ => Advantage.Normal,
    };
}
