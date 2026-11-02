using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// What a loot table gave up: some gold and some items.
/// </summary>
/// <remarks>The victory screen shows it and puts it in the party's gold and bag. Empty rather than null when there's nothing.</remarks>
public sealed class LootResult
{
    /// <summary>The gold.</summary>
    public int Gold { get; init; }

    /// <summary>The items, by id, one entry for each: two pelts are there twice.</summary>
    public IReadOnlyList<string> Items { get; init; } = Array.Empty<string>();
}
