using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Rules;

namespace LegendQuest.Combat;

/// <summary>
/// The fight's clock: initiative, rounds, whose turn it is, what happens at the start and end of
/// each, death saves, waves arriving, and how it ends.
/// </summary>
/// <remarks>
/// A turn starts with 15 Focus (and whatever the kit adds), after any burning, bleeding or
/// poison; Frozen and Stunned lose the turn, and getting up from Prone takes the Shift. A round ends
/// with the supports acting, then the fight's won if it only had to be survived this long. The next
/// one starts with its waves, the guests due in it, and any foes who were waiting for room.
/// </remarks>
public sealed partial class CombatEngine
{
    /// <summary>What a death save has to reach to succeed: 5e's 10.</summary>
    private const int DeathSaveDc = 10;

    /// <summary>How many successes stabilise someone Downed, and how many failures kill them.</summary>
    private const int DeathSavesNeeded = 3;

    /// <summary>The sides of the die a foe's ability recharges on.</summary>
    private const int RechargeDie = 6;

    /// <summary>How many turns can pass in one go without anybody being able to act before giving up, as many rounds as there are people plus one.</summary>
    private int TurnGuard => _order.Count * (_order.Count + 1) + 1;

    /// <summary>Starts the fight: everyone rolls initiative, round 1 starts, and the first turn with it.</summary>
    /// <returns>The rolls and the first turn starting.</returns>
    public IReadOnlyList<CombatEvent> Begin()
    {
        if (_begun) return Array.Empty<CombatEvent>();
        _begun = true;

        foreach (Combatant each in _combatants) RollInitiative(each);
        _order.AddRange(_combatants);
        _order.Sort(CompareInitiative);
        RefreshOrder();

        Round = 1;
        Emit(CombatEventKind.RoundStarted, null, null, Round, "Round 1.");
        RefreshSupports();
        _turn = -1;
        CheckOutcome();
        if (Outcome == CombatOutcome.Ongoing) NextTurn();
        return Flush();
    }

    /// <summary>Gives the fight to the party: "Win this fight", on Story after it's been lost twice.</summary>
    /// <returns>The concession, then the victory; nothing if the fight isn't going.</returns>
    public IReadOnlyList<CombatEvent> Concede()
    {
        if (!_begun || Outcome != CombatOutcome.Ongoing) return Array.Empty<CombatEvent>();
        Emit(CombatEventKind.Conceded, null, null, 0, "The fight is yours.");
        Win();
        return Flush();
    }

    /// <summary>Ends whoever's turn it is: their conditions tick, and the next turn starts. A Downed party member's turn is only their death save, so it's rolled and passed by here too.</summary>
    /// <remarks>An AI turn isn't played by this; RunAiTurn does that when the screen asks.</remarks>
    /// <returns>What happened.</returns>
    public IReadOnlyList<CombatEvent> EndTurn()
    {
        if (!_begun || Outcome != CombatOutcome.Ongoing || Current is not Combatant ending) return Array.Empty<CombatEvent>();
        FinishTurn(ending);
        NextTurn();
        return Flush();
    }

    /// <summary>Plays the whole of the current turn with EnemyMind, ends it, and starts the next: a foe's, a guest's, or a party member's when the simulator's playing them.</summary>
    /// <returns>Everything that happened, the next turn starting included.</returns>
    public IReadOnlyList<CombatEvent> RunAiTurn()
    {
        if (!_begun || Outcome != CombatOutcome.Ongoing || Current is not Combatant playing) return Array.Empty<CombatEvent>();

        EnemyMind.Play(this, playing);
        if (Outcome == CombatOutcome.Ongoing && Current == playing)
        {
            FinishTurn(playing);
            NextTurn();
        }
        return Flush();
    }

    /// <summary>Rolls someone's initiative: d20 + Dexterity.</summary>
    private void RollInitiative(Combatant who)
    {
        int dexterity = who.Scores.Modifier(Ability.Dexterity);
        int roll = Dice.Roll(_random, Dice.D20);
        who.Initiative = roll + dexterity;

        var result = new RollResult { Dice = new[] { roll }, Modifiers = new[] { ("DEX", dexterity) }, Total = who.Initiative };
        Emit(CombatEventKind.InitiativeRolled, who, null, who.Initiative,
            $"{who.Label} rolls initiative: {roll} {CombatText.Signed(dexterity)} = {who.Initiative}.", result);
    }

