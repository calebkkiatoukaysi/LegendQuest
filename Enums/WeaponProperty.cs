using System;

namespace LegendQuest;

/// <summary>
/// What's special about a weapon, from the SRD 5.1's weapons table. A weapon can have several.
/// </summary>
/// <remarks>Flags, so a dagger is Finesse | Light | Thrown. ItemDefinition.Has checks for one.</remarks>
[Flags]
public enum WeaponProperty
{
    /// <summary>Nothing special.</summary>
    None = 0,

    /// <summary>Attacks with it can use Dexterity instead of Strength.</summary>
    Finesse = 1,

    /// <summary>Small enough to fight with one in each hand.</summary>
    Light = 2,

    /// <summary>Too big for small people to swing well.</summary>
    Heavy = 4,

    /// <summary>Needs both hands, so nothing goes in the off hand.</summary>
    TwoHanded = 8,

    /// <summary>Can be held in both hands for its VersatileDamage.</summary>
    Versatile = 16,

    /// <summary>Shoots: its Range is how far it shoots, and an enemy next to you gives disadvantage.</summary>
    Ranged = 32,

    /// <summary>A melee weapon that can be thrown, out to its ThrowRange.</summary>
    Thrown = 64,

    /// <summary>Reaches a tile further than other melee weapons.</summary>
    Reach = 128,

    /// <summary>Takes so long to load that it's one shot a turn, whatever else you could do.</summary>
    Loading = 256,
}
