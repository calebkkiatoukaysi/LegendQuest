using System;
using System.Collections.Generic;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Every ability, by AbilityId, and which of them each class has at level 1.
/// </summary>
/// <remarks>
/// The SRD 5.1's numbers, at level 1, with the ranges in metres (five feet is 1.5 m) and the long
/// ones cut to 18 m so a fight fits on a map. Fire Bolt and Eldritch Blast are 120 feet in the book,
/// which is most of a map here. The Warden's level-1 pick is the Protection fighting style, since a
/// level-1 paladin has no spells yet and the story's Warden is the party's shield. What each ability
/// spends is in ResourceCatalog.
/// </remarks>
public static class AbilityCatalog
{
    /// <summary>A long-range spell's reach: 60 feet in the book.</summary>
    private const float Far = 18f;

    /// <summary>Touch: the tiles beside you.</summary>
    private const float Touch = Measure.MeleeReach;

    private static readonly AbilityDefinition[] Table =
    {
        // Anyone.
        new()
        {
            Id = AbilityId.Attack, Name = "Attack", Kind = AbilityKind.Weapon, Target = TargetKind.Enemy, NeedsSight = true,
            Description = "Attack with the weapon in your main hand.",
        },
        new()
        {
            Id = AbilityId.OffHandAttack, Name = "Off-hand Attack", Kind = AbilityKind.OffHand, Cost = ActionCost.BonusAction,
            Target = TargetKind.Enemy, NeedsSight = true,
            Description = "After attacking with a light weapon, strike with the light weapon in your other hand. No modifier on its damage.",
        },
        new()
        {
            Id = AbilityId.Dash, Name = "Dash", Kind = AbilityKind.Dash,
            Description = "Spend your action on as much movement again.",
        },
        new()
        {
            Id = AbilityId.Disengage, Name = "Disengage", Kind = AbilityKind.Condition, Conditions = new[] { ConditionId.Disengaged }, Turns = 1,
            Description = "Your movement doesn't provoke opportunity attacks for the rest of the turn.",
        },
        new()
        {
            Id = AbilityId.Dodge, Name = "Dodge", Kind = AbilityKind.Condition, Conditions = new[] { ConditionId.Dodging },
            Description = "Until your next turn, attacks against you have disadvantage and you have advantage on Dexterity saves.",
        },
        new()
        {
            Id = AbilityId.Help, Name = "Help", Kind = AbilityKind.Help, Target = TargetKind.Ally, Range = Touch, NotSelf = true,
            Conditions = new[] { ConditionId.Helped }, Turns = 1,
            Description = "Get a Downed ally beside you back up with 1 HP, or give one advantage on their next attack.",
        },
        new()
        {
            Id = AbilityId.DrinkPotion, Name = "Drink Potion", Kind = AbilityKind.Potion, Cost = ActionCost.BonusAction,
            Description = "Drink a healing potion from the party's bag.",
        },
        new()
        {
            Id = AbilityId.DrinkElixir, Name = "Drink Elixir", Kind = AbilityKind.Potion, Cost = ActionCost.BonusAction, Item = "elixir-of-focus",
            Description = "Drink an elixir of focus from the party's bag and get a spent spell slot back.",
        },
        new()
        {
            Id = AbilityId.ThrowAlchemistsFire, Name = "Alchemist's Fire", Kind = AbilityKind.Thrown, Target = TargetKind.Enemy,
            NeedsSight = true, Item = "alchemists-fire",
            Description = "Throw a flask of alchemist's fire from the party's bag. It bursts into flame on a hit.",
        },
        new()
        {
            Id = AbilityId.SoulLinkStrike, Name = "Soul-Link Strike", Kind = AbilityKind.SoulLink, Target = TargetKind.Enemy, NeedsSight = true,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Radiant,
            Description = "When the bond is full, you and the Soul-Bound strike as one: both weapons, a sure hit, and 1d8 radiant.",
        },

        // Fighter.
        new()
        {
            Id = AbilityId.SecondWind, Name = "Second Wind", Kind = AbilityKind.Heal, Cost = ActionCost.BonusAction,
            Heals = DiceExpression.Parse("1d10"), HealsAddLevel = true, Resource = ResourceCatalog.SecondWind,
            Description = "Catch your breath and heal 1d10 + your level. Once a short rest.",
        },
        new()
        {
            Id = AbilityId.Defence, Name = "Defence", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Description = "Fighting style: +1 AC while you're wearing armour.",
        },

        // Rogue.
        new()
        {
            Id = AbilityId.SneakAttack, Name = "Sneak Attack", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Damage = DiceExpression.Parse("1d6"),
            Description = "Once a turn, 1d6 more damage with a finesse or ranged weapon, when you have advantage or an ally is beside your target.",
        },
        new()
        {
            Id = AbilityId.CunningDash, Name = "Cunning Dash", Kind = AbilityKind.Dash, Cost = ActionCost.BonusAction,
            Description = "Cunning Action: Dash as a bonus action.",
        },
        new()
        {
            Id = AbilityId.CunningDisengage, Name = "Cunning Disengage", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Conditions = new[] { ConditionId.Disengaged }, Turns = 1,
            Description = "Cunning Action: Disengage as a bonus action.",
        },

        // Cleric.
        new()
        {
            Id = AbilityId.SacredFlame, Name = "Sacred Flame", Kind = AbilityKind.Save, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Radiant, Save = Ability.Dexterity, Spell = true,
            Description = "Radiant flame falls on a foe you can see: 1d8 radiant unless they make a Dexterity save. A cantrip.",
        },
        new()
        {
            Id = AbilityId.HealingWord, Name = "Healing Word", Kind = AbilityKind.Heal, Cost = ActionCost.BonusAction,
            Target = TargetKind.Ally, Range = Far, NeedsSight = true, Heals = DiceExpression.Parse("1d4"), HealsAddCasting = true,
            Spell = true, SpellSlotLevel = 1, Resource = ResourceCatalog.SpellSlot,
            Description = "A word of power heals an ally you can see 1d4 + your casting modifier, and gets them up if they're Downed.",
        },
        new()
        {
            Id = AbilityId.CureWounds, Name = "Cure Wounds", Kind = AbilityKind.Heal, Target = TargetKind.Ally, Range = Touch,
            Heals = DiceExpression.Parse("1d8"), HealsAddCasting = true, Spell = true, SpellSlotLevel = 1, Resource = ResourceCatalog.SpellSlot,
            Description = "A touch heals 1d8 + your casting modifier.",
        },

        // Wizard.
        new()
        {
            Id = AbilityId.FireBolt, Name = "Fire Bolt", Kind = AbilityKind.SpellAttack, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d10"), DamageType = DamageType.Fire, Spell = true,
            Description = "Hurl a mote of fire: a ranged spell attack for 1d10 fire. A cantrip.",
        },
        new()
        {
            Id = AbilityId.RayOfFrost, Name = "Ray of Frost", Kind = AbilityKind.SpellAttack, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Cold, Conditions = new[] { ConditionId.Slowed }, Turns = 1, Spell = true,
            Description = "A frigid beam: 1d8 cold, and the target moves 3 m less on its next turn. A cantrip.",
        },
        new()
        {
            Id = AbilityId.MagicMissile, Name = "Magic Missile", Kind = AbilityKind.AutoHit, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d4+1"), DamageType = DamageType.Force, Darts = 3,
            Spell = true, SpellSlotLevel = 1, Resource = ResourceCatalog.SpellSlot,
            Description = "Three glowing darts that never miss, 1d4 + 1 force each. Any left when the target falls fly on to the next foe in sight.",
        },

        // Paladin.
        new()
        {
            Id = AbilityId.LayOnHands, Name = "Lay on Hands", Kind = AbilityKind.Heal, Target = TargetKind.Ally, Range = Touch,
            Resource = ResourceCatalog.LayOnHands,
            Description = "Heal an ally you touch from your pool of healing, as much as they need and the pool has.",
        },
        new()
        {
            Id = AbilityId.Protection, Name = "Protection", Kind = AbilityKind.Passive, Cost = ActionCost.Reaction,
            Description = "Fighting style: with a shield up, your reaction turns an attack on an ally beside you to disadvantage.",
        },

        // Druid.
        new()
        {
            Id = AbilityId.ProduceFlame, Name = "Produce Flame", Kind = AbilityKind.SpellAttack, Target = TargetKind.Enemy, Range = 9f, NeedsSight = true,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Fire, Spell = true,
            Description = "A flame flickers into your palm and you throw it: a ranged spell attack for 1d8 fire. A cantrip.",
        },
        new()
        {
            Id = AbilityId.Entangle, Name = "Entangle", Kind = AbilityKind.Save, Target = TargetKind.Area, Range = Far, Radius = Touch, NeedsSight = true,
            Save = Ability.Strength, Conditions = new[] { ConditionId.Restrained }, Concentration = true,
            Spell = true, SpellSlotLevel = 1, Resource = ResourceCatalog.SpellSlot,
            Description = "Weeds and vines burst from the ground. Everyone in the patch, friend or foe, is Restrained unless they make a Strength save.",
        },

        // Warlock.
        new()
        {
            Id = AbilityId.EldritchBlast, Name = "Eldritch Blast", Kind = AbilityKind.SpellAttack, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d10"), DamageType = DamageType.Force, Spell = true,
            Description = "A beam of crackling energy: a ranged spell attack for 1d10 force. A cantrip.",
        },
        new()
        {
            Id = AbilityId.Hex, Name = "Hex", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction, Target = TargetKind.Enemy, Range = Far,
            NeedsSight = true, Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Necrotic, Conditions = new[] { ConditionId.Hexed },
            Concentration = true, Spell = true, SpellSlotLevel = 1, Resource = ResourceCatalog.PactSlot,
            Description = "Curse a foe: every hit you land on them does 1d6 necrotic more. When they fall, Hex can be moved to another for free.",
        },

        // Bard.
        new()
        {
            Id = AbilityId.ViciousMockery, Name = "Vicious Mockery", Kind = AbilityKind.Save, Target = TargetKind.Enemy, Range = Far, NeedsSight = true,
            Damage = DiceExpression.Parse("1d4"), DamageType = DamageType.Psychic, Save = Ability.Wisdom,
            Conditions = new[] { ConditionId.Mocked }, Turns = 1, Spell = true,
            Description = "An insult laced with magic: 1d4 psychic and disadvantage on their next attack, unless they make a Wisdom save. A cantrip.",
        },
        new()
        {
            Id = AbilityId.BardicInspiration, Name = "Bardic Inspiration", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Target = TargetKind.Ally, Range = Far, NeedsSight = true, NotSelf = true, Damage = DiceExpression.Parse("1d6"),
            Conditions = new[] { ConditionId.Inspired }, Resource = ResourceCatalog.BardicInspiration,
            Description = "Give an ally a d6. It's added to the first attack or save that would fail by little enough for it to matter.",
        },

        // Foes.
        new()
        {
            Id = AbilityId.Multiattack, Name = "Multiattack", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Description = "Its Attack action makes every one of its attacks.",
        },
        new()
        {
            Id = AbilityId.PackTactics, Name = "Pack Tactics", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Description = "Advantage on an attack when one of its own is beside the target.",
        },
        new()
        {
            Id = AbilityId.Trip, Name = "Trip", Kind = AbilityKind.Passive, Cost = ActionCost.Free, Save = Ability.Strength, Dc = 11,
            Conditions = new[] { ConditionId.Prone },
            Description = "A melee hit knocks the target Prone unless they make a DC 11 Strength save.",
        },
        new()
        {
            Id = AbilityId.SmokeBomb, Name = "Smoke Bomb", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Conditions = new[] { ConditionId.Dodging, ConditionId.Disengaged }, Turns = 1, RechargeOn = 5,
            Description = "A cloud of smoke: attacks against them have disadvantage and they slip away without provoking. Recharges on a 5 or 6.",
        },
        new()
        {
            Id = AbilityId.BlastingCharge, Name = "Blasting Charge", Kind = AbilityKind.Save, Target = TargetKind.Area, Range = 9f, Radius = Touch,
            NeedsSight = true, Damage = DiceExpression.Parse("2d6"), DamageType = DamageType.Fire, Save = Ability.Dexterity, Dc = 12,
            SaveHalves = true, RechargeOn = 6,
            Description = "A lit charge of blasting powder: 2d6 fire to everyone near where it lands, half on a DC 12 Dexterity save. Recharges on a 6.",
        },
        new()
        {
            Id = AbilityId.ThornBurst, Name = "Thorn Burst", Kind = AbilityKind.Save, Radius = Touch, Damage = DiceExpression.Parse("2d6"),
            DamageType = DamageType.Piercing, Save = Ability.Dexterity, Dc = 12, SaveHalves = true, RechargeOn = 5,
            Description = "Thorns burst from her hide: 2d6 piercing to everyone beside her, half on a DC 12 Dexterity save. Recharges on a 5 or 6.",
        },
    };

