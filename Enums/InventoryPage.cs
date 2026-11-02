namespace LegendQuest;

/// <summary>
/// Which page the inventory's right-hand panel is on. The member and what they're wearing stay on
/// the left either way.
/// </summary>
public enum InventoryPage
{
    /// <summary>The party's bag, its gold and the key items. I opens it.</summary>
    Bag,

    /// <summary>The character sheet: scores, saves, attacks and abilities. C opens it.</summary>
    Character,
}
