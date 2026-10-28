using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Particles;

/// <summary>
/// Blossom petals coming off the trees behind the title: they fall slowly, swing from side to side
/// as they go, and fade out near the bottom.
/// </summary>
/// <remarks>
/// On the tutorial's engine like the embers were, falling instead of rising. The petal is pixel art
/// cut off one of my pictures, so it's point-sampled and drawn at the same 4x as the scene, and it
/// doesn't turn, since a turned pixel-art petal goes jagged. The swing is the sideways speed
/// following a sine, a bit different for every petal: its lifetime is as good as a random number,
/// so that picks how fast it swings, where in the swing it starts, and its tint.
/// </remarks>
public class PetalParticleSystem : ParticleSystem
{
    /// <summary>How many petals one fall lets go, at least and at most (exclusive).</summary>
    private const int FewestPerFall = 1;
    private const int MostPerFall = 3;

    /// <summary>How fast they fall, in screen pixels a second, and how far and how often they swing.</summary>
    private const float FallMin = 26f;
    private const float FallMax = 52f;
    private const float SwingSpeed = 22f;
    private const float SwingRateMin = 0.6f;
    private const float SwingRateMax = 1.3f;

    /// <summary>How long one lasts. Long enough to reach the terrace from the top of a tree.</summary>
    private const float LifeMin = 9f;
    private const float LifeMax = 15f;

    /// <summary>How big a petal's drawn: the same 4x as the scene, so its pixels are the same size.</summary>
    private const float Scale = 4f;

    /// <summary>How far through its life a petal starts to fade.</summary>
    private const float FadeFrom = 0.75f;

    /// <summary>The tints a petal can have. White leaves it its own pink; the others shade it a little.</summary>
    private static readonly Color[] Tints = { Color.White, new(236, 220, 236), new(214, 196, 220) };

    /// <summary>Makes room for a set number of petals.</summary>
    /// <param name="maxPetals">How many can be in the air at once.</param>
    public PetalParticleSystem(int maxPetals) : base(maxPetals) { }

    /// <inheritdoc/>
    protected override void InitializeConstants()
    {
        textureFilename = "Title/petal";
        minNumParticles = FewestPerFall;
        maxNumParticles = MostPerFall;
        samplerState = SamplerState.PointClamp;
    }

    /// <inheritdoc/>
    protected override void InitializeParticle(ref Particle p, Vector2 where)
    {
        float lifetime = RandomHelper.NextFloat(LifeMin, LifeMax);
        p.Initialize(where, new Vector2(0f, RandomHelper.NextFloat(FallMin, FallMax)), Vector2.Zero,
            Tints[(int)(lifetime * 1000f) % Tints.Length], lifetime, Scale);
    }

    /// <inheritdoc/>
    protected override void UpdateParticle(ref Particle particle, float dt)
    {
        float rate = MathHelper.Lerp(SwingRateMin, SwingRateMax, particle.Lifetime % 1f);
        particle.Velocity.X = MathF.Sin((particle.TimeSinceStart * rate + particle.Lifetime) * MathHelper.TwoPi) * SwingSpeed;
        base.UpdateParticle(ref particle, dt);

        float age = particle.Age;
        Color tint = Tints[(int)(particle.Lifetime * 1000f) % Tints.Length];
        particle.Color = age > FadeFrom ? tint * (1f - (age - FadeFrom) / (1f - FadeFrom)) : tint;
    }

    /// <summary>Lets a petal or two go from somewhere in a tree.</summary>
    /// <param name="tree">The part of the screen the tree's blossom is in.</param>
    public void Fall(Rectangle tree) => AddParticles(tree);
}
