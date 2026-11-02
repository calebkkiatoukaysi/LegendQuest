using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// One shop: what it's called, what it sells, and how much over the odds it charges.
/// </summary>
/// <remarks>
/// A shop never runs out of what it stocks, and buys anything at all for half what it's worth, the
/// way the spec has selling work. Data only: the shop screen does the buying and selling with the
/// party's gold and bag.
/// </remarks>
public sealed class ShopDefinition
{
    /// <summary>What you're paid for something, as a share of its value: half.</summary>
    private const int SellDivisor = 2;

    /// <summary>What the scripts call it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>What it sells, by item id, in the order it shows them.</summary>
    public IReadOnlyList<string> Stock { get; init; } = Array.Empty<string>();

    /// <summary>What it charges, as a multiple of an item's value: 1 is the fair price.</summary>
    public float BuyMarkup { get; init; } = 1f;

    /// <summary>What buying something here costs: its value times the markup, rounded, and never less than a gold piece.</summary>
    /// <param name="item">The item.</param>
    public int Price(ItemDefinition item) =>
        Math.Max(1, (int)MathF.Round(item.Value * BuyMarkup, MidpointRounding.AwayFromZero));

    /// <summary>What selling something here gets you: half its value, rounded down.</summary>
    /// <param name="item">The item.</param>
    public int Offer(ItemDefinition item) => item.Value / SellDivisor;
}
