using System;
using Microsoft.Xna.Framework;

namespace LegendQuest.Particles;

/// <summary>
/// Motes of coloured light in a fight: warm ones rising off someone healed, a spell's colour
/// gathering round its caster, radiant light falling on a foe, a trail behind a bolt.
/// </summary>
/// <remarks>
/// The Black Box's heal motes at the map's size, in world pixels, one colour to a system. They drift
/// up (or down, for light falling on someone), sway, and fade in and back out over a half sine so
/// none of them pops. mote.png drawn small and additive, the same as the sparks.
/// </remarks>
public class MoteParticleSystem : ParticleSystem
{
    /// <summary>How many motes one cloud lets go, at least and at most (exclusive).</summary>
    private const int FewestPerCloud = 8;
    private const int PerCloud = 14;

    /// <summary>How fast they rise or fall, and how much they sway, in world pixels a second; and how much faster they go each second.</summary>
    private const float SpeedMin = 8f;
    private const float SpeedMax = 26f;
    private const float Sway = 7f;
    private const float Lift = 10f;

    /// <summary>How long one lasts, in seconds, and how big it's drawn (mote.png is 8 pixels across).</summary>
    private const float LifeMin = 0.6f;
    private const float LifeMax = 1.25f;
    private const float ScaleMin = 0.22f;
    private const float ScaleMax = 0.45f;

    /// <summary>The colour every mote is.</summary>
    private readonly Color _colour;

    /// <summary>Which way the next cloud drifts: -1 up, 1 down.</summary>
    private float _drift = -1f;

    /// <summary>Makes room for a few clouds of one colour at once.</summary>
    /// <param name="maxClouds">How many clouds can be in the air together.</param>
    /// <param name="colour">Their colour.</param>
    public MoteParticleSystem(int maxClouds, Color colour) : base(maxClouds * PerCloud) => _colour = colour;

    /// <inheritdoc/>
    protected override void InitializeConstants()
    {
        textureFilename = "mote";
        minNumParticles = FewestPerCloud;
        maxNumParticles = PerCloud;
        blendState = Blends.PremultipliedAdditive;
    }

    /// <inheritdoc/>
    protected override void InitializeParticle(ref Particle p, Vector2 where)
    {
        var velocity = new Vector2(RandomHelper.NextFloat(-Sway, Sway), _drift * RandomHelper.NextFloat(SpeedMin, SpeedMax));

        p.Initialize(where, velocity, new Vector2(0f, _drift * Lift), Color.Transparent,
            lifetime: RandomHelper.NextFloat(LifeMin, LifeMax),
            scale: RandomHelper.NextFloat(ScaleMin, ScaleMax));
    }

    /// <inheritdoc/>
    protected override void UpdateParticle(ref Particle particle, float dt)
    {
        base.UpdateParticle(ref particle, dt);
        particle.Color = _colour * MathF.Sin(MathF.PI * particle.Age);
    }

    /// <summary>Lets a cloud of motes go from somewhere.</summary>
    /// <param name="area">Where they start, in world pixels: anywhere inside it.</param>
    /// <param name="falling">Whether they drift down instead of up.</param>
    public void Release(Rectangle area, bool falling)
    {
        _drift = falling ? 1f : -1f;
        AddParticles(area);
    }
}
