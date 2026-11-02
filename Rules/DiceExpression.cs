using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LegendQuest.Rules;

/// <summary>
/// Some dice and a number to add to them, written 5e's way: 2d6+3, 1d8, 1d10-1, or a flat 3. Parsed
/// once out of its text, then rolled as often as it's needed.
/// </summary>
/// <remarks>
/// From the SRD 5.1. A critical hit rolls the dice twice over and adds the bonus once, the way 5e
/// doubles a crit's damage dice and nothing else. The default is None, no dice and nothing added, so
/// an item with no extra damage just leaves it unset. Parse throws on anything that isn't dice, and
/// the tables that use it parse in their static constructors, so a typo shows up when the game starts.
/// </remarks>
/// <param name="Count">How many dice.</param>
/// <param name="Sides">How many sides each one has.</param>
/// <param name="Bonus">What's added to them, or taken off if it's below zero.</param>
public readonly record struct DiceExpression(int Count, int Sides, int Bonus)
{
    /// <summary>The most dice one can roll before a critical doubles them. More than this is a typo.</summary>
    private const int MostDice = 40;

    /// <summary>The fewest sides a die can have: a d2, which is a coin.</summary>
    private const int FewestSides = 2;

    /// <summary>The most sides a die can have: a d100.</summary>
    private const int MostSides = 100;

    /// <summary>The most that can be added or taken off.</summary>
    private const int BiggestBonus = 99;

    /// <summary>Dice with an optional bonus (2d6+3), or just a number (3, -1). Spaces come out before it's matched.</summary>
    private static readonly Regex Notation = new(
        @"^(?:(?<count>\d+)d(?<sides>\d+)(?<bonus>[+-]\d+)?|(?<flat>-?\d+))$",
        RegexOptions.CultureInvariant);

    /// <summary>No dice and nothing added.</summary>
    public static DiceExpression None => default;

    /// <summary>Whether it's None. A flat number isn't: it has something added.</summary>
    public bool IsNone => this == None;

    /// <summary>The lowest it can roll: every die a 1.</summary>
    public int Min => Count + Bonus;

    /// <summary>The highest it can roll: every die on its top face.</summary>
    public int Max => Count * Sides + Bonus;

    /// <summary>What it rolls on average: half of one more than the sides, for each die, plus the bonus.</summary>
    public double Average => Count * (Sides + 1) / 2.0 + Bonus;

    /// <summary>Reads dice out of their text.</summary>
    /// <param name="text">Like 2d6+3, 1d8, 1d10-1 or 3. Upper or lower case, with spaces or without.</param>
    /// <returns>The dice.</returns>
    /// <exception cref="FormatException">If it isn't dice, or it's dice nobody would roll: a d1, fifty of them, +500.</exception>
    public static DiceExpression Parse(string text)
    {
        Match match = Notation.Match((text ?? string.Empty).Replace(" ", string.Empty).ToLowerInvariant());
        if (!match.Success) throw Unreadable(text, "they're written like 2d6+3, 1d8 or 3");

        int count = 0, sides = 0, bonus;
        if (match.Groups["flat"].Success) bonus = Number(text, match.Groups["flat"].Value);
        else
        {
            count = Number(text, match.Groups["count"].Value);
            sides = Number(text, match.Groups["sides"].Value);
            bonus = match.Groups["bonus"].Success ? Number(text, match.Groups["bonus"].Value) : 0;

            if (count < 1 || count > MostDice) throw Unreadable(text, $"there have to be 1 to {MostDice} dice");
            if (sides < FewestSides || sides > MostSides) throw Unreadable(text, $"a die has {FewestSides} to {MostSides} sides");
        }

        if (bonus < -BiggestBonus || bonus > BiggestBonus) throw Unreadable(text, $"what's added has to be within {BiggestBonus} either way");
        return new DiceExpression(count, sides, bonus);
    }

    /// <summary>Rolls it.</summary>
    /// <param name="random">Where the dice come from.</param>
    /// <param name="source">Where the bonus comes from, as the log names it: the weapon, the potion.</param>
    /// <param name="critical">Whether it's a critical hit's damage, which rolls twice the dice and adds the bonus once.</param>
    /// <returns>Every die, the bonus by its source if there is one, and the total.</returns>
    public RollResult Roll(Random random, string source, bool critical = false)
    {
        var dice = new int[critical ? Count * 2 : Count];
        int total = Bonus;
        for (int i = 0; i < dice.Length; i++)
        {
            dice[i] = Dice.Roll(random, Sides);
            total += dice[i];
        }

        return new RollResult
        {
            Dice = dice,
            Modifiers = Bonus == 0 ? Array.Empty<(string, int)>() : new (string, int)[] { (source, Bonus) },
            Total = total,
        };
    }

    /// <summary>Writes it the way Parse reads it: 2d6+3, 1d8, 1d10-1, or a flat 3.</summary>
    public override string ToString()
    {
        if (Count == 0) return Bonus.ToString(CultureInfo.InvariantCulture);

        string dice = Count.ToString(CultureInfo.InvariantCulture) + "d" + Sides.ToString(CultureInfo.InvariantCulture);
        return Bonus switch
        {
            > 0 => dice + "+" + Bonus.ToString(CultureInfo.InvariantCulture),
            < 0 => dice + Bonus.ToString(CultureInfo.InvariantCulture),
            _ => dice,
        };
    }

    /// <summary>One number out of the text.</summary>
    /// <param name="text">All of it, for the message.</param>
    /// <param name="digits">The number, with its sign if it has one.</param>
    private static int Number(string text, string digits) =>
        int.TryParse(digits, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value)
            ? value
            : throw Unreadable(text, $"{digits} is too big a number");

    /// <summary>The error for text that isn't dice.</summary>
    /// <param name="text">The text.</param>
    /// <param name="why">What's wrong with it.</param>
    private static FormatException Unreadable(string text, string why) => new($"\"{text}\" isn't dice: {why}.");
}
