using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Rows: who can reach whom, Shifting from one slot to another, the opportunity attack for leaving
/// the front, pulls and pushes, and how much a Shift would risk, for EnemyMind.
/// </summary>
/// <remarks>
/// <para>
/// Reach is 11-combat.md §2.2's. "The opposing front" is the other side's effective front row (its
/// back row when nobody's standing in front), and anyone lying in its front row. Melee needs you in
/// your own effective front row and reaches only the opposing front; a reach weapon reaches either
/// row from your front and the opposing front from your back; ranged attacks, spells and "any row"
/// skills reach anyone. Allies are always in reach.
/// </para>
/// <para>
/// A Shift is once a turn and free: a swap with an ally next to you (beside you in your row, or in
/// your lane in the other), or a step into an empty slot in the other row. Restrained, Prone and
/// Slowed stop it. Leaving the front row for the back while a foe in theirs has its reaction lets
/// the one that'd hurt most swing first, unless you're Disengaged.
/// </para>
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>The slots someone could Shift to now, front row first, left to right. Empty if they can't Shift.</summary>
    /// <param name="who">Who.</param>
    public IReadOnlyList<Slot> Shifts(Combatant who)
    {
        var shifts = new List<Slot>();
        if (who is null) return shifts;
        foreach (Slot slot in Formation.AllSlots(who.Side))
            if (WhyNotShift(who, slot) is null) shifts.Add(slot);
        return shifts;
    }

    /// <summary>Why someone can't Shift to a slot, or null if they can.</summary>
    /// <param name="who">Who.</param>
    /// <param name="to">Where to.</param>
    /// <returns>A few words for the tooltip, or null.</returns>
    public string WhyNotShift(Combatant who, Slot to)
    {
        if (!_begun) return "The fight hasn't started.";
        if (Outcome != CombatOutcome.Ongoing) return "The fight's over.";
        if (who is null || who != Current) return "It isn't their turn.";
        if (!Standing(who)) return "They're down.";
        if (SlotOf(who) is not Slot from) return "They aren't on the field.";
        if (who.Big) return "Too big to shift.";
        if (who.Has(ConditionId.Restrained)) return "Restrained: they can't shift.";
        if (who.Has(ConditionId.Prone)) return "Prone: they can't shift.";
        if (who.Has(ConditionId.Slowed)) return "Slowed: they can't shift.";
        if (!who.Budget.Shift) return "They've shifted this turn.";
        if (to.Side != who.Side || to.Lane < 0 || to.Lane >= Slot.LanesOf(to.Side)) return "That isn't a slot on their side.";
        if (to == from) return "They're there already.";

        Combatant there = Formation.At(to);
        if (there is null) return to.Row != from.Row ? null : "Only into the other row, or a swap.";
        if (there.Big) return "Too big to swap with.";
        return Contains(Formation.NextTo(who), there) ? null : "Too far to swap with.";
    }

    /// <summary>The foe who'd get an attack in if someone Shifted to a slot, or null if nobody would: of those in the opposing front with their reaction, the one whose swing would do most.</summary>
    /// <param name="who">Who'd Shift.</param>
    /// <param name="to">Where to.</param>
    public Combatant Opportunist(Combatant who, Slot to)
    {
        if (who is null || who.Has(ConditionId.Disengaged) || SlotOf(who) is not Slot from) return null;
        if (from.Row != Row.Front || to.Row != Row.Back || !Formation.InFront(who)) return null;

        Combatant best = null;
        double most = -1.0;
        foreach (Combatant each in _combatants)
        {
            if (!Hostile(each, who) || !Standing(each) || !each.Budget.Reaction || !Formation.InFront(each)) continue;
            double expected = SwingOdds(each, who);
            if (expected <= most) continue;
            most = expected;
            best = each;
        }
        return best;
    }

    /// <summary>Shifts whoever's turn it is to a slot, if they can.</summary>
    /// <param name="to">Where to.</param>
    /// <returns>Everything it caused, the opportunity attack included; nothing if WhyNotShift says they can't.</returns>
    public IReadOnlyList<CombatEvent> Shift(Slot to)
    {
        if (Current is Combatant who) DoShift(who, to);
        return Flush();
    }

    /// <summary>The Shift itself, leaving the events for the caller to hand back.</summary>
    /// <remarks>The Shift's spent before the opportunity attack, and if that drops them they stay where they were.</remarks>
    /// <param name="who">Who.</param>
    /// <param name="to">Where to.</param>
    /// <returns>Whether they Shifted, or tried and were stopped.</returns>
    internal bool DoShift(Combatant who, Slot to)
    {
        if (WhyNotShift(who, to) is not null) return false;
        Slot from = SlotOf(who).Value;
        Combatant threat = Opportunist(who, to);
        who.Budget.SpendShift();

        if (threat is not null)
        {
            OpportunityAttack(threat, who);
            if (!Standing(who) || Outcome != CombatOutcome.Ongoing)
            {
                CheckOutcome();
                return true;
            }
        }

        Combatant there = Formation.At(to);
        Formation.MoveTo(who, to);
        string text = there is not null ? $"{who.Label} swaps places with {there.Label}."
            : to.Row == Row.Back ? $"{who.Label} falls back to the back row." : $"{who.Label} steps up to the front row.";
        EmitShift(who, from, to, text);
        if (there is not null) EmitShift(there, to, from, null);
        CheckOutcome();
        return true;
    }

    /// <summary>The hit points a Shift to a slot would be likely to cost: the opportunist's chance to hit times its average damage, or 0 if nobody gets a swing.</summary>
    /// <param name="who">Who'd Shift.</param>
    /// <param name="to">Where to.</param>
    internal double Risk(Combatant who, Slot to) => Opportunist(who, to) is Combatant threat ? SwingOdds(threat, who) : 0.0;

    /// <summary>How far an ability reaches for someone: its own reach, or for one that takes the weapon's, what they hold (a foe's longest attack), and anything else that doesn't say is ranged.</summary>
    /// <param name="who">Who'd use it.</param>
    /// <param name="ability">What.</param>
    internal Reach ReachOf(Combatant who, AbilityDefinition ability)
    {
        if (ability.Reach != Reach.Weapon) return ability.Reach;
        switch (ability.Kind)
        {
            case AbilityKind.Weapon:
                if (who.Member is not null) return who.MainHand.Reach;
                Reach longest = Reach.Melee;
                foreach (AttackProfile attack in who.Attacks) longest = (Reach)Math.Max((int)longest, (int)attack.Reach);
                return longest;
            case AbilityKind.OffHand:
                return who.OffHand?.Reach ?? Reach.Melee;
            case AbilityKind.SoulLink:
                return who.MainHand.Reach;
            default:
                return Reach.Ranged;
        }
    }

    /// <summary>Whether someone could get at a target with a reach, from where they both stand. Allies are always in reach.</summary>
    /// <param name="who">Who.</param>
    /// <param name="reach">The reach.</param>
    /// <param name="target">At whom.</param>
    internal bool CanReach(Combatant who, Reach reach, Combatant target)
    {
        if (!Hostile(who, target)) return true;
        return reach switch
        {
            Reach.Melee => Formation.InFront(who) && Exposed(target),
            Reach.Reach => Formation.InFront(who) || Exposed(target),
            _ => true,
        };
    }

    /// <summary>Whether someone's in "the opposing front" for whoever's against them: in their side's effective front row, or lying in its front row.</summary>
    private bool Exposed(Combatant who) => SlotOf(who) is Slot slot && (slot.Row == Row.Front || slot.Row == Formation.FrontOf(who.Side));

    /// <summary>Why an ability's out of reach, for the tooltip.</summary>
    private static string OutOfReach(Reach reach) =>
        reach == Reach.Melee ? "Out of reach: melee only reaches their front row, from yours." : "Out of reach from the back row: only their front row.";

    /// <summary>The expected damage of one swing with someone's best melee attack at a target: an opportunity attack's.</summary>
    private double SwingOdds(Combatant attacker, Combatant target)
    {
        Advantage advantage = AdvantageOf(attacker, target, false, false, out _);
        return AttackMath.HitChance(attacker.Melee.Bonus + attacker.RollBonus, target.ArmourClass, advantage) * attacker.Melee.AverageDamage;
    }

    /// <summary>An opportunity attack for leaving the front row: the reaction spent, and one swing with their best melee attack.</summary>
    private void OpportunityAttack(Combatant attacker, Combatant mover)
    {
        attacker.Budget.Spend(ActionCost.Reaction);
        Emit(CombatEventKind.OpportunityAttack, attacker, mover, 0, $"{attacker.Label} lashes out as {mover.Label} pulls away.", ability: AbilityId.Attack);
        Strike(attacker, attacker.Melee, mover, AbilityCatalog.Get(AbilityId.Attack), false);
    }

    /// <summary>A pull or a push landing: the target to the nearest free slot in its other row (nearest lane first), or swapped with whoever's in its lane there if that row's full. Big foes don't move.</summary>
    private void MoveRow(Combatant user, AbilityDefinition ability, Combatant target)
    {
        if (ability.RowMove == RowMove.None || target.Big || !Standing(target) || SlotOf(target) is not Slot was) return;
        Row row = ability.RowMove == RowMove.Pull ? Row.Front : Row.Back;
        if (was.Row == row) return;

        Slot to = NearestFree(target.Side, row, was.Lane) ?? was with { Row = row };
        Combatant there = Formation.At(to);
        if (there is not null && there.Big) return;

        Formation.MoveTo(target, to);
        string verb = ability.RowMove == RowMove.Pull ? "drags" : "knocks";
        EmitShift(target, was, to, $"{user.Label}'s {ability.Name.ToUpperInvariant()} {verb} {target.Label} into the {(row == Row.Front ? "front" : "back")} row.");
        if (there is not null) EmitShift(there, to, was, null);
    }

    /// <summary>The free slot in a row nearest a lane, or null if the row's full.</summary>
    private Slot? NearestFree(Side side, Row row, int lane)
    {
        int lanes = Slot.LanesOf(side);
        for (int step = 0; step < lanes; step++)
        {
            foreach (int each in new[] { lane - step, lane + step })
            {
                var slot = new Slot(side, row, each);
                if (each >= 0 && each < lanes && Formation.At(slot) is null) return slot;
            }
        }
        return null;
    }

    /// <summary>Notes someone moving from one slot to another, for the screen to walk them.</summary>
    private void EmitShift(Combatant who, Slot from, Slot to, string text) =>
        _events.Add(new CombatEvent(CombatEventKind.Shifted, who.Id, null, 0, text, null, from, to, null, null));

    /// <summary>Everyone on the field on one side, in slot order, a big foe once.</summary>
    private List<Combatant> OnField(Side side)
    {
        var field = new List<Combatant>();
        foreach (Slot slot in Formation.AllSlots(side))
            if (Formation.At(slot) is Combatant who && !field.Contains(who)) field.Add(who);
        return field;
    }

    /// <summary>Whether a list has someone in it.</summary>
    private static bool Contains(IEnumerable<Combatant> people, Combatant who)
    {
        foreach (Combatant each in people)
            if (each == who) return true;
        return false;
    }
}
