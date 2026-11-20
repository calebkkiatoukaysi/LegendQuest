using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using LegendQuest.Characters;
using LegendQuest.Combat;
using LegendQuest.Combat.Realtime;
using LegendQuest.Components;

namespace LegendQuest.Screens;

/// <summary>
/// The fight's clock: the engine stepped in its fixed 20 ms ticks, everything that happens shown on
/// the frame it happens, and everyone placed where the engine has them, walking, turning and moving
/// as they go.
/// </summary>
/// <remarks>
/// <para>
/// The room keeps running round the fight (Story/11-combat.md §2): the camera follows you, you walk
/// with the keys or the mouse, the torches flicker and the toasts come and go; only the ways out,
/// the doors, the triggers and the scenes wait until it's over. The engine owns where everyone is.
/// The screen keeps its own clock, the fight's time as shown, and steps the engine until it's a tick
/// ahead of it, so every figure is drawn between the last two ticks (interpolated) and every event
/// is played when the shown clock reaches its tick. A blow's damage is stamped with the tick its
/// clip's impact falls on, so its number, its flash and its sparks come on the frame the weapon
/// lands. Under the proof run the clock moves exactly a sixtieth of a second for each drawn frame
/// (lockstep), whatever the real clock does, so a captured run of frames is evenly spaced.
/// </para>
/// <para>
/// Each figure plays its clips on its own (ExploreScreen.CombatCues), so any number of people swing
/// at once. A hit-stop holds a figure's clip for a few frames while the engine goes on; once it's
/// over, the clip plays a little faster until it's caught up with the engine's moment, so the next
/// blow of a combo still lands on its frame. A tip that holds everything holds the fight too, so does a lesson, and so
/// does the pause menu (the screen isn't updated under it) and the panel at the end.
/// </para>
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How long after the fight's won or lost the panel comes up, so the last blow and the victory poses are seen.</summary>
    private const float EndHoldSeconds = 1.6f;

    /// <summary>How long the panel at the end waits before going by itself when the proof run's playing, in the fight's time: only if the run hasn't put it away after its shot (ExploreScreen.ProofFights), so the two never race.</summary>
    private const float AutoContinueSeconds = 30f;

    /// <summary>A sixtieth of a second: the proof run's lockstep frame.</summary>
    private const float ProofFrameSeconds = 1f / 60f;

    /// <summary>How far behind the engine's moment a clip can fall before it's hurried, in seconds, and how much faster it plays for each second it's behind; and the fastest it plays.</summary>
    private const float ClipSlack = 0.02f;
    private const float CatchUpRate = 6f;
    private const float FastestCatchUp = 2.5f;

    /// <summary>How far someone has to move in a tick, in world pixels, to be walking.</summary>
    private const float WalkingStep = 0.05f;

    /// <summary>Two standing fighters closer than this across and down the floor, in world pixels, are drawn eased apart across it (Spread), by at most SpreadMost each, at SpreadRate a second.</summary>
    private const float SpreadWidth = 20f, SpreadDepth = 12f, SpreadMost = 9f, SpreadRate = 4f;

    /// <summary>How far each fighter's figure is drawn off where the engine has them, across the room, so a crowd doesn't heap up.</summary>
    private readonly Dictionary<string, float> _spread = new();

    /// <summary>What the engine's done that hasn't been shown yet, in order.</summary>
    private readonly Queue<BattleEvent> _waiting = new();

    /// <summary>Where each fighter stood at the tick before the engine's last, to draw them between the two.</summary>
    private readonly Dictionary<string, System.Numerics.Vector2> _before = new();

    /// <summary>Things the cues have set for a moment of the fight's shown time: a swing's whoosh just before its blow, a marker resolving, a death's fade.</summary>
    private readonly List<(float AtMs, Action Do)> _timed = new();

    /// <summary>The clip each fighter's walker was started on for its move, and when in the fight, to keep it in step with the engine.</summary>
    private readonly Dictionary<string, (Clip Clip, int StartMs)> _moves = new();

    /// <summary>Each figure sliding from one point to another: out of the fight's way, or back.</summary>
    private readonly Dictionary<WalkerSprite, (Vector2 From, Vector2 To, float Clock, float Seconds)> _glides = new();

    /// <summary>Each figure fading in or out: the opacity it's going to and how much it changes a second.</summary>
    private readonly Dictionary<WalkerSprite, (float To, float Rate)> _fades = new();

    /// <summary>What's being said over people's heads mid-fight: who, what, how long it's been up and how long it stays.</summary>
    private readonly List<(WalkerSprite Walker, string Text, float Clock, float Length)> _barks = new();

    /// <summary>The fight's time as shown, in milliseconds: the engine is always a tick ahead of it.</summary>
    private float _shownMs;

    /// <summary>How long the fight's been over, for the panel's moment.</summary>
    private float _endClock;

    /// <summary>Whether the proof run's having the party's AI drive the leader, and how much faster than real time the fight runs for it.</summary>
    private bool _proofAutoplay;
    private float _combatSpeed = 1f;

    /// <summary>The proof run's frame count, set by the run once for each drawn frame (LegendQuestGame.Proof), and the last one the fight moved on for.</summary>
    internal static int ProofFrame { get; set; }
    private int _proofFrameSeen = -1;

    /// <summary>How long the panel at the end has been up while the proof run's playing.</summary>
    private float _autoContinue;

    /// <summary>Starts the clock for a new fight.</summary>
    private void BeginLoop()
    {
        _waiting.Clear();
        _before.Clear();
        _timed.Clear();
        _moves.Clear();
        _cueMarks.Clear();
        _spread.Clear();
        _shownMs = 0f;
        _endClock = 0f;
        _autoContinue = 0f;
        _proofFrameSeen = ProofFrame;
        _held = Vector2.Zero;
        foreach (Fighter fighter in _battle.Fighters) _before[fighter.Id] = fighter.Position;
    }

    /// <summary>Lets go of the clock's pieces once the fight's packed away.</summary>
    private void EndLoop()
    {
        _waiting.Clear();
        _before.Clear();
        _timed.Clear();
        _moves.Clear();
        _cueMarks.Clear();
        _proofAutoplay = false;
        _combatSpeed = 1f;
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

    /// <summary>How long this frame is for the fight: the real frame at the fight's speed, or under the proof run a sixtieth of a second for each frame drawn since the last update and nothing for an update with no frame drawn.</summary>
    private float FightSeconds(GameTime gameTime)
    {
        if (ProofSeed is null) return (float)gameTime.ElapsedGameTime.TotalSeconds * _combatSpeed;
        if (ProofFrame == _proofFrameSeen) return 0f;
        int frames = Math.Clamp(ProofFrame - _proofFrameSeen, 0, 4);
        _proofFrameSeen = ProofFrame;
        return frames * ProofFrameSeconds * _combatSpeed;
    }

    /// <summary>Runs the fight a frame on, if there is one: the engine, what it did, everyone's figures, what's thrown about, the screen's pieces and the camera on you.</summary>
    /// <remarks>Replaces the room's own running while a fight's on, so nobody strolls off, no trigger goes off and no scene plays mid-fight. The torches, the toasts and the room's life still run.</remarks>
    /// <param name="gameTime">The frame's timing.</param>
    /// <returns>True if a fight's on, so the room's own running should stop here.</returns>
    private bool UpdateCombat(GameTime gameTime)
    {
        if (!InCombat) return false;

        float seconds = FightSeconds(gameTime);
        var time = new GameTime(gameTime.TotalGameTime, TimeSpan.FromSeconds(seconds));
        bool held = _tutorials.Current is { Blocking: true } || LessonHolds || _result.Showing;

        ApplyFightSettings();
        if (!held) StepFight(seconds);
        Spread(seconds);
        PlaceFighters();

        foreach (WalkerSprite walker in _walkers) walker.Update(seconds);
        foreach (Npc npc in _npcs) npc.Walker.Update(seconds);
        foreach (WalkerSprite walker in _spawned) walker.Update(seconds);
        foreach (TorchSprite torch in _torches) torch.Update(gameTime);
        UpdateMotion(seconds);

        _toast.Update(gameTime);
        _effects.Update(seconds);
        _popups.Update(seconds);
        _banner.Update(time);
        _log.Update(time);
        _sigil.Update(time);
        _result.Update(time);
        _hudView = BuildHudView();
        ChimeReady(_hudView);
        _hud.Update(time, _hudView);
        _camera.Follow(Leader.Position, gameTime);

        if (_result.Showing)
        {
            _autoContinue += seconds;
            if (_proofAutoplay && _result.Ready && _autoContinue >= AutoContinueSeconds) DismissResult();
            return true;
        }
        if (held) return true;

        if ((_battle.Outcome != FightOutcome.Ongoing || _handedWin) && _waiting.Count == 0 && !_fightEnded)
        {
            _endClock += seconds;
            if (_endClock >= EndHoldSeconds) EndOfFight();
        }
        GuideFight();
        return true;
    }

    /// <summary>Moves the fight's shown clock on, steps the engine until it's a tick ahead, and plays everything that's happened up to the shown moment.</summary>
    /// <param name="seconds">How long the frame was, at the fight's speed.</param>
    private void StepFight(float seconds)
    {
        _shownMs += seconds * 1000f;
        while (_battle.Outcome == FightOutcome.Ongoing && !_handedWin && _battle.NowMs <= _shownMs)
        {
            foreach (Fighter fighter in _battle.Fighters) _before[fighter.Id] = fighter.Position;
            foreach (BattleEvent happened in _battle.Step()) _waiting.Enqueue(happened);
        }
        if (_battle.Outcome != FightOutcome.Ongoing)
            foreach (BattleEvent happened in _battle.Step()) _waiting.Enqueue(happened);

        while (_waiting.Count > 0 && (_waiting.Peek().AtMs <= _shownMs || _battle.Outcome != FightOutcome.Ongoing && _waiting.Peek().AtMs <= _battle.NowMs))
            Cue(_waiting.Dequeue());

        for (int i = 0; i < _timed.Count; i++)
        {
            if (_timed[i].AtMs > _shownMs) continue;
            Action due = _timed[i].Do;
            _timed.RemoveAt(i--);
            due();
        }
    }

    /// <summary>Does something when the fight's shown clock reaches a moment (at once if it's already there).</summary>
    /// <param name="atMs">When, in the fight's time.</param>
    /// <param name="what">What.</param>
    private void AtShown(float atMs, Action what)
    {
        if (atMs <= _shownMs) what();
        else _timed.Add((atMs, what));
    }

    /// <summary>How far between the engine's last two ticks the shown clock is, 0 to 1.</summary>
    private float Between => _battle.Outcome != FightOutcome.Ongoing ? 1f : MathHelper.Clamp((_shownMs - (_battle.NowMs - Battle.TickMs)) / Battle.TickMs, 0f, 1f);

    /// <summary>
    /// Puts every fighter's figure where the engine has them, between its last two ticks, turned the
    /// way the engine has them facing and walking while they move; and keeps each one's move in step
    /// with the engine's moment once a hit-stop's let go of it.
    /// </summary>
    /// <remarks>
    /// A crowd round one foe can stand a few pixels apart in the engine, which reads as one heap of
    /// figures. Those standing almost on one spot are drawn eased apart across the room by a few
    /// pixels (Spread); only the drawing moves, never where the engine has them.
    /// </remarks>
    private void PlaceFighters()
    {
        float between = Between;
        foreach (Fighter fighter in _battle.Fighters)
        {
            if (!fighter.Present || !_fighters.TryGetValue(fighter.Id, out WalkerSprite walker) || _glides.ContainsKey(walker)) continue;

            System.Numerics.Vector2 now = fighter.Position;
            System.Numerics.Vector2 was = _before.TryGetValue(fighter.Id, out System.Numerics.Vector2 before) ? before : now;
            walker.Position = ToScreen(System.Numerics.Vector2.Lerp(was, now, between)) + new Vector2(_spread.GetValueOrDefault(fighter.Id), 0f);
            bool standing = !fighter.IsDown && !fighter.IsDead;
            walker.IsWalking = standing && System.Numerics.Vector2.Distance(was, now) > WalkingStep;
            if (standing) walker.Facing = fighter.Facing < 0 ? Direction.Left : Direction.Right;
            walker.WeaponKind = WeaponKinds.Clip(fighter.Form?.Weapon ?? fighter.WeaponKind);
            KeepInStep(fighter.Id, walker);
        }
    }

    /// <summary>
    /// Eases each standing fighter's drawn offset across the room toward one that keeps them clear of
    /// anyone standing almost on their spot: half the overlap each way, the earlier joiner to the left
    /// when they're on the very same spot, at most SpreadMost either way.
    /// </summary>
    /// <param name="seconds">How long the frame was, at the fight's speed.</param>
    private void Spread(float seconds)
    {
        IReadOnlyList<Fighter> all = _battle.Fighters;
        var want = new Dictionary<string, float>();
        for (int i = 0; i < all.Count; i++)
        {
            Fighter a = all[i];
            if (!a.Present || !a.IsStanding) continue;
            for (int j = i + 1; j < all.Count; j++)
            {
                Fighter b = all[j];
                if (!b.Present || !b.IsStanding) continue;
                float across = b.Position.X - a.Position.X, down = b.Position.Y - a.Position.Y;
                if (MathF.Abs(down) >= SpreadDepth || MathF.Abs(across) >= SpreadWidth) continue;
                float push = (SpreadWidth - MathF.Abs(across)) / 2f * (across >= 0f ? 1f : -1f);
                want[a.Id] = want.GetValueOrDefault(a.Id) - push;
                want[b.Id] = want.GetValueOrDefault(b.Id) + push;
            }
        }

        float ease = 1f - MathF.Exp(-SpreadRate * seconds);
        foreach (Fighter fighter in all)
        {
            float to = Math.Clamp(want.GetValueOrDefault(fighter.Id), -SpreadMost, SpreadMost);
            float now = _spread.GetValueOrDefault(fighter.Id);
            _spread[fighter.Id] = now + (to - now) * ease;
        }
    }

    /// <summary>Plays a figure's move a little faster while it's behind the moment the engine's at, once nothing's holding it.</summary>
    private void KeepInStep(string id, WalkerSprite walker)
    {
        if (!_moves.TryGetValue(id, out (Clip Clip, int StartMs) move) || walker.Move != move.Clip)
        {
            if (walker.Move is not null && walker.MoveSpeed != 1f) walker.MoveSpeed = 1f;
            return;
        }
        if (walker.Frozen) return;

        float behind = (_shownMs - move.StartMs) / 1000f - walker.MoveTime;
        walker.MoveSpeed = behind > ClipSlack ? MathF.Min(FastestCatchUp, 1f + behind * CatchUpRate) : 1f;
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
            walker.IsWalking = false;
            if (_restFacing.Remove(walker, out Direction facing)) walker.Facing = facing;
        }

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
