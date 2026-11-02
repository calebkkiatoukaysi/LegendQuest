using LegendQuest.Save;

namespace LegendQuest.Items;

/// <summary>
/// Buying and selling across a shop's counter: gold one way, the item the other, and why a deal
/// can't be done.
/// </summary>
/// <remarks>
/// The prices are the shop's (ShopDefinition.Price and Offer); this just moves the gold and the
/// goods, both in the save, so a deal can't be half done. A shop never runs out of its stock or of
/// gold. Draws nothing.
/// </remarks>
public static class Trade
{
    /// <summary>Why the party can't buy something at a price, or null if it can.</summary>
    /// <param name="game">The game, for the party's gold.</param>
    /// <param name="item">What they'd buy.</param>
    /// <param name="price">What it costs here.</param>
    /// <returns>A sentence in the game's voice, or null.</returns>
    public static string WhyNotBuy(SaveData game, ItemDefinition item, int price) =>
        game.Gold < price ? $"{item.Name} is {price} gold, and the party has {game.Gold}." : null;

    /// <summary>Why a shop won't buy something, or null if it will: it won't give nothing for it.</summary>
    /// <param name="shop">The shop.</param>
    /// <param name="item">What the party would sell.</param>
    /// <returns>A sentence in the game's voice, or null.</returns>
    public static string WhyNotSell(ShopDefinition shop, ItemDefinition item) =>
        shop.Offer(item) < 1 ? $"{shop.Name} won't give a copper for {item.Name}." : null;

    /// <summary>Buys one of something: the gold goes, and it goes in the bag. Check WhyNotBuy first.</summary>
    /// <param name="game">The game.</param>
    /// <param name="id">The item.</param>
    /// <param name="price">What it costs here.</param>
    public static void Buy(SaveData game, string id, int price)
    {
        game.Gold -= price;
        game.Bag.Add(id);
    }

    /// <summary>Sells some of something out of the bag, at what the shop offers for each.</summary>
    /// <param name="game">The game.</param>
    /// <param name="shop">The shop.</param>
    /// <param name="id">The item.</param>
    /// <param name="count">How many. The bag has to have that many.</param>
    /// <returns>The gold the party got, or 0 if the bag didn't have that many and nothing was sold.</returns>
    public static int Sell(SaveData game, ShopDefinition shop, string id, int count)
    {
        if (!game.Bag.Remove(id, count)) return 0;

        int gold = shop.Offer(ItemCatalog.Get(id)) * count;
        game.Gold += gold;
        return gold;
    }
}
