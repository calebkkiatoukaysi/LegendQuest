using System;
using System.Collections.Generic;
using System.Globalization;
using LegendQuest.Combat;

namespace LegendQuest.Items;

/// <summary>
/// An item put into words for its tooltip: what sort of thing it is and where it's worn, and its
/// numbers, written the way the SRD writes them.
/// </summary>
/// <remarks>
/// Draws nothing, so the inventory, the shop and anything later that shows an item all say the same
/// thing about it. Each number is a line of its own, so a tooltip can set them one under another.
/// Distances are in metres, the way the combat grid counts them.
/// </remarks>
public static class ItemReadout
{
    /// <summary>What goes between the parts of the kind line.</summary>
    private const string Separator = "  ·  ";

    /// <summary>What a weapon's properties are called, in the order they're listed.</summary>
    private static readonly (WeaponProperty Property, string Name)[] PropertyNames =
    {
        (WeaponProperty.Finesse, "Finesse"),
        (WeaponProperty.Light, "Light"),
        (WeaponProperty.Heavy, "Heavy"),
        (WeaponProperty.TwoHanded, "Two-handed"),
        (WeaponProperty.Versatile, "Versatile"),
        (WeaponProperty.Ranged, "Ranged"),
        (WeaponProperty.Thrown, "Thrown"),
        (WeaponProperty.Reach, "Reach"),
        (WeaponProperty.Loading, "Loading"),
    };

    /// <summary>What sort of thing it is and where it's worn, like "Uncommon  ·  Martial weapon  ·  Main hand".</summary>
    /// <param name="item">The item.</param>
    public static string Kind(ItemDefinition item)
    {
        var parts = new List<string>();
        if (item.Rarity != Rarity.Common) parts.Add(item.Rarity.ToString());
        parts.Add(KindName(item));
        if (item.Slot is EquipmentSlot slot) parts.Add(item.Fits(EquipmentSlot.OffHand) && slot != EquipmentSlot.OffHand ? "Either hand" : SlotName(slot));
        return string.Join(Separator, parts);
    }

