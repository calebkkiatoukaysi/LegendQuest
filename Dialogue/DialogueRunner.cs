using System;
using System.Collections.Generic;
using System.Linq;
using LegendQuest.Characters;
using LegendQuest.Rules;
using LegendQuest.Save;

namespace LegendQuest.Dialogue;

/// <summary>
/// Plays a conversation against a game: where it's up to, which options show, and what picking one
/// does.
/// </summary>
/// <remarks>
/// The Black Box's DiscussionPeriod pattern: it draws nothing and takes its Random, so a script can
/// be played through without a window. Every flag, meter, item, recruit, swap, leaving and coming
/// back, and all the experience it changes goes straight into the save, and it hands back what
/// happened as events for the screen to turn into toasts, dice and sounds. A change to a fight that's
/// going (support-off, fight-add, fight-guest) can't go in the save: it comes back as a FightOrder
/// event, for the screen to hand the fight once the interlude's over. A line's effects happen when it's said (Say): the
/// screen says it once it's up, after its stage cues, so someone a line sends to camp is still there
/// for the cues that come before it, and going on past a line says it first if nothing has. Not from
/// a tutorial.
/// </remarks>
public sealed class DialogueRunner
{
    private readonly DialogueScript _script;
    private readonly SaveData _game;
    private readonly Random _random;
    private readonly List<DialogueEvent> _events = new();

    /// <summary>Where it's up to in the script's nodes, or -1 once it's over.</summary>
    private int _at = -1;

    /// <summary>Whether the line it's on has been said, and its effects have happened.</summary>
    private bool _said;

    /// <summary>Starts a conversation on its first line that can be said.</summary>
    /// <param name="script">What gets said.</param>
    /// <param name="game">The game it changes.</param>
    /// <param name="random">Where the dice come from.</param>
    public DialogueRunner(DialogueScript script, SaveData game, Random random)
    {
        _script = script;
        _game = game;
        _random = random;
        GoTo(0);
    }

    /// <summary>The script being played.</summary>
    public DialogueScript Script => _script;

    /// <summary>The line it's on, or null once it's over.</summary>
    public DialogueNode Current => _at < 0 ? null : _script.Nodes[_at];

    /// <summary>Whether it's over.</summary>
    public bool IsOver => _at < 0;

    /// <summary>The options under the current line that show: the open ones, and the locked ones greyed.</summary>
    /// <remarks>Empty for a line that isn't a question, and for a question whose options are all hidden, which then reads on like any line. Asked once the line's said, since what it does can open or close them.</remarks>
    public IReadOnlyList<ShownOption> Options()
    {
        var shown = new List<ShownOption>();
        if (Current is not DialogueNode node) return shown;

        foreach (DialogueOption option in node.Options)
        {
            string reason = null;
            foreach (DialogueCondition condition in option.Conditions)
            {
                if (condition.Holds(_game)) continue;
                reason = condition.Reason;
                break;
            }

            if (reason is null) shown.Add(new ShownOption(option, true, null));
            else if (option.Locked) shown.Add(new ShownOption(option, false, reason));
        }
        return shown;
    }

    /// <summary>Says the line it's on: its effects happen, once. The screen does it as the line comes up.</summary>
    public void Say()
    {
        if (IsOver || _said) return;
        _said = true;
        Apply(Current.Effects);
    }

    /// <summary>Goes on past a line that isn't a question, saying it first if it hasn't been.</summary>
    public void Continue()
    {
        if (IsOver) return;
        Say();
        GoTo(Follow(_at, Current.Next));
    }

    /// <summary>Picks an option: the question's said if it hasn't been, then the option's effects, then its check if it has one, then wherever it goes.</summary>
    /// <param name="shown">One of what Options handed back. A greyed one does nothing.</param>
    public void Choose(ShownOption shown)
    {
        if (IsOver || !shown.Open) return;

        Say();
        DialogueOption option = shown.Option;
        Apply(option.Effects);

        if (option.Check is not DialogueCheck check)
        {
            GoTo(Follow(_at, option.Next));
            return;
        }

        bool advantage = false;
        foreach (DialogueCondition condition in check.Advantage) advantage |= condition.Holds(_game);
        CheckResult result = AbilityCheck.Roll(_random, check.Label, check.Ability, _game.You.Scores, check.Dc, advantage);
        _events.Add(new DialogueEvent(DialogueEventKind.Check, check.Label, Check: result));
        GoTo(Follow(_at, result.Success ? option.OnSuccess : option.OnFailure));
    }

    /// <summary>The clock on a question ran out: goes wherever the script says that leads.</summary>
    public void TimeOut()
    {
        if (IsOver || Current.Seconds <= 0f) return;
        Say();
        GoTo(Follow(_at, Current.Timeout));
    }

