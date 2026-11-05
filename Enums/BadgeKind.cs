namespace LegendQuest;

/// <summary>
/// The badge floating over a door, or beside a name, that says what's there (12-rooms §2.2). Zone
/// files name them in lower case with a hyphen, so StairUp is stair-up.
/// </summary>
public enum BadgeKind
{
    /// <summary>A coin purse: somewhere to buy things.</summary>
    Shop,

    /// <summary>An anvil: a smith or an armourer.</summary>
    Anvil,

    /// <summary>A bottle: an alchemist or an apothecary.</summary>
    Bottle,

    /// <summary>A tankard: an inn or a tavern.</summary>
    Inn,

    /// <summary>Crossed swords: a class trainer.</summary>
    Trainer,

    /// <summary>A scroll: the Guild, an archive, a library.</summary>
    Scroll,

    /// <summary>A shield: the Watch House, a barracks.</summary>
    Shield,

    /// <summary>A sun: a temple, an abbey, a shrine.</summary>
    Sun,

    /// <summary>A crown: the palace.</summary>
    Crown,

    /// <summary>A plain door: a home, or anywhere else you can go in. Only shown on hover or with Alt.</summary>
    Door,

    /// <summary>A gate, an arch or a great door into somewhere big.</summary>
    Gate,

    /// <summary>A stair up.</summary>
    StairUp,

    /// <summary>A stair down.</summary>
    StairDown,
}
