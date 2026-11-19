using System;

namespace LegendQuest.Characters.Motion;

/// <summary>
/// A second-order follower: a value that chases another the way a weight on a spring does, with a
/// natural frequency, damping and response, for what hangs and swings (a skirt, a cape).
/// </summary>
/// <remarks>
/// The damped-spring form animators use for procedural secondary motion (t3ssel8r's "giving
/// personality to procedural animations"): <c>y + k1·y' + k2·y'' = x + k3·x'</c>, with k1 = ζ/(πf),
/// k2 = 1/(2πf)², k3 = rζ/(2πf). f is how quickly it answers (hertz), ζ how soon it settles (under 1
/// overshoots and swings back), r how it starts (0 sluggish, 1 at once, over 1 anticipating). It's
/// stepped semi-implicitly with k2 clamped to what the step can take, so a long frame can't make it
/// blow up. A step of nothing leaves it where it is, so a hit-stop holds it.
/// </remarks>
public sealed class Spring
{
    private readonly float _k1, _k2, _k3;
    private float _x, _y, _v;
    private bool _started;

    /// <summary>A spring with its frequency, damping and response.</summary>
    /// <param name="frequency">How quickly it answers, in hertz.</param>
    /// <param name="damping">How soon it settles: under 1 it swings past and back.</param>
    /// <param name="response">How it starts: 0 sluggish, 1 at once, more than 1 ahead of the move.</param>
    public Spring(float frequency, float damping, float response = 0f)
    {
        float w = MathF.PI * 2f * frequency;
        _k1 = damping / (MathF.PI * frequency);
        _k2 = 1f / (w * w);
        _k3 = response * damping / w;
    }

    /// <summary>Where it is now.</summary>
    public float Value => _y;

    /// <summary>Puts it at a value at rest.</summary>
    public void Reset(float value)
    {
        _x = _y = value;
        _v = 0f;
        _started = true;
    }

    /// <summary>Gives it a push: adds to how fast it's going.</summary>
    /// <param name="speed">How much, in its units a second.</param>
    public void Push(float speed) => _v += speed;

    /// <summary>Moves it on toward where it's chasing.</summary>
    /// <param name="seconds">How long since the last update; nothing happens for none.</param>
    /// <param name="target">Where it's chasing now.</param>
    /// <returns>Where it is now.</returns>
    public float Update(float seconds, float target)
    {
        if (!_started) Reset(target);
        if (seconds <= 0f) return _y;
        float t = MathF.Min(seconds, 1f / 20f);
        float xd = (target - _x) / t;
        _x = target;
        float k2 = MathF.Max(_k2, MathF.Max(t * t / 2f + t * _k1 / 2f, t * _k1));
        _y += t * _v;
        _v += t * (target + _k3 * xd - _y - _k1 * _v) / k2;
        return _y;
    }
}
