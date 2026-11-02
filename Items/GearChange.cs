using System;
using System.Globalization;

namespace LegendQuest.Items;

/// <summary>
/// One number that putting something on would change, and by how much: AC +1, Speed -3 m.
/// </summary>
/// <remarks>Every number GearComparison lists is better higher, so a rise is good news and a fall is bad.</remarks>
/// <param name="Stat">What it's a change to, as the tooltip names it.</param>
/// <param name="Amount">How much it goes up, or down if it's below zero.</param>
/// <param name="Unit">What goes after the number, like " m", or empty.</param>
public readonly record struct GearChange(string Stat, double Amount, string Unit)
{
    /// <summary>Whether it's a change for the better.</summary>
    public bool Better => Amount > 0.0;

    /// <summary>The change as the tooltip writes it: the stat, then the amount with its sign and at most one decimal place, then the unit.</summary>
    public override string ToString() =>
        Stat + " " + (Amount < 0.0 ? "-" : "+") + Math.Abs(Amount).ToString("0.#", CultureInfo.InvariantCulture) + Unit;
}
