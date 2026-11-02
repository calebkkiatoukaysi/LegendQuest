using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Items;

namespace LegendQuest.Combat;

/// <summary>
/// Abilities: which ones someone has, whether one can be used right now and why not, what it'll do
/// if it is, and using it.
/// </summary>
public sealed partial class CombatEngine
{
    /// <summary>Everything someone can do from the hotbar, in hotbar order: the common actions, then their class's kit or their stat block's. Always-on abilities aren't on it.</summary>
    /// <param name="who">Who.</param>
    public IReadOnlyList<AbilityDefinition> Abilities(Combatant who)
    {
        var abilities = new List<AbilityDefinition>();
        foreach (AbilityId id in Kit(who))
        {
            AbilityDefinition ability = AbilityCatalog.Get(id);
            if (ability.Kind != AbilityKind.Passive && Offered(who, ability)) abilities.Add(ability);
        }
        return abilities;
    }

    /// <summary>Why someone can't use an ability on a target or a tile right now, or null if they can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom, for an ability pointed at someone.</param>
    /// <param name="cell">Where, for an area or a tile.</param>
    /// <returns>A few words for the tooltip, or null.</returns>
    public string WhyNot(Combatant who, AbilityDefinition ability, Combatant target, Point cell) =>
        Unusable(who, ability) ?? TargetFault(who, ability, target, cell)
        ?? ReachFrom(who, ability, who.Cell, ability.Target is TargetKind.Enemy or TargetKind.Ally ? target.Cell : cell);

    /// <summary>Why someone can't use an ability at all right now, whoever or wherever it's aimed: not their turn, nothing left to pay with, and so on. Null if they can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    internal string Unusable(Combatant who, AbilityDefinition ability)
    {
        if (!_begun) return "The fight hasn't started.";
        if (Outcome != CombatOutcome.Ongoing) return "The fight's over.";
        if (who != Current) return "It isn't their turn.";
        if (who.IsOut) return "They're down.";
        if (!Has(who, ability.Id) || !Offered(who, ability)) return "They can't do that.";
        if (ability.Kind == AbilityKind.Passive) return "It's always on.";
        if (!who.Budget.Has(ability.Cost)) return ability.Cost == ActionCost.BonusAction ? "No bonus action left." : "No action left.";
        if (ability.Spell && who.ArmourUntrained) return "Can't cast in armour they aren't trained for.";
        if (Unpaid(who, ability) is string unpaid) return unpaid;

        return ability.Kind switch
        {
            AbilityKind.OffHand when !who.Budget.Attacked => "Attack with the main hand first.",
            AbilityKind.SoulLink when !SoulLink.IsFull => "The bond isn't full yet.",
            AbilityKind.SoulLink when Partner(who) is not { IsOut: false } => $"{Partner(who)?.Name ?? "The Soul-Bound"} has to be standing.",
            AbilityKind.Potion => Undrinkable(who, ability),
            _ => null,
        };
    }

    /// <summary>Uses an ability for whoever's turn it is, if it can be used.</summary>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom, for an ability pointed at someone; ignored for one on themselves.</param>
    /// <param name="cell">Where, for an area or a tile.</param>
    /// <returns>Everything it caused; nothing if WhyNot says it can't be used.</returns>
    public IReadOnlyList<CombatEvent> Use(AbilityDefinition ability, Combatant target, Point cell)
    {
        Perform(ability, target, cell);
        return Flush();
    }

