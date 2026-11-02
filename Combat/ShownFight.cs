using System;
using System.Collections.Generic;

namespace LegendQuest.Combat;

/// <summary>
/// A fight as far as the screen has shown it: everyone's hit points, conditions and death saves,
/// who's rolled for initiative, who's died, whose turn and which round it is, and the Soul-Link's
/// charge, each as it was after the last event the screen played.
/// </summary>
/// <remarks>
/// The engine has done the whole of a command by the time it hands its events back, so reading its
/// Combatants while they're still being played would show a wolf dead before the arrow reaches it.
/// The screen reads these instead, and Show catches them up one event at a time as each one plays,
/// so the bars drain when the number pops and the turn order fills in as the dice land. It draws
/// nothing and only reads events, so it can't disagree with the engine once everything's played.
/// </remarks>
public sealed class ShownFight
{
    /// <summary>How many successes or failures a death save track holds.</summary>
    private const int DeathSaveTrack = 3;

    /// <summary>A death save that reaches this succeeds; a 1 counts twice; a 20 gets them up.</summary>
    private const int DeathSaveDc = 10;
    private const int Fumble = 1;
    private const int Natural20 = 20;

    private readonly CombatEngine _engine;
    private readonly Dictionary<string, int> _hp = new();
    private readonly Dictionary<string, List<ConditionId>> _conditions = new();
    private readonly Dictionary<string, (int Successes, int Failures)> _deathSaves = new();
    private readonly HashSet<string> _rolled = new();
    private readonly HashSet<string> _died = new();

    /// <summary>Whoever the last critical shown landed on, until its damage is shown, so a critical on someone Downed costs them two death saves.</summary>
    private string _critical;

    /// <summary>Starts from how everyone is before the fight begins.</summary>
    /// <param name="engine">The fight, before Begin.</param>
    public ShownFight(CombatEngine engine)
    {
        _engine = engine;
        foreach (Combatant each in engine.Combatants) Track(each, each.Hp);
    }

    /// <summary>The round the screen's in, or 0 before the first is shown.</summary>
    public int Round { get; private set; }

    /// <summary>The id of whoever the screen last said it was the turn of, or null before anyone's.</summary>
    public string Turn { get; private set; }

    /// <summary>The Soul-Link's charge as shown.</summary>
    public int Charge { get; private set; }

    /// <summary>Someone's hit points as shown.</summary>
    /// <param name="who">Who.</param>
    public int Hp(Combatant who) => _hp.TryGetValue(who.Id, out int hp) ? hp : who.Hp;

    /// <summary>The conditions on someone as shown, in the order they went on.</summary>
    /// <param name="who">Who.</param>
    public IReadOnlyList<ConditionId> Conditions(Combatant who) =>
        _conditions.TryGetValue(who.Id, out List<ConditionId> on) ? on : Array.Empty<ConditionId>();

    /// <summary>Someone's death saves as shown: successes and failures, each up to three.</summary>
    /// <param name="who">Who.</param>
    public (int Successes, int Failures) DeathSaves(Combatant who) => _deathSaves.GetValueOrDefault(who.Id);

    /// <summary>Whether someone's shown at 0 hit points and not yet dead: Downed, or a foe knocked out in a fight to yield.</summary>
    /// <param name="who">Who.</param>
    public bool IsDown(Combatant who) => !HasDied(who) && Hp(who) <= 0;

    /// <summary>Whether someone's death has been shown: a foe gone, or a party member fallen for the rest of the fight.</summary>
    /// <param name="who">Who.</param>
    public bool HasDied(Combatant who) => _died.Contains(who.Id);

    /// <summary>Whether someone's initiative roll has been shown, so they're on the turn order.</summary>
    /// <param name="who">Who.</param>
    public bool HasRolled(Combatant who) => _rolled.Contains(who.Id);

    /// <summary>The turn order as shown: everyone who's rolled, in the engine's order, without the foes who've died. A fallen party member keeps their place, greyed, since they're still yours.</summary>
    public IReadOnlyList<Combatant> Order
    {
        get
        {
            var order = new List<Combatant>();
            foreach (Combatant each in _engine.Combatants)
                if (_rolled.Contains(each.Id) && !(each.Side == Side.Foes && _died.Contains(each.Id))) order.Add(each);
            order.Sort(CompareInitiative);
            return order;
        }
    }

