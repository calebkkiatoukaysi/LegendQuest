namespace LegendQuest;

/// <summary>
/// What sort of thing the keys are on in the inventory.
/// </summary>
public enum InventorySpot
{
    /// <summary>One of the slots round whoever's being looked at.</summary>
    Slot,

    /// <summary>A place in the bag.</summary>
    Bag,

    /// <summary>One of the key items under the bag.</summary>
    KeyItem,
}
