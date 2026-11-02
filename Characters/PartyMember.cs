using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Characters;

/// <summary>
/// Someone in the party: who they are, what they are, how hurt they are, what they're wearing, how
/// they look, and where they're standing.
/// </summary>
/// <remarks>
/// Plain data that goes in the save as it is. The sprite that walks them round a map is separate,
/// and gets made from this when the map loads. What they're wearing is item ids by slot, and every
/// worn bonus is worked out from it when it's asked for, so nothing can get out of step with it.
/// Approval and conditions get their fields when the systems that own them exist.
/// </remarks>
public class PartyMember
{
    /// <summary>Who they are, for the game rather than the screen: you, or one of the companions in Origins.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>What they're called on screen.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Their class.</summary>
    public ClassId Class { get; set; }

    /// <summary>Their level. Everyone's level 1 in the skeleton; levelling up comes later.</summary>
    public int Level { get; set; } = 1;

    /// <summary>The experience they've earned. Nothing spends it until there's levelling up.</summary>
    public int Xp { get; set; }

    /// <summary>Their six ability scores.</summary>
    public AbilityScores Scores { get; set; } = new();

    /// <summary>The hit points they have now.</summary>
    public int Hp { get; set; }

    /// <summary>What they're wearing and holding, by slot, as item ids in ItemCatalog. A slot that's missing is empty.</summary>
    /// <remarks>Change it with Equip, Unequip and PutOn, which keep the hands right and the hit points in range.</remarks>
    public Dictionary<EquipmentSlot, string> Equipment { get; set; } = new();

    /// <summary>How many uses of each limited thing they've spent since they last rested, by the resource's id: spell slots, Second Wind, the Lay on Hands pool. One that's missing has none spent.</summary>
    public Dictionary<string, int> Spent { get; set; } = new();

    /// <summary>How they look.</summary>
    public CharacterLook Look { get; set; } = new();

    /// <summary>Which pronouns the lines use for them. Only the Soul-Bound's are ever read.</summary>
    public Pronouns Pronouns { get; set; }

    /// <summary>How far across the map their feet are, in world pixels.</summary>
    public float X { get; set; }

    /// <summary>How far down the map their feet are, in world pixels.</summary>
    public float Y { get; set; }

    /// <summary>Which way they're facing.</summary>
    public Direction Facing { get; set; }

    /// <summary>The most hit points they can have.</summary>
    /// <remarks>5e at level 1: the class's whole hit die plus the Constitution modifier, never less than 1, plus whatever anything they're wearing adds.</remarks>
    [JsonIgnore]
    public int MaxHp
    {
        get
        {
            int most = Math.Max(1, ClassCatalog.Get(Class).HitDie + Scores.Modifier(Ability.Constitution));
            foreach (ItemDefinition item in Worn) most += item.MaxHpBonus;
            return most;
        }
    }

    /// <summary>Everything they're wearing and holding.</summary>
    [JsonIgnore]
    public IEnumerable<ItemDefinition> Worn
    {
        get
        {
            foreach (string id in Equipment.Values) yield return ItemCatalog.Get(id);
        }
    }

    /// <summary>X and Y together.</summary>
    [JsonIgnore]
    public Vector2 Feet
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    /// <summary>What's in one slot, or null if it's empty.</summary>
    /// <param name="slot">The slot.</param>
    public ItemDefinition WornIn(EquipmentSlot slot) => Equipment.TryGetValue(slot, out string id) ? ItemCatalog.Get(id) : null;

    /// <summary>Puts something on, taking off whatever's in the way.</summary>
    /// <remarks>
    /// A two-handed weapon takes the off hand with it, and anything for the off hand takes a
    /// two-handed weapon off. It doesn't check they're trained with it: 5e lets anyone pick anything
    /// up, and makes them pay for it in a fight. Hit points come down if the most they can have does.
    /// </remarks>
    /// <param name="id">The item. It has to exist and fit the slot.</param>
    /// <param name="slot">Where it goes. ItemDefinition.SlotOn says where it should.</param>
    /// <returns>What came off, to go back in the bag.</returns>
    public List<string> Equip(string id, EquipmentSlot slot)
    {
        ItemDefinition item = ItemCatalog.Get(id);
        if (!item.Fits(slot)) throw new ArgumentException($"{id} doesn't go in the {slot} slot.", nameof(slot));

        var off = new List<string>();
        TakeOff(slot, off);
        if (slot == EquipmentSlot.MainHand && item.Has(WeaponProperty.TwoHanded)) TakeOff(EquipmentSlot.OffHand, off);
        if (slot == EquipmentSlot.OffHand && WornIn(EquipmentSlot.MainHand)?.Has(WeaponProperty.TwoHanded) == true) TakeOff(EquipmentSlot.MainHand, off);

        Equipment[slot] = id;
        KeepHpInRange();
        return off;
    }

    /// <summary>Takes off whatever's in a slot.</summary>
    /// <param name="slot">The slot.</param>
    /// <returns>What came off, to go back in the bag, or null if the slot was empty.</returns>
    public string Unequip(EquipmentSlot slot)
    {
        if (!Equipment.Remove(slot, out string id)) return null;
        KeepHpInRange();
        return id;
    }

    /// <summary>Dresses them in a kit, in place of whatever they had on: their class's when they're made, or a companion's own.</summary>
    /// <remarks>Each item goes where SlotOn puts it, so a second light weapon goes in the off hand. What they had on is gone, not put in the bag.</remarks>
    /// <param name="kit">Item ids, every one something worn. ClassDefinition.KitProblem checks a kit.</param>
    public void PutOn(IReadOnlyList<string> kit)
    {
        Equipment.Clear();
        foreach (string id in kit)
        {
            EquipmentSlot? slot = ItemCatalog.Get(id).SlotOn(Equipment);
            Equip(id, slot ?? throw new ArgumentException($"{id} isn't something you wear.", nameof(kit)));
        }
    }

    /// <summary>Takes whatever's in a slot off, onto a list of what's come off.</summary>
    /// <param name="slot">The slot.</param>
    /// <param name="off">The list.</param>
    private void TakeOff(EquipmentSlot slot, List<string> off)
    {
        if (Equipment.Remove(slot, out string id)) off.Add(id);
    }

    /// <summary>Brings their hit points down to the most they can have, if taking something off lowered it.</summary>
    private void KeepHpInRange() => Hp = Math.Min(Hp, MaxHp);
}