    /// <summary>Initiative order: the higher roll, then the higher Dexterity, then the party, then whoever joined first.</summary>
    private static int CompareInitiative(Combatant a, Combatant b)
    {
        int order = b.Initiative.CompareTo(a.Initiative);
        if (order == 0) order = b.Scores.Dexterity.CompareTo(a.Scores.Dexterity);
        if (order == 0) order = a.Side.CompareTo(b.Side);
        return order != 0 ? order : a.Joined.CompareTo(b.Joined);
    }

    /// <summary>Moves the turn on to the next person who can act, starting a new round (and its waves) when it wraps.</summary>
    /// <remarks>Anyone who can't act (Downed, stable, or a foe who's given up) has their turn started and finished on the spot, death save and all.</remarks>
    private void NextTurn()
    {
        for (int guard = TurnGuard; Outcome == CombatOutcome.Ongoing && guard > 0; guard--)
        {
            if (_turn + 1 >= _order.Count)
            {
                EndRound();
                if (Outcome != CombatOutcome.Ongoing) return;
                _turn = -1;
                Round++;
                Emit(CombatEventKind.RoundStarted, null, null, Round, $"Round {Round}.");
                while (_wave < _encounter.Waves.Count && _encounter.Waves[_wave].Round <= Round) Arrive(_encounter.Waves[_wave++]);
                StartRound();
            }

            _turn++;
            Combatant next = _order[_turn];
            if (next.IsDead || next.FellBack) continue;
            if (BeginTurn(next)) return;
            FinishTurn(next);
        }
    }

    /// <summary>The start of someone's turn: their budget back with its Shift, Dodging off, recharges, a death save if they're Downed, burning, bleeding and poison, losing the turn to Frozen or Stunned, then their Focus, and getting up from Prone with the Shift.</summary>
    /// <returns>Whether they can do anything this turn.</returns>
    private bool BeginTurn(Combatant who)
    {
        _turnNumber++;
        who.TurnsTaken++;
        who.Budget.Start();
        Emit(CombatEventKind.TurnStarted, who, null);

        RemoveCondition(who, ConditionId.Dodging);
        Recharge(who);

        if (who.FellBack) return false;
        if (who.IsDown && !who.Has(ConditionId.Stable)) DeathSave(who);
        if (who.IsOut || Beaten(who)) return false;

        foreach (ConditionId hurt in new[] { ConditionId.Burning, ConditionId.Bleeding, ConditionId.Poisoned })
        {
            if (who.Find(hurt) is not ActiveCondition hurting || hurting.Die.IsNone) continue;
            DamageType kind = hurt switch { ConditionId.Burning => DamageType.Fire, ConditionId.Poisoned => DamageType.Poison, _ => DamageType.Slashing };
            RollResult roll = hurting.Die.Roll(_random, ConditionInfo.Name(hurt).ToUpperInvariant());
            int amount = Math.Max(0, roll.Total);
            Emit(CombatEventKind.Log, who, who, 0, $"{who.Label} is {ConditionInfo.Word(hurt)}: {hurting.Die} = {amount} {kind.ToString().ToLowerInvariant()}.", roll, hurting.From);
            Damage(who, amount, kind, null, false, hurting.From, roll);
            if (who.IsOut || Beaten(who) || Outcome != CombatOutcome.Ongoing) return false;
        }

        if ((who.Find(ConditionId.Frozen) ?? who.Find(ConditionId.Stunned)) is ActiveCondition held)
        {
            Emit(CombatEventKind.Log, who, null, 0, $"{who.Label} is {ConditionInfo.Word(held.Id)}, and loses the turn.");
            return false;
        }

        GainFocus(who, FocusPerTurn + _rules.Kits.FocusPerTurnBonus(who));
        if (who.Has(ConditionId.Prone))
        {
            who.Budget.SpendShift();
            RemoveCondition(who, ConditionId.Prone);
        }
        return true;
    }

