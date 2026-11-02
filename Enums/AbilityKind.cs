namespace LegendQuest;

/// <summary>
/// How an ability works when it's used: what the engine rolls and what it does with the result.
/// </summary>
public enum AbilityKind
{
    /// <summary>An attack roll with the weapon in the main hand (or a foe's attacks) against AC.</summary>
    Weapon,

    /// <summary>An attack roll with the light weapon in the off hand, without the ability modifier on the damage.</summary>
    OffHand,

    /// <summary>An attack roll with something thrown from the bag, using its thrown damage and range.</summary>
    Thrown,

    /// <summary>A spell attack roll (proficiency and the casting modifier) against AC.</summary>
    SpellAttack,

    /// <summary>Every target makes a saving throw against the user's DC, or takes the damage and the conditions.</summary>
    Save,

    /// <summary>Damage that always lands, a die at a time (Magic Missile's darts).</summary>
    AutoHit,

    /// <summary>Hit points back, which gets a Downed ally up.</summary>
    Heal,

    /// <summary>Something drunk from the party's bag: the ability's own item if it names one, or else the smallest healing potion that makes up what's missing (the biggest there is, if none does).</summary>
    Potion,

    /// <summary>Puts its conditions on the target, with no roll.</summary>
    Condition,

    /// <summary>As much movement again.</summary>
    Dash,

    /// <summary>Gets a Downed ally up with 1 HP, or helps one who's standing.</summary>
    Help,

    /// <summary>The Soul-Link Strike: both of the duo's weapons and radiant light, on a sure hit.</summary>
    SoulLink,

    /// <summary>Always on: it changes other rolls rather than being used.</summary>
    Passive,
}