    /// <summary>Everything that's happened since the last time this was asked, oldest first.</summary>
    public IReadOnlyList<DialogueEvent> Drain()
    {
        DialogueEvent[] happened = _events.ToArray();
        _events.Clear();
        return happened;
    }

    /// <summary>Where a target leads from a line: null is the next line (or the end after the last), End is the end, and anything else is that line.</summary>
    private int Follow(int from, string target)
    {
        if (target is not null) return _script.IndexOf(target);
        return from + 1 < _script.Nodes.Count ? from + 1 : -1;
    }

    /// <summary>Goes to a line, skipping on past any whose conditions aren't met. What it does waits till it's said.</summary>
    /// <remarks>Gives up after as many skips as there are lines, so two lines that skip to each other end the conversation instead of hanging the game.</remarks>
    private void GoTo(int index)
    {
        for (int skipped = 0; index >= 0 && skipped <= _script.Nodes.Count; skipped++)
        {
            DialogueNode node = _script.Nodes[index];
            bool said = true;
            foreach (DialogueCondition condition in node.Conditions) said &= condition.Holds(_game);

            if (said)
            {
                _at = index;
                _said = false;
                return;
            }
            index = Follow(index, node.Next);
        }
        _at = -1;
    }

    /// <summary>Does what a line or an option does to the game, and notes it for the screen.</summary>
    /// <remarks>An effect whose conditions don't hold is skipped. They're looked at as it comes, so an effect before it can change them.</remarks>
    private void Apply(IReadOnlyList<DialogueEffect> effects)
    {
        foreach (DialogueEffect effect in effects)
        {
            if (!effect.Applies(_game)) continue;

            switch (effect.Kind)
            {
                case EffectKind.SetFlag:
                    _game.Flags.Add(effect.Id);
                    _events.Add(new DialogueEvent(DialogueEventKind.Flag, effect.Id, 1));
                    break;

                case EffectKind.ClearFlag:
                    _game.Flags.Remove(effect.Id);
                    _events.Add(new DialogueEvent(DialogueEventKind.Flag, effect.Id));
                    break;

                case EffectKind.AddMeter:
                    _game.SetMeter(effect.Id, _game.Meter(effect.Id) + effect.Amount);
                    _events.Add(new DialogueEvent(DialogueEventKind.Meter, effect.Id, effect.Amount));
                    break;

                case EffectKind.SetMeter:
                    _game.SetMeter(effect.Id, effect.Amount);
                    _events.Add(new DialogueEvent(DialogueEventKind.Meter, effect.Id));
                    break;

                case EffectKind.Gold:
                    _game.Gold += effect.Amount;
                    _events.Add(new DialogueEvent(DialogueEventKind.Gold, effect.Id, effect.Amount));
                    break;

                case EffectKind.GiveItem:
                    if (!_game.Items.Contains(effect.Id)) _game.Items.Add(effect.Id);
                    _events.Add(new DialogueEvent(DialogueEventKind.Item, effect.Id, 1));
                    break;

                case EffectKind.TakeItem:
                    _game.Items.Remove(effect.Id);
                    _events.Add(new DialogueEvent(DialogueEventKind.Item, effect.Id));
                    break;

                case EffectKind.Objective:
                    _game.Objective = effect.Id;
                    _events.Add(new DialogueEvent(DialogueEventKind.Objective, effect.Id));
                    break;

                case EffectKind.Recruit:
                    Recruit(effect.Id);
                    break;

                case EffectKind.Rest:
                    Rest();
                    break;

                case EffectKind.Swap:
                    if (effect.Id.Split(' ') is [string waiting, string member]) Swap(waiting, member);
                    break;

                case EffectKind.Leave:
                    Leave(effect.Id);
                    break;

                case EffectKind.Rejoin:
                    Rejoin(effect.Id);
                    break;

                case EffectKind.Voice:
                    _game.Voice = Enum.Parse<Voice>(effect.Id);
                    break;

                case EffectKind.Travel:
                    _events.Add(new DialogueEvent(DialogueEventKind.Travel, effect.Id));
                    break;

                case EffectKind.Shop:
                    _events.Add(new DialogueEvent(DialogueEventKind.Shop, effect.Id));
                    break;

                case EffectKind.Fight:
                    _events.Add(new DialogueEvent(DialogueEventKind.Fight, effect.Id));
                    break;

                case EffectKind.Experience:
                    Experience(effect.Amount);
                    break;

                case EffectKind.SupportOff:
                case EffectKind.FightAdd:
                case EffectKind.FightGuest:
                    _events.Add(new DialogueEvent(DialogueEventKind.FightOrder, effect.Id, effect.Amount) { Order = effect.Order });
                    break;
            }
        }
    }

