using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Items;

namespace LegendQuest.Rules;

/// <summary>
/// Works out someone's armour class from what they're wearing.
/// </summary>
/// <remarks>
/// The spec's table (6.5), which is the SRD 5.1's: the body armour's base, plus Dexterity (all of it
/// in light armour or none, up to +2 in medium, none at all in heavy), plus every AcBonus worn: a
/// shield's +2, a magic cloak's or ring's +1. With nothing on the body it's 10 plus Dexterity.
/// Wearing armour you aren't trained in doesn't change it; it costs you in combat instead. Class
/// features that add to it, like the Blademaster's Defence, are the combat engine's.
/// </remarks>
public static class ArmourClass
{
    /// <summary>The AC of someone with nothing on, before their Dexterity.</summary>
    public const int Unarmoured = 10;

    /// <summary>Someone's AC, as they're dressed now.</summary>
    /// <param name="member">Who.</param>
    public static int Of(PartyMember member) => Of(member.Scores, member.Worn);

    /// <summary>The AC someone with these scores would have in these things, for comparing before you equip something.</summary>
    /// <param name="scores">Their ability scores.</param>
    /// <param name="worn">Everything they'd be wearing and holding.</param>
    public static int Of(AbilityScores scores, IEnumerable<ItemDefinition> worn)
    {
        int dexterity = scores.Modifier(Ability.Dexterity);
        int body = Unarmoured + dexterity;
        int bonuses = 0;

        foreach (ItemDefinition item in worn)
        {
            if (item.Kind == ItemKind.Armour) body = item.BaseAc + DexterityIn(item, dexterity);
            bonuses += item.AcBonus;
        }

        return body + bonuses;
    }

    /// <summary>How much of someone's Dexterity a suit of armour lets count.</summary>
    /// <remarks>Heavy armour's MaxDex is 0, and it ignores a Dexterity penalty as well as a bonus.</remarks>
    /// <param name="armour">The armour.</param>
    /// <param name="dexterity">Their Dexterity modifier.</param>
    private static int DexterityIn(ItemDefinition armour, int dexterity) => armour.MaxDex switch
    {
        null => dexterity,
        0 => 0,
        int most => Math.Min(dexterity, most),
    };
}
