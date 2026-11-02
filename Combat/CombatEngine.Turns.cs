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
public sealed partial class CombatEngine
{
    /// <summary>What a death save has to reach to succeed: 5e's 10.</summary>
    private const int DeathSaveDc = 10;

    /// <summary>How many successes stabilise someone Downed, and how many failures kill them.</summary>
    private const int DeathSavesNeeded = 3;

    /// <summary>How much less someone Slowed moves on their turn, in metres: Ray of Frost's ten feet.</summary>
    private const float SlowedBy = 3f;

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
        _turn = -1;
        CheckOutcome();
        if (Outcome == CombatOutcome.Ongoing) NextTurn();
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
                _turn = -1;
                Round++;
                Emit(CombatEventKind.RoundStarted, null, null, Round, $"Round {Round}.");
                while (_wave < _encounter.Waves.Count && _encounter.Waves[_wave].Round <= Round) Arrive(_encounter.Waves[_wave++]);
            }

            _turn++;
            Combatant next = _order[_turn];
            if (next.IsDead) continue;
            if (BeginTurn(next)) return;
            FinishTurn(next);
        }
    }

    /// <summary>The start of someone's turn: their budget back, Dodging off, foes' abilities recharging, a death save if they're Downed, and getting up if they're Prone.</summary>
    /// <returns>Whether they can do anything this turn.</returns>
    private bool BeginTurn(Combatant who)
    {
        _turnNumber++;
        float movement = who.Has(ConditionId.Restrained) ? 0f : who.Speed - (who.Has(ConditionId.Slowed) ? SlowedBy : 0f);
        who.Budget.Start(movement);
        Emit(CombatEventKind.TurnStarted, who, null);

        RemoveCondition(who, ConditionId.Dodging);
        Recharge(who);

        if (who.IsDown && !who.Has(ConditionId.Stable)) DeathSave(who);
        if (who.IsOut || Beaten(who)) return false;

        if (who.Has(ConditionId.Prone))
        {
            who.Budget.Walk(who.Budget.Movement / 2f);
            RemoveCondition(who, ConditionId.Prone);
        }
        return true;
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

    /// <summary>A wave arriving: the Soul-Bound's shout, then each foe on its tile, rolling initiative and taking their place in the order.</summary>
    private void Arrive(EncounterWave wave)
    {
        if (wave.Bark is not null && Find(Origins.SoulBound) is { IsDuo: true, IsOut: false } shouter)
            Emit(CombatEventKind.Bark, shouter, null, 0, wave.Bark);

        for (int i = 0; i < wave.Foes.Count; i++)
        {
            EncounterFoe foe = wave.Foes[i];
            string id = $"{foe.Enemy}-round{wave.Round}-{i + 1}";
            Combatant joined = Join(CombatantSetup.ForFoe(foe, id, foe.Cell ?? default, Direction.Down));
            Emit(CombatEventKind.Joined, joined, null, 0, $"{joined.Label} joins the fight!");
            RollInitiative(joined);

            int place = 0;
            while (place < _order.Count && CompareInitiative(_order[place], joined) < 0) place++;
            _order.Insert(place, joined);
            if (place <= _turn) _turn++;
        }
        RefreshOrder();
    }

    /// <summary>Works out whether the fight's over: won when every foe is beaten and no wave is left (a wave still to come arrives at once instead), lost when everyone on the party's side is down.</summary>
    private void CheckOutcome()
    {
        if (Outcome != CombatOutcome.Ongoing) return;

        bool foesLeft = false, partyLeft = false;
        foreach (Combatant each in _combatants)
        {
            if (each.Side == Side.Foes) foesLeft |= !Beaten(each);
            else partyLeft |= !each.IsOut;
        }

        if (!foesLeft && _begun && _wave < _encounter.Waves.Count)
        {
            Arrive(_encounter.Waves[_wave++]);
            return;
        }

        if (!foesLeft) Win();
        else if (!partyLeft)
        {
            Outcome = CombatOutcome.Defeat;
            Emit(CombatEventKind.Defeat, null, null, 0, "The party has fallen.");
        }
    }

    /// <summary>The fight's won: anyone Downed comes round on 1 HP, and the XP's shared between the party.</summary>
    /// <remarks>Guests get none of it, since they aren't in the party. The XP goes straight onto the PartyMembers, the way hit points do.</remarks>
    private void Win()
    {
        Outcome = CombatOutcome.Victory;

        var sharing = new List<PartyMember>();
        foreach (Combatant each in _combatants)
        {
            if (each.Side != Side.Party || each.IsDead) continue;
            if (each.IsDown) Revive(each, 1, null);
            if (each.Member is not null && !each.Guest) sharing.Add(each.Member);
        }

        int share = sharing.Count == 0 ? 0 : _encounter.Xp / sharing.Count;
        foreach (PartyMember member in sharing) member.Xp += share;
        Emit(CombatEventKind.Victory, null, null, share, share > 0 ? $"Victory! {share} XP each." : "Victory!");
    }

    /// <summary>An ability's name for the log, or a placeholder when nothing named caused it.</summary>
    private static string AbilityName(AbilityId? id) => id is AbilityId ability ? AbilityCatalog.Get(ability).Name.ToUpperInvariant() : "IT";
}
