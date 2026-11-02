namespace LegendQuest;

/// <summary>
/// Where on someone an item goes. Everyone has one of each.
/// </summary>
/// <remarks>
/// The spec's slots (6.5). Saves keep what's in each by name, so renaming one breaks saves and
/// reordering them doesn't.
/// </remarks>
public enum EquipmentSlot
{
    /// <summary>The weapon they attack with.</summary>
    MainHand,

    /// <summary>A shield, or a second light weapon.</summary>
    OffHand,

    /// <summary>Clothes or armour.</summary>
    Body,

    /// <summary>A cap, a helm or a circlet.</summary>
    Head,

    /// <summary>Gloves or gauntlets.</summary>
    Hands,

    /// <summary>Boots.</summary>
    Feet,

    /// <summary>A cloak.</summary>
    Cloak,

    /// <summary>An amulet. Nothing's drawn for it.</summary>
    Amulet,

    /// <summary>A ring. Nothing's drawn for it.</summary>
    Ring,
}
