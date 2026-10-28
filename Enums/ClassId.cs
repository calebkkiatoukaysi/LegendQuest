namespace LegendQuest;

/// <summary>
/// The classes a character can be. ClassCatalog has what each one is.
/// </summary>
/// <remarks>
/// Saves keep a character's class by name, so renaming one breaks saves and reordering them doesn't.
/// The names are the SRD's; the story calls them something else, and ClassCatalog has that name.
/// New ones go on the end.
/// </remarks>
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

    /// <summary>Oath-bound: a shield for the party and a healer's hands.</summary>
    Paladin,

    /// <summary>Nature magic, and a beast at their side.</summary>
    Druid,

    /// <summary>Pact magic that drains the life out of things.</summary>
    Warlock,

    /// <summary>Songs that lift the party and talk that wins arguments.</summary>
    Bard,
}
