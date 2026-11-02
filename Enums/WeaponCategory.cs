namespace LegendQuest;

/// <summary>
/// Whether a weapon's one anyone can pick up or one that takes training, as the SRD 5.1 sorts them.
/// </summary>
/// <remarks>Every class here is trained in simple weapons; ClassDefinition says which can use martial ones.</remarks>
public enum WeaponCategory
{
    /// <summary>Daggers, maces, staves, spears, shortbows: anyone can use them.</summary>
    Simple,

    /// <summary>Swords, axes, hammers, longbows: they take training.</summary>
    Martial,
}
