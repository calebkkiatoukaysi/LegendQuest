using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Particles;

/// <summary>One of the fight's effect particles: where it is and how it moves, its shape, its size and colour from birth to death, how it turns, and whether it's light (added) or matter (laid over).</summary>
public struct EffectParticle
{
    /// <summary>Where it is, in world pixels.</summary>
    public Vector2 Position;

    /// <summary>How fast it's going, in world pixels a second.</summary>
    public Vector2 Velocity;

    /// <summary>What pulls on it, in world pixels a second each second (positive down).</summary>
    public Vector2 Gravity;

    /// <summary>How much of its speed it loses a second to the air.</summary>
    public float Drag;

    /// <summary>How long it lives and how long it's lived, in seconds.</summary>
    public float Life, Age;

    /// <summary>Its size at birth and at death, in world pixels across.</summary>
    public float Size0, Size1;

    /// <summary>Its colour at birth and at death (not premultiplied), and its strength at birth (it fades to nothing by death as the square).</summary>
    public Color Colour0, Colour1;
    public float Strength;

    /// <summary>Which way it's turned and how fast it turns, in radians.</summary>
    public float Rotation, Spin;

    /// <summary>Its shape.</summary>
    public EffectShape Shape;

    /// <summary>Whether it's turned along the way it's going and stretched by its speed, like a spark's streak: how many world pixels long for each world pixel a second, 0 for not.</summary>
    public float Stretch;

    /// <summary>How long it waits before it shows, in seconds.</summary>
    public float Delay;

    /// <summary>Whether it's laid over what's behind (dust, dark smoke) rather than added as light.</summary>
    public bool Matter;

    /// <summary>Whether it's alive.</summary>
    public readonly bool Alive => Age < Life;
}

/// <summary>
/// The pool every effect particle of a fight lives in: sparks, embers, shards, leaves, smoke, dust,
/// rays, rings and motes, each with its own shape, colours, size over its life, drag and gravity, so
/// one pool and two draws (light added, matter laid over) cover every school's burst.
/// </summary>
/// <remarks>
/// The old particle systems (SparkParticleSystem, MoteParticleSystem) are one colour and one
/// behaviour a system; a burst of fire is embers rising, a flame lick and a flash together, so these
/// carry everything per particle. A fixed pool: when it's full the oldest go first. A particle's
/// colour runs from its birth colour to its death colour, and its strength falls as the square of
/// its age, so nothing pops out. Drawn in world pixels through the camera, smooth-sampled.
/// </remarks>
public sealed class EffectParticles
{
    private readonly EffectParticle[] _pool;
    private int _next;
    private readonly Random _random;

    /// <summary>A pool of a size, with its own dice (seeded, so a proof run's bursts come out the same each time).</summary>
    public EffectParticles(int size = 2048, int seed = 11)
    {
        _pool = new EffectParticle[size];
        _random = new Random(seed);
    }

    /// <summary>How many are alive.</summary>
    public int Count
    {
        get
        {
            int n = 0;
            foreach (EffectParticle p in _pool)
                if (p.Alive) n++;
            return n;
        }
    }

    /// <summary>A number between two, from the pool's dice.</summary>
    public float Range(float least, float most) => least + (float)_random.NextDouble() * (most - least);

    /// <summary>Adds one, in place of a dead one or else the next in turn.</summary>
    public void Add(in EffectParticle particle)
    {
        for (int tries = 0; tries < _pool.Length; tries++)
        {
            int i = (_next + tries) % _pool.Length;
            if (_pool[i].Alive) continue;
            _pool[i] = particle;
            _next = (i + 1) % _pool.Length;
            return;
        }
        _pool[_next] = particle;
        _next = (_next + 1) % _pool.Length;
    }

    /// <summary>Moves every live one on.</summary>
    /// <param name="seconds">Seconds since the last update.</param>
    public void Update(float seconds)
    {
        for (int i = 0; i < _pool.Length; i++)
        {
            ref EffectParticle p = ref _pool[i];
            if (!p.Alive) continue;
            if (p.Delay > 0f)
            {
                p.Delay -= seconds;
                continue;
            }
            p.Age += seconds;
            p.Velocity += p.Gravity * seconds;
            p.Velocity *= MathF.Max(0f, 1f - p.Drag * seconds);
            p.Position += p.Velocity * seconds;
            p.Rotation += p.Spin * seconds;
        }
    }

    /// <summary>Lets every one go.</summary>
    public void Clear()
    {
        for (int i = 0; i < _pool.Length; i++) _pool[i].Age = _pool[i].Life;
    }

    /// <summary>Draws the live ones of one kind: the light ones added, the matter laid over.</summary>
    /// <param name="spriteBatch">The SpriteBatch, not begun.</param>
    /// <param name="textures">The shapes.</param>
    /// <param name="world">The camera's matrix.</param>
    /// <param name="matter">Whether to draw the matter rather than the light.</param>
    public void Draw(SpriteBatch spriteBatch, EffectTextures textures, Matrix world, bool matter)
    {
        bool begun = false;
        foreach (EffectParticle p in _pool)
        {
            if (!p.Alive || p.Delay > 0f || p.Matter != matter) continue;
            if (!begun)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, matter ? BlendState.AlphaBlend : Blends.PremultipliedAdditive, SamplerState.LinearClamp, transformMatrix: world);
                begun = true;
            }
            float u = MathHelper.Clamp(p.Age / p.Life, 0f, 1f);
            float fade = (1f - u) * (1f - u);
            Color colour = Color.Lerp(p.Colour0, p.Colour1, u) * (p.Strength * fade);
            Texture2D texture = textures[p.Shape];
            float size = MathHelper.Lerp(p.Size0, p.Size1, u);
            var scale = new Vector2(size / texture.Width, size / texture.Width);
            float rotation = p.Rotation;
            if (p.Stretch > 0f && p.Velocity.LengthSquared() > 1f)
            {
                rotation = MathF.Atan2(p.Velocity.Y, p.Velocity.X);
                scale.X = MathF.Max(scale.X, (size + p.Velocity.Length() * p.Stretch) / texture.Width);
            }
            spriteBatch.Draw(texture, p.Position, null, colour, rotation, new Vector2(texture.Width, texture.Height) / 2f, scale, SpriteEffects.None, 0f);
        }
        if (begun) spriteBatch.End();
    }
}
