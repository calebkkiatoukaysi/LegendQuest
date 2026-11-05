using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Items;

namespace LegendQuest.Combat;

/// <summary>
/// Abilities: which ones someone has, whether one can be used right now and why not, who it can be
/// pointed at and who it'd catch, what it costs, and using it.
/// </summary>
/// <remarks>
/// An ability costs Focus, and may have a cooldown counted in its user's own turns, be once a fight,
/// or draw on a pool. A stat block pays no Focus: an ability with a Focus cost is once a fight for
/// it instead, which is what the foes' spell-slot abilities became. Whom it can be pointed at is its
/// Shape and its Reach; Taunted, Charmed and Frozen narrow that.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>The Focus everyone gains at the start of each of their own turns, before anything their kit adds.</summary>
    internal const int FocusPerTurn = 15;

    /// <summary>Everything someone can do from the bar, in bar order: their kit from the fight's rules. Always-on abilities aren't on it.</summary>
    /// <param name="who">Who.</param>
    public IReadOnlyList<AbilityDefinition> Abilities(Combatant who)
    {
        var abilities = new List<AbilityDefinition>();
        foreach (AbilityId id in who.Kit)
        {
            AbilityDefinition ability = AbilityCatalog.Get(id);
            if (ability.Kind != AbilityKind.Passive && Offered(who, ability)) abilities.Add(ability);
        }
        return abilities;
    }

    /// <summary>Why someone can't use an ability on a target from where they stand, or null if they can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom: null for one with no pick (on themselves, or a whole row or side of theirs).</param>
    /// <returns>A few words for the tooltip, or null.</returns>
    public string WhyNot(Combatant who, AbilityDefinition ability, Combatant target) =>
        Unusable(who, ability) ?? PickFault(who, ability, target);

    /// <summary>Everyone an ability could be pointed at by someone from where they stand, whatever's left of their turn: the plates the screen lights, and EnemyMind's choices. Slot order, the party's side first.</summary>
    /// <remarks>For an ability with no pick (on themselves, their row, their side) it's just them. For a shape that catches more than its pick, any of these picks catches what Caught says.</remarks>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    public IReadOnlyList<Combatant> Targets(Combatant who, AbilityDefinition ability)
    {
        var targets = new List<Combatant>();
        if (who is null) return targets;
        if (NoPick(ability))
        {
            if (PickFault(who, ability, who) is null) targets.Add(who);
            return targets;
        }

        foreach (Side side in new[] { Side.Party, Side.Foes })
            foreach (Combatant each in OnField(side))
                if (PickFault(who, ability, each) is null) targets.Add(each);
        return targets;
    }

    /// <summary>Everyone an ability pointed at someone would catch, by its shape: the one picked, their neighbours, a row, a column, a whole side. Slot order.</summary>
    /// <remarks>Only those standing are caught by a shape that catches more than its pick. For RandomFoes it's everyone a pick could land on; the picks themselves (which can repeat) are only rolled when it's used.</remarks>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="picked">Who it's pointed at: null for one with no pick.</param>
    public IReadOnlyList<Combatant> Caught(Combatant who, AbilityDefinition ability, Combatant picked)
    {
        var caught = new List<Combatant>();
        Side theirs = who.Side == Side.Party ? Side.Foes : Side.Party;
        switch (ability.Shape)
        {
            case TargetShape.Self:
                caught.Add(who);
                break;
            case TargetShape.Ally or TargetShape.AllyNextTo or TargetShape.Foe:
                if (picked is not null) caught.Add(picked);
                break;
            case TargetShape.AllAllies:
                AddStanding(caught, OnField(who.Side));
                break;
            case TargetShape.YourRow:
                if (SlotOf(who) is Slot mine) AddStanding(caught, Formation.InRow(who.Side, mine.Row));
                break;
            case TargetShape.YourBackRow:
                AddStanding(caught, Formation.InRow(who.Side, Row.Back));
                break;
            case TargetShape.FoeAndNeighbours:
                if (picked is null) break;
                caught.Add(picked);
                AddStanding(caught, Formation.Neighbours(picked).Where(each => Hostile(each, who)));
                break;
            case TargetShape.FoeRow:
                if (SlotOf(picked) is Slot row) AddStanding(caught, Formation.InRow(picked.Side, row.Row));
                break;
            case TargetShape.FoeColumn:
                if (picked is null) break;
                foreach (Slot slot in Formation.SlotsOf(picked)) AddStanding(caught, Formation.Column(picked.Side, slot.Lane));
                break;
            case TargetShape.AllFoes:
                AddStanding(caught, OnField(theirs));
                break;
            case TargetShape.RandomFoes:
                foreach (Combatant each in OnField(theirs))
                    if (FoeFault(who, ability, each, false) is null) caught.Add(each);
                break;
        }
        return caught;
    }

    /// <summary>How many more of someone's own turns until an ability's off cooldown, or 0 if it's ready.</summary>
    /// <remarks>Counted from the turn it was used on, so one with a cooldown of 1 used this turn says 2 now and 1 on their next turn, and is back on the one after.</remarks>
    /// <param name="who">Who.</param>
    /// <param name="ability">What.</param>
    public int CooldownLeft(Combatant who, AbilityDefinition ability) =>
        who.ReadyOn.TryGetValue(ability.Id, out int ready) ? Math.Max(0, ready - who.TurnsTaken) : 0;

    /// <summary>What a foe means to do on its next turn and to whom, for the icon over it, or null if it means nothing yet.</summary>
    /// <remarks>EnemyMind's Predict: nothing moves and nothing's rolled, so asking changes nothing.</remarks>
    /// <param name="foe">Which foe.</param>
    public (AbilityDefinition Ability, Combatant Target)? Intent(Combatant foe) => foe is null ? null : EnemyMind.Predict(this, foe);

    /// <summary>Why someone can't use an ability at all right now, whoever it's aimed at: not their turn, nothing left to pay with, and so on. Null if they can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    public string Unusable(Combatant who, AbilityDefinition ability)
    {
        if (!_begun) return "The fight hasn't started.";
        if (Outcome != CombatOutcome.Ongoing) return "The fight's over.";
        if (who != Current) return "It isn't their turn.";
        if (who.IsOut) return "They're down.";
        if (!Has(who, ability.Id) || !Offered(who, ability)) return "They can't do that.";
        if (ability.Kind == AbilityKind.Passive) return "It's always on.";
        if (!who.Budget.Has(ability.Cost)) return ability.Cost == ActionCost.BonusAction ? "No bonus action left." : "No action left.";
        if (ability.Spell && who.ArmourUntrained) return "Can't cast in armour they aren't trained for.";
        if (Unready(who, ability) is string unready) return unready;

        return ability.Kind switch
        {
            AbilityKind.OffHand when !who.Budget.Attacked => "Attack with the main hand first.",
            AbilityKind.SoulLink when !SoulLink.IsFull => "The bond isn't full yet.",
            AbilityKind.SoulLink when Partner(who) is not { IsOut: false } => $"{Partner(who)?.Name ?? "The Soul-Bound"} has to be standing.",
            AbilityKind.Potion when ability.Item is null && PickPotion(who) is null => "No healing potions in the bag.",
            _ => null,
        };
    }

    /// <summary>Uses an ability for whoever's turn it is, if it can be used.</summary>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom, for an ability with a pick; ignored for one without.</param>
    /// <returns>Everything it caused; nothing if WhyNot says it can't be used.</returns>
    public IReadOnlyList<CombatEvent> Use(AbilityDefinition ability, Combatant target)
    {
        Perform(ability, target);
        return Flush();
    }

    /// <summary>The using itself, leaving the events for the caller to hand back: paying for it, then what it does to everyone it catches, then the Focus an auto attack gives back.</summary>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom.</param>
    /// <returns>Whether it was used.</returns>
    internal bool Perform(AbilityDefinition ability, Combatant target)
    {
        if (Current is not Combatant user || WhyNot(user, ability, target) is not null) return false;
        if (NoPick(ability)) target = user;

        bool freeHex = ability.Id == AbilityId.Hex && HexCanMove(user);
        user.Budget.Spend(ability.Cost);
        if (!freeHex) Settle(user, ability);
        if (ability.Concentration) Concentrate(user, ability.Id);

        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                user.Budget.Attacked = true;
                foreach (Combatant each in Hits(user, ability, target)) WeaponAction(user, ability, each);
                break;
            case AbilityKind.OffHand:
                Strike(user, user.OffHand, target, ability, true);
                break;
            case AbilityKind.Thrown:
                foreach (Combatant each in Hits(user, ability, target)) Throw(user, ability, each);
                break;
            case AbilityKind.SpellAttack:
                foreach (Combatant each in Hits(user, ability, target)) SpellAttack(user, ability, each);
                break;
            case AbilityKind.Save:
                SavingThrows(user, ability, target);
                break;
            case AbilityKind.AutoHit:
                Darts(user, ability, target);
                break;
            case AbilityKind.Heal:
                foreach (Combatant each in Hits(user, ability, target)) Mend(user, ability, each);
                break;
            case AbilityKind.Potion:
                Drink(user, ability, target);
                break;
            case AbilityKind.Condition:
                foreach (Combatant each in Hits(user, ability, target)) Bestow(user, ability, each);
                break;
            case AbilityKind.Help:
                Help(user, ability, target);
                break;
            case AbilityKind.SoulLink:
                SoulLinkStrike(user, ability, target);
                break;
        }

        if (ability.FreeShift && !user.IsOut)
        {
            user.Budget.GiveShift();
            if (!user.Has(ConditionId.Disengaged)) AddCondition(user, ConditionId.Disengaged, 1, user, ability.Id);
        }
        if (ability.FocusGain > 0 && !user.IsOut) GainFocus(user, ability.FocusGain);

        CheckOutcome();
        return true;
    }

    /// <summary>Whether someone has an ability at all: whether it's in their kit.</summary>
    internal bool Has(Combatant who, AbilityId id)
    {
        foreach (AbilityId each in who.Kit)
            if (each == id) return true;
        return false;
    }

    /// <summary>Whether a warlock's Hex can be moved for free: they're still concentrating on it, and nobody's hexed by them any more.</summary>
    internal bool HexCanMove(Combatant who)
    {
        if (who.Concentrating != AbilityId.Hex) return false;
        foreach (Combatant each in _combatants)
            if (!each.IsDead && each.Find(ConditionId.Hexed) is { } hex && hex.Source == who.Id) return false;
        return true;
    }

    /// <summary>What an ability costs someone in Focus: its cost for a party member or a companion guest, nothing for a stat block.</summary>
    internal static int FocusCost(Combatant who, AbilityDefinition ability) => who.Member is not null ? ability.Focus : 0;

    /// <summary>Whether an ability works once a fight for someone: an ultimate, or for a stat block anything with a Focus cost.</summary>
    internal static bool OnceAFight(Combatant who, AbilityDefinition ability) => ability.OncePerFight || who.Member is null && ability.Focus > 0;

    /// <summary>Whether an ability belongs on someone's bar in this fight: the off-hand attack only with a light weapon in each hand, the Soul-Link only for the duo while the bond holds, and the bag only for the party proper.</summary>
    private bool Offered(Combatant who, AbilityDefinition ability)
    {
        if (ability.Kind == AbilityKind.OffHand) return who.OffHand is not null;
        if (ability.Kind == AbilityKind.SoulLink) return who.IsDuo && SoulLink.Active;
        if (ability.Item is not null || ability.Kind == AbilityKind.Potion) return who.Member is not null && !who.Guest && Bag is not null;
        return true;
    }

    /// <summary>Why someone can't pay for an ability (Focus, a cooldown, once a fight, a recharge, an empty pool, the bag), or null if they can.</summary>
    private string Unready(Combatant who, AbilityDefinition ability)
    {
        if (OnceAFight(who, ability) && who.UsedThisFight.Contains(ability.Id)) return "Once a fight.";
        int left = CooldownLeft(who, ability);
        if (left > 0) return left == 1 ? "Ready in 1 turn." : $"Ready in {left} turns.";
        int cost = FocusCost(who, ability);
        if (who.Focus < cost && !(ability.Id == AbilityId.Hex && HexCanMove(who))) return $"Needs {cost} Focus.";
        if (ability.RechargeOn > 0 && who.IsSpent(ability.Id)) return "Still recharging.";
        if (ability.PoolPerLevel > 0 && who.Pool.GetValueOrDefault(ability.Id) <= 0) return "Nothing left in the pool.";
        if (ability.Item is not null && (Bag is null || Bag.Count(ability.Item) < 1)) return "None left in the bag.";
        return null;
    }

    /// <summary>Pays for an ability: its Focus, its cooldown, its once a fight, its recharge, and anything it uses up from the bag. A pool pays as it heals.</summary>
    private void Settle(Combatant who, AbilityDefinition ability)
    {
        int cost = FocusCost(who, ability);
        if (cost > 0) GainFocus(who, -cost);
        if (ability.Cooldown > 0) who.ReadyOn[ability.Id] = who.TurnsTaken + ability.Cooldown + 1;
        if (OnceAFight(who, ability)) who.UsedThisFight.Add(ability.Id);
        if (ability.RechargeOn > 0) who.MarkSpent(ability.Id, true);
        if (ability.Item is not null) Bag.Remove(ability.Item);
    }

    /// <summary>Changes someone's Focus, and says so if it changed.</summary>
    private void GainFocus(Combatant who, int by)
    {
        int changed = who.ChangeFocus(by);
        if (changed != 0) Emit(CombatEventKind.FocusChanged, who, who, changed);
    }

    /// <summary>Whether an ability is pointed at nobody in particular: it's on the user, or on their whole row or side.</summary>
    private static bool NoPick(AbilityDefinition ability) =>
        ability.Shape is TargetShape.Self or TargetShape.AllAllies or TargetShape.YourRow or TargetShape.YourBackRow;

    /// <summary>Why an ability can't be pointed at a target, whatever's left of the turn, or null if it can.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom: ignored for one with no pick.</param>
    private string PickFault(Combatant who, AbilityDefinition ability, Combatant target)
    {
        if (ability.Kind == AbilityKind.Potion && ability.Shape == TargetShape.Self) return Undrinkable(ability, who);
        if (NoPick(ability)) return null;
        if (ability.Shape is TargetShape.Ally or TargetShape.AllyNextTo) return AllyFault(who, ability, target);
        return FoeFault(who, ability, target, true);
    }

    /// <summary>Why an ability for one ally can't go to this one, or null if it can.</summary>
    private string AllyFault(Combatant who, AbilityDefinition ability, Combatant target)
    {
        if (target is null || Hostile(who, target)) return "Pick an ally.";
        if (target.IsDead) return "They're beyond help.";
        if (SlotOf(target) is null) return target.FellBack ? "They've fallen back." : "They aren't on the field.";
        if (ability.NotSelf && target == who) return "Not on themselves.";
        if (ability.Shape == TargetShape.AllyNextTo && target != who && !Contains(Formation.NextTo(who), target)) return "They have to be next to them.";
        if (target.Has(ConditionId.Frozen)) return "Frozen in time: only an area can reach them.";
        if (ability.Kind == AbilityKind.Heal && !target.IsDown && target.Hp >= target.MaxHp) return "They aren't hurt.";
        if (ability.Kind == AbilityKind.Potion) return Undrinkable(ability, target);
        return ability.Kind == AbilityKind.Condition && ability.Conditions.Count > 0 && target.Has(ability.Conditions[0]) ? "They have it already." : null;
    }

    /// <summary>Why an ability for the other side can't be pointed at this one, or null if it can: who they are, whether it reaches, and what Frozen, Charmed and Taunted say.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    /// <param name="target">At whom.</param>
    /// <param name="taunt">Whether to hold a Taunted user to their taunter, which looking for the taunter itself mustn't.</param>
    private string FoeFault(Combatant who, AbilityDefinition ability, Combatant target, bool taunt)
    {
        if (target is null || !Hostile(who, target)) return "Pick a foe.";
        if (target.IsDead) return "They're dead.";
        if (target.Side == Side.Foes && Beaten(target)) return "They've given up.";
        if (target.IsDown && _encounter.Nonlethal) return "Leave them be.";
        if (SlotOf(target) is null) return "They aren't on the field.";
        if (ability.Shape == TargetShape.Foe && target.Has(ConditionId.Frozen)) return "Frozen in time: only an area can reach them.";
        if (who.Find(ConditionId.Charmed) is { } charm && charm.Source == target.Id) return "Charmed: they won't.";

        if (ability.Kind == AbilityKind.SoulLink)
        {
            foreach (Combatant striker in new[] { who, Partner(who) })
                if (striker is not null && AutoOf(striker) is AbilityDefinition auto && !CanReach(striker, ReachOf(striker, auto), target))
                    return $"{striker.Name} can't reach them.";
        }
        else if (ReachOf(who, ability) is var reach && !CanReach(who, reach, target)) return OutOfReach(reach);

        if (taunt && ability.Shape == TargetShape.Foe && Taunter(who) is Combatant taunter && taunter != target
            && FoeFault(who, ability, taunter, false) is null) return $"Taunted: it has to be {taunter.Name}.";
        return ability.Kind == AbilityKind.Condition && ability.Conditions.Count > 0 && target.Has(ability.Conditions[0]) ? "They have it already." : null;
    }

    /// <summary>Whoever taunted someone, if they're still standing; otherwise null.</summary>
    private Combatant Taunter(Combatant who) =>
        who.Find(ConditionId.Taunted) is { Source: string id } && Find(id) is Combatant taunter && Standing(taunter) ? taunter : null;

    /// <summary>Someone's auto attack: the first thing in their kit on the bar's Auto button, or null.</summary>
    private static AbilityDefinition AutoOf(Combatant who)
    {
        foreach (AbilityId id in who.Kit)
            if (AbilityCatalog.Get(id).Bar == BarSlot.Auto) return AbilityCatalog.Get(id);
        return null;
    }

    /// <summary>Who an ability lands on, one at a time: everyone it catches who's still up when their turn comes, or for RandomFoes its Count picks, each rolled from whoever's still up.</summary>
    private IEnumerable<Combatant> Hits(Combatant user, AbilityDefinition ability, Combatant target)
    {
        IReadOnlyList<Combatant> caught = Caught(user, ability, target);
        if (ability.Shape != TargetShape.RandomFoes)
        {
            foreach (Combatant each in caught)
                if (!each.IsDead && !(each.Side == Side.Foes && Beaten(each) && each != target)) yield return each;
            yield break;
        }

        for (int pick = 0; pick < Math.Max(1, ability.Count) && Outcome == CombatOutcome.Ongoing; pick++)
        {
            var left = caught.Where(Standing).ToList();
            if (left.Count == 0) yield break;
            yield return left[_random.Next(left.Count)];
        }
    }

    /// <summary>Adds everyone standing from a list who isn't in it yet.</summary>
    private void AddStanding(List<Combatant> caught, IEnumerable<Combatant> people)
    {
        foreach (Combatant each in people)
            if (Standing(each) && !caught.Contains(each)) caught.Add(each);
    }

    /// <summary>Why drinking something wouldn't work or wouldn't help whoever it's for, or null if it would.</summary>
    /// <param name="ability">What.</param>
    /// <param name="drinker">Who it's for.</param>
    private string Undrinkable(AbilityDefinition ability, Combatant drinker)
    {
        if (ability.Item is null)
        {
            if (PickPotion(drinker) is null) return "No healing potions in the bag.";
            return drinker.IsDown || drinker.Hp < drinker.MaxHp ? null : "Already at full health.";
        }

        ItemDefinition item = ItemCatalog.Get(ability.Item);
        bool heals = !item.Heals.IsNone && drinker.Hp < drinker.MaxHp;
        bool focus = item.FocusRestored > 0 && drinker.Focus < Combatant.MaxFocus;
        return heals || focus ? null : "It wouldn't do anything for them now.";
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
