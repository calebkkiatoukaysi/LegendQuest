namespace LegendQuest;

/// <summary>
/// What kind of hurt something does: the SRD 5.1's damage types.
/// </summary>
/// <remarks>The combat log names it, and later resistances and weaknesses read it. New ones go on the end.</remarks>
public enum DamageType
{
    /// <summary>Blades and axes.</summary>
    Slashing,

    /// <summary>Points: daggers, spears, arrows, teeth.</summary>
    Piercing,

    /// <summary>Blunt things: maces, hammers, staves, falls.</summary>
    Bludgeoning,

    /// <summary>Flame.</summary>
    Fire,

    /// <summary>Frost and ice.</summary>
    Cold,

    /// <summary>Lightning.</summary>
    Lightning,

    /// <summary>A blast of sound.</summary>
    Thunder,

    /// <summary>Holy light, and sunlight.</summary>
    Radiant,

    /// <summary>The cold that drains life out of things.</summary>
    Necrotic,

    /// <summary>Venom and toxins.</summary>
    Poison,

    /// <summary>An attack on the mind.</summary>
    Psychic,

    /// <summary>Pure magic.</summary>
    Force,

    /// <summary>Things that eat through other things.</summary>
    Acid,
}
