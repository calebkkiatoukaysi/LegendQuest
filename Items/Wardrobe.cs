using System;
using System.Collections.Generic;
using LegendQuest.Characters;
using LegendQuest.Combat;

namespace LegendQuest.Items;

/// <summary>
/// Dressing the party out of the bag and drinking from it: what goes where, what comes off, and why
/// something can't be used.
/// </summary>
/// <remarks>
/// The rules of the hands are PartyMember's (which slot, what a two-handed weapon takes with it);
/// this is the bag's side of them, so nothing is ever in two places at once or lost: whatever comes
/// off goes back in the bag, and whatever goes on comes out of it. Being untrained stops nobody
/// putting something on, since 5e lets anyone pick anything up; GearComparison's warnings say what
/// it'll cost instead. Draws nothing and takes its Random, like the rest of the rules.
/// </remarks>
public static class Wardrobe
{
    /// <summary>Why someone can't use something from the bag out of a fight, or null if they can: put it on if it's worn, drink it if it's drunk.</summary>
    /// <param name="member">Who'd use it.</param>
    /// <param name="item">What.</param>
    /// <returns>A sentence in the game's voice, or null.</returns>
    public static string WhyNot(PartyMember member, ItemDefinition item)
    {
        if (item.Slot is not null) return null;
        if (item.Kind == ItemKind.Material) return $"{item.Name} is for selling, not wearing.";
        if (!item.ThrownDamage.IsNone) return $"{item.Name} is for throwing, and that's for a fight.";
        if (!item.Heals.IsNone && member.Hp >= member.MaxHp) return $"{member.Name} isn't hurt.";
        if (item.SlotsRestored > 0 && ResourceCatalog.SlotsSpent(member) == 0) return $"{member.Name} has no spent spell slots.";
        return null;
    }

    /// <summary>Puts something from the bag on someone, where SlotOn says it goes, and puts whatever that takes off back in the bag.</summary>
    /// <param name="member">Who.</param>
    /// <param name="bag">The party's bag. It has to have one.</param>
    /// <param name="id">The item. It has to be something worn.</param>
    /// <returns>What came off, in the bag now.</returns>
    public static IReadOnlyList<string> Wear(PartyMember member, Inventory bag, string id)
    {
        EquipmentSlot slot = ItemCatalog.Get(id).SlotOn(member.Equipment) ?? throw new ArgumentException($"{id} isn't something you wear.", nameof(id));
        if (!bag.Remove(id)) throw new InvalidOperationException($"There's no {id} in the bag to put on.");

        List<string> off = member.Equip(id, slot);
        foreach (string each in off) bag.Add(each);
        return off;
    }

    /// <summary>Takes off whatever's in one of someone's slots, into the bag.</summary>
    /// <param name="member">Who.</param>
    /// <param name="bag">The party's bag.</param>
    /// <param name="slot">The slot.</param>
    /// <returns>What came off, or null if the slot was empty.</returns>
    public static string TakeOff(PartyMember member, Inventory bag, EquipmentSlot slot)
    {
        string id = member.Unequip(slot);
        if (id is not null) bag.Add(id);
        return id;
    }

    /// <summary>Someone drinks something from the bag: a healing potion heals them, never past their most, and an elixir gives back spent spell slots.</summary>
    /// <remarks>Check WhyNot first, so nothing's drunk to no effect.</remarks>
    /// <param name="member">Who drinks it.</param>
    /// <param name="bag">The party's bag. It has to have one.</param>
    /// <param name="item">What they drink: a consumable that isn't thrown.</param>
    /// <param name="random">Where the healing dice come from.</param>
    /// <returns>The hit points it healed, or the spell slots it gave back.</returns>
    public static int Drink(PartyMember member, Inventory bag, ItemDefinition item, Random random)
    {
        if (item.Kind != ItemKind.Consumable || !item.ThrownDamage.IsNone) throw new ArgumentException($"{item.Id} isn't something you drink.", nameof(item));
        if (!bag.Remove(item.Id)) throw new InvalidOperationException($"There's no {item.Id} in the bag to drink.");

        if (item.Heals.IsNone) return ResourceCatalog.Restore(member, item.SlotsRestored);

        int before = member.Hp;
        member.Hp = Math.Min(member.MaxHp, member.Hp + item.Heals.Roll(random, item.Name).Total);
        return member.Hp - before;
    }
}
