using System;
using System.Collections.Generic;
using LegendQuest.Items;

namespace LegendQuest.Rules;

/// <summary>
/// One class: what it's called, its hit die, what it's trained to wear and wield, what it casts
/// with, how fast it moves, and the kit it sets out in. The abilities are the combat engine's.
/// </summary>
public sealed class ClassDefinition
{
    /// <summary>How far anyone moves in a turn, in metres, unless their class says otherwise: 5e's 30 feet, six tiles.</summary>
    public const int DefaultSpeed = 9;

    /// <summary>Which class this is.</summary>
    public required ClassId Id { get; init; }

    /// <summary>What the story calls it on screen. The ClassId is the SRD's name for it.</summary>
    public required string Name { get; init; }

    /// <summary>What it's for, in a few words, for the creation screen.</summary>
    public required string Role { get; init; }

    /// <summary>The biggest number on its hit die. At level 1 a character has this many hit points, plus their Constitution modifier.</summary>
    public required int HitDie { get; init; }

    /// <summary>Where 5e's standard array (15, 14, 13, 12, 10, 8) goes for this class. The creation screen starts you on it.</summary>
    public required AbilityScores Recommended { get; init; }

    /// <summary>The heaviest armour it's trained in. ArmourCategory goes lightest to heaviest, so it's trained in everything up to this too.</summary>
    public ArmourCategory ArmourUpTo { get; init; }

    /// <summary>Whether it's trained with shields.</summary>
    public bool Shields { get; init; }

    /// <summary>Whether it's trained with every martial weapon. Every class is trained with the simple ones.</summary>
    public bool MartialWeapons { get; init; }

    /// <summary>The martial weapons it's trained with when it isn't trained with them all, by item id. A magic one counts if its Base is here.</summary>
    public IReadOnlyList<string> WeaponsAlso { get; init; } = Array.Empty<string>();

    /// <summary>The ability its spells use, for their attack rolls and save DCs, or null for a class that doesn't cast.</summary>
    public Ability? CastingAbility { get; init; }

    /// <summary>What it sets out in, by item id, in the order it's put on: a second light weapon goes in the off hand. ClassCatalog.Kit adds what everyone wears.</summary>
    public required IReadOnlyList<string> StartingKit { get; init; }

    /// <summary>How far it moves in a turn, in metres, before armour and boots.</summary>
    public int Speed { get; init; } = DefaultSpeed;

    /// <summary>Whether it's trained with an item, so it can use it without penalty.</summary>
    /// <remarks>Anything that isn't a weapon, armour or a shield needs no training.</remarks>
    /// <param name="item">The item.</param>
    public bool Proficient(ItemDefinition item) => item.Kind switch
    {
        ItemKind.Weapon => item.Category == WeaponCategory.Simple || MartialWeapons || Trained(item.Id) || Trained(item.Base),
        ItemKind.Armour => item.Armour <= ArmourUpTo,
        ItemKind.Shield => Shields,
        _ => true,
    };

    /// <summary>Why someone of this class couldn't set out in a kit, or null if they could.</summary>
    /// <remarks>
    /// Every item has to exist, be something you wear, be something this class is trained in, and
    /// have a slot to itself: a second light weapon goes in the off hand, and nothing goes there
    /// beside a two-handed weapon.
    /// </remarks>
    /// <param name="kit">Item ids, in the order they're put on.</param>
    public string KitProblem(IReadOnlyList<string> kit)
    {
        var worn = new Dictionary<EquipmentSlot, string>();

        foreach (string id in kit)
        {
            if (!ItemCatalog.Exists(id)) return $"has {id} in it, which isn't an item";

            ItemDefinition item = ItemCatalog.Get(id);
            if (item.SlotOn(worn) is not EquipmentSlot slot) return $"has {id} in it, which isn't something you wear";
            if (!Proficient(item)) return $"has {id} in it, which a {Id} isn't trained with";
            if (!worn.TryAdd(slot, id)) return $"has two things for the {slot} slot";
        }

        bool twoHanded = worn.TryGetValue(EquipmentSlot.MainHand, out string main) && ItemCatalog.Get(main).Has(WeaponProperty.TwoHanded);
        return twoHanded && worn.ContainsKey(EquipmentSlot.OffHand) ? $"has something in the off hand beside the two-handed {main}" : null;
    }

    /// <summary>Whether a weapon's id is one of WeaponsAlso.</summary>
    /// <param name="id">The id, or null for a weapon with no Base.</param>
    private bool Trained(string id)
    {
        foreach (string weapon in WeaponsAlso)
            if (weapon == id) return true;
        return false;
    }
}
