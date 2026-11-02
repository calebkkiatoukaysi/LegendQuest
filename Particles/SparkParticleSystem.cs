using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// Sparks off a blow in a fight: a quick fan of hot points that fly out, slow, fall and go out, in
/// one colour.
/// </summary>
/// <remarks>
/// The Black Box's sparks at the map's size, in world pixels, with the camera's matrix. They start
/// white hot and cool through their colour (ember for steel, ice for frost, gold for radiant light)
/// to nothing. The texture is mote.png, the white dot, drawn small, and the blend is additive, so a
/// burst on a dark map lights it and two that cross get brighter. One system per colour, since a
/// particle only has the one colour it's drawn in.
/// </remarks>
public class SparkParticleSystem : ParticleSystem
{
    /// <summary>How many sparks one burst throws, at least and at most (exclusive).</summary>
    private const int FewestPerBurst = 10;
    private const int PerBurst = 17;

    /// <summary>How fast a spark leaves, in world pixels a second.</summary>
    private const float SpeedMin = 40f;
    private const float SpeedMax = 115f;

    /// <summary>How hard they fall, and how much of their speed they lose a second to the air, which is what makes them hang at the top.</summary>
    private const float Gravity = 170f;
    private const float Drag = 3.2f;

    /// <summary>How long a spark lasts, in seconds, and how big it's drawn (mote.png is 8 pixels across, so a quarter is two art pixels).</summary>
    private const float LifeMin = 0.25f;
    private const float LifeMax = 0.6f;
    private const float ScaleMin = 0.18f;
    private const float ScaleMax = 0.32f;

    /// <summary>How far through its life a spark stops being white hot.</summary>
    private const float Cooled = 0.25f;

    /// <summary>What every spark starts as.</summary>
    private static readonly Color WhiteHot = new(255, 246, 226);

    /// <summary>The colour the sparks cool through.</summary>
    private readonly Color _colour;

    /// <summary>Which way the next burst points, and how far either side of that it fans out, in radians.</summary>
    private float _facing;
    private float _spread;

    /// <summary>Makes room for a few bursts of one colour at once.</summary>
    /// <param name="maxBursts">How many bursts can be in the air together.</param>
    /// <param name="colour">The colour they cool through.</param>
    public SparkParticleSystem(int maxBursts, Color colour) : base(maxBursts * PerBurst) => _colour = colour;

    /// <inheritdoc/>
    protected override void InitializeConstants()
    {
        textureFilename = "mote";
        minNumParticles = FewestPerBurst;
        maxNumParticles = PerBurst;
        blendState = Blends.PremultipliedAdditive;
    }

    /// <inheritdoc/>
    protected override void InitializeParticle(ref Particle p, Vector2 where)
    {
        float angle = RandomHelper.NextFloat(_facing - _spread, _facing + _spread);
        Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * RandomHelper.NextFloat(SpeedMin, SpeedMax);

        p.Initialize(where, velocity, Vector2.UnitY * Gravity, WhiteHot,
            lifetime: RandomHelper.NextFloat(LifeMin, LifeMax),
            scale: RandomHelper.NextFloat(ScaleMin, ScaleMax));
    }

    /// <inheritdoc/>
    protected override void UpdateParticle(ref Particle particle, float dt)
    {
        particle.Velocity *= MathF.Max(0f, 1f - Drag * dt);
        base.UpdateParticle(ref particle, dt);

        float age = particle.Age;
        Color colour = age < Cooled ? Color.Lerp(WhiteHot, _colour, age / Cooled) : _colour;
        particle.Color = colour * (1f - age * age);
    }

    /// <summary>Throws a burst of sparks out of a point.</summary>
    /// <param name="where">Where they come from, in world pixels.</param>
    /// <param name="facing">Which way the middle of the burst goes, in radians. Up the screen is -pi/2.</param>
    /// <param name="spread">How far either side of that they can go, in radians.</param>
    public void Burst(Vector2 where, float facing, float spread)
    {
        _facing = facing;
        _spread = spread;
        AddParticles(new Rectangle((int)where.X, (int)where.Y, 1, 1));
    }
}
