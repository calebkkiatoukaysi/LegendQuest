using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// What the story's foes give up when they're beaten, and what the Old Crown Vault holds.
/// </summary>
/// <remarks>
/// One table per enemy, called what the enemy is, so an EnemyDefinition's LootTable is usually its
/// own id; old-crown-vault is the vault fight's as a whole. Wolves leave pelts, the Hollowed a few
/// coins and now and then a black crystal shard, the Red Gulls what a thug carries, and the bosses
/// something worth the trouble: the Thornmother the bow grown out of her den, and the vault its gold
/// and the Sunsteel Blade from the Old Crown's armoury. Kael's duel gives nothing; you don't loot a friend.
/// </remarks>
public static class LootTableCatalog
{
    private static readonly LootTable[] Table =
    {
        // The Prologue's sea-wraiths: a few coins out of drowned pockets.
        new() { Id = "sea-wraith", GoldMin = 0, GoldMax = 3 },

        // The Red Gulls on the docks, and Bosun Grell.
        new()
        {
            Id = "red-gull-thug", GoldMin = 3, GoldMax = 8, Rolls = 1,
            Drops = new[] { (LootTable.Nothing, 4), ("dagger", 1), (ItemCatalog.HealingPotion, 1) },
        },
        new()
        {
            Id = "bosun-grell", GoldMin = 15, GoldMax = 25, Rolls = 2,
            Drops = new[] { ("scimitar", 1), (ItemCatalog.HealingPotion, 2), ("alchemists-fire", 1) },
        },

        // The Mirewood: gloomfang wolves, and the Thornmother in her den.
        new()
        {
            Id = "gloomfang", Rolls = 1,
            Drops = new[] { ("wolf-pelt", 3), (LootTable.Nothing, 1) },
        },
        new() { Id = "thornmother", Always = new[] { "thornwood-bow", "black-crystal-shard" } },

        // The Hollowed: a few coins, the odd shard, and what each of them was carrying.
        new()
        {
            Id = "hollowed-footman", GoldMin = 1, GoldMax = 6, Rolls = 1,
            Drops = new[] { (LootTable.Nothing, 6), ("black-crystal-shard", 1), (ItemCatalog.HealingPotion, 1) },
        },
        new()
        {
            Id = "hollowed-hexer", GoldMin = 2, GoldMax = 8, Rolls = 1,
            Drops = new[] { (LootTable.Nothing, 5), ("black-crystal-shard", 2), ("elixir-of-focus", 1) },
        },
        new()
        {
            Id = "hollowed-sapper", GoldMin = 1, GoldMax = 6, Rolls = 1,
            Drops = new[] { (LootTable.Nothing, 5), ("black-crystal-shard", 1), ("alchemists-fire", 2) },
        },
        new()
        {
            Id = "sapper-chief", GoldMin = 10, GoldMax = 20, Rolls = 1, Always = new[] { "alchemists-fire" },
            Drops = new[] { ("black-crystal-shard", 2), ("greater-healing-potion", 1) },
        },

        // The Old Crown Vault: gold leaf off its constructs, and the vault's own treasure for the fight in it.
        new() { Id = "vault-construct", GoldMin = 5, GoldMax = 15 },
        new()
        {
            Id = "old-crown-vault", GoldMin = 40, GoldMax = 60, Rolls = 1, Always = new[] { "sunsteel-blade" },
            Drops = new[] { (ItemCatalog.HealingPotion, 2), ("greater-healing-potion", 1) },
        },
    };

    private static readonly Dictionary<string, LootTable> ById = new();

    /// <summary>Indexes the table and checks every row.</summary>
    /// <remarks>Throws on purpose, so a loot table naming an item that doesn't exist shows up when the game starts.</remarks>
    static LootTableCatalog()
    {
        foreach (LootTable table in Table)
        {
            if (Problem(table) is string problem) throw new InvalidOperationException($"Loot table {table.Id} {problem}.");
            if (!ById.TryAdd(table.Id, table)) throw new InvalidOperationException($"Two loot tables are called {table.Id}.");
        }
    }

    /// <summary>Every loot table. Reading it checks the table, which the game does at startup.</summary>
    public static IEnumerable<LootTable> All => ById.Values;

    /// <summary>Whether there's a loot table called this.</summary>
    /// <param name="id">What an encounter or an enemy calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One loot table.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static LootTable Get(string id) => ById[id];

    /// <summary>What's wrong with a row, or null if nothing is.</summary>
    /// <param name="table">The row.</param>
    private static string Problem(LootTable table)
    {
        if (table.GoldMin < 0 || table.GoldMax < table.GoldMin) return $"gives {table.GoldMin} to {table.GoldMax} gold";
        if (table.Rolls < 0) return $"draws {table.Rolls} times";
        if (table.Rolls > 0 != table.Drops.Count > 0) return "has to have drops if and only if it draws";
        if (table.GoldMax == 0 && table.Rolls == 0 && table.Always.Count == 0) return "never gives anything";

        foreach ((string item, int weight) in table.Drops)
        {
            if (weight < 1) return $"weights a drop {weight}, and a weight's from 1 up";
            if (item != LootTable.Nothing && !ItemCatalog.Exists(item)) return $"drops {item}, which isn't an item";
        }

        foreach (string item in table.Always)
            if (!ItemCatalog.Exists(item)) return $"always gives {item}, which isn't an item";

        return null;
    }
}
