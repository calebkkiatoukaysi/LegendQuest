using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// One particle in a particle system.
/// </summary>
/// <remarks>
/// Same struct as the particle system tutorial. It's a struct so a whole system is one array with
/// no garbage. I dropped angular acceleration since none of mine use it (same as The Black Box).
/// </remarks>
public struct Particle
{
    /// <summary>Where it is, in screen (or world) pixels.</summary>
    public Vector2 Position;

    /// <summary>How fast it's moving, in pixels a second.</summary>
    public Vector2 Velocity;

    /// <summary>How fast the velocity changes, in pixels a second each second.</summary>
    public Vector2 Acceleration;

    /// <summary>Which way it's turned, in radians. Passed straight to SpriteBatch.Draw.</summary>
    public float Rotation;

    /// <summary>How fast it turns, in radians a second.</summary>
    public float AngularVelocity;

    /// <summary>How big it's drawn. 1 is the texture's own size.</summary>
    public float Scale;

    /// <summary>How long it lives, in seconds.</summary>
    public float Lifetime;

    /// <summary>How long it's been alive, in seconds.</summary>
    public float TimeSinceStart;

    /// <summary>The colour it's drawn in. The textures are white, so this is the whole colour.</summary>
    public Color Color;

    /// <summary>Whether it's still alive and should be drawn.</summary>
    public bool Active => TimeSinceStart < Lifetime;

    /// <summary>How far through its life it is, from 0 to 1.</summary>
    public float Age => TimeSinceStart / Lifetime;

    /// <summary>Sets the particle up for use, the same way the tutorial's Initialize does.</summary>
    /// <param name="position">Where it starts.</param>
    /// <param name="velocity">How fast it starts moving.</param>
    /// <param name="acceleration">What pulls on it.</param>
    /// <param name="color">What colour it starts.</param>
    /// <param name="lifetime">How long it lives, in seconds.</param>
    /// <param name="scale">How big it starts.</param>
    /// <param name="rotation">Which way it starts turned.</param>
    /// <param name="angularVelocity">How fast it turns.</param>
    public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, Color color,
        float lifetime = 1f, float scale = 1f, float rotation = 0f, float angularVelocity = 0f)
    {
        Position = position;
        Velocity = velocity;
        Acceleration = acceleration;
        Color = color;
        Lifetime = lifetime;
        TimeSinceStart = 0f;
        Scale = scale;
        Rotation = rotation;
        AngularVelocity = angularVelocity;
    }
}