    /// <summary>The actions every party member has, in hotbar order, before their class's kit.</summary>
    private static readonly AbilityId[] PartyCommon =
    {
        AbilityId.Attack, AbilityId.OffHandAttack, AbilityId.Dash, AbilityId.Disengage, AbilityId.Dodge, AbilityId.Help,
        AbilityId.DrinkPotion, AbilityId.DrinkElixir, AbilityId.ThrowAlchemistsFire, AbilityId.SoulLinkStrike,
    };

    /// <summary>The actions every foe has, before the ones its stat block lists.</summary>
    private static readonly AbilityId[] FoeCommon = { AbilityId.Attack, AbilityId.Dash, AbilityId.Disengage, AbilityId.Dodge };

    /// <summary>Each class's level-1 kit, in hotbar order.</summary>
    private static readonly Dictionary<ClassId, AbilityId[]> Kits = new()
    {
        [ClassId.Fighter] = new[] { AbilityId.SecondWind, AbilityId.Defence },
        [ClassId.Rogue] = new[] { AbilityId.SneakAttack, AbilityId.CunningDash, AbilityId.CunningDisengage },
        [ClassId.Cleric] = new[] { AbilityId.SacredFlame, AbilityId.HealingWord, AbilityId.CureWounds },
        [ClassId.Wizard] = new[] { AbilityId.FireBolt, AbilityId.RayOfFrost, AbilityId.MagicMissile },
        [ClassId.Paladin] = new[] { AbilityId.LayOnHands, AbilityId.Protection },
        [ClassId.Druid] = new[] { AbilityId.ProduceFlame, AbilityId.HealingWord, AbilityId.Entangle },
        [ClassId.Warlock] = new[] { AbilityId.EldritchBlast, AbilityId.Hex },
        [ClassId.Bard] = new[] { AbilityId.ViciousMockery, AbilityId.HealingWord, AbilityId.BardicInspiration },
    };