    /// <summary>A long rest: everyone, in the party and at camp, back to full hit points. Nothing else is spent between fights: Focus and cooldowns are each fight's own.</summary>
    private void Rest()
    {
        foreach (PartyMember member in _game.Party.Concat(_game.Camp)) member.Hp = member.MaxHp;
    }

    /// <summary>Experience for everyone, in the party, at camp and away, so nobody waiting falls behind: an event for it, and one for each level anyone goes up.</summary>
    /// <param name="amount">How much each.</param>
    private void Experience(int amount)
    {
        _events.Add(new DialogueEvent(DialogueEventKind.Experience, "xp", amount));
        foreach (PartyMember member in _game.Party.Concat(_game.Camp).Concat(_game.Away))
        {
            int from = member.Level;
            Leveling.Award(member, amount);
            for (int level = from + 1; level <= member.Level; level++) _events.Add(new DialogueEvent(DialogueEventKind.LevelUp, member.Id, level));
        }
    }

    /// <summary>Someone joins: into the party if there's room, to wait at camp if not, at your level. Someone already waiting at camp comes out of it once there's room, and stays where they are until there is. Nobody joins twice, and someone who's away only comes back by Rejoin.</summary>
    private void Recruit(string id)
    {
        if (_game.InParty(id) || _game.Away.Exists(each => each.Id == id)) return;

        PartyMember waiting = _game.Camp.Find(each => each.Id == id);
        bool joins = _game.Party.Count < Origins.PartySize;
        if (waiting is not null && !joins) return;

        if (waiting is not null) _game.Camp.Remove(waiting);
        PartyMember member = waiting ?? JoinsAtYourLevel(Origins.Create(id));
        if (joins) _game.Party.Add(member);
        else _game.Camp.Add(member);
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, id, joins ? 1 : 0));
    }

    /// <summary>Someone new joins at your level, so the story's fights are tuned for them too: at full health, with no experience towards the next.</summary>
    /// <param name="member">Who, fresh from their origin.</param>
    private PartyMember JoinsAtYourLevel(PartyMember member)
    {
        Leveling.SetLevel(member, _game.Party.Count > 0 ? _game.You.Level : 1);
        member.Hp = member.MaxHp;
        return member;
    }

    /// <summary>Someone waiting at camp takes a companion's place in the party, and the companion goes to wait in their seat, so the party and the camp stay in the same order. Never you or the Soul-Bound: the story's about the two of you.</summary>
    /// <param name="waiting">Who's waiting, by origin id.</param>
    /// <param name="member">Whose place they take, by origin id.</param>
    private void Swap(string waiting, string member)
    {
        int seat = _game.Camp.FindIndex(each => each.Id == waiting);
        int place = _game.Party.FindIndex(each => each.Id == member);
        if (seat < 0 || place < 0 || _game.Party[place] == _game.You || member == Origins.SoulBound) return;

        (_game.Camp[seat], _game.Party[place]) = (_game.Party[place], _game.Camp[seat]);
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, member, 0));
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, waiting, 1));
    }

    /// <summary>Someone goes away for a while: out of the party, or out of the camp, and into Away till a Rejoin brings them back. Nobody goes twice, and you never go: the party's yours.</summary>
    /// <remarks>
    /// You keep control of whoever you were controlling, or of yourself if they're the one going. It
    /// tells the screen as a Recruited event of -1, so the map catches its figures up the way it does
    /// for someone joining.
    /// </remarks>
    /// <param name="id">Who, by origin id.</param>
    private void Leave(string id)
    {
        if (id == Origins.You) return;

        int place = _game.Party.FindIndex(each => each.Id == id);
        PartyMember member = place >= 0 ? _game.Party[place] : _game.Camp.Find(each => each.Id == id);
        if (member is null) return;

        if (place < 0)
        {
            _game.Camp.Remove(member);
        }
        else
        {
            PartyMember leader = place == _game.Leader ? _game.You : _game.LeaderMember;
            _game.Party.RemoveAt(place);
            _game.Leader = Math.Max(0, _game.Party.IndexOf(leader));
        }
        _game.Away.Add(member);
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, id, -1));
    }

    /// <summary>Someone who went away comes back into the party, even if it's full: it can hold Origins.MostInParty for a while. Past that they wait at camp. Someone who isn't away stays where they are.</summary>
    /// <param name="id">Who, by origin id.</param>
    private void Rejoin(string id)
    {
        PartyMember member = _game.Away.Find(each => each.Id == id);
        if (member is null) return;

        bool joins = _game.Party.Count < Origins.MostInParty;
        _game.Away.Remove(member);
        if (joins) _game.Party.Add(member);
        else _game.Camp.Add(member);
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, id, joins ? 1 : 0));
    }
}
