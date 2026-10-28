using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Particles;

/// <summary>
/// A pool of particles that all share one texture and one blend state. Subclasses say what the
/// particles look like and how they move.
/// </summary>
/// <remarks>
/// This is the particle system tutorial's class with the same one change as The Black Box: it isn't
/// a DrawableGameComponent. As a component it drew after the ScreenManager, so particles went over
/// the pause menu and kept moving while it was up. Now the screen that owns it updates and draws it,
/// and hands it the screen's matrix so the particles move with the camera.
/// </remarks>
public abstract class ParticleSystem
{
    /// <summary>Every particle, alive or not.</summary>
    private readonly Particle[] _particles;

    /// <summary>The indices of the particles that aren't alive, ready to be used.</summary>
    private readonly Queue<int> _freeParticles;

    private Texture2D _texture;

    /// <summary>The middle of the texture, so particles turn and grow around their centre.</summary>
    private Vector2 _origin;

    /// <summary>The blend state the batch begins with. Set in InitializeConstants.</summary>
    protected BlendState blendState = BlendState.AlphaBlend;

    /// <summary>The sampler the batch begins with: smooth for soft dots of light, point for anything that's pixel art.</summary>
    protected SamplerState samplerState = SamplerState.LinearClamp;

    /// <summary>The texture to load. Set in InitializeConstants.</summary>
    protected string textureFilename;

    /// <summary>The fewest particles one AddParticles makes.</summary>
    protected int minNumParticles;

    /// <summary>The most particles one AddParticles makes, exclusive, like Random.Next.</summary>
    protected int maxNumParticles;

    /// <summary>Makes the pool.</summary>
    /// <param name="maxParticles">The most particles that can be alive at once. Past that AddParticles just makes fewer.</param>
    protected ParticleSystem(int maxParticles)
    {
        // A default particle has no lifetime, so they all start dead.
        _particles = new Particle[maxParticles];
        _freeParticles = new Queue<int>(maxParticles);
        for (int i = 0; i < _particles.Length; i++) _freeParticles.Enqueue(i);

        InitializeConstants();
    }

    /// <summary>Sets textureFilename, minNumParticles, maxNumParticles, blendState and samplerState.</summary>
    protected abstract void InitializeConstants();

    /// <summary>Starts one particle off at a point. Each system decides how.</summary>
    /// <param name="p">The particle to start.</param>
    /// <param name="where">Where it starts.</param>
    protected abstract void InitializeParticle(ref Particle p, Vector2 where);

    /// <summary>Moves and turns one particle. Subclasses call this, then change its colour and size.</summary>
    /// <param name="particle">The particle to update.</param>
    /// <param name="dt">Seconds since the last frame.</param>
    protected virtual void UpdateParticle(ref Particle particle, float dt)
    {
        particle.Velocity += particle.Acceleration * dt;
        particle.Position += particle.Velocity * dt;

        particle.Rotation += particle.AngularVelocity * dt;

        particle.TimeSinceStart += dt;
    }

    /// <summary>Loads the texture and works out its middle.</summary>
    /// <param name="content">The ContentManager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        if (string.IsNullOrEmpty(textureFilename))
            throw new InvalidOperationException(GetType().Name + " never set textureFilename in InitializeConstants.");

        _texture = content.Load<Texture2D>(textureFilename);
        _origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
    }

    /// <summary>Updates every live particle and frees the ones that just died.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        for (int i = 0; i < _particles.Length; i++)
        {
            if (!_particles[i].Active) continue;

            UpdateParticle(ref _particles[i], dt);
            if (!_particles[i].Active) _freeParticles.Enqueue(i);
        }
    }

    /// <summary>Draws every live particle in its own batch, with this system's blend state.</summary>
    /// <remarks>The rotation and the colour go straight into Draw, so each particle turns and fades on its own.</remarks>
    /// <param name="spriteBatch">The SpriteBatch to draw with. Must not be in the middle of a batch.</param>
    /// <param name="transform">The screen's matrix: identity on the title, the camera on a map.</param>
    public void Draw(SpriteBatch spriteBatch, Matrix transform)
    {
        if (_freeParticles.Count == _particles.Length) return;

        spriteBatch.Begin(SpriteSortMode.Deferred, blendState, samplerState, transformMatrix: transform);

        foreach (Particle p in _particles)
        {
            if (!p.Active) continue;

            spriteBatch.Draw(_texture, p.Position, null, p.Color,
                p.Rotation, _origin, p.Scale, SpriteEffects.None, 0f);
        }

        spriteBatch.End();
    }

    /// <summary>Starts a handful of particles, each at a random point inside an area. If the pool is used up it starts as many as it can.</summary>
    /// <param name="where">The area they start in.</param>
    protected void AddParticles(Rectangle where)
    {
        int numParticles = RandomHelper.Next(minNumParticles, maxNumParticles);

        for (int i = 0; i < numParticles && _freeParticles.Count > 0; i++)
            InitializeParticle(ref _particles[_freeParticles.Dequeue()], RandomHelper.RandomPosition(where));
    }
}
