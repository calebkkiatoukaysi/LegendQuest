namespace LegendQuest;

/// <summary>
/// How rare an item is. Anything magic is at least Uncommon; the icons show it with a glint.
/// </summary>
/// <remarks>The DMG's scale goes higher, but nothing in Act 1 needs it to. New ones go on the end.</remarks>
public enum Rarity
{
    /// <summary>Anyone sells it.</summary>
    Common,

    /// <summary>Magic, or finely made.</summary>
    Uncommon,

    /// <summary>One of a kind, near enough.</summary>
    Rare,
}