    /// <summary>A slot's name, the way the tooltips write it: "Main hand", "Body".</summary>
    /// <param name="slot">The slot.</param>
    public static string SlotName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.MainHand => "Main hand",
        EquipmentSlot.OffHand => "Off hand",
        _ => slot.ToString(),
    };

    /// <summary>Its numbers, a line each: damage, properties and reach for a weapon, AC and what it asks of you for armour, what it adds when it's worn, and what drinking or throwing it does.</summary>
    /// <param name="item">The item.</param>
    public static IReadOnlyList<string> Stats(ItemDefinition item)
    {
        var lines = new List<string>();
        switch (item.Kind)
        {
            case ItemKind.Weapon:
                WeaponLines(item, lines);
                break;
            case ItemKind.Armour:
                ArmourLines(item, lines);
                break;
            case ItemKind.Consumable:
                ConsumableLines(item, lines);
                break;
        }

        if (item.Slot is not null) WornLines(item, lines);
        return lines;
    }

    /// <summary>A whole number with its sign: +1, -2, +0.</summary>
    /// <param name="value">The number.</param>
    public static string Signed(int value) => (value < 0 ? "-" : "+") + Math.Abs(value).ToString(CultureInfo.InvariantCulture);

    /// <summary>A distance in metres, without a trailing .0: 1.5, 24.</summary>
    /// <param name="metres">The distance.</param>
    public static string Metres(float metres) => metres.ToString("0.#", CultureInfo.InvariantCulture);

    /// <summary>What it is, without its rarity or slot.</summary>
    private static string KindName(ItemDefinition item) => item.Kind switch
    {
        ItemKind.Weapon => item.Category + " weapon",
        ItemKind.Armour => item.Armour == ArmourCategory.None ? "Clothes" : item.Armour + " armour",
        ItemKind.Head => "Headgear",
        ItemKind.Hands => "Gloves",
        ItemKind.Feet => "Boots",
        ItemKind.Consumable when !item.Heals.IsNone => "Potion",
        ItemKind.Consumable when !item.ThrownDamage.IsNone => "Thrown flask",
        ItemKind.Consumable => "Elixir",
        ItemKind.Material => "Trade goods",
        _ => item.Kind.ToString(),
    };

    /// <summary>A weapon's damage, its magic, its properties and how far it reaches.</summary>
    private static void WeaponLines(ItemDefinition item, List<string> lines)
    {
        string damage = item.Damage + " " + Lower(item.DamageType);
        if (!item.VersatileDamage.IsNone) damage += ", " + item.VersatileDamage + " in two hands";
        lines.Add(damage);

        if (item.AttackBonus > 0 && item.AttackBonus == item.DamageBonus) lines.Add(Signed(item.AttackBonus) + " to hit and damage");
        else
        {
            if (item.AttackBonus != 0) lines.Add(Signed(item.AttackBonus) + " to hit");
            if (item.DamageBonus != 0) lines.Add(Signed(item.DamageBonus) + " damage");
        }
        if (!item.ExtraDamage.IsNone) lines.Add("+" + item.ExtraDamage + " " + Lower(item.ExtraDamageType));

        var properties = new List<string>();
        foreach ((WeaponProperty property, string name) in PropertyNames)
            if (item.Has(property)) properties.Add(name);
        if (properties.Count > 0) lines.Add(string.Join(", ", properties));

        string reach = (item.Has(WeaponProperty.Ranged) ? "Range " : "Reach ") + Metres(item.Range) + " m";
        if (item.ThrowRange > 0f) reach += Separator + "thrown " + Metres(item.ThrowRange) + " m";
        lines.Add(reach);
    }

    /// <summary>Body armour's AC, how much Dexterity counts in it, and what it asks of whoever wears it.</summary>
    private static void ArmourLines(ItemDefinition item, List<string> lines)
    {
        string ac = "AC " + item.BaseAc.ToString(CultureInfo.InvariantCulture);
        lines.Add(item.MaxDex switch
        {
            null => ac + " + DEX",
            0 => ac,
            int most => ac + " + DEX (up to " + Signed(most) + ")",
        });

        if (item.StrengthNeeded > 0)
            lines.Add("Needs STR " + item.StrengthNeeded.ToString(CultureInfo.InvariantCulture) + ", or " + Metres(CombatStats.HeavyArmourPenalty) + " m slower");
        if (item.StealthDisadvantage) lines.Add("Disadvantage on Stealth");
    }

    /// <summary>What drinking or throwing it does.</summary>
    private static void ConsumableLines(ItemDefinition item, List<string> lines)
    {
        if (!item.Heals.IsNone)
            lines.Add("Heals " + item.Heals + ", " + item.Heals.Average.ToString("0.#", CultureInfo.InvariantCulture) + " on average");
        if (!item.ThrownDamage.IsNone)
            lines.Add("Thrown " + Metres(item.ThrowRange) + " m: " + item.ThrownDamage + " " + Lower(item.ThrownDamageType));
        if (item.SlotsRestored > 0)
            lines.Add("Gives back " + item.SlotsRestored.ToString(CultureInfo.InvariantCulture) + " spent spell slot" + (item.SlotsRestored == 1 ? string.Empty : "s"));
    }

    /// <summary>What wearing it adds: AC, damage, speed, hit points, spell attacks.</summary>
    private static void WornLines(ItemDefinition item, List<string> lines)
    {
        if (item.AcBonus != 0) lines.Add(Signed(item.AcBonus) + " AC");
        if (item.Kind != ItemKind.Weapon && item.DamageBonus != 0) lines.Add(Signed(item.DamageBonus) + " damage with every weapon");
        if (item.SpeedBonus != 0f) lines.Add("+" + Metres(item.SpeedBonus) + " m speed");
        if (item.MaxHpBonus != 0) lines.Add(Signed(item.MaxHpBonus) + " max HP");
        if (item.SpellAttackBonus != 0) lines.Add(Signed(item.SpellAttackBonus) + " to spell attacks");
    }

    /// <summary>A damage type in lower case, the way it's read in a sentence.</summary>
    private static string Lower(DamageType type) => type.ToString().ToLowerInvariant();
}