    /// <summary>The end of a round: the supports act, then the fight's won if it only had to be survived to the end of this one.</summary>
    private void EndRound()
    {
        Emit(CombatEventKind.RoundEnded, null, null, Round, null);
        SupportsAct();
        if (Outcome != CombatOutcome.Ongoing) return;
        if (_encounter.EndsAfterRounds > 0 && Round >= _encounter.EndsAfterRounds)
        {
            Emit(CombatEventKind.Log, null, null, Round, $"They held for {Round} rounds.");
            Win();
        }
    }

    /// <summary>The start of a round, after its waves: whoever's due this round joins, foes waiting for room get it if there is some, and the supports see whether they act.</summary>
    private void StartRound()
    {
        _hitThisRound.Clear();
        foreach (CombatantSetup setup in _waiting.ToArray())
        {
            if (setup.FromRound > Round) continue;
            _waiting.Remove(setup);
            Bring(setup);
        }
        PlaceQueued();
        RefreshSupports();
    }

    /// <summary>Gives foes waiting for room a slot each, in the order they came, while there are slots.</summary>
    private void PlaceQueued()
    {
        CombatantSetup[] waiting = _queue.ToArray();
        _queue.Clear();
        foreach (CombatantSetup setup in waiting) Bring(setup);
    }

    /// <summary>Brings someone into a fight partway through: on the field, rolling initiative and taking their place in the order. A foe with no room waits; a guest with no room stays away.</summary>
    private void Bring(CombatantSetup setup)
    {
        if (setup.Side == Side.Party && Formation.Free(Side.Party).Count == 0)
        {
            Emit(CombatEventKind.Log, null, null, 0, $"There's no room on the field for {setup.Member?.Name ?? setup.Enemy?.Name ?? setup.Id}.");
            return;
        }
        if (JoinRows(setup) is Combatant joined && _begun) Enter(joined);
    }

    /// <summary>Someone who's just joined a fight that's going: said so, initiative rolled, and put in the order, after the turn that's going if they'd have been before it.</summary>
    private void Enter(Combatant joined)
    {
        Emit(CombatEventKind.Joined, joined, null, 0, $"{joined.Label} joins the fight!");
        RollInitiative(joined);

        int place = 0;
        while (place < _order.Count && CompareInitiative(_order[place], joined) < 0) place++;
        _order.Insert(place, joined);
        if (place <= _turn) _turn++;
        RefreshOrder();
    }

    /// <summary>The end of someone's turn: a Strength save against whatever's restraining them, then every condition ticks down a turn.</summary>
    private void FinishTurn(Combatant who)
    {
        if (!who.IsDead)
        {
            if (!who.IsOut && who.Find(ConditionId.Restrained) is ActiveCondition held && held.Dc > 0)
            {
                bool free = RollSave(who, Ability.Strength, held.Dc, AbilityName(held.From), out RollResult result, out string line);
                Emit(CombatEventKind.SaveRolled, who, who, result.Total, line, result, held.From);
                if (free) RemoveCondition(who, ConditionId.Restrained);
            }

            foreach (ActiveCondition condition in who.Conditions.ToArray())
            {
                if (condition.Turns <= 0) continue;
                condition.Turns--;
                if (condition.Turns == 0) RemoveCondition(who, condition.Id);
            }
        }
        SoulLink.TurnEnded(who.Id, ++_clock);
    }

    /// <summary>A foe's spent abilities with a recharge each roll a d6 at the start of its turn, and come back on a high enough one.</summary>
    private void Recharge(Combatant who)
    {
        foreach (AbilityId id in who.Spent.ToArray())
        {
            int needs = AbilityCatalog.Get(id).RechargeOn;
            if (needs <= 0) continue;

            int roll = Dice.Roll(_random, RechargeDie);
            if (roll < needs) continue;
            who.MarkSpent(id, false);
            Emit(CombatEventKind.Log, who, null, roll, $"{who.Label}'s {AbilityName(id)} recharges ({roll} on a d{RechargeDie}).", ability: id);
        }
    }

