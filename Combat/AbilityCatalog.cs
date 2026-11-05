using System;
using System.Collections.Generic;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Every ability, by AbilityId, and the interim kits the rows are tuned on: each class's, the actions
/// the party shares, and what every stat block has.
/// </summary>
/// <remarks>
/// The SRD 5.1's dice, with 11-combat.md's rows on top: each row's Shape, Reach, Focus, Cooldown and
/// Bar say who it's for, how far it reaches, what it costs and where it sits on the bar. The kits are
/// m4-plan's §2 table, until SkillCatalog brings 10-classes.md's. The Warden's pick is the Protection
/// fighting style, since the story's Warden is the party's shield. Dash, Disengage, Cunning Dash and
/// the off-hand attack are in no kit any more, but keep their places, since AbilityId's order is the
/// icons'.
/// </remarks>
public static class AbilityCatalog
{
    /// <summary>The Focus an auto attack gives back when it's used.</summary>
    private const int AutoFocus = 10;

    private static readonly AbilityDefinition[] Table =
    {
        // Anyone.
        new()
        {
            Id = AbilityId.Attack, Name = "Attack", Kind = AbilityKind.Weapon,
            Shape = TargetShape.Foe, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Description = "Attack with the weapon in your main hand.",
        },
        new()
        {
            Id = AbilityId.OffHandAttack, Name = "Off-hand Attack", Kind = AbilityKind.OffHand, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Foe, Bar = BarSlot.Hidden,
            Description = "After attacking with a light weapon, strike with the light weapon in your other hand. No modifier on its damage.",
        },
        new()
        {
            Id = AbilityId.Dash, Name = "Dash", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Hidden,
            Description = "In no kit: a fight in rows has nowhere to dash to.",
        },
        new()
        {
            Id = AbilityId.Disengage, Name = "Disengage", Kind = AbilityKind.Condition, Conditions = new[] { ConditionId.Disengaged }, Turns = 1,
            Shape = TargetShape.Self, Bar = BarSlot.Hidden,
            Description = "Leaving the front row doesn't give anyone an opportunity attack for the rest of the turn.",
        },
        new()
        {
            Id = AbilityId.Dodge, Name = "Dodge", Kind = AbilityKind.Condition, Conditions = new[] { ConditionId.Dodging },
            Shape = TargetShape.Self, Bar = BarSlot.Shared,
            Description = "Until your next turn, attacks against you have disadvantage and you have advantage on Dexterity saves.",
        },
        new()
        {
            Id = AbilityId.Help, Name = "Help", Kind = AbilityKind.Help, NotSelf = true,
            Shape = TargetShape.Ally, Bar = BarSlot.Shared,
            Conditions = new[] { ConditionId.Helped }, Turns = 1,
            Description = "Get a Downed ally back up with 1 HP, or give one advantage on their next attack.",
        },
        new()
        {
            Id = AbilityId.DrinkPotion, Name = "Drink Potion", Kind = AbilityKind.Potion, Cost = ActionCost.BonusAction,
            Shape = TargetShape.AllyNextTo, Bar = BarSlot.Potion,
            Description = "Drink a healing potion from the party's bag, or hand one to an ally next to you.",
        },
        new()
        {
            Id = AbilityId.DrinkElixir, Name = "Drink Elixir", Kind = AbilityKind.Potion, Cost = ActionCost.BonusAction, Item = "elixir-of-focus",
            Shape = TargetShape.Self, Bar = BarSlot.Bag,
            Description = "Drink an elixir of focus from the party's bag and get Focus back.",
        },
        new()
        {
            Id = AbilityId.ThrowAlchemistsFire, Name = "Alchemist's Fire", Kind = AbilityKind.Thrown, Item = "alchemists-fire",
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Bag,
            Description = "Throw a flask of alchemist's fire from the party's bag at any foe. It bursts into flame on a hit.",
        },
        new()
        {
            Id = AbilityId.SoulLinkStrike, Name = "Soul-Link Strike", Kind = AbilityKind.SoulLink,
            Shape = TargetShape.Foe, Bar = BarSlot.Hidden,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Radiant,
            Description = "When the bond is full, you and the Soul-Bound strike as one: both weapons, a sure hit, and 1d8 radiant. You both have to be able to reach it.",
        },

        // Fighter.
        new()
        {
            Id = AbilityId.SecondWind, Name = "Second Wind", Kind = AbilityKind.Heal, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Self, Bar = BarSlot.Skill, Focus = 20, Cooldown = 4,
            Heals = DiceExpression.Parse("1d10"), HealsAddLevel = true,
            Description = "Catch your breath and heal 1d10 + your level.",
        },
        new()
        {
            Id = AbilityId.Defence, Name = "Defence", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Description = "Fighting style: +1 AC while you're wearing armour.",
        },

        // Rogue.
        new()
        {
            Id = AbilityId.SneakAttack, Name = "Sneak Attack", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Damage = DiceExpression.Parse("1d6"),
            Description = "Once a turn, 1d6 more damage with a finesse or ranged weapon, with advantage, or on a target an ally's hit this round, or one that's Marked or Prone.",
        },
        new()
        {
            Id = AbilityId.CunningDash, Name = "Cunning Dash", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Hidden,
            Description = "In no kit: a fight in rows has nowhere to dash to.",
        },
        new()
        {
            Id = AbilityId.CunningDisengage, Name = "Cunning Step", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Self, Bar = BarSlot.Skill, Focus = 20, Cooldown = 2, FreeShift = true,
            Conditions = new[] { ConditionId.Disengaged, ConditionId.Dodging }, Turns = 1,
            Description = "Cunning Action: a free Shift that nobody gets a swing at, and attacks on you have disadvantage until your next turn.",
        },
        new()
        {
            Id = AbilityId.Backstab, Name = "Backstab", Kind = AbilityKind.Weapon,
            Shape = TargetShape.Foe, Reach = Reach.AnyRow, Bar = BarSlot.Skill, Focus = 20, Cooldown = 0,
            Description = "Slip round the line and strike any foe with your weapon, front row or back.",
        },

        // Cleric.
        new()
        {
            Id = AbilityId.SacredFlame, Name = "Sacred Flame", Kind = AbilityKind.Save,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Radiant, Save = Ability.Dexterity, Spell = true,
            Description = "Radiant flame falls on any foe: 1d8 radiant unless they make a Dexterity save.",
        },
        new()
        {
            Id = AbilityId.HealingWord, Name = "Healing Word", Kind = AbilityKind.Heal, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Ally, Bar = BarSlot.Skill, Focus = 20, Cooldown = 1,
            Heals = DiceExpression.Parse("1d4"), HealsAddCasting = true, Spell = true,
            Description = "A word of power heals an ally 1d4 + your casting modifier, and gets them up if they're Downed.",
        },
        new()
        {
            Id = AbilityId.CureWounds, Name = "Cure Wounds", Kind = AbilityKind.Heal,
            Shape = TargetShape.Ally, Bar = BarSlot.Skill, Focus = 25, Cooldown = 1,
            Heals = DiceExpression.Parse("1d8"), HealsAddCasting = true, Spell = true,
            Description = "Heal an ally 1d8 + your casting modifier.",
        },

        // Wizard.
        new()
        {
            Id = AbilityId.FireBolt, Name = "Fire Bolt", Kind = AbilityKind.SpellAttack,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Damage = DiceExpression.Parse("1d10"), DamageType = DamageType.Fire, Spell = true,
            Description = "Hurl a mote of fire: a ranged spell attack for 1d10 fire.",
        },
        new()
        {
            Id = AbilityId.RayOfFrost, Name = "Ray of Frost", Kind = AbilityKind.SpellAttack,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Skill, Focus = 15,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Cold, Conditions = new[] { ConditionId.Slowed }, Turns = 1, Spell = true,
            Description = "A frigid beam: 1d8 cold, and the target can't Shift on its next turn.",
        },
        new()
        {
            Id = AbilityId.MagicMissile, Name = "Magic Missile", Kind = AbilityKind.AutoHit,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Skill, Focus = 25, Cooldown = 1, Count = 3,
            Damage = DiceExpression.Parse("1d4+1"), DamageType = DamageType.Force, Spell = true,
            Description = "Three glowing darts that never miss, 1d4 + 1 force each. Any left when the target falls fly on to the next foe.",
        },

        // Paladin.
        new()
        {
            Id = AbilityId.LayOnHands, Name = "Lay on Hands", Kind = AbilityKind.Heal,
            Shape = TargetShape.Ally, Bar = BarSlot.Skill, Focus = 20, PoolPerLevel = 5,
            Description = "Heal an ally from your pool of healing, 5 hit points a level each fight: as much as they need and the pool has.",
        },
        new()
        {
            Id = AbilityId.Protection, Name = "Protection", Kind = AbilityKind.Passive, Cost = ActionCost.Reaction,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Description = "Fighting style: with a shield up, your reaction turns an attack on an ally next to you to disadvantage.",
        },

        // Druid.
        new()
        {
            Id = AbilityId.ProduceFlame, Name = "Produce Flame", Kind = AbilityKind.SpellAttack,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Damage = DiceExpression.Parse("1d8"), DamageType = DamageType.Fire, Spell = true,
            Description = "A flame flickers into your palm and you throw it: a ranged spell attack for 1d8 fire.",
        },
        new()
        {
            Id = AbilityId.Entangle, Name = "Entangle", Kind = AbilityKind.Save,
            Shape = TargetShape.FoeRow, Reach = Reach.Ranged, Bar = BarSlot.Skill, Focus = 30, Cooldown = 3, Turns = 2,
            Save = Ability.Strength, Conditions = new[] { ConditionId.Restrained }, Concentration = true, Spell = true,
            Description = "Weeds and vines burst up under one of their rows: everyone in it is Restrained for 2 turns unless they make a Strength save.",
        },

        // Warlock.
        new()
        {
            Id = AbilityId.EldritchBlast, Name = "Eldritch Blast", Kind = AbilityKind.SpellAttack,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Damage = DiceExpression.Parse("1d10"), DamageType = DamageType.Force, Spell = true,
            Description = "A beam of crackling energy: a ranged spell attack for 1d10 force.",
        },
        new()
        {
            Id = AbilityId.Hex, Name = "Hex", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Skill, Focus = 20,
            Damage = DiceExpression.Parse("1d6"), DamageType = DamageType.Necrotic, Conditions = new[] { ConditionId.Hexed },
            Concentration = true, Spell = true,
            Description = "Curse a foe: every hit you land on them does 1d6 necrotic more. When they fall, Hex can be moved to another for free.",
        },

        // Bard.
        new()
        {
            Id = AbilityId.ViciousMockery, Name = "Vicious Mockery", Kind = AbilityKind.Save,
            Shape = TargetShape.Foe, Reach = Reach.Ranged, Bar = BarSlot.Auto, FocusGain = AutoFocus,
            Damage = DiceExpression.Parse("1d4"), DamageType = DamageType.Psychic, Save = Ability.Wisdom,
            Conditions = new[] { ConditionId.Mocked }, Turns = 1, Spell = true,
            Description = "An insult laced with magic: 1d4 psychic and disadvantage on their next attack, unless they make a Wisdom save.",
        },
        new()
        {
            Id = AbilityId.BardicInspiration, Name = "Bardic Inspiration", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Ally, Bar = BarSlot.Skill, Focus = 20, Cooldown = 1,
            NotSelf = true, Damage = DiceExpression.Parse("1d6"), Conditions = new[] { ConditionId.Inspired },
            Description = "Give an ally a d6. It's added to the first attack or save that would fail by little enough for it to matter.",
        },

        // Foes.
        new()
        {
            Id = AbilityId.Multiattack, Name = "Multiattack", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Description = "Its Attack action makes every one of its attacks.",
        },
        new()
        {
            Id = AbilityId.PackTactics, Name = "Pack Tactics", Kind = AbilityKind.Passive, Cost = ActionCost.Free,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Description = "Advantage on an attack on someone in the front when another of its pack is standing in its own front row.",
        },
        new()
        {
            Id = AbilityId.Trip, Name = "Trip", Kind = AbilityKind.Passive, Cost = ActionCost.Free, Save = Ability.Strength, Dc = 11,
            Shape = TargetShape.Self, Bar = BarSlot.Passive,
            Conditions = new[] { ConditionId.Prone },
            Description = "A melee hit knocks the target Prone unless they make a DC 11 Strength save.",
        },
        new()
        {
            Id = AbilityId.SmokeBomb, Name = "Smoke Bomb", Kind = AbilityKind.Condition, Cost = ActionCost.BonusAction,
            Shape = TargetShape.Self, Bar = BarSlot.Skill,
            Conditions = new[] { ConditionId.Dodging, ConditionId.Disengaged }, Turns = 1, RechargeOn = 5,
            Description = "A cloud of smoke: attacks against them have disadvantage and they slip away without provoking. Recharges on a 5 or 6.",
        },
        new()
        {
            Id = AbilityId.BlastingCharge, Name = "Blasting Charge", Kind = AbilityKind.Save,
            Shape = TargetShape.FoeAndNeighbours, Reach = Reach.Ranged, Bar = BarSlot.Skill,
            Damage = DiceExpression.Parse("2d6+2"), DamageType = DamageType.Fire, Save = Ability.Dexterity, Dc = 12, SaveHalves = true, RechargeOn = 6,
            Description = "A lit charge of blasting powder: 2d6 + 2 fire to a foe and those beside it, half on a DC 12 Dexterity save. Recharges on a 6.",
        },
        new()
        {
            Id = AbilityId.ThornBurst, Name = "Thorn Burst", Kind = AbilityKind.Save,
            Shape = TargetShape.FoeRow, Reach = Reach.Melee, Bar = BarSlot.Skill,
            Damage = DiceExpression.Parse("2d6"), DamageType = DamageType.Piercing, Save = Ability.Dexterity, Dc = 12, SaveHalves = true, RechargeOn = 5,
            Description = "Thorns burst from her hide: 2d6 piercing to everyone in the row in front of her, half on a DC 12 Dexterity save. Recharges on a 5 or 6.",
        },
    };

