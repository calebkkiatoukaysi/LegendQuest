using System;
using System.Collections.Generic;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// Allies: the supports behind the rows and what they do at the end of each round, the changes a
/// script makes to a fight that's going, and the fight's other ways of ending.
/// </summary>
/// <remarks>
/// Supports act after the round's last turn and before the next round starts, in the encounter's
/// order. They always land, with the engine's Random, and their damage has nobody behind it, so it
/// never charges the Soul-Link. On the party's own side what they roll heals rather than hurts.
/// Guests come in as CombatantSetups (FightRoster's), and fall back at 0 hit points (Health).
/// </remarks>
public sealed partial class CombatEngine
{
    private readonly List<SupportState> _supports = new();

    /// <summary>How many foes scripts have added, for their ids.</summary>
    private int _added;

    /// <summary>The fight's supports that came (their story flag held), in the encounter's order.</summary>
    public IReadOnlyList<SupportState> Supports => _supports;

    /// <summary>The conversation the fight's waiting on before the next round, by its DialogueCatalog id, or null. Always null for now: validation turns down an encounter with an interlude until the pause for one comes in.</summary>
    public string PendingInterlude => null;

    /// <summary>Makes a change a script asked for in the middle of the fight: a support stood down, foes added, a guest joining.</summary>
    /// <param name="order">What to change.</param>
    /// <returns>What it caused: a support's last line, the newcomers joining and rolling initiative.</returns>
    public IReadOnlyList<CombatEvent> Apply(FightOrder order)
    {
        if (order is null || Outcome != CombatOutcome.Ongoing) return Flush();

        switch (order.Kind)
        {
            case FightOrderKind.SupportOff:
                foreach (SupportState support in _supports)
                {
                    if (support.Id != order.Id || support.Off) continue;
                    support.Off = true;
                    support.Ready = false;
                    Emit(CombatEventKind.Log, null, null, 0, $"{support.Support.Name.ToUpperInvariant()} can't help any more.");
                }
                break;

            case FightOrderKind.AddFoes:
                EnemyDefinition enemy = EnemyCatalog.Get(order.Id);
                for (int i = 0; i < Math.Max(1, order.Count); i++)
                    Bring(new CombatantSetup($"{order.Id}-added-{++_added}", Side.Foes, null, enemy, false));
                break;

            case FightOrderKind.AddGuest:
                Bring(CombatantSetup.ForGuest(new EncounterGuest(order.Id)));
                break;
        }

        CheckOutcome();
        return Flush();
    }

    /// <summary>Works out which supports act at the end of this round: those with an ability, between their first round and their last, that nothing's stood down.</summary>
    private void RefreshSupports()
    {
        foreach (SupportState support in _supports)
        {
            EncounterSupport line = support.Support;
            support.Ready = !support.Off && line.Ability is { Target: not SupportTarget.None }
                && Round >= line.FromRound && (line.UntilRound <= 0 || Round <= line.UntilRound);
        }
    }

    /// <summary>The end of a round's supports, each in turn, while the fight's still going.</summary>
    private void SupportsAct()
    {
        foreach (SupportState support in _supports)
        {
            if (Outcome != CombatOutcome.Ongoing) return;
            if (support.Ready) Act(support);
        }
    }

    /// <summary>One support's ability: its dice on each of those it lands on (as healing on the party's side), then its condition.</summary>
    private void Act(SupportState support)
    {
        SupportAbility ability = support.Support.Ability;
        string name = support.Support.Name.ToUpperInvariant();
        string kind = ability.Type.ToString().ToLowerInvariant();

        foreach (Combatant target in Landing(ability))
        {
            if (Outcome != CombatOutcome.Ongoing) return;
            if (!Standing(target)) continue;

            RollResult roll = ability.Damage.IsNone ? null : ability.Damage.Roll(_random, name);
            int amount = roll is null ? 0 : Math.Max(0, roll.Total);
            bool friend = target.Side == Side.Party;
            string text = roll is null ? $"{name}: {ability.Text}"
                : friend ? $"{name}: {target.Label} gets {ability.Damage} = {amount} HP." : $"{name}: {target.Label} takes {ability.Damage} = {amount} {kind}.";
            _events.Add(new CombatEvent(CombatEventKind.SupportActed, support.Id, target.Id, amount, text, roll, null, SlotOf(target), null, ability.Type));

            if (amount > 0)
            {
                if (friend) Heal(target, amount, null);
                else Damage(target, amount, ability.Type, null, false, null, roll);
            }
            if (ability.Condition is ConditionId condition && Standing(target)) AddCondition(target, condition, ability.Turns, null, null);
        }
    }

    /// <summary>Who a support's ability lands on now, in slot order. If it names a foe, only that one (by its id in the fight, or its enemy id) is in the running.</summary>
    private List<Combatant> Landing(SupportAbility ability)
    {
        var foes = new List<Combatant>();
        foreach (Combatant each in OnField(Side.Foes))
            if (Standing(each) && (ability.Foe is null || each.Id == ability.Foe || each.Enemy?.Id == ability.Foe)) foes.Add(each);

        var landing = new List<Combatant>();
        switch (ability.Target)
        {
            case SupportTarget.RandomFoe:
                if (foes.Count > 0) landing.Add(foes[_random.Next(foes.Count)]);
                break;
            case SupportTarget.Foe:
                if (foes.Count > 0) landing.Add(foes[0]);
                break;
            case SupportTarget.FoeRow:
                Row front = Formation.FrontOf(Side.Foes);
                foreach (Combatant each in foes)
                    if (SlotOf(each) is Slot slot && slot.Row == front) landing.Add(each);
                break;
            case SupportTarget.AllFoes:
                landing.AddRange(foes);
                break;
            case SupportTarget.YourFrontRow:
                AddStanding(landing, Formation.InRow(Side.Party, Formation.FrontOf(Side.Party)));
                break;
            case SupportTarget.AllAllies:
                AddStanding(landing, OnField(Side.Party));
                break;
        }
        return landing;
    }

    /// <summary>Whether the foe the fight ends on (or all the foes together) is down to the share of its hit points that wins it.</summary>
    private bool HurtEnough()
    {
        if (_encounter.EndsAtHealthShare <= 0f) return false;
        int hp = 0, most = 0;
        foreach (Combatant each in _combatants)
        {
            if (each.Side != Side.Foes || _encounter.EndsAtFoe is not null && each.Id != _encounter.EndsAtFoe && each.Enemy?.Id != _encounter.EndsAtFoe) continue;
            hp += each.IsDead ? 0 : each.Hp;
            most += each.MaxHp;
        }
        return most > 0 && hp <= _encounter.EndsAtHealthShare * most;
    }
}
