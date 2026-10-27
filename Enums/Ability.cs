namespace LegendQuest;

/// <summary>
/// The six ability scores, the same as 5e's.
/// </summary>
/// <remarks>Saves keep a character's scores by these names, so renaming one breaks saves and reordering them doesn't.</remarks>
public enum Ability
{
    /// <summary>Strength: hitting hard, lifting, forcing doors.</summary>
    Strength,

    /// <summary>Dexterity: balance, aim, quick hands, light armour.</summary>
    Dexterity,

    /// <summary>Constitution: toughness. It adds to hit points.</summary>
    Constitution,

    /// <summary>Intelligence: learning and reasoning. A wizard casts with it.</summary>
    Intelligence,

    /// <summary>Wisdom: noticing things and reading people. A cleric casts with it.</summary>
    Wisdom,

    /// <summary>Charisma: talking people round.</summary>
    Charisma,
}
