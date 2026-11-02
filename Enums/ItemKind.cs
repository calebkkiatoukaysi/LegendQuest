namespace LegendQuest;

/// <summary>
/// What sort of thing an item is, which decides where it's worn, if it's worn at all.
/// </summary>
/// <remarks>ItemDefinition.Slot turns this into an EquipmentSlot. New ones go on the end.</remarks>
public enum ItemKind
{
    /// <summary>Held in the main hand, or a light one in the off hand.</summary>
    Weapon,

    /// <summary>Worn on the body: clothes, robes, leather, mail or plate.</summary>
    Armour,

    /// <summary>Carried in the off hand.</summary>
    Shield,

    /// <summary>A cap, a helm or a circlet.</summary>
    Head,

    /// <summary>Gloves or gauntlets.</summary>
    Hands,

    /// <summary>Boots.</summary>
    Feet,

    /// <summary>Worn over the shoulders.</summary>
    Cloak,

    /// <summary>Worn round the neck. Stats only: nothing's drawn for it.</summary>
    Amulet,

    /// <summary>Worn on a finger. Stats only: nothing's drawn for it.</summary>
    Ring,

    /// <summary>Used up: drunk or thrown.</summary>
    Consumable,

    /// <summary>Something picked up to sell: pelts, shards.</summary>
    Material,
}