    /// <summary>The using itself, leaving the events for the caller to hand back.</summary>
    /// <returns>Whether it was used.</returns>
    internal bool Perform(AbilityDefinition ability, Combatant target, Point cell)
    {
        if (Current is not Combatant user || WhyNot(user, ability, target, cell) is not null) return false;
        if (ability.Target == TargetKind.Self) target = user;

        bool freeHex = ability.Id == AbilityId.Hex && HexCanMove(user);
        user.Budget.Spend(ability.Cost);
        if (!freeHex) Pay(user, ability);
        if (target is not null && target != user) user.Facing = FacingOf(user.Cell, target.Cell, user.Facing);
        else if (ability.Target is TargetKind.Area or TargetKind.Point) user.Facing = FacingOf(user.Cell, cell, user.Facing);
        if (ability.Concentration) Concentrate(user, ability.Id);

        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                user.Budget.Attacked = true;
                WeaponAction(user, ability, target);
                break;
            case AbilityKind.OffHand:
                Strike(user, user.OffHand, target, ability, true);
                break;
            case AbilityKind.Thrown:
                Throw(user, ability, target);
                break;
            case AbilityKind.SpellAttack:
                SpellAttack(user, ability, target);
                break;
            case AbilityKind.Save:
                SavingThrows(user, ability, target, cell);
                break;
            case AbilityKind.AutoHit:
                Darts(user, ability, target);
                break;
            case AbilityKind.Heal:
                Mend(user, ability, target);
                break;
            case AbilityKind.Potion:
                Drink(user, ability);
                break;
            case AbilityKind.Condition:
                Bestow(user, ability, target);
                break;
            case AbilityKind.Dash:
                user.Budget.Dash();
                Emit(CombatEventKind.AbilityUsed, user, user, 0, $"{user.Label} uses {ability.Name.ToUpperInvariant()}.", ability: ability.Id);
                break;
            case AbilityKind.Help:
                Help(user, ability, target);
                break;
            case AbilityKind.SoulLink:
                SoulLinkStrike(user, ability, target);
                break;
        }