    private static readonly Dictionary<AbilityId, AbilityDefinition> ById = new();

    /// <summary>Indexes the table and checks every row and every kit.</summary>
    /// <remarks>Throws on purpose, so an ability added to the enum and not here, or a row that can't work, shows up when the game starts.</remarks>
    static AbilityCatalog()
    {
        foreach (AbilityDefinition row in Table)
        {
            Check(row);
            if (!ById.TryAdd(row.Id, row)) throw new InvalidOperationException($"Ability {row.Id} is in the table twice.");
        }

        foreach (AbilityId id in Enum.GetValues<AbilityId>())
            if (!ById.ContainsKey(id)) throw new InvalidOperationException($"Ability {id} isn't in AbilityCatalog.");

        foreach (ClassId id in Enum.GetValues<ClassId>())
        {
            if (!Kits.TryGetValue(id, out AbilityId[] kit)) throw new InvalidOperationException($"Class {id} has no kit in AbilityCatalog.");
            foreach (AbilityId ability in kit)
                if (Array.IndexOf(PartyCommon, ability) >= 0) throw new InvalidOperationException($"Class {id}'s kit has {ability}, which everyone has already.");
        }
    }

    /// <summary>One ability.</summary>
    /// <param name="id">Which.</param>
    public static AbilityDefinition Get(AbilityId id) => ById[id];

