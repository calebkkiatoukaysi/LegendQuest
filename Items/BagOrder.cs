using System.Collections.Generic;

namespace LegendQuest.Items;

/// <summary>
/// The order the bag is shown in: the catalogue's, so the weapons come first, then the armour,
/// shields and what's worn with them, then the potions, then what's for selling.
/// </summary>
/// <remarks>
/// The save keeps the bag in the order things came in, which is no order at all to look through.
/// An item's Icon is its place in ItemCatalog, which is grouped by kind, so sorting by it groups
/// the bag the same way; two of the same thing keep the order they came in. Only the view is
/// sorted, never the save.
/// </remarks>
public static class BagOrder
{
    /// <summary>The bag's places, sorted to show.</summary>
    /// <param name="stacks">The bag, as the save has it.</param>
    /// <returns>A new list of the same stacks.</returns>
    public static List<ItemStack> Of(IReadOnlyList<ItemStack> stacks)
    {
        var sorted = new List<(ItemStack Stack, int Place)>(stacks.Count);
        for (int i = 0; i < stacks.Count; i++) sorted.Add((stacks[i], i));
        sorted.Sort((a, b) =>
        {
            int byIcon = ItemCatalog.Get(a.Stack.Id).Icon.CompareTo(ItemCatalog.Get(b.Stack.Id).Icon);
            return byIcon != 0 ? byIcon : a.Place.CompareTo(b.Place);
        });

        var order = new List<ItemStack>(sorted.Count);
        foreach ((ItemStack stack, _) in sorted) order.Add(stack);
        return order;
    }
}
