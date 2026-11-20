using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using LegendQuest.Characters;
using LegendQuest.StateManagement;

namespace LegendQuest.Screens;

/// <summary>
/// Getting hit points back between fights: slowly as you go about (1% of the most each has every
/// 2 s), and quickly while the party rests (R, out of a fight: everyone kneels and gets 5% of their
/// most back every second, until they're full or you move).
/// </summary>
/// <remarks>
/// Story/11-combat.md §2.6. It's what makes the fields (ExploreScreen.Field) something to fight over
/// and over: a field fight heals nothing afterwards, as a story fight does. Resting stops when you
/// walk (keys or a click), when someone talks to you, and when a fight starts, an aggressive field
/// coming at you included. Party members only; whoever's waiting at camp is resting already.
/// </remarks>
public partial class ExploreScreen
{
    /// <summary>How often walking about gives back a share of everyone's most hit points, in seconds, and how big a share.</summary>
    private const float RegenSeconds = 2f;
    private const float RegenShare = 0.01f;

    /// <summary>How often resting gives back a share, in seconds, and how big a share.</summary>
    private const float RestSeconds = 1f;
    private const float RestShare = 0.05f;

    private readonly InputAction _rest = new(Array.Empty<Buttons>(), new[] { Keys.R }, true);

    /// <summary>Whether the party's resting, and how long since the last share came back.</summary>
    private bool _resting;
    private float _regenClock;

    /// <summary>R out of a fight: the party kneels and rests, or gets up again. Says whether it took the key.</summary>
    /// <param name="input">The input this frame.</param>
    private bool HandleRestInput(InputState input)
    {
        if (!_rest.Occurred(input) || InCombat || _talking) return false;
        if (_resting) StopResting();
        else StartResting();
        return true;
    }

    /// <summary>Everyone in the party down on one knee, if anyone's hurt.</summary>
    private void StartResting()
    {
        if (!AnyoneHurt())
        {
            _toast.Show("Nobody needs a rest");
            return;
        }
        StopPath();
        _resting = true;
        _regenClock = 0f;
        foreach (WalkerSprite walker in _walkers)
        {
            walker.IsWalking = false;
            walker.PlayClip("kneel");
        }
        _toast.Show("The party rests");
    }

    /// <summary>Everyone back on their feet.</summary>
    private void StopResting()
    {
        if (!_resting) return;
        _resting = false;
        _regenClock = 0f;
        foreach (WalkerSprite walker in _walkers) walker.Release();
    }

    /// <summary>Runs the hit points coming back a frame on, out of a fight: a share every few seconds, faster while resting; and ends a rest that's done or been broken.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    private void UpdateRest(GameTime gameTime)
    {
        if (InCombat || _talking || _resting && Leader.IsWalking)
        {
            StopResting();
            if (InCombat) return;
        }

        float every = _resting ? RestSeconds : RegenSeconds;
        _regenClock += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_regenClock < every) return;
        _regenClock -= every;

        float share = _resting ? RestShare : RegenShare;
        foreach (PartyMember member in _game.Party)
        {
            if (member.Hp <= 0 || member.Hp >= member.MaxHp) continue;
            member.Hp = Math.Min(member.MaxHp, member.Hp + Math.Max(1, (int)MathF.Round(member.MaxHp * share)));
        }

        if (_resting && !AnyoneHurt())
        {
            StopResting();
            _toast.Show("Everyone's rested");
        }
    }

    /// <summary>Whether anyone in the party is short of their most hit points.</summary>
    private bool AnyoneHurt()
    {
        foreach (PartyMember member in _game.Party)
            if (member.Hp < member.MaxHp) return true;
        return false;
    }
}