        CheckOutcome();
        return true;
    }

    /// <summary>How far an ability reaches for someone, in metres: its own range, or their weapon's, or the thrown item's.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    internal float RangeOf(Combatant who, AbilityDefinition ability)
    {
        if (ability.Range > 0f) return ability.Range;
        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                float furthest = 0f;
                foreach (AttackProfile attack in who.Attacks) furthest = MathF.Max(furthest, attack.Range);
                return furthest;
            case AbilityKind.OffHand:
                return who.OffHand?.Range ?? 0f;
            case AbilityKind.SoulLink:
                return who.MainHand.Range;
            case AbilityKind.Thrown:
                return ItemCatalog.Get(ability.Item).ThrowRange;
            default:
                return 0f;
        }
    }

    /// <summary>Whether someone has an ability at all: everyone's common actions, and their class's kit or their stat block.</summary>
    internal bool Has(Combatant who, AbilityId id)
    {
        foreach (AbilityId each in Kit(who))
            if (each == id) return true;
        return false;
    }

    /// <summary>Everything someone has, in hotbar order.</summary>
    private static IEnumerable<AbilityId> Kit(Combatant who)
    {
        IReadOnlyList<AbilityId> common = who.Member is not null ? AbilityCatalog.CommonToParty : AbilityCatalog.CommonToFoes;
        IReadOnlyList<AbilityId> own = who.Member is not null ? AbilityCatalog.Kit(who.Member.Class) : who.Enemy.Abilities;
        foreach (AbilityId id in common) yield return id;
        foreach (AbilityId id in own) yield return id;
    }

    /// <summary>Whether an ability belongs on someone's hotbar in this fight: the off-hand attack only with a light weapon in each hand, the Soul-Link only for the duo, and the bag only for the party proper.</summary>
    private bool Offered(Combatant who, AbilityDefinition ability)
    {
        if (ability.Kind == AbilityKind.OffHand) return who.OffHand is not null;
        if (ability.Kind == AbilityKind.SoulLink) return who.IsDuo && SoulLink.Active;
        if (ability.Item is not null || ability.Kind == AbilityKind.Potion) return who.Member is not null && !who.Guest && Bag is not null;
        return true;
    }

    /// <summary>Why someone can't pay for an ability (slots, uses, a recharge, the bag), or null if they can.</summary>
    private string Unpaid(Combatant who, AbilityDefinition ability)
    {
        if (ability.Resource is not null)
        {
            if (who.Member is not null)
            {
                bool freeHex = ability.Id == AbilityId.Hex && HexCanMove(who);
                if (!freeHex && ResourceCatalog.Left(who.Member, ability.Resource) < ability.Spends)
                    return ability.Spell ? "No spell slots left." : "None left until a rest.";
            }
            else if (who.IsSpent(ability.Id)) return "Already used this fight.";
        }
        if (ability.RechargeOn > 0 && who.IsSpent(ability.Id)) return "Still recharging.";
        if (ability.Item is not null && Bag.Count(ability.Item) < 1) return "None left in the bag.";
        return null;
    }

    /// <summary>Why drinking something wouldn't work or wouldn't help, or null if it would.</summary>
    private string Undrinkable(Combatant who, AbilityDefinition ability)
    {
        if (ability.Item is null) return PickPotion(who) is null ? "No healing potions in the bag." : who.Hp >= who.MaxHp ? "Already at full health." : null;

        ItemDefinition item = ItemCatalog.Get(ability.Item);
        bool heals = !item.Heals.IsNone && who.Hp < who.MaxHp;
        bool restores = item.SlotsRestored > 0 && ResourceCatalog.SlotsSpent(who.Member) > 0;
        return heals || restores ? null : "It wouldn't do anything for them now.";
    }

    /// <summary>Why an ability can't be pointed at a target or a tile, wherever it's used from, or null if it can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom, for an ability pointed at someone.</param>
    /// <param name="cell">Where, for a tile.</param>
    internal string TargetFault(Combatant who, AbilityDefinition ability, Combatant target, Point cell)
    {
        switch (ability.Target)
        {
            case TargetKind.Enemy:
                if (target is null || !Hostile(who, target)) return "Pick a foe.";
                if (target.IsDead) return "They're dead.";
                if (target.Side == Side.Foes && Beaten(target)) return "They've given up.";
                return target.IsDown && _encounter.Nonlethal ? "Leave them be." : null;

            case TargetKind.Ally:
                if (target is null || Hostile(who, target)) return "Pick an ally.";
                if (ability.NotSelf && target == who) return "Not on themselves.";
                if (target.IsDead) return "They're beyond help.";
                if (ability.Kind == AbilityKind.Heal && !target.IsDown && target.Hp >= target.MaxHp) return "They aren't hurt.";
                return ability.Kind == AbilityKind.Condition && ability.Conditions.Count > 0 && target.Has(ability.Conditions[0]) ? "They have it already." : null;

            case TargetKind.Point:
                return !_grid.Walkable(cell) || At(cell) is not null ? "Pick a free tile." : null;

            default:
                return null;
        }
    }

    /// <summary>Why a tile's out of an ability's reach from where it'd be used (too far, or out of sight), or null if it isn't. Always null for one on the user.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="from">The tile they'd use it from.</param>
    /// <param name="cell">The target's tile, or the tile it's aimed at.</param>
    internal string ReachFrom(Combatant who, AbilityDefinition ability, Point from, Point cell)
    {
        if (ability.Target == TargetKind.Self) return null;
        if (!Measure.Within(from, cell, RangeOf(who, ability))) return "Out of range.";
        return ability.NeedsSight && !CanSee(from, cell) ? "Can't see there from here." : null;
    }

    /// <summary>Pays for an ability: a party member's slot or use, a foe's once-a-fight or recharge, and anything it uses up from the bag.</summary>
    /// <remarks>Lay on Hands pays as it heals, and a potion picked by need comes out of the bag when it's drunk.</remarks>
    private void Pay(Combatant who, AbilityDefinition ability)
    {
        bool pool = ability.Kind == AbilityKind.Heal && ability.Heals.IsNone;
        if (ability.Resource is not null && !pool)
        {
            if (who.Member is not null) ResourceCatalog.Spend(who.Member, ability.Resource, ability.Spends);
            else who.MarkSpent(ability.Id, true);
        }
        if (ability.RechargeOn > 0) who.MarkSpent(ability.Id, true);
        if (ability.Item is not null) Bag.Remove(ability.Item);
    }

    /// <summary>Whether a warlock's Hex can be moved for free: they're still concentrating on it, and nobody's hexed by them any more.</summary>
    internal bool HexCanMove(Combatant who)
    {
        if (who.Concentrating != AbilityId.Hex) return false;
        foreach (Combatant each in _combatants)
            if (!each.IsDead && each.Find(ConditionId.Hexed) is { } hex && hex.Source == who.Id) return false;
        return true;
    }

    /// <summary>The healing potion someone would drink: the smallest in the bag that makes up what they're missing, or the biggest there is if none does.</summary>
    /// <returns>The potion, or null if the bag has none.</returns>
    private ItemDefinition PickPotion(Combatant who)
    {
        if (Bag is null) return null;

        int missing = who.MaxHp - who.Hp;
        ItemDefinition enough = null, biggest = null;
        foreach (ItemDefinition item in ItemCatalog.All)
        {
            if (item.Kind != ItemKind.Consumable || item.Heals.IsNone || Bag.Count(item.Id) < 1) continue;
            if (biggest is null || item.Heals.Average > biggest.Heals.Average) biggest = item;
            if (item.Heals.Average >= missing && (enough is null || item.Heals.Average < enough.Heals.Average)) enough = item;
        }
        return enough ?? biggest;
    }
}