    /// <summary>Catches up with one event as it's shown.</summary>
    /// <param name="happened">The event.</param>
    public void Show(CombatEvent happened)
    {
        Combatant actor = happened.Actor is null ? null : _engine.Find(happened.Actor);
        Combatant target = happened.Target is null ? null : _engine.Find(happened.Target);

        switch (happened.Kind)
        {
            case CombatEventKind.Joined:
                if (actor is not null) Track(actor, actor.MaxHp);
                break;

            case CombatEventKind.InitiativeRolled:
                if (actor is not null) _rolled.Add(actor.Id);
                break;

            case CombatEventKind.RoundStarted:
                Round = happened.Amount;
                break;

            case CombatEventKind.TurnStarted:
                Turn = happened.Actor;
                break;

            case CombatEventKind.AttackRolled:
                _critical = null;
                break;

            case CombatEventKind.Critical:
                _critical = happened.Target;
                break;

            case CombatEventKind.Damaged when target is not null:
                // A hit on someone already down is written out ("is hit while down"); a fresh one isn't.
                if (happened.Text is not null) Fail(target.Id, _critical == target.Id ? 2 : 1);
                else _hp[target.Id] = Math.Max(0, Hp(target) - happened.Amount);
                _critical = null;
                break;

            case CombatEventKind.Healed when target is not null:
                _hp[target.Id] = Math.Min(target.MaxHp, Hp(target) + happened.Amount);
                break;

            case CombatEventKind.ConditionAdded when target is not null:
                List<ConditionId> on = ConditionsOf(target.Id);
                if (!on.Contains((ConditionId)happened.Amount)) on.Add((ConditionId)happened.Amount);
                break;

            case CombatEventKind.ConditionRemoved when target is not null:
                ConditionsOf(target.Id).Remove((ConditionId)happened.Amount);
                break;

            case CombatEventKind.Downed when target is not null:
                _hp[target.Id] = 0;
                _deathSaves[target.Id] = (0, 0);
                break;

            case CombatEventKind.DeathSave when target is not null:
                ShowDeathSave(target.Id, happened.Amount);
                break;

            case CombatEventKind.Revived when target is not null:
                _deathSaves[target.Id] = (0, 0);
                break;

            case CombatEventKind.Died when target is not null:
                _died.Add(target.Id);
                _hp[target.Id] = 0;
                ConditionsOf(target.Id).Clear();
                break;

            case CombatEventKind.SoulLinkCharged:
                Charge = happened.Amount;
                break;

            case CombatEventKind.SoulLinkStrike:
                Charge = 0;
                break;
        }
    }

    /// <summary>Starts keeping track of someone: their hit points as given, and whatever they start the fight with on them.</summary>
    private void Track(Combatant who, int hp)
    {
        _hp[who.Id] = hp;
        List<ConditionId> on = ConditionsOf(who.Id);
        on.Clear();
        foreach (ActiveCondition condition in who.Conditions) on.Add(condition.Id);
    }

    /// <summary>Someone's shown conditions, made if they've none yet.</summary>
    private List<ConditionId> ConditionsOf(string id)
    {
        if (!_conditions.TryGetValue(id, out List<ConditionId> on))
        {
            on = new List<ConditionId>();
            _conditions[id] = on;
        }
        return on;
    }

    /// <summary>A death save's d20, the engine's way: 10 or more a success, a 1 two failures, a 20 back up (the Revived after it wipes the track).</summary>
    private void ShowDeathSave(string id, int roll)
    {
        if (roll == Natural20) return;
        if (roll >= DeathSaveDc)
        {
            (int successes, int failures) = _deathSaves.GetValueOrDefault(id);
            _deathSaves[id] = (Math.Min(DeathSaveTrack, successes + 1), failures);
            return;
        }
        Fail(id, roll == Fumble ? 2 : 1);
    }

    /// <summary>Marks failed death saves on someone's track.</summary>
    private void Fail(string id, int count)
    {
        (int successes, int failures) = _deathSaves.GetValueOrDefault(id);
        _deathSaves[id] = (successes, Math.Min(DeathSaveTrack, failures + count));
    }

    /// <summary>The engine's initiative order: the higher roll, then the higher Dexterity, then the party, then whoever joined first.</summary>
    private static int CompareInitiative(Combatant a, Combatant b)
    {
        int order = b.Initiative.CompareTo(a.Initiative);
        if (order == 0) order = b.Scores.Dexterity.CompareTo(a.Scores.Dexterity);
        if (order == 0) order = a.Side.CompareTo(b.Side);
        return order != 0 ? order : a.Joined.CompareTo(b.Joined);
    }
}
