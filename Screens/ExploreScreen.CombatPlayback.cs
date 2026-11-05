using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Components;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's clock: the events waiting to be shown and the one showing, the foes' turns played
/// at a pace that can be followed, and the people moving, fading and talking while it goes.
/// </summary>
/// <remarks>
/// The engine works a whole command out at once; this plays what it caused a beat at a time
/// (ExploreScreen.CombatCues says what each beat looks like and how long it takes), so nothing's
/// shown before what led to it. When there's nothing left to show it's either your turn, which waits
/// for you, or someone else's, which waits a moment after their banner and then asks EnemyMind. A
/// tip that holds everything holds the fight too, and so does the panel at the end until it's read.
/// The camera holds on the middle of the fight (FightStage.Centre), and anyone who's got where they
/// were walking turns to face the other side.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long the fight waits after a foe's turn starts before it plays, so its banner can be read.</summary>
    private const float AiPauseSeconds = 0.45f;

    /// <summary>How long the panel at the end waits before going by itself when the proof run's playing the party, in the fight's time: only if the run hasn't put it away after its shot (ExploreScreen.ProofFights), so the two never race.</summary>
    private const float AutoContinueSeconds = 30f;

    /// <summary>The events still to show, in order.</summary>
    private readonly Queue<CombatEvent> _beats = new();

    /// <summary>Each figure sliding from one point to another: to their slot as the fight starts, a Shift, or out of the fight's way.</summary>
    private readonly Dictionary<WalkerSprite, (Vector2 From, Vector2 To, float Clock, float Seconds)> _glides = new();

    /// <summary>Each figure fading in or out: the opacity it's going to and how much it changes a second.</summary>
    private readonly Dictionary<WalkerSprite, (float To, float Rate)> _fades = new();

    /// <summary>What's being said over people's heads mid-fight: who, what, how long it's been up and how long it stays.</summary>
    private readonly List<(WalkerSprite Walker, string Text, float Clock, float Length)> _barks = new();

    /// <summary>The event showing, how long it's been showing and how long it shows for.</summary>
    private CombatEvent _beat;
    private float _beatClock;
    private float _beatLength;

    /// <summary>What happens partway through the beat (the blow landing after the wind-up, the bolt leaving the hand), when, and whether it has.</summary>
    private Action _beatStrike;
    private float _beatStrikeAt;

    /// <summary>The log line the beat writes when it ends rather than when it starts (an attack's, as the blow lands), and its colour.</summary>
    private string _beatLine;
    private Color _beatLineColour;

    /// <summary>How long the fight's waiting before it shows anything else (the fight's name as it starts).</summary>
    private float _holdLeft;

    /// <summary>How long it's waited since a turn that isn't yours started.</summary>
    private float _aiWait;

    /// <summary>Whether the proof run's having EnemyMind play the party's turns too, and how much faster than real time the fight runs for it.</summary>
    private bool _proofAutoplay;
    private float _combatSpeed = 1f;

    /// <summary>How long the panel at the end has been up while the proof run's playing.</summary>
    private float _autoContinue;

    /// <summary>Whether whoever's turn it is is yours to play: one of the party proper (not a guest), standing, and the proof run not playing for you.</summary>
    private bool PlayersTurn => _combat.Current is { Side: Side.Party, Member: not null, Guest: false, IsOut: false } && !_proofAutoplay;

    /// <summary>Whether the fight's waiting on you: everything's been shown, it's still on, and it's your turn.</summary>
    private bool AwaitingPlayer =>
        InCombat && _beat is null && _beats.Count == 0 && _holdLeft <= 0f && _combat.Outcome == CombatOutcome.Ongoing && PlayersTurn && !_result.Showing;

    /// <summary>Starts the clock for a new fight: nothing waiting, and a hold before the first beat.</summary>
    /// <param name="hold">How long to hold first, in seconds.</param>
    private void BeginPlayback(float hold)
    {
        _beats.Clear();
        _beat = null;
        _beatStrike = null;
        _beatLine = null;
        _holdLeft = hold;
        _aiWait = 0f;
        _autoContinue = 0f;
        _eventsShown = 0;
        _intentsFor = null;
        _intents.Clear();
    }

    /// <summary>Puts a command's events in line to be shown.</summary>
    /// <param name="events">What the command caused, in order.</param>
    private void Play(IReadOnlyList<CombatEvent> events)
    {
        foreach (CombatEvent happened in events) _beats.Enqueue(happened);
    }

    /// <summary>Slides a figure to a point, walking.</summary>
    /// <param name="walker">Who.</param>
    /// <param name="to">Where, in world pixels.</param>
    /// <param name="seconds">How long it takes.</param>
    private void Glide(WalkerSprite walker, Vector2 to, float seconds)
    {
        _glides[walker] = (walker.Position, to, 0f, MathF.Max(seconds, float.Epsilon));
        walker.IsWalking = true;
        walker.Face(to);
    }

    /// <summary>Fades a figure in or out.</summary>
    /// <param name="walker">Who.</param>
    /// <param name="opacity">What to fade to, 0 to 1.</param>
    /// <param name="seconds">How long it takes from the other end.</param>
    private void FadeTo(WalkerSprite walker, float opacity, float seconds) => _fades[walker] = (opacity, 1f / MathF.Max(seconds, float.Epsilon));

    /// <summary>Runs the fight a frame on, if there is one: everyone's figures, what's thrown about, the screen's pieces, the camera, and then whatever's next.</summary>
    /// <remarks>Replaces the room's own running while a fight's on, so nobody strolls off, no trigger goes off and no scene plays mid-fight. The torches, the toasts and the room's life still run.</remarks>
    /// <param name="gameTime">The frame's timing.</param>
    /// <returns>True if a fight's on, so the room's own running should stop here.</returns>
    private bool UpdateCombat(GameTime gameTime)
    {
        if (!InCombat) return false;

        var time = new GameTime(gameTime.TotalGameTime, TimeSpan.FromTicks((long)(gameTime.ElapsedGameTime.Ticks * _combatSpeed)));
        float seconds = (float)time.ElapsedGameTime.TotalSeconds;

        foreach (WalkerSprite walker in _walkers) walker.Update(time);
        foreach (Npc npc in _npcs) npc.Walker.Update(time);
        foreach (WalkerSprite walker in _spawned) walker.Update(time);
        foreach (TorchSprite torch in _torches) torch.Update(gameTime);
        UpdateMotion(seconds);

        _toast.Update(gameTime);
        _effects.Update(time);
        _popups.Update(time);
        _banner.Update(time);
        _log.Update(time);
        _hotbar.Update(time);
        _sigil.Update(time);
        _result.Update(time);
        _initiative.Update(time, OrderViews());
        _camera.Follow(_stage.Centre, gameTime);

        if (_tutorials.Current is { Blocking: true }) return true;
        if (_result.Showing)
        {
            _autoContinue += seconds;
            if (_proofAutoplay && _result.Ready && _autoContinue >= AutoContinueSeconds) DismissResult();
            return true;
        }

        Direct(seconds);
        if (AwaitingPlayer) GuideTurn();
        return true;
    }

    /// <summary>Moves the fight on: the beat showing, then the next, then (once everything's shown) the end, your turn, or someone else's played.</summary>
    /// <param name="seconds">How long the frame was, at the fight's speed.</param>
    private void Direct(float seconds)
    {
        if (_holdLeft > 0f)
        {
            _holdLeft -= seconds;
            return;
        }

        if (_beat is not null)
        {
            _beatClock += seconds;
            if (_beatStrike is not null && _beatClock >= _beatStrikeAt) Strike();
            if (_beatClock < _beatLength) return;
            EndBeat();
        }

        while (_beats.Count > 0)
        {
            StartBeat(_beats.Dequeue());
            if (_beatLength > 0f) return;
            EndBeat();
        }

        if (_combat.Outcome != CombatOutcome.Ongoing)
        {
            if (!_fightEnded) EndOfFight();
            return;
        }

        if (_combat.Current is null || PlayersTurn) return;

        _aiWait += seconds;
        if (_aiWait < AiPauseSeconds) return;
        _aiWait = 0f;
        Play(_combat.RunAiTurn());
    }

    /// <summary>Does what happens partway through the beat, once.</summary>
    private void Strike()
    {
        Action strike = _beatStrike;
        _beatStrike = null;
        strike();
    }

    /// <summary>Finishes the beat showing: anything it hadn't done yet, and its log line if it writes it at the end.</summary>
    private void EndBeat()
    {
        if (_beatStrike is not null) Strike();
        if (_beatLine is not null) _log.Add(_beatLine, _beatLineColour);
        _beatLine = null;
        _beat = null;
    }

    /// <summary>Runs the slides, the fades and the clocks on what's being said.</summary>
    /// <param name="seconds">How long the frame was, at the fight's speed.</param>
    private void UpdateMotion(float seconds)
    {
        foreach (WalkerSprite walker in new List<WalkerSprite>(_glides.Keys))
        {
            (Vector2 from, Vector2 to, float clock, float length) = _glides[walker];
            clock = MathF.Min(length, clock + seconds);
            walker.Position = Vector2.Lerp(from, to, clock / length);
            if (clock < length)
            {
                _glides[walker] = (from, to, clock, length);
                continue;
            }
            _glides.Remove(walker);
            if (_restFacing.Remove(walker, out Direction facing)) walker.Facing = facing;
        }

        // Anyone who's got where they were sliding stops walking. A run of steps doesn't stutter: the
        // next step's slide starts this same frame, before the walk cycle's looked at again.
        foreach (WalkerSprite walker in _fighters.Values)
            if (!_glides.ContainsKey(walker)) walker.IsWalking = false;
        foreach (Npc npc in _standingBack.Keys)
            if (!_glides.ContainsKey(npc.Walker)) npc.Walker.IsWalking = false;
        foreach (WalkerSprite walker in _supportFigures.Values)
            if (!_glides.ContainsKey(walker)) walker.IsWalking = false;
        foreach (WalkerSprite walker in _benched.Keys)
            if (!_glides.ContainsKey(walker)) walker.IsWalking = false;

        foreach (WalkerSprite walker in new List<WalkerSprite>(_fades.Keys))
        {
            (float to, float rate) = _fades[walker];
            float step = rate * seconds;
            walker.Opacity = walker.Opacity < to ? MathF.Min(to, walker.Opacity + step) : MathF.Max(to, walker.Opacity - step);
            if (walker.Opacity == to) _fades.Remove(walker);
        }

        for (int i = _barks.Count - 1; i >= 0; i--)
        {
            (WalkerSprite walker, string text, float clock, float length) = _barks[i];
            clock += seconds;
            if (clock >= length) _barks.RemoveAt(i);
            else _barks[i] = (walker, text, clock, length);
        }
    }
}
