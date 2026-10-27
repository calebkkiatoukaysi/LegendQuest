namespace LegendQuest;

/// <summary>
/// The classes a character can be. ClassCatalog has what each one is.
/// </summary>
/// <remarks>Saves keep a character's class by name, so renaming one breaks saves and reordering them doesn't.</remarks>
public enum ClassId
{
    /// <summary>Armour, weapons, and getting back up.</summary>
    Fighter,

    /// <summary>Quick, quiet, and dangerous to anyone already busy with somebody else.</summary>
    Rogue,

    /// <summary>Heals, and calls down light on the dead.</summary>
    Cleric,

    /// <summary>Spells out of a book.</summary>
    Wizard,
}