    /// <summary>A class's level-1 kit, in hotbar order.</summary>
    /// <param name="id">Which class.</param>
    public static IReadOnlyList<AbilityId> Kit(ClassId id) => Kits[id];

    /// <summary>The actions every party member has, in hotbar order.</summary>
    public static IReadOnlyList<AbilityId> CommonToParty => PartyCommon;

    /// <summary>The actions every foe has, before its own.</summary>
    public static IReadOnlyList<AbilityId> CommonToFoes => FoeCommon;

    /// <summary>Whether an ability is one everybody has, so a stat block has no need to list it.</summary>
    /// <param name="id">Which.</param>
    public static bool IsCommon(AbilityId id) => Array.IndexOf(PartyCommon, id) >= 0 || Array.IndexOf(FoeCommon, id) >= 0;

    /// <summary>Checks one row makes sense for its kind, and throws with what's wrong if it doesn't.</summary>
    private static void Check(AbilityDefinition row)
    {
        string name = $"Ability {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Description))
            throw new InvalidOperationException($"{name} needs a name and a description.");
        if (row.Range < 0f || row.Radius < 0f || row.Turns < 0 || row.Spends < 1)
            throw new InvalidOperationException($"{name} has a negative range, radius or duration, or spends less than one.");
        if (row.Target == TargetKind.Area && (row.Radius <= 0f || row.Range <= 0f))
            throw new InvalidOperationException($"{name} is an area with no radius or no range.");
        bool pointed = row.Target is TargetKind.Ally or TargetKind.Enemy or TargetKind.Point;
        bool weaponReach = row.Kind is AbilityKind.Weapon or AbilityKind.OffHand or AbilityKind.SoulLink or AbilityKind.Thrown;
        if (pointed && !weaponReach && row.Range <= 0f)
            throw new InvalidOperationException($"{name} is pointed at someone but has no range.");
        if (row.Resource is not null && !ResourceCatalog.Exists(row.Resource))
            throw new InvalidOperationException($"{name} spends {row.Resource}, which isn't in ResourceCatalog.");
        if (row.Item is not null && !ItemCatalog.Exists(row.Item))
            throw new InvalidOperationException($"{name} uses up {row.Item}, which isn't in ItemCatalog.");
        if (row.SpellSlotLevel > 0 && (!row.Spell || row.Resource is not (ResourceCatalog.SpellSlot or ResourceCatalog.PactSlot)))
            throw new InvalidOperationException($"{name} takes a level-{row.SpellSlotLevel} slot, so it has to be a spell that spends one.");
        if (row.Concentration && !row.Spell) throw new InvalidOperationException($"{name} needs concentration but isn't a spell.");
        if (row.Save is null && (row.Kind == AbilityKind.Save || row.Dc > 0 || row.SaveHalves))
            throw new InvalidOperationException($"{name} has a DC or a save's damage but no ability to save with.");

        bool damages = !row.Damage.IsNone;
        bool conditions = row.Conditions.Count > 0;
        bool broken = row.Kind switch
        {
            AbilityKind.SpellAttack => !damages || !row.Spell,
            AbilityKind.Save => !damages && !conditions,
            AbilityKind.AutoHit => !damages || row.Darts < 1,
            AbilityKind.Heal => row.Heals.IsNone && row.Resource is null,
            AbilityKind.Thrown => row.Item is null || ItemCatalog.Get(row.Item).ThrownDamage.IsNone || ItemCatalog.Get(row.Item).ThrowRange <= 0f,
            AbilityKind.Condition or AbilityKind.Help => !conditions,
            AbilityKind.SoulLink => !damages,
            _ => false,
        };
        if (broken) throw new InvalidOperationException($"{name} is missing what a {row.Kind} ability needs (damage, conditions, darts, healing, an item, or to be a spell).");
    }
}