    /// <summary>What everyone in the party has after their class's own, in bar order: the potion, the strip, and the Soul-Link (for the duo).</summary>
    private static readonly AbilityId[] PartyShared =
    {
        AbilityId.DrinkPotion, AbilityId.Dodge, AbilityId.Help, AbilityId.ThrowAlchemistsFire, AbilityId.DrinkElixir, AbilityId.SoulLinkStrike,
    };

    /// <summary>What every stat block has before the abilities it lists.</summary>
    private static readonly AbilityId[] FoeShared = { AbilityId.Attack, AbilityId.Dodge };

    /// <summary>Each class's interim kit, in bar order: its auto attack, then its skills and what's always on.</summary>
    private static readonly Dictionary<ClassId, AbilityId[]> Kits = new()
    {
        [ClassId.Fighter] = new[] { AbilityId.Attack, AbilityId.SecondWind, AbilityId.Defence },
        [ClassId.Rogue] = new[] { AbilityId.Attack, AbilityId.Backstab, AbilityId.CunningDisengage, AbilityId.SneakAttack },
        [ClassId.Cleric] = new[] { AbilityId.SacredFlame, AbilityId.CureWounds, AbilityId.HealingWord },
        [ClassId.Wizard] = new[] { AbilityId.FireBolt, AbilityId.MagicMissile, AbilityId.RayOfFrost },
        [ClassId.Paladin] = new[] { AbilityId.Attack, AbilityId.LayOnHands, AbilityId.Protection },
        [ClassId.Druid] = new[] { AbilityId.ProduceFlame, AbilityId.Entangle, AbilityId.HealingWord },
        [ClassId.Warlock] = new[] { AbilityId.EldritchBlast, AbilityId.Hex },
        [ClassId.Bard] = new[] { AbilityId.ViciousMockery, AbilityId.BardicInspiration, AbilityId.HealingWord },
    };

