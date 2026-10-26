using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// Embers coming up off a torch. They rise, wander sideways, flicker, and cool from yellow to a
/// dull red before they go out.
/// </summary>
/// <remarks>
/// Built on the tutorial's engine the same way The Black Box's motes were, just going the other way:
/// fast and bright at the start, slowing as they cool. The texture is a white dot and the colour
/// does the rest. Additive, so two crossing embers get brighter.
/// </remarks>
public class EmberParticleSystem : ParticleSystem
{
    /// <summary>How many embers one puff off a torch lets go, at least and at most (exclusive).</summary>
    private const int FewestPerPuff = 1;
    private const int MostPerPuff = 3;

    /// <summary>How far round the tip of the flame they can start, in pixels.</summary>
    private const int Spread = 10;

    /// <summary>How fast they leave going up, and sideways, in pixels a second.</summary>
    private const float RiseMin = 40f;
    private const float RiseMax = 110f;
    private const float Drift = 24f;

    /// <summary>How hard the hot air keeps lifting them, and how much speed they lose a second.</summary>
    /// <remarks>The drag beats the lift once they slow down, which is what makes them hang near the top.</remarks>
    private const float Lift = 30f;
    private const float Drag = 0.9f;

    /// <summary>How hard they get pushed about sideways while they rise, in pixels a second each second.</summary>
    private const float Wander = 60f;

    /// <summary>How long one lasts, and how big it's drawn (mote.png is 8 pixels across).</summary>
    private const float LifeMin = 1.2f;
    private const float LifeMax = 2.8f;
    private const float ScaleMin = 0.45f;
    private const float ScaleMax = 1.0f;

    /// <summary>How fast an ember flickers, in radians a second, and how much.</summary>
    private const float Twinkle = 19f;
    private const float TwinkleDepth = 0.35f;

    /// <summary>How far through its life an ember stops being yellow hot.</summary>
    private const float Cooled = 0.25f;

    private static readonly Color Hot = new(255, 226, 150);
    private static readonly Color Glowing = new(244, 120, 40);
    private static readonly Color Dying = new(120, 26, 10);

    /// <summary>Makes room for a set number of embers.</summary>
    /// <param name="maxEmbers">How many can be in the air at once.</param>
    public EmberParticleSystem(int maxEmbers) : base(maxEmbers) { }

    /// <inheritdoc/>
    protected override void InitializeConstants()
    {
        textureFilename = "mote";
        minNumParticles = FewestPerPuff;
        maxNumParticles = MostPerPuff;
        blendState = Blends.PremultipliedAdditive;
    }

    /// <inheritdoc/>
    protected override void InitializeParticle(ref Particle p, Vector2 where)
    {
        var velocity = new Vector2(RandomHelper.NextFloat(-Drift, Drift), -RandomHelper.NextFloat(RiseMin, RiseMax));
        var acceleration = new Vector2(RandomHelper.NextFloat(-Wander, Wander), -Lift);

        p.Initialize(where, velocity, acceleration, Hot,
            lifetime: RandomHelper.NextFloat(LifeMin, LifeMax),
            scale: RandomHelper.NextFloat(ScaleMin, ScaleMax));
    }

    /// <inheritdoc/>
    protected override void UpdateParticle(ref Particle particle, float dt)
    {
        particle.Velocity *= MathF.Max(0f, 1f - Drag * dt);
        base.UpdateParticle(ref particle, dt);

        float age = particle.Age;
        Color colour = age < Cooled
            ? Color.Lerp(Hot, Glowing, age / Cooled)
            : Color.Lerp(Glowing, Dying, (age - Cooled) / (1f - Cooled));

        // Each one flickers at its own point in the wave (its lifetime is as good as a random number),
        // and fades out over the last part of its life.
        float twinkle = 1f - TwinkleDepth * (0.5f + 0.5f * MathF.Sin(particle.TimeSinceStart * Twinkle + particle.Lifetime * 40f));
        particle.Color = colour * (twinkle * (1f - age * age));
    }

    /// <summary>Lets a few embers go from round a point.</summary>
    /// <param name="where">The tip of the flame they come off.</param>
    public void Puff(Vector2 where) => AddParticles(new Rectangle((int)where.X - Spread / 2, (int)where.Y - Spread / 2, Spread, Spread));
}
