namespace LegendQuest;

/// <summary>
/// How heavy a suit of armour is, which decides how much Dexterity it lets through and who's trained to wear it.
/// </summary>
/// <remarks>
/// The SRD 5.1's categories, plus None for clothes and robes, which anyone can wear. They go
/// lightest to heaviest, so ClassDefinition compares them: a class trained up to Medium can wear
/// None, Light and Medium. Keep that order.
/// </remarks>
public enum ArmourCategory
{
    /// <summary>Clothes and robes: 10 plus Dexterity, the same as nothing at all.</summary>
    None,

    /// <summary>Padded and leather: all of Dexterity counts.</summary>
    Light,

    /// <summary>Hide, a chain shirt, scale: Dexterity counts up to +2.</summary>
    Medium,

    /// <summary>Chain mail and plate: Dexterity doesn't count, and it's slow without the Strength for it.</summary>
    Heavy,
}
