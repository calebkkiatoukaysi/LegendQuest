namespace LegendQuest.Items;

/// <summary>
/// One place in the party's bag: an item, and how many of it.
/// </summary>
/// <remarks>
/// Plain settable properties so it goes in the save as it is. Something that stacks has one of
/// these however many there are; anything else has one each with a count of 1, so each sword has
/// its own place in the grid. Inventory keeps it that way.
/// </remarks>
public sealed class ItemStack
{
    /// <summary>The item, by its id in ItemCatalog.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>How many of it.</summary>
    public int Count { get; set; }
}
