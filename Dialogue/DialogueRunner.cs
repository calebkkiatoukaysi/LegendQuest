using System;
using System.Collections.Generic;
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
/// be played through without a window. Every flag, meter, item and recruit it changes goes straight
/// into the save, and it hands back what happened as events for the screen to turn into toasts,
/// dice and sounds. Not from a tutorial.
/// </remarks>
public sealed class DialogueRunner
{
    private readonly DialogueScript _script;
    private readonly SaveData _game;
    private readonly Random _random;
    private readonly List<DialogueEvent> _events = new();

    /// <summary>Where it's up to in the script's nodes, or -1 once it's over.</summary>
    private int _at = -1;

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
    /// <remarks>Empty for a line that isn't a question, and for a question whose options are all hidden, which then reads on like any line.</remarks>
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

    /// <summary>Goes on past a line that isn't a question.</summary>
    public void Continue()
    {
        if (IsOver) return;
        GoTo(Follow(_at, Current.Next));
    }

    /// <summary>Picks an option: its effects, then its check if it has one, then wherever it goes.</summary>
    /// <param name="shown">One of what Options handed back. A greyed one does nothing.</param>
    public void Choose(ShownOption shown)
    {
        if (IsOver || !shown.Open) return;

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

    /// <summary>Goes to a line, skipping on past any whose conditions aren't met, and does what it does.</summary>
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
                Apply(node.Effects);
                return;
            }
            index = Follow(index, node.Next);
        }
        _at = -1;
    }

    /// <summary>Does what a line or an option does to the game, and notes it for the screen.</summary>
    private void Apply(IReadOnlyList<DialogueEffect> effects)
    {
        foreach (DialogueEffect effect in effects)
        {
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

                case EffectKind.Voice:
                    _game.Voice = Enum.Parse<Voice>(effect.Id);
                    break;

                case EffectKind.Travel:
                    _events.Add(new DialogueEvent(DialogueEventKind.Travel, effect.Id));
                    break;
            }
        }
    }

    /// <summary>Someone joins: into the party if there's room, to wait at camp if not. Nobody joins twice.</summary>
    private void Recruit(string id)
    {
        if (_game.InParty(id)) return;
        foreach (PartyMember waiting in _game.Camp)
            if (waiting.Id == id) return;

        PartyMember member = Origins.Create(id);
        bool joins = _game.Party.Count < Origins.PartySize;
        if (joins) _game.Party.Add(member);
        else _game.Camp.Add(member);
        _events.Add(new DialogueEvent(DialogueEventKind.Recruited, id, joins ? 1 : 0));
    }
}
