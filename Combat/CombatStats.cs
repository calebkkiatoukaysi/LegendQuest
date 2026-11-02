using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// A party member's numbers for a fight, worked out from their class, scores and what they're
/// wearing: AC, speed, saves, spellcasting, and how each hand attacks.
/// </summary>
/// <remarks>
/// 5e's rules from the SRD 5.1. Armour's AC is the Rules' ArmourClass; the Defence fighting style is
/// added here because it's a class feature, not the armour's. Heavy armour you aren't strong enough
/// for costs 3 m of movement, and armour you aren't trained in puts disadvantage on attacks and
/// Strength and Dexterity saves and stops spells (spec 6.5). Training is the class's own Proficient,
/// so a magic weapon counts as the one it's a finer version of.
/// </remarks>
public static class CombatStats
{
    /// <summary>What heavy armour costs someone too weak for it, in metres (ten feet).</summary>
    public const float HeavyArmourPenalty = 3f;

    /// <summary>What an unarmed strike does before Strength: 1, the SRD's.</summary>
    private const int UnarmedDamage = 1;

    /// <summary>What the Defence fighting style adds to AC in armour.</summary>
    private const int DefenceBonus = 1;

    /// <summary>The two saving throws each class is proficient in, from the SRD. Here rather than on ClassDefinition since only a fight rolls saves.</summary>
    private static readonly Dictionary<ClassId, Ability[]> Saves = new()
    {
        [ClassId.Fighter] = new[] { Ability.Strength, Ability.Constitution },
        [ClassId.Rogue] = new[] { Ability.Dexterity, Ability.Intelligence },
        [ClassId.Cleric] = new[] { Ability.Wisdom, Ability.Charisma },
        [ClassId.Wizard] = new[] { Ability.Intelligence, Ability.Wisdom },
        [ClassId.Paladin] = new[] { Ability.Wisdom, Ability.Charisma },
        [ClassId.Druid] = new[] { Ability.Intelligence, Ability.Wisdom },
        [ClassId.Warlock] = new[] { Ability.Wisdom, Ability.Charisma },
        [ClassId.Bard] = new[] { Ability.Dexterity, Ability.Charisma },
    };

    /// <summary>Checks every class has its saves.</summary>
    /// <remarks>Throws on purpose, so a class added to the enum and not here shows up when the game starts.</remarks>
    static CombatStats()
    {
        foreach (ClassId id in Enum.GetValues<ClassId>())
            if (!Saves.TryGetValue(id, out Ability[] saves) || saves.Length != 2)
                throw new InvalidOperationException($"Class {id} needs its two saving throws in CombatStats.");
    }

    /// <summary>The short name of an ability, the way the log and the dice show it: STR, DEX.</summary>
    /// <param name="ability">The ability.</param>
    public static string Abbreviation(Ability ability) => ability.ToString()[..3].ToUpperInvariant();

    /// <summary>Their armour class: the armour's, plus Defence if their class has it and they're wearing armour.</summary>
    /// <param name="member">Who.</param>
    public static int ArmourClassOf(PartyMember member)
    {
        int ac = ArmourClass.Of(member);
        bool defence = AbilityCatalog.Kit(member.Class).Contains(AbilityId.Defence);
        return defence && member.WornIn(EquipmentSlot.Body) is { Armour: not ArmourCategory.None } ? ac + DefenceBonus : ac;
    }

    /// <summary>How far they can move in a turn, in metres: the class's speed, plus anything worn that adds to it, less heavy armour's cost.</summary>
    /// <param name="member">Who.</param>
    public static float SpeedOf(PartyMember member)
    {
        float speed = ClassCatalog.Get(member.Class).Speed;
        foreach (ItemDefinition item in member.Worn) speed += item.SpeedBonus;

        if (member.WornIn(EquipmentSlot.Body) is ItemDefinition body && body.StrengthNeeded > member.Scores.Strength)
            speed -= HeavyArmourPenalty;
        return MathF.Max(0f, speed);
    }

    /// <summary>Whether they're wearing armour, or carrying a shield, their class isn't trained in.</summary>
    /// <param name="member">Who.</param>
    public static bool ArmourUntrained(PartyMember member)
    {
        ClassDefinition kind = ClassCatalog.Get(member.Class);
        foreach (ItemDefinition item in member.Worn)
            if (item.Kind is (ItemKind.Armour or ItemKind.Shield) && !kind.Proficient(item)) return true;
        return false;
    }

    /// <summary>What everything they're wearing adds to their spell attacks (the circlet of focus).</summary>
    /// <param name="member">Who.</param>
    public static int SpellAttackBonus(PartyMember member)
    {
        int bonus = 0;
        foreach (ItemDefinition item in member.Worn) bonus += item.SpellAttackBonus;
        return bonus;
    }

