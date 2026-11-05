namespace LegendQuest;

/// <summary>
/// The conditions a fight can put on someone. Each shows as an icon on the party strip and the
/// initiative bar, in this order.
/// </summary>
/// <remarks>
/// 5e's from the SRD 5.1, cut down to the ones the level-1 kits and the enemies so far can cause,
/// plus Helped and Mocked, which are the Help action's advantage and Vicious Mockery's disadvantage
/// given a name so they can be seen. New ones go on the end.
/// </remarks>
public enum ConditionId
{
    /// <summary>At 0 hit points and out of the fight: a party member rolls death saves until they're healed, helped up, stable or dead.</summary>
    Downed,

    /// <summary>On the ground: melee attacks against them have advantage, ranged ones disadvantage, and their own attacks have disadvantage. Getting up takes their Shift.</summary>
    Prone,

    /// <summary>They can't Shift on their next turn (Ray of Frost).</summary>
    Slowed,

    /// <summary>Leaving the front row doesn't give anyone an opportunity attack, for the rest of this turn.</summary>
    Disengaged,

    /// <summary>Attacks against them have disadvantage, and their Dexterity saves advantage, until the start of their next turn.</summary>
    Dodging,

    /// <summary>Held where they stand: no Shift, attacks against them have advantage, theirs have disadvantage. A Strength save at the end of each of their turns breaks it (Entangle).</summary>
    Restrained,

    /// <summary>Cursed: whoever hexed them does an extra 1d6 necrotic every time they hit them.</summary>
    Hexed,

    /// <summary>Holding a bardic inspiration die, added to the first attack roll or save that would otherwise fail by no more than the die can make up.</summary>
    Inspired,

    /// <summary>Downed but no longer dying: no more death saves, but still out until healed.</summary>
    Stable,

    /// <summary>Someone's helping: advantage on their next attack roll before the end of their next turn.</summary>
    Helped,

    /// <summary>Rattled by Vicious Mockery: disadvantage on their next attack roll before the end of their next turn.</summary>
    Mocked,

    /// <summary>The next attack on them has advantage, and uses the mark up.</summary>
    Marked,

    /// <summary>Their single-target attacks have to go at whoever taunted them (the Source) whenever they can.</summary>
    Taunted,

    /// <summary>On fire: the condition's die in fire damage at the start of each of their turns.</summary>
    Burning,

    /// <summary>Bleeding: the condition's die in damage at the start of each of their turns.</summary>
    Bleeding,

    /// <summary>Stopped in time: they skip their turns, and only an area (any shape but one foe or one ally) can reach them.</summary>
    Frozen,

    /// <summary>The condition's die in poison damage at the start of each of their turns, and disadvantage on their attacks.</summary>
    Poisoned,

    /// <summary>They skip their turns, and attacks on them have advantage.</summary>
    Stunned,

    /// <summary>Disadvantage on their attacks, and attacks on them have advantage.</summary>
    Blinded,

    /// <summary>Disadvantage on their attacks.</summary>
    Frightened,

    /// <summary>They can't attack whoever charmed them (the Source).</summary>
    Charmed,
}
