using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// What a foe or a fight can give up: some gold between two numbers, anything it always has, and a
/// few weighted draws.
/// </summary>
/// <remarks>
/// Each draw picks one of Drops with a chance in proportion to its weight, and a drop of Nothing is
/// a draw that finds nothing, which is how a Hollowed only sometimes has a shard on it. It takes its
/// Random, so a seeded one gives the same loot every time.
/// </remarks>
public sealed class LootTable
{
    /// <summary>A drop that's no item at all: weight it to make the others rarer.</summary>
    public const string Nothing = null;

    /// <summary>What the encounters and the enemies call it.</summary>
    public required string Id { get; init; }

    /// <summary>What a draw can find, each with its weight: one weighted 3 comes up three times as often as one weighted 1.</summary>
    public IReadOnlyList<(string Item, int Weight)> Drops { get; init; } = Array.Empty<(string, int)>();

    /// <summary>How many draws it makes from Drops.</summary>
    public int Rolls { get; init; }

    /// <summary>The least gold it gives.</summary>
    public int GoldMin { get; init; }

    /// <summary>The most gold it gives.</summary>
    public int GoldMax { get; init; }

    /// <summary>Items it always gives, by id, on top of the draws: a boss's prize.</summary>
    public IReadOnlyList<string> Always { get; init; } = Array.Empty<string>();

    /// <summary>Rolls for what it gives up.</summary>
    /// <param name="random">Where the rolls come from.</param>
    /// <returns>The gold, then Always and whatever the draws found, in that order.</returns>
    public LootResult Roll(Random random)
    {
        int totalWeight = 0;
        foreach ((_, int weight) in Drops) totalWeight += weight;

        var items = new List<string>(Always);
        for (int draw = 0; draw < Rolls; draw++)
        {
            int pick = random.Next(totalWeight);
            foreach ((string item, int weight) in Drops)
            {
                if (pick < weight)
                {
                    if (item != Nothing) items.Add(item);
                    break;
                }
                pick -= weight;
            }
        }

        return new LootResult { Gold = random.Next(GoldMin, GoldMax + 1), Items = items };
    }
}
