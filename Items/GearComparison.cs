using System;
using System.Collections.Generic;
using System.Globalization;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Rules;

namespace LegendQuest.Items;

/// <summary>
/// What putting something on would change for someone, against what they're wearing now: their AC,
/// how surely and how hard their weapons hit, their speed, their most hit points and their spells;
/// and what they ought to know before they do.
/// </summary>
/// <remarks>
/// Worked out the honest way rather than by rules of its own: a copy of them is dressed in it with
/// PartyMember.Equip, so a shield that takes a greatsword out of their hands, or a longsword losing
/// its two-handed die to it, comes out in the numbers. The numbers are CombatStats', the ones a fight
/// uses. Only what changes is listed, and everything listed is better higher. Draws nothing.
/// </remarks>
public static class GearComparison
{
    /// <summary>Two averages closer than this are the same number, not a change.</summary>
    private const double Tolerance = 0.001;

    /// <summary>What changes if someone puts something on in a slot.</summary>
    /// <param name="member">Who, as they're dressed now.</param>
    /// <param name="item">What they'd put on.</param>
    /// <param name="slot">Where it'd go. It has to fit there: ItemDefinition.SlotOn says where.</param>
    /// <returns>Every number that changes, in the order the tooltip lists them. Empty if it's what they're wearing there already.</returns>
    public static IReadOnlyList<GearChange> Compare(PartyMember member, ItemDefinition item, EquipmentSlot slot)
    {
        var changes = new List<GearChange>();
        if (member.WornIn(slot)?.Id == item.Id) return changes;

        PartyMember dressed = Dressed(member, item.Id, slot);
        AttackProfile before = MainHand(member), after = MainHand(dressed);

        Note(changes, "AC", CombatStats.ArmourClassOf(member), CombatStats.ArmourClassOf(dressed), string.Empty);
        Note(changes, "To hit", before.Bonus, after.Bonus, string.Empty);
        Note(changes, "Average damage", before.AverageDamage, after.AverageDamage, string.Empty);
        Note(changes, "Off-hand damage", OffHandDamage(member), OffHandDamage(dressed), string.Empty);
        Note(changes, "Speed", CombatStats.SpeedOf(member), CombatStats.SpeedOf(dressed), " m");
        Note(changes, "Max HP", member.MaxHp, dressed.MaxHp, string.Empty);
        if (CombatStats.CastingAbility(member) is not null)
            Note(changes, "Spell attacks", CombatStats.SpellAttackBonus(member), CombatStats.SpellAttackBonus(dressed), string.Empty);
        return changes;
    }

    /// <summary>What someone ought to know before they put something on: that they aren't trained with it, or aren't strong enough for it.</summary>
    /// <remarks>Neither stops them, since 5e lets anyone pick anything up; it costs them in a fight instead (spec 6.5).</remarks>
    /// <param name="member">Who.</param>
    /// <param name="item">What they'd put on.</param>
    /// <returns>A sentence for each, in the game's voice, or none.</returns>
    public static IReadOnlyList<string> Warnings(PartyMember member, ItemDefinition item)
    {
        var warnings = new List<string>();
        if (!ClassCatalog.Get(member.Class).Proficient(item))
        {
            warnings.Add(item.Kind == ItemKind.Weapon
                ? $"{member.Name} isn't trained with it: no proficiency bonus to hit."
                : $"{member.Name} isn't trained in it: disadvantage on attacks and STR and DEX saves, and no spells.");
        }

        if (item.StrengthNeeded > member.Scores.Strength)
        {
            warnings.Add($"It wants STR {item.StrengthNeeded.ToString(CultureInfo.InvariantCulture)}, and {member.Name} has "
                + $"{member.Scores.Strength.ToString(CultureInfo.InvariantCulture)}: {ItemReadout.Metres(CombatStats.HeavyArmourPenalty)} m slower in it.");
        }
        return warnings;
    }

    /// <summary>A copy of someone, dressed in one thing more. Only what the numbers read is copied.</summary>
    private static PartyMember Dressed(PartyMember member, string id, EquipmentSlot slot)
    {
        var copy = new PartyMember
        {
            Id = member.Id,
            Name = member.Name,
            Class = member.Class,
            Level = member.Level,
            Scores = member.Scores,
            Hp = member.Hp,
            Equipment = new Dictionary<EquipmentSlot, string>(member.Equipment),
        };
        copy.Equip(id, slot);
        return copy;
    }

    /// <summary>How someone attacks with their main hand: the weapon in it, or a fist.</summary>
    private static AttackProfile MainHand(PartyMember member) =>
        CombatStats.Weapon(member, EquipmentSlot.MainHand) ?? CombatStats.Unarmed(member.Scores, member.Level);

    /// <summary>What a weapon in someone's off hand does on average, or 0 for no weapon there.</summary>
    private static double OffHandDamage(PartyMember member) =>
        CombatStats.Weapon(member, EquipmentSlot.OffHand)?.AverageDamage ?? 0.0;

    /// <summary>Lists a number if it changes.</summary>
    private static void Note(List<GearChange> changes, string stat, double before, double after, string unit)
    {
        if (Math.Abs(after - before) > Tolerance) changes.Add(new GearChange(stat, after - before, unit));
    }
}
