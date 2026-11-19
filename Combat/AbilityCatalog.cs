using System;

namespace LegendQuest.Combat;

/// <summary>
/// What's left of the turn-based ability table: which of a stat block's AbilityIds every fighter has
/// already, so EnemyCatalog can refuse a foe that lists one.
/// </summary>
/// <remarks>
/// The kits themselves are SkillCatalog's (by skill id); a foe's AbilityIds now only name what its
/// stat block does in the real-time engine (Multiattack, Pack Tactics, Sneak Attack, its moves).
/// </remarks>
public static class AbilityCatalog
{
    /// <summary>What every fighter has without a stat block saying so: the auto attack, dodging, the potion, helping someone up, the bag's throwables, and the Soul-Link.</summary>
    private static readonly AbilityId[] Common =
    {
        AbilityId.Attack, AbilityId.Dodge, AbilityId.DrinkPotion, AbilityId.Help, AbilityId.ThrowAlchemistsFire, AbilityId.DrinkElixir, AbilityId.SoulLinkStrike,
    };

    /// <summary>Whether everyone has an ability already, so a stat block mustn't list it.</summary>
    /// <param name="id">The ability.</param>
    public static bool IsCommon(AbilityId id) => Array.IndexOf(Common, id) >= 0;
}
