using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LegendQuest.Components;

/// <summary>
/// A torch's flame, and the light flickering round it. The flame is cut from torch-flame.png and
/// the light is the glow texture drawn additively over whatever is already lit.
/// </summary>
/// <remarks>
/// The pool of light itself is baked into whatever is behind the torch. This only adds the part
/// that moves, which is cheaper than rebuilding the light every frame. The flicker is three sine
/// waves at speeds that never line up, so it doesn't look like it loops, and each torch starts them
/// somewhere different (seeded from where it is) so two torches never flicker together.
/// </remarks>
public class TorchSprite
{
    /// <summary>One frame of torch-flame.png in art pixels, and how many frames there are. Has to match FLAME_W, FLAME_H and FLAME_FRAMES in tools/generate_world.py.</summary>
    private const int FrameWidth = 12;
    private const int FrameHeight = 20;
    private const int Frames = 6;

    /// <summary>Seconds per flame frame.</summary>
    private const float FrameTime = 0.09f;

    /// <summary>How far above its base the flame's tip is, in art pixels. The light's centred halfway up it.</summary>
    private const float TipHeight = 13f;

    /// <summary>The three flicker speeds (radians a second) and how much each one moves the light.</summary>
    private static readonly (float Speed, float Depth)[] FlickerWaves = { (7.3f, 0.08f), (13.1f, 0.05f), (23.7f, 0.04f) };

    /// <summary>How bright the light is with all three waves at zero.</summary>
    private const float FlickerBase = 0.85f;

    private readonly float[] _phases = new float[FlickerWaves.Length];
    private readonly float _frameOffset;

    private Texture2D _flame;
    private Texture2D _glow;
    private float _clock;

    /// <summary>How bright the light is this frame, about 0.7 to 1.</summary>
    private float _flicker = FlickerBase;

    /// <summary>The bottom middle of the flame, where it sits in its sconce.</summary>
    public Vector2 Position { get; }

    /// <summary>How much the art is blown up. Whole numbers only, so the pixels stay square.</summary>
    public float Scale { get; }

    /// <summary>Puts a torch somewhere.</summary>
    /// <param name="position">The bottom middle of the flame.</param>
    /// <param name="scale">How much the art is blown up.</param>
    public TorchSprite(Vector2 position, float scale)
    {
        Position = position;
        Scale = scale;

        // Seeded from the position so each torch flickers its own way, and the same way every time.
        var random = new Random((int)(position.X * 31f + position.Y * 17f));
        for (int i = 0; i < _phases.Length; i++) _phases[i] = (float)random.NextDouble() * MathHelper.TwoPi;
        _frameOffset = (float)random.NextDouble() * Frames * FrameTime;
    }

    /// <summary>Loads the flame sheet and the glow.</summary>
    /// <param name="content">The content manager to load with.</param>
    public void LoadContent(ContentManager content)
    {
        _flame = content.Load<Texture2D>("torch-flame");
        _glow = content.Load<Texture2D>("glow");
    }

    /// <summary>Runs the flame and the flicker.</summary>
    /// <param name="gameTime">The frame's timing.</param>
    public void Update(GameTime gameTime)
    {
        _clock += (float)gameTime.ElapsedGameTime.TotalSeconds;

        _flicker = FlickerBase;
        for (int i = 0; i < FlickerWaves.Length; i++)
            _flicker += FlickerWaves[i].Depth * MathF.Sin(_clock * FlickerWaves[i].Speed + _phases[i]);
    }

    /// <summary>Draws the flame. Goes in the point sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="layerDepth">Where it sorts.</param>
    public void Draw(SpriteBatch spriteBatch, float layerDepth)
    {
        int frame = (int)((_clock + _frameOffset) / FrameTime) % Frames;
        var source = new Rectangle(frame * FrameWidth, 0, FrameWidth, FrameHeight);
        var origin = new Vector2(FrameWidth / 2, FrameHeight);

        spriteBatch.Draw(_flame, Position, source, Color.White, 0f, origin, Scale, SpriteEffects.None, layerDepth);
    }

    /// <summary>Draws the flickering light round the flame. Goes in an additive, linear sampled batch.</summary>
    /// <param name="spriteBatch">The SpriteBatch to render with.</param>
    /// <param name="radius">How far the light reaches, in the same units as Position.</param>
    /// <param name="color">The light's colour at full strength (premultiplied, so scale it down to dim it).</param>
    public void DrawGlow(SpriteBatch spriteBatch, float radius, Color color)
    {
        Vector2 centre = Position - new Vector2(0f, TipHeight * Scale / 2f);
        var area = new Rectangle((int)(centre.X - radius), (int)(centre.Y - radius), (int)(radius * 2f), (int)(radius * 2f));
        spriteBatch.Draw(_glow, area, color * _flicker);
    }
}