    /// <summary>What they add to a saving throw: the modifier, and proficiency if their class has the save.</summary>
    /// <param name="member">Who.</param>
    /// <param name="ability">Which save.</param>
    public static int SaveBonus(PartyMember member, Ability ability) =>
        member.Scores.Modifier(ability) + (Array.IndexOf(Saves[member.Class], ability) >= 0 ? AttackMath.ProficiencyBonus : 0);

    /// <summary>The ability their class casts with, or null if it doesn't.</summary>
    /// <param name="member">Who.</param>
    public static Ability? CastingAbility(PartyMember member) => ClassCatalog.Get(member.Class).CastingAbility;

    /// <summary>How they attack with what's in a hand, or null if it isn't a weapon.</summary>
    /// <remarks>
    /// Finesse uses the better of Strength and Dexterity, ranged uses Dexterity, everything else
    /// Strength. A versatile weapon does its bigger die with the other hand empty. The off hand adds no
    /// ability modifier to its damage unless the modifier is negative, as in 5e.
    /// </remarks>
    /// <param name="member">Who.</param>
    /// <param name="slot">MainHand or OffHand.</param>
    public static AttackProfile Weapon(PartyMember member, EquipmentSlot slot)
    {
        if (member.WornIn(slot) is not { Kind: ItemKind.Weapon } item) return null;

        bool ranged = item.Has(WeaponProperty.Ranged);
        bool finesse = item.Has(WeaponProperty.Finesse);
        int strength = member.Scores.Modifier(Ability.Strength), dexterity = member.Scores.Modifier(Ability.Dexterity);
        Ability ability = ranged || finesse && dexterity > strength ? Ability.Dexterity : Ability.Strength;
        int modifier = member.Scores.Modifier(ability);

        int proficiency = ClassCatalog.Get(member.Class).Proficient(item) ? AttackMath.ProficiencyBonus : 0;

        var modifiers = new List<(string, int)> { (Abbreviation(ability), modifier) };
        if (proficiency != 0) modifiers.Add(("PROF", proficiency));
        if (item.AttackBonus != 0) modifiers.Add(("MAGIC", item.AttackBonus));

        bool twoHands = item.Has(WeaponProperty.Versatile) && slot == EquipmentSlot.MainHand && member.WornIn(EquipmentSlot.OffHand) is null;
        DiceExpression dice = twoHands && !item.VersatileDamage.IsNone ? item.VersatileDamage : item.Damage;
        int flat = (slot == EquipmentSlot.OffHand ? Math.Min(0, modifier) : modifier) + item.DamageBonus + WornDamageBonus(member);

        return new AttackProfile(item.Name, modifier + proficiency + item.AttackBonus, modifiers, dice with { Bonus = dice.Bonus + flat },
            item.DamageType, item.ExtraDamage, item.ExtraDamageType, item.Range > 0f ? item.Range : Measure.MeleeReach, item.Properties);
    }

    /// <summary>A fist, for anyone with nothing better in reach: 1 + Strength bludgeoning, and everyone's proficient.</summary>
    /// <param name="scores">Whose scores.</param>
    public static AttackProfile Unarmed(AbilityScores scores)
    {
        int strength = scores.Modifier(Ability.Strength);
        var modifiers = new List<(string, int)> { ("STR", strength), ("PROF", AttackMath.ProficiencyBonus) };
        return new AttackProfile("Unarmed Strike", strength + AttackMath.ProficiencyBonus, modifiers,
            new DiceExpression(0, 0, Math.Max(UnarmedDamage, UnarmedDamage + strength)), DamageType.Bludgeoning,
            DiceExpression.None, DamageType.Bludgeoning, Measure.MeleeReach, WeaponProperty.None);
    }

    /// <summary>One of a foe's attacks, as a profile.</summary>
    /// <param name="attack">The attack from its stat block.</param>
    public static AttackProfile Of(EnemyAttack attack) =>
        new(attack.Name, attack.Bonus, new List<(string, int)> { ("ATTACK", attack.Bonus) }, attack.Damage, attack.DamageType,
            DiceExpression.None, attack.DamageType, attack.Range, attack.Properties);

    /// <summary>The damage bonus from anything worn that isn't a weapon (gauntlets of might), which every weapon hit gets.</summary>
    private static int WornDamageBonus(PartyMember member)
    {
        int bonus = 0;
        foreach (ItemDefinition item in member.Worn)
            if (item.Kind != ItemKind.Weapon) bonus += item.DamageBonus;
        return bonus;
    }
}
