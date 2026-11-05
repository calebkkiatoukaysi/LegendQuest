using System;
using System.Collections.Generic;
using LegendQuest.Items;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// The odds: what an ability would do to someone from where everyone stands, worked out without rolling anything,
/// for the hover tooltip and for EnemyMind's sums.
/// </summary>
/// <remarks>
/// The hit chance is AttackMath's exact one; a save's is counted the same way, face by face. The
/// damage range is the dice's least and most, with Sneak Attack and Hex in it when they'd ride on
/// the hit. A critical's doubled dice aren't in the most, since the tooltip shows what to expect.
/// It's from where everyone stands now, with a foe's share of the difficulty in its rolls.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>What an ability would do to a target from where everyone stands now.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom: null for one with no pick.</param>
    public AttackPreview Estimate(Combatant who, AbilityDefinition ability, Combatant target)
    {
        if (target is null && !NoPick(ability)) return Nothing;
        target ??= who;
        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                return WeaponOdds(who, target);

            case AbilityKind.OffHand:
                return who.OffHand is null ? Nothing : AttackOdds(who, target, who.OffHand.Bonus, who.OffHand.Damage, who.OffHand.Extra, who.OffHand.Ranged, who.OffHand.Sneaky);

            case AbilityKind.Thrown:
                ItemDefinition item = ItemCatalog.Get(ability.Item);
                return AttackOdds(who, target, who.Scores.Modifier(Ability.Dexterity), item.ThrownDamage, DiceExpression.None, true, false);

            case AbilityKind.SpellAttack:
                bool ranged = ReachOf(who, ability) == Reach.Ranged;
                return AttackOdds(who, target, who.SpellAttack, ability.Damage, DiceExpression.None, ranged, false);

            case AbilityKind.Save:
                int dc = DcOf(who, ability);
                Ability save = ability.Save ?? Ability.Dexterity;
                string text = $"DC {dc} {CombatStats.Abbreviation(save)} SAVE";
                int least = ability.SaveHalves ? ability.Damage.Min / 2 : 0;
                return new AttackPreview(FailChance(target, save, dc), Math.Max(0, least), Math.Max(0, ability.Damage.Max), text, SaveAdvantage(target, save), false);

            case AbilityKind.AutoHit:
                return new AttackPreview(1.0, ability.Count * ability.Damage.Min, ability.Count * ability.Damage.Max, null, Advantage.Normal, false);

            case AbilityKind.Heal:
                if (ability.Heals.IsNone)
                {
                    int pool = who.Pool.GetValueOrDefault(ability.Id);
                    int amount = Math.Min(pool, target.MaxHp - target.Hp);
                    return new AttackPreview(1.0, amount, amount, null, Advantage.Normal, true);
                }
                int flat = (ability.HealsAddCasting ? who.CastingModifier : 0) + (ability.HealsAddLevel ? who.Level : 0);
                return new AttackPreview(1.0, Math.Max(0, ability.Heals.Min + flat), Math.Max(0, ability.Heals.Max + flat), null, Advantage.Normal, true);

            case AbilityKind.Potion:
                DiceExpression heals = ability.Item is not null ? ItemCatalog.Get(ability.Item).Heals : PickPotion(target)?.Heals ?? DiceExpression.None;
                return new AttackPreview(1.0, Math.Max(0, heals.Min), Math.Max(0, heals.Max), null, Advantage.Normal, true);

            case AbilityKind.SoulLink:
                Combatant partner = Partner(who);
                if (partner is null) return Nothing;
                int min = who.MainHand.Damage.Min + partner.MainHand.Damage.Min + ability.Damage.Min;
                int max = who.MainHand.Damage.Max + partner.MainHand.Damage.Max + ability.Damage.Max;
                return new AttackPreview(1.0, Math.Max(0, min), max, null, Advantage.Normal, false);

            default:
                return Nothing;
        }
    }

    /// <summary>A preview for something that can't miss and does nothing to count: a Dash, a Dodge.</summary>
    private static AttackPreview Nothing => new(1.0, 0, 0, null, Advantage.Normal, false);

    /// <summary>The Attack action's odds: a party member's weapon, or every attack of a foe's that reaches (all of them with Multiattack, the best one without).</summary>
    private AttackPreview WeaponOdds(Combatant who, Combatant target)
    {
        if (who.Member is not null) return AttackOdds(who, target, who.MainHand.Bonus, who.MainHand.Damage, who.MainHand.Extra, who.MainHand.Ranged, who.MainHand.Sneaky);

        AttackProfile best = BestAttack(who, target) ?? who.MainHand;
        AttackPreview odds = AttackOdds(who, target, best.Bonus, best.Damage, best.Extra, best.Ranged, best.Sneaky);
        if (!Has(who, AbilityId.Multiattack)) return odds;

        int min = 0, max = 0;
        foreach (AttackProfile attack in who.Attacks)
        {
            if (!CanReach(who, attack.Reach, target)) continue;
            min += attack.Damage.Min;
            max += attack.Damage.Max;
        }
        return odds with { Min = Math.Max(0, min), Max = Math.Max(odds.Max, max) };
    }

    /// <summary>An attack roll's odds and damage, with Sneak Attack and Hex in the damage when they'd ride on it.</summary>
    private AttackPreview AttackOdds(Combatant who, Combatant target, int bonus, DiceExpression damage, DiceExpression extra, bool ranged, bool sneaky)
    {
        bonus += who.RollBonus;
        Advantage advantage = AdvantageOf(who, target, ranged, false, out _);
        int min = damage.Min + (extra.IsNone ? 0 : extra.Min);
        int max = damage.Max + (extra.IsNone ? 0 : extra.Max);

        if (sneaky && SneakAttackApplies(who, target, advantage, target.Has(ConditionId.Marked)))
        {
            DiceExpression sneak = AbilityCatalog.Get(AbilityId.SneakAttack).Damage;
            min += sneak.Min;
            max += sneak.Max;
        }
        if (target.Find(ConditionId.Hexed) is { } hex && hex.Source == who.Id)
        {
            min += hex.Die.Min;
            max += hex.Die.Max;
        }

        return new AttackPreview(AttackMath.HitChance(bonus, target.ArmourClass, advantage), Math.Max(0, min), Math.Max(0, max), null, advantage, false);
    }

    /// <summary>The chance someone fails a save, counted over the d20's faces, with advantage or disadvantage from their conditions.</summary>
    private static double FailChance(Combatant who, Ability ability, int dc)
    {
        int faces = Math.Clamp(Dice.D20 + 1 - (dc - who.SaveBonus(ability)), 0, Dice.D20);
        double once = faces / (double)Dice.D20;
        double makes = SaveAdvantage(who, ability) switch
        {
            Advantage.Advantage => 1.0 - (1.0 - once) * (1.0 - once),
            Advantage.Disadvantage => once * once,
            _ => once,
        };
        return 1.0 - makes;
    }
}