    private static readonly Dictionary<AbilityId, AbilityDefinition> ById = new();

    /// <summary>Indexes the table and checks every row and every kit.</summary>
    /// <remarks>Throws on purpose, so an ability added to the enum and not here, or a row or kit that can't work, shows up when the game starts.</remarks>
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
            if (Get(kit[0]).Bar != BarSlot.Auto) throw new InvalidOperationException($"Class {id}'s kit doesn't start with an auto attack.");
            foreach (AbilityId ability in kit)
                if (Array.IndexOf(PartyShared, ability) >= 0) throw new InvalidOperationException($"Class {id}'s kit has {ability}, which everyone has already.");
        }
    }

    /// <summary>The kits the rows are tuned on, as the engine takes them: a class's kit then what the party shares (or for a stat block, Attack and Dodge then what it lists), with nothing added to anyone's Focus a turn. SkillCatalog's class kits replace it.</summary>
    public static IKitSource Interim { get; } = new InterimKits();

    /// <summary>One ability.</summary>
    /// <param name="id">Which.</param>
    public static AbilityDefinition Get(AbilityId id) => ById[id];

    /// <summary>A class's own kit, in bar order: its auto attack first, then its skills and what's always on.</summary>
    /// <param name="id">Which class.</param>
    public static IReadOnlyList<AbilityId> Kit(ClassId id) => Kits[id];

    /// <summary>Whether an ability is one every stat block or every party member has, so a stat block has no need to list it.</summary>
    /// <param name="id">Which.</param>
    public static bool IsCommon(AbilityId id) => Array.IndexOf(PartyShared, id) >= 0 || Array.IndexOf(FoeShared, id) >= 0;

    /// <summary>Checks one row makes sense for its kind, and throws with what's wrong if it doesn't.</summary>
    private static void Check(AbilityDefinition row)
    {
        string name = $"Ability {row.Id}";
        if (string.IsNullOrWhiteSpace(row.Name) || string.IsNullOrWhiteSpace(row.Description))
            throw new InvalidOperationException($"{name} needs a name and a description.");
        if (row.Turns < 0 || row.Focus < 0 || row.Cooldown < 0 || row.Count < 0 || row.FocusGain < 0 || row.PoolPerLevel < 0)
            throw new InvalidOperationException($"{name} has a negative duration, Focus, cooldown, count, Focus gain or pool.");
        if (row.PoolPerLevel > 0 && !(row.Kind == AbilityKind.Heal && row.Heals.IsNone)) throw new InvalidOperationException($"{name} has a pool but isn't a pool's heal.");
        if (row.Item is not null && !ItemCatalog.Exists(row.Item))
            throw new InvalidOperationException($"{name} uses up {row.Item}, which isn't in ItemCatalog.");
        if (row.Concentration && !row.Spell) throw new InvalidOperationException($"{name} needs concentration but isn't a spell.");
        if (row.Save is null && (row.Kind == AbilityKind.Save || row.Dc > 0 || row.SaveHalves))
            throw new InvalidOperationException($"{name} has a DC or a save's damage but no ability to save with.");
        if (row.Kind == AbilityKind.Passive && row.Bar is not (BarSlot.Passive or BarSlot.Hidden))
            throw new InvalidOperationException($"{name} is always on, so it's a Passive on the bar or Hidden.");

        bool damages = !row.Damage.IsNone;
        bool conditions = row.Conditions.Count > 0;
        bool broken = row.Kind switch
        {
            AbilityKind.SpellAttack => !damages || !row.Spell,
            AbilityKind.Save => !damages && !conditions,
            AbilityKind.AutoHit => !damages || row.Count < 1,
            AbilityKind.Heal => row.Heals.IsNone && row.PoolPerLevel <= 0,
            AbilityKind.Thrown => row.Item is null || ItemCatalog.Get(row.Item).ThrownDamage.IsNone,
            AbilityKind.Condition or AbilityKind.Help => !conditions,
            AbilityKind.SoulLink => !damages,
            _ => false,
        };
        if (broken) throw new InvalidOperationException($"{name} is missing what a {row.Kind} ability needs (damage, conditions, a Count, healing or a pool, an item, or to be a spell).");
    }

    /// <summary>The kits behind Interim.</summary>
    private sealed class InterimKits : IKitSource
    {
        /// <inheritdoc/>
        public IReadOnlyList<AbilityId> Kit(Combatant who)
        {
            if (who.Member is null)
            {
                var block = new List<AbilityId>(FoeShared);
                foreach (AbilityId id in who.Enemy.Abilities)
                    if (!block.Contains(id)) block.Add(id);
                return block;
            }

            var kit = new List<AbilityId>(Kits[who.Member.Class]);
            kit.AddRange(PartyShared);
            return kit;
        }

        /// <inheritdoc/>
        public int FocusPerTurnBonus(Combatant who) => 0;
    }
}
