namespace LegendQuest;

/// <summary>
/// The classes a character can be. ClassCatalog has what each one is.
/// </summary>
/// <remarks>
/// Saves keep a character's class by name, so renaming one breaks saves and reordering them doesn't.
/// The first eight's names are the SRD's; the story calls them something else, and ClassCatalog has
/// that name. The advanced classes after them are the code ids of 10-classes.md §3 in PascalCase. New
/// ones go on the end.
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

    /// <summary>The Watch's way: a defender who holds the line and makes arrests.</summary>
    Watchblade,

    /// <summary>Old Tomas's nets and hooks: control and mending from the back.</summary>
    Netcaster,

    /// <summary>Captain Odelle's crew: a skirmisher with a cutlass and a ship's guns.</summary>
    Deckhand,

    /// <summary>The Rangers' Lodge: bow, hawk, and a count of everything.</summary>
    LodgeRanger,

    /// <summary>The Arcanum Spire's battlefield caster.</summary>
    Battlemage,

    /// <summary>Doomward's bruiser, who fights in ash.</summary>
    Ashwalker,

    /// <summary>Brannock's shield: the best defender there is.</summary>
    Bulwark,

    /// <summary>Mira's healing, which cauterises.</summary>
    EmberMender,

    /// <summary>Kael's thieving and twin cuts.</summary>
    Lanerunner,

    /// <summary>Pedlar Mags's jack of all trades.</summary>
    Wayfarer,

    /// <summary>The Gilded Order's sun knight.</summary>
    GildedKnight,

    /// <summary>Valencia's blood magic, paid for in hit points.</summary>
    Hemomancer,

    /// <summary>Sister Orla's healing, and her bees.</summary>
    Hivekeeper,

    /// <summary>Lady Isadora's illusions of light, out of her journal.</summary>
    Glasswright,

    /// <summary>Mother Anselle's protector-healers.</summary>
    Lamplighter,

    /// <summary>Elara's commander and duellist.</summary>
    Crownsworn,

    /// <summary>Thessaly's time magic, after the coronation.</summary>
    Hourwarden,
}
