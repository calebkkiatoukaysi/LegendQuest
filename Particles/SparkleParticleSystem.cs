using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// Motes of light coming up off the lake behind the title, the city's magic or just the evening:
/// they rise slowly, drift, twinkle, and fade.
/// </summary>
/// <remarks>
/// The embers' system from the night gate, cooled down and slowed down: the same white dot and the
/// same additive blend, so two that cross get brighter, but pale blue, pink or gold, and in no hurry.
/// </remarks>
public class SparkleParticleSystem : ParticleSystem
{
    /// <summary>How many motes one rise lets go, at least and at most (exclusive).</summary>
    private const int FewestPerRise = 1;
    private const int MostPerRise = 2;

    /// <summary>How fast they rise and drift, in screen pixels a second, and how much they wander.</summary>
    private const float RiseMin = 8f;
    private const float RiseMax = 22f;
    private const float Drift = 10f;
    private const float Wander = 6f;

    /// <summary>How long one lasts, and how big it's drawn (mote.png is 8 pixels across).</summary>
    private const float LifeMin = 3f;
    private const float LifeMax = 6f;
    private const float ScaleMin = 0.4f;
    private const float ScaleMax = 0.9f;

    /// <summary>How fast a mote twinkles, in radians a second, and how much.</summary>
    private const float Twinkle = 7f;
    private const float TwinkleDepth = 0.5f;

    /// <summary>The colours a mote can be.</summary>
    private static readonly Color[] Colours = { new(170, 220, 255), new(255, 190, 226), new(255, 230, 170) };

    /// <summary>Makes room for a set number of motes.</summary>
    /// <param name="maxSparkles">How many can be in the air at once.</param>
    public SparkleParticleSystem(int maxSparkles) : base(maxSparkles) { }

    /// <inheritdoc/>
    protected override void InitializeConstants()
    {
        textureFilename = "mote";
        minNumParticles = FewestPerRise;
        maxNumParticles = MostPerRise;
        blendState = Blends.PremultipliedAdditive;
    }

    /// <inheritdoc/>
    protected override void InitializeParticle(ref Particle p, Vector2 where)
    {
        var velocity = new Vector2(RandomHelper.NextFloat(-Drift, Drift), -RandomHelper.NextFloat(RiseMin, RiseMax));
        var acceleration = new Vector2(RandomHelper.NextFloat(-Wander, Wander), 0f);

        p.Initialize(where, velocity, acceleration, Color.Transparent,
            lifetime: RandomHelper.NextFloat(LifeMin, LifeMax),
            scale: RandomHelper.NextFloat(ScaleMin, ScaleMax));
    }

    /// <inheritdoc/>
    protected override void UpdateParticle(ref Particle particle, float dt)
    {
        base.UpdateParticle(ref particle, dt);

        // Its lifetime is as good as a random number, so it picks the colour and where in the
        // twinkle it starts. It fades in and out over its life so none of them pops.
        Color colour = Colours[(int)(particle.Lifetime * 1000f) % Colours.Length];
        float twinkle = 1f - TwinkleDepth * (0.5f + 0.5f * MathF.Sin(particle.TimeSinceStart * Twinkle + particle.Lifetime * 40f));
        float life = MathF.Sin(MathHelper.Pi * particle.Age);
        particle.Color = colour * (twinkle * life);
    }

    /// <summary>Lets a mote or two go from somewhere off the water.</summary>
    /// <param name="water">The part of the screen they come up from.</param>
    public void Rise(Rectangle water) => AddParticles(water);
}