    /// <summary>A Downed party member's death save: 10 or more is a success, a 1 two failures, a 20 back up on 1 HP. Three successes and they're stable, three failures and they die.</summary>
    private void DeathSave(Combatant who)
    {
        int roll = Dice.Roll(_random, Dice.D20);
        var result = new RollResult { Dice = new[] { roll }, Total = roll };

        if (roll == AttackMath.CriticalRoll)
        {
            Emit(CombatEventKind.DeathSave, who, who, roll, $"{who.Label}'s death save: {roll}. Back up with 1 HP!", result);
            Revive(who, 1, null);
            return;
        }

        if (roll == AttackMath.FumbleRoll) who.DeathFailures += 2;
        else if (roll < DeathSaveDc) who.DeathFailures++;
        else who.DeathSuccesses++;

        string verdict = roll >= DeathSaveDc ? "SUCCESS" : "FAILURE";
        Emit(CombatEventKind.DeathSave, who, who, roll,
            $"{who.Label}'s death save: {roll} - {verdict} (successes {who.DeathSuccesses}, failures {Math.Min(who.DeathFailures, DeathSavesNeeded)}).", result);

        if (who.DeathFailures >= DeathSavesNeeded) Die(who);
        else if (who.DeathSuccesses >= DeathSavesNeeded)
        {
            AddCondition(who, ConditionId.Stable, 0, null, null);
            Emit(CombatEventKind.Stabilised, who, who, 0, $"{who.Label} stops dying, but stays down.");
        }
    }

    /// <summary>A wave arriving: the Soul-Bound's shout, then each foe in its slot (or the nearest free one, or the queue), rolling initiative and taking their place in the order.</summary>
    private void Arrive(EncounterWave wave)
    {
        if (wave.Bark is not null && Find(Origins.SoulBound) is { IsDuo: true, IsOut: false } shouter)
            Emit(CombatEventKind.Bark, shouter, null, 0, wave.Bark);

        for (int i = 0; i < wave.Foes.Count; i++)
        {
            EncounterFoe foe = wave.Foes[i];
            string id = $"{foe.Enemy}-round{wave.Round}-{i + 1}";
            Bring(CombatantSetup.ForFoe(foe, id));
        }
        RefreshOrder();
    }

    /// <summary>Works out whether the fight's over: won when every foe is beaten and no wave is left (a wave still to come, or foes waiting for room, arrive at once instead), or when the foe it ends on is hurt enough; lost when everyone on the party's side is down.</summary>
    private void CheckOutcome()
    {
        if (Outcome != CombatOutcome.Ongoing) return;

        bool foesLeft = false, partyLeft = false;
        foreach (Combatant each in _combatants)
        {
            if (each.Side == Side.Foes) foesLeft |= !Beaten(each);
            else partyLeft |= !each.IsOut;
        }

        if (!foesLeft && _begun && _queue.Count > 0)
        {
            PlaceQueued();
            return;
        }
        if (!foesLeft && _begun && _wave < _encounter.Waves.Count)
        {
            Arrive(_encounter.Waves[_wave++]);
            return;
        }

        if (!foesLeft || _begun && partyLeft && HurtEnough()) Win();
        else if (!partyLeft)
        {
            Outcome = CombatOutcome.Defeat;
            Emit(CombatEventKind.Defeat, null, null, 0, "The party has fallen.");
        }
    }

    /// <summary>The fight's won: anyone Downed comes round on 1 HP.</summary>
    /// <remarks>The screen awards the XP (LevelTable.FightXp), so the Victory's amount is 0. A guest who fell back is left as they are.</remarks>
    private void Win()
    {
        Outcome = CombatOutcome.Victory;
        foreach (Combatant each in _combatants)
            if (each.Side == Side.Party && !each.IsDead && !each.FellBack && each.IsDown) Revive(each, 1, null);
        Emit(CombatEventKind.Victory, null, null, 0, "Victory!");
    }

    /// <summary>An ability's name for the log, or a placeholder when nothing named caused it.</summary>
    private static string AbilityName(AbilityId? id) => id is AbilityId ability ? AbilityCatalog.Get(ability).Name.ToUpperInvariant() : "IT";
}
