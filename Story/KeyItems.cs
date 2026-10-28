using System;
using System.Collections.Generic;

namespace LegendQuest.Story;

/// <summary>
/// The things the story hands you: the trinkets, the relics and the letters.
/// </summary>
/// <remarks>
/// They go in the save as a list of ids until the inventory exists, which is when they get their
/// pictures and stats. Kept as a table so a script handing over an item that doesn't exist is
/// found when the game starts.
/// </remarks>
public static class KeyItems
{
    private static readonly KeyItem[] Table =
    {
        new("tomas-hook", "Tomas's Lucky Hook", "A brass fish hook on a cord. Luck, he says."),
        new("courier-letter", "Courier's Letter", "Charred paper with a cube pressed into its seal."),
        new("black-iron-coin", "Black-Iron Coin", "A black coin stamped with a cube."),
        new("sunheart", "The Sunheart", "A gold gem with a warm light inside it."),
    };

    private static readonly Dictionary<string, KeyItem> ById = new();

    /// <summary>Indexes the table.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts.</remarks>
    static KeyItems()
    {
        foreach (KeyItem item in Table)
            if (!ById.TryAdd(item.Id, item)) throw new InvalidOperationException($"Two key items are called {item.Id}.");
    }

    /// <summary>Whether there's an item called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One item.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static KeyItem Get(string id) => ById[id];
}
