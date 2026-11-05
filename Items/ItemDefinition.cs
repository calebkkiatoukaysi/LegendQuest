using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Items;

/// <summary>
/// One kind of item: what it's called, what it looks like, what it's worth, and what it does.
/// </summary>
/// <remarks>
/// One shape for every kind, like The Black Box's ItemDefinition, with only the settings its Kind
/// uses set; ItemCatalog refuses the rest when the game starts. The weapon and armour numbers are
/// the SRD 5.1's. Distances are in metres, since that's what the combat grid counts in: a tile is
/// 1.5 m.
/// </remarks>
public sealed class ItemDefinition
{
    /// <summary>What the tables, the maps and the save call it.</summary>
    public required string Id { get; init; }

    /// <summary>What it's called on screen.</summary>
    public required string Name { get; init; }

    /// <summary>What sort of thing it is, which decides its Slot.</summary>
    public required ItemKind Kind { get; init; }

    /// <summary>One or two sentences about it, in the game's voice, for its tooltip.</summary>
    public required string Description { get; init; }

    /// <summary>What it's worth in gold. Shops sell it for this times their markup, and buy it for half.</summary>
    public int Value { get; init; }

    /// <summary>How rare it is. Anything magic is at least Uncommon.</summary>
    public Rarity Rarity { get; init; }

    /// <summary>Its picture: which 24 by 24 icon in Content/Items/icons.png, counting along the rows of 16 from the top left. Its place in ItemCatalog's table.</summary>
    public int Icon { get; init; }

    /// <summary>The paperdoll gear layer it draws on whoever's wearing it, or null if it draws nothing.</summary>
    public string Layer { get; init; }

    /// <summary>Whether several of it share one place in the bag. Consumables and materials do; anything you wear has a place each.</summary>
    public bool Stacks { get; init; }

    /// <summary>The ordinary item a magic one is a finer version of (the thornwood bow's is the shortbow), or null. Training with that one counts for this.</summary>
    public string Base { get; init; }

    /// <summary>A weapon: simple or martial.</summary>
    public WeaponCategory Category { get; init; }

    /// <summary>A weapon: the damage it does in one hand.</summary>
    public DiceExpression Damage { get; init; }

    /// <summary>A Versatile weapon: the damage it does in both hands. None otherwise.</summary>
    public DiceExpression VersatileDamage { get; init; }

    /// <summary>A weapon: the kind of damage it does.</summary>
    public DamageType DamageType { get; init; }

    /// <summary>A weapon: what's special about it.</summary>
    public WeaponProperty Properties { get; init; }

    /// <summary>A weapon: how far it reaches, in metres. 1.5 for melee (the next tile), 3 for a Reach weapon, and a Ranged one's normal range.</summary>
    public float Range { get; init; }

    /// <summary>A Thrown weapon or a thrown consumable: how far it goes at its normal range, in metres. 0 for anything that isn't thrown.</summary>
    public float ThrowRange { get; init; }

    /// <summary>A weapon: what it adds to the attack roll. A +1 weapon adds 1.</summary>
    public int AttackBonus { get; init; }

    /// <summary>What it adds to damage. A +1 weapon adds 1 to its own; anything else worn (the gauntlets of might) adds it to every weapon attack.</summary>
    public int DamageBonus { get; init; }

    /// <summary>A weapon: damage it does on top, of ExtraDamageType, like the ember dagger's 1d4 fire. None for most.</summary>
    public DiceExpression ExtraDamage { get; init; }

    /// <summary>A weapon: the kind of damage ExtraDamage does.</summary>
    public DamageType ExtraDamageType { get; init; }

    /// <summary>Body armour: how heavy it is, which decides how much Dexterity counts and who's trained in it.</summary>
    public ArmourCategory Armour { get; init; }

    /// <summary>Body armour: its AC before Dexterity. 0 for anything else.</summary>
    public int BaseAc { get; init; }

    /// <summary>Body armour: the most Dexterity can add, 2 for medium and 0 for heavy, or null for no limit.</summary>
    public int? MaxDex { get; init; }

