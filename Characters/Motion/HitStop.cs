using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Characters.Motion;

/// <summary>Which side of a blow a figure frozen by it is on.</summary>
public enum HitStopRole
{
    /// <summary>The one hit: frozen dead, shaking.</summary>
    Victim,

    /// <summary>The one who struck: barely creeping on, not shaking.</summary>
    Attacker,
}

/// <summary>
/// One figure's hit-stop: its own clock frozen for a few frames when a blow lands, the world going on
/// round it.
/// </summary>
/// <remarks>
/// Sakurai's rules, as the fighting games do it: the one hit shakes, the attacker doesn't; the shake
/// is across on the ground (up and down in the air), tapers off through the stop, and moves only the
/// picture, never where the figure stands; and the attacker keeps creeping on a little rather than
/// stopping dead, so the blow feels like it's pushing through. Only the two fighters freeze: the
/// screen, the particles and everyone else go on. While it's on, the figure's clip, its springs, its
/// nudges and its flash's fade all stand still (Scale gives how much of each second they get).
/// </remarks>
public struct HitStop
{
    /// <summary>How much of each second an attacker's clock gets while frozen: enough to creep.</summary>
    public const float Creep = 0.08f;

    /// <summary>How many frames the shake holds each side before swapping: two, so it reads at 60 a second.</summary>
    private const int ShakeFrames = 2;

    private float _left, _length, _clock;
    private float _pixels;
    private Vector2 _axis;

    /// <summary>Whether it's frozen now.</summary>
    public readonly bool Active => _left > 0f;

    /// <summary>Which side of the blow it is.</summary>
    public HitStopRole Role { get; private set; }

    /// <summary>
    /// Freezes it for a number of frames, or for longer if it's already frozen longer. The victim
    /// shakes by up to <paramref name="pixels"/> along <paramref name="axis"/>.
    /// </summary>
    /// <param name="frames">How long, at 60 a second.</param>
    /// <param name="role">Which side of the blow it's on.</param>
    /// <param name="pixels">How far the victim shakes each way at first, in world pixels (1 to 2).</param>
    /// <param name="axis">Which way it shakes: across (1, 0) on the ground, up and down (0, 1) in the air.</param>
    public void Start(int frames, HitStopRole role, float pixels = 1.5f, Vector2? axis = null)
    {
        float seconds = Feel.Seconds(Math.Max(0, frames));
        if (seconds <= _left) return;
        _left = _length = seconds;
        _clock = 0f;
        Role = role;
        _pixels = pixels;
        _axis = axis ?? Vector2.UnitX;
    }

    /// <summary>Lets it go at once.</summary>
    public void Stop() => _left = 0f;

    /// <summary>Runs its own clock (in real seconds, not the figure's) and says how many of the figure's seconds pass this update: none while a victim's frozen, a creep while an attacker is, all of them otherwise.</summary>
    /// <param name="seconds">Real seconds since the last update.</param>
    public float Scale(float seconds)
    {
        if (_left <= 0f) return seconds;
        float frozen = MathF.Min(seconds, _left);
        _left -= frozen;
        _clock += frozen;
        float free = seconds - frozen;
        return free + frozen * (Role == HitStopRole.Attacker ? Creep : 0f);
    }

    /// <summary>Where the picture is drawn off where the figure stands now: the victim's shake, swapping sides every two frames and tapering off; nothing for an attacker or when it's not frozen.</summary>
    public readonly Vector2 Offset
    {
        get
        {
            if (_left <= 0f || Role != HitStopRole.Victim || _length <= 0f) return Vector2.Zero;
            int frame = (int)(_clock * Feel.FramesPerSecond);
            float side = frame / ShakeFrames % 2 == 0 ? 1f : -1f;
            float taper = _left / _length;
            return _axis * (side * _pixels * (0.35f + 0.65f * taper));
        }
    }
}
