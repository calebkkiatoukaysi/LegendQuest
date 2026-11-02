using System;
using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// Every shop: the Lantern Market's trader, Halden's Forge, the apothecary, and the Chandler's.
/// </summary>
/// <remarks>
/// Kept as a table like the other catalogues, so a shop stocking something that doesn't exist shows
/// up when the game starts. The markups follow what the shopkeepers say: the market trader's
/// "everything's half price, half of a higher price" is a quarter over the odds, and the Chandler's
/// prices are honest. Magic is rare in Questeria's shops, and priced to be saved up for.
/// </remarks>
public static class ShopCatalog
{
    private static readonly ShopDefinition[] Table =
    {
        // The Trader's stall in the Lantern Market: travelling gear, a few weapons, potions, and a couple of curios.
        new()
        {
            Id = "lantern-market", Name = "The Trader's Stall", BuyMarkup = 1.25f,
            Stock = new[]
            {
                ItemCatalog.HealingPotion, "alchemists-fire", "travelling-clothes", "padded-armour", "leather-armour", "wooden-shield",
                "travelling-cloak", "leather-cap", "leather-gloves", "leather-boots", "dagger", "quarterstaff", "spear", "handaxe",
                "shortbow", "ember-dagger", "amulet-of-vigour", "circlet-of-focus",
            },
        },

        // Halden's Forge, at the market's east end: every weapon and suit of armour in the book, and the Royal Wardens' shields.
        new()
        {
            Id = "forge", Name = "Halden's Forge",
            Stock = new[]
            {
                "dagger", "shortsword", "longsword", "rapier", "scimitar", "handaxe", "battleaxe", "greatsword", "warhammer", "mace",
                "quarterstaff", "spear", "shortbow", "longbow", "light-crossbow", "padded-armour", "leather-armour", "studded-leather",
                "hide-armour", "chain-shirt", "scale-mail", "chain-mail", "plate-armour", "wooden-shield", "steel-shield",
                "warden-shield", "iron-helm", "iron-boots", "gauntlets-of-might",
            },
        },

        // An apothecary's shelf: potions, fire and focus, at a little over cost.
        new()
        {
            Id = "apothecary", Name = "The Apothecary", BuyMarkup = 1.1f,
            Stock = new[] { ItemCatalog.HealingPotion, "greater-healing-potion", "alchemists-fire", "elixir-of-focus" },
        },

        // The Chandler's, by the docks: what a ship's crew needs ashore.
        new()
        {
            Id = "chandler", Name = "The Chandler's",
            Stock = new[]
            {
                ItemCatalog.HealingPotion, "alchemists-fire", "travelling-clothes", "travelling-cloak", "leather-cap", "leather-gloves",
                "leather-boots", "dagger", "light-crossbow",
            },
        },
    };

    private static readonly Dictionary<string, ShopDefinition> ById = new();

    /// <summary>Indexes the table and checks every shop.</summary>
    /// <remarks>Throws on purpose, so a typo shows up when the game starts.</remarks>
    static ShopCatalog()
    {
        foreach (ShopDefinition shop in Table)
        {
            if (string.IsNullOrWhiteSpace(shop.Name)) throw new InvalidOperationException($"Shop {shop.Id} has no name.");
            if (shop.BuyMarkup < 1f) throw new InvalidOperationException($"Shop {shop.Id} sells for less than things are worth, and would buy them back for more.");
            if (shop.Stock.Count == 0) throw new InvalidOperationException($"Shop {shop.Id} sells nothing.");

            var stocked = new HashSet<string>();
            foreach (string id in shop.Stock)
            {
                if (!ItemCatalog.Exists(id)) throw new InvalidOperationException($"Shop {shop.Id} sells {id}, which isn't an item.");
                if (!stocked.Add(id)) throw new InvalidOperationException($"Shop {shop.Id} sells {id} twice.");
            }

            if (!ById.TryAdd(shop.Id, shop)) throw new InvalidOperationException($"Two shops are called {shop.Id}.");
        }
    }

    /// <summary>Every shop. Reading it checks the table, which the game does at startup.</summary>
    public static IEnumerable<ShopDefinition> All => ById.Values;

    /// <summary>Whether there's a shop called this.</summary>
    /// <param name="id">What a script calls it.</param>
    public static bool Exists(string id) => ById.ContainsKey(id ?? string.Empty);

    /// <summary>One shop.</summary>
    /// <param name="id">Its id. It has to exist.</param>
    public static ShopDefinition Get(string id) => ById[id];
}