    /// <summary>Heavy armour: the Strength score its wearer needs, or they move 3 m less.</summary>
    public int StrengthNeeded { get; init; }

    /// <summary>Armour: whether it gives disadvantage on sneaking.</summary>
    public bool StealthDisadvantage { get; init; }

    /// <summary>What wearing it adds to AC: a shield's 2, a cloak or ring of protection's 1.</summary>
    public int AcBonus { get; init; }

    /// <summary>Anything worn: metres it adds to its wearer's speed.</summary>
    public float SpeedBonus { get; init; }

    /// <summary>Anything worn: what it adds to its wearer's most hit points.</summary>
    public int MaxHpBonus { get; init; }

    /// <summary>Anything worn: what it adds to its wearer's spell attack rolls, like the circlet of focus's 1.</summary>
    public int SpellAttackBonus { get; init; }

    /// <summary>A consumable: the hit points drinking it gives back. None if it doesn't heal.</summary>
    public DiceExpression Heals { get; init; }

    /// <summary>A consumable: the damage it does where it lands when it's thrown, out to ThrowRange. None if it isn't thrown.</summary>
    public DiceExpression ThrownDamage { get; init; }

    /// <summary>A thrown consumable: the kind of damage ThrownDamage does.</summary>
    public DamageType ThrownDamageType { get; init; }

    /// <summary>A consumable: the Focus drinking it in a fight gives back, never past the most there is (an elixir's 30). 0 for most. Only a fight has Focus, so out of one it can't be drunk.</summary>
    public int FocusRestored { get; init; }

    /// <summary>Where it's worn, from its Kind: a weapon in the main hand, a shield in the off hand, armour on the body, and so on. Null for what isn't worn.</summary>
    /// <remarks>A light weapon can go in the off hand as well; Fits says where something can go, and SlotOn where it should.</remarks>
    public EquipmentSlot? Slot => Kind switch
    {
        ItemKind.Weapon => EquipmentSlot.MainHand,
        ItemKind.Shield => EquipmentSlot.OffHand,
        ItemKind.Armour => EquipmentSlot.Body,
        ItemKind.Head => EquipmentSlot.Head,
        ItemKind.Hands => EquipmentSlot.Hands,
        ItemKind.Feet => EquipmentSlot.Feet,
        ItemKind.Cloak => EquipmentSlot.Cloak,
        ItemKind.Amulet => EquipmentSlot.Amulet,
        ItemKind.Ring => EquipmentSlot.Ring,
        _ => null,
    };

    /// <summary>Whether it has a weapon property, or all of several.</summary>
    /// <param name="property">The property.</param>
    public bool Has(WeaponProperty property) => (Properties & property) == property;

    /// <summary>Whether it can go in a slot: its own, or the off hand for a light weapon.</summary>
    /// <param name="slot">The slot.</param>
    public bool Fits(EquipmentSlot slot) =>
        slot == Slot || slot == EquipmentSlot.OffHand && Kind == ItemKind.Weapon && Has(WeaponProperty.Light);

    /// <summary>Where it goes on someone already wearing these, which is where clicking it in the bag puts it.</summary>
    /// <remarks>
    /// Its own slot, except a light weapon goes in the off hand when the main hand's full and the off
    /// hand isn't, so a second dagger is a second dagger and not a swap. Not beside a two-handed
    /// weapon, though: then it takes the main hand.
    /// </remarks>
    /// <param name="worn">What they've on now, by slot, as item ids.</param>
    /// <returns>The slot, or null for something that isn't worn.</returns>
    public EquipmentSlot? SlotOn(IReadOnlyDictionary<EquipmentSlot, string> worn)
    {
        if (Slot != EquipmentSlot.MainHand || !Fits(EquipmentSlot.OffHand)) return Slot;
        if (!worn.TryGetValue(EquipmentSlot.MainHand, out string main) || worn.ContainsKey(EquipmentSlot.OffHand)) return EquipmentSlot.MainHand;
        return ItemCatalog.Get(main).Has(WeaponProperty.TwoHanded) ? EquipmentSlot.MainHand : EquipmentSlot.OffHand;
    }
}
