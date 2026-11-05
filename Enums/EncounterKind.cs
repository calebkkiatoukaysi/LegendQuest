namespace LegendQuest;

/// <summary>
/// What sort of fight an encounter is: which win rate the simulator holds it to, and what it's
/// worth.
/// </summary>
/// <remarks>11-combat.md §6.2's kinds. New ones go on the end.</remarks>
public enum EncounterKind
{
    /// <summary>A fight the story brings.</summary>
    Story,

    /// <summary>The first fight: the tips play, and it's tuned so nobody loses it.</summary>
    Tutorial,

    /// <summary>A fight met on the road, not written into the story.</summary>
    Random,

    /// <summary>A strong foe partway through a chapter.</summary>
    MiniBoss,

    /// <summary>A chapter's or an act's boss: its own song.</summary>
    Boss,

    /// <summary>The last fight.</summary>
    Sovereign,
}
