namespace LegendQuest;

/// <summary>
/// A place in the paperdoll's stack of layers: one sheet, or nothing, goes in each when someone's
/// baked.
/// </summary>
/// <remarks>
/// PaperdollOrder says what order they go down in for each way someone faces; Paperdoll says which
/// sheet a look and what they're wearing put in each. The cape and the tails have two slots each,
/// a half that hangs behind and a half that goes over.
/// </remarks>
public enum PaperdollSlot
{
    /// <summary>The half of a cape or cloak that hangs behind them.</summary>
    CapeBack,

    /// <summary>The half of a tail that hangs behind them.</summary>
    TailBack,

    /// <summary>The body, in smallclothes: skin, eyes, shirt, vest and breeches.</summary>
    Body,

    /// <summary>Boots or greaves.</summary>
    Boots,

    /// <summary>A skirt over the breeches, if their look has one.</summary>
    Legs,

    /// <summary>Body armour, or a robe.</summary>
    Armour,

    /// <summary>Gloves or gauntlets.</summary>
    Hands,

    /// <summary>A face other than the body's own.</summary>
    Face,

    /// <summary>A beard.</summary>
    Beard,

    /// <summary>The half of a cape or cloak that goes over them: the drape on the shoulders from the front, all of it from behind.</summary>
    CapeFront,

    /// <summary>The hair.</summary>
    Hair,

    /// <summary>A hat from their look, or a helm they're wearing in its place.</summary>
    Hat,

    /// <summary>A people's ears.</summary>
    Ears,

    /// <summary>A people's horns.</summary>
    Horns,

    /// <summary>The half of a tail that shows over them from behind.</summary>
    TailFront,

    /// <summary>A shield, on the arm that isn't holding the weapon.</summary>
    Shield,

    /// <summary>The weapon in their hand.</summary>
    Weapon,
}
